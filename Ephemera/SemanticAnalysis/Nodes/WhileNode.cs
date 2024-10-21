using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record WhileNode(Expr Expr, OperandNode Condition, BlockNode Block) : SemanticNode(Expr);
}
