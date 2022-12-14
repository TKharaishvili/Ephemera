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
            var actual = await CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("\"Hello, world!\"", "Hello, world!")]
        public async Task Simple_Expressions_Work(string expression, object expected)
        {
            var actual = await CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3", 3)]
        [InlineData("3.5", 3.5)]
        public async Task Number_Literal_Expressions_Work(string expression, decimal expected)
        {
            var actual = await CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Null_Expression_Works()
        {
            var cs = TranspileToCSharp($"def x: number? = null");
            var state = await CSharpScript.RunAsync(cs);
            var actual = Assert.Single(state.Variables).Value;
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        //[InlineData("!!false", false)] TODO - double negation should definitely work here!
        public async Task Boolean_Unary_Expressions_Work(string expression, bool expected)
        {
            var actual = await CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("-3", -3)]
        [InlineData("-3.5", -3.5)]
        //[InlineData("--3", 3)] TODO - should the double negation work?
        public async Task Number_Unary_Expressions_Work(string expression, decimal expected)
        {
            var actual = await CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        private async Task<object> CompileExpression(string expression)
        {
            var cs = TranspileToCSharp($"def x = {expression}");
            var state = await CSharpScript.RunAsync(cs);
            var actual = Assert.Single(state.Variables).Value;
            return actual;
        }

        private string TranspileToCSharp(string source)
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