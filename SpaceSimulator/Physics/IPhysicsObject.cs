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
        /// Returns the state of the object
        /// </summary>
        ObjectState State
        {
            get;
        }

        /// <summary>
        /// Returns the configuration of the object
        /// </summary>
        ObjectConfig Configuration
        {
            get;
        }

        /// <summary>
        /// Returns the standard gravitational parameter
        /// </summary>
        double StandardGravitationalParameter
        {
            get;
        }

        /// <summary>
        /// The primary body
        /// </summary>
        IPhysicsObject PrimaryBody
        {
            get;
        }

        /// <summary>
        /// Indicates if the object is the object of reference
        /// </summary>
        bool IsObjectOfReference { get; }
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
            return Matrix3x3d.RotationAxis(physicsObject.Configuration.AxisOfRotation, physicsObject.State.Rotation);
        }

        /// <summary>
        /// Returns the inverse rotational transform for the object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public static Matrix3x3d GetInverseRotationalTransform(this IPhysicsObject physicsObject)
        {
             return Matrix3x3d.RotationAxis(physicsObject.Configuration.AxisOfRotation, -physicsObject.State.Rotation);
        }
    }
}
