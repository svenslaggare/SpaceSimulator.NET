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

        /// <summary>
        /// Creates a new simulator using the given numeric integrator
        /// </summary>
        /// <param name="numericIntegrator">The numeric integrator</param>
        public CowellSimulator(INumericIntegrator numericIntegrator)
        {
            this.numericIntegrator = numericIntegrator;
        }

        /// <summary>
        /// Calculates the non-gravity based acceleration of the given object
        /// </summary>
        /// <param name="artificialObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="timeStep">The time step</param>
        private Vector3d CalculateNonGravityAcceleration(ArtificialPhysicsObject artificialObject, ref ObjectState state, double timeStep)
        {
            var nonGravityAcceleration = Vector3d.Zero;
            var primaryBodyState = new ObjectState();

            if (artificialObject is RocketObject rocketObject && rocketObject.IsEngineRunning)
            {
                nonGravityAcceleration += rocketObject.EngineAcceleration();

                //var currentStage = rocketObject.Stages.CurrentStage;

                //if (currentStage.Engines.Count > 0)
                //{
                //    var currentEngine = currentStage.Engines[0];
                //    var deltaV = currentEngine.EffectiveExhaustVelocity * Math.Log(rocketObject.Mass / (rocketObject.Mass - timeStep * currentEngine.MassFlowRate));
                //    var engineAcceleration = currentStage.Engines.Count * (deltaV / timeStep);
                //    nonGravityAcceleration += MathHelpers.Normalized(rocketObject.EngineAcceleration()) * engineAcceleration;
                //}
            }

            if (artificialObject.PrimaryBody is PlanetObject primaryPlanet
                && primaryPlanet.AtmosphericModel.Inside(
                    primaryPlanet,
                    ref primaryBodyState,
                    ref state))
            {
                nonGravityAcceleration += primaryPlanet.AtmosphericModel.CalculateDrag(
                    primaryPlanet,
                    ref primaryBodyState,
                    artificialObject.AtmosphericProperties,
                    ref state) / artificialObject.Mass;
            }

            return nonGravityAcceleration;
        }

        /// <summary>
        /// Calculates the sum of the acceleration applied to the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="timeStep">The time step</param>
        private Vector3d CalculateAcceleration(PhysicsObject physicsObject, ref ObjectState state, double timeStep)
        {
            if (physicsObject.PrimaryBody == null)
            {
                return Vector3d.Zero;
            }

            var nonGravityAcceleration = Vector3d.Zero;
            if (physicsObject is ArtificialPhysicsObject artificialPhysicsObject)
            {
                nonGravityAcceleration = this.CalculateNonGravityAcceleration(artificialPhysicsObject, ref state, timeStep);
            }

            return OrbitFormulas.GravityAcceleration(
                physicsObject.PrimaryBody.StandardGravitationalParameter,
                state.Position) + nonGravityAcceleration;
        }

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
                (double t, ref ObjectState state) => this.CalculateAcceleration(currentObject, ref state, timeStep));

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
    }
}
