using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Represents the geometric shape for an object
    /// </summary>
    public abstract class GeometricShape
    {
        /// <summary>
        /// Indicates if the current shape intersect the given shape
        /// </summary>
        /// <param name="currentPosition">The position of the current shape</param>
        /// <param name="otherShape">The other shape</param>
        /// <param name="otherPosition">The position of the other shape</param>
        public abstract bool Intersect(Vector3d currentPosition, GeometricShape otherShape, Vector3d otherPosition);
    }

    /// <summary>
    /// Represents a sphere shape
    /// </summary>
    public sealed class SphereShape : GeometricShape
    {
        /// <summary>
        /// The radius of the sphere
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Creates a new sphere shape
        /// </summary>
        /// <param name="radius">The radius</param>
        public SphereShape(double radius)
        {
            this.Radius = radius;
        }

        /// <summary>
        /// Indicates if the current shape intersect the given shape
        /// </summary>
        /// <param name="currentPosition">The position of the current shape</param>
        /// <param name="otherShape">The other shape</param>
        /// <param name="otherPosition">The position of the other shape</param>
        public override bool Intersect(Vector3d currentPosition, GeometricShape otherShape, Vector3d otherPosition)
        {
            switch (otherShape)
            {
                case SphereShape circle:
                    return CollisionHelpers.SphereIntersection(
                        currentPosition,
                        this.Radius,
                        otherPosition,
                        circle.Radius);
                case CylinderShape cylinder:
                    return CollisionHelpers.SphereIntersection(
                        currentPosition,
                        this.Radius,
                        otherPosition,
                        Math.Max(cylinder.Radius, cylinder.Height / 2.0));
            }

            throw new ArgumentException("Shape not supported.");
        }
    }

    /// <summary>
    /// Represents a cylinder shape
    /// </summary>
    public sealed class CylinderShape : GeometricShape
    {
        /// <summary>
        /// The radius of the cylinder
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// The height of the cylinder
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Creates a new cylinder shape
        /// </summary>
        /// <param name="radius">The radius</param>
        /// <param name="height">The height</param>
        public CylinderShape(double radius, double height)
        {
            this.Radius = radius;
            this.Height = height;
        }

        /// <summary>
        /// Indicates if the current shape intersect the given shape
        /// </summary>
        /// <param name="currentPosition">The position of the current shape</param>
        /// <param name="otherShape">The other shape</param>
        /// <param name="otherPosition">The position of the other shape</param>
        public override bool Intersect(Vector3d currentPosition, GeometricShape otherShape, Vector3d otherPosition)
        {
            switch (otherShape)
            {
                case SphereShape circle:
                    return CollisionHelpers.SphereIntersection(
                        currentPosition,
                        Math.Max(this.Radius, this.Height / 2.0),
                        otherPosition,
                        circle.Radius);
                case CylinderShape cylinder:
                    return CollisionHelpers.SphereIntersection(
                        currentPosition,
                        Math.Max(this.Radius, this.Height / 2.0),
                        otherPosition,
                        Math.Max(cylinder.Radius, cylinder.Height / 2.0));
            }

            throw new ArgumentException("Shape not supported.");
        }
    }
}
