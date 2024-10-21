using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record IfNode(Expr Expr, OperandNode Condition, BlockNode Block, IfNode ElseIfNode, BlockNode ElseBlock, TypeDescriptor Type) : SemanticNode(Expr);
}
