using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceSimulator.Mathematics
{
    /// <summary>
    /// Represents a random generator using a normal distribution.
    /// </summary>
    /// <remarks>The implemented generator is the Box-Muller transform.</remarks>
    public sealed class NormalRandomGenerator
    {
        private readonly System.Random random;
        private bool hasNext = false;
        private double nextValue;

        /// <summary>
        /// Creates a new normal random generator
        /// </summary>
        /// <param name="random">The uniform random generator to use</param>
        public NormalRandomGenerator(System.Random random)
        {
            this.random = random;
        }

        /// <summary>
        /// Generates a random value from the given normal distribution.
        /// </summary>
        /// <param name="standardDeviation">The standard deviation</param>
        /// <param name="mean">The mean</param>
        public double Next(double standardDeviation = 1.0, double mean = 0.0)
        {
            //Since the algorithm generates two random values, we reuse one of them.
            if (this.hasNext)
            {
                this.hasNext = false;
                return standardDeviation * this.nextValue + mean;
            }
            else
            {
                double u;
                double v;
                double s;

                while (true)
                {
                    //Generate two random numbers in the range [-1, +1]
                    u = 2 * this.random.NextDouble() - 1;
                    v = 2 * this.random.NextDouble() - 1;
                    s = u * u + v * v;

                    //If s >= 1 or s == 0 try again.
                    if (s > 0 && s < 1)
                    {
                        break;
                    }
                }

                double factor = Math.Sqrt(-2 * Math.Log(s) / s);
                double z0 = u * factor;
                double z1 = v * factor;

                this.hasNext = true;
                this.nextValue = z1;

                return standardDeviation * z0 + mean;
            }
        }
    }
}
