using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    //todo do I really need three literal classes for all types
    //don't think so....
    public record BooleanLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.Bool)
    {
        public override string ToString() => Expr.ToString();
    }

    //ToDo can I combine BooleanOperationNode, NumberBooleanOperationNode and NumberOperationNode In one class?
    //I mean why not?
    public record BooleanOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(BinaryExpr, TypeDescriptor);

    public record NumberBooleanOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : BooleanOperationNode(BinaryExpr, Left, Right, TypeDescriptor);
}
