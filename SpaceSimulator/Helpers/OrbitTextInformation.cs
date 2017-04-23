using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Contain methods for creating text information about orbit
    /// </summary>
    public static class OrbitTextInformation
    {
        /// <summary>
        /// Returns the full information for the given orbit
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="state">The state</param>
        /// <param name="orbit">The orbit and the position in it</param>
        /// <param name="physicsObject">The optional physics object</param>
        public static string FullInformation(
            PhysicsObject primaryBody,
            ObjectState state,
            OrbitPosition orbitPosition,
            PhysicsObject physicsObject = null)
        {
            var orbit = orbitPosition.Orbit;
            var infoBuilder = new StringBuilder();

            if (physicsObject != null)
            {
                infoBuilder.AppendLine($"Object: {physicsObject.Name}");
            }
            else
            {
                infoBuilder.AppendLine("Object");
            }

            if (physicsObject != null && physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
            {
                infoBuilder.AppendLine("Mass: " + DataFormatter.Format(physicsObject.Mass, DataUnit.Mass, useBase10: true));
            }

            var refPosition = Vector3d.Zero;
            var refVelocity = Vector3d.Zero;

            if (primaryBody != null)
            {
                refPosition = primaryBody.Position;
                refVelocity = primaryBody.Velocity;
            }

            if (physicsObject == null || !physicsObject.Impacted)
            {
                infoBuilder.AppendLine("Distance: " + DataFormatter.Format((state.Position - refPosition).Length(), DataUnit.Distance));
                infoBuilder.AppendLine("Speed: " + DataFormatter.Format((state.Velocity - refVelocity).Length(), DataUnit.Velocity));

                if (physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
                {
                    var relativeVelocity = state.Velocity - refVelocity;
                    var gravityAccelerationDirection = MathHelpers.Normalized(primaryBody.Position - state.Position);

                    (var horizontalSpeed, var verticalSpeed) = OrbitHelpers.ComputeHorizontalAndVerticalVelocity(gravityAccelerationDirection, relativeVelocity);

                    infoBuilder.AppendLine("Horizontal speed: " + DataFormatter.Format(horizontalSpeed, DataUnit.Velocity));
                    infoBuilder.AppendLine("Vertical speed: " + DataFormatter.Format(verticalSpeed, DataUnit.Velocity));
                }
            }

            infoBuilder.AppendLine("Acceleration: " + DataFormatter.Format(state.Acceleration.Length(), DataUnit.Acceleration));

            double latitude = 0;
            double longitide = 0;

            if (primaryBody != null)
            {
                OrbitHelpers.GetCoordinates(primaryBody, state.Position, out latitude, out longitide);

                infoBuilder.AppendLine("Latitude: " + DataFormatter.Format(latitude, DataUnit.Latitude, 2));
                infoBuilder.AppendLine("Longitude: " + DataFormatter.Format(longitide, DataUnit.Longitude, 2));
                infoBuilder.AppendLine("Altitude: " + DataFormatter.Format(primaryBody.Altitude(state.Position), DataUnit.Distance));
            }

            if (physicsObject != null && physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
            {
                infoBuilder.AppendLine("Used Δv: " + DataFormatter.Format(physicsObject.UsedDeltaV, DataUnit.Velocity));
            }

            if (physicsObject == null || !physicsObject.Impacted)
            {
                infoBuilder.AppendLine();

                infoBuilder.AppendLine("Orbit (" + orbit.Type + ")");
                if (orbit.PrimaryBody != null)
                {
                    infoBuilder.AppendLine("Primary body: " + ((PhysicsObject)orbit.PrimaryBody).Name);
                }

                var nonSIDistanceUnit = DataUnit.EarthRadii;
                if (primaryBody != null && primaryBody.Name == "Sun")
                {
                    nonSIDistanceUnit = DataUnit.AstronomicalUnits;
                }

                var primaryBodyRadius = 0.0;
                if (physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
                {
                    primaryBodyRadius = primaryBody.Configuration.Radius;
                }

                var parameter = orbit.Parameter;
                infoBuilder.AppendLine(string.Format("Semi-latus rectum: {0} ({1})",
                    DataFormatter.Format(parameter - primaryBodyRadius, DataUnit.Distance),
                    DataFormatter.Format(parameter - primaryBodyRadius, nonSIDistanceUnit)));

                if (orbit.IsElliptical || orbit.IsHyperbolic)
                {
                    var semiMajorAxis = orbit.SemiMajorAxis;
                    infoBuilder.AppendLine(string.Format("Semi-major axis: {0} ({1})",
                        DataFormatter.Format(semiMajorAxis - primaryBodyRadius, DataUnit.Distance),
                        DataFormatter.Format(semiMajorAxis - primaryBodyRadius, nonSIDistanceUnit)));
                }

                infoBuilder.AppendLine(string.Format("Periapsis: {0} ({1})",
                    DataFormatter.Format(orbit.Periapsis - primaryBodyRadius, DataUnit.Distance),
                    DataFormatter.Format(orbit.Periapsis - primaryBodyRadius, nonSIDistanceUnit)));

                if (orbit.IsElliptical)
                {
                    infoBuilder.AppendLine(string.Format("Apoapsis: {0} ({1})",
                        DataFormatter.Format(orbit.Apoapsis - primaryBodyRadius, DataUnit.Distance),
                        DataFormatter.Format(orbit.Apoapsis - primaryBodyRadius, nonSIDistanceUnit)));
                }

                var completelyVertical = (orbit.IsParabolic && orbit.Periapsis <= 0.001);

                infoBuilder.AppendLine("Eccentricity: " + DataFormatter.Format(orbit.Eccentricity, DataUnit.NoUnit));
                infoBuilder.AppendLine("Inclination: " + DataFormatter.Format(orbit.Inclination, DataUnit.Angle));
                infoBuilder.AppendLine("Longitude of ascending node: " + DataFormatter.Format(orbit.LongitudeOfAscendingNode, DataUnit.Angle));
                infoBuilder.AppendLine("Argument of periapsis: " + DataFormatter.Format(orbit.ArgumentOfPeriapsis, DataUnit.Angle));

                if (!completelyVertical)
                {
                    infoBuilder.AppendLine("True anomaly: " + DataFormatter.Format(orbitPosition.TrueAnomaly, DataUnit.Angle));
                }

                if (physicsObject != null && primaryBody != null)
                {
                    infoBuilder.AppendLine("Flight path angle: " + DataFormatter.Format(physicsObject.FlightPathAngle, DataUnit.Angle));
                }
    
                if (orbit.IsElliptical)
                {
                    infoBuilder.AppendLine("Eccentric anomaly: " + DataFormatter.Format(orbitPosition.EccentricAnomaly, DataUnit.Angle));
                }
                else if (orbit.IsParabolic)
                {
                    if (!completelyVertical)
                    {
                        infoBuilder.AppendLine("Parabolic eccentric anomaly: " + DataFormatter.Format(orbitPosition.ParabolicEccentricAnomaly, DataUnit.Angle));
                    }
                }
                else if (orbit.IsHyperbolic)
                {
                    infoBuilder.AppendLine("Hyperbolic eccentric anomaly: " + DataFormatter.Format(orbitPosition.HyperbolicEccentricAnomaly, DataUnit.Angle));
                }

                if (orbit.IsBound)
                {
                    var angleToPrograde = OrbitHelpers.AngleToPrograde(
                        refPosition,
                        refVelocity,
                        state.Position);
                    infoBuilder.AppendLine("Angle to prograde: " + DataFormatter.Format(angleToPrograde, DataUnit.Angle));
                }

                if (orbit.IsElliptical)
                {
                    infoBuilder.AppendLine("Period: " + DataFormatter.Format(orbit.Period, DataUnit.Time, 0));

                    if (orbit.PrimaryBody != null)
                    {
                        infoBuilder.AppendLine("Time to periapsis: " + DataFormatter.Format(orbitPosition.TimeToPeriapsis(), DataUnit.Time, 0));
                        infoBuilder.AppendLine("Time to apoapsis: " + DataFormatter.Format(orbitPosition.TimeToApoapsis(), DataUnit.Time, 0));
                    }
                }

                //if (orbit.IsUnbound)
                //{
                //    var timeToLeaveSOI = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(orbit);
                //    if (timeToLeaveSOI != null)
                //    {
                //        infoBuilder.AppendLine("Time to leave sphere-of-influence: " + DataFormatter.Format(timeToLeaveSOI ?? 0, DataUnit.Time, 0));
                //    }
                //}

                if (orbit.IsUnbound && !completelyVertical)
                {
                    var timeToPeriapsis = orbitPosition.TimeToPeriapsis();
                    if (Math.Abs(timeToPeriapsis) <= 200.0 * 365.0 * 24.0 * 60.0 * 60.0)
                    {
                        infoBuilder.AppendLine("Time to periapsis: " + DataFormatter.Format(timeToPeriapsis, DataUnit.Time, 0));
                    }
                }

                //if (primaryBody != null)
                //{
                //    var timeToImpact = OrbitCalculators.TimeToImpact(orbit);

                //    if (timeToImpact != null)
                //    {
                //        infoBuilder.AppendLine("Time to impact: " + DataFormatter.Format(timeToImpact ?? 0, DataUnit.Time, 0));
                //    }
                //}
            }
            else
            {
                //var primaryRotationalSpeed = SurfaceSpeedDueToRotation(primaryBody.Configuration, latitude);
                //primaryRotationalSpeed = state.Velocity.Length();
                //infoBuilder.AppendLine("Surface speed: " + DataFormatter.Format(primaryRotationalSpeed, DataUnit.Velocity));
            }

            return infoBuilder.ToString();
        }
    }
}
