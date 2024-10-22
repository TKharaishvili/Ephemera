using System.Threading.Tasks;
using Xunit;
using TH = Ephemera.Tests.TestHelpers;

namespace Ephemera.Tests;

public class OperatorPrecedenceE2eTests
{
    [Theory]
    [InlineData("2 + 3 * 4", 14, false)]
    [InlineData("2 + 3 * 4", 14, true)]
    [InlineData("10 - 2 * 3", 4, false)]
    [InlineData("10 - 2 * 3", 4, true)]
    public async Task Multiplication_Binds_Stronger_Than_Addition_And_Subtraction(string expression, decimal expected, bool il)
    {
        var actual = await TH.RunExpression(expression, il);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2 + 3 / 4", 2.75, false)]
    [InlineData("2 + 3 / 4", 2.75, true)]
    [InlineData("10 - 9 / 3", 7, false)]
    [InlineData("10 - 9 / 3", 7, true)]
    public async Task Division_Binds_Stronger_Than_Addition_And_Subtraction(string expression, decimal expected, bool il)
    {
        var actual = await TH.RunExpression(expression, il);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2 + 9 % 4", 3, false)]
    [InlineData("2 + 9 % 4", 3, true)]
    [InlineData("10 - 9 % 7", 8, false)]
    [InlineData("10 - 9 % 7", 8, true)]
    public async Task Modulo_Binds_Stronger_Than_Addition_And_Subtraction(string expression, decimal expected, bool il)
    {
        var actual = await TH.RunExpression(expression, il);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("5 > 3 + 2", false)]
    [InlineData("5 >= 3 + 2", true)]
    [InlineData("5 < 3 + 2", false)]
    [InlineData("5 <= 3 + 2", true)]
    public async Task Addition_Binds_Stronger_Than_Comparison(string expression, bool expected)
    {
        var actual = await TH.CompileExpression(expression);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("5 > 7 - 2", false)]
    [InlineData("5 >= 7 - 2", true)]
    [InlineData("5 < 7 - 2", false)]
    [InlineData("5 <= 7 - 2", true)]
    public async Task Subtraction_Binds_Stronger_Than_Comparison(string expression, bool expected)
    {
        var actual = await TH.CompileExpression(expression);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("true == 7 > 5", true)]
    [InlineData("true == 7 >= 5", true)]
    [InlineData("true == 7 < 5", false)]
    [InlineData("true == 7 <= 5", false)]
    [InlineData("true != 7 > 5", false)]
    [InlineData("true != 7 >= 5", false)]
    [InlineData("true != 7 < 5", true)]
    [InlineData("true != 7 <= 5", true)]
    public async Task Comparison_Binds_Stronger_Than_Equality_Comparison(string expression, bool expected)
    {
        var actual = await TH.CompileExpression(expression);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("true && 7 == 5", false)]
    [InlineData("true && 7 != 5", true)]
    public async Task EqualityComparison_Binds_Stronger_Than_And(string expression, bool expected)
    {
        var actual = await TH.CompileExpression(expression);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("true || true && false", true)]
    public async Task And_Binds_Stronger_Than_Or(string expression, bool expected)
    {
        var actual = await TH.CompileExpression(expression);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Or_Binds_Stronger_Than_Null_Coalescing()
    {
        var code = @"
def f: bool? = false
def result = f ?? false || true
";
        var actual = await TH.CompileCode(code);
        Assert.Equal(false, actual);
    }

    [Theory]
    [InlineData("8 * (n ?? 10)", 80)]
    [InlineData("8 / (n ?? 10)", 0.8)]
    [InlineData("83 % (n ?? 10)", 3)]
    public async Task Parenthezied_Binds_Stronger_Than_Multiplication_Division_And_Modulo(string expression, decimal expected)
    {
        var code = $@"
def n: number? = null
def result = {expression}
";
        var actual = await TH.CompileCode(code);
        Assert.Equal(expected, actual);
    }
}
