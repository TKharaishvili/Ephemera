using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record DefinitionNode(Expr Expr, Token Identifier, TypeDescriptor Type) : SemanticNode(Expr)
    {
        public string Name => Identifier.Lexeme.Word;
    }
}
