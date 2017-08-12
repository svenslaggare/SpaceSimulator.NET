using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a control program for executing a maneuver
    /// </summary>
    public sealed class ExecuteManeuverProgram : IRocketControlProgram
    {
        private readonly RocketObject rocketObject;
        private readonly Vector3d deltaVelocity;
        private double appliedDeltaVelocity;
        private bool isDone;

        private readonly PIDController thrustThrottleController;

        /// <summary>
        /// Returns the thrust direction
        /// </summary>
        public Vector3d ThrustDirection { get; private set; }

        /// <summary>
        /// Creates a new maneuver program
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="deltaVelocity">The maneuver</param>
        public ExecuteManeuverProgram(RocketObject rocketObject, Vector3d deltaVelocity)
        {
            this.rocketObject = rocketObject;
            this.deltaVelocity = deltaVelocity;
            this.isDone = false;
            this.appliedDeltaVelocity = 0.0;
            this.thrustThrottleController = new PIDController(0.01, 0.005, 0.0);
        }

        /// <summary>
        /// Indicates if the program is completed
        /// </summary>
        public bool Completed => this.isDone;

        public void Start(double totalTime)
        {
            this.rocketObject.StartEngine();
            this.rocketObject.EngineThrottle = 0.2;
            Console.WriteLine("Engine start.");
        }

        public void Update(double totalTime, double timeStep)
        {
            if (!this.isDone)
            {
                this.ThrustDirection = this.deltaVelocity.Normalized();
                this.appliedDeltaVelocity += this.rocketObject.EngineAcceleration().Length() * timeStep;
                this.rocketObject.EngineThrottle = this.thrustThrottleController.ComputeCommand(
                    this.deltaVelocity.Length() - this.appliedDeltaVelocity, 
                    timeStep);

                var minThrottle = 0.005;
                if (this.rocketObject.EngineThrottle < minThrottle)
                {
                    this.rocketObject.EngineThrottle = minThrottle;
                }

                if (this.appliedDeltaVelocity >= this.deltaVelocity.Length())
                {
                    this.isDone = true;
                    this.rocketObject.StopEngine();
                    Console.WriteLine("Engine shutdown.");
                    this.rocketObject.EngineThrottle = 1.0;
                }
            }
        }
    }
}
