using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a control program for a rocket
    /// </summary>
    public interface IRocketControlProgram
    {
        /// <summary>
        /// Indicates if the program is complete
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// The thrust direction
        /// </summary>
        Vector3d ThrustDirection { get; }

        /// <summary>
        /// The none-force torque
        /// </summary>
        Vector3d Torque { get; }

        /// <summary>
        /// Starts the program
        /// </summary>
        /// <param name="totalTime">The total time</param>
        void Start(double totalTime);

        /// <summary>
        /// Updates the program
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        void Update(double totalTime, double timeStep);
    }
}
