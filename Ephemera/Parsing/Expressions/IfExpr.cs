using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions
{
    public class IfExpr : Expr
    {
        public Token If { get; }
        public ParenthesizedExpr Condition { get; }
        public BlockExpr Block { get; }
        public IfExpr ElseIfClause { get; }
        public BlockExpr ElseClause { get; }

        public IfExpr(Token @if, ParenthesizedExpr condition, BlockExpr block, IfExpr elseIfClause = null, BlockExpr elseClause = null)
        {
            If = @if;
            Condition = condition;
            Block = block;
            ElseIfClause = elseIfClause;
            ElseClause = elseClause;
        }

        public override string ToString() => $"if {Condition}";
        public override int Start => If.Lexeme.StartPosition;
    }
}
