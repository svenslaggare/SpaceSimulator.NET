using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// Contains rendevouz maneuvers
    /// </summary>
    public static class RendevouzManeuver
    {
        /// <summary>
        /// The rendevouz methods
        /// </summary>
        private enum RendevouzMethod
        {
            InCircularOrbit,
            InSameOrbit
        }

        /// <summary>
        /// Rendevouz with the object at the current orbit (both in circular orbits and in same plane)
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="orbitPosition">The orbit of the object</param>
        /// <param name="targetOrbitPosition">The orbit of the object to rendevouz with</param>
        public static OrbitalManeuvers RendevouzInCircularOrbit(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref OrbitPosition orbitPosition,
            ref OrbitPosition targetOrbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;

            if (!(orbit.IsCircular && targetOrbit.IsCircular))
            {
                throw new ArgumentException("Both orbits must be circular.");
            }

            if (!orbit.SamePlane(ref targetOrbit))
            {
                throw new ArgumentException("Both orbits must lie in the same plane.");
            }

            var r1 = orbit.SemiMajorAxis;
            var r2 = targetOrbit.SemiMajorAxis;

            //Calculate the time when the maneuver should be applied
            var t = HohmannTransferOrbit.TimeToAlignment(ref orbitPosition, ref targetOrbitPosition);

            //Schedule the maneuver
            var state = physicsObject.State;
            return HohmannTransferOrbit.Create(
                simulatorEngine,
                physicsObject,
                ref state,
                ref orbitPosition,
                r1,
                r2,
                OrbitalManeuverTime.TimeFromNow(t));
        }

        /// <summary>
        /// Rendevouz with an object in the same orbit (but at different positions)
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="orbitPosition">The orbit of the object</param>
        /// <param name="targetOrbitPosition">The orbit of the object to rendevouz with</param>
        /// <param name="numPhasingOrbits">The number of phasing orbits.</param>
        public static OrbitalManeuvers RendevouzInSameOrbit(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref OrbitPosition orbitPosition,
            ref OrbitPosition targetOrbitPosition,
            int numPhasingOrbits = 1)
        {
            var orbit = orbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;

            if (!orbit.SameOrbit(ref targetOrbit))
            {
                throw new ArgumentException("Both orbits must be the same");
            }

            //var mu = physicsObject.PrimaryBody.StandardGravitationalParameter;
            //var deltaTrueAnomaly = orbit.TrueAnomaly - targetOrbit.TrueAnomaly;

            //var n = Math.Sqrt(mu / Math.Pow(targetOrbit.EllipticalSemiMajorAxis(), 3.0));
            //var P = (2 * Math.PI * numPhasingOrbits + deltaTrueAnomaly) / n;
            //var ap = Math.Pow(mu * Math.Pow(P / (2.0 * Math.PI * numPhasingOrbits), 2.0), 1.0 / 3.0);
            //var deltaV = Math.Sqrt(((2.0 * mu) / orbit.Periapsis) - mu / ap) - Math.Sqrt(mu / orbit.Periapsis);

            //var periapsis = orbit;
            //periapsis.TrueAnomaly = 0.0;
            //var dir = periapsis.CalculateState().Prograde;

            //Burn(
            //    physicsSystem,
            //    physicsObject,
            //    deltaV * dir,
            //    OrbitalManeuverTime.Periapsis());

            //Burn(
            //    physicsSystem,
            //    physicsObject,
            //    deltaV * -dir,
            //    OrbitalManeuverTime.TimeFromNow(orbit.TimeToPeriapsis(physicsSystem.TotalTime) + P));

            var mu = physicsObject.PrimaryBody.StandardGravitationalParameter;

            //Calculate the time to periapsis, and the difference in true anomaly at periapsis
            var timeToPeriapsis = orbitPosition.TimeToPeriapsis();
            var targetOrbitState = targetOrbitPosition.CalculateState();
            var initalPrimaryBodyState = physicsObject.PrimaryBody.State;
            var targetOrbitWhenPeriapsis = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject,
                ref initalPrimaryBodyState,
                ref targetOrbitState,
                targetOrbit,
                timeToPeriapsis);

            var deltaTrueAnomaly = OrbitPosition.CalculateOrbitPosition(physicsObject.PrimaryBody, ref targetOrbitWhenPeriapsis).TrueAnomaly;

            //Calculate the period of the phasing orbit
            var T1 = orbit.Period;
            var e1 = orbit.Eccentricity;
            var E = 2 * Math.Atan(Math.Sqrt((1.0 - e1) / (1.0 + e1)) * Math.Tan(deltaTrueAnomaly / 2.0));
            var t = (T1 / (2.0 * Math.PI * numPhasingOrbits)) * (E - e1 * Math.Sin(E));
            var T2 = (T1 - t) * numPhasingOrbits;

            //Compute the semi-major axis, periapsis and apoapsis of the phasing orbit
            var a2 = Math.Pow(mu * Math.Pow(T2 / (2.0 * Math.PI * numPhasingOrbits), 2.0), 1.0 / 3.0);
            var rp = orbit.Periapsis;
            var ra = 2.0 * a2 - rp;

            //Calculate the deltaV
            var h1 = Math.Sqrt(orbit.Parameter * mu);
            var h2 = Math.Sqrt(2 * mu) * Math.Sqrt((ra * rp) / (ra + rp));
            var deltaV = (h2 / orbit.Periapsis) - (h1 / orbit.Periapsis);

            //Calculate the direction at the burn
            var periapsis = orbitPosition;
            periapsis.TrueAnomaly = 0.0;
            var dir = periapsis.CalculateState().Prograde;

            return OrbitalManeuvers.Sequence(
                OrbitalManeuver.Burn(simulatorEngine, physicsObject, deltaV * dir, OrbitalManeuverTime.TimeFromNow(timeToPeriapsis)),
                OrbitalManeuver.Burn(simulatorEngine, physicsObject, deltaV * -dir, OrbitalManeuverTime.TimeFromNow(timeToPeriapsis + T2)));
        }

        /// <summary>
        /// Rendevouz with the object at the current orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="targetOrbitPosition">The orbit of the object to rendevouz with</param>
        public static OrbitalManeuvers Rendevouz(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref OrbitPosition targetOrbitPosition)
        {
            var targetOrbit = targetOrbitPosition.Orbit;
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            var orbit = orbitPosition.Orbit;

            var rendevouzMethod = RendevouzMethod.InCircularOrbit;

            if (orbit.IsCircular && targetOrbit.IsCircular)
            {
                if (!orbit.SameOrbit(ref targetOrbit))
                {
                    rendevouzMethod = RendevouzMethod.InCircularOrbit;
                }
                else
                {
                    rendevouzMethod = RendevouzMethod.InSameOrbit;
                }
            }
            else
            {
                if (orbit.SameOrbit(ref targetOrbit))
                {
                    rendevouzMethod = RendevouzMethod.InSameOrbit;
                }
                else
                {
                    //TODO: Add plane change and match periapsis/apoapsis, then use InSameOrbit.
                    throw new ArgumentException("Current orbit not supported");
                }
            }

            switch (rendevouzMethod)
            {
                case RendevouzMethod.InCircularOrbit:
                    return RendevouzInCircularOrbit(simulatorEngine, physicsObject, ref orbitPosition, ref targetOrbitPosition);
                case RendevouzMethod.InSameOrbit:
                    return RendevouzInSameOrbit(simulatorEngine, physicsObject, ref orbitPosition, ref targetOrbitPosition);
                default:
                    return new OrbitalManeuvers(new List<OrbitalManeuver>());
            }
        }
    }
}
