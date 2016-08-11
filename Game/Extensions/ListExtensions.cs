using System.Collections.Generic;
using System.Linq;

namespace Playblack.Extensions {

    public static class ListExtensions {

        /// <summary>
        /// Resize the list and fill new slots with defaults
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="sz">Size.</param>
        /// <param name="c">C.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Resize<T>(this List<T> list, int sz, T c = default(T)) {
            int cur = list.Count;
            if (sz < cur) {
                list.RemoveRange(sz, cur - sz);
            }
            else if (sz > cur) {
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }
    }
}