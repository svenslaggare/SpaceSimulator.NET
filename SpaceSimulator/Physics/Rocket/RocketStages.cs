using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Physics.Rocket
{
    /// <summary>
    /// Represents a collection of rocket stages
    /// </summary>
    public sealed class RocketStages : IEnumerable<RocketStage>
    {
        private readonly Queue<RocketStage> stages;
        
        /// <summary>
        /// Returns the current stage
        /// </summary>
        public RocketStage CurrentStage { get; private set; }

        /// <summary>
        /// The initial total mass
        /// </summary>
        public double InitialTotalMass { get; }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stage</param>
        public RocketStages(IList<RocketStage> stages)
        {
            this.stages = new Queue<RocketStage>(stages);
            this.CurrentStage = this.stages.Dequeue();
            this.InitialTotalMass = stages.Sum(x => x.InitialTotalMass);
        }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stages</param>
        /// <param name="currentStage">The current stage</param>
        /// <param name="initialTotalMass">The initial total mass</param>
        private RocketStages(IEnumerable<RocketStage> stages, RocketStage currentStage, double initialTotalMass)
        {
            this.stages = new Queue<RocketStage>(stages);
            this.CurrentStage = currentStage;
            this.InitialTotalMass = initialTotalMass;
        }

        /// <summary>
        /// Creates a new collection of stages
        /// </summary>
        /// <param name="stages">The stages</param>
        public static RocketStages New(params RocketStage[] stages)
        {
            return new RocketStages(new List<RocketStage>(stages));
        }

        /// <summary>
        /// Returns the total mass in the remaining stages
        /// </summary>
        public double TotalMass
        {
            get { return stages.Sum(x => x.InitialTotalMass) + this.CurrentStage.DryMass + this.FuelMassRemaining; }
        }

        /// <summary>
        /// Returns the amount of fuel mass remaining (in kg) in the current stage
        /// </summary>
        public double FuelMassRemaining
        {
            get { return this.CurrentStage.FuelMassRemaining; }
        }

        /// <summary>
        /// The atmospheric properties for the stage
        /// </summary>
        public AtmosphericProperties AtmosphericProperties
        {
            get
            {
                if (this.stages.Count > 0)
                {
                    return this.stages.Last().AtmosphericProperties;
                }
                else
                {
                    return this.CurrentStage.AtmosphericProperties;
                }
            }
        }

        /// <summary>
        /// Clones the current stages
        /// </summary>
        public RocketStages Clone()
        {
            return new RocketStages(this.stages.Select(x => x.Clone()), this.CurrentStage.Clone(), this.InitialTotalMass);
        }

        /// <summary>
        /// Uses fuel in the current stage for the given amount of time
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns>The change in mass, or null if there is not enough fuel</returns>
        public double? UseFuel(double time)
        {
            return this.CurrentStage.UseFuel(time);
        }

        /// <summary>
        /// Stages the rocket if possible
        /// </summary>
        /// <param name="oldStage">The old stage if staged</param>
        /// <returns>True if staged, else false. If staged, sets the out variables.</returns>
        public bool Stage(out RocketStage oldStage)
        {
            if (this.stages.Count > 0)
            {
                oldStage = this.CurrentStage;
                this.CurrentStage = this.stages.Dequeue();
                return true;
            }
            else
            {
                oldStage = null;
                return false;
            }
        }

        public IEnumerator<RocketStage> GetEnumerator()
        {
            yield return this.CurrentStage;

            foreach (var stage in this.stages)
            {
                yield return stage;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
