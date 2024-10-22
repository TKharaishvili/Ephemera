using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions;

public class DefinitionExpr : Expr
{
    public Token Identifier { get; }
    public TypeExpr Type { get; }
    public OperandExpr Source { get; }

    public DefinitionExpr(Token identifier, TypeExpr type, OperandExpr source)
    {
        Identifier = identifier;
        Type = type;
        Source = source;
    }

    public override string ToString()
    {
        var def = $"def {Identifier.Lexeme.Word}";

        if (Type != null)
        {
            def += ":" + Type;
        }

        return def += " = " + Source;
    }

    public override int Start => Identifier.Lexeme.StartPosition;
    public override int End => Source.End;
}
