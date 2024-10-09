using Xunit;

namespace Ephemera.Tests;

public class FunctionE2eTests
{
    [Theory]
    [InlineData("0", 0, false)]
    [InlineData("0", 0, true)]
    [InlineData("3 + 4", 7, false)]
    [InlineData("3 + 4", 7, true)]
    public async void Function_Definition_And_Invocation_Work(string returnExpr, double expected, bool il)
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
}
