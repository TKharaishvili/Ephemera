using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
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
