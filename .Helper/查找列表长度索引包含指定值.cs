using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] dx = new int[] { 0, 1, 1, 2, 2, 2, 2, 3, 4, 5, 5, 6, 7, 8, 9 };

            int value = 2;
            var res = TwoPointsSearch(dx, value, 0, dx.Length - 1);
            var left = res[0];
            var right = res[1];

            Console.WriteLine($"[{left},{right}]");
            if(left == right)
            {
                Console.WriteLine(dx[left]);
            }
            else
            {
                for(int j = left; j < right; j++)
                {
                    Console.WriteLine(dx[j]);
                }
            }
        }

        /// <summary>
        /// 查找列表长度索引包含指定值
        /// </summary>
        /// <param name="array"> 数组</param>
        /// <param name="data"> 待查找的值</param>
        /// <param name="leftIndex"> 左侧起始索引</param>
        /// <param name="rightIndex"> 右侧起始索引</param>
        /// <returns> </returns>
        public static int[] TwoPointsSearch(int[] array, int data, int leftIndex, int rightIndex)
        {
            var left = TwoPointsSearchLeft(array, data, leftIndex, rightIndex);
            var right = TwoPointsSearchRight(array, data, leftIndex, rightIndex);
            return new int[] { left, right };
        }

        private static int TwoPointsSearchLeft(int[] array, int data, int leftIndex, int rightIndex)
        {
            if(leftIndex >= rightIndex)
            {
                return 0;
            }
            else
            {
                int MiddleIndex = (leftIndex + rightIndex) / 2;
                // 左侧区间
                if(array[MiddleIndex - 1] < data && array[MiddleIndex] <= data && array[MiddleIndex + 1] >= data)
                {
                    return MiddleIndex;
                }
                else
                {
                    if(array[MiddleIndex] > data)
                    {
                        return TwoPointsSearchLeft(array, data, leftIndex, MiddleIndex - 1);
                    }
                    else
                    {
                        return TwoPointsSearchLeft(array, data, MiddleIndex + 1, rightIndex);
                    }
                }
            }
        }

        private static int TwoPointsSearchRight(int[] array, int data, int leftIndex, int rightIndex)
        {

            if(leftIndex >= rightIndex)
            {
                return array.Length - 1;
            }
            else
            {
                int MiddleIndex = (leftIndex + rightIndex) / 2;
                // 右侧区间
                if(array[MiddleIndex - 1] <= data && array[MiddleIndex] >= data && array[MiddleIndex + 1] > data)
                {
                    return MiddleIndex;
                }
                else
                {
                    if(array[MiddleIndex] > data)
                    {
                        return TwoPointsSearchRight(array, data, leftIndex, MiddleIndex - 1);
                    }
                    else
                    {
                        return TwoPointsSearchRight(array, data, MiddleIndex + 1, rightIndex);
                    }
                }
            }
        }
    }
}
