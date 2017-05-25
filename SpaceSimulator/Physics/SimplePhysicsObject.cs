using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="initialState">The initial state</param>
        public SimplePhysicsObject(IPrimaryBodyObject primaryBody, double mass, double rotationalPeriod, Vector3d axisOfRotation, ObjectState initialState)
        {
            this.Mass = mass;
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
    }
}
