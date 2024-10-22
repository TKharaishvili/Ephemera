using System.Collections.Generic;

namespace Ephemera.Parsing.Expressions;

public class RootExpr : Expr
{
    public IReadOnlyList<Expr> Expressions { get; }

    public RootExpr(IReadOnlyList<Expr> expressions)
    {
        Expressions = expressions;
    }
}
