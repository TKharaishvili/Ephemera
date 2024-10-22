using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions;

public class ReturnExpr : Expr
{
    public Token ReturnToken { get; }
    public OperandExpr Value { get; }

    public ReturnExpr(Token returnToken, OperandExpr value)
    {
        ReturnToken = returnToken;
        Value = value;
    }

    public override string ToString() => $"return {Value}";
}
