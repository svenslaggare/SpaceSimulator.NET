using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Mathematics
{
    /// <summary>
    /// Converts between different vector types
    /// </summary>
    public static class MathConversionsHelpers
    {
        /// <summary>
        /// Converts between the given vector types
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3 ToFloat(Vector3d vector)
        {
            return new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        /// <summary>
        /// Converts between the given vector types
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d ToDouble(Vector3 vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Swaps the y and z components
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d SwapYZ(Vector3d vector)
        {
            return new Vector3d(vector.X, vector.Z, vector.Y);
        }

        /// <summary>
        /// Converts the given scalar in world scale to draw scale
        /// </summary>
        /// <param name="world">The world scalar</param>
        public static float ToDraw(double world)
        {
            return (float)(Constants.DistanceScale * world);
        }

        /// <summary>
        /// Converts the given position in world position to draw
        /// </summary>
        /// <param name="worldPosition">The position in the world</param>
        public static Vector3 ToDraw(Vector3d worldPosition)
        {
            return ToFloat(Constants.DistanceScale * worldPosition);
        }

        /// <summary>
        /// Converts the given position in world position to a draw position
        /// </summary>
        /// <param name="worldPosition">The position in the world</param>
        public static Vector3 ToDrawPosition(Vector3d worldPosition)
        {
            return ToDraw(worldPosition);
        }
    }
}
