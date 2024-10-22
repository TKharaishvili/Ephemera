using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis;
using Ephemera.SemanticAnalysis.Typing;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly MethodReference _greaterThan;
    private readonly MethodReference _greaterThanOrEqual;
    private readonly MethodReference _lessThan;
    private readonly MethodReference _lessThanOrEqual;

    private readonly Dictionary<string, MethodReference> _methods = [];

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

        _greaterThan = module.ImportReference(typeof(decimal).GetMethod("op_GreaterThan"));
        _greaterThanOrEqual = module.ImportReference(typeof(decimal).GetMethod("op_GreaterThanOrEqual"));
        _lessThan = module.ImportReference(typeof(decimal).GetMethod("op_LessThan"));
        _lessThanOrEqual = module.ImportReference(typeof(decimal).GetMethod("op_LessThanOrEqual"));

        _class = new TypeDefinition(assemblyName, className, TypeAttributes.Public | TypeAttributes.Class)
        {
            BaseType = module.ImportReference(typeof(object))
        };

        module.Types.Add(_class);
    }

    public Assembly Emit(IReadOnlyList<SemanticNode> nodes)
    {
        var newNodes = new List<SemanticNode>(nodes);
        var lastOperand = nodes.Last() as OperandNode;
        if (lastOperand != null)
        {
            newNodes[newNodes.Count - 1] = new ReturnNode(new Expr(), lastOperand.TypeDescriptor, lastOperand);
        }

        var returnType = lastOperand?.TypeDescriptor ?? new SimpleTypeDescriptor(SimpleType.Unit);
        EmitFuncDefinition(_methodName, [], returnType, newNodes);

        var ms = new MemoryStream();
        _assembly.Write(ms);
        return Assembly.Load(ms.GetBuffer());
    }

    private void EmitFuncDefinition(string name, IReadOnlyList<DefinitionNode> @params, TypeDescriptor returnType, IReadOnlyList<SemanticNode> statements)
    {
        var method = new MethodDefinition(name, MethodAttributes.Public | MethodAttributes.Static, GetType(returnType));
        var methodSymbols = new Dictionary<DefinitionNode, (ParameterDefinition Param, VariableDefinition Var)>();
        _class.Methods.Add(method);
        _methods.Add(name, method);

        // Expressions like x < y < z need to store an intermediate number value somewhere and
        // that's what this variable is for. Only functions that use such expressions would need it
        // but my focus is not on optimisation at the moment so I'm declaring it in every function
        method.Body.Variables.Add(new VariableDefinition(_decimalRef));

        for (int i = 0; i < @params.Count; i++)
        {
            var param = @params[i];
            method.Parameters.Add(new ParameterDefinition(param.Name, ParameterAttributes.None, GetType(param.Type)));
            methodSymbols.Add(param, (method.Parameters[i], null));
        }

        var il = method.Body.GetILProcessor();

        foreach (var statement in statements)
        {
            Emit(statement, il, method, methodSymbols);
        }
    }

    private void Emit(SemanticNode node, ILProcessor il, MethodDefinition method, Dictionary<DefinitionNode, (ParameterDefinition Param, VariableDefinition Var)> methodSymbols)
    {
        EmitInner(node);

        void EmitInner(SemanticNode node)
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
                        EmitInner(u.Operand);

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
                        EmitInner(nop.Left);
                        EmitInner(nop.Right);
                        il.Emit(OpCodes.Call, GetDecimalOperation(nop.BinaryExpr.Operator.Class));
                        break;
                    }
                case NumberBooleanOperationNode nbop:
                    {
                        EmitInner(nbop.Left);

                        var op = nbop.BinaryExpr.Operator.Class;
                        var right = nbop.Right;

                        var falseValue = il.Create(OpCodes.Ldc_I4_0);
                        var nop = il.Create(OpCodes.Nop);

                        while (right is NumberBooleanOperationNode r)
                        {
                            EmitInner(r.Left);

                            //duplicate the right value and store it in a local variable
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc_0);

                            il.Emit(OpCodes.Call, GetDecimalOperation(op));
                            //if the result of the comparison is false, jump to the false value
                            il.Emit(OpCodes.Brfalse, falseValue);

                            il.Emit(OpCodes.Ldloc_0);

                            op = r.BinaryExpr.Operator.Class;
                            right = r.Right;
                        }

                        EmitInner(right);

                        il.Emit(OpCodes.Call, GetDecimalOperation(op));
                        il.Emit(OpCodes.Br, nop);

                        il.Append(falseValue);
                        il.Append(nop);
                        break;
                    }
                case BooleanOperationNode bop:
                    {
                        EmitInner(bop.Left);

                        if (bop.BinaryExpr.Operator.Class == TokenClass.AndOperator)
                        {
                            var refFalse = il.Create(OpCodes.Ldc_I4_0);
                            var refNop = il.Create(OpCodes.Nop);

                            il.Emit(OpCodes.Brfalse, refFalse);

                            EmitInner(bop.Right);
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

                            EmitInner(bop.Right);
                            il.Emit(OpCodes.Br, refNop);

                            il.Append(refTrue);
                            il.Append(refNop);
                            break;
                        }

                        throw new ArgumentOutOfRangeException("A boolean operation must either have a '&&' or a '||' operator");
                    }
                case ReturnNode rn:
                    {
                        EmitInner(rn.Value);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case FuncDefinitionNode fd:
                    {
                        EmitFuncDefinition(fd.Name, fd.Params, fd.ReturnType, fd.Body.Children);
                        break;
                    }
                case FuncInvocationNode fi:
                    {
                        foreach (var arg in fi.Params)
                        {
                            EmitInner(arg);
                        }

                        il.Emit(OpCodes.Call, _methods[fi.Name]);
                        break;
                    }
                case IdentifierNode i:
                    {
                        var (p, v) = methodSymbols[i.Definition];
                        if (p != null)
                        {
                            il.Emit(OpCodes.Ldarg, p);
                            break;
                        }

                        if (v != null)
                        {
                            il.Emit(OpCodes.Ldloc, v);
                            break;
                        }

                        throw new ArgumentOutOfRangeException("An identifier must either be a parameter or a variable");
                    }
                case VariableDefinitionNode vd:
                    {
                        var v = new VariableDefinition(GetType(vd.Type));
                        method.Body.Variables.Add(v);
                        methodSymbols.Add(vd, (Param: null, Var: v));

                        EmitInner(vd.Source);
                        il.Emit(OpCodes.Stloc, v);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException($"Can't compile an expression of type {node.GetType().FullName}");
            }
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
            TokenClass.GreaterOperator => _greaterThan,
            TokenClass.GreaterOrEqualsOperator => _greaterThanOrEqual,
            TokenClass.LessOperator => _lessThan,
            TokenClass.LessOrEqualsOperator => _lessThanOrEqual,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private TypeReference GetType(TypeDescriptor td)
    {
        //TODO this needs to support all types
        return GetType((SimpleTypeDescriptor)td);
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