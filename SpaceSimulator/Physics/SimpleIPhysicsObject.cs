using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Provides a simple implementation of the <see cref="IPhysicsObject"/> interface
    /// </summary>
    public class SimplePhysicsObject : IPhysicsObject
    {
        private ObjectState state;

        /// <summary>
        /// Returns the primary body
        /// </summary>
        public IPhysicsObject PrimaryBody { get; }

        /// <summary>
        /// Returns the configuration of the object
        /// </summary>
        public ObjectConfig Configuration { get; }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="initialState">The initial state</param>
        public SimplePhysicsObject(IPhysicsObject primaryBody, ObjectConfig configuration, ObjectState initialState)
        {
            this.PrimaryBody = primaryBody;
            this.Configuration = configuration;
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
                return this.Configuration.Mass * Constants.G;
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
