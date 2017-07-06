﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// Contains method for calcuating the Hohmann transfer orbit
    /// </summary>
    public static class HohmannTransferOrbit
    {
        /// <summary>
        /// Calculates the time left to apply a Hohmann transfer orbit in order to rendevouz with the target orbit
        /// </summary>
        /// <param name="orbit">The current orbit</param>
        /// <param name="targetOrbit">The target orbit</param>
        public static double TimeToAlignment(ref OrbitPosition orbitPosition, ref OrbitPosition targetOrbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;

            var r1 = orbit.SemiMajorAxis;
            var r2 = targetOrbit.SemiMajorAxis;
            var a1 = orbitPosition.TrueAnomaly;
            var a2 = targetOrbitPosition.TrueAnomaly;

            //Calculate the required angular alignment
            var alpha = Math.PI * (1.0 - (1.0 / (2.0 * Math.Sqrt(2))) * Math.Sqrt(Math.Pow(r1 / r2 + 1, 3)));

            //Calculate the time when the alignment is reached
            var deltaAngle = 0.0;
            if (r2 > r1)
            {
                deltaAngle = a2 - a1;
            }
            else
            {
                deltaAngle = a1 - a2;
            }

            if (deltaAngle < 0)
            {
                deltaAngle += 2 * Math.PI;
            }

            var mu = orbit.StandardGravitationalParameter;
            var w1 = Math.Sqrt(mu / Math.Pow(r1, 3));
            var w2 = Math.Sqrt(mu / Math.Pow(r2, 3));
            var angularDeltaSpeed = w1 - w2;

            var t = 0.0;
            if (r2 > r1)
            {
                t = Math.Abs(deltaAngle - alpha) / Math.Abs(angularDeltaSpeed);
            }
            else
            {
                t = Math.Abs(deltaAngle + alpha) / Math.Abs(angularDeltaSpeed);
            }

            return t;
        }

        /// <summary>
        /// The data for a Hohmann transfer
        /// </summary>
        public struct HohmannTransferOrbitData
        {
            /// <summary>
            /// The deltaV for the first maneuver
            /// </summary>
            public double FirstBurn { get; }

            /// <summary>
            /// The deltaV for the second maneuver
            /// </summary>
            public double SecondBurn { get; }

            /// <summary>
            /// The coast time
            /// </summary>
            public double CoastTime { get; }

            /// <summary>
            /// Creates new data
            /// </summary>
            /// <param name="firstBurn">The deltaV for the first maneuver</param>
            /// <param name="secondBurn">The deltaV for the second maneuver</param>
            /// <param name="coastTime">The coast time</param>
            public HohmannTransferOrbitData(double firstBurn, double secondBurn, double coastTime)
            {
                this.FirstBurn = firstBurn;
                this.SecondBurn = secondBurn;
                this.CoastTime = coastTime;
            }
        }

        /// <summary>
        /// Calculates the burns and coast time for a Hohmann transfer orbit
        /// </summary>
        /// <param name="standardGravitationalParameter">The standard gravitational parameter</param>
        /// <param name="currentRadius">The current radius</param>
        /// <param name="newRadius">The new radis</param>
        public static HohmannTransferOrbitData CalculateBurn(double standardGravitationalParameter, double currentRadius, double newRadius)
        {
            //DeltaV for the burns
            var burn1DeltaV = Math.Sqrt(standardGravitationalParameter / currentRadius) * (Math.Sqrt((2 * newRadius) / (currentRadius + newRadius)) - 1);
            var burn2DeltaV = Math.Sqrt(standardGravitationalParameter / newRadius) * (1 - Math.Sqrt((2 * currentRadius) / (currentRadius + newRadius)));

            //The time between the maneuvers
            var coastTime = Math.PI * Math.Sqrt(Math.Pow(currentRadius + newRadius, 3) / (8 * standardGravitationalParameter));

            return new HohmannTransferOrbitData(burn1DeltaV, burn2DeltaV, coastTime);
        }

        /// <summary>
        /// Creates a Hohmann transfer from the current orbit to an orbit of the given radius
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="state">The state of the object</param>
        /// <param name="orbitPosition">The orbit position</param>
        /// <param name="physicsObject">The object</param>
        /// <param name="newRadius">The new radius</param>
        /// <param name="currentRadius">The current radius</param>
        /// <param name="maneuverTime">When the maneuver will be applied</param>
        /// <remarks>The current orbit must be circular</remarks>
        public static OrbitalManeuvers Create(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref ObjectState state,
            ref OrbitPosition orbitPosition,
            double currentRadius,
            double newRadius,
            OrbitalManeuverTime maneuverTime)
        {
            var orbit = orbitPosition.Orbit;
            var initalPrimaryBodyState = physicsObject.PrimaryBody.State;
            if (!orbit.IsCircular)
            {
                throw new ArgumentException("The orbit is not circular (e = " + orbit.Eccentricity + ")");
            }

            var r1 = currentRadius;
            var r2 = newRadius;

            var mu = orbit.StandardGravitationalParameter;

            //Calculate the directions at the two burns, and the time when to apply the first one
            var t1 = 0.0;

            switch (maneuverTime.Type)
            {
                case OrbitalManeuverTimeType.Periapsis:
                    t1 = orbitPosition.TimeToPeriapsis();
                    break;
                case OrbitalManeuverTimeType.Apoapsis:
                    t1 = orbitPosition.TimeToApoapsis();
                    break;
                case OrbitalManeuverTimeType.Now:
                    break;
                case OrbitalManeuverTimeType.TimeFromNow:
                    t1 = maneuverTime.Value;
                    break;
            }

             SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                physicsObject,
                state,
                orbit,
                t1,
                out var burnState,
                out var burnPrimaryState);
            var dir1 = MathHelpers.Normalized(burnState.Velocity - burnPrimaryState.Velocity);
            var dir2 = -dir1;

            var data = CalculateBurn(mu, r1, r2);

            return OrbitalManeuvers.Sequence(
                new OrbitalManeuver(simulatorEngine.TotalTime + t1, dir1 * data.FirstBurn),
                new OrbitalManeuver(simulatorEngine.TotalTime + t1 + data.CoastTime, dir2 * data.SecondBurn));
        }

        /// <summary>
        /// Creates a Hohmann transfer from the current orbit to an orbit of the given radius
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="state">The state of the object</param>
        /// <param name="orbitPosition">The orbit position</param>
        /// <param name="physicsObject">The object</param>
        /// <param name="newRadius">The new radius</param>
        /// <param name="maneuverTime">When the maneuver will be applied</param>
        /// <remarks>The current orbit must be circular</remarks>
        public static OrbitalManeuvers Create(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref ObjectState state,
            ref OrbitPosition orbitPosition,
            double newRadius,
            OrbitalManeuverTime maneuverTime)
        {
            return Create(
                simulatorEngine,
                physicsObject,
                ref state,
                ref orbitPosition,
                state.Distance(physicsObject.PrimaryBody.State),
                newRadius,
                maneuverTime);
        }

        /// <summary>
        /// Creates a Hohmann transfer from the current orbit to an orbit of the given radius
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="state">The state of the object</param>
        /// <param name="orbitPosition">The orbit position</param>
        /// <param name="physicsObject">The object</param>
        /// <param name="newRadius">The new radius</param>
        /// <param name="maneuverTime">When the maneuver will be applied</param>
        /// <remarks>The current orbit must be circular</remarks>
        public static OrbitalManeuvers Create(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            double newRadius,
            OrbitalManeuverTime maneuverTime)
        {
            var objectState = physicsObject.State;
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            return Create(simulatorEngine, physicsObject, ref objectState, ref orbitPosition, newRadius, maneuverTime);
        }
    }
}
