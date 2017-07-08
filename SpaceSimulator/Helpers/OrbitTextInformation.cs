using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
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
            NaturalSatelliteObject primaryBody,
            ObjectState state,
            OrbitPosition orbitPosition,
            PhysicsObject physicsObject = null)
        {
            var orbit = orbitPosition.Orbit;
            var infoBuilder = new StringBuilder();
            
            void AddBulletItem(string item)
            {
                infoBuilder.AppendLine("    • " + item);
            }

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
                string FormatMass(double mass)
                {
                    return DataFormatter.Format(mass, DataUnit.Mass, useBase10: true);
                }

                infoBuilder.AppendLine("Mass: " + FormatMass(physicsObject.Mass));

                if (physicsObject is RocketObject rocketObject)
                {
                    foreach (var stage in rocketObject.Stages)
                    {
                        if (stage.InitialFuelMass > 0.0)
                        {
                            AddBulletItem($"{stage.Name}: {FormatMass(stage.Mass)} ({Math.Round(100 * (stage.FuelMassRemaining / stage.InitialFuelMass), 1)}%)");
                        }
                        else
                        {
                            AddBulletItem($"{stage.Name}: {FormatMass(stage.Mass)}");
                        }
                    }
                }
            }

            var refPosition = Vector3d.Zero;
            var refVelocity = Vector3d.Zero;

            if (primaryBody != null)
            {
                refPosition = primaryBody.Position;
                refVelocity = primaryBody.Velocity;
            }

            if (physicsObject == null || !physicsObject.HasImpacted)
            {
                infoBuilder.AppendLine("Distance: " + DataFormatter.Format((state.Position - refPosition).Length(), DataUnit.Distance));
                infoBuilder.AppendLine("Speed: " + DataFormatter.Format((state.Velocity - refVelocity).Length(), DataUnit.Velocity));

                if (physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
                {
                    var relativeVelocity = state.Velocity - refVelocity;
                    var gravityAccelerationDirection = MathHelpers.Normalized(primaryBody.Position - state.Position);

                    (var horizontalSpeed, var verticalSpeed) = OrbitHelpers.ComputeHorizontalAndVerticalVelocity(gravityAccelerationDirection, relativeVelocity);

                    AddBulletItem("Horizontal speed: " + DataFormatter.Format(horizontalSpeed, DataUnit.Velocity));
                    AddBulletItem("Vertical speed: " + DataFormatter.Format(verticalSpeed, DataUnit.Velocity));
                }
            }

            infoBuilder.AppendLine("Acceleration: " + DataFormatter.Format(state.Acceleration.Length(), DataUnit.Acceleration));

            if (physicsObject != null && physicsObject.Type == PhysicsObjectType.ArtificialSatellite)
            {
                var gravityAcceleration = OrbitFormulas.GravityAcceleration(physicsObject.PrimaryBody.StandardGravitationalParameter, state.Position - refPosition);
                AddBulletItem("Gravity: " + DataFormatter.Format(gravityAcceleration.Length(), DataUnit.Acceleration));

                if (physicsObject is ArtificialPhysicsObject artificialPhysicsObject)
                {
                    var primaryPlanet = primaryBody as PlanetObject;
                    var dragAcceleration = primaryPlanet.DragOnObject(artificialPhysicsObject, ref state) / artificialPhysicsObject.Mass;

                    if (physicsObject is RocketObject rocketObject)
                    {
                        var thrustAcceleration = rocketObject.EngineAcceleration();
                        AddBulletItem("Thrust: " + DataFormatter.Format(thrustAcceleration.Length(), DataUnit.Acceleration));
                    }

                    AddBulletItem("Drag: " + DataFormatter.Format(dragAcceleration.Length(), DataUnit.Acceleration));
                }
            }

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

            {
                if (primaryBody is PlanetObject primaryPlanet)
                {
                    var atmosphericModel = primaryPlanet.AtmosphericModel;

                    if (atmosphericModel.Inside(primaryBody, ref state))
                    {
                        infoBuilder.AppendLine("");
                        infoBuilder.AppendLine("Atmosphere");
                        var altitude = primaryBody.Altitude(state.Position);

                        (var pressure, var temperature) = atmosphericModel.PressureAndTemperature(altitude);
                        infoBuilder.AppendLine($"Pressure: {DataFormatter.Format(pressure, DataUnit.Pressure)}");
                        infoBuilder.AppendLine($"Temperature: {DataFormatter.Format(temperature, DataUnit.TemperatureCelsius)}");

                        if (atmosphericModel is EarthAtmosphericModel earthAtmosphericModel)
                        {
                            var densityOfAir = AtmosphericFormulas.DensityOfAir(pressure, temperature);
                            var dynamicPressure = AtmosphericFormulas.DynamicPressure(densityOfAir, (state.Velocity - refVelocity).Length());
                            infoBuilder.AppendLine($"Density of air: {DataFormatter.Format(densityOfAir, DataUnit.Density, useBase10: true)}");
                            infoBuilder.AppendLine($"Dynamic pressure: {DataFormatter.Format(dynamicPressure, DataUnit.Pressure)}");
                        }
                    }
                }
            }

            if (physicsObject == null || !physicsObject.HasImpacted)
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
                    primaryBodyRadius = primaryBody.Radius;
                }

                var parameter = orbit.Parameter;
                infoBuilder.AppendLine(string.Format("Semi-latus rectum: {0} ({1})",
                    DataFormatter.Format(parameter - primaryBodyRadius, DataUnit.Distance),
                    DataFormatter.Format(parameter - primaryBodyRadius, nonSIDistanceUnit)));

                if (orbit.IsBound || orbit.IsHyperbolic)
                {
                    var semiMajorAxis = orbit.SemiMajorAxis;
                    infoBuilder.AppendLine(string.Format("Semi-major axis: {0} ({1})",
                        DataFormatter.Format(semiMajorAxis - primaryBodyRadius, DataUnit.Distance),
                        DataFormatter.Format(semiMajorAxis - primaryBodyRadius, nonSIDistanceUnit)));
                }

                infoBuilder.AppendLine(string.Format("Periapsis: {0} ({1})",
                    DataFormatter.Format(orbit.Periapsis - primaryBodyRadius, DataUnit.Distance),
                    DataFormatter.Format(orbit.Periapsis - primaryBodyRadius, nonSIDistanceUnit)));

                if (orbit.IsBound)
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
    
                if (orbit.IsBound)
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

                if (orbit.IsBound)
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

        /// <summary>
        /// Returns the target information for the given orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state</param>
        /// <param name="orbitPosition">The orbit</param>
        /// <param name="target">The target</param>
        /// <param name="targetState">The target state</param>
        /// <param name="targetOrbitPosition">The target orbit</param>
        /// <param name="calculateClosestApproach">Indicates if the closest approach should be caclulated</param>
        /// <param name="closestApproach">The closest approach data</param>
        public static string TargetInformation(
            ISimulatorEngine simulatorEngine,
            PhysicsObject physicsObject,
            ObjectState state,
            OrbitPosition orbitPosition,
            PhysicsObject target,
            ObjectState targetState,
            OrbitPosition targetOrbitPosition,
            bool calculateClosestApproach = true,
            OrbitCalculators.ApproachData? closestApproach = null)
        {
            var orbit = orbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;

            var primaryBody = physicsObject.PrimaryBody;

            var infoBuilder = new StringBuilder();
            infoBuilder.AppendLine("Target: " + target.Name);

            infoBuilder.AppendLine(
                "Distance: " + DataFormatter.Format(Vector3d.Distance(state.Position, targetState.Position), DataUnit.Distance));
            infoBuilder.AppendLine(
                "Relative velocity: " + DataFormatter.Format(Vector3d.Distance(state.Velocity, targetState.Velocity), DataUnit.Velocity));

            if (primaryBody == target.PrimaryBody)
            {
                if (calculateClosestApproach)
                {
                    if (closestApproach == null)
                    {
                        closestApproach = OrbitCalculators.ClosestApproach(
                            simulatorEngine.KeplerProblemSolver,
                            physicsObject,
                            orbitPosition,
                            target,
                            targetOrbitPosition);
                    }

                    var timeToClosestApproach = closestApproach.Value.Time - simulatorEngine.TotalTime;
                    infoBuilder.AppendLine("Closest approach: " + DataFormatter.Format(closestApproach.Value.Distance, DataUnit.Distance));
                    infoBuilder.AppendLine("Time to closest approach: " + DataFormatter.Format(Math.Round(timeToClosestApproach), DataUnit.Time));
                }

                if (orbit.IsBound && targetOrbit.IsBound)
                {
                    infoBuilder.AppendLine(
                        "Synodic period: " + DataFormatter.Format(Math.Round(OrbitFormulas.SynodicPeriod(orbit.Period, targetOrbit.Period)), DataUnit.Time));
                }
            }

            if (target is NaturalSatelliteObject targetNatrual)
            {
                infoBuilder.AppendLine(
                    "Sphere of influence: " + DataFormatter.Format(target.SphereOfInfluence ?? 0, DataUnit.Distance));

                var enterOrbitPosition = OrbitPosition.CalculateOrbitPosition(targetNatrual, state);
                var timeToLeaveSOI = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(enterOrbitPosition);
                if (timeToLeaveSOI != null)
                {
                    infoBuilder.AppendLine("Time to enter sphere-of-influence: " + DataFormatter.Format(timeToLeaveSOI ?? 0, DataUnit.Time, 0));
                }
            }

            var phaseAngle = 0.0;
            if (primaryBody == target.PrimaryBody)
            {
                phaseAngle = MathHelpers.CalculateMinAngleDifference(targetOrbitPosition.TrueAnomaly, orbitPosition.TrueAnomaly);
            }
            else if (primaryBody.PrimaryBody == target.PrimaryBody)
            {
                var primaryBodyOrbitPosition = OrbitPosition.CalculateOrbitPosition(primaryBody);
                phaseAngle = MathHelpers.CalculateMinAngleDifference(targetOrbitPosition.TrueAnomaly, primaryBodyOrbitPosition.TrueAnomaly);
            }

            infoBuilder.AppendLine("Phase angle: " + DataFormatter.Format(phaseAngle, DataUnit.Angle));

            return infoBuilder.ToString();
        }

    }
}
