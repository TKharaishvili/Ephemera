using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record StringLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.String)
    {
        public override string ToString() => Expr.ToString();
    }
}
