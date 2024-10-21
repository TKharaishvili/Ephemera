using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record BlockNode(Expr Expr, IReadOnlyList<SemanticNode> Children, TypeDescriptor Type) : SemanticNode(Expr);
}
