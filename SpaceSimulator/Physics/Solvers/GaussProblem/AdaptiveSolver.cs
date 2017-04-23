using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Picks the best method to solve a given instance of the gauss problem
    /// </summary>
    public sealed class GaussProblemAdaptiveSolver : IGaussProblemSolver
    {
        private readonly IGaussProblemSolver universalVariableSolver = new GaussProblemUniversalVariableSolver();
        private readonly IGaussProblemSolver pMethodSolver = new GaussProblemPMethodSolver();

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
        public GaussProblemResult Solve(
            IPhysicsObject primaryBody,
            ObjectState primaryBodyState1,
            ObjectState primaryBodyState2,
            Vector3d position1,
            Vector3d position2,
            double time,
            bool shortWay = true)
        {
            try
            {
                return this.universalVariableSolver.Solve(
                    primaryBody,
                    primaryBodyState1,
                    primaryBodyState2,
                    position1,
                    position2,
                    time,
                    shortWay);
            }
            catch
            {
                return this.pMethodSolver.Solve(
                    primaryBody,
                    primaryBodyState1,
                    primaryBodyState2,
                    position1,
                    position2,
                    time,
                    shortWay);
            }
        }
    }
}
