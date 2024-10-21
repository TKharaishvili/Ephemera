using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record StringConcatNode(Expr Expr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(Expr, TypeDescriptor);
}
