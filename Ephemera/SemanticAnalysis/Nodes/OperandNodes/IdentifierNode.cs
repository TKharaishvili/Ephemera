using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record IdentifierNode(Expr Expr, DefinitionNode Definition, TypeDescriptor TypeDescriptor) : OperandNode(Expr, TypeDescriptor);
}
