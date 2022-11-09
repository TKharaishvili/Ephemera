using CompilerPlayground.Lexing;

namespace CompilerPlayground.Parsing.Expressions
{
    public class NullExpr : OperandExpr
    {
        public Token NullToken { get; }

        public NullExpr(Token nullToken)
        {
            NullToken = nullToken;
        }

        public override string ToString() => "null";
    }
}