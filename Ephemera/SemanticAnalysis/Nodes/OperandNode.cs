using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record OperandNode(Expr Expr, TypeDescriptor TypeDescriptor) : SemanticNode(Expr);

    //todo do I really need three literal classes for all types
    //don't think so....
    public record BooleanLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.Bool)
    {
        public override string ToString() => Expr.ToString();
    }

    public record NumberLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.Number);

    public record StringLiteralNode(LiteralExpr Literal) : OperandNode(Literal, SimpleTypeDescriptor.String)
    {
        public override string ToString() => Expr.ToString();
    }

    public record NullNode(Expr Expr) : OperandNode(Expr, NullTypeDescriptor.Null);

    public record ListNode(Expr Expr, TypeDescriptor TypeDescriptor, IReadOnlyList<OperandNode> Elements) : OperandNode(Expr, TypeDescriptor);

    public record IdentifierNode(Expr Expr, DefinitionNode Definition, TypeDescriptor TypeDescriptor) : OperandNode(Expr, TypeDescriptor);

    public record UnaryOperationNode(UnaryOperationExpr UnaryOperationExpr, OperandNode Operand, TypeDescriptor TypeDescriptor) : OperandNode(UnaryOperationExpr, TypeDescriptor)
    {
        public Token Operator { get; } = UnaryOperationExpr.Operator;
    }

    //ToDo can I combine BooleanOperationNode, NumberBooleanOperationNode and NumberOperationNode In one class?
    //I mean why not?
    public record BooleanOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(BinaryExpr, TypeDescriptor);

    public record NumberBooleanOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : BooleanOperationNode(BinaryExpr, Left, Right, TypeDescriptor);

    public record NumberOperationNode(BinaryOperationExpr BinaryExpr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(BinaryExpr, TypeDescriptor);

    public record StringConcatNode(Expr Expr, OperandNode Left, OperandNode Right, TypeDescriptor TypeDescriptor) : OperandNode(Expr, TypeDescriptor);

    public record QuestionQuestionOperationNode(Expr Expr, TypeDescriptor TypeDescriptor, OperandNode Left, OperandNode Right) : OperandNode(Expr, TypeDescriptor);

    public record FuncInvocationNode(FuncInvocationExpr FuncInvocationExpr, TypeDescriptor TypeDescriptor, IReadOnlyList<OperandNode> Params, OperandNode PreParam, bool IsConditionalInvocation, FuncNamedNode NamedFunc = null) : OperandNode(FuncInvocationExpr, TypeDescriptor)
    {
        public string Name { get; } = FuncInvocationExpr.Identifier.Word;

        //TODO: the implementation of this prop was changed without testing
        //do make sure the previous and current implementations work the same
        //preferrably with a unit test
        public bool HasConditionalInvocation
        {
            get
            {
                return IsConditionalInvocation ||
                    PreParam is FuncInvocationNode { HasConditionalInvocation: true };
            }
        }

        public FuncInvocationNode With(TypeDescriptor typeDescriptor)
        {
            return new FuncInvocationNode(FuncInvocationExpr, typeDescriptor, Params, PreParam, IsConditionalInvocation, NamedFunc);
        }
    }

    public record ParenthesizedNode(Expr Expr, OperandNode InnerNode) : OperandNode(Expr, InnerNode.TypeDescriptor);
}
