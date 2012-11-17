using System;
using System.Collections.Generic;

namespace FunksjonellFagdag
{
    static class FunctionalExtensions
    {
        public static void Each<T>(this IEnumerable<T> liste, Action<T> toDo)
        {
            foreach (var l in liste)
            {
                toDo(l);
            }
        }

        public static void Each<T>(this IEnumerable<T> list, Action<int, T> toDo)
        {
            var i = default(int);
            foreach (var l in list)
            {
                toDo(i++, l);
            }
        }

        public static IEnumerable<T2> IfNotNull<T1, T2>(this T1 obj, 
            Func<T1, IEnumerable<T2>> func) where T1 : class
        {
            return obj == null ? new T2[] { } : func(obj);
        }
    }
}
