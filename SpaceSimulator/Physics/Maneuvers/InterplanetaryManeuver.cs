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
        /// Calculates an insertion burn to the given planet from interplanetary space
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="target">The target planet</param>
        public static OrbitalManeuvers PlanetaryInsertion(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            IPrimaryBodyObject target)
        {
            for (double deltaV = -100.0; deltaV <= 100.0; deltaV += 1)
            {
                var physicsObjectState = physicsObject.State;
                var relativePhysicsObjectState = physicsObjectState;
                relativePhysicsObjectState.MakeRelative(physicsObject.PrimaryBody.State);
                physicsObjectState.Velocity += relativePhysicsObjectState.Prograde * deltaV;

                var physicsObjectOrbit = Orbit.CalculateOrbit(physicsObject.PrimaryBody, physicsObjectState);
                var targetOrbit = Orbit.CalculateOrbit(target);

                var soi = OrbitFormulas.SphereOfInfluence(
                    targetOrbit.SemiMajorAxis,
                    target.Mass,
                    target.PrimaryBody.Mass);

                if (OrbitHelpers.SOIChangeLikely(physicsObjectOrbit, targetOrbit, soi))
                {
                    var enterOrbit = OrbitPosition.CalculateOrbitPosition(target, ref physicsObjectState);
                    var soiChangeTime = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(enterOrbit);

                    if (soiChangeTime != null && soiChangeTime > 0)
                    {
                        var objectAtSOI = SolverHelpers.AfterTime(
                            simulatorEngine.KeplerProblemSolver,
                            physicsObject,
                            physicsObjectState,
                            physicsObjectOrbit,
                            soiChangeTime ?? 0.0);

                        var targetAtSOI = SolverHelpers.AfterTime(
                            simulatorEngine.KeplerProblemSolver,
                            target,
                            target.State,
                            targetOrbit,
                            soiChangeTime ?? 0.0);

                        var distance = Vector3d.Distance(objectAtSOI.Position, targetAtSOI.Position);

                        if (Math.Abs(deltaV) < 1E-9)
                        {
                            Console.WriteLine(
                                $"{DataFormatter.Format(distance, DataUnit.Distance)}: " +
                                $"{DataFormatter.Format(soi, DataUnit.Distance)}");
                        }

                        if (distance < soi)
                        {
                            var objectOrbitAroundTarget = OrbitPosition.CalculateOrbitPosition(target, ref targetAtSOI, ref objectAtSOI);
                            Console.WriteLine($"{deltaV}: {objectOrbitAroundTarget.Orbit.Eccentricity}, {objectOrbitAroundTarget.Orbit.RelativePeriapsis}");
                        }
                    }
                }
            }

            return OrbitalManeuvers.Sequence();
        }

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
