using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class SemanticNode
    {
        public Expr Expr { get; }

        public SemanticNode(Expr expr)
        {
            Expr = expr;
        }
    }

    public class InvalidNode : SemanticNode
    {
        public InvalidNode(Expr expr) : base(expr)
        {

        }
    }
}
