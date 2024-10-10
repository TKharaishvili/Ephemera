using System.Threading.Tasks;
using Xunit;

namespace Ephemera.Tests;

public class FunctionE2eTests
{
    [Theory]
    [InlineData("0", 0, false)]
    [InlineData("0", 0, true)]
    [InlineData("3 + 4", 7, false)]
    [InlineData("3 + 4", 7, true)]
    public async Task Function_Definition_And_Invocation_Work(string returnExpr, double expected, bool il)
    {
        var contextSrc = @$"
fun PureFun()
{{
    return {returnExpr}
}}
";

        var src = "PureFun()";
        var actual = await TestHelpers.RunExpression(src, il, contextSrc);
        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Function_Invoking_Another_Function_Works(bool il)
    {
        var contextSrc = @"
fun PureFunOne()
{
    return 5
}

fun PureFunTwo()
{
    return PureFunOne()
}
";
        var src = "PureFunTwo()";
        var actual = await TestHelpers.RunExpression(src, il, contextSrc);
        Assert.Equal(5m, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Function_Definition_With_Parameters_And_Invocation_Works(bool il)
    {
        var contextSrc = @"
fun PureFun(x: number, y: number)
{
    return x + y
}
";
        var src = "PureFun(1, 2)";
        var actual = await TestHelpers.RunExpression(src, il, contextSrc);
        Assert.Equal(3m, actual);
    }
}
