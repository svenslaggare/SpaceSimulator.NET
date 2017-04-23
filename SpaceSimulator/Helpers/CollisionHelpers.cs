using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Contains helper methods for collisions
    /// </summary>
    public static class CollisionHelpers
    {
        /// <summary>
        /// Checks if the two given spheres intersects
        /// </summary>
        /// <param name="center1">The center of the first sphere</param>
        /// <param name="radius1">The radius of the first sphere</param>
        /// <param name="center2">The center of the second sphere</param>
        /// <param name="radius2">The radius of the second sphere></param>
        /// <returns>True if intersection else false</returns>
        public static bool SphereIntersection(Vector3d center1, double radius1, Vector3d center2, double radius2)
        {
            var sumOfRadii = radius1 + radius2;
            return Vector3d.DistanceSquared(center1, center2) <= sumOfRadii * sumOfRadii;
        }

        /// <summary>
        /// Computes sphere-ray intersection
        /// </summary>
        /// <param name="center">The center</param>
        /// <param name="radius">The radius</param>
        /// <param name="start">The start position of the object</param>
        /// <param name="dir">The direction of the object</param>
        /// <param name="ip1">The first intersection point</param>
        /// <param name="ip2">The second intersection point</param>
        /// <returns>True if intesection, else false.</returns>
        public static bool SphereRayIntersection(Vector3d center, double radius, Vector3d start, Vector3d dir, out Vector3d ip1, out Vector3d ip2)
        {
            //Compute the value t of the closest point to the circle center (Cx, Cy)
            dir.Normalize();
            var t = dir.X * (center.X - start.X) + dir.Z * (center.Z - start.Z);

            //Since we are a ray, we don't allow collision in the opposite direction.
            if (t < 0)
            {
                ip1 = new Vector3d();
                ip2 = new Vector3d();
                return false;
            }

            // This is the projection of C on the line from A to B.

            // compute the coordinates of the point E on line and closest to C
            var E = new Vector3d(t * dir.X + start.X, 0, t * dir.Z + start.Z);
            var LEC = (E - center).Length();

            if (LEC < radius)
            {
                // compute distance from t to circle intersection point
                var dt = Math.Sqrt(radius * radius - LEC * LEC);

                //Compute the intersection points
                ip1 = new Vector3d(
                    (t - dt) * dir.X + start.X,
                    0,
                    (t - dt) * dir.Z + start.Z);

                ip2 = new Vector3d(
                   (t + dt) * dir.X + start.X,
                   0,
                   (t + dt) * dir.Z + start.Z);

                return true;
            }
            else
            {
                ip1 = new Vector3d();
                ip2 = new Vector3d();
            }

            return false;
        }
    }
}
