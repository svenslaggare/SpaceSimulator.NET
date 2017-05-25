using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Represents a solver for the Keper problem
    /// </summary>
    public interface IKeplerProblemSolver
    {
        /// <summary>
        /// Returns the state at the given time
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="primaryBodyStateAtTime">The primary body state at the time</param>
        /// <param name="time">The time since the initial state/orbit</param>
        ObjectState Solve(
            IPhysicsObject physicsObject,
            ref ObjectState initialPrimaryBodyState,
            ref ObjectState initialState,
            Orbit initialOrbit,
            ref ObjectState primaryBodyStateAtTime,
            double time);
    }

    /// <summary>
    /// Contains extensions for IKeplerProblemSolver
    /// </summary>
    public static class IKeplerProblemSolverExtensions
    {
        /// <summary>
        /// Returns the state at the given time
        /// </summary>
        /// <param name="solver">The solver</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="time">The time since the initial state/orbit</param>
        public static ObjectState Solve(
            this IKeplerProblemSolver solver,
            IPhysicsObject physicsObject,
            ref ObjectState initialPrimaryBodyState,
            ref ObjectState initialState,
            Orbit initialOrbit,
            double time)
        {
            var primaryBodyStateAtTime = initialOrbit.PrimaryBody.State;
            return solver.Solve(
                physicsObject,
                ref initialPrimaryBodyState,
                ref initialState,
                initialOrbit,
                ref primaryBodyStateAtTime,
                time);
        }

        /// <summary>
        /// Returns the state at the given time
        /// </summary>
        /// <param name="solver">The solver</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="primaryBodyStateAtTime">The primary body state at the time</param>
        /// <param name="time">The time since the initial state/orbit</param>
        /// 
        public static ObjectState Solve(
            this IKeplerProblemSolver solver,
            IPhysicsObject physicsObject,
            ObjectState initialPrimaryBodyState,
            ObjectState initialState,
            Orbit initialOrbit,
            ObjectState primaryBodyStateAtTime,
            double time)
        {
            return solver.Solve(
                physicsObject,
                ref initialPrimaryBodyState,
                ref initialState,
                initialOrbit,
                ref primaryBodyStateAtTime,
                time);
        }

        /// <summary>
        /// Returns the state at the given time
        /// </summary>
        /// <param name="solver">The solver</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="time">The time since the initial state/orbit</param>
        public static ObjectState Solve(
            this IKeplerProblemSolver solver,
            IPhysicsObject physicsObject,
            ObjectState initialPrimaryBodyState,
            ObjectState initialState,
            Orbit initialOrbit,
            double time)
        {
            var primaryBodyStateAtTime = initialOrbit.PrimaryBody.State;
            return solver.Solve(
                physicsObject,
                ref initialPrimaryBodyState,
                ref initialState,
                initialOrbit,
                ref primaryBodyStateAtTime,
                time);
        }
    }
}
