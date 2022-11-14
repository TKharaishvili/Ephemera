using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class VariableDefinitionNode : DefinitionNode
    {
        public OperandNode Source { get; }

        public VariableDefinitionNode(DefinitionExpr expr, TypeDescriptor type, OperandNode source = null)
            : base(expr, expr.Identifier, type)
        {
            Source = source;
        }
    }
}
