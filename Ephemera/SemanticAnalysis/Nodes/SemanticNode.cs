using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record SemanticNode(Expr Expr);

    public record InvalidNode(Expr Expr) : SemanticNode(Expr);
}
