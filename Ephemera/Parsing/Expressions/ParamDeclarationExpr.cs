using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions;

public class ParamDeclarationExpr : Expr
{
    public Token Identifier { get; }
    public TypeExpr Type { get; }
    public string Name => Identifier.Lexeme.Word;

    public ParamDeclarationExpr(Token identifier, TypeExpr type)
    {
        Identifier = identifier;
        Type = type;
    }
}
