using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class NumberLiteralNode : OperandNode
    {
        public LiteralExpr Literal { get; }

        public NumberLiteralNode(LiteralExpr literal) : base(literal, SimpleTypeDescriptor.Number)
        {
            Literal = literal;
        }
    }

    public class NumberOperationNode : OperandNode
    {
        public BinaryOperationExpr BinaryExpr { get; }
        public OperandNode Left { get; }
        public OperandNode Right { get; }

        public NumberOperationNode(BinaryOperationExpr expr, OperandNode left, OperandNode right, TypeDescriptor typeDescriptor)
            : base(expr, typeDescriptor)
        {
            BinaryExpr = expr;
            Left = left;
            Right = right;
        }
    }
}