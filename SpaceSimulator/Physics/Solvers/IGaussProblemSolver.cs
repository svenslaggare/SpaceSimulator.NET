using System;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// The result from a Gauss problem solver
    /// </summary>
    public struct GaussProblemResult
    {
        /// <summary>
        /// The first velocity
        /// </summary>
        public Vector3d Velocity1 { get; private set; }

        /// <summary>
        /// The second velocity
        /// </summary>
        public Vector3d Velocity2 { get; private set; }

        /// <summary>
        /// Creates a new result
        /// </summary>
        /// <param name="velocity1">The first velocity</param>
        /// <param name="velocity2">The second velocity</param>
        public GaussProblemResult(Vector3d velocity1, Vector3d velocity2)
        {
            this.Velocity1 = velocity1;
            this.Velocity2 = velocity2;
        }
    }

    /// <summary>
    /// Represents a solver for the Gauss problem
    /// </summary>
    public interface IGaussProblemSolver
    {
        /// <summary>
        /// Solves for the velocites of the given positions and time-of-flight
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyState1">The state of the primary body at the first position</param>
        /// <param name="primaryBodyState2">The state of the primary body at the second position</param>
        /// <param name="position1">The first position</param>
        /// <param name="position2">The second position</param>
        /// <param name="time">The time between the positions</param>
        /// <remarks>The time between the positions should be large enough so that they don't appear to be on a line.</remarks>
        /// <param name="shortWay">Indicates if the short way is taken</param>
        GaussProblemResult Solve(
            IPhysicsObject primaryBody,
            ObjectState primaryBodyState1,
            ObjectState primaryBodyState2,
            Vector3d position1,
            Vector3d position2,
            double time,
            bool shortWay = true);
    }
}