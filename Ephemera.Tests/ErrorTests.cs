using Ephemera.Reusable;
using Xunit;

namespace Ephemera.Tests;

public class ErrorTests
{
    [Fact]
    public void Using_A_Non_Function_Invocation_Expression_As_A_Statement_Fails()
    {
        var contextSrc = "1 + 2";
        var actual = TestHelpers.GetError(contextSrc);
        Assert.Equal(ErrorMessages.CantUseExpressionsAsStatements, actual);
    }
}
