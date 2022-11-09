using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
