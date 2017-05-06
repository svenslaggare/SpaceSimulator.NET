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
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="state">The state</param>
        /// <param name="timeStep">The time step</param>
        private Vector3d CalculateNonGravityAcceleration(RocketObject rocketObject, ref ObjectState state, double timeStep)
        {
            var nonGravityAcceleration = Vector3d.Zero;
            var primaryBodyState = rocketObject.PrimaryBody.State;

            if (rocketObject.IsEngineRunning)
            {
                nonGravityAcceleration += rocketObject.EngineAcceleration();
                //if (rocketObject.IsEngineRunning)
                //{
                //    var currentStage = rocketObject.Stages.CurrentStage;
                //    var deltaV = currentStage.EffectiveExhaustVelocity * Math.Log(rocketObject.Mass / (rocketObject.Mass - timeStep * currentStage.MassFlowRate));
                //    var engineAcceleration = currentStage.NumberOfEngines * (deltaV / timeStep);
                //    nonGravityAcceleration += MathHelpers.Normalized(rocketObject.EngineAcceleration()) * engineAcceleration;
                //}
            }

            if (rocketObject.PrimaryBody is PlanetObject primaryPlanet)
            {
                nonGravityAcceleration += primaryPlanet.AtmosphericModel.CalculateDrag(
                    primaryPlanet.Configuration,
                    ref primaryBodyState,
                    rocketObject.AtmosphericProperties,
                    ref state) / rocketObject.Mass;
            }

            return nonGravityAcceleration;
        }

        /// <summary>
        /// Calculates the sum of the acceleration applied to the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="objects">The objects</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="primary">Indicates if this is the primary acceleration</param>
        private Vector3d CalculateAcceleration(PhysicsObject physicsObject, ref ObjectState state, IList<PhysicsObject> objects, double timeStep, bool primary)
        {
            //var totalAcceleration = Vector3d.Zero;
            //foreach (var object2 in objects)
            //{
            //    if (physicsObject != object2 && object2.Type != PhysicsObjectType.ArtificialSatellite)
            //    {
            //        totalAcceleration += Formulas.GravityAcceleration(
            //            object2.StandardGravitationalParameter,
            //            state.Position - object2.Position);
            //    }
            //}
            //return totalAcceleration;

            //return Formulas.GravityAcceleration(
            //    physicsObject.PrimaryBody.StandardGravitationalParameter,
            //    state.Position - physicsObject.PrimaryBody.Position);
            if (physicsObject.PrimaryBody == null)
            {
                return Vector3d.Zero;
            }

            var nonGravityAcceleration = Vector3d.Zero;
            var primaryBodyState = physicsObject.PrimaryBody.State;

            if (primary && physicsObject is RocketObject rocketObject)
            {
                nonGravityAcceleration = this.CalculateNonGravityAcceleration(rocketObject, ref state, timeStep);
            }

            return OrbitFormulas.GravityAcceleration(
                physicsObject.PrimaryBody.StandardGravitationalParameter,
                state.Position - physicsObject.PrimaryBody.State.Position)
                + nonGravityAcceleration
                + CalculateAcceleration(physicsObject.PrimaryBody, ref primaryBodyState, objects, timeStep, false);
        }

        /// <summary>
        /// Updates the simulator one step
        /// </summary>
        /// <param name="totalTime">The total simulated time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="currentObject">The current object</param>
        /// <param name="otherObjects">The other objects</param>
        /// <param name="addObject">A function to add a new object</param>
        public void Update(double totalTime, double timeStep, PhysicsObject currentObject, IList<PhysicsObject> otherObjects, Action<PhysicsObject> addObject)
        {
            var currentState = currentObject.State;
            var nextState = this.numericIntegrator.Solve(
                currentObject.PrimaryBody,
                currentObject.Configuration,
                ref currentState,
                totalTime,
                timeStep,
                (double t, ref ObjectState state) => this.CalculateAcceleration(currentObject, ref state, otherObjects, timeStep, true));

            if (currentObject is RocketObject rocketObject)
            {
                if (rocketObject.IsEngineRunning)
                {
                    rocketObject.AfterImpulse(timeStep);
                }

                rocketObject.ClearStagedObjects(addObject);
            }

            currentObject.SetNextState(nextState);
        }
    }
}
