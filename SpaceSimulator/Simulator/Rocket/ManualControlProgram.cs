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

        public bool IsCompleted => false;

        public Vector3d ThrustDirection => this.thrustDirection;

        public void Start(double totalTime)
        {
            //this.thrustDirection = OrbitHelpers.SphereNormal(this.rocketObject.PrimaryBody, this.rocketObject.Latitude, this.rocketObject.Longitude);
        }

        public void Update(double totalTime, double timeStep)
        {
            //this.thrustDirection = Vector3d.Transform(Vector3d.ForwardRH, this.rocketObject.Orientation);
            this.thrustDirection = Vector3d.Transform(Vector3d.ForwardRH, Quaterniond.RotationAxis(Vector3d.Right, 30.0 * MathUtild.Deg2Rad));
        }
    }
}
