using CompilerPlayground.Lexing;

namespace CompilerPlayground.Parsing.Expressions
{
    public class KeywordExpr : Expr
    {
        public Token Keyword { get; }

        public KeywordExpr(Token keyword)
        {
            Keyword = keyword;
        }
    }
}
