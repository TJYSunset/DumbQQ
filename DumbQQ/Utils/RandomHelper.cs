using System;

namespace DumbQQ.Utils
{
    internal static class RandomHelper
    {
        private static readonly Random _rand = new Random();

        /// <summary>
        ///     返回一个随机int。
        /// </summary>
        /// <returns>随机数。</returns>
        public static long GetRandomInt()
        {
            return _rand.Next();
        }

        /// <summary>
        ///     返回一个随机double。
        /// </summary>
        /// <returns>随机数。</returns>
        public static double GetRandomDouble()
        {
            return _rand.NextDouble();
        }
    }
}