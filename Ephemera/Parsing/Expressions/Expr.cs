using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions
{
    public class Expr
    {
        public virtual int Start { get; }
        public virtual int End { get; }
    }

    public class OperandExpr : Expr
    {

    }

    public class UnaryOperationExpr : OperandExpr
    {
        public Token Operator { get; }
        public OperandExpr Operand { get; }

        public UnaryOperationExpr(Token @operator, OperandExpr operand)
        {
            Operator = @operator;
            Operand = operand;
        }

        public override string ToString() => $"{Operator.Lexeme.Word}{Operand}";
        public override int Start => Operator.Lexeme.StartPosition;
        public override int End => Operand.End;
    }

    public class BinaryOperationExpr : OperandExpr
    {
        public OperandExpr Left { get; }
        public OperandExpr Right { get; }
        public Token Operator { get; }

        public BinaryOperationExpr(OperandExpr left, OperandExpr right, Token @operator)
        {
            Left = left;
            Right = right;
            Operator = @operator;
        }

        public override string ToString() => $"{Left} {Operator.Lexeme.Word} {Right}";
    }

    public class LiteralExpr : OperandExpr
    {
        public Token Token { get; }

        public LiteralExpr(Token token)
        {
            Token = token;
        }

        public override string ToString() => $"{Token.Lexeme.Word}";
        public override int Start => Token.Lexeme.StartPosition;
        public override int End => Token.Lexeme.EndPosition;
    }

    public class IdentifierExpr : OperandExpr
    {
        public Token Token { get; }

        public IdentifierExpr(Token token)
        {
            Token = token;
        }

        public override string ToString() => $"{Token.Lexeme.Word}";
        public override int Start => Token.Lexeme.StartPosition;
        public override int End => Token.Lexeme.EndPosition;
    }

    public class ParenthesizedExpr : OperandExpr
    {
        public Token OpenParen { get; }
        public OperandExpr Expr { get; }
        public Token CloseParen { get; }

        public ParenthesizedExpr(Token openParen, OperandExpr expr, Token closeParen)
        {
            OpenParen = openParen;
            Expr = expr;
            CloseParen = closeParen;
        }

        public override string ToString() => $"({Expr})";
        public override int Start => OpenParen.Lexeme.StartPosition;
        public override int End => CloseParen.Lexeme.EndPosition;
    }
}
