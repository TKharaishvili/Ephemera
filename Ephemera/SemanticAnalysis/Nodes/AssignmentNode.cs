using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record AssignmentNode(Expr Expr, IdentifierNode Identifier, OperandNode Source) : SemanticNode(Expr);
}
