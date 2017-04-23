using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents a body in the solar system
    /// </summary>
    public class SolarSystemBody
    {
        /// <summary>
        /// The equatorial radius of the body
        /// </summary>
        public double EquatorialRadius { get; }

        /// <summary>
        /// The mean radius of the body
        /// </summary>
        public double MeanRadius { get; }

        /// <summary>
        /// The mass of the body
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// The rotational period
        /// </summary>
        public double RotationalPeriod { get; }

        private readonly Orbit referenceOrbit;

        /// <summary>
        /// Creates a new planet
        /// </summary>
        /// <param name="equatorialRadius">The equatorial radius of the body</param>
        /// <param name="meanRadius">The mean radius of the body</param>
        /// <param name="mass">The mass of the body</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="orbit">The orbit around the primary body</param>
        public SolarSystemBody(double equatorialRadius, double meanRadius, double mass, double rotationalPeriod, Orbit orbit)
        {
            this.EquatorialRadius = equatorialRadius;
            this.MeanRadius = meanRadius;
            this.Mass = mass;
            this.RotationalPeriod = rotationalPeriod;
            this.referenceOrbit = orbit;
        }

        /// <summary>
        /// The radius of the body (assuming spherical shape)
        /// </summary>
        public double Radius
        {
            get { return this.MeanRadius; }
        }

        /// <summary>
        /// Returns an orbit around the primary body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        public Orbit Orbit(IPhysicsObject primaryBody)
        {
            return new Physics.Orbit(
                primaryBody,
                this.referenceOrbit.Parameter,
                this.referenceOrbit.Eccentricity,
                this.referenceOrbit.Inclination,
                this.referenceOrbit.LongitudeOfAscendingNode,
                this.referenceOrbit.ArgumentOfPeriapsis);
        }

        /// <summary>
        /// Creates the configuration
        /// </summary>
        public ObjectConfig CreateConfig()
        {
            return new ObjectConfig(this.Radius, this.Mass, this.RotationalPeriod);
        }

        /// <summary>
        /// Creates a physics object using the current body around the given body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="name">The name of the body</param>
        /// <param name="initialTrueAnomaly">The initial true anomaly</param>
        public PhysicsObject Create(PhysicsObject primaryBody, string name, double initialTrueAnomaly)
        {
            var initialOrbit = this.Orbit(primaryBody);
            return new PhysicsObject(
                name,
                PhysicsObjectType.NaturalSatellite,
                this.CreateConfig(),
                primaryBody,
                initialOrbit.CalculateState(initialTrueAnomaly, primaryBody.State),
                initialOrbit);
        }
    }
}
