using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    //todo do I really need three literal classes for all types
    //don't think so....
    public class BooleanLiteralNode : OperandNode
    {
        public LiteralExpr Literal { get; }

        public BooleanLiteralNode(LiteralExpr expr) : base(expr, SimpleTypeDescriptor.Bool)
        {
            Literal = expr;
        }

        public override string ToString() => Expr.ToString();
    }

    //ToDo can I combine BooleanOperationNode, NumberBooleanOperationNode and NumberOperationNode In one class?
    //I mean why not?
    public class BooleanOperationNode : OperandNode
    {
        public BinaryOperationExpr BinaryExpr { get; }
        public OperandNode Left { get; }
        public OperandNode Right { get; }

        public BooleanOperationNode(BinaryOperationExpr expr, OperandNode left, OperandNode right, TypeDescriptor typeDescriptor)
            : base(expr, typeDescriptor)
        {
            BinaryExpr = expr;
            Left = left;
            Right = right;
        }
    }

    public class NumberBooleanOperationNode : BooleanOperationNode
    {
        public NumberBooleanOperationNode(BinaryOperationExpr expr, OperandNode left, OperandNode right, TypeDescriptor typeDescriptor)
            : base(expr, left, right, typeDescriptor)
        {
        }
    }
}
