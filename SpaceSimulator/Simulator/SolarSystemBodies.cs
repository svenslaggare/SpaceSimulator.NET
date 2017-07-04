using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// List of important bodies in the solar system
    /// </summary>
    public static class SolarSystemBodies
    {
        private static readonly SolarSystemBody sun;
        private static readonly SolarSystemBody mercury;
        private static readonly SolarSystemBody venus;
        private static readonly SolarSystemBody earth;
        private static readonly SolarSystemBody moon;
        private static readonly SolarSystemBody mars;
        private static readonly SolarSystemBody jupiter;
        private static readonly SolarSystemBody saturn;
        private static readonly SolarSystemBody uranus;
        private static readonly SolarSystemBody neptune;
        private static readonly SolarSystemBody pluto;

        static SolarSystemBodies()
        {
            var allowRotation = 1.0;
            var oneDay = 24.0 * 60.0 * 60.0;
            var oneHour = 60.0 * 60.0;

            sun = new SolarSystemBody(
                695700 * 1000,
                695700 * 1000,
                1.98855E30,
                0,
                Vector3d.Up,
                new NoAtmosphereModel(),
                new Orbit());

            mercury = new SolarSystemBody(
                2439.7 * 1000,
                2439.7 * 1000,
                3.3011E23,
                58.6462 * oneDay * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 57909050E3,
                   eccentricity: 0.20563,
                   inclination: MathUtild.Deg2Rad * 7.005,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 48.331,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 29.124
               ));

            venus = new SolarSystemBody(
                6051.8 * 1000,
                6051.8 * 1000,
                4.8675E24,
                -243.0185 * oneDay * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 108208000E3,
                   eccentricity: 0.006772,
                   inclination: MathUtild.Deg2Rad * 3.39458,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 76.680,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 54.884
                ));

            earth = new SolarSystemBody(
                6378.1370 * 1000,
                6371 * 1000,
                5.9722E24,
                Constants.SiderealDay * allowRotation,
                Vector3d.Up,
                new EarthAtmosphericModel(),
                Orbit.New(
                   semiMajorAxis: 149598023E3,
                   eccentricity: 0.0167086,
                   inclination: MathUtild.Deg2Rad * 0.00005,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * -11.26064,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 114.20783
                ));

            moon = new SolarSystemBody(
                1738.1 * 1000,
                1737.1 * 1000,
                7.342E22,
                27.321661 * oneDay * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                    semiMajorAxis: 384399E3,
                    eccentricity: 0.0549,
                    inclination: MathUtild.Deg2Rad * 5.145
                ));

            mars = new SolarSystemBody(
                3396.2 * 1000,
                3389.5 * 1000,
                6.4171E23,
                24.622962 * oneHour * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 227.9392E9,
                   eccentricity: 0.0934,
                   inclination: MathUtild.Deg2Rad * 1.850,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 49.558,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 286.502
               ));

            jupiter = new SolarSystemBody(
                71492 * 1000,
                69911 * 1000,
                1.8986E27,
                (9.0 * oneHour + 55.0 * 60.0 + 29.685) * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 778.299E9,
                   eccentricity: 0.048498,
                   inclination: MathUtild.Deg2Rad * 1.303,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 100.464,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 273.867
               ));

            saturn = new SolarSystemBody(
                60268 * 1000,
                58232 * 1000,
                5.6836E26,
                (10.0 * oneHour + 39.0 * 60.0 + 22.4) * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 1429.39E9,
                   eccentricity: 0.05555,
                   inclination: MathUtild.Deg2Rad * 2.485240,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 113.665,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 339.392
               ));

            uranus = new SolarSystemBody(
                25559 * 1000,
                25362 * 1000,
                8.6810E25,
                17.24 * oneHour * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 2875.04E9,
                   eccentricity: 0.046381,
                   inclination: MathUtild.Deg2Rad * 0.773,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 74.006,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 96.998857
               ));

            neptune = new SolarSystemBody(
                24764 * 1000,
                24622 * 1000,
                1.0243E26,
                16.11 * oneHour * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 4504.45E9,
                   eccentricity: 0.009456,
                   inclination: MathUtild.Deg2Rad * 1.767975,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 131.784,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 276.336
               ));

            pluto = new SolarSystemBody(
                1187 * 1000,
                1187 * 1000,
                1.303E22,
                6.387230 * oneDay * allowRotation,
                Vector3d.Up,
                new NoAtmosphereModel(),
                Orbit.New(
                   semiMajorAxis: 5915E9,
                   eccentricity: 0.24905,
                   inclination: MathUtild.Deg2Rad * 17.1405,
                   longitudeOfAscendingNode: MathUtild.Deg2Rad * 110.299,
                   argumentOfPeriapsis: MathUtild.Deg2Rad * 113.834
               ));
        }

        /// <summary>
        /// Returns the sun
        /// </summary>
        public static SolarSystemBody Sun
        {
            get { return sun; }
        }

        /// <summary>
        /// Returns the planet Mercury
        /// </summary>
        public static SolarSystemBody Mercury
        {
            get { return mercury; }
        }

        /// <summary>
        /// Returns the planet Venus
        /// </summary>
        public static SolarSystemBody Venus
        {
            get { return venus; }
        }

        /// <summary>
        /// Returns the planet Earth
        /// </summary>
        public static SolarSystemBody Earth
        {
            get { return earth; }
        }

        /// <summary>
        /// Returns the earth's moon
        /// </summary>
        public static SolarSystemBody Moon
        {
            get { return moon; }
        }

        /// <summary>
        /// Returns the planet Mars
        /// </summary>
        public static SolarSystemBody Mars
        {
            get { return mars; }
        }

        /// <summary>
        /// Returns the planet Jupiter
        /// </summary>
        public static SolarSystemBody Jupiter
        {
            get { return jupiter; }
        }

        /// <summary>
        /// Returns the planet Saturn
        /// </summary>
        public static SolarSystemBody Saturn
        {
            get { return saturn; }
        }

        /// <summary>
        /// Returns the planet Uranus
        /// </summary>
        public static SolarSystemBody Uranus
        {
            get { return uranus; }
        }

        /// <summary>
        /// Returns the planet Neptune
        /// </summary>
        public static SolarSystemBody Neptune
        {
            get { return neptune; }
        }

        /// <summary>
        /// Returns the planet Pluto
        /// </summary>
        public static SolarSystemBody Pluto
        {
            get { return pluto; }
        }
    }
}
