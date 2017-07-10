using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Calculates the positions in the orbit used for rendering
    /// </summary>
    public static class OrbitPositions
    {
        /// <summary>
        /// Creates orbit positions for a bound orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="relative">Indicates if the positions are relative</param>
        private static IList<Orbit.Point> CreateForBound(Physics.Orbit orbit, bool relative)
        {
            var orbitPositions = new List<Orbit.Point>();

            if (orbit.PrimaryBody == null || orbit.Parameter == 0.0)
            {
                return orbitPositions;
            }

            var deltaAngle = 0.01 / 2.0;
            for (double trueAnomaly = 0; trueAnomaly <= MathUtild.TwoPi; trueAnomaly += deltaAngle)
            {
                var primaryBodyState = relative ? new ObjectState() : orbit.PrimaryBody.State;
                var newState = orbit.CalculateState(trueAnomaly, ref primaryBodyState);

                var position = newState.Position;
                if (!(MathHelpers.IsNaN(position) || MathHelpers.IsInfinity(position) || double.IsInfinity(position.LengthSquared())))
                {
                    orbitPositions.Add(new Orbit.Point(position, trueAnomaly));
                }
            }

            return orbitPositions;
        }

        /// <summary>
        /// Creates orbit positions for a radial trajectory
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="orbitPosition">The position in the orbit</param>
        public static IList<Orbit.Point> CreateRadialTrajectory(PhysicsObject physicsObject, OrbitPosition orbitPosition)
        {
            var orbitPositions = new List<Orbit.Point>();

            var primaryBody = physicsObject.PrimaryBody;
            var radius = physicsObject.State.Position - primaryBody.State.Position;
            var maxDistance = 1E12;

            orbitPositions.Add(new Orbit.Point(radius + -maxDistance * MathHelpers.Normalized(radius), 0.0));
            orbitPositions.Add(new Orbit.Point(radius, 0.0));
            orbitPositions.Add(new Orbit.Point(primaryBody.Radius * MathHelpers.Normalized(radius), 0.0));
            orbitPositions.Add(new Orbit.Point(radius + maxDistance * MathHelpers.Normalized(radius), 0.0));
            orbitPositions.Sort((x, y) => x.Position.Length().CompareTo(y.Position.Length()));

            return orbitPositions;
        }

        /// <summary>
        /// Creates orbit positions for a unbound orbit for the given amount of time
        /// </summary>
        /// <param name="keplerProblemSolver">The kepler problem solver</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="orbit">The orbit of the object</param>
        /// <param name="duration">The duration</param>
        /// <param name="deltaTime">The time step</param>
        public static IList<Orbit.Point> CreateForUnbound(
            IKeplerProblemSolver keplerProblemSolver,
            PhysicsObject physicsObject,
            Physics.Orbit orbit,
            double duration,
            double deltaTime = 100.0)
        {
            var orbitPositions = new List<Orbit.Point>();

            var initialState = physicsObject.State;
            var initialPrimaryBodyState = physicsObject.PrimaryBody.State;

            for (double t = 0; t <= duration; t += deltaTime)
            {
                var nextState = keplerProblemSolver.Solve(
                    physicsObject,
                    ref initialPrimaryBodyState,
                    ref initialState,
                    orbit,
                    ref initialPrimaryBodyState,
                    t);

                var orbitPosition = OrbitPosition.CalculateOrbitPosition(
                    physicsObject.PrimaryBody,
                    ref initialPrimaryBodyState,
                    ref nextState);

                orbitPositions.Add(new Orbit.Point(
                    nextState.Position - initialPrimaryBodyState.Position,
                    orbitPosition.TrueAnomaly));
            }

            return orbitPositions;
        }

        /// <summary>
        /// Creates orbit positions for a unbound orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="relative">Indicates if the positions are relative</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        private static IList<Orbit.Point> CreateForUnbound(Physics.Orbit orbit, bool relative, double? trueAnomaly)
        {
            var orbitPositions = new List<Orbit.Point>();

            if (orbit.PrimaryBody == null)
            {
                return orbitPositions;
            }

            if (orbit.Parameter == 0.0)
            {
                return orbitPositions;
            }

            var theta = Math.Acos(-1 / orbit.Eccentricity);
            //var theta = -MathUtild.Pi;

            var deltaAngle = 0.001 / 2.0;
            //deltaAngle /= 10000.0;

            var startTrueAnomaly = -theta;
            var stopTrueAnomaly = theta;
            if (trueAnomaly.HasValue)
            {
                startTrueAnomaly = Math.Min(trueAnomaly.Value, startTrueAnomaly);
                stopTrueAnomaly = Math.Max(trueAnomaly.Value, stopTrueAnomaly);
            }

            for (double currentTrueAnomaly = startTrueAnomaly; currentTrueAnomaly <= stopTrueAnomaly; currentTrueAnomaly += deltaAngle)
            {
                var primaryBodyState = relative ? new ObjectState() : orbit.PrimaryBody.State;
                var newState = orbit.CalculateState(currentTrueAnomaly, ref primaryBodyState);

                var position = newState.Position;
                if (!(MathHelpers.IsNaN(position) || MathHelpers.IsInfinity(position) || double.IsInfinity(position.LengthSquared())))
                {
                    orbitPositions.Add(new Orbit.Point(position, currentTrueAnomaly));
                }
            }

            return orbitPositions;
        }

        /// <summary>
        /// Creates the positions for the given orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="relative">Indicates if the positions are relative</param>
        /// <param name="trueAnomaly">The current true anomaly. Useful for hyperbolic orbits.</param>
        public static IList<Orbit.Point> Create(Physics.Orbit orbit, bool relative = false, double? trueAnomaly = null)
        {
            if (orbit.IsBound)
            {
                return CreateForBound(orbit, relative);
            }
            else
            {
                return CreateForUnbound(orbit, relative, trueAnomaly);
            }
        }
    }
}
