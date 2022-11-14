using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class StringConcatNode : OperandNode
    {
        public OperandNode Left { get; }
        public OperandNode Right { get; }

        public StringConcatNode(Expr expr, OperandNode left, OperandNode right, TypeDescriptor typeDescriptor)
            : base(expr, typeDescriptor)
        {
            Left = left;
            Right = right;
        }
    }
}
