using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Represents a simulation engine
    /// </summary>
    public interface ISimulatorEngine
    {
        /// <summary>
        /// The total amount of simulated time
        /// </summary>
        double TotalTime { get; }

        /// <summary>
        /// Returns a solver for the kepler problem
        /// </summary>
        IKeplerProblemSolver KeplerProblemSolver { get; }

        /// <summary>
        /// Returns a solver for the gauss problem
        /// </summary>
        IGaussProblemSolver GaussProblemSolver { get; }
    }
}
