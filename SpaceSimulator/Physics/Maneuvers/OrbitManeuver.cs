using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// The point in time when a maneuver will be executed
    /// </summary>
    public enum OrbitalManeuverTimeType
    {
        Periapsis,
        Apoapsis,
        Now,
        TimeFromNow,
    }

    /// <summary>
    /// Defiens when an orbital maneuver should occure
    /// </summary>
    public struct OrbitalManeuverTime
    {
        /// <summary>
        /// The type
        /// </summary>
        public OrbitalManeuverTimeType Type { get; }

        /// <summary>
        /// The (optional) value
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Creates a new maneuver time
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="value">The value</param>
        public OrbitalManeuverTime(OrbitalManeuverTimeType type, double value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Creates a new maneuver at the periapsis
        /// </summary>
        public static OrbitalManeuverTime Periapsis()
        {
            return new OrbitalManeuverTime(OrbitalManeuverTimeType.Periapsis, 0);
        }

        /// <summary>
        /// Creates a new maneuver at the apoapsis
        /// </summary>
        public static OrbitalManeuverTime Apoapsis()
        {
            return new OrbitalManeuverTime(OrbitalManeuverTimeType.Apoapsis, 0);
        }

        /// <summary>
        /// Creates a new maneuver now
        /// </summary>
        public static OrbitalManeuverTime Now()
        {
            return new OrbitalManeuverTime(OrbitalManeuverTimeType.Now, 0);
        }

        /// <summary>
        /// Creates a new maneuver at the given time from now
        /// </summary>
        /// <param name="timeFromNow">The time from now</param>
        public static OrbitalManeuverTime TimeFromNow(double timeFromNow)
        {
            return new OrbitalManeuverTime(OrbitalManeuverTimeType.TimeFromNow, timeFromNow);
        }
    }

    /// <summary>
    /// Represents an orbital maneuver
    /// </summary>
    public class OrbitalManeuver
    {
        private readonly double maneuverTime;
        private readonly Vector3d deltaVelocity;

        /// <summary>
        /// Creates a new maneuver at the given time
        /// </summary>
        /// <param name="maneuverTime">The maneuver</param>
        /// <param name="deltaVelocity">The amount of delta V to apply</param>
        public OrbitalManeuver(double maneuverTime, Vector3d deltaVelocity)
        {
            this.maneuverTime = maneuverTime;
            this.deltaVelocity = deltaVelocity;
        }

        /// <summary>
        /// Returns the time when the maneuver should be applied
        /// </summary>
        public double ManeuverTime
        {
            get { return this.maneuverTime; }
        }

        /// <summary>
        /// Returns the amount of delta V to apply
        /// </summary>
        public Vector3d DeltaVelocity
        {
            get { return this.deltaVelocity; }
        }

        /// <summary>
        /// Creates a burn at the given time
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="deltaVelocity">The amount of delta V to apply</param>
        /// <param name="maneuverTime">When the maneuver will be applied</param>
        public static OrbitalManeuver Burn(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            Vector3d deltaVelocity,
            OrbitalManeuverTime maneuverTime)
        {
            var state = physicsObject.State;
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject.PrimaryBody, ref state);

            var time = simulatorEngine.TotalTime;
            switch (maneuverTime.Type)
            {
                case OrbitalManeuverTimeType.Periapsis:
                    time += orbitPosition.TimeToPeriapsis();
                    break;
                case OrbitalManeuverTimeType.Apoapsis:
                    time += orbitPosition.TimeToApoapsis();
                    break;
                case OrbitalManeuverTimeType.Now:
                    break;
                case OrbitalManeuverTimeType.TimeFromNow:
                    time += maneuverTime.Value;
                    break;
            }

            return new OrbitalManeuver(time, deltaVelocity);
        }

        public override string ToString()
        {
            return
                "time: " + DataFormatter.Format(this.ManeuverTime, DataUnit.Time)
                + ", Δv: " + DataFormatter.Format(this.DeltaVelocity.Length(), DataUnit.Velocity);
        }
    }

    /// <summary>
    /// Represents a sequence of orbital maneuvers
    /// </summary>
    public class OrbitalManeuvers : IEnumerable<OrbitalManeuver>
    {
        private readonly List<OrbitalManeuver> maneuvers;

        /// <summary>
        /// Creates a sequence of maneuvers
        /// </summary>
        /// <param name="maneuvers">The maneuverers</param>
        public OrbitalManeuvers(IList<OrbitalManeuver> maneuvers)
        {
            this.maneuvers = new List<OrbitalManeuver>(maneuvers);
        }

        /// <summary>
        /// Returns the maneuvers
        /// </summary>
        public IReadOnlyList<OrbitalManeuver> Maneuvers
        {
            get { return this.maneuvers; }
        }

        /// <summary>
        /// Creates a single maneuver
        /// </summary>
        /// <param name="maneuver">The maneuver</param>
        public static OrbitalManeuvers Single(OrbitalManeuver maneuver)
        {
            return new OrbitalManeuvers(new List<OrbitalManeuver>() { maneuver });
        }

        /// <summary>
        /// Creates a sequence of maneuvers
        /// </summary>
        /// <param name="maneuvers">The maneuvers</param>
        public static OrbitalManeuvers Sequence(params OrbitalManeuver[] maneuvers)
        {
            return new OrbitalManeuvers(maneuvers.ToList());
        }

        public IEnumerator<OrbitalManeuver> GetEnumerator()
        {
            return this.maneuvers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.maneuvers.GetEnumerator();
        }
    }
}
