using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class ReturnNode : SemanticNode
    {
        public TypeDescriptor Type { get; }
        public OperandNode Value { get; }

        public ReturnNode(Expr expr, TypeDescriptor type, OperandNode value)
            : base(expr)
        {
            Value = value;
            Type = type;
        }
    }
}
