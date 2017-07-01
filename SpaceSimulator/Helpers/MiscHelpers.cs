using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Contains various helper methods
    /// </summary>
    public static class MiscHelpers
    {
        /// <summary>
        /// Returns a random double in the range [min, max]
        /// </summary>
        /// <param name="random">The random generator</param>
        /// <param name="min">The min</param>
        /// <param name="max">The max</param>
        public static double NextDouble(this System.Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Rounds the given time to whole days
        /// </summary>
        /// <param name="time">The time in seconds</param>
        public static double RoundToDays(double time)
        {
            var day = 24.0 * 60.0 * 60.0;
            return Math.Round(time / day) * day;
        }

        /// <summary>
        /// Normalizes the given value into the range [0, 1] such that the min value becomes 0 and the max value 1
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="value">The value</param>
        public static double RangeNormalize(double min, double max, double value)
        {
            return (value - min) / (max - min);
        }
    }
}
