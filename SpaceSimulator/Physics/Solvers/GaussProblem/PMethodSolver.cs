using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Solves the Gauss problem using the p-method
    /// </summary>
    public sealed class GaussProblemPMethodSolver : IGaussProblemSolver
    {
        private readonly System.Random random = new System.Random(1337);
        private readonly int maxNumIterations = 1000;
        private readonly double convergenceEpsilon = 1E-4;

        private struct FAndGValues
        {
            public double f;
            public double g;
            public double fp;
        }

        /// <summary>
        /// Calculates the f and g values
        /// </summary>
        private FAndGValues CalculateFAndG(
            double r1Length, double r2Length, double mu, 
            double cosDeltaTrueAnomaly, double sinDeltaTrueAnomaly, double tanHalfTrueAnomaly,
            double p)
        {
            var f = 1 - (r2Length / p) * (1 - cosDeltaTrueAnomaly);
            var g = (r1Length * r2Length * sinDeltaTrueAnomaly) / Math.Sqrt(mu * p);
            var fp = Math.Sqrt(mu / p) * tanHalfTrueAnomaly * (((1 - cosDeltaTrueAnomaly) / p) - (1 / r1Length) - (1 / r2Length));
            return new FAndGValues()
            {
                f = f,
                g = g,
                fp = fp
            };
        }

        /// <summary>
        /// Solves for p
        /// </summary>
        private double SolveForP(
            double r1Length, double r2Length, double mu,
            double cosDeltaTrueAnomaly, double sinDeltaTrueAnomaly, double tanHalfTrueAnomaly,
            double time)
        {
            var k = r1Length * r2Length * (1 - cosDeltaTrueAnomaly);
            var l = r1Length + r2Length;
            var m = r1Length * r2Length * (1 + cosDeltaTrueAnomaly);

            var pLower = k / (l + Math.Sqrt(2 * m));
            var pUpper = k / (l - Math.Sqrt(2 * m));
            var p = random.NextDouble(pLower, pUpper);
            var prevP = 0.0;
            
            for (int i = 0; i < this.maxNumIterations; i++)
            {
                var a = (m * k * p) / ((2 * m - l * l) * p * p + (2 * k * l * p) - k * k);
                var res = CalculateFAndG(r1Length, r2Length, mu, cosDeltaTrueAnomaly, sinDeltaTrueAnomaly, tanHalfTrueAnomaly, p);
                var f = res.f;
                var g = res.g;
                var fp = res.fp;

                var anomalyRHS = 1 - (r1Length / a) * (1 - f);
                var expectedTime = 0.0;
                bool isElliptical = a >= 0;
                var sinDeltaAnomaly = 0.0;

                if (isElliptical)
                {
                    //Elliptical
                    var deltaE = Math.Acos(anomalyRHS);
                    var sinDeltaE = (-r1Length * r2Length * fp) / Math.Sqrt(mu * a);
                    sinDeltaAnomaly = sinDeltaE;
                    expectedTime = g + Math.Sqrt(Math.Pow(a, 3) / mu) * (deltaE - sinDeltaE);
                }
                else
                {
                    //Hyperbolic
                    var deltaF = MathHelpers.Acosh(anomalyRHS);
                    var sinhDeltaF = Math.Sinh(deltaF);
                    sinDeltaAnomaly = sinhDeltaF;
                    expectedTime = g + Math.Sqrt(Math.Pow(-a, 3) / mu) * (sinhDeltaF - deltaF);
                }

                //Check how far we are from the correct value
                var dt = time - expectedTime;
                if (Math.Abs(dt) <= this.convergenceEpsilon)
                {
                    break;
                }

                //Adjust the p value
                var dtdp = 0.0;

                var dtdpTerm1 = (-g / (2.0 * p))
                                - ((3.0 / 2.0) * a * (time - g)) * ((k * k + (2 * m - l * l) * p * p) / (m * k * p * p));
                if (isElliptical)
                {
                    dtdp = dtdpTerm1
                           + Math.Sqrt(Math.Pow(a, 3) / mu) * ((2 * k * sinDeltaAnomaly) / (p * (k - l * p)));
                }
                else
                {
                    dtdp = dtdpTerm1
                           - Math.Sqrt(Math.Pow(-a, 3) / mu) * ((2 * k * sinDeltaAnomaly) / (p * (k - l * p)));
                }

                prevP = p;
                p += dt / dtdp;

                //Get a new initial guess if not converging!
                bool newGuess = false;
                //if (Math.Abs(prevP - p) >= 100 && Math.Abs(dt) >= 100)
                //{
                //    newGuess = true;
                //}

                if (i > 0 && i % 100 == 0)
                {
                    newGuess = true;
                }

                if (newGuess)
                {
                    p = random.NextDouble(pLower, pUpper);
                }

                //This means that we have not reached desired level of accuracy
                if (i == this.maxNumIterations - 1)
                {
                    throw new ArgumentException("The gauss problem cannot be solved with the current parameters.");
                }
            }

            return p;
        }

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

            var cosDeltaTrueAnomaly = Math.Cos(deltaTrueAnomaly);
            var sinDeltaTrueAnomaly = Math.Sin(deltaTrueAnomaly);
            var tanHalfTrueAnomaly = Math.Tan(deltaTrueAnomaly / 2);

            //Solve for p
            var p = this.SolveForP(
                r1Length, 
                r2Length,
                mu, 
                cosDeltaTrueAnomaly,
                sinDeltaTrueAnomaly, 
                tanHalfTrueAnomaly, 
                time);

            //Calculate velocities
            var res = CalculateFAndG(
                r1Length, 
                r2Length, 
                mu, 
                cosDeltaTrueAnomaly, 
                sinDeltaTrueAnomaly,
                tanHalfTrueAnomaly,
                p);

            var f = res.f;
            var g = res.g;
            var fp = res.fp;

            var gp = 1 - (r1Length / p) * (1 - cosDeltaTrueAnomaly);
            var v1 = (r2 - f * r1) / g;
            var v2 = fp * r1 + gp * v1;
            return new GaussProblemResult(primaryBodyState1.Velocity + v1, primaryBodyState2.Velocity + v2);
        }
    }
}
