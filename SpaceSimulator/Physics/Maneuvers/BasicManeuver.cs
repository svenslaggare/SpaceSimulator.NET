using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// Contains basic orbit maneuvers
    /// </summary>
    public static class BasicManeuver
    {
        /// <summary>
        /// Changes the periapsis
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="newPeriapsis">The new periapsis</param>
        public static OrbitalManeuvers ChangePeriapsis(ISimulatorEngine simulatorEngine, IPhysicsObject physicsObject, double newPeriapsis)
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            var orbit = orbitPosition.Orbit;

            if (newPeriapsis > orbit.Apoapsis)
            {
                throw new ArgumentException("New perapsis cannot be higher than the apoapsis.");
            }

            orbitPosition.TrueAnomaly = Math.PI;
            var apoapsis = orbitPosition.CalculateState();

            var newSemiMajorAxis = (newPeriapsis + orbit.Apoapsis) / 2.0;
            var newEccentricity = (orbit.Apoapsis - newPeriapsis) / (orbit.Apoapsis + newPeriapsis);
            var newOrbit = orbit.Set(
                eccentricity: newEccentricity,
                parameter: OrbitFormulas.ParameterFromSemiMajorAxis(newSemiMajorAxis, newEccentricity));
            var deltaV = newOrbit.CalculateState(orbitPosition.TrueAnomaly).Velocity - apoapsis.Velocity;

            return OrbitalManeuvers.Single(OrbitalManeuver.Burn(simulatorEngine, physicsObject, deltaV, OrbitalManeuverTime.Apoapsis()));
        }

        /// <summary>
        /// Changes the apoapsis
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="newApoapsis">The new apoapsis</param>
        public static OrbitalManeuvers ChangeApoapsis(ISimulatorEngine simulatorEngine, IPhysicsObject physicsObject, double newApoapsis)
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            var orbit = orbitPosition.Orbit;

            if (newApoapsis < orbit.Periapsis)
            {
                throw new ArgumentException("New apoapsis cannot be lower than the periapsis.");
            }

            orbitPosition.TrueAnomaly = 0.0;
            var periapsis = orbitPosition.CalculateState();

            var newSemiMajorAxis = (newApoapsis + orbit.Periapsis) / 2.0;
            var newEccentricity = (newApoapsis - orbit.Periapsis) / (newApoapsis + orbit.Periapsis);
            var newOrbit = orbit.Set(
                eccentricity: newEccentricity,
                parameter: OrbitFormulas.ParameterFromSemiMajorAxis(newSemiMajorAxis, newEccentricity));
            var deltaV = newOrbit.CalculateState(orbitPosition.TrueAnomaly).Velocity - periapsis.Velocity;

            return OrbitalManeuvers.Single(OrbitalManeuver.Burn(simulatorEngine, physicsObject, deltaV, OrbitalManeuverTime.Periapsis()));
        }

        /// <summary>
        /// Changes the inclination
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="newinclination">The new inclination</param>
        public static OrbitalManeuvers ChangeInclination(ISimulatorEngine simulatorEngine, IPhysicsObject physicsObject, double newInclination)
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            var orbit = orbitPosition.Orbit;

            orbitPosition = orbitPosition.Set(trueAnomaly: Math.PI, argumentOfPeriapsis: 0.0);
            var velocity = orbitPosition.CalculateState().Velocity;
            orbitPosition = orbitPosition.Set(inclination: newInclination);
            var velocityNext = orbitPosition.CalculateState().Velocity;
            var deltaV = velocityNext - velocity;

            return OrbitalManeuvers.Single(OrbitalManeuver.Burn(simulatorEngine, physicsObject, deltaV, OrbitalManeuverTime.Apoapsis()));

            //var ascendingNodePosition = orbit.AscendingNodePosition;
            //var state = ascendingNodePosition.CalculateState();
            //state.MakeRelative(physicsObject.PrimaryBody.State);
            //var velocity = state.Velocity;
            //var deltaV = (velocity + state.Normal * 1000) - velocity;

            //return OrbitalManeuvers.Single(
            //    OrbitalManeuver.Burn(
            //        simulatorEngine, 
            //        physicsObject, 
            //        deltaV, 
            //        OrbitalManeuverTime.TimeFromNow(orbitPosition.TimeToTrueAnomaly(ascendingNodePosition.TrueAnomaly))));
        }
    }
}
