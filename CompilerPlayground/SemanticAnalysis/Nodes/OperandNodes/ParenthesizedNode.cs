using CompilerPlayground.Parsing.Expressions;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
