using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class ListNode : OperandNode
    {
        public IReadOnlyList<OperandNode> Elements { get; }

        public ListNode(Expr expr, TypeDescriptor typeDescriptor, IReadOnlyList<OperandNode> elements) : base(expr, typeDescriptor)
        {
            Elements = elements;
        }
    }
}
