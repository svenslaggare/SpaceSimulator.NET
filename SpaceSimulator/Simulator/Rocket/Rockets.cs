using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Rocket;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Contains list of rockets
    /// </summary>
    public static class Rockets
    {
        /// <summary>
        /// Creates a new Falcon 9 rocket with the given payload mass
        /// </summary>
        /// <param name="payloadMass">The mass of the payload</param>
        public static RocketStages CreateFalcon9(double payloadMass)
        {
            return RocketStages.New(
                RocketStage.FromBurnTime(0, "Stage 1", 9, 845E3, 282, 5000, 470, new AtmosphericProperties(AtmosphericFormulas.CircleArea(3.75), 0.1 * 1.0), 162),
                RocketStage.FromBurnTime(1, "Stage 2", 1, 934E3, 348, 500, 470, new AtmosphericProperties(AtmosphericFormulas.CircleArea(3.75), 0.1 * 1.0), 397),
                RocketStage.Payload(2, "Payload", payloadMass, new AtmosphericProperties(AtmosphericFormulas.ConeNoseSurfaceArea(3.7, 1.5), 0.01 * 1.0))
            );
        }
    }
}
