using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class OperandNode : SemanticNode
    {
        public TypeDescriptor TypeDescriptor { get; }

        public OperandNode(Expr expr, TypeDescriptor typeDescriptor)
            : base(expr)
        {
            TypeDescriptor = typeDescriptor;
        }
    }
}
