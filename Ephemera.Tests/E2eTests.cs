using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TH = Ephemera.Tests.TestHelpers;

namespace Ephemera.Tests
{
    public class E2eTests
    {
        [Theory]
        [InlineData("3 + 4", 7, false)]
        [InlineData("3 + 4", 7, true)]
        [InlineData("3 - 4", -1, false)]
        [InlineData("3 - 4", -1, true)]
        [InlineData("3 + -4", -1, false)]
        [InlineData("3 + -4", -1, true)]
        [InlineData("-4 + 3", -1, false)]
        [InlineData("-4 + 3", -1, true)]
        [InlineData("3 * 4", 12, false)]
        [InlineData("3 * 4", 12, true)]
        [InlineData("3 / 4", 0.75, false)]
        [InlineData("3 / 4", 0.75, true)]
        [InlineData("3 % 4", 3, false)]
        [InlineData("3 % 4", 3, true)]
        [InlineData("7 % 4", 3, false)]
        [InlineData("7 % 4", 3, true)]
        [InlineData("5 % 4", 1, false)]
        [InlineData("5 % 4", 1, true)]
        public async Task Simple_Arithmetic_Computations_Work(string expression, decimal expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("true", true, false)]
        [InlineData("true", true, true)]
        [InlineData("false", false, false)]
        [InlineData("false", false, true)]
        [InlineData("\"Hello, world!\"", "Hello, world!", false)]
        [InlineData("\"Hello, world!\"", "Hello, world!", true)]
        public async Task Simple_Expressions_Work(string expression, object expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3", 3, false)]
        [InlineData("3", 3, true)]
        [InlineData("3.5", 3.5, false)]
        [InlineData("3.5", 3.5, true)]
        public async Task Number_Literal_Expressions_Work(string expression, decimal expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Null_Expression_Works()
        {
            var cs = TH.TranspileToCSharp($"def result: number? = null");
            var state = await CSharpScript.RunAsync(cs);
            var actual = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Null(actual);
        }

        [Fact]
        public async Task Variable_Expression_Works()
        {
            var src = @"
def x = 5
def result = x
";
            var cs = TH.TranspileToCSharp(src);
            var state = await CSharpScript.RunAsync(cs);
            var actual = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Equal(5m, actual);
        }

        [Theory]
        [InlineData("Three()", 3)]
        [InlineData("Add(3, 4)", 7)]
        [InlineData("3.Inc()", 4)]
        [InlineData("3.Inc().IncBy(5)", 9)]
        public async Task Function_Invocation_Expression_Works(string expression, decimal expected)
        {
            var actual = await TH.CompileExpression(expression);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3", "x?.Inc()", 4)]
        [InlineData("3", "x?.IncBy(4)", 7)]
        public Task Conditional_Function_Invocation_Expression_Works_For_Non_Nulls(string valueOfX, string expression, decimal expected)
        {
            return Conditional_Function_Invocation_Expression_Works(valueOfX, expression, expected);
        }

        [Theory]
        [InlineData("null", "x?.Inc()", null)]
        [InlineData("3", "x?.TakesNumReturnsNull()?.Inc()", null)]
        [InlineData("null", "x?.TakesNumReturnsNull()?.Inc()", null)]
        public Task Conditional_Function_Invocation_Expression_Works_For_Nulls(string valueOfX, string expression, object expected)
        {
            return Conditional_Function_Invocation_Expression_Works(valueOfX, expression, expected);
        }

        [Theory]
        [InlineData("3", "x?.Inc().Inc()", 5)]
        [InlineData("3", "x.TakesAndReturnsNullableNum()?.Inc()", 4)]
        public Task Mixed_Function_Invocation_Expression_Works_For_Non_Nulls(string valueOfX, string expression, decimal expected)
        {
            return Conditional_Function_Invocation_Expression_Works(valueOfX, expression, expected);
        }

        [Theory]
        [InlineData("null", "x?.Inc().Inc()", null)]
        [InlineData("null", "x.TakesAndReturnsNullableNum()?.Inc()", null)]
        public Task Mixed_Function_Invocation_Expression_Works_For_Nulls(string valueOfX, string expression, object expected)
        {
            return Conditional_Function_Invocation_Expression_Works(valueOfX, expression, expected);
        }

        private async Task Conditional_Function_Invocation_Expression_Works(string valueOfX, string expression, object expected)
        {
            var src = $@"
def x: number? = {valueOfX}
def result = {expression}
";
            var cs = TH.TranspileToCSharp(src);
            var state = await CSharpScript.RunAsync(cs);
            var actual = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("!true", false, false)]
        [InlineData("!true", false, true)]
        [InlineData("!false", true, false)]
        [InlineData("!false", true, true)]
        //[InlineData("!!false", false)] TODO - double negation should definitely work here!
        public async Task Boolean_Unary_Expressions_Work(string expression, bool expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("-3", -3, false)]
        [InlineData("-3", -3, true)]
        [InlineData("-3.5", -3.5, false)]
        [InlineData("-3.5", -3.5, true)]
        //[InlineData("--3", 3)] TODO - should the double negation work?
        //TODO these are probabaly parsed as just number literals, which means this is not an actual unary operation
        public async Task Number_Unary_Expressions_Work(string expression, decimal expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("true && true", true, false)]
        [InlineData("true && true", true, true)]
        [InlineData("true && false", false, false)]
        [InlineData("true && false", false, true)]
        [InlineData("true || true", true, false)]
        [InlineData("true || true", true, true)]
        [InlineData("true || false", true, false)]
        [InlineData("true || false", true, true)]
        [InlineData("false && false", false, false)]
        [InlineData("false && false", false, true)]
        [InlineData("false && true", false, false)]
        [InlineData("false && true", false, true)]
        [InlineData("false || false", false, false)]
        [InlineData("false || false", false, true)]
        [InlineData("false || true", true, false)]
        [InlineData("false || true", true, true)]
        public async Task Boolean_Binary_Expressions_Work(string expression, bool expected, bool il)
        {
            var actual = await TH.RunExpression(expression, il);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("True() && True()", 2, 0, true)]
        [InlineData("True() && False()", 1, 1, false)]
        [InlineData("True() || True()", 1, 0, true)]
        [InlineData("True() || False()", 1, 0, true)]
        [InlineData("False() && False()", 0, 1, false)]
        [InlineData("False() && True()", 0, 1, false)]
        [InlineData("False() || False()", 0, 2, false)]
        [InlineData("False() || True()", 1, 1, true)]
        public async Task Boolean_Binary_Expressions_Short_Circuit_In_Appropriate_Cases
        (
            string expression,
            int expectedTrueCount,
            int expectedFalseCount,
            bool expectedResult
        )
        {
            var cs = TH.TranspileToCSharp($"def result = {expression}");
            var state = await CSharpScript.RunAsync(cs);
            var actualTrueCount = state.Variables.Single(v => v.Name == "trueCount").Value;
            var actualFalseCount = state.Variables.Single(v => v.Name == "falseCount").Value;
            var actualResult = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Equal(expectedTrueCount, actualTrueCount);
            Assert.Equal(expectedFalseCount, actualFalseCount);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("3 < 4", true)]
        [InlineData("3 > 4", false)]
        [InlineData("5 < 4", false)]
        [InlineData("5 > 4", true)]
        public async Task Comparison_Expressions_Work(string expression, bool expected)
        {
            var actual = await TH.RunExpression(expression, il: false);
            Assert.Equal(expected, actual);

            actual = await TH.RunExpression(expression, il: true);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3 < 4 < 5", true)]
        [InlineData("3 > 4 > 5", false)]
        [InlineData("5 < 4 < 3", false)]
        [InlineData("5 > 4 > 3", true)]
        [InlineData("5 > 4 < 5", true)]
        [InlineData("5 < 4 > 5", false)]
        [InlineData("3 > 4 < 3", false)]
        [InlineData("3 < 4 > 3", true)]
        public async Task Triple_Comparison_Expressions_Work(string expression, bool expected)
        {
            var actual = await TH.RunExpression(expression, il: false);
            Assert.Equal(expected, actual);

            actual = await TH.RunExpression(expression, il: true);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3 < Four() < 5", true)]
        [InlineData("3 > Four() > 5", false)]
        [InlineData("5 < Four() < 3", false)]
        [InlineData("5 > Four() > 3", true)]
        [InlineData("5 > Four() < 5", true)]
        [InlineData("5 < Four() > 5", false)]
        [InlineData("3 > Four() < 3", false)]
        [InlineData("3 < Four() > 3", true)]
        public async Task Triple_Comparison_Expression_Middle_Operand_Gets_Evaluated_Once(string expression, bool expectedResult)
        {
            var cs = TH.TranspileToCSharp($"def result = {expression}");
            var state = await CSharpScript.RunAsync(cs);
            var actualFourCount = state.Variables.Single(v => v.Name == "fourCount").Value;
            var actualResult = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Equal(1, actualFourCount);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("2 < 3 < 4 < 5", true)]
        [InlineData("2 > 3 > 4 > 5", false)]
        [InlineData("5 < 4 < 3 < 2", false)]
        [InlineData("5 > 4 > 3 > 2", true)]
        public async Task Quadruple_Comparison_Expressions_Work(string expression, bool expected)
        {
            var actual = await TH.RunExpression(expression, il: false);
            Assert.Equal(expected, actual);

            actual = await TH.RunExpression(expression, il: true);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2 < Three() < Four() < 5", 1, 1, true)]
        [InlineData("2 > Three() > Four() > 5", 1, 0, false)]
        [InlineData("5 < Four() < Three() < 2", 0, 1, false)]
        [InlineData("5 > Four() > Three() > 2", 1, 1, true)]
        public async Task Quadruple_Comparison_Expression_Middle_Operands_Get_Evaluated_Once(string expression, int expectedThreeCount, int expectedFourCount, bool expectedResult)
        {
            var cs = TH.TranspileToCSharp($"def result = {expression}");
            var state = await CSharpScript.RunAsync(cs);
            var actualThreeCount = state.Variables.Single(v => v.Name == "threeCount").Value;
            var actualFourCount = state.Variables.Single(v => v.Name == "fourCount").Value;
            var actualResult = state.Variables.Single(v => v.Name == "result").Value;
            Assert.Equal(expectedThreeCount, actualThreeCount);
            Assert.Equal(expectedFourCount, actualFourCount);
            Assert.Equal(expectedResult, actualResult);
        }
    }
}