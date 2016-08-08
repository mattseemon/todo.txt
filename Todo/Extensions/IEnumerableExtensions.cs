using System;
using System.Collections.Generic;
using System.Linq;

namespace Seemon.Todo.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if(!items.IsNullOrEmpty())
            {
                foreach (var item in items)
                    if (item != null)
                        action(item);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static bool HasValue<T>(this IEnumerable<T> items)
        {
            return !items.IsNullOrEmpty();
        }
    }
}
