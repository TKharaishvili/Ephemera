using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class NullNode : OperandNode
    {
        public NullNode(Expr expr) : base(expr, NullTypeDescriptor.Null)
        {
        }
    }
}
