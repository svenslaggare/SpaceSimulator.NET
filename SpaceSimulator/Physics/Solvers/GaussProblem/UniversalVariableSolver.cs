using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Solves the Gauss problem using universal variables
    /// </summary>
    public sealed class GaussProblemUniversalVariableSolver : IGaussProblemSolver
    {
        private readonly System.Random random = new System.Random(1337);
        private readonly NormalRandomGenerator normalRandom = new NormalRandomGenerator(new System.Random(1337));
        private readonly int maxNumIterations = 1000;
        private readonly double convergenceEpsilon = 1E-6;

        /// <summary>
        /// The y variable
        /// </summary>
        private double CalculateY(double r1Length, double r2Length, double z, double S, double C, double A)
        {
            return r1Length + r2Length - (A * (1 - z * S)) / Math.Sqrt(C);
        }

        /// <summary>
        /// Solves for z
        /// </summary>
        private double SolveForZ(double deltaTrueAnomaly, double time, double sqrtMu, double r1Length, double r2Length, double A)
        {
            var max = Math.Pow(2 * Math.PI, 2);
            //var zn = random.NextDouble(-max, max);
            var zn = deltaTrueAnomaly * deltaTrueAnomaly;

            for (int i = 0; i < this.maxNumIterations; i++)
            {
                var S = MathHelpers.S(zn);
                var C = MathHelpers.C(zn);
                var y = CalculateY(r1Length, r2Length, zn, S, C, A);

                //If we get a negative z value, choose a new one
                if (y < 0)
                {
                    if (i < this.maxNumIterations - 1)
                    {
                        zn = random.NextDouble(-max, max);
                        //zn = this.normalRandom.Next(max, deltaTrueAnomaly * deltaTrueAnomaly);
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException("The gauss problem cannot be solved with the current parameters.");
                    }
                }

                var x = Math.Sqrt(y / C);
                var sqrtY = Math.Sqrt(y);

                //Check how far we are from the correct value
                var expectedTime = (x * x * x * S + A * sqrtY) / sqrtMu;
                var dt = time - expectedTime;
                if (Math.Abs(dt) <= this.convergenceEpsilon)
                {
                    break;
                }

                //Calculates the derivatives for S and C
                var Sp = 0.0;
                var Cp = 0.0;

                if (Math.Abs(zn) < 1E-3)
                {
                    Sp = (1.0 / 120.0) + (2.0 * zn) / 5040.0 - (3 * zn * zn) / 362880.0 + (4 * zn * zn * zn) / 39916800.0;
                    Cp = (1.0 / 24.0) + (2.0 * zn) / 720.0 - (3 * zn * zn) / 40320.0 + (4 * zn * zn * zn) / 3628800.0;
                }
                else
                {
                    Sp = (1 / (2 * zn)) * (C - 3 * S);
                    Cp = (1 / (2 * zn)) * (1 - zn * S - 2 * C);
                }

                //Adjust the z value
                var dtdz = x * x * x * (Sp - (3.0 * S * Cp) / (2.0 * C)) + (A / 8.0) * ((3.0 * S * sqrtY) / C + A / x);
                dtdz /= sqrtMu;
                zn += dt / dtdz;

                if (i % 200 == 0 && i > 0)
                {
                    zn = random.NextDouble(-max, max);
                    //zn = this.normalRandom.Next(max, deltaTrueAnomaly * deltaTrueAnomaly);
                }

                //This means that we have not reached desired level of accuracy
                if (i == this.maxNumIterations - 1)
                {
                    throw new ArgumentException("The gauss problem cannot be solved with the current parameters.");
                }
            }

            return zn;
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
            var sqrtMu = Math.Sqrt(mu);

            var deltaTrueAnomaly = MathHelpers.AngleBetween(r1, r2);
            if (!shortWay)
            {
                deltaTrueAnomaly = 2 * Math.PI - deltaTrueAnomaly;
            }

            //Solve for y
            var A = Math.Sign(Math.PI - deltaTrueAnomaly) * Math.Sqrt(r1Length * r2Length * (1 + Math.Cos(deltaTrueAnomaly)));
            var z = this.SolveForZ(deltaTrueAnomaly, time, sqrtMu, r1Length, r2Length, A);
            var y = CalculateY(r1Length, r2Length, z, MathHelpers.S(z), MathHelpers.C(z), A);

            //Calculate the velocity vectors at the two positions
            var f = 1 - y / r1Length;
            var g = A * Math.Sqrt(y / mu);
            var gp = 1 - y / r2Length;
       
            var v1 = (r2 - f * r1) / g;
            var v2 = (gp * r2 - r1) / g;
            return new GaussProblemResult(primaryBodyState1.Velocity + v1, primaryBodyState2.Velocity + v2);
        }
    }
}
