using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Represents a solver for the Gauss problem using Gauss' method.
    /// This method should only be used if the delta true anomaly is small, else the method will diverge.
    /// </summary>
    public sealed class GaussProblemGaussMethodSolver : IGaussProblemSolver
    {
        private readonly int maxNumIterations = 500;
        private readonly double convergenceEpsilon = 1E-5;

        /// <summary>
        /// Solves for the velocites of the given positions and time-of-flight
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyState1">The state of the primary body at the first position</param>
        /// <param name="primaryBodyState2">The state of the primary body at the second position</param>
        /// <param name="position1">The first position</param>
        /// <param name="position2">The second position</param>
        /// <param name="time">The time between the positions</param>
        /// <remarks>The time between the positions should be large enough so that they don't appear to be on a line.</remarks>
        /// <param name="shortWay">Indicates if the short way is taken</param>
        public GaussProblemResult Solve(
            IPhysicsObject primaryBody,
            ObjectState primaryBodyState1,
            ObjectState primaryBodyState2,
            Vector3d position1,
            Vector3d position2,
            double time,
            bool shortWay = true)
        {
            var r1 = position1 - primaryBodyState1.Position;
            var r2 = position2 - primaryBodyState2.Position;

            var r1Length = r1.Length();
            var r2Length = r2.Length();
            var mu = primaryBody.StandardGravitationalParameter;

            var deltaTrueAnomaly = MathHelpers.AngleBetween(r1, r2);
            if (!shortWay)
            {
                deltaTrueAnomaly = 2 * Math.PI - deltaTrueAnomaly;
            }

            var c = Math.Sqrt(r1Length * r2Length) * Math.Cos(deltaTrueAnomaly / 2);
            var s = (r1Length + r2Length) / (4.0 * c) - 0.5;
            var w = (mu * time * time) / Math.Pow(2 * c, 3);

            //Compute y
            var yn = 1.0;
            var pyn = 0.0;
            for (int i = 0; i < this.maxNumIterations; i++)
            {
                var xn = (w / (yn * yn)) - s;
                var Xn = 
                    (4.0 / 3.0)
                    * (1 
                        + (6.0 / 5.0) * xn
                        + ((6.0 * 8.0) / (5.0 * 7.0)) * Math.Pow(xn, 2)
                        + ((6.0 * 8.0 * 10.0) / (5.0 * 7.0 * 9.0)) * Math.Pow(xn, 3)
                        + ((6.0 * 8.0 * 10.0 * 12.0) / (5.0 * 7.0 * 9.0 * 11.0)) * Math.Pow(xn, 4)
                        + ((6.0 * 8.0 * 10.0 * 12.0 * 14.0) / (5.0 * 7.0 * 9.0 * 11.0 * 13.0)) * Math.Pow(xn, 5));

                pyn = yn;
                yn = 1 + Xn * (s + xn);

                if (Math.Abs(pyn - yn) <= this.convergenceEpsilon)
                {
                    break;
                }
            }

            var x = w / (yn * yn) - s;
            var cosFactor = 0.0;
            if (x > 0)
            {
                //Elliptic
                cosFactor = 1 - 2.0 * (w / (yn * yn) - s);
            }
            else if (x < 0)
            {
                //Hyperbolic
                cosFactor = 1 - 2.0 * (w / (yn * yn) - s);
            }
            else
            {
                //Parabolic
                throw new ArgumentException("Parabolic not implemented");
            }

            var p = (r1Length * r2Length * (1.0 - Math.Cos(deltaTrueAnomaly))) / (r1Length + r2Length - 2.0 * c * cosFactor);
            var f = 1 - (r2Length / p) * (1 - Math.Cos(deltaTrueAnomaly));
            var g = (r1Length * r2Length * Math.Sin(deltaTrueAnomaly)) / Math.Sqrt(mu * p);
            var fp = Math.Sqrt(mu / p) * Math.Tan(deltaTrueAnomaly / 2.0) * ((1 - Math.Cos(deltaTrueAnomaly)) / p - (1 / r1Length) - (1 / r2Length));
            var gp = 1.0 - (r1Length / p) * (1 - Math.Cos(deltaTrueAnomaly));

            var v1 = (r2 - f * r1) / g;
            var v2 = fp * r1 + gp * v1;

            return new GaussProblemResult(primaryBodyState1.Velocity + v1, primaryBodyState2.Velocity + v2);
        }
    }
}
