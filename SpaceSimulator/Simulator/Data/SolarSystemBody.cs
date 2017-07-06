using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Simulator.Data
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

        /// <summary>
        /// The axis-of-rotation
        /// </summary>
        public Vector3d AxisOfRotation { get; }

        /// <summary>
        /// The atmospheric model
        /// </summary>
        public IAtmosphericModel AtmosphericModel { get; }

        private readonly Orbit referenceOrbit;

        /// <summary>
        /// Creates a new planet
        /// </summary>
        /// <param name="equatorialRadius">The equatorial radius of the body</param>
        /// <param name="meanRadius">The mean radius of the body</param>
        /// <param name="mass">The mass of the body</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="atmosphericModel">The atmospheric model</param>
        /// <param name="orbit">The orbit around the primary body</param>
        public SolarSystemBody(
            double equatorialRadius,
            double meanRadius,
            double mass,
            double rotationalPeriod,
            Vector3d axisOfRotation,
            IAtmosphericModel atmosphericModel,
            Orbit orbit)
        {
            this.EquatorialRadius = equatorialRadius;
            this.MeanRadius = meanRadius;
            this.Mass = mass;
            this.RotationalPeriod = rotationalPeriod;
            this.AxisOfRotation = axisOfRotation;
            this.AtmosphericModel = atmosphericModel;
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
        public Orbit Orbit(IPrimaryBodyObject primaryBody)
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
        /// Creates a physics object using the current body around the given body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="name">The name of the body</param>
        /// <param name="initialTrueAnomaly">The initial true anomaly</param>
        public PlanetObject Create(NaturalSatelliteObject primaryBody, string name, double initialTrueAnomaly)
        {
            var initialOrbit = this.Orbit(primaryBody);
            return new PlanetObject(
                name,
                PhysicsObjectType.NaturalSatellite,
                this.Mass,
                this.Radius,
                this.RotationalPeriod,
                this.AxisOfRotation,
                this.AtmosphericModel,
                primaryBody,
                initialOrbit.CalculateState(initialTrueAnomaly, primaryBody.State),
                initialOrbit);
        }
    }
}
