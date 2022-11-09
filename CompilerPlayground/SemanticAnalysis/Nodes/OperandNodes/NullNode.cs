using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class NullNode : OperandNode
    {
        public NullNode(Expr expr) : base(expr, NullTypeDescriptor.Null)
        {
        }
    }
}
