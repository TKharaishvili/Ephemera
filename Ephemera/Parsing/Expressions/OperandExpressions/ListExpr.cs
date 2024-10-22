using Ephemera.Lexing;
using System.Collections.Generic;

namespace Ephemera.Parsing.Expressions;

public class ListExpr : OperandExpr
{
    public Token OpenSquare { get; }
    public IReadOnlyList<OperandExpr> Items { get; }
    public Token CloseSquare { get; }

    public ListExpr(Token openSquare, IReadOnlyList<OperandExpr> items, Token closeSquare)
    {
        OpenSquare = openSquare;
        Items = items;
        CloseSquare = closeSquare;
    }

    public override int Start => OpenSquare.Start;
    public override int End => CloseSquare.End;
}
