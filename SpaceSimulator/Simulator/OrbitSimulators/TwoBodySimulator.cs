using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// Represents a simulator using a two-body problem solver
    /// </summary>
    public sealed class TwoBodySimulator : IOrbitSimulator
    {
        private readonly IKeplerProblemSolver keplerProblemSolver;

        /// <summary>
        /// Creates a new simulator
        /// </summary>
        /// <param name="keplerProblemSolver">The kepler problem solver</param>
        public TwoBodySimulator(IKeplerProblemSolver keplerProblemSolver)
        {
            this.keplerProblemSolver = keplerProblemSolver;
        }

        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="addObject">A function to add a new object</param>
        /// 
        public void Update(double totalTime, double timeStep, PhysicsObject currentObject, Action<PhysicsObject> addObject)
        {
            var initialPrimaryState = currentObject.ReferencePrimaryBodyState;
            var initialState = currentObject.ReferenceState;
            var initialOrbit = currentObject.ReferenceOrbit;
            var nextState = this.keplerProblemSolver.Solve(
                currentObject,
                ref initialPrimaryState,
                ref initialState,
                initialOrbit,
                (totalTime + timeStep) - currentObject.ReferenceState.Time);

            currentObject.SetNextState(nextState);
        }
    }
}
