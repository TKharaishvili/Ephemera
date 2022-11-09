using CompilerPlayground.Lexing;

namespace CompilerPlayground.Parsing.Expressions
{
    public class RangeExpr : OperandExpr
    {
        public OperandExpr Left { get; }
        public Token RangeToken { get; }
        public OperandExpr Right { get; }

        public RangeExpr(OperandExpr left, Token rangeToken, OperandExpr right)
        {
            Left = left;
            RangeToken = rangeToken;
            Right = right;
        }
    }
}
