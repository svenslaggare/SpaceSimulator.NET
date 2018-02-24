using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator
{
    /// <summary>
    /// Debug writer
    /// </summary>
    public static class DebugWriter
    {
        private static readonly IList<string> lines = new List<string>();

        /// <summary>
        /// Writes the given lines to the debugger
        /// </summary>
        /// <param name="line">The line</param>
        public static void WriteLine(string line)
        {
            lines.Add(line);
        }

        /// <summary>
        /// Returns the lines
        /// </summary>
        public static IList<string> Lines => lines;

        /// <summary>
        /// Clears the lines
        /// </summary>
        public static void Clear()
        {
            lines.Clear();
        }
    }
}
