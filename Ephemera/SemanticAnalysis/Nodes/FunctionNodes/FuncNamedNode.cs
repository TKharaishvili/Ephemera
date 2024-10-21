using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record FuncNamedNode(Expr Expr, Token Identifier, IReadOnlyList<DefinitionNode> Params, TypeDescriptor ReturnType, bool IsExtension) : SemanticNode(Expr)
    {
        public string Name => Identifier.Word;
    }
}
