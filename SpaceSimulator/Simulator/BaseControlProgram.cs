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
        private readonly PointInDirectionController pointInDirectionController;

        /// <summary>
        /// Creates a new base control program
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        public BaseControlProgram(RocketObject rocketObject)
        {
            this.rocketObject = rocketObject;
            this.pointInDirectionController = new PointInDirectionController(this.rocketObject);
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
        /// The none-force torque
        /// </summary>
        public Vector3d Torque { get; protected set; }

        /// <summary>
        /// Sets the orientation to face the thrust direction
        /// </summary>
        protected void SetFaceThrustDirection()
        {
            this.rocketObject.SetOrientation(MathHelpers.FaceDirectionQuaternion(this.absoluteThrustDirection));
        }

        /// <summary>
        /// Rotates to face the given direction
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="direction">The direction</param>
        protected void FaceDirection(double totalTime, double timeStep, Vector3d direction)
        {
            this.pointInDirectionController.SetDirection(Vector3d.ForwardLH);
            this.Torque = this.pointInDirectionController.Update(totalTime, timeStep);
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
