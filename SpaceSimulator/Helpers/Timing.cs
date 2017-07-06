using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Calculates the time it takes to execute a block
    /// </summary>
    public struct Timing : IDisposable
    {
        private readonly DateTime startTime;
        private readonly string stringFormat;

        /// <summary>
        /// Creates a new timing
        /// </summary>
        /// <param name="stringFormat">The format to print</param>
        public Timing(string stringFormat)
        {
            this.startTime = DateTime.UtcNow;
            this.stringFormat = stringFormat;
        }

        public void Dispose()
        {
            Console.WriteLine(string.Format(this.stringFormat, (DateTime.UtcNow - this.startTime)));
        }
    }
}
