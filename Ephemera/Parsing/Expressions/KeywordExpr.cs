using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions
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
