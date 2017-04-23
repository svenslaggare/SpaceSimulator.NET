using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains helper methods for orbits
    /// </summary>
    public static class OrbitHelpers
    {
        /// <summary>
        /// Returns the prograde vector
        /// </summary>
        /// <param name="velocity">The velocity vector</param>
        public static Vector3d Prograde(Vector3d velocity)
        {
            return MathHelpers.Normalized(velocity);
        }

        /// <summary>
        /// Returns the normal vector
        /// </summary>
        /// <param name="prograde">The prograde vector</param>
        public static Vector3d Normal(Vector3d prograde)
        {
            return MathHelpers.Normalized(Vector3d.Cross(prograde, Radial(prograde)));
        }

        /// <summary>
        /// Returns the radial vector
        /// </summary>
        /// <param name="prograde">The prograde vector</param>
        public static Vector3d Radial(Vector3d prograde)
        {
            return MathHelpers.Normalized(Vector3d.Transform(prograde, Matrix3x3d.RotationY(Math.PI / 2)));
        }

        /// <summary>
        /// Computes the components for the given velocity
        /// </summary>
        /// <param name="state">The reference state</param>
        /// <param name="velocity">The velocity</param>
        /// <returns>(Prograde, Normal, Radial)</returns>
        public static (double, double, double) ComputeVelocityComponents(ref ObjectState state, Vector3d velocity)
        {
            var basis = Matrix<double>.Build.Dense(3, 3);
            basis.SetColumn(0, state.Prograde.ToArray());
            basis.SetColumn(1, state.Normal.ToArray());
            basis.SetColumn(2, state.Radial.ToArray());

            var input = Vector<double>.Build.Dense(velocity.ToArray());
            var result = basis.Solve(input);
            return (result[0], result[1], result[2]);
        }

        /// <summary>
        /// Computes the horizontal and vertical components for the given velocity
        /// </summary>
        /// <param name="gravityAccelerationDirection">The direction of the gravitational acceleration</param>
        /// <param name="velocity">The velocity</param>
        /// <returns>(Horizontal, Vertical)</returns>
        public static (double, double) ComputeHorizontalAndVerticalVelocity(Vector3d gravityAccelerationDirection, Vector3d velocity)
        {
            var verticalDir = MathHelpers.Normalized(-gravityAccelerationDirection);
            var horizontalDir = MathHelpers.Normalized(Vector3d.Transform(verticalDir, Matrix3x3d.RotationY(Math.PI / 2)));

            var basis = Matrix<double>.Build.Dense(3, 2);
            basis.SetColumn(0, horizontalDir.ToArray());
            basis.SetColumn(1, verticalDir.ToArray());

            var input = Vector<double>.Build.Dense(velocity.ToArray());
            var result = basis.Solve(input);
            return (result[0], result[1]);
        }

        /// <summary>
        /// Returns the angle to prograde
        /// </summary>
        /// <param name="primaryBodyPosition">The position of the primary body</param>
        /// <param name="primaryBodyVelocity">The velocity of the primary body</param>
        /// <param name="objectPosition">The position of the object</param>
        public static double AngleToPrograde(Vector3d primaryBodyPosition, Vector3d primaryBodyVelocity, Vector3d objectPosition)
        {
            var progradeAngle = MathHelpers.AngleBetween(
                primaryBodyVelocity,
                objectPosition - primaryBodyPosition,
                Normal(Prograde(primaryBodyVelocity)));

            if (progradeAngle < 0)
            {
                progradeAngle += 2.0 * Math.PI;
            }

            return progradeAngle;
        }

        /// <summary>
        /// Returns the spherical coordinates for the given radius vector
        /// </summary>
        /// <param name="radius">The radius vector</param>
        /// <param name="latitude">The latitude (in radians)</param>
        /// <param name="longitude">The longitude (in radians)</param>
        public static void GetSphericalCoordinates(Vector3d radius, out double latitude, out double longitude)
        {
            latitude = Math.Acos(radius.Y / radius.Length());
            longitude = Math.Atan2(radius.Z, radius.X);
        }

        /// <summary>
        /// Computes the cartesian coordinates from the given latitide, longitude and elevation
        /// </summary>
        /// <param name="latitude">The latitude (in radians)</param>
        /// <param name="longitude">The longitude (in radians)</param>
        /// <param name="elevation">The elevation</param>
        public static Vector3d FromSphericalCoordinates(double latitude, double longitude, double elevation)
        {
            return new Vector3d(
                elevation * Math.Sin(latitude) * Math.Cos(longitude),
                elevation * Math.Cos(latitude),
                elevation * Math.Sin(latitude) * Math.Sin(longitude));
        }

        /// <summary>
        /// Computes the coordinates for the given object at the given primary body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="position">The position of the object</param>
        /// <param name="latitude">The latitude (in radians)</param>
        /// <param name="longitude">The longitude (in radians)</param>
        public static void GetCoordinates(IPhysicsObject primaryBody, Vector3d position, out double latitude, out double longitude)
        {
            GetSphericalCoordinates(
                primaryBody.GetInverseRotationalTransform() * (position - primaryBody.State.Position),
                out latitude,
                out longitude);
            latitude = Math.PI / 2.0 - latitude;
        }

        /// <summary>
        /// Computes the cartesian coordinates from the given latitide and longitude
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="latitude">The latitude (in radians)</param>
        /// <param name="longitude">The longitude (in radians)</param>
        /// <param name="elevation">The elevation, defaults to the radius of the primary body</param>
        public static Vector3d FromCoordinates(IPhysicsObject primaryBody, double latitude, double longitude, double? elevation = null)
        {
            return primaryBody.State.Position
                   + primaryBody.GetRotationalTransform()
                   * FromSphericalCoordinates(Math.PI / 2.0 - latitude, longitude, elevation ?? primaryBody.Configuration.Radius);
        }

        /// <summary>
        /// Computes the normal for a sphere at the given latitude, longitude and elevation
        /// </summary>
        /// <param name="latitude">The latitude (in radians)</param>
        /// <param name="longitude">The longitude (in radians)</param>
        /// <param name="elevation">The elevation</param>
        public static Vector3d SphereNormal(IPhysicsObject primaryBody, double latitude, double longitude, double? elevation = null)
        {
            var direction = FromCoordinates(primaryBody, latitude, longitude, elevation) - primaryBody.State.Position;
            direction.Normalize();
            return direction;
        }

        /// <summary>
        /// Returns the surface speed due to rotation of the primary body
        /// </summary>
        /// <param name="primaryBodyConfig">The configuration of the primary body</param>
        /// <param name="latitude">The latitude of the object</param>
        public static double SurfaceSpeedDueToRotation(ObjectConfig primaryBodyConfig, double latitude)
        {
            if (primaryBodyConfig.RotationalPeriod == 0)
            {
                return 0;
            }

            return ((2.0 * Math.PI * primaryBodyConfig.Radius) / primaryBodyConfig.RotationalPeriod) * Math.Cos(latitude);
        }

        /// <summary>
        /// Determines if a SOI change is likely for an object in the given orbit
        /// </summary>
        /// <param name="orbit">The orbit of the current object</param>
        /// <param name="nextSOIOrbit">The orbit of the potential next SOI</param>
        /// <param name="soi">The SOI</param>
        public static bool SOIChangeLikely(Orbit orbit, Orbit nextSOIOrbit, double soi)
        {
            return orbit.Apoapsis >= nextSOIOrbit.Periapsis;
        }
    }
}
