﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Represents a numeric integrator using the Runge-Kutta method of order 4
    /// </summary>
    public class RungeKutta4Integrator : INumericIntegrator
    {
        private static DerivativeState zero = new DerivativeState();

        /// <summary>
        /// Holds the state for the derivatives
        /// </summary>
        private struct DerivativeState
        {
            /// <summary>
            /// The velocity
            /// </summary>
            public Vector3d Velocity { get; set; }

            /// <summary>
            /// The acceleration
            /// </summary>
            public Vector3d Acceleration { get; set; }

            /// <summary>
            /// Creates a new derivative state
            /// </summary>
            /// <param name="velocity">The velocity</param>
            /// <param name="acceleration">The acceleration</param>
            public DerivativeState(Vector3d velocity, Vector3d acceleration)
            {
                this.Velocity = velocity;
                this.Acceleration = acceleration;
            }
        }

        /// <summary>
        /// Evaluates the state at the given time
        /// </summary>
        private DerivativeState Evaluate(
            IPrimaryBodyObject primaryBody,
            ref ObjectState initial,
            double totalTime,
            double deltaTime,
            ref DerivativeState derivative,
            CalculateAcceleration calculateAcceleration)
        {
            var state = new ObjectState(
                initial.Time + deltaTime,
                initial.Position + derivative.Velocity * deltaTime,
                initial.Velocity + derivative.Acceleration * deltaTime,
                derivative.Acceleration);

            var acceleration = calculateAcceleration(totalTime, ref state);
            return new DerivativeState(state.Velocity, acceleration);
        }

        /// <summary>
        /// Solves the given integration problem
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="totalTime">The total time</param>
        /// <param name="deltaTime">The delta time</param>
        /// <param name="calculateAcceleration">Calculates the acceleration</param>
        public ObjectState Solve(
            IPrimaryBodyObject primaryBody,
            IPhysicsObject physicsObject,
            ref ObjectState state,
            double totalTime,
            double deltaTime, 
            CalculateAcceleration calculateAcceleration)
        {
            if (state.HasImpacted)
            {
                return SolverHelpers.MoveImpactedObject(
                    primaryBody,
                    primaryBody.State,
                    primaryBody.State,
                    state,
                    deltaTime);
            }

            var k0 = this.Evaluate(primaryBody, ref state, totalTime, 0, ref zero, calculateAcceleration);
            var k1 = this.Evaluate(primaryBody, ref state, totalTime + deltaTime * 0.5, deltaTime * 0.5, ref k0, calculateAcceleration);
            var k2 = this.Evaluate(primaryBody, ref state, totalTime + deltaTime * 0.5, deltaTime * 0.5, ref k1, calculateAcceleration);
            var k3 = this.Evaluate(primaryBody, ref state, totalTime + deltaTime, deltaTime, ref k2, calculateAcceleration);

            var velocity = (1.0 / 6.0) * (k0.Velocity + 2 * (k1.Velocity + k2.Velocity) + k3.Velocity);
            var acceleration = (1.0 / 6.0) * (k0.Acceleration + 2 * (k1.Acceleration + k2.Acceleration) + k3.Acceleration);

            state.Time += deltaTime;
            state.Position += velocity * deltaTime;
            state.Velocity += acceleration * deltaTime;
            state.Acceleration = acceleration;
            state.Rotation = SolverHelpers.CalculateRotation(physicsObject.RotationalPeriod, state.Rotation, deltaTime);
            return state;
        }
    }
}
