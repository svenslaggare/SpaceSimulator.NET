using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Simulator.Rocket
{
    public class ManualControlProgram : IRocketControlProgram
    {
        private readonly RocketObject rocketObject;
        private readonly ITextOutputWriter textOutputWriter;
        private Vector3d thrustDirection;

        private bool runEngine = false;

        /// <summary>
        /// Indicates if the program is completed
        /// </summary>
        public bool IsCompleted { get; private set; } = false;

        /// <summary>
        /// The none-force torque
        /// </summary>
        public Vector3d Torque { get; private set; }

        /// <summary>
        /// Creates a new manual control program
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="textOutputWriter">The text output writer</param>
        public ManualControlProgram(RocketObject rocketObject, ITextOutputWriter textOutputWriter)
        {
            this.rocketObject = rocketObject;
            this.textOutputWriter = textOutputWriter;
        }

        public Vector3d ThrustDirection => this.thrustDirection;

        public void Start(double totalTime)
        {
            this.runEngine = true;
        }

        public void Update(double totalTime, double timeStep)
        {
            if (this.runEngine)
            {
                var altitude = this.rocketObject.PrimaryBody.Altitude(this.rocketObject.Position);

                //var pitchAltitude = 18750.0;
                //var pitchAmount = 2000.0;

                var pitchAltitude = 1000.0;
                var pitchAmount = 2000.0;

                if (altitude >= pitchAltitude && altitude <= pitchAltitude + pitchAmount)
                {
                    this.thrustDirection = Vector3d.Transform(Vector3d.ForwardRH, Quaterniond.RotationAxis(Vector3d.Up, 0.22 * MathUtild.Deg2Rad));
                }
                else
                {
                    this.thrustDirection = Vector3d.ForwardRH;
                }

                //var thrustAngle = MathHelpers.AngleBetween(
                //    RocketHelpers.RelativeToAbsoluteThrustDirection(this.rocketObject, this.thrustDirection),
                //    this.rocketObject.State.Prograde);

                ////Console.WriteLine(thrustAngle * MathHelpers.Rad2Deg);
                //if (thrustAngle * MathHelpers.Rad2Deg >= 2.0 && altitude > 200E3)
                //{

                //}

                //if (altitude >= 200E3 && MathUtild.Rad2Deg * this.rocketObject.AngularVelocity.Length() >= 0.01)
                //if (altitude >= 50E3 
                //    && Orbit.CalculateOrbit(this.rocketObject).RelativeApoapsis >= 300E3
                //    && MathUtild.Rad2Deg * this.rocketObject.AngularVelocity.Length() >= 0.01)
                //{
                //    this.thrustDirection = Vector3d.Transform(Vector3d.ForwardRH, Quaterniond.RotationAxis(Vector3d.Up, -1.0 * MathUtild.Deg2Rad));
                //}
            }

            var orbit = Orbit.CalculateOrbit(this.rocketObject);
            if (orbit.RelativePeriapsis >= 300E3)
            {
                this.thrustDirection = Vector3d.Zero;
                this.runEngine = false;
                this.rocketObject.StopEngines();

                //this.IsCompleted = true;
                //this.Torque = -this.rocketObject.AngularVelocity.Normalized() * 100;
            }
        }
    }
}
