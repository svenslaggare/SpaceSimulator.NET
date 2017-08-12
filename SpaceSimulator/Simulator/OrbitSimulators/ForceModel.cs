using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Simulator.OrbitSimulators
{
    /// <summary>
    /// The force model
    /// </summary>
    public interface IForceModel
    {
        /// <summary>
        /// Calculates the acceleration applied to the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="integratorState">The state of the integrator</param>
        /// <param name="state">The state of the object</param>
        AccelerationState CalculateAcceleration(PhysicsObject physicsObject, ref IntegratorState integratorState, ref ObjectState state);
    }

    /// <summary>
    /// The default force model
    /// </summary>
    public class DefaultForceModel : IForceModel
    {
        /// <summary>
        /// Calculates the non-gravity based acceleration of the given object
        /// </summary>
        /// <param name="artificialObject">The physics object</param>
        /// <param name="integratorState">The state of the integrator</param>
        /// <param name="state">The state</param>
        private (Vector3d, double) CalculateNonGravityAcceleration(ArtificialPhysicsObject artificialObject, ref IntegratorState integratorState, ref ObjectState state)
        {
            var acceleration = Vector3d.Zero;
            var deltaMass = 0.0;
            var primaryBodyState = new ObjectState();

            var mass = integratorState.Mass;
            if (artificialObject is RocketObject rocketObject && rocketObject.IsEngineRunning)
            {
                acceleration += rocketObject.EngineThrust() / mass;
                deltaMass = -rocketObject.Stages.CurrentStage.TotalMassFlowRate * integratorState.TimeStep;
            }

            if (artificialObject.PrimaryBody is PlanetObject primaryPlanet
                && primaryPlanet.AtmosphericModel.Inside(primaryPlanet, ref primaryBodyState, ref state))
            {
                acceleration += primaryPlanet.AtmosphericModel.CalculateDrag(
                    primaryPlanet,
                    ref primaryBodyState,
                    artificialObject.AtmosphericProperties,
                    ref state) / mass;
            }

            return (acceleration, deltaMass);
        }

        /// <summary>
        /// Calculates the acceleration applied to the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="integratorState">The state of the integrator</param>
        public AccelerationState CalculateAcceleration(PhysicsObject physicsObject, ref IntegratorState integratorState, ref ObjectState state)
        {
            if (physicsObject.PrimaryBody == null)
            {
                return new AccelerationState(Vector3d.Zero, 0);
            }

            var nonGravityAcceleration = Vector3d.Zero;
            var deltaMass = 0.0;
            if (physicsObject is ArtificialPhysicsObject artificialPhysicsObject)
            {
                (nonGravityAcceleration, deltaMass) = this.CalculateNonGravityAcceleration(artificialPhysicsObject, ref integratorState, ref state);
            }

            var gravityAcceleration = OrbitFormulas.GravityAcceleration(
                physicsObject.PrimaryBody.StandardGravitationalParameter,
                state.Position);
            return new AccelerationState(gravityAcceleration + nonGravityAcceleration, deltaMass);
        }
    }
}
