using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// The type of the orbit
    /// </summary>
    public enum OrbitType
    {
        Circular,
        Elliptical,
        Parabolic,
        Hyperbolic
    }

    /// <summary>
    /// Represents an orbit
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public sealed class Orbit
    {
        /// <summary>
        /// The p-value (semi-latus rectum)
        /// </summary>
        public double Parameter { get; }

        /// <summary>
        /// The eccentricity
        /// </summary>
        public double Eccentricity { get; }

        /// <summary>
        /// The inclination
        /// </summary>
        public double Inclination { get; }

        /// <summary>
        /// The longitude of the ascending node
        /// </summary>
        public double LongitudeOfAscendingNode { get; }

        /// <summary>
        /// The argument of periapsis
        /// </summary>
        public double ArgumentOfPeriapsis { get; }

        /// <summary>
        /// The primary body
        /// </summary>
        public IPrimaryBodyObject PrimaryBody { get; }

        /// <summary>
        /// The epsilon used for eccentricity calcuations
        /// </summary>
        public const double EccentricityEpsilon = 1E-4;

        /// <summary>
        /// Creates an empty orbit (e.g. orbit of the reference object)
        /// </summary>
        public Orbit()
        {

        }

        /// <summary>
        /// Creates a new orbit
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="parameter">The p-value (semi-latus rectum)</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        public Orbit(IPrimaryBodyObject primaryBody, double parameter, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis)
        {
            this.PrimaryBody = primaryBody;
            this.Parameter = parameter;
            this.Eccentricity = eccentricity;
            this.Inclination = inclination;
            this.LongitudeOfAscendingNode = longitudeOfAscendingNode;
            this.ArgumentOfPeriapsis = argumentOfPeriapsis;
        }

        /// <summary>
        /// Creates a new orbit
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="parameter">The semi-latis rectum</param>
        /// <param name="semiMajorAxis">The semi-major axis</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        /// <remarks>If both the parameter and semi-major axis is set, then the semi-major axis is used to calculate the parameter.</remarks>
        public static Orbit New(
            IPrimaryBodyObject primaryBody = null,
            double parameter = 0,
            double semiMajorAxis = 0,
            double eccentricity = 0,
            double inclination = 0,
            double longitudeOfAscendingNode = 0,
            double argumentOfPeriapsis = 0)
        {
            if (semiMajorAxis != 0)
            {
                parameter = OrbitFormulas.ParameterFromSemiMajorAxis(semiMajorAxis, eccentricity);
            }

            return new Orbit(primaryBody, parameter, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis);
        }

        /// <summary>
        /// Adds the given values, creating a new orbit
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        /// <returns>A new orbit with the added values</returns>
        public Orbit Add(double parameter = 0.0, double eccentricity = 0.0, double inclination = 0.0, double longitudeOfAscendingNode = 0.0, double argumentOfPeriapsis = 0.0)
        {
            return new Orbit(
                this.PrimaryBody,
                this.Parameter + parameter,
                this.Eccentricity + eccentricity,
                this.Inclination + inclination,
                this.LongitudeOfAscendingNode + longitudeOfAscendingNode,
                this.ArgumentOfPeriapsis + argumentOfPeriapsis);
        }

        /// <summary>
        /// Sets the given values, creating a new orbit
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        /// <returns>A new orbit with the none null values set</returns>
        public Orbit Set(double? parameter = null, double? eccentricity = null, double? inclination = null, double? longitudeOfAscendingNode = null, double? argumentOfPeriapsis = null)
        {
            return new Orbit(
                this.PrimaryBody,
                parameter ?? this.Parameter,
                eccentricity ?? this.Eccentricity,
                inclination ?? this.Inclination,
                longitudeOfAscendingNode ?? this.LongitudeOfAscendingNode,
                argumentOfPeriapsis ?? this.ArgumentOfPeriapsis);
        }

        /// <summary>
        /// Returns the standard gravitational parameter (mu)
        /// </summary>
        public double StandardGravitationalParameter
        {
            get
            {
                if (this.PrimaryBody == null)
                {
                    return 0;
                }

                return this.PrimaryBody.StandardGravitationalParameter;
            }
        }

        /// <summary>
        /// Returns the type of the orbit
        /// </summary>
        public OrbitType Type
        {
            get
            {
                if (this.IsCircular)
                {
                    return OrbitType.Circular;
                }
                else if (this.IsElliptical)
                {
                    return OrbitType.Elliptical;
                }
                else if (this.IsParabolic)
                {
                    return OrbitType.Parabolic;
                }
                else
                {
                    return OrbitType.Hyperbolic;
                }
            }
        }

        /// <summary>
        /// Returns the periapsis
        /// </summary>
        public double Periapsis => this.Parameter / (1 + this.Eccentricity);

        /// <summary>
        /// Returns the apoapsis
        /// </summary>
        public double Apoapsis
        {
            get
            {
                if (this.IsUnbound)
                {
                    return double.PositiveInfinity;
                }

                return this.Parameter / (1 - this.Eccentricity);
            }
        }

        /// <summary>
        /// Returns the periapsis relative to the surface
        /// </summary>
        public double RelativePeriapsis => this.Periapsis - this.PrimaryBody.Radius;

        /// <summary>
        /// Returns the apoapsis relative to the surface
        /// </summary>
        public double RelativeApoapsis => this.Apoapsis - this.PrimaryBody.Radius;

        /// <summary>
        /// Returns the semi-major axis
        /// </summary>
        public double SemiMajorAxis
        {
            get
            {
                if (this.IsHyperbolic || this.IsBound)
                {
                    var e = this.Eccentricity;
                    return this.Parameter / (1 - e * e);
                }
                else
                {
                    return double.PositiveInfinity;
                }
            }
        }

        /// <summary>
        /// Indicates if the current orbit is circular
        /// </summary>
        public bool IsCircular => this.Eccentricity <= EccentricityEpsilon;

        /// <summary>
        /// Indicates if the current orbit is elliptical
        /// </summary>
        public bool IsElliptical => this.Eccentricity > EccentricityEpsilon && this.Eccentricity < 1.0 - EccentricityEpsilon;

        /// <summary>
        /// Indicates if the current orbit is parabolic
        /// </summary>
        public bool IsParabolic => Math.Abs(this.Eccentricity - 1.0) <= EccentricityEpsilon;

        /// <summary>
        /// Indicates if the current orbit is hyperbolic
        /// </summary>
        public bool IsHyperbolic => this.Eccentricity > 1.0 + EccentricityEpsilon;

        /// <summary>
        /// Indicates if the current orbit is bound (circular or elliptical)
        /// </summary>
        public bool IsBound => this.Eccentricity < 1.0 - EccentricityEpsilon;

        /// <summary>
        /// Indicates if the orbit is unbound (parabolic or hyperbolic)
        /// </summary>
        public bool IsUnbound => !this.IsBound;

        /// <summary>
        /// Indicates if the current orbit is a radial parabolic
        /// </summary>
        public bool IsRadialParabolic => this.Parameter <= 1E-5 && this.IsParabolic;

        /// <summary>
        /// Returns the period
        /// </double>
        public double Period => OrbitFormulas.OrbitalPeriod(this.StandardGravitationalParameter, this.SemiMajorAxis);

        /// <summary>
        /// Returns the change-of-basis matrix from the perifocal coordinate system to the geocentric-equatorial
        /// </summary>
        public Matrix3x3d ChangeOfBasisMatrix
        {
            get
            {
                var longitudeOfAscendingNode = this.LongitudeOfAscendingNode;
                var argumentOfPeriapsis = this.ArgumentOfPeriapsis;

                if (double.IsNaN(longitudeOfAscendingNode))
                {
                    longitudeOfAscendingNode = 0;
                }

                if (double.IsNaN(argumentOfPeriapsis) || this.IsCircular)
                {
                    argumentOfPeriapsis = 0;
                }

                var cosOmega = Math.Cos(longitudeOfAscendingNode);
                var sinOmega = Math.Sin(longitudeOfAscendingNode);
                var cosArgumentOfPeriapsis = Math.Cos(argumentOfPeriapsis);
                var sinArgumentOfPeriapsis = Math.Sin(argumentOfPeriapsis);
                var cosInclination = Math.Cos(this.Inclination);
                var sinInclination = Math.Sin(this.Inclination);

                var R = new Matrix3x3d();
                R[0, 0] = cosOmega * cosArgumentOfPeriapsis - sinOmega * sinArgumentOfPeriapsis * cosInclination;
                R[0, 1] = -cosOmega * sinArgumentOfPeriapsis - sinOmega * cosArgumentOfPeriapsis * cosInclination;
                R[0, 2] = sinOmega * sinInclination;

                R[1, 0] = sinOmega * cosArgumentOfPeriapsis + cosOmega * sinArgumentOfPeriapsis * cosInclination;
                R[1, 1] = -sinOmega * sinArgumentOfPeriapsis + cosOmega * cosArgumentOfPeriapsis * cosInclination;
                R[1, 2] = -cosOmega * sinInclination;

                R[2, 0] = sinArgumentOfPeriapsis * sinInclination;
                R[2, 1] = cosArgumentOfPeriapsis * sinInclination;
                R[2, 2] = cosInclination;

                return R;
            }
        }

        /// <summary>
        /// Calculates the state vectors (position and velocity) from the current orbit for the given true anomaly
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        public ObjectState CalculateState(double trueAnomaly, ref ObjectState primaryBodyState)
        {
            var R = this.ChangeOfBasisMatrix;
            var P = Vector3d.Right;
            var Q = Vector3d.Up;

            var cosTrueAnomaly = Math.Cos(trueAnomaly);
            var sinTrueAnomaly = Math.Sin(trueAnomaly);

            var dist = this.Parameter / (1 + this.Eccentricity * cosTrueAnomaly);
            var r = dist * cosTrueAnomaly * P + dist * sinTrueAnomaly * Q;
            var v = Math.Sqrt(this.StandardGravitationalParameter / this.Parameter)
                        * (-sinTrueAnomaly * P + (this.Eccentricity + cosTrueAnomaly) * Q);

            var radius = primaryBodyState.Position + MathHelpers.SwapYZ(R * r);
            var velocity = primaryBodyState.Velocity + MathHelpers.SwapYZ(R * v);

            return new ObjectState(
                primaryBodyState.Time,
                radius,
                velocity);
        }

        /// <summary>
        /// Calculates the state vectors (position and velocity) from the current orbit for the given true anomaly
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        public ObjectState CalculateState(double trueAnomaly, ObjectState primaryBodyState)
        {
            return CalculateState(trueAnomaly, ref primaryBodyState);
        }

        /// <summary>
        /// Calculates the state vectors (position and velocity) from the current orbit for the given true anomaly
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        public ObjectState CalculateState(double trueAnomaly)
        {
            var primaryBodyState = this.PrimaryBody.State;
            return CalculateState(trueAnomaly, ref primaryBodyState);
        }

        /// <summary>
        /// Calculates the orbit from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="state">The state of the object</param>
        /// <remarks>This does not calculate the true anomaly (position in the orbit), see <see cref="SpaceSimulator.Physics.OrbitPosition.CalculateOrbitPosition"/>.</remarks>
        public static Orbit CalculateOrbit(IPrimaryBodyObject primaryBody, ref ObjectState primaryBodyState, ref ObjectState state)
        {
            return OrbitPosition.CalculateOrbitPosition(primaryBody, ref primaryBodyState, ref state).Orbit;
        }

        /// <summary>
        /// Calculates the orbit from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="state">The state of the object</param>
        public static Orbit CalculateOrbit(IPrimaryBodyObject primaryBody, ObjectState state)
        {
            return CalculateOrbit(primaryBody, ref state);
        }

        /// <summary>
        /// Calculates the orbit from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="state">The state of the object</param>
        public static Orbit CalculateOrbit(IPrimaryBodyObject primaryBody, ref ObjectState state)
        {
            var primaryBodyState = primaryBody.State;
            return CalculateOrbit(primaryBody, ref primaryBodyState, ref state);
        }

        /// <summary>
        /// Calculates the orbit for the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        public static Orbit CalculateOrbit(IPhysicsObject physicsObject)
        {
            var primaryBodyState = physicsObject.PrimaryBody.State;
            var state = physicsObject.State;
            return CalculateOrbit(physicsObject.PrimaryBody, ref primaryBodyState, ref state);
        }

        /// <summary>
        /// Compares if the current orbit is equal to the given orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="epsilon">The epsilon</param>
        public bool SameOrbit(ref Orbit orbit, double epsilon = 1E-4)
        {
            if (Math.Abs(this.Parameter - orbit.Parameter) > epsilon)
            {
                return false;
            }

            if (Math.Abs(this.Eccentricity - orbit.Eccentricity) > epsilon)
            {
                return false;
            }

            if (Math.Abs(this.Inclination - orbit.Inclination) > epsilon)
            {
                return false;
            }

            if (Math.Abs(this.LongitudeOfAscendingNode - orbit.LongitudeOfAscendingNode) > epsilon)
            {
                return false;
            }

            if (Math.Abs(this.ArgumentOfPeriapsis - orbit.ArgumentOfPeriapsis) > epsilon)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the given orbit lies in the same plane as the current one
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="epsilon">The epsilon</param>
        public bool SamePlane(ref Orbit orbit, double epsilon = 1E-4)
        {
            if (double.IsNaN(this.LongitudeOfAscendingNode) && double.IsNaN(orbit.LongitudeOfAscendingNode))
            {
                return Math.Abs(this.Inclination - orbit.Inclination) <= epsilon;
            }

            return Math.Abs(this.Inclination - orbit.Inclination) <= epsilon
                   && Math.Abs(this.LongitudeOfAscendingNode - orbit.LongitudeOfAscendingNode) <= epsilon;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Returns a string representation of the orbit
        /// </summary>
        /// <param name="extended">Indicates if more information is included</param>
        public string ToString(bool extended)
        {
            var format = "";
            var args = new List<object>()
            {
                DataFormatter.Format(this.Parameter, DataUnit.Distance),
                DataFormatter.Format(this.Eccentricity, DataUnit.NoUnit),
                DataFormatter.Format(this.Inclination, DataUnit.Angle),
                DataFormatter.Format(this.LongitudeOfAscendingNode, DataUnit.Angle),
                DataFormatter.Format(this.ArgumentOfPeriapsis, DataUnit.Angle),
            };

            if (!extended)
            {
                format = "{{ p: {0}, e: {1}, i: {2}, Ω: {3}, ω: {4}, ν: {5}}}";
            }
            else
            {
                format = "{{ p: {0}, e: {1}, i: {2}, Ω: {3}, ω: {4}, ν: {5}, rp: {6}, ra: {7}, T: {8}}}";
                args.Add(DataFormatter.Format(this.Periapsis, DataUnit.Distance));
                args.Add(DataFormatter.Format(this.Apoapsis, DataUnit.Distance));

                if (this.IsBound)
                {
                    args.Add(DataFormatter.Format(this.Period, DataUnit.Time));
                }
                else
                {
                    args.Add("NaN");
                }
            }

            return string.Format(format, args.ToArray());
        }
    }
}
