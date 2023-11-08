using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public static class Extensions
    {
        public static T[] ElementsFrom<T>(this T[] sourceArray, int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= sourceArray.Length || count <= 0)
                throw new ArgumentException("Invalid startIndex or count.");
            if (startIndex + count > sourceArray.Length)
                count = sourceArray.Length - startIndex;
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = sourceArray[startIndex + i];
            return result;
        }
    }
}
