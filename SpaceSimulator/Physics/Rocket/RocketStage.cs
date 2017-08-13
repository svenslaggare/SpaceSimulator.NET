using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Physics.Rocket
{
    /// <summary>
    /// Represents a rocket stage
    /// </summary>
    public sealed class RocketStage
    {
        private readonly List<RocketEngine> engines;

        /// <summary>
        /// The number of the stage
        /// </summary>
        public int Number { get; }

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
        public double InitialFuelMass { get; }

        /// <summary>
        /// The dry mass (in kg)
        /// </summary>
        public double DryMass { get; }

        /// <summary>
        /// The atmospheric properties for the stage
        /// </summary>
        public AtmosphericProperties AtmosphericProperties { get; }

        /// <summary>
        /// Returns the amount of fuel mass remaining (in kg) in the stage
        /// </summary>
        public double FuelMassRemaining { get; private set; }

        private double totalThrust;
        private double totalMassFlowRate;
        private double engineThrottle = 1.0;

        /// <summary>
        /// Creates a new stage with the given engines
        /// </summary>
        /// <param name="number">The number of the stage</param>
        /// <param name="name">The name of the stage</param>
        /// <param name="engines">The engines</param>
        /// <param name="nonEngineDryMass">The non-engine dry mass</param>
        /// <param name="fuelMass">The fuel mass (in kg)</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        public RocketStage(
            int number, 
            string name, 
            IList<RocketEngine> engines, 
            double nonEngineDryMass,
            double fuelMass, 
            AtmosphericProperties atmosphericProperties)
        {
            this.Number = number;
            this.Name = name;
            this.engines = new List<RocketEngine>(engines);

            this.DryMass = nonEngineDryMass + this.engines.Sum(x => x.Mass);
            this.InitialFuelMass = fuelMass;
            this.InitialTotalMass = this.DryMass + this.InitialFuelMass;
            this.FuelMassRemaining = fuelMass;

            this.totalThrust = this.engines.Sum(engine => engine.Thrust);
            this.totalMassFlowRate = this.engines.Sum(engine => engine.MassFlowRate);

            this.AtmosphericProperties = atmosphericProperties;
        }

        /// <summary>
        /// Copies the given stage
        /// </summary>
        /// <param name="rocketStage">The rocket stage to copy</param>
        private RocketStage(RocketStage rocketStage)
        {
            this.Number = rocketStage.Number;
            this.Name = rocketStage.Name;
            this.engines = new List<RocketEngine>(rocketStage.engines);

            this.DryMass = rocketStage.DryMass;
            this.InitialFuelMass = rocketStage.InitialFuelMass;
            this.InitialTotalMass = rocketStage.InitialTotalMass;
            this.FuelMassRemaining = rocketStage.FuelMassRemaining;

            this.totalThrust = this.engines.Sum(engine => engine.Thrust);
            this.totalMassFlowRate = this.engines.Sum(engine => engine.MassFlowRate);

            this.AtmosphericProperties = rocketStage.AtmosphericProperties;
        }

        /// <summary>
        /// Creates a clone of the current stage
        /// </summary>
        public RocketStage Clone()
        {
            return new RocketStage(this);
        }

        /// <summary>
        /// Creates a new rocket stage with the given burn time and same type of engines
        /// </summary>
        /// <param name="number">The number of the stage</param>
        /// <param name="name">The name of the stage</param>
        /// <param name="numberOfEngines">The number of engines</param>
        /// <param name="thrust">The thrust (in newtons)</param>
        /// <param name="specificImpulse">The specific impulse (in seconds)</param>
        /// <param name="nonEngineDryMass">The non-engine dry mass</param>
        /// <param name="engineMass">The mass of an engine</param>
        /// <param name="burnTime">The burn time</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        public static RocketStage FromBurnTime(
            int number,
            string name,
            int numberOfEngines,
            double thrust,
            double specificImpulse,
            double nonEngineDryMass,
            double engineMass,
            AtmosphericProperties atmosphericProperties,
            double burnTime)
        {
            var engines = new List<RocketEngine>();
            for (int i = 0; i < numberOfEngines; i++)
            {
                engines.Add(new RocketEngine(thrust, specificImpulse, engineMass));
            }

            var fuelMass = numberOfEngines * burnTime * RocketFormulas.MassFlowRate(thrust, specificImpulse);
            return new RocketStage(number, name, engines, nonEngineDryMass, fuelMass, atmosphericProperties);
        }

        /// <summary>
        /// Creates a paylod stage
        /// </summary>
        /// <param name="number">The number of the stage</param>
        /// <param name="name">The name of the stage</param>
        /// <param name="mass">The mass</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        public static RocketStage Payload(int number, string name, double mass, AtmosphericProperties atmosphericProperties)
        {
            return new RocketStage(number, name, new List<RocketEngine>(), mass, 0.0, atmosphericProperties);
        }

        /// <summary>
        /// Returns the mass of the stage
        /// </summary>
        public double Mass
        {
            get { return this.DryMass + this.FuelMassRemaining; }
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
            get { return this.totalThrust * this.engineThrottle; }
        }

        /// <summary>
        /// Returns the total mass flow rate of all the engines (in kg/s)
        /// </summary>
        public double TotalMassFlowRate
        {
            get { return this.totalMassFlowRate * this.engineThrottle; }
        }

        /// <summary>
        /// The engine throttle [0, 1]
        /// </summary>
        public double EngineThrottle
        {
            get { return this.engineThrottle; }
            set
            {
                this.engineThrottle = MathHelpers.Clamp(0, 1, value);
            }
        }

        /// <summary>
        /// Returns the ratio of the remaining fuel mass
        /// </summary>
        public double FuelMassRemainingRatio => this.FuelMassRemaining / this.InitialFuelMass;

        /// <summary>
        /// Uses fuel in the stage for the given amount of time
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns>The change in mass, or null if there is not enough fuel</returns>
        public double? UseFuel(double time)
        {
            var deltaMass = this.TotalMassFlowRate * time;
            if (this.FuelMassRemaining - deltaMass > 0)
            //if ((this.FuelMassRemaining - deltaMass) / this.InitialFuelMass > 0.10)
            {
                this.FuelMassRemaining -= deltaMass;
                return deltaMass;
            }
            else
            {
                return null;
            }
        }
    }
}
