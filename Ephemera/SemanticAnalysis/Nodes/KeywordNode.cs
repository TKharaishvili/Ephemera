using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record KeywordNode(KeywordExpr KeywordExpr) : SemanticNode(KeywordExpr);
}
