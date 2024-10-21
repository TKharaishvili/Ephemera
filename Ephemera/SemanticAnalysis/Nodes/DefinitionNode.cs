using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    //TODO: only VariableDefinitionNode derives from this record. Do we really need it?
    public record DefinitionNode(Expr Expr, Token Identifier, TypeDescriptor Type) : SemanticNode(Expr)
    {
        public string Name => Identifier.Lexeme.Word;
    }
}
