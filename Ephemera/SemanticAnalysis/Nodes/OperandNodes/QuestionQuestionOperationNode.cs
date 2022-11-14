using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class QuestionQuestionOperationNode : OperandNode
    {
        public OperandNode Left { get; }
        public OperandNode Right { get; }

        public QuestionQuestionOperationNode(Expr expr, TypeDescriptor type, OperandNode left, OperandNode right)
            : base(expr, type)
        {
            Left = left;
            Right = right;
        }
    }
}
