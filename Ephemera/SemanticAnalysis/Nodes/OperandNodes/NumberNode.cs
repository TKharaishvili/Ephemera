using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record NumberLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.Number);

    public record NumberOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(BinaryExpr, TypeDescriptor);
}