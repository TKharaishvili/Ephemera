using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;
using System.Collections.Generic;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
