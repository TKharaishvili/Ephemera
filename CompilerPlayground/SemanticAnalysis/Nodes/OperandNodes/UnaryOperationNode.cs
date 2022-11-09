using CompilerPlayground.Lexing;
using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
