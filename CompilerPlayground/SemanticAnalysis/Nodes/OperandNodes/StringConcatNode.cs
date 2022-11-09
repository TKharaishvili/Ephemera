using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
