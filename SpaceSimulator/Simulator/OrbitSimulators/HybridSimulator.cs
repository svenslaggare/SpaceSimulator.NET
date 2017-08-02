using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// Represents a hybrid simulator
    /// </summary>
    public sealed class HybridSimulator : IOrbitSimulator
    {
        private readonly IOrbitSimulator integratorSimulator;
        private readonly IOrbitSimulator keplerSolverSimulator;
        private IOrbitSimulator currentSimulator;

        /// <summary>
        /// Creates a new hybrid solver
        /// </summary>
        /// <param name="integratorSimulator">The integrator simulator</param>
        /// <param name="keplerSolverSimulator">The kepler solver simulator</param>
        public HybridSimulator(IOrbitSimulator integratorSimulator, IOrbitSimulator keplerSolverSimulator)
        {
            this.integratorSimulator = integratorSimulator;
            this.keplerSolverSimulator = keplerSolverSimulator;
            this.currentSimulator = integratorSimulator;
        }

        /// <summary>
        /// Defines how the simulator handles time
        /// </summary>
        public SimulatorTimeMode TimeMode => this.currentSimulator.TimeMode;

        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="addObject">A function to add a new object</param>
        public void Update(double totalTime, double timeStep, PhysicsObject currentObject, Action<PhysicsObject> addObject = null)
        {
            this.currentSimulator.Update(totalTime, timeStep, currentObject, addObject);
        }

        /// <summary>
        /// Updates the internal mode of the simulator
        /// </summary>
        /// <param name="objects">The physics objects</param>
        public void UpdateMode(IList<PhysicsObject> objects)
        {
            var allGravity = true;
            foreach (var currentObject in objects)
            {
                if (currentObject is ArtificialPhysicsObject artificialPhysicsObject
                    && currentObject.PrimaryBody is PlanetObject planet)
                {
                    var insideAtmosphere = planet.InsideAtmosphere(artificialPhysicsObject);
                    if (artificialPhysicsObject is RocketObject rocketObject)
                    {
                        if (rocketObject.IsEngineRunning || !rocketObject.IsIdle)
                        {
                            allGravity = false;
                            break;
                        }
                        else if (insideAtmosphere && !artificialPhysicsObject.HasImpacted)
                        {
                            allGravity = false;
                            break;
                        }
                    }
                    else if (insideAtmosphere && !artificialPhysicsObject.HasImpacted)
                    {
                        allGravity = false;
                        break;
                    }

                    //if (currentObject.PrimaryBody is PlanetObject planet && insideAtmosphere)
                    //{
                    //    allGravity = false;
                    //    break;
                    //}
                    //else if (artificialPhysicsObject is RocketObject rocketObject
                    //    && (rocketObject.IsEngineRunning || !rocketObject.IsIdle))
                    //{
                    //    allGravity = false;
                    //    break;
                    //}
                }
            }

            if (allGravity)
            {
                this.currentSimulator = this.keplerSolverSimulator;
            }
            else
            {
                this.currentSimulator = this.integratorSimulator;
            }
        }

        public override string ToString()
        {
            return $"Hybrid [{this.currentSimulator.ToString()}]";
        }
    }
}
