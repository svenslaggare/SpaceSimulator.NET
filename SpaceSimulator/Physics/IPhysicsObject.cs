using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Represents an object affected by physics
    /// </summary>
    public interface IPhysicsObject
    {
        /// <summary>
        /// The mass
        /// </summary>
        double Mass { get; }

        /// <summary>
        /// The rotational period (time to complete one rotation around its axis) of the object
        /// </summary>
        double RotationalPeriod { get; }

        /// <summary>
        /// The axis-of-rotation
        /// </summary>
        Vector3d AxisOfRotation { get; }

        /// <summary>
        /// Returns the standard gravitational parameter
        /// </summary>
        double StandardGravitationalParameter { get; }

        /// <summary>
        /// Returns the state of the object
        /// </summary>
        ObjectState State { get; }

        /// <summary>
        /// The primary body
        /// </summary>
        IPrimaryBodyObject PrimaryBody { get; }

        /// <summary>
        /// Indicates if the object is the object of reference
        /// </summary>
        bool IsObjectOfReference { get; }
    }

    /// <summary>
    /// Represents an object affected by physics that can be a primary body
    /// </summary>
    public interface IPrimaryBodyObject : IPhysicsObject
    {
        /// <summary>
        /// Returns the radius of the object
        /// </summary>
        double Radius { get; }
    }

    /// <summary>
    /// Extension methods for the <see cref="IPhysicsObject"/> interface
    /// </summary>
    public static class IPhysicsObjectExtensions
    {
        /// <summary>
        /// Returns the rotational transform for the object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public static Matrix3x3d GetRotationalTransform(this IPhysicsObject physicsObject)
        {
            return Matrix3x3d.RotationAxis(physicsObject.AxisOfRotation, physicsObject.State.Rotation);
        }

        /// <summary>
        /// Returns the inverse rotational transform for the object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public static Matrix3x3d GetInverseRotationalTransform(this IPhysicsObject physicsObject)
        {
             return Matrix3x3d.RotationAxis(physicsObject.AxisOfRotation, -physicsObject.State.Rotation);
        }

        /// <summary>
        /// Returns the rotational speed in radians/second
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public static double RotationalSpeed(this IPhysicsObject physicsObject)
        {
            return (2.0 * Math.PI) / physicsObject.RotationalPeriod;
        }
    }
}
