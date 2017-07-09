using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Helpers;

namespace SpaceSimulator.Rendering.Plot
{
    /// <summary>
    /// Contains helper methods for plots
    /// </summary>
    public static class PlotHelpers
    {
        /// <summary>
        /// Splits the given values into parts
        /// </summary>
        /// <param name="values">The values</param>
        /// <param name="splitPart">Determines when parts should be split</param>
        public static IEnumerable<IList<Vector2>> SplitIntoParts(IList<Vector2> values, Func<Vector2, Vector2, bool> splitPart)
        {
            var i = 0;
            while (i < values.Count)
            {
                var prev = values[i];
                var end = values.Count;

                for (int j = i + 1; j < values.Count; j++)
                {
                    var value = values[j];
                    if (splitPart(prev, value))
                    {
                        end = j;
                        break;
                    }

                    prev = value;
                }

                //Console.WriteLine($"{i}-{end}");
                yield return values.GetRange(i, end - i);
                i += end - i;
            }
        }
    }
}
