﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics.Rocket
{
    /// <summary>
    /// Represents a rocket stage
    /// </summary>
    public sealed class RocketStage
    {
        private readonly List<RocketEngine> engines;

        /// <summary>
        /// The name of the stage
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The initial total mass (in kg)
        /// </summary>
        public double InitialTotalMass { get; }

        /// <summary>
        /// The amount of mass dedicated to fuel (in kg)
        /// </summary>
        public double FuelMass { get; }

        /// <summary>
        /// The dry mass (in kg)
        /// </summary>
        public double DryMass { get; }

        private double totalThrust;
        private double totalMassFlowRate;

        /// <summary>
        /// Creates a new stage with the given engines
        /// </summary>
        /// <param name="name">The name of the stage</param>
        /// <param name="engines">The engines</param>
        /// <param name="nonEngineDryMass">The non-engine dry mass</param>
        /// <param name="fuelMass">The fuel mass (in kg)</param>
        public RocketStage(string name, List<RocketEngine> engines, double nonEngineDryMass, double fuelMass)
        {
            this.Name = name;
            this.engines = new List<RocketEngine>(engines);
            this.DryMass = nonEngineDryMass + this.engines.Sum(x => x.Mass);
            this.FuelMass = fuelMass;
            this.InitialTotalMass = this.DryMass + this.FuelMass;
            this.totalThrust = this.engines.Sum(engine => engine.Thrust);
            this.totalMassFlowRate = this.engines.Sum(engine => engine.MassFlowRate);
        }

        /// <summary>
        /// Creates a new rocket stage with the given burn time and same type of engines
        /// </summary>
        /// <param name="name">The name of the stage</param>
        /// <param name="numberOfEngines">The number of engines</param>
        /// <param name="thrust">The thrust (in newtons)</param>
        /// <param name="specificImpulse">The specific impulse (in seconds)</param>
        /// <param name="nonEngineDryMass">The non-engine dry mass</param>
        /// <param name="engineMass">The mass of an engine</param>
        /// <param name="burnTime">The burn time</param>
        public static RocketStage CreateFromBurnTime(string name, int numberOfEngines, double thrust, double specificImpulse, double nonEngineDryMass, double engineMass, double burnTime)
        {
            var engines = new List<RocketEngine>();
            for (int i = 0; i < numberOfEngines; i++)
            {
                engines.Add(new RocketEngine(thrust, specificImpulse, engineMass));
            }

            var fuelMass = numberOfEngines * burnTime * RocketFormulas.CalculateMassFlowRate(thrust, specificImpulse);
            return new RocketStage(name, engines, nonEngineDryMass, fuelMass);
        }

        /// <summary>
        /// Creates a paylod stage
        /// </summary>
        /// <param name="name">The name of the stage</param>
        /// <param name="mass">The mass</param>
        public static RocketStage Payload(string name, double mass)
        {
            return new RocketStage(name, new List<RocketEngine>(), mass, 0.0);
        }

        /// <summary>
        /// Returns the rocket engines
        /// </summary>
        public IReadOnlyList<RocketEngine> Engines
        {
            get { return this.engines; }
        }

        /// <summary>
        /// Returns the total amount of thrust generated by all engines (in newtons)
        /// </summary>
        public double TotalThrust
        {
            get { return this.totalThrust; }
        }

        /// <summary>
        /// Returns the total mass flow rate of all the engines (in kg/s)
        /// </summary>
        public double TotalMassFlowRate
        {
            get { return this.totalMassFlowRate; }
        }
    }
}
