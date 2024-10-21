using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record ListNode(Expr Expr, TypeDescriptor TypeDescriptor, IReadOnlyList<OperandNode> Elements) : OperandNode(Expr, TypeDescriptor);
}
