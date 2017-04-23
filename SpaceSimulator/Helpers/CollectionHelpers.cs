using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Contains helper methods for collections
    /// </summary>
    public static class CollectionHelpers
    {
        private class ComparisonComparer<T> : IComparer<T>, IComparer
        {
            private readonly Comparison<T> comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return comparison(x, y);
            }

            public int Compare(object o1, object o2)
            {
                return comparison((T)o1, (T)o2);
            }
        }

        /// <summary>
        /// Sorts the given list in place.
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="list">The list</param>
        /// <param name="comparison">The comparision</param>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
        }

        /// <summary>
        /// Sorts hte given list in place.
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="list">The list</param>
        public static void Sort<T>(this IList<T> list) where T : IComparable<T>
        {
            Sort(list, (x, y) => x.CompareTo(y));
        }
    }
}
