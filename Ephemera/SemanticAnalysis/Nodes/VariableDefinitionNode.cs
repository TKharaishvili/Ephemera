using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record VariableDefinitionNode(DefinitionExpr DefinitionExpr, TypeDescriptor Type, OperandNode Source = null) : DefinitionNode(DefinitionExpr, DefinitionExpr.Identifier, Type);
}
