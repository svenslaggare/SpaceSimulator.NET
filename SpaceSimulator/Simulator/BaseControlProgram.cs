using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Base implementation for a rocket control program
    /// </summary>
    public abstract class BaseControlProgram : IRocketControlProgram
    {
        protected readonly RocketObject rocketObject;
        protected Vector3d absoluteThrustDirection;

        /// <summary>
        /// Creates a new base control program
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        public BaseControlProgram(RocketObject rocketObject)
        {
            this.rocketObject = rocketObject;
        }

        /// <summary>
        /// Indicates if the program is complete
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Returns the thrust direction
        /// </summary>
        public Vector3d ThrustDirection => RocketHelpers.AbsoluteToRelativeThrustDirection(this.rocketObject, this.absoluteThrustDirection);

        /// <summary>
        /// Sets the orientation to face the thrust direction
        /// </summary>
        protected void SetFaceThrustDirection()
        {
            this.rocketObject.SetState(this.rocketObject.State.WithOrientation(MathHelpers.FaceDirectionQuaternion(this.absoluteThrustDirection)));
        }

        /// <summary>
        /// Starts the program
        /// </summary>
        /// <param name="totalTime">The total time</param>
        public abstract void Start(double totalTime);

        /// <summary>
        /// Updates the program
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        public abstract void Update(double totalTime, double timeStep);
    }
}
