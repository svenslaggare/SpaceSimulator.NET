using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// Represents a simulator using Cowell's method
    /// </summary>
    public sealed class CowellSimulator : IOrbitSimulator
    {
        private readonly INumericIntegrator numericIntegrator;
        private readonly IForceModel forceModel;

        /// <summary>
        /// Creates a new simulator using the given numeric integrator
        /// </summary>
        /// <param name="numericIntegrator">The numeric integrator</param>
        /// <param name="forceModel">The force model</param>
        public CowellSimulator(INumericIntegrator numericIntegrator, IForceModel forceModel)
        {
            this.numericIntegrator = numericIntegrator;
            this.forceModel = forceModel;
        }

        /// <summary>
        /// Defines how the simulator handles time
        /// </summary>
        public SimulatorTimeMode TimeMode => SimulatorTimeMode.Step;

        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="addObject">A function to add a new object</param>
        public void Update(double totalTime, double timeStep, PhysicsObject currentObject, Action<PhysicsObject> addObject)
        {
            var currentState = currentObject.State;
            currentState.MakeRelative(currentObject.PrimaryBody.State);

            var nextState = this.numericIntegrator.Solve(
                currentObject.PrimaryBody,
                currentObject,
                ref currentState,
                totalTime,
                timeStep,
                (double t, ref ObjectState state) => this.forceModel.CalculateAcceleration(currentObject, ref state, timeStep));

            nextState.MakeAbsolute(currentObject.PrimaryBody.State);
            currentObject.SetNextState(nextState);

            if (currentObject is RocketObject rocketObject)
            {
                if (rocketObject.IsEngineRunning)
                {
                    rocketObject.AfterImpulse(timeStep);
                }

                rocketObject.ClearStagedObjects(addObject);
            }
        }

        public override string ToString()
        {
            return "Integrator";
        }
    }
}
