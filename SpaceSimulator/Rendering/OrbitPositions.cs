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

                var position = MathConversionsHelpers.ToDraw(newState.Position);
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
        private static IList<Orbit.Point> CreateForUnbound(Physics.Orbit orbit, bool relative)
        {
            var orbitPositions = new List<Orbit.Point>();

            if (orbit.PrimaryBody == null || orbit.Parameter == 0.0)
            {
                return orbitPositions;
            }

            var theta = Math.Acos(-1 / orbit.Eccentricity);

            var deltaAngle = 0.01;
            for (double trueAnomaly = -theta; trueAnomaly <= theta; trueAnomaly += deltaAngle)
            {
                var primaryBodyState = relative ? new ObjectState() : orbit.PrimaryBody.State;
                var newState = orbit.CalculateState(trueAnomaly, ref primaryBodyState);

                var position = MathConversionsHelpers.ToDraw(newState.Position);
                if (!(MathHelpers.IsNaN(position) || MathHelpers.IsInfinity(position) || float.IsInfinity(position.LengthSquared())))
                {
                    orbitPositions.Add(new Orbit.Point(position, trueAnomaly));
                }
            }

            return orbitPositions;
        }

        /// <summary>
        /// Creates the positions for the given orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="relative">Indicates if the positions are relative</param>
        public static IList<Orbit.Point> Create(Physics.Orbit orbit, bool relative = false)
        {
            if (orbit.IsBound)
            {
                return CreateForBound(orbit, relative);
            }
            else
            {
                return CreateForUnbound(orbit, relative);
            }
        }
    }
}
