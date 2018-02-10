using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
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
        private (Vector3d, Vector3d, double) CalculateNonGravityAcceleration(ArtificialPhysicsObject artificialObject, ref IntegratorState integratorState, ref ObjectState state)
        {
            var acceleration = Vector3d.Zero;
            var torque = Vector3d.Zero;

            var deltaMass = 0.0;
            var primaryBodyState = new ObjectState();

            var mass = integratorState.Mass;
            if (artificialObject is RocketObject rocketObject)
            {
                if (rocketObject.IsEngineRunning)
                {
                    var engineThrust = rocketObject.EngineThrust();
                    acceleration += engineThrust / mass;
                    deltaMass = -rocketObject.CurrentStage.TotalMassFlowRate * integratorState.TimeStep;

                    var orientation = artificialObject.Orientation;
                    var forwardDirection = Vector3d.Transform(Vector3d.ForwardRH, orientation).Normalized();
                    //var relativeApplyPoint = (state.Position - 20.0 * forwardDirection) - state.Position;
                    //var relativeApplyPoint = -20.0 * forwardDirection;
                    var relativeApplyPoint = -forwardDirection;
                    torque = Vector3d.Cross(relativeApplyPoint, engineThrust);

                    //if (torque.Length() < 1E-8)
                    //{
                    //    torque = Vector3d.Zero;
                    //}

                    //if (torque.Length() != 0.0)
                    //{
                    //    Console.WriteLine(
                    //        $"F: {DataFormatter.Format(engineThrust.Length(), DataUnit.Force, useBase10: true)}, " +
                    //        $"T: {DataFormatter.Format(torque.Length(), DataUnit.Force, useBase10: true)}, " +
                    //        $"S: {Math.Sin(MathHelpers.AngleBetween(engineThrust.Normalized(), relativeApplyPoint.Normalized()))}");
                    //}
                }

                torque += rocketObject.RotationTorque();
            }

            if (artificialObject.PrimaryBody is PlanetObject primaryPlanet
                && primaryPlanet.AtmosphericModel.Inside(primaryPlanet, ref primaryBodyState, ref state))
            {
                (var dragForce, var dragTorque) = primaryPlanet.AtmosphericModel.CalculateDrag(
                    primaryPlanet,
                    ref primaryBodyState,
                    artificialObject,
                    artificialObject.AtmosphericProperties,
                    ref state);

                acceleration += dragForce / mass;
                torque += dragTorque;
            }

            return (acceleration, torque, deltaMass);
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
                return new AccelerationState(Vector3d.Zero, Vector3d.Zero, 0);
            }

            var nonGravityAcceleration = Vector3d.Zero;
            var torque = Vector3d.Zero;
            var deltaMass = 0.0;
            if (physicsObject is ArtificialPhysicsObject artificialPhysicsObject)
            {
                (nonGravityAcceleration, torque, deltaMass) = this.CalculateNonGravityAcceleration(artificialPhysicsObject, ref integratorState, ref state);
            }

            var gravityAcceleration = OrbitFormulas.GravityAcceleration(
                physicsObject.PrimaryBody.StandardGravitationalParameter,
                state.Position);

            return new AccelerationState(
                gravityAcceleration + nonGravityAcceleration,
                torque,
                deltaMass);
        }
    }
}
