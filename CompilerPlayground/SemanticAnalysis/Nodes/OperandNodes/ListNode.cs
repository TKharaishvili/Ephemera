using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
