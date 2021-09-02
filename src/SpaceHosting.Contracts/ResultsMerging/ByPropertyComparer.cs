using System;
using System.Collections;
using System.Collections.Generic;

namespace SpaceHosting.Contracts.ResultsMerging
{
    public class ByPropertyComparer<T, TProperty> : IComparer, IComparer<T>
        where T : class
        where TProperty : IComparable
    {
        private readonly Func<T, TProperty> property;

        public ByPropertyComparer(Func<T, TProperty> property)
        {
            this.property = property;
        }

        public int Compare(object? x, object? y)
        {
            return Compare((T?)x, (T?)y);
        }

        public int Compare(T? x, T? y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return property(x).CompareTo(property(y));
        }
    }
}
