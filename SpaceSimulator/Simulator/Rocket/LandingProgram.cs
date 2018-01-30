using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a control program for landing a rocket
    /// </summary>
    public sealed class LandingProgram : BaseControlProgram
    {
        private readonly ITextOutputWriter textOutputWriter;

        private State state;

        private enum State
        {
            BoostbackBurn,
            WaitingForLandingBurn,
            LandingBurn,
            Landed
        }

        /// <summary>
        /// Creates a new landing program for the given rocket
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="textOutputWriter">The text output writer</param>
        public LandingProgram(RocketObject rocketObject, ITextOutputWriter textOutputWriter)
            : base(rocketObject)
        {
            this.textOutputWriter = textOutputWriter;
        }

        /// <summary>
        /// Indicates if the program is completed
        /// </summary>
        public override bool IsCompleted => this.state == State.Landed;

        public override void Start(double totalTime)
        {
            this.rocketObject.StartEngines();
            this.state = State.BoostbackBurn;
            this.LogStatus("Boostback burn started.");
        }

        /// <summary>
        /// Logs the given message
        /// </summary>
        /// <param name="message">The message</param>
        private void LogStatus(string message)
        {
            this.textOutputWriter.WriteLine(this.rocketObject.Name, message);
        }

        public override void Update(double totalTime, double timeStep)
        {
            var state = this.rocketObject.State;
            state.MakeRelative(this.rocketObject.PrimaryBody.State);

            var gravityAccelerationDirection = -state.Position.Normalized();
            (var horizontalSpeed, var verticalSpeed) = OrbitHelpers.ComputeHorizontalAndVerticalVelocity(gravityAccelerationDirection, state.Velocity);
            var altitude = this.rocketObject.PrimaryBody.Altitude(this.rocketObject.Position);

            switch (this.state)
            {
                case State.BoostbackBurn:
                    if (Math.Abs(horizontalSpeed) >= 20.0)
                    {
                        this.absoluteThrustDirection = state.Retrograde;
                        //(var launchLatitude, var launchLongitude) = this.rocketObject.LaunchCoordinates.Value;
                        //this.ThrustDirection = (
                        //    OrbitHelpers.FromCoordinates(this.rocketObject.PrimaryBody, launchLatitude, launchLongitude) 
                        //    - this.rocketObject.Position).Normalized();
                    }
                    else
                    {
                        this.rocketObject.StopEngines();
                        this.state = State.WaitingForLandingBurn;
                        this.LogStatus("Boostback burn completed.");
                    }
                    break;
                case State.WaitingForLandingBurn:
                    if (altitude <= 560)
                    {
                        this.state = State.LandingBurn;
                        this.rocketObject.StartEngines();
                        this.LogStatus("Landing burn started.");
                    }
                    break;
                case State.LandingBurn:
                    this.absoluteThrustDirection = OrbitHelpers.SphereNormal(
                        this.rocketObject.PrimaryBody,
                        this.rocketObject.Latitude, 
                        this.rocketObject.Longitude);

                    if (verticalSpeed > 0)
                    {
                        this.rocketObject.StopEngines();
                        this.LogStatus("Killing engines.");
                    }

                    //if (altitude <= 20.0)
                    //{
                    //    this.LogStatus(state.Velocity.Length().ToString());
                    //}

                    if (altitude <= 11.1)
                    {
                        this.state = State.Landed;
                        this.LogStatus($"Landed (alt: {altitude}, speed: {state.Velocity.Length()})");
                        this.rocketObject.StopEngines();
                    }
                    break;
                case State.Landed:
                    break;
                default:
                    break;
            }

            this.SetFaceThrustDirection();
        }
    }
}
