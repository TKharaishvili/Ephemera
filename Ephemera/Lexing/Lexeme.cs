namespace Ephemera.Lexing
{
    public class Lexeme
    {
        public string Word { get; }
        public int StartPosition { get; }
        public int EndPosition { get; }

        public Lexeme(string word, int startPosition, int endPosition)
        {
            Word = word;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }
}
