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
    /// Represents a position in an orbit
    /// </summary>
    public struct OrbitPosition
    {
        /// <summary>
        /// The orbit
        /// </summary>
        public Orbit Orbit { get; }

        /// <summary>
        /// The true anomaly
        /// </summary>
        public double TrueAnomaly { get; set; }

        /// <summary>
        /// Creates a new position in an orbit
        /// </summary>
        /// <param name="orbit">The orbit</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        public OrbitPosition(Orbit orbit, double trueAnomaly)
        {
            this.Orbit = orbit;
            this.TrueAnomaly = trueAnomaly;
        }

        /// <summary>
        /// Returns the eccentric anomaly.
        /// </summary>
        /// <remarks>This value is only defined for elliptical orbits</remarks>
        public double EccentricAnomaly
        {
            get
            {
                if (this.Orbit.IsBound)
                {
                    return OrbitFormulas.EccentricAnomaly(this.Orbit.Eccentricity, this.TrueAnomaly);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Returns the hyperbolic eccentric anomaly.
        /// </summary>
        /// <remarks>This value is only defined for hyperbolic orbits</remarks>
        public double HyperbolicEccentricAnomaly
        {
            get
            {
                if (this.Orbit.IsHyperbolic)
                {
                    return OrbitFormulas.HyperbolicEccentricAnomaly(this.Orbit.Eccentricity, this.TrueAnomaly);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Returns the parabolic eccentric anomaly.
        /// </summary>
        /// <remarks>This value is only defined for parabolic orbits</remarks>
        public double ParabolicEccentricAnomaly
        {
            get
            {
                if (this.Orbit.IsParabolic)
                {
                    return OrbitFormulas.ParabolicEccentricAnomaly(this.TrueAnomaly);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Calculates the time until the orbit has the given true anomaly for an elliptical orbit
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        private double TimeToTrueAnomalyElliptical(double trueAnomaly)
        {
            var e = this.Orbit.Eccentricity;
            var E = OrbitFormulas.EccentricAnomaly(e, this.TrueAnomaly);
            var targetE = OrbitFormulas.EccentricAnomaly(e, trueAnomaly);
            var mu = this.Orbit.PrimaryBody.StandardGravitationalParameter;
            var a = this.Orbit.SemiMajorAxis;

            var factor = Math.Sqrt(Math.Pow(a, 3) / mu);
            var relativeTime = factor * (targetE - e * Math.Sin(targetE));
            var timeToTrueAnomaly = relativeTime - factor * (E - e * Math.Sin(E));

            //This means that we have passed the angle, add the period
            if (timeToTrueAnomaly < 0)
            {
                timeToTrueAnomaly += 2.0 * Math.PI * factor;
            }

            return timeToTrueAnomaly;
        }

        /// <summary>
        /// Calculates the time until the orbit has the given true anomaly for a hyperbolic orbit
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        private double TimeToTrueAnomalyHyperbolic(double trueAnomaly)
        {
            var e = this.Orbit.Eccentricity;
            var F = OrbitFormulas.HyperbolicEccentricAnomaly(e, this.TrueAnomaly);
            var targetF = OrbitFormulas.HyperbolicEccentricAnomaly(e, trueAnomaly);
            var mu = this.Orbit.PrimaryBody.StandardGravitationalParameter;
            var a = this.Orbit.SemiMajorAxis;

            var factor = Math.Sqrt(Math.Pow(-a, 3) / mu);
            var relativeTime = factor * (e * Math.Sinh(targetF) - targetF);
            return relativeTime - factor * (e * Math.Sinh(F) - F);
        }

        /// <summary>
        /// Calculates the time until the orbit has the given true anomaly for a parabolic orbit
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        private double TimeToTrueAnomalyParabolic(double trueAnomaly)
        {
            var p = this.Orbit.Parameter;
            var sqrtP = Math.Sqrt(p);
            var D = sqrtP * OrbitFormulas.ParabolicEccentricAnomaly(this.TrueAnomaly);
            var targetD = sqrtP * OrbitFormulas.ParabolicEccentricAnomaly(trueAnomaly);
            var mu = this.Orbit.PrimaryBody.StandardGravitationalParameter;

            var factor = 1.0 / (2.0 * Math.Sqrt(mu));
            var relativeTime = factor * (p * targetD + (1.0 / 3.0) * Math.Pow(targetD, 3));
            return relativeTime - factor * (p * D + (1.0 / 3.0) * Math.Pow(D, 3));
        }

        /// <summary>
        /// Calculates the time to the given true anomaly
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly</param>
        public double TimeToTrueAnomaly(double trueAnomaly)
        {
            if (this.Orbit.IsBound)
            {
                return TimeToTrueAnomalyElliptical(trueAnomaly);
            }
            else if (this.Orbit.IsParabolic)
            {
                return TimeToTrueAnomalyParabolic(trueAnomaly);
            }
            else if (this.Orbit.IsHyperbolic)
            {
                return TimeToTrueAnomalyHyperbolic(trueAnomaly);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calclates the time to periapsis
        /// </summary>
        public double TimeToPeriapsis()
        {
            return TimeToTrueAnomaly(2.0 * Math.PI);
        }

        /// <summary>
        /// Calclates the time to apoapsis
        /// </summary>
        public double TimeToApoapsis()
        {
            if (this.Orbit.IsBound)
            {
                return TimeToTrueAnomalyElliptical(Math.PI);
            }
            else
            {
                return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Adds the given values, creating a new orbit position
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        /// <returns>A new orbit position with the added values</returns>
        public OrbitPosition Add(
            double parameter = 0.0,
            double eccentricity = 0.0,
            double inclination = 0.0,
            double longitudeOfAscendingNode = 0.0,
            double argumentOfPeriapsis = 0.0,
            double trueAnomaly = 0.0)
        {
            var newOrbit = Orbit.Add(parameter, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis);
            return new OrbitPosition(newOrbit, this.TrueAnomaly + trueAnomaly);
        }

        /// <summary>
        /// Sets the given values, creating a new orbit position
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="eccentricity">The eccentricity</param>
        /// <param name="inclination">The inclination</param>
        /// <param name="longitudeOfAscendingNode">The longitude of the ascending node</param>
        /// <param name="argumentOfPeriapsis">The argument of periapsis</param>
        /// <param name="trueAnomaly">The true anomaly</param>
        /// <returns>A new orbit position with the none null values set</returns>
        public OrbitPosition Set(
            double? parameter = null,
            double? eccentricity = null,
            double? inclination = null,
            double? longitudeOfAscendingNode = null,
            double? argumentOfPeriapsis = null,
            double? trueAnomaly = null)
        {
            var newOrbit = Orbit.Set(parameter, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis);
            return new OrbitPosition(newOrbit, trueAnomaly ?? this.TrueAnomaly);
        }

        /// <summary>
        /// Calculates the state of position
        /// </summary>
        public ObjectState CalculateState()
        {
            var primaryBodyState = this.Orbit.PrimaryBody.State;
            return this.Orbit.CalculateState(this.TrueAnomaly, ref primaryBodyState);
        }

        /// <summary>
        /// Calculates the state of position
        /// </summary>
        /// <param name="primaryBodyState">The state of the primary body</param>
        public ObjectState CalculateState(ref ObjectState primaryBodyState)
        {
            return this.Orbit.CalculateState(this.TrueAnomaly, ref primaryBodyState);
        }

        /// <summary>
        /// Calculates the orbit position from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="state">The state of the object</param>
        public static OrbitPosition CalculateOrbitPosition(IPrimaryBodyObject primaryBody, ref ObjectState primaryBodyState, ref ObjectState state)
        {
            var r = MathHelpers.SwapYZ(state.Position - primaryBodyState.Position);
            var v = MathHelpers.SwapYZ(state.Velocity - primaryBodyState.Velocity);
            var mu = primaryBody.StandardGravitationalParameter;

            var h = Vector3d.Cross(r, v);
            var e = (1 / mu) * ((v.LengthSquared() - (mu / r.Length())) * r - (Vector3d.Dot(r, v) * v));
            var n = Vector3d.Cross(MathHelpers.SwapYZ(Vector3d.Up), h);

            var parameter = h.LengthSquared() / mu;
            var eccentricity = e.Length();
            var inclination = Math.Acos(h.Z / h.Length());
            var nonEquatorial = inclination > 0 && Math.Abs(inclination - Math.PI) > 1E-4;
            var longitudeOfAscendingNode = 0.0;
            var argumentOfPeriapsis = 0.0;
            var isCircular = eccentricity <= Orbit.EccentricityEpsilon;
            var trueAnomaly = 0.0;

            if (nonEquatorial)
            {
                longitudeOfAscendingNode = Math.Acos(n.X / n.Length());

                if (n.Y < 0 && longitudeOfAscendingNode != 0)
                {
                    longitudeOfAscendingNode = 2 * Math.PI - longitudeOfAscendingNode;
                }
            }

            if (nonEquatorial)
            {
                argumentOfPeriapsis = Math.Acos(Vector3d.Dot(n, e) / (n.Length() * e.Length()));
                if (e.Z < 0 && argumentOfPeriapsis != 0)
                {
                    argumentOfPeriapsis = 2 * Math.PI - argumentOfPeriapsis;
                }
            }
            else if (!isCircular)
            {
                argumentOfPeriapsis = Math.Atan2(e.Y, e.X);
                if (Vector3d.Cross(r, v).Z < 0)
                {
                    argumentOfPeriapsis = 2.0 * Math.PI - argumentOfPeriapsis;
                }

                //Probably bug why we get negative angle
                if (argumentOfPeriapsis < 0)
                {
                    argumentOfPeriapsis += 2.0 * Math.PI;
                }
            }

            if (isCircular)
            {
                if (inclination == 0)
                {
                    trueAnomaly = Math.Acos(r.X / r.Length());
                    if (v.X > 0)
                    {
                        trueAnomaly = 2 * Math.PI - trueAnomaly;
                    }
                }
                else
                {
                    trueAnomaly = Math.Acos(Vector3d.Dot(n, r) / (n.Length() * r.Length()));
                    if (r.Y < 0)
                    {
                        trueAnomaly = 2 * Math.PI - trueAnomaly;
                    }
                }
            }
            else
            {
                trueAnomaly = Math.Acos(Vector3d.Dot(e, r) / (e.Length() * r.Length()));
                if (Vector3d.Dot(r, v) < 0)
                {
                    trueAnomaly = 2 * Math.PI - trueAnomaly;
                }

                if (double.IsNaN(trueAnomaly))
                {
                    trueAnomaly = 0.0;
                }
            }

            var orbit = new Orbit(primaryBody, parameter, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis);
            return new OrbitPosition(orbit, trueAnomaly);
        }

        /// <summary>
        /// Calculates the orbit position from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="state">The state of the object</param>
        public static OrbitPosition CalculateOrbitPosition(IPrimaryBodyObject primaryBody, ref ObjectState state)
        {
            var primaryBodyState = primaryBody.State;
            return CalculateOrbitPosition(primaryBody, ref primaryBodyState, ref state);
        }

        /// <summary>
        /// Calculates the orbit position from the given state
        /// </summary>
        /// <param name="primaryBody">The object the orbit is around</param>
        /// <param name="state">The state of the object</param>
        public static OrbitPosition CalculateOrbitPosition(IPrimaryBodyObject primaryBody, ObjectState state)
        {
            var primaryBodyState = primaryBody.State;
            return CalculateOrbitPosition(primaryBody, ref primaryBodyState, ref state);
        }

        /// <summary>
        /// Calculates the orbit position for the given object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public static OrbitPosition CalculateOrbitPosition(IPhysicsObject physicsObject)
        {
            var primaryBody = physicsObject.PrimaryBody;
            var primaryBodyState = primaryBody.State;
            var state = physicsObject.State;
            return CalculateOrbitPosition(primaryBody, ref primaryBodyState, ref state);
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
                DataFormatter.Format(this.Orbit.Parameter, DataUnit.Distance),
                DataFormatter.Format(this.Orbit.Eccentricity, DataUnit.NoUnit),
                DataFormatter.Format(this.Orbit.Inclination, DataUnit.Angle),
                DataFormatter.Format(this.Orbit.LongitudeOfAscendingNode, DataUnit.Angle),
                DataFormatter.Format(this.Orbit.ArgumentOfPeriapsis, DataUnit.Angle),
                DataFormatter.Format(this.TrueAnomaly, DataUnit.Angle)
            };

            if (!extended)
            {
                format = "{{ p: {0}, e: {1}, i: {2}, Ω: {3}, ω: {4}, ν: {5}}}";
            }
            else
            {
                format = "{{ p: {0}, e: {1}, i: {2}, Ω: {3}, ω: {4}, ν: {5}, rp: {6}, ra: {7}, T: {8}}}";
                args.Add(DataFormatter.Format(this.Orbit.Periapsis, DataUnit.Distance));
                args.Add(DataFormatter.Format(this.Orbit.Apoapsis, DataUnit.Distance));

                if (this.Orbit.IsBound)
                {
                    args.Add(DataFormatter.Format(this.Orbit.Period, DataUnit.Time));
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
