using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class IfNode : SemanticNode
    {
        public OperandNode Condition { get; }
        public BlockNode Block { get; }
        public IfNode ElseIfNode { get; }
        public BlockNode ElseBlock { get; }
        public TypeDescriptor Type { get; }

        public IfNode(Expr expr, OperandNode condition, BlockNode block, IfNode elseIfNode, BlockNode elseBlock, TypeDescriptor type)
            : base(expr)
        {
            Condition = condition;
            Block = block;
            ElseIfNode = elseIfNode;
            ElseBlock = elseBlock;
            Type = type;
        }
    }
}
