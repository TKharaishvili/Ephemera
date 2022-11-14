namespace Ephemera.Reusable
{
    /// <summary>
    /// This record is used for reporting code errors
    /// </summary>
    public class CodeError
    {
        public string Message { get; }
        public int Line { get; }
        public int Start { get; }
        public int End { get; }

        public CodeError(string message, int line, int start, int end)
        {
            Message = message;
            Line = line;
            Start = start;
            End = end;
        }
    }
}
