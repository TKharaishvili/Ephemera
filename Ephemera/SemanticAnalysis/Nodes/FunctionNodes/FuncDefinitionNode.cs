using System.Collections.Generic;
using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record FuncDefinitionNode(Expr Expr, Token Identifier, IReadOnlyList<DefinitionNode> Params, TypeDescriptor ReturnType, bool IsExtension, BlockNode Body) : FuncNamedNode(Expr, Identifier, Params, ReturnType, IsExtension);
}
