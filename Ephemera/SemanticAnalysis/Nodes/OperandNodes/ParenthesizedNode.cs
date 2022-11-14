using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class ParenthesizedNode : OperandNode
    {
        public OperandNode InnerNode { get; }

        public ParenthesizedNode(Expr expr, OperandNode innerNode) : base(expr, innerNode.TypeDescriptor)
        {
            InnerNode = innerNode;
        }
    }
}
