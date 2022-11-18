using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.SemanticAnalysis;
using Ephemera.Transpilation;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Ephemera.Tests
{
    public class E2eTests
    {
        [Theory]
        [InlineData("3 + 4", 7)]
        [InlineData("3 - 4", -1)]
        [InlineData("3 + -4", -1)]
        [InlineData("-4 + 3", -1)]
        [InlineData("3 * 4", 12)]
        [InlineData("3 / 4", 0.75)]
        [InlineData("3 % 4", 3)]
        [InlineData("7 % 4", 3)]
        [InlineData("5 % 4", 1)]
        public async Task Simple_Arithmetic_Computations_Work(string expression, decimal expected)
        {
            var cs = TranspileToCSharp($"def x = {expression}");
            var state = await CSharpScript.RunAsync(cs);
            var actual = Assert.Single(state.Variables).Value;
            Assert.Equal(expected, actual);
        }

        string TranspileToCSharp(string source)
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

            var cSharpCode = new CSharpTranspiler().Transpile(nodes);

            return cSharpCode;
        }
    }
}