using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains formulas for orbits
    /// </summary>
    public static class OrbitFormulas
    {
        /// <summary>
        /// Computes the parameter based on the semi-major axis and the eccentricity 
        /// </summary>
        /// <param name="semiMajorAxis">The semi-major axis</param>
        /// <param name="eccentricity">The eccentricity</param>
        public static double ParameterFromSemiMajorAxis(double semiMajorAxis, double eccentricity)
        {
            return semiMajorAxis * (1 - eccentricity * eccentricity);
        }

        /// <summary>
        /// Returns the gravity force applied on object 2 by object 1
        /// </summary>
        /// <param name="r1">The position of the first object</param>
        /// <param name="m1">The mass of the first object</param>
        /// <param name="r2">The position of the second object</param>
        /// <param name="m2">The mass of the second object</param>
        public static Vector3d GravityForce(Vector3d r1, double m1, Vector3d r2, double m2)
        {
            var r = r2 - r1;
            var normSquared = r.LengthSquared();
            if (normSquared == 0)
            {
                return r;
            }

            return -Constants.G * ((m1 * m2) * MathHelpers.Normalized(r)) / normSquared;
        }

        /// <summary>
        /// Returns the acceleration due to gravity
        /// </summary>
        /// <param name="standardGravitationalParameter">The standard gravitational parameter of the other object</param>
        /// <param name="r">The distance between the centers</param>
        public static Vector3d GravityAcceleration(double standardGravitationalParameter, Vector3d r)
        {
            var normSquared = r.LengthSquared();
            return -(standardGravitationalParameter * r) / (normSquared * Math.Sqrt(normSquared));
        }

        /// <summary>
        /// Returns the initial orbital speed of object 2 around object 1
        /// </summary>
        /// <param name="position1">The position of the first object</param>
        /// <param name="mass1">The mass of the first object</param>
        /// <param name="position2">The position of the second object</param>
        /// <param name="mass2">The mass of the second object</param>
        public static double OrbitalSpeed(Vector3d position1, double mass1, Vector3d position2, double mass2)
        {
            return Math.Sqrt((Constants.G * (mass1 + mass2)) / (position2 - position1).Length());
        }

        /// <summary>
        /// Returns the orbital period of the an object with the given semi-major axis
        /// </summary>
        /// <param name="standardGravitationalParameter">The standard gravitational parameter of the object being orbited</param>
        /// <param name="semiMajorAxis">The semi-major axis</param>
        public static double OrbitalPeriod(double standardGravitationalParameter, double semiMajorAxis)
        {
            return ((2 * Math.PI) / Math.Sqrt(standardGravitationalParameter)) * Math.Pow(semiMajorAxis, 3.0 / 2.0);
        }

        /// <summary>
        /// Calculates the synodic period between the given orbits
        /// </summary>
        /// <param name="orbitalPeriod1">The period of the first object</param>
        /// <param name="orbitalPeriod2">The period of the second object</param>
        public static double SynodicPeriod(double orbitalPeriod1, double orbitalPeriod2)
        {
            var P1 = orbitalPeriod1;
            var P2 = orbitalPeriod2;

            if (Math.Abs(P1 - P2) <= 1E-2)
            {
                return 0;
            }

            if (!(P1 < P2))
            {
                P1 = orbitalPeriod2;
                P2 = orbitalPeriod1;
            }

            return 1.0 / ((1.0 / P1) - (1.0 / P2));
        }

        /// <summary>
        /// Returns the semi-major axis with the given orbital period
        /// </summary>
        /// <param name="standardGravitationalParameter">The standard gravitational parameter of the object being orbited</param>
        /// <param name="period">The orbital period</param>
        public static double SemiMajorAxisFromOrbitalPeriod(double standardGravitationalParameter, double period)
        {
            return Math.Pow(
                (standardGravitationalParameter * period * period) / (4 * Math.PI * Math.PI),
                1.0 / 3.0);
        }

        /// <summary>
        /// Calculates the sphere of influence between the two objects
        /// </summary>
        /// <param name="semiMajorAxis">The semi-major axis for the smaller object around the larger</param>
        /// <param name="massSmaller">The mass of the smaller</param>
        /// <param name="massLarger">The mass of the larger</param>
        public static double SphereOfInfluence(double semiMajorAxis, double massSmaller, double massLarger)
        {
            return semiMajorAxis * Math.Pow(massSmaller / massLarger, 2.0 / 5.0);
        }

        /// <summary>
        /// Calculates the true anomaly at the given distance
        /// </summary>
        /// <param name="distance">The distance</param>
        /// <param name="parameter">The parameter (semi-latus rectum) of the orbit</param>
        /// <param name="eccentricity">The eccentricity of the orbit</param>
        /// <param name="trueAnomaly1">The first solution</param>
        /// <param name="trueAnomaly2">The second solution</param>
        /// <returns>True if solutions exists</returns>  
        public static bool TrueAnomalyAt(
            double distance, double parameter, double eccentricity,
            out double trueAnomaly1, out double trueAnomaly2)
        {
            var cosTrueAnomaly = (parameter - distance) / (eccentricity * distance);
            var trueAnomaly = Math.Acos(cosTrueAnomaly);

            //No solutions
            if (double.IsNaN(trueAnomaly))
            {
                trueAnomaly1 = 0;
                trueAnomaly2 = 0;
                return false;
            }

            trueAnomaly1 = MathHelpers.ClampAngle(trueAnomaly);
            trueAnomaly2 = MathHelpers.ClampAngle(-trueAnomaly);
            return true;
        }

        /// <summary>
        /// Returns the angular velocity at the given distance from the center
        /// </summary>
        /// <param name="semiMajorAxis">The semi-major axis of the oribt</param>
        /// <param name="eccentricity">The eccentricity of the orbit</param>
        /// <param name="period">The period of the orbit</param>
        /// <param name="distance">The distance</param>
        public static double AngularVelocity(double semiMajorAxis, double eccentricity, double period, double distance)
        {
            if (eccentricity < 1E-6)
            {
                return (2.0 * Math.PI) / period;
            }

            var semiMinorAxis = semiMajorAxis * Math.Sqrt(1 - eccentricity * eccentricity);
            return (2.0 * Math.PI * semiMajorAxis * semiMinorAxis) / (period * distance * distance);
        }

        /// <summary>
        /// Calculates the eccentric anomaly from the true anomaly
        /// </summary>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        public static double EccentricAnomaly(double eccentricity, double trueAnomaly)
        {
            var cosTrueAnomaly = Math.Cos(trueAnomaly);
            var E = Math.Acos((eccentricity + cosTrueAnomaly) / (1 + eccentricity * cosTrueAnomaly));
            if (trueAnomaly > Math.PI)
            {
                E = 2.0 * Math.PI - E;
            }

            return E;
        }

        /// <summary>
        /// Calculates the hyperbolic eccentric anomaly from the true anomaly
        /// </summary>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        public static double HyperbolicEccentricAnomaly(double eccentricity, double trueAnomaly)
        {
            var cosTrueAnomaly = Math.Cos(trueAnomaly);
            var F = MathHelpers.Acosh((eccentricity + cosTrueAnomaly) / (1 + eccentricity * cosTrueAnomaly));
            if (trueAnomaly >= Math.PI && trueAnomaly <= 2.0 * Math.PI)
            {
                F *= -1;
            }

            return F;
        }

        /// <summary>
        /// Calculates the parabolic eccentric anomaly from the true anomaly
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        public static double ParabolicEccentricAnomaly(double trueAnomaly)
        {
            return Math.Tan(trueAnomaly / 2.0);
        }

        /// <summary>
        /// Calculates the mean anomaly (using Kepler's equation)
        /// </summary>
        /// <param name="eccentricity">The eccentricity of the orbit</param>
        /// <param name="eccentricAnomaly">The eccentricity anomaly</param>
        public static double MeanAnomaly(double eccentricity, double eccentricAnomaly)
        {
            return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
        }

        /// <summary>
        /// Calculates the true anomaly from the eccentric anomaly
        /// </summary>
        /// <param name="eccentricity">The eccentricity of the orbit</param>
        /// <param name="eccentricAnomaly">The eccentricity anomaly</param>
        public static double TrueAnomalyFromEccentricAnomaly(double eccentricity, double eccentricAnomaly)
        {
            return 2.0 * Math.Atan2(
                Math.Sqrt(1 + eccentricity) * Math.Sin(eccentricAnomaly / 2.0),
                Math.Sqrt(1 - eccentricity) * Math.Cos(eccentricAnomaly / 2.0));
        }

        /// <summary>
        /// Calculates the altitude over the given primary body
        /// </summary>
        /// <param name="primaryBodyPosition">The position of the primary body</param>
        /// <param name="primaryBodyRadius">The radius of the primary body</param>
        /// <param name="objectPosition">The position of the object</param>
        public static double Altitude(Vector3d primaryBodyPosition, double primaryBodyRadius, Vector3d objectPosition)
        {
             return (objectPosition - primaryBodyPosition).Length() - primaryBodyRadius;
        }
    }
}
