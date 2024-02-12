using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.SemanticAnalysis.Nodes;
using Ephemera.SemanticAnalysis.Typing;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using Assembly = System.Reflection.Assembly;

namespace Ephemera.Emitting;

public class ILEmitter
{
    private readonly string _methodName;

    private readonly AssemblyDefinition _assembly;
    private readonly TypeDefinition _class;

    private readonly TypeReference _decimalRef;
    private readonly TypeReference _boolRef;
    private readonly TypeReference _stringRef;

    private readonly MethodReference _decimalCtor;
    private readonly MethodReference _add;
    private readonly MethodReference _subtract;
    private readonly MethodReference _multiply;
    private readonly MethodReference _divide;
    private readonly MethodReference _modulus;
    private readonly MethodReference _negate;

    public ILEmitter(string assemblyName, string className, string methodName)
    {
        _methodName = methodName;

        var name = new AssemblyNameDefinition(assemblyName, new Version(1, 0));
        _assembly = AssemblyDefinition.CreateAssembly(name, assemblyName, ModuleKind.Dll);
        var module = _assembly.MainModule;

        _decimalRef = module.ImportReference(typeof(decimal));
        _boolRef = module.ImportReference(typeof(bool));
        _stringRef = module.ImportReference(typeof(string));

        _decimalCtor = module.ImportReference(typeof(decimal).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)]));
        _add = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Add), [typeof(decimal), typeof(decimal)]));
        _subtract = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Subtract), [typeof(decimal), typeof(decimal)]));
        _multiply = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Multiply), [typeof(decimal), typeof(decimal)]));
        _divide = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Divide), [typeof(decimal), typeof(decimal)]));
        _modulus = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Remainder), [typeof(decimal), typeof(decimal)]));
        _negate = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Negate), [typeof(decimal)]));

        _class = new TypeDefinition(assemblyName, className, TypeAttributes.Public | TypeAttributes.Class)
        {
            BaseType = module.ImportReference(typeof(object))
        };

        module.Types.Add(_class);
    }

    public Assembly Emit(SemanticNode node)
    {
        if (node is OperandNode op && op.TypeDescriptor is SimpleTypeDescriptor st)
        {
            var method = new MethodDefinition(_methodName, MethodAttributes.Public | MethodAttributes.Static, GetType(st));
            _class.Methods.Add(method);

            var il = method.Body.GetILProcessor();

            EmitForExpression(node, il);

            il.Emit(OpCodes.Ret);

            var ms = new MemoryStream();
            _assembly.Write(ms);
            return Assembly.Load(ms.GetBuffer());
        }
        throw new ArgumentOutOfRangeException($"Can't compile an expression of type {node.GetType().FullName}");
    }

    private void EmitForExpression(SemanticNode node, ILProcessor il)
    {
        switch (node)
        {
            case NumberLiteralNode n:
                {
                    var d = decimal.Parse(n.Literal.Token.Word);
                    var parts = DecomposeDecimal(d);

                    il.Emit(OpCodes.Ldc_I4, parts.Low);
                    il.Emit(OpCodes.Ldc_I4, parts.Mid);
                    il.Emit(OpCodes.Ldc_I4, parts.High);
                    il.Emit(OpCodes.Ldc_I4, parts.IsNegative ? 1 : 0);
                    il.Emit(OpCodes.Ldc_I4, (int)parts.Scale);
                    il.Emit(OpCodes.Newobj, _decimalCtor);
                    break;
                }
            case BooleanLiteralNode b:
                {
                    if (b.Literal.Token.Class == TokenClass.TrueKeyword)
                    {
                        il.Emit(OpCodes.Ldc_I4_1);
                        break;
                    }

                    if (b.Literal.Token.Class == TokenClass.FalseKeyword)
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        break;
                    }

                    throw new ArgumentOutOfRangeException("Boolean literal must either be true or false");
                }
            case StringLiteralNode s:
                {
                    il.Emit(OpCodes.Ldstr, s.Literal.Token.Word.Trim('"'));
                    break;
                }
            case UnaryOperationNode u:
                {
                    EmitForExpression(u.Operand, il);

                    if (u.Operator.Class == TokenClass.MinusOperator)
                    {
                        il.Emit(OpCodes.Call, _negate);
                        break;
                    }

                    if (u.Operator.Class == TokenClass.NegationOperator)
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                    }

                    throw new ArgumentOutOfRangeException("A unary operation must either have a '-' or a '!' operator");
                }
            case NumberOperationNode nop:
                {
                    EmitForExpression(nop.Left, il);
                    EmitForExpression(nop.Right, il);
                    il.Emit(OpCodes.Call, GetDecimalOperation(nop.BinaryExpr.Operator.Class));
                    break;
                }
            case BooleanOperationNode bop:
                {
                    EmitForExpression(bop.Left, il);

                    if (bop.BinaryExpr.Operator.Class == TokenClass.AndOperator)
                    {
                        var refFalse = il.Create(OpCodes.Ldc_I4_0);
                        var refNop = il.Create(OpCodes.Nop);

                        il.Emit(OpCodes.Brfalse, refFalse);

                        EmitForExpression(bop.Right, il);
                        il.Emit(OpCodes.Br, refNop);

                        il.Append(refFalse);
                        il.Append(refNop);
                        break;
                    }

                    if (bop.BinaryExpr.Operator.Class == TokenClass.OrOperator)
                    {
                        var refTrue = il.Create(OpCodes.Ldc_I4_1);
                        var refNop = il.Create(OpCodes.Nop);

                        il.Emit(OpCodes.Brtrue, refTrue);

                        EmitForExpression(bop.Right, il);
                        il.Emit(OpCodes.Br, refNop);

                        il.Append(refTrue);
                        il.Append(refNop);
                        break;
                    }

                    throw new ArgumentOutOfRangeException("A boolean operation must either have a '&&' or a '||' operator");
                }
            default:
                throw new ArgumentOutOfRangeException($"Can't compile an expression of type {node.GetType().FullName}");
        }
    }

    private static (int Low, int Mid, int High, bool IsNegative, byte Scale) DecomposeDecimal(decimal d)
    {
        int[] parts = decimal.GetBits(d);
        var isNegative = (parts[3] & 0x80000000) != 0;
        var scale = (byte)((parts[3] >> 16) & 0x7F);
        return (parts[0], parts[1], parts[2], isNegative, scale);
    }

    private MethodReference GetDecimalOperation(TokenClass c)
    {
        return c switch
        {
            TokenClass.PlusOperator => _add,
            TokenClass.MinusOperator => _subtract,
            TokenClass.TimesOperator => _multiply,
            TokenClass.DivisionOperator => _divide,
            TokenClass.PercentOperator => _modulus,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private TypeReference GetType(SimpleTypeDescriptor st)
    {
        return st.SimpleType switch
        {
            SimpleType.Number => _decimalRef,
            SimpleType.Bool => _boolRef,
            SimpleType.String => _stringRef,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}