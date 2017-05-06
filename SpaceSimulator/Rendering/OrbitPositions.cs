using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

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

                var position = MathHelpers.ToDrawPosition(newState.Position);
                if (!(MathHelpers.IsNaN(position) || MathHelpers.IsInfinity(position) || float.IsInfinity(position.LengthSquared())))
                {
                    orbitPositions.Add(new Orbit.Point(position, trueAnomaly));
                }
            }

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

            if (orbit.PrimaryBody == null || orbit.Parameter == 0.0)
            {
                return orbitPositions;
            }

            var theta = Math.Acos(-1 / orbit.Eccentricity);
            //var theta = -MathUtild.Pi;

            var deltaAngle = 0.001 / 2;
            //for (double currentTrueAnomaly = -theta; currentTrueAnomaly <= theta; currentTrueAnomaly += deltaAngle)

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

                var position = MathHelpers.ToDrawPosition(newState.Position);
                if (!(MathHelpers.IsNaN(position) || MathHelpers.IsInfinity(position) || float.IsInfinity(position.LengthSquared())))
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
