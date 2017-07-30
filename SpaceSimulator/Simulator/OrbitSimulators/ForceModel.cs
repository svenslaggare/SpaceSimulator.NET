using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

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
        /// <param name="state">The state of the object</param>
        /// <param name="timeStep">The time step</param>
        Vector3d CalculateAcceleration(PhysicsObject physicsObject, ref ObjectState state, double timeStep);
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
        /// <param name="state">The state</param>
        /// <param name="timeStep">The time step</param>
        private Vector3d CalculateNonGravityAcceleration(ArtificialPhysicsObject artificialObject, ref ObjectState state, double timeStep)
        {
            var nonGravityAcceleration = Vector3d.Zero;
            var primaryBodyState = new ObjectState();

            if (artificialObject is RocketObject rocketObject && rocketObject.IsEngineRunning)
            {
                nonGravityAcceleration += rocketObject.EngineAcceleration();
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
        /// Calculates the acceleration applied to the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="timeStep">The time step</param>
        public Vector3d CalculateAcceleration(PhysicsObject physicsObject, ref ObjectState state, double timeStep)
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
    }
}
