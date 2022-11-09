using System.Collections.Generic;

namespace CompilerPlayground.Parsing.Expressions
{
    public class RootExpr : Expr
    {
        public IReadOnlyList<Expr> Expressions { get; }

        public RootExpr(IReadOnlyList<Expr> expressions)
        {
            Expressions = expressions;
        }
    }
}
