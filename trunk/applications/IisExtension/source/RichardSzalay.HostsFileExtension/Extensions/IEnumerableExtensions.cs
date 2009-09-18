using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T,bool> predicate)
        {
            foreach(T item in enumerable.Where(predicate))
            {
                return true;
            }

            return false;
        }
    }
}
