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
    /// Represents a satellite physics object
    /// </summary>
    public class SatelliteObject : ArtificialPhysicsObject
    {
        /// <summary>
        /// Returns the atmospheric properties
        /// </summary>
        public override AtmosphericProperties AtmosphericProperties { get; }

        /// <summary>
        /// Creates a new satellite object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="config">The configuration for the object</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public SatelliteObject(
            string name,
            ObjectConfig config,
            AtmosphericProperties atmosphericProperties,
            PhysicsObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
            : base(name, config, primaryBody, initialState, initialOrbit)
        {
            this.AtmosphericProperties = atmosphericProperties;
        }
    }
}
