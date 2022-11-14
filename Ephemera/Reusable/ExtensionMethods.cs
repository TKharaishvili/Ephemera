using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Ephemera.Reusable
{
    public static class ExtensionMethods
    {
        [DebuggerStepThrough]
        public static char? CharAt(this string str, int index)
            => str.Length > index ? str[index] : default(char?);

        [DebuggerStepThrough]
        public static T At<T>(this IReadOnlyList<T> list, int index) where T : class
            => list.Count > index ? list[index] : default(T);

        [DebuggerStepThrough]
        public static T PopSafe<T>(this Stack<T> stack) where T : class
            => stack.Any() ? stack.Pop() : default(T);

        [DebuggerStepThrough]
        public static T PeekSafe<T>(this Stack<T> stack) where T : class
            => stack.Count > 0 ? stack.Peek() : default(T);

        [DebuggerStepThrough]
        public static T DequeueSafe<T>(this Queue<T> queue) where T : class
            => queue.Count > 0 ? queue.Dequeue() : default(T);

        [DebuggerStepThrough]
        public static StringBuilder AppendAll(this StringBuilder sb, params string[] values)
        {
            foreach (var value in values)
            {
                sb.Append(value);
            }
            return sb;
        }

        [DebuggerStepThrough]
        public static string JoinStrings<T>(this IEnumerable<T> collection, string separator = "") => string.Join(separator, collection);

        [DebuggerStepThrough]
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;
            foreach (var x in source)
            {
                yield return x;
            }
        }

        [DebuggerStepThrough]
        public static bool TryAdd<T>(this HashSet<T> set, T item)
        {
            if (set.Contains(item))
            {
                return false;
            }
            set.Add(item);
            return true;
        }
    }
}
