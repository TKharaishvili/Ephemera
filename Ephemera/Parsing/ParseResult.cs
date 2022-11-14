using Ephemera.Parsing.Expressions;
using Ephemera.Reusable;

namespace Ephemera.Parsing
{
    public interface IParseResult<out T>
        where T : Expr
    {
        T Expr { get; }
        int Index { get; }
        CodeError Error { get; }
        bool Fail { get; }
        ParseResult<TResult> Cast<TResult>() where TResult : Expr;
    }

    public class ParseResult<T> : IParseResult<T>
        where T : Expr
    {
        public T Expr { get; }
        public int Index { get; }
        public CodeError Error { get; }
        public bool Fail => Expr == null;

        public ParseResult(T expr, int index)
        {
            Expr = expr;
            Index = index;
        }

        public ParseResult(CodeError error)
        {
            Error = error;
            Index = -1;
        }

        public ParseResult<TResult> Cast<TResult>() where TResult : Expr => Error.ToParseResult<TResult>();
    }

    public static class ParseResultRelatedExtensions
    {
        public static ParseResult<T> ToParseResult<T>(this T expr, int index) where T : Expr =>
             new ParseResult<T>(expr, index);

        public static ParseResult<T> ToParseResult<T>(this CodeError ce) where T : Expr =>
            new ParseResult<T>(ce);
    }
}
