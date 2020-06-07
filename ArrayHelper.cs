using System;

namespace BackgammonCore
{
    /// <summary>
    /// Provides optimized helper methods for operations on an Int16 (short) array.
    /// </summary>
    public static class ArrayHelper
    {
        // https://stackoverflow.com/questions/23248872/fast-array-copy-in-c-sharp
        public static short[] FastArrayCopy(short[] source)
        {
            var copy = new short[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }

        // https://stackoverflow.com/questions/457453/remove-element-of-a-regular-array
        public static short[] RemoveAt(short[] source, int index)
        {
            short[] dest = new short[source.Length - 1];
            if (index > 0)
            {
                Array.Copy(source, 0, dest, 0, index);
            }

            if (index < source.Length - 1)
            {
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
            }

            return dest;
        }
    }
}
