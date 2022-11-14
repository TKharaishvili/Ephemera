using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class KeywordNode : SemanticNode
    {
        public KeywordExpr KeywordExpr { get; }

        public KeywordNode(KeywordExpr expr) : base(expr)
        {
            KeywordExpr = expr;
        }
    }
}
