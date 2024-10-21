using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record ReturnNode(Expr Expr, TypeDescriptor Type, OperandNode Value) : SemanticNode(Expr);
}
