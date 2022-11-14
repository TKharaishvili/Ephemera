using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class DefinitionNode : SemanticNode
    {
        public Token Identifier { get; }
        public TypeDescriptor Type { get; }
        public string Name => Identifier.Lexeme.Word;

        public DefinitionNode(Expr expr, Token identifier, TypeDescriptor type) : base(expr)
        {
            Identifier = identifier;
            Type = type;
        }
    }
}
