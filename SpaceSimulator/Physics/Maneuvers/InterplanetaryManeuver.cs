using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// Contains methods for calcuating interplanetary maneuvers
    /// </summary>
    public static class InterplanetaryManeuver
    { 
        /// <summary>
        /// Transfers from the current planet to the given
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="target">The object to rendevouz with</param>
        /// <param name="possibleDepartureBurns">The possible depature burns</param>
        public static OrbitalManeuvers PlanetaryTransfer(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            IPhysicsObject target,
            out IList<InterceptManeuver.PossibleLaunch> possibleDepartureBurns)
        {
            var planetaryTransfer = new PlanetaryTransfer(simulatorEngine, physicsObject, target);
            var maneuvers = planetaryTransfer.Compute();
            possibleDepartureBurns = planetaryTransfer.PossibleDepartureBurns;
            return maneuvers;
        }
    }
}
