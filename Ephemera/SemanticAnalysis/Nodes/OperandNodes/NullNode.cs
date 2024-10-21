using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record NullNode(Expr Expr) : OperandNode(Expr, NullTypeDescriptor.Null);
}
