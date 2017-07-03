using System;
using System.Collections.Generic;

namespace RestApiClientBuilder.Core.Utils
{
    public static class ListExtentions
    {
        public static void Foreach<T>(this IList<T> list, Action<T> action)
        {
            foreach (T o in list)
            {
                action(o);
            }
        }
    }
}