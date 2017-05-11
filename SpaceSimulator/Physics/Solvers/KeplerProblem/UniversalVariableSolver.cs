using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Represents a solver to the Kepler Problem using the universal variable formulation
    /// </summary>
    public sealed class KeplerProblemUniversalVariableSolver : IKeplerProblemSolver
    {
        private readonly System.Random random = new System.Random();
        private readonly int maxNumIterations = 1500;
        private readonly double convergenceEpsilon = 1E-6;

        /// <summary>
        /// Returns an initial guess for the universal variable
        /// </summary>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="r0">The radius vector</param>
        /// <param name="v0">The velocity vector</param>
        /// <param name="time">The time</param>
        /// <param name="sqrtMu">The square root of mu</param>
        /// <param name="alpha">The alpha value (1 / a)</param>
        private double InitialUniversalVariableGuess(
            Orbit initialOrbit,
            ref Vector3d r0,
            ref Vector3d v0, 
            ref double time,
            ref double sqrtMu,
            ref double alpha)
        {
            if (initialOrbit.IsElliptical)
            {
                return (sqrtMu * time) * alpha;
            }
            else if (initialOrbit.IsHyperbolic)
            {
                var a = 1.0 / alpha;
                var mu = initialOrbit.StandardGravitationalParameter;
                var lnFactor = (-2 * mu * time)
                               / (a * (Vector3d.Dot(r0, v0) + Math.Sign(time) * Math.Sqrt(-mu * a) * (1 - (r0.Length() * alpha))));
                var guess = Math.Sign(time) * Math.Sqrt(-a) * Math.Log(lnFactor);

                //Will diverage if to big guess
                if (Math.Abs(guess) > 100)
                {
                    return random.NextDouble();
                }

                return guess;
            }
            else
            {
                //TODO: Find formula for parabolic guess!
                return random.NextDouble();
            }
        }

        /// <summary>
        /// Solves for the universal variable
        /// </summary>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="r0">The radius vector</param>
        /// <param name="v0">The velocity vector</param>
        /// <param name="time">The time</param>
        /// <param name="sqrtMu">The square root of mu</param>
        /// <param name="alpha">The alpha value (1/a)</param>
        private double SolveForUniversalVariable(
            Orbit initialOrbit,
            ref Vector3d r0,
            ref Vector3d v0, 
            ref double time,
            ref double sqrtMu,
            ref double alpha)
        {
            var r0v0dot = Vector3d.Dot(r0, v0);
            var xn = InitialUniversalVariableGuess(initialOrbit, ref r0, ref v0, ref time, ref sqrtMu, ref alpha);
            var r0Length = r0.Length();
            var r0v0dotBySqrtMu = r0v0dot / sqrtMu;

            for (int i = 0; i < this.maxNumIterations; i++)
            {
                var xnSquared = xn * xn;
                var z = xnSquared * alpha;
                var Cz = MathHelpers.C(z);
                var Sz = MathHelpers.S(z);
                var tn = (r0v0dotBySqrtMu * xnSquared * Cz
                          + (1 - r0Length * alpha) * xnSquared * xn * Sz + r0Length * xn)
                          / sqrtMu;

                var dtdx = xnSquared * Cz + r0v0dotBySqrtMu * xn * (1 - z * Sz) + r0Length * (1 - z * Cz);
                dtdx /= sqrtMu;
                var dt = time - tn;
                xn += dt / dtdx;

                if (Math.Abs(dt) <= this.convergenceEpsilon)
                {
                    break;
                }
            }

            return xn;
        }

        /// <summary>
        /// Returns the state at the given time
        /// </summary>
        /// <param name="config">The configuration of the object</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="primaryBodyStateAtTime">The primary body state at the time</param>
        /// <param name="time">The time since the initial state/orbit</param>
        public ObjectState Solve(
            ObjectConfig config,
            ref ObjectState initialPrimaryBodyState,
            ref ObjectState initialState,
            Orbit initialOrbit,
            ref ObjectState primaryBodyStateAtTime,
            double time)
        {
            if (time == 0)
            {
                return initialState;
            }

            if (initialState.HasImpacted)
            {
                return SolverHelpers.MoveImpactedObject(
                    initialOrbit.PrimaryBody.Configuration,
                    initialPrimaryBodyState,
                    primaryBodyStateAtTime,
                    initialState,
                    time);
            }

            //Compute the radius and velocity vectors
            var r0 = initialState.Position - initialPrimaryBodyState.Position;
            var v0 = initialState.Velocity - initialPrimaryBodyState.Velocity;

            //Some useful constants
            var r0Length = r0.Length();
            var mu = initialOrbit.StandardGravitationalParameter;
            var sqrtMu = Math.Sqrt(mu);
            var alpha = ((2 * mu) / r0Length - v0.LengthSquared()) / mu;
            if (initialOrbit.IsParabolic)
            {
                alpha = 0;
            }

            //Solve for x
            var x = SolveForUniversalVariable(initialOrbit, ref r0, ref v0, ref time, ref sqrtMu, ref alpha);
            var z = (x * x) * alpha;
            var Sz = MathHelpers.S(z);
            var Cz = MathHelpers.C(z);

            //Compute the radius vector
            var f = 1 - ((x * x) / r0Length) * Cz;
            var g = time - ((x * x * x) / sqrtMu) * Sz;
            var r = f * r0 + g * v0;

            //Compute the velocity vector
            var rLength = r.Length();
            var gp = 1 - ((x * x) / rLength) * Cz;
            var fp = (sqrtMu / (r0Length * rLength)) * x * (z * Sz - 1);
            var v = fp * r0 + gp * v0;

            var rotation = SolverHelpers.CalculateRotation(config.RotationalPeriod, initialState.Rotation, time);

            //The acceleration is merely used for display
            return new ObjectState(
                initialState.Time + time,
                primaryBodyStateAtTime.Position + r,
                primaryBodyStateAtTime.Velocity + v,
                OrbitFormulas.GravityAcceleration(mu, r),
                rotation);
        }
    }
}
