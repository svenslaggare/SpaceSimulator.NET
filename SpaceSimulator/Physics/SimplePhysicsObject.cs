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
    /// Provides a simple implementation of the <see cref="IPhysicsObject"/> interface.
    /// </summary>
    public class SimplePhysicsObject : IPhysicsObject
    {
        private ObjectState state;

        /// <summary>
        /// The mass
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// The moment-of-inertia
        /// </summary>
        public double MomentOfInertia { get; }

        /// <summary>
        /// The rotational period
        /// </summary>
        public double RotationalPeriod { get; }

        /// <summary>
        /// The axis-of-rotation
        /// </summary>
        public Vector3d AxisOfRotation { get; }

        /// <summary>
        /// Returns the primary body
        /// </summary>
        public IPrimaryBodyObject PrimaryBody { get; }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="momentOfInertia">The moment of inertia</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="initialState">The initial state</param>
        public SimplePhysicsObject(
            IPrimaryBodyObject primaryBody,
            double mass, 
            double momentOfInertia,
            double rotationalPeriod, 
            Vector3d axisOfRotation, 
            ObjectState initialState)
        {
            this.Mass = mass;
            this.MomentOfInertia = momentOfInertia;
            this.RotationalPeriod = rotationalPeriod;
            this.AxisOfRotation = axisOfRotation;
            this.PrimaryBody = primaryBody;
            this.state = initialState;
        }

        /// <summary>
        /// Returns the state of the object
        /// </summary>
        public ObjectState State   
        {
            get { return this.state; }
        }

        /// <summary>
        /// Returns the standard gravitational parameter
        /// </summary>
        public double StandardGravitationalParameter
        {
            get
            {
                return this.Mass * Constants.G;
            }
        }

        /// <summary>
        /// Indicates if the object is the object of reference
        /// </summary>
        public bool IsObjectOfReference
        {
            get { return this.PrimaryBody == null; }
        }

        /// <summary>
        /// Indicates if the current object intersects the given primary body at the given position
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyPosition">The position of the primary</param>
        /// <param name="position">The current position of the object</param>
        public bool Intersects(IPrimaryBodyObject primaryBody, Vector3d primaryBodyPosition, Vector3d position)
        {
            return CollisionHelpers.SphereIntersection(primaryBodyPosition, primaryBody.Radius, position, 10);
        }
    }
}
