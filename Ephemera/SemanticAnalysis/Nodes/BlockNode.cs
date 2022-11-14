using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class BlockNode : SemanticNode
    {
        public TypeDescriptor Type { get; }
        public IReadOnlyList<SemanticNode> Children { get; }

        public BlockNode(Expr expr, IReadOnlyList<SemanticNode> children, TypeDescriptor type) : base(expr)
        {
            Type = type;
            Children = children;
        }
    }
}
