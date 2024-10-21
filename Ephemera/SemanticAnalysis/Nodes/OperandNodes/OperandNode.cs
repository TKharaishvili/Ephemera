using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record OperandNode(Expr Expr, TypeDescriptor TypeDescriptor) : SemanticNode(Expr);
}
