using Ephemera.Emitting;
using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis;
using Ephemera.Transpilation;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ephemera.Tests;

internal static class TestHelpers
{
    internal static Task<object> RunExpression(string expression, bool il, string contextSrc = "")
    {
        return il ? CompileExpressionToIL(expression, contextSrc) : CompileExpression(expression, contextSrc);
    }

    internal static string GetError(string source)
    {
        var result = TryGetNodes(source);
        return result.Error;
    }

    internal static Task<object> CompileExpression(string expression, string contextSrc = "")
    {
        return CompileCode($"{contextSrc} \n def result = {expression}");
    }

    private static Task<object> CompileExpressionToIL(string expression, string contextSrc = "")
    {
        var assembly = EmitIL($"{contextSrc} \n def result = {expression}");
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
        var nodes = GetNodes(source);
        var cSharpCode = new CSharpTranspiler().Transpile(nodes, generateTestingSource: true);
        return cSharpCode;
    }

    private static Assembly EmitIL(string source)
    {
        var nodes = GetNodes(source);
        var newNodes = new List<SemanticNode>(nodes);
        var node = (VariableDefinitionNode)nodes.Last();
        newNodes[^1] = new ReturnNode(new Expr(), node.Source.TypeDescriptor, node.Source);
        var returnType = node.Source.TypeDescriptor;
        var funcDefinition = new FuncDefinitionNode(
            new Expr(),
            new Token(TokenClass.Identifier, new Lexeme("__GeneratedMethod", 0, 0)),
            [],
            returnType,
            IsExtension: false,
            new BlockNode(new Expr(), newNodes, returnType)
        );

        var ilEmitter = new ILEmitter("Ephemera", "__GeneratedClass", "__GeneratedMethod");
        return ilEmitter.Emit(funcDefinition);
    }

    private static object InvokeGeneratedMethod(Assembly assembly)
    {
        var type = assembly.GetType("Ephemera.__GeneratedClass");
        var method = type.GetMethod("__GeneratedMethod", BindingFlags.Static | BindingFlags.Public);
        var result = method.Invoke(null, null);
        return result;
    }

    private static IReadOnlyList<SemanticNode> GetNodes(string source)
    {
        return TryGetNodes(source) switch
        {
            (var nodes, ErrorType.None, _) => nodes,
            (_, ErrorType.Lexer, _) => throw new Exception("Lexing failed"),
            (_, ErrorType.Parser, var error) => throw new Exception("Parsing failed with an error - " + error),
            (_, ErrorType.Analyser, var error) => throw new Exception("Analyser failed with error(s) - " + error),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static (IReadOnlyList<SemanticNode> Nodes, ErrorType ErrorType, string Error) TryGetNodes(string source)
    {
        var tokens = Lexer.Lex(source)
            .Where(x => x.Class != TokenClass.Whitespace && x.Class != TokenClass.NewLine)
            .ToList();

        if (tokens == null)
        {
            return (null, ErrorType.Lexer, null);
        }

        var parseResult = Parser.Parse(tokens);
        if (parseResult.Fail)
        {
            return (null, ErrorType.Parser, parseResult.Error.Message);
        }

        var analyser = new SemanticAnalyser();
        var nodes = analyser.Analyse(parseResult.Expr);

        if (analyser.CodeErrors.Any())
        {
            return (null, ErrorType.Analyser, analyser.CodeErrors.First().Message);
        }

        return (nodes, ErrorType.None, null);
    }

    private enum ErrorType
    {
        None,
        Lexer,
        Parser,
        Analyser,
    }
}
