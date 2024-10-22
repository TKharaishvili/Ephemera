using System.Collections.Generic;

namespace Ephemera.Parsing.Expressions;

public class FuncDefinitionBaseExpr : Expr
{
    public IReadOnlyList<ParamDeclarationExpr> Parameters { get; }
    public TypeExpr ReturnType { get; }

    protected FuncDefinitionBaseExpr(FuncSignatureExpr signature)
    {
        Parameters = signature.Parameters;
        ReturnType = signature.ReturnType;
    }
}
