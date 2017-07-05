using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Rocket;
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
        /// Indicates if the engine is running
        /// </summary>
        public bool IsEngineRunning
        {
            get { return this.engineRunning; }
        }

        /// <summary>
        /// Returns the stages
        /// </summary>
        public RocketStages Stages
        {
            get { return this.rocketStages; }
        }

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
        /// Starts the rocket engine
        /// </summary>
        public void StartEngine()
        {
            if (this.controlProgram != null)
            {
                this.engineRunning = true;

                if (this.state.HasImpacted)
                {
                    this.state.HasImpacted = false;
                    this.state.Velocity = this.PrimaryBody.Velocity;
                }

                this.controlProgram.Start(this.state.Time);
            }
        }

        /// <summary>
        /// Stops the engine
        /// </summary>
        public void StopEngine()
        {
            this.engineRunning = false;
        }

        /// <summary>
        /// Returns the acceleration produced by the engine
        /// </summary>
        public Vector3d EngineAcceleration()
        {
            if (this.engineRunning)
            {
                var direction = this.controlProgram.ThrustDirection;
                return (this.rocketStages.CurrentStage.TotalThrust / this.Mass) * direction;
            }
            else
            {
                return Vector3d.Zero;
            }
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
        /// <returns>True if staged else false</returns>
        public bool Stage()
        {
            if (this.rocketStages.Stage(out var oldStage))
            {
                var spentStageObject = new SatelliteObject(
                    $"{this.Name} - {oldStage.Name}",
                    oldStage.Mass,
                    this.AtmosphericProperties,
                    this.PrimaryBody,
                    this.ReferenceState,
                    this.ReferenceOrbit);
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
        public void ClearStagedObjects(Action<PhysicsObject> apply = null)
        {
            if (apply != null)
            {
                foreach (var staged in this.toStage)
                {
                    apply(staged);
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

                this.ReferencePrimaryBodyState = primaryBodyState;
                this.ReferenceState = state;
                this.ReferenceOrbit = Orbit.CalculateOrbit(this.PrimaryBody, ref primaryBodyState, ref state);
                this.orbitChanged = true;

                this.updateOrbit = false;
            }

            this.controlProgram?.Update(totalTime, timeStep);
        }
    }
}
