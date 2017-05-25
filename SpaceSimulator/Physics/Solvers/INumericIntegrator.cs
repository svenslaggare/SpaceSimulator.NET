using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Calculates the acceleration at the given state
    /// </summary>
    /// <param name="totalTime">The time of the state</param>
    /// <param name="state">The state</param>
    public delegate Vector3d CalculateAcceleration(double totalTime, ref ObjectState state);

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
