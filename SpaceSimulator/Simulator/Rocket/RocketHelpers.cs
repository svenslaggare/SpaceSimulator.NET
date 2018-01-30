using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Contains helper functions for rockets
    /// </summary>
    public static class RocketHelpers
    {
        /// <summary>
        /// Calculates the absolute thrust direction from a relative thrust direction
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="thrustDirection">The relative thrust direction</param>
        public static Vector3d RelativeToAbsoluteThrustDirection(RocketObject rocketObject, Vector3d thrustDirection)
        {
            return Vector3d.Transform(thrustDirection, rocketObject.Orientation).Normalized();
        }

        /// <summary>
        /// Calculates the relative thrust direction from an absolute thrust direction
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="thrustDirection">The absolute thrust direction</param>
        public static Vector3d AbsoluteToRelativeThrustDirection(RocketObject rocketObject, Vector3d thrustDirection)
        {
            var orientation = rocketObject.Orientation;
            orientation.Conjugate();
            orientation.Normalize();
            return Vector3d.Transform(thrustDirection, orientation).Normalized();
        }
    }
}
