using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vektonn.IndexShard
{
    internal class ByteArrayComparer : IEqualityComparer<byte[]>, IComparer<byte[]>
    {
        public static readonly ByteArrayComparer Instance = new ByteArrayComparer();

        public bool Equals(byte[]? x, byte[]? y)
        {
            if ((x == null) ^ (y == null))
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x!.Length != y!.Length)
                return false;

            return MemCmp(x, y, x.Length) == 0;
        }

        public int Compare(byte[]? x, byte[]? y)
        {
            if (x == null)
                return y == null ? 0 : -1;

            if (y == null)
                return 1;

            if (ReferenceEquals(x, y))
                return 0;

            if (x.Length < y.Length)
            {
                var res = MemCmp(x, y, x.Length);
                return res != 0 ? res : -1;
            }

            if (x.Length > y.Length)
            {
                var res = MemCmp(x, y, y.Length);
                return res != 0 ? res : 1;
            }

            return MemCmp(x, y, x.Length);
        }

        public int GetHashCode(byte[]? bytes)
        {
            if (bytes == null)
                return 0;

            unchecked
            {
                unsafe
                {
                    var length = bytes.Length;
                    var hashCode = length;
                    fixed (byte* bytesPtr = bytes)
                    {
                        var i = 0;
                        var currentIntPtr = (int*)bytesPtr;
                        var intsBound = length - sizeof(int);
                        for (; i <= intsBound; i += sizeof(int), currentIntPtr++)
                            hashCode = (hashCode * 397) ^ *currentIntPtr;
                        var currentBytePtr = (byte*)currentIntPtr;
                        for (; i < length; i++, currentBytePtr++)
                            hashCode = (hashCode * 397) ^ *currentBytePtr;
                        return hashCode;
                    }
                }
            }
        }

        /// <remarks>
        ///     Code from
        ///     https://github.com/ravendb/ravendb/blob/a13de74595846cac6ee5e254a13ef868d2b31779/Raven.Voron/Voron/Util/MemoryUtils.cs#L11
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int MemCmp(byte[] x, byte[] y, long n)
        {
            if (n == 0)
                return 0;

            fixed (byte* px = x)
            {
                fixed (byte* py = y)
                {
                    var lhs = px;
                    var rhs = py;
                    const int sizeOfUInt = sizeof(uint);

                    if (n > sizeOfUInt)
                    {
                        var lUintAlignment = (long)lhs % sizeOfUInt;
                        var rUintAlignment = (long)rhs % sizeOfUInt;

                        if (lUintAlignment != 0 && lUintAlignment == rUintAlignment)
                        {
                            var toAlign = sizeOfUInt - lUintAlignment;
                            while (toAlign > 0)
                            {
                                var r = *lhs++ - *rhs++;
                                if (r != 0)
                                    return r;
                                n--;

                                toAlign--;
                            }
                        }

                        var lp = (uint*)lhs;
                        var rp = (uint*)rhs;

                        while (n > sizeOfUInt)
                        {
                            if (*lp != *rp)
                                break;

                            lp++;
                            rp++;

                            n -= sizeOfUInt;
                        }

                        lhs = (byte*)lp;
                        rhs = (byte*)rp;
                    }

                    while (n > 0)
                    {
                        var r = *lhs++ - *rhs++;
                        if (r != 0)
                            return r;
                        n--;
                    }

                    return 0;
                }
            }
        }
    }
}
