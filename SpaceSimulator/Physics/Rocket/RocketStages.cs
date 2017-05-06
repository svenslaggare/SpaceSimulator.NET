using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics.Rocket
{
    /// <summary>
    /// Represents a collection of rocket stages
    /// </summary>
    public class RocketStages
    {
        private readonly Queue<RocketEngine> stages;

        /// <summary>
        /// Returns the current stage
        /// </summary>
        public RocketEngine CurrentStage { get; private set; }

        /// <summary>
        /// The initial total mass
        /// </summary>
        public double InitialTotalMass { get; }

        /// <summary>
        /// Returns the amount of fuel mass remaining (in kg) in the current stage
        /// </summary>
        public double FuelMassRemaining { get; private set; }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stage</param>
        public RocketStages(IList<RocketEngine> stages)
        {
            this.stages = new Queue<RocketEngine>(stages);
            this.CurrentStage = this.stages.Dequeue();
            this.InitialTotalMass = stages.Sum(x => x.InitialTotalMass);
            this.FuelMassRemaining = this.CurrentStage.FuelMass;
        }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stages</param>
        /// <param name="currentStage">The current stage</param>
        /// <param name="initialTotalMass">The initial total mass</param>
        /// <param name="fuelMassRemaining">The amount of fuel mass remaining in the current stage</param>
        private RocketStages(Queue<RocketEngine> stages, RocketEngine currentStage, double initialTotalMass, double fuelMassRemaining)
        {
            this.stages = new Queue<RocketEngine>(stages);
            this.CurrentStage = currentStage;
            this.InitialTotalMass = initialTotalMass;
            this.FuelMassRemaining = fuelMassRemaining;
        }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stages</param>
        public static RocketStages New(params RocketEngine[] stages)
        {
            return new RocketStages(new List<RocketEngine>(stages));
        }

        /// <summary>
        /// Returns the total mass in the remaining stages
        /// </summary>
        public double TotalMass
        {
            get { return stages.Sum(x => x.InitialTotalMass) + this.CurrentStage.DryMass + this.FuelMassRemaining; }
        }

        /// <summary>
        /// Clones the current stages
        /// </summary>
        public RocketStages Clone()
        {
            return new RocketStages(this.stages, this.CurrentStage, this.InitialTotalMass, this.FuelMassRemaining);
        }

        /// <summary>
        /// Stages the rocket if possible
        /// </summary>
        /// <param name="oldStage">The old stage if staged</param>
        /// <param name="oldStageFuelMassRemaining">The amount of fuel remaining in the old stage</param>
        /// <returns>True if staged, else false. If staged, sets the out variables.</returns>
        public bool Stage(out RocketEngine oldStage, out double oldStageFuelMassRemaining)
        {
            if (this.stages.Count > 0)
            {
                oldStage = this.CurrentStage;
                oldStageFuelMassRemaining = this.FuelMassRemaining;
                this.CurrentStage = this.stages.Dequeue();
                this.FuelMassRemaining = this.CurrentStage.FuelMass;
                return true;
            }
            else
            {
                oldStage = null;
                oldStageFuelMassRemaining = 0.0;
                return false;
            }
        }

        /// <summary>
        /// Uses fuel in the current stage for the given amount of time
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns>The change in mass, or null if there is not enough fuel</returns>
        public double? UseFuel(double time)
        {
            var deltaMass = this.CurrentStage.TotalMassFlowRate * time;
            if (this.FuelMassRemaining - deltaMass > 0)
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
