using System.Collections.Generic;

namespace Ephemera.Parsing.Expressions
{
    public class BlockExpr : Expr
    {
        public IReadOnlyList<Expr> Expressions { get; }

        public BlockExpr(IReadOnlyList<Expr> expressions)
        {
            Expressions = expressions;
        }

        public BlockExpr(params Expr[] expressions)
        {
            Expressions = expressions;
        }
    }
}
