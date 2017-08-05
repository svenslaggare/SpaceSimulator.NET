using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// Defines different modes for handling time
    /// </summary>
    public enum SimulatorTimeMode
    {
        Step,
        Interval
    }

    /// <summary>
    /// Adds a new physics object
    /// </summary>
    /// <param name="parentObject">The parent object</param>
    /// <param name="newObject">The new object</param>
    public delegate void AddPhysicsObject(PhysicsObject parentObject, PhysicsObject newObject);

    /// <summary>
    /// Represents an orbit simulator
    /// </summary>
    public interface IOrbitSimulator
    {
        /// <summary>
        /// Defines how the simulator handles time
        /// </summary>
        SimulatorTimeMode TimeMode { get; }

        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="addObject">A function to add a new object</param>
        void Update(double totalTime, double timeStep, PhysicsObject currentObject, AddPhysicsObject addObject = null);
    }
}
