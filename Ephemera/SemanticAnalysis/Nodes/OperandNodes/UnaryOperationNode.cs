using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class UnaryOperationNode : OperandNode
    {
        public Token Operator { get; }
        public OperandNode Operand { get; }

        public UnaryOperationNode(UnaryOperationExpr expr, OperandNode operand, TypeDescriptor typeDescriptor)
            : base(expr, typeDescriptor)
        {
            Operator = expr.Operator;
            Operand = operand;
        }
    }
}
