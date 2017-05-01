using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents a planet physics object
    /// </summary>
    public class PlanetObject : PhysicsObject
    {
        /// <summary>
        /// The atmospheric model
        /// </summary>
        public IAtmosphericModel AtmosphericModel { get; }

        /// <summary>
        /// Creates a new planet object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="config">The configuration for the object</param>
        /// <param name="atmosphericModel">The atmospheric model</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public PlanetObject(
            string name,
            PhysicsObjectType type,
            ObjectConfig config,
            IAtmosphericModel atmosphericModel,
            PhysicsObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
            : base(name, type, config, primaryBody, initialState, initialOrbit, true)
        {
            this.AtmosphericModel = atmosphericModel;
        }
    }
}
