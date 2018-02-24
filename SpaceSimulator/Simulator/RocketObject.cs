using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Rocket;
using SpaceSimulator.Simulator.OrbitSimulators;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents a physics object with a rocket engine
    /// </summary>
    public class RocketObject : ArtificialPhysicsObject
    {
        private readonly RocketStages rocketStages;

        private bool engineRunning;
        private IRocketControlProgram controlProgram;
        private bool updateOrbit = false;

        private readonly IList<PhysicsObject> toStage = new List<PhysicsObject>();

        /// <summary>
        /// The launch coordinates
        /// </summary>
        public (double, double)? LaunchCoordinates { get; private set; }

        private readonly ITextOutputWriter textOutputWriter;

    /// <summary>
    /// Creates a new rocket object
    /// </summary>
    /// <param name="name">The name of the object</param>
    /// <param name="mass">The mass of the object</param>
    /// <param name="primaryBody">The primary body</param>
    /// <param name="initialState">The initial state</param>
    /// <param name="initialOrbit">The initial orbit</param>
    /// <param name="rocketStages">The rocket stages</param>
    /// <param name="textOutputWriter">The text output writer</param>
    public RocketObject(
            string name,
            double mass,
            NaturalSatelliteObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit,
            RocketStages rocketStages,
            ITextOutputWriter textOutputWriter)
            : base(name, mass, primaryBody, initialState, initialOrbit)
        {
            this.rocketStages = rocketStages;
            this.textOutputWriter = textOutputWriter;
        }

        /// <summary>
        /// Indicates if the engines are running
        /// </summary>
        public bool IsEngineRunning => this.engineRunning;

        /// <summary>
        /// Indicates if the rocket is idle
        /// </summary>
        public bool IsIdle => this.controlProgram?.IsCompleted ?? true;

        /// <summary>
        /// Returns the stages
        /// </summary>
        public RocketStages Stages => this.rocketStages;

        /// <summary>
        /// Returns the current stage
        /// </summary>
        public RocketStage CurrentStage => this.rocketStages.CurrentStage;

        /// <summary>
        /// Returns the atmospheric properties
        /// </summary>
        public override AtmosphericProperties AtmosphericProperties => this.Stages.AtmosphericProperties;

        /// <summary>
        /// Sets the control program
        /// </summary>
        /// <param name="controlProgram">The control program</param>
        public void SetControlProgram(IRocketControlProgram controlProgram)
        {
            this.controlProgram = controlProgram;
        }

        /// <summary>
        /// Starts the current program
        /// </summary>
        public void StartProgram()
        {
            if (this.controlProgram != null)
            {
                this.StartEngines();

                if (this.state.HasImpacted)
                {
                    this.state.HasImpacted = false;
                    this.LaunchCoordinates = (this.Latitude, this.Longitude);

                    //var surfaceSpeedDir = Vector3d.Cross(
                    //    OrbitHelpers.SphereNormal(this.PrimaryBody, this.Latitude, this.Longitude),
                    //    this.PrimaryBody.AxisOfRotation);
                    //surfaceSpeedDir.Normalize();
                    //var surfaceVelocity = OrbitHelpers.SurfaceSpeedDueToRotation(this.PrimaryBody, Math.PI / 2.0 - this.Latitude) * surfaceSpeedDir;

                    this.state.Velocity = this.PrimaryBody.Velocity;
                }

                this.controlProgram.Start(this.state.Time);
            }
        }

        /// <summary>
        /// Starts the given program
        /// </summary>
        /// <param name="controlProgram">The control program</param>
        public void StartProgram(IRocketControlProgram controlProgram)
        {
            this.SetControlProgram(controlProgram);
            this.StartProgram();
        }

        /// <summary>
        /// Starts the engines
        /// </summary>
        public void StartEngines()
        {
            this.engineRunning = true;
        }

        /// <summary>
        /// Stops the engines
        /// </summary>
        public void StopEngines()
        {
            this.engineRunning = false;
        }

        /// <summary>
        /// The engine throttle
        /// </summary>
        public double EngineThrottle
        {
            get { return this.CurrentStage.EngineThrottle; }
            set { this.CurrentStage.EngineThrottle = value; }
        }

        /// <summary>
        /// Returns the total thrust generated by the engines
        /// </summary>
        public Vector3d EngineThrust()
        {
            if (this.engineRunning)
            {
                //var direction = this.controlProgram.ThrustDirection;
                var direction = RocketHelpers.RelativeToAbsoluteThrustDirection(this, this.controlProgram.ThrustDirection);
                return this.rocketStages.CurrentStage.TotalThrust * direction;
            }
            else
            {
                return Vector3d.Zero;
            }
        }

        /// <summary>
        /// Returns the rotation torque
        /// </summary>
        public Vector3d RotationTorque()
        {
            if (this.controlProgram is null)
            {
                return Vector3d.Zero;
            }

            return this.controlProgram.Torque;
        }

        /// <summary>
        /// Returns the acceleration produced by the engines
        /// </summary>
        public Vector3d EngineAcceleration()
        {
            return this.EngineThrust() / this.Mass;
        }

        public override void ApplyBurn(double totalTime, Vector3d deltaV)
        {
            this.SetControlProgram(new ExecuteManeuverProgram(this, deltaV));
            this.StartProgram();
        }

        /// <summary>
        /// Marks that the orbit was updated
        /// </summary>
        public void UpdateOrbit()
        {
            this.updateOrbit = true;
        }

        /// <summary>
        /// Handles what happens after the current rocket impulse
        /// </summary>
        /// <param name="time">The time of the impulse</param>
        public void AfterImpulse(double time)
        {
            this.updateOrbit = true;

            var deltaMass = this.rocketStages.UseFuel(time);
            if (deltaMass != null)
            {
                this.UsedDeltaV += RocketFormulas.DeltaV(
                    this.CurrentStage.Engines[0].EffectiveExhaustVelocity,
                    this.Mass,
                    this.Mass - deltaMass.Value);

                this.Mass -= deltaMass.Value;
            }
            else
            {
                if (!this.Stage())
                {
                    this.engineRunning = false;
                }
            }
        }

        /// <summary>
        /// Stages the current stage
        /// </summary>
        /// <param name="applyToStaged">Appies to the new staged object</param>
        /// <returns>True if staged else false</returns>
        public bool Stage(Action<RocketObject> applyToStaged = null)
        {
            if (this.rocketStages.Stage(out var oldStage))
            {
                //var spentStageObject = new SatelliteObject(
                //    $"{this.Name} - {oldStage.Name}",
                //    oldStage.Mass,
                //    this.AtmosphericProperties,
                //    this.PrimaryBody,
                //    this.ReferenceState,
                //    this.ReferenceOrbit);
                var spentStageObject = new RocketObject(
                    $"{this.Name} - {oldStage.Name}",
                    oldStage.Mass,
                    this.PrimaryBody,
                    this.ReferenceState,
                    this.ReferenceOrbit,
                    new RocketStages(new List<RocketStage>() { oldStage }),
                    this.textOutputWriter)
                {
                    LaunchCoordinates = this.LaunchCoordinates
                };

                applyToStaged?.Invoke(spentStageObject);

                spentStageObject.SetNextState(this.ReferenceState);
                this.toStage.Add(spentStageObject);

                this.Mass = this.rocketStages.TotalMass;
                this.textOutputWriter.WriteLine($"{this.Name}: staged '{oldStage.Name}'.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the staged objects
        /// </summary>
        /// <param name="apply">Applied to each staged object</param>
        public void ClearStagedObjects(AddPhysicsObject apply = null)
        {
            if (apply != null)
            {
                foreach (var staged in this.toStage)
                {
                    apply(this, staged);
                }
            }

            this.toStage.Clear();
        }

        public override void Update(double totalTime, double timeStep)
        {
            base.Update(totalTime, timeStep);

            if (this.updateOrbit)
            {
                var primaryBodyState = this.PrimaryBody.State;
                var state = this.State;
                this.UpdateReferenceOrbit();
                this.updateOrbit = false;
            }

            if (this.controlProgram != null)
            {
                this.controlProgram.Update(totalTime, timeStep);

                if (this.controlProgram.IsCompleted)
                {
                    this.controlProgram = null;
                    this.StopEngines();
                }
            }

            if (!this.HasImpacted)
            {
                var state = this.state;
                state.MakeRelative(this.PrimaryBody.State);
            }
            //else
            //{
            //    this.RocketOrientation = OrbitHelpers.SphereNormal(this.PrimaryBody, this.Latitude, this.Longitude);
            //}
        }

        /// <summary>
        /// Sets the orientation of the rocket
        /// </summary>
        /// <param name="orientation">The orientation</param>
        public void SetOrientation(Quaterniond orientation)
        {
            this.state.Orientation = orientation;
        }

        public void KillRotation()
        {
            this.state.AngularMomentum = Vector3d.Zero;
        }
    }
}
