﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Rocket;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Simulator
{
    public enum RocketObjectState
    {
        InitialAscent,
        Coast,
        Circularizing,
        InOrbit
    }

    /// <summary>
    /// Represents a physics object with a rocket engine
    /// </summary>
    public class RocketObject : PhysicsObject
    {
        private readonly RocketStages rocketStages;
        private bool engineRunning;
        private IRocketControlProgram controlProgram;

        /// <summary>
        /// Creates a new rocket object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="config">The configuration</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="rocketStages">The rocket stages</param>
        public RocketObject(
            string name,
            ObjectConfig config,
            PhysicsObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit,
            RocketStages rocketStages)
            : base(name, PhysicsObjectType.ArtificialSatellite, config, primaryBody, initialState, initialOrbit, false)
        {
            this.rocketStages = rocketStages;
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
                this.state.Impacted = false;

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
            var primaryBodyState = this.PrimaryBody.NextState;
            var state = this.NextState;

            this.ReferencePrimaryBodyState = primaryBodyState;
            this.ReferenceState = state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(this.PrimaryBody, ref primaryBodyState, ref state);
            this.orbitChanged = true;

            var deltaMass = this.rocketStages.UseFuel(time);
            if (deltaMass != null)
            {
                this.Configuration = this.Configuration.WithMass(this.Mass - deltaMass.Value);
            }
            else
            {
                if (this.rocketStages.Stage())
                {
                    this.Configuration = this.Configuration.WithMass(this.rocketStages.TotalMass);
                }
                else
                {
                    this.engineRunning = false;
                }
            }
        }

        public override void Update(double totalTime, double timeStep)
        {
            base.Update(totalTime, timeStep);
            this.controlProgram?.Update(totalTime, timeStep);
        }
    }
}
