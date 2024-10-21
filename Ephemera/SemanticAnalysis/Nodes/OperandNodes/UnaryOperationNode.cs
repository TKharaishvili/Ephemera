using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record UnaryOperationNode(UnaryOperationExpr UnaryOperationExpr, OperandNode Operand, TypeDescriptor TypeDescriptor) : OperandNode(UnaryOperationExpr, TypeDescriptor)
    {
        public Token Operator { get; } = UnaryOperationExpr.Operator;
    }
}
