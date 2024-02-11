using Ephemera.Lexing;
using Ephemera.SemanticAnalysis.Nodes;
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
    private readonly MethodReference _decimalCtor;
    private readonly MethodReference _add;
    private readonly MethodReference _subtract;
    private readonly MethodReference _multiply;
    private readonly MethodReference _divide;
    private readonly MethodReference _modulus;

    public ILEmitter(string assemblyName, string className, string methodName)
    {
        _methodName = methodName;

        var name = new AssemblyNameDefinition(assemblyName, new Version(1, 0));
        _assembly = AssemblyDefinition.CreateAssembly(name, assemblyName, ModuleKind.Dll);
        var module = _assembly.MainModule;

        _decimalRef = module.ImportReference(typeof(decimal));
        _decimalCtor = module.ImportReference(typeof(decimal).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)]));
        _add = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Add), [typeof(decimal), typeof(decimal)]));
        _subtract = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Subtract), [typeof(decimal), typeof(decimal)]));
        _multiply = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Multiply), [typeof(decimal), typeof(decimal)]));
        _divide = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Divide), [typeof(decimal), typeof(decimal)]));
        _modulus = module.ImportReference(typeof(decimal).GetMethod(nameof(decimal.Remainder), [typeof(decimal), typeof(decimal)]));

        _class = new TypeDefinition(assemblyName, className, TypeAttributes.Public | TypeAttributes.Class)
        {
            BaseType = module.ImportReference(typeof(object))
        };

        module.Types.Add(_class);
    }

    private bool ExtractNumber(OperandNode operand, out decimal result)
    {
        if (operand is NumberLiteralNode n)
        {
            result = decimal.Parse(n.Literal.Token.Word);
            return true;
        }

        if (operand is UnaryOperationNode lu &&
            lu.Operator.Class == TokenClass.MinusOperator &&
            lu.Operand is NumberLiteralNode l)
        {
            result = -decimal.Parse(l.Literal.Token.Word);
            return true;
        }

        result = 0;
        return false;
    }

    public Assembly Emit(SemanticNode node)
    {
        if (node is NumberOperationNode op && ExtractNumber(op.Left, out var ld) && ExtractNumber(op.Right, out var rd))
        {
            var method = new MethodDefinition(_methodName, MethodAttributes.Public | MethodAttributes.Static, _decimalRef);
            _class.Methods.Add(method);

            var il = method.Body.GetILProcessor();

            var lparts = DecomposeDecimal(ld);
            var rparts = DecomposeDecimal(rd);

            il.Emit(OpCodes.Ldc_I4, lparts.Low);
            il.Emit(OpCodes.Ldc_I4, lparts.Mid);
            il.Emit(OpCodes.Ldc_I4, lparts.High);
            il.Emit(OpCodes.Ldc_I4, lparts.IsNegative ? 1 : 0);
            il.Emit(OpCodes.Ldc_I4, (int)lparts.Scale);
            il.Emit(OpCodes.Newobj, _decimalCtor);

            il.Emit(OpCodes.Ldc_I4, rparts.Low);
            il.Emit(OpCodes.Ldc_I4, rparts.Mid);
            il.Emit(OpCodes.Ldc_I4, rparts.High);
            il.Emit(OpCodes.Ldc_I4, rparts.IsNegative ? 1 : 0);
            il.Emit(OpCodes.Ldc_I4, (int)rparts.Scale);
            il.Emit(OpCodes.Newobj, _decimalCtor);

            il.Emit(OpCodes.Call, GetDecimalOperation(op.BinaryExpr.Operator.Class));
            il.Emit(OpCodes.Ret);

            var ms = new MemoryStream();
            _assembly.Write(ms);
            return Assembly.Load(ms.GetBuffer());
        }

        throw new ArgumentOutOfRangeException($"Can't compile an expression of type {node.GetType().FullName}");
    }

    private static (int Low, int Mid, int High, bool IsNegative, byte Scale) DecomposeDecimal(decimal d)
    {
        int[] parts = decimal.GetBits(d);
        var sign = (parts[3] & 0x80000000) != 0;
        var scale = (byte)((parts[3] >> 16) & 0x7F);
        return (parts[0], parts[1], parts[2], sign, scale);
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
}