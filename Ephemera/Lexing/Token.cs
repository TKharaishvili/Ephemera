namespace Ephemera.Lexing;

public class Token
{
    public TokenClass Class { get; }
    public Lexeme Lexeme { get; }
    public string Word => Lexeme.Word;
    public int Start => Lexeme.StartPosition;
    public int End => Lexeme.EndPosition;

    public Token(TokenClass @class, Lexeme lexeme)
    {
        Class = @class;
        Lexeme = lexeme;
    }

    public Token(TokenClass @class, string lexeme, int startPosition, int endPosition)
        : this(@class, new Lexeme(lexeme, startPosition, endPosition))
    {
    }

    public Token(TokenClass @class, char lexeme, int startPosition, int endPosition)
        : this(@class, lexeme.ToString(), startPosition, endPosition)
    {
    }

    public override string ToString() => $"{Lexeme.Word}";
}
