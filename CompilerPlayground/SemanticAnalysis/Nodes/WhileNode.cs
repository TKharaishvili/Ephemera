using CompilerPlayground.Parsing.Expressions;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class WhileNode : SemanticNode
    {
        public OperandNode Condition { get; }
        public BlockNode Block { get; }

        public WhileNode(Expr expr, OperandNode condition, BlockNode block) : base(expr)
        {
            Condition = condition;
            Block = block;
        }
    }
}
