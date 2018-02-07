using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// The acceleration state
    /// </summary>
    public struct AccelerationState
    {
        /// <summary>
        /// The acceleration
        /// </summary>
        public Vector3d Acceleration { get; set; }

        /// <summary>
        /// The torque
        /// </summary>
        public Vector3d Torque { get; set; }

        /// <summary>
        /// The change in mass of the object
        /// </summary>
        public double DeltaMass { get; set; }

        /// <summary>
        /// Creates a new acceleration state
        /// </summary>
        /// <param name="acceleration">The acceleration</param>
        /// <param name="torque">The torque</param>
        /// <param name="deltaMass">The change in mass of the object</param>
        public AccelerationState(Vector3d acceleration, Vector3d torque, double deltaMass)
        {
            this.Acceleration = acceleration;
            this.Torque = torque;
            this.DeltaMass = deltaMass;
        }
    }

    /// <summary>
    /// The state of the integrator
    /// </summary>
    public struct IntegratorState
    {
        /// <summary>
        /// The mass of the object
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The total time
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// The time step
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Creates a new integrator state
        /// </summary>
        /// <param name="mass">The mass of the object</param>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        public IntegratorState(double mass, double totalTime, double timeStep)
        {
            this.Mass = mass;
            this.TotalTime = totalTime;
            this.TimeStep = timeStep;
        }
    }

    /// <summary>
    /// Calculates the acceleration at the given state
    /// </summary>
    /// <param name="integratorState">The state of the integrator</param>
    /// <param name="state">The state of the object</param>
    public delegate AccelerationState CalculateAcceleration(ref IntegratorState integratorState, ref ObjectState state);

    /// <summary>
    /// Represents a numeric integrator
    /// </summary>
    public interface INumericIntegrator
    {
        /// <summary>
        /// Solves the given integration problem
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="totalTime">The total time</param>
        /// <param name="deltaTime">The delta time</param>
        /// <param name="calculateAcceleration">Calculates the acceleration</param>
       ObjectState Solve(
            IPrimaryBodyObject primaryBody,
            IPhysicsObject physicsObject,
            ref ObjectState state,
            double totalTime,
            double deltaTime, 
            CalculateAcceleration calculateAcceleration);
    }
}
