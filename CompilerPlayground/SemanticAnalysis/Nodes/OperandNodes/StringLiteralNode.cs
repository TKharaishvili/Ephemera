using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class StringLiteralNode : OperandNode
    {
        public LiteralExpr Literal { get; }

        public StringLiteralNode(LiteralExpr expr) : base(expr, SimpleTypeDescriptor.String)
        {
            Literal = expr;
        }

        public override string ToString() => Expr.ToString();
    }
}
