using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// Represents an orbit simulator
    /// </summary>
    public interface IOrbitSimulator
    {
        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="addObject">A function to add a new object</param>
        void Update(double totalTime, double timeStep, PhysicsObject currentObject, Action<PhysicsObject> addObject = null);
    }
}
