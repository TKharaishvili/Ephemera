using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record QuestionQuestionOperationNode(Expr Expr, TypeDescriptor TypeDescriptor, OperandNode Left, OperandNode Right) : OperandNode(Expr, TypeDescriptor);
}
