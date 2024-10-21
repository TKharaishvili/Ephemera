using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record ParenthesizedNode(Expr Expr, OperandNode InnerNode) : OperandNode(Expr, InnerNode.TypeDescriptor);
}
