using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class ArrayHelper
    {
        public static T[,] GetNew2DArray<T>(int x, int y, T initialValue)
        {
            T[,] nums = new T[x, y];
            for (int i = 0; i < x * y; i++) nums[i % x, i / x] = initialValue;
            return nums;
        }
    }
}
