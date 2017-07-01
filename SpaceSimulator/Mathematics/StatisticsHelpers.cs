using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Mathematics
{
    /// <summary>
    /// Contains statistics related helper methods
    /// </summary>
    public static class StatisticsHelpers
    {
        /// <summary>
        /// Computes the 2D pdf for the given normal distribution
        /// </summary>
        /// <param name="mean">The mean</param>
        /// <param name="standardDeviation">The standard deviation</param>
        /// <param name="correlation">The correlation between x and y</param>
        /// <param name="p">The point to calculate for</param>
        public static double NormalPDF(Vector2d mean, Vector2d standardDeviation, double correlation, Vector2d p)
        {
            var standardDeviationProduct = standardDeviation.X * standardDeviation.Y;
            var diffX = p.X - mean.X;
            var diffY = p.Y - mean.Y;

            var squaredCorrelation = correlation * correlation;

            var z = 
                (MathHelpers.Square(diffX) / MathHelpers.Square(standardDeviation.X))
                - ((2 * correlation * diffX * diffY) / standardDeviationProduct)
                + (MathHelpers.Square(diffY) / MathHelpers.Square(standardDeviation.Y));

            var scaleFactor = MathUtild.TwoPi * standardDeviationProduct * Math.Sqrt(1 - squaredCorrelation);

            return (1.0 / scaleFactor) * Math.Exp(-z / (2.0 * (1 - squaredCorrelation)));
        }
    }
}
