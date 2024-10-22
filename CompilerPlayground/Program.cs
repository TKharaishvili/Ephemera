using Ephemera.Lexing;
using System.Collections.Generic;
using System.Linq;
using System;
using Ephemera.Parsing;
using Ephemera.SemanticAnalysis;
using Ephemera.Parsing.Expressions;
using Ephemera.Transpilation;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Ephemera.Reusable;

namespace CompilerPlayground;

class Program
{
    static void Main(string[] args)
    {
        args?.GetSource()
            ?.Lex()
            ?.Parse()
            ?.Analyse()
            ?.Transpile()
            ?.Run();

        Console.ReadKey();
    }
}

internal static class ProgramExtensions
{
    internal static string GetSource(this string[] args)
    {
        if (args != null && args.All(File.Exists))
        {
            return args.Select(File.ReadAllText).JoinStrings(Environment.NewLine);
        }
        Console.WriteLine("Source not found");
        return null;
    }

    internal static IReadOnlyList<Token> Lex(this string source) =>
        Lexer.Lex(source).Where(x => x.Class != TokenClass.Whitespace && x.Class != TokenClass.NewLine).ToList();

    internal static RootExpr Parse(this IReadOnlyList<Token> tokens)
    {
        if (tokens == null) return null;
        var result = Parser.Parse(tokens);
        if (result.Fail)
        {
            Console.WriteLine(result.Error.Message);
            return null;
        }
        return result.Expr;
    }

    internal static IReadOnlyList<SemanticNode> Analyse(this RootExpr expr)
    {
        if (expr == null) return null;
        var analyser = new SemanticAnalyser();
        var result = analyser.Analyse(expr);

        if (analyser.CodeErrors.Any())
        {
            foreach (var error in analyser.CodeErrors)
            {
                Console.WriteLine(error.Message);
            }
            return null;
        }
        return result;
    }

    internal static string Transpile(this IReadOnlyList<SemanticNode> nodes)
    {
        if (nodes == null) return null;
        var result = new CSharpTranspiler().Transpile(nodes);
        return result;
    }

    internal static void Run(this string cSharp)
    {
        if (string.IsNullOrWhiteSpace(cSharp))
        {
            return;
        }

        try
        {
            CSharpScript.RunAsync(cSharp).Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
