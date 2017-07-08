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
        /// <param name="relative">Indicates if the positions are relative</param>
        public static IList<Orbit.Point> CreateRadialTrajectory(PhysicsObject physicsObject, OrbitPosition orbitPosition, bool relative)
        {
            var orbitPositions = new List<Orbit.Point>();

            //var keplerProblemSolver = new KeplerProblemUniversalVariableSolver();
            //var initialState = physicsObject.State;
            //var maxTime = 2.0 * TimeConstants.OneDay;

            var primaryBody = physicsObject.PrimaryBody;
            //var impactTime = 1000.0;
            //var altitude = primaryBody.Altitude(physicsObject.Position);

            //if (altitude >= 1E3)
            //{
            //    impactTime = Math.Sqrt((2.0 * Math.Pow(altitude, 3.0)) / (9.0 * primaryBody.StandardGravitationalParameter));
            //}

            //var previousDistance = 0.0;
            //var deltaTime = 600.0;
            //for (double t = -maxTime * 0.0; t <= impactTime + deltaTime; t += deltaTime)
            //{
            //    var state = SolverHelpers.AfterTime(
            //        keplerProblemSolver,
            //        physicsObject,
            //        initialState,
            //        orbitPosition.Orbit,
            //        Math.Min(t, impactTime),
            //        relative: relative);
            //    orbitPositions.Add(new Orbit.Point(state.Position, 0.0));
            //    //Console.WriteLine(state.Position.Length());

            //    var distance = state.Position.Length();
            //    var deltaDistance = distance - previousDistance;
            //    previousDistance = distance;

            //    //if (state.Position.Length() < physicsObject.PrimaryBody.Radius)
            //    //{
            //    //    break;
            //    //}
            //}

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
        /// Creates orbit positions for a bound orbit
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
            //if (trueAnomaly.HasValue)
            //{
            //    var range = theta / 2.0;
            //    startTrueAnomaly = trueAnomaly.Value - range;
            //    stopTrueAnomaly = trueAnomaly.Value + range;
            //}

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
