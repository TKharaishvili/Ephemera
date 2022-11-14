using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class IdentifierNode : OperandNode
    {
        public DefinitionNode Definition { get; }

        public IdentifierNode(Expr expr, DefinitionNode definition, TypeDescriptor typeDescriptor) : base(expr, typeDescriptor)
        {
            Definition = definition;
        }
    }
}
