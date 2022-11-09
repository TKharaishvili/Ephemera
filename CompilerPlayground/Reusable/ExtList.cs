using System.Collections.Generic;

namespace CompilerPlayground.Reusable
{
    public class ExtList<T> : List<T>
    {
        public ExtList()
        {

        }

        public ExtList(int capacity) : base(capacity)
        {

        }

        public static ExtList<T> operator +(ExtList<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        public static ExtList<T> operator +(ExtList<T> list, IEnumerable<T> items)
        {
            list.AddRange(items);
            return list;
        }
    }
}
