namespace CompilerPlayground.Parsing.Expressions
{
    public class WhileExpr : Expr
    {
        public ParenthesizedExpr Condition { get; }
        public BlockExpr Block { get; }

        public WhileExpr(ParenthesizedExpr condition, BlockExpr block)
        {
            Condition = condition;
            Block = block;
        }

        public override string ToString() => $"while {Condition}";
    }
}
