using Ephemera.Emitting;
using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.SemanticAnalysis;
using Ephemera.Transpilation;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ephemera.Tests
{
    internal static class TestHelpers
    {
        internal static Task<object> RunExpression(string expression, bool il)
        {
            return il ? CompileExpressionToIL(expression) : CompileExpression(expression);
        }

        internal static Task<object> CompileExpression(string expression)
        {
            return CompileCode($"def result = {expression}");
        }

        internal static Task<object> CompileExpressionToIL(string expression)
        {
            var assembly = EmitIL(expression);
            var result = InvokeGeneratedMethod(assembly);
            return Task.FromResult(result);
        }

        internal static async Task<object> CompileCode(string code)
        {
            var cs = TranspileToCSharp(code);
            var state = await CSharpScript.RunAsync(cs);
            var actual = state.Variables.Single(v => v.Name == "result").Value;
            return actual;
        }

        internal static string TranspileToCSharp(string source)
        {
            var additionalSource = @"
fun Add(x:number, y:number)
{
    return x + y
}

fun Inc(pre x:number)
{
    return x + 1
}

fun IncBy(pre x:number, y:number)
{
    return x + y
}

fun TakesNumReturnsNull(pre x:number):number?
{
    return null
}

fun TakesAndReturnsNullableNum(pre x:number?)
{
    return x
}

[<""True"">]
fun True():bool

[<""False"">]
fun False():bool

[<""Three"">]
fun Three():number

[<""Four"">]
fun Four():number
";

            source = additionalSource + source;

            var tokens = Lexer.Lex(source)
                .Where(x => x.Class != TokenClass.Whitespace && x.Class != TokenClass.NewLine)
                .ToList();

            if (tokens == null)
            {
                throw new Exception("Lexing failed");
            }

            var parseResult = Parser.Parse(tokens);
            if (parseResult.Fail)
            {
                throw new Exception("Parsing failed with an error - " + parseResult.Error.Message);
            }

            var analyser = new SemanticAnalyser();
            var nodes = analyser.Analyse(parseResult.Expr);

            if (analyser.CodeErrors.Any())
            {
                throw new Exception("Analyser failed with error(s) - " + analyser.CodeErrors.First().Message);
            }

            var cSharpCode = new CSharpTranspiler().Transpile(nodes, generateTestingSource: true);

            return cSharpCode;
        }

        internal static Assembly EmitIL(string source)
        {
            var tokens = Lexer.Lex(source)
                .Where(x => x.Class != TokenClass.Whitespace && x.Class != TokenClass.NewLine)
                .ToList();

            if (tokens == null)
            {
                throw new Exception("Lexing failed");
            }

            var parseResult = Parser.Parse(tokens);
            if (parseResult.Fail)
            {
                throw new Exception("Parsing failed with an error - " + parseResult.Error.Message);
            }

            var analyser = new SemanticAnalyser();
            var nodes = analyser.Analyse(parseResult.Expr);

            if (analyser.CodeErrors.Any())
            {
                throw new Exception("Analyser failed with error(s) - " + analyser.CodeErrors.First().Message);
            }

            var ilEmitter = new ILEmitter("Ephemera", "__GeneratedClass", "__GeneratedMethod");
            return ilEmitter.Emit(nodes[0]);
        }

        private static object InvokeGeneratedMethod(Assembly assembly)
        {
            var type = assembly.GetType("Ephemera.__GeneratedClass");
            var method = type.GetMethod("__GeneratedMethod", BindingFlags.Static | BindingFlags.Public);
            var result = method.Invoke(null, null);
            return result;
        }
    }
}
