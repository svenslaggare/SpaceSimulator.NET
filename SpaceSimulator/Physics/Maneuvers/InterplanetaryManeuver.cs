using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics.Maneuvers
{
    /// <summary>
    /// Contains methods for calcuating interplanetary maneuvers
    /// </summary>
    public static class InterplanetaryManeuver
    {
        /// <summary>
        /// Calculates the planetary intercept orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object</param>
        /// <param name="primaryBodyOrbit">The orbit of the primary body</param>
        /// <param name="injectionState">The state of at the injection point</param>
        /// <param name="injectionPrimaryState">The state of the primary body at the injection point</param>
        /// <param name="injectionBurn">The injection burn</param>
        /// <param name="injectionOrbit">The injection orbit</param>
        /// <param name="heliocentricOrbit">The heliocentric orbit (after leaving SOI)</param>
        /// <param name="timeToLeaveSOI">The time to leave the SOI after the injection burn</param>
        /// <param name="leaveSOIState">The state of the object when it leaves the SOI</param>
        private static void CalculatePlanteryInterceptBurnOrbit(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ref Orbit primaryBodyOrbit,
            ObjectState injectionState,
            ref ObjectState injectionPrimaryState,
            ref Vector3d injectionBurn,
            out OrbitPosition injectionOrbit,
            out OrbitPosition heliocentricOrbit,
            out ObjectState leaveSOIState,
            out double timeToLeaveSOI)
        {
            var sun = physicsObject.PrimaryBody.PrimaryBody;

            //Calculate the injection orbit (escape orbit from primary body)
            injectionState.Velocity += injectionBurn;
            injectionOrbit = OrbitPosition.CalculateOrbitPosition(physicsObject.PrimaryBody, ref injectionPrimaryState, ref injectionState);
            injectionOrbit.TrueAnomaly = 0;

            //Calculate the state when the object leaves the SOI
            timeToLeaveSOI = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(injectionOrbit) ?? 0;

            var leaveSOIPrimaryState = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject.PrimaryBody,
                sun.State,
                injectionPrimaryState,
                primaryBodyOrbit,
                timeToLeaveSOI);

            leaveSOIState = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject,
                injectionPrimaryState,
                injectionState,
                injectionOrbit.Orbit,
                leaveSOIPrimaryState,
                timeToLeaveSOI);

            //Console.WriteLine(MathHelpers.Normalized(leaveSOIState.Velocity));
            //Console.WriteLine(MathHelpers.Normalized(leaveSOIPrimaryState.Velocity));

            //Calculate the final heliocentric transfer orbit
            heliocentricOrbit = OrbitPosition.CalculateOrbitPosition(sun, ref leaveSOIState);
        }

        /// <summary>
        /// Calculates the correct burn time for a planetary intercept
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to intercept with</param>
        /// <param name="currentPlanet">The current planet of the object</param>
        /// <param name="objectOrbit">The orbit of the object</param>
        /// <param name="currentPlanetOrbit">The orbit of the planet</param>
        /// <param name="targetOrbit">The orbit of the target</param>
        /// <param name="alignmentTime">The hohmann alignment time</param>
        /// <param name="r0">The relative distance of the object</param>
        /// <param name="v0">The injection speed</param>
        /// <param name="injectionDeltaV">The delta V for the injection burn</param>
        private static double CalculatePlanetaryBurnTime(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            IPhysicsObject currentPlanet,
            Orbit objectOrbit,
            Orbit currentPlanetOrbit,
            Orbit targetOrbit,
            double alignmentTime,
            double r0,
            double v0,
            double injectionDeltaV,
            double hyperbolicExcessSpeed)
        {
            var sun = currentPlanet.PrimaryBody;
            var planetMu = currentPlanet.StandardGravitationalParameter;
            var E = (v0 * v0 * 0.5) - planetMu / r0;
            var h = r0 * v0;
            var e = Math.Sqrt(1 + (2 * E * h * h) / (planetMu * planetMu));
            var requiredEjectionAngle = Math.Acos(-1.0 / e);

            var t = alignmentTime;
            //var ejectionAngle = 0.0;

            var w = Math.Sqrt(planetMu / Math.Pow(r0, 3));

            var transferDir = 1;
            if (targetOrbit.SemiMajorAxis < currentPlanetOrbit.SemiMajorAxis)
            {
                transferDir = -1;
            }

            //Compute the time
            var injectionPrimaryState = simulatorEngine.KeplerProblemSolver.Solve(
                currentPlanet,
                sun.State,
                currentPlanet.State,
                currentPlanetOrbit,
                alignmentTime);

            var injectionState = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject,
                currentPlanet.State,
                physicsObject.State,
                objectOrbit,
                injectionPrimaryState,
                alignmentTime);

            var currentEjectionAngle = OrbitHelpers.AngleToPrograde(
                injectionPrimaryState.Position,
                injectionPrimaryState.Velocity * transferDir,
                injectionState.Position);

            var tn = (currentEjectionAngle - requiredEjectionAngle) / w;
            t = alignmentTime + tn;

            //Console.WriteLine(Math.Abs(ejectionAngle - requiredEjectionAngle));
            //Console.WriteLine(alignmentTime - t);
            return t;
        }

        /// <summary>
        /// Transfers from the current planet to the given
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="targetOrbit">The planet to rendevouz with</param>
        public static OrbitalManeuvers PlanetaryTransfer(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            IPhysicsObject target)
        {
            if (!target.PrimaryBody.IsObjectOfReference)
            {
                throw new ArgumentException("The target must be a planet.");
            }

            var startTime = DateTime.UtcNow;

            var day = 24.0 * 60.0 * 60.0;
            var sun = physicsObject.PrimaryBody.PrimaryBody;
            var currentPlanet = physicsObject.PrimaryBody;

            if (target == currentPlanet)
            {
                throw new ArgumentException("The target cannot be the same as the current planet.");
            }

            var currentPlanetOrbitPosition = OrbitPosition.CalculateOrbitPosition(currentPlanet);
            var targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(target);
            var objectOrbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
            var currentPlanetOrbit = currentPlanetOrbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;
            var objectOrbit = objectOrbitPosition.Orbit;

            //Calculate the heliocentric transfer orbit burn
            var state = physicsObject.State;
            var hohmannCoastTime = MiscHelpers.RoundToDays(HohmannTransferOrbit.CalculateBurn(
                sun.StandardGravitationalParameter,
                currentPlanetOrbit.SemiMajorAxis,
                targetOrbit.SemiMajorAxis).CoastTime);

            var synodicPeriod = OrbitFormulas.SynodicPeriod(currentPlanetOrbit.Period, targetOrbit.Period);
            var testStartTime = DateTime.UtcNow;
            var possibleLaunches = InterceptManeuver.Intercept(
                simulatorEngine,
                sun,
                currentPlanet,
                currentPlanet.State,
                currentPlanetOrbitPosition,
                target,
                targetOrbitPosition,
                hohmannCoastTime * 0.5,
                hohmannCoastTime * 2.0,
                0,
                MiscHelpers.RoundToDays(synodicPeriod),
                out var heliocentricTransferBurnVector,
                out var t,
                out var coastTime,
                day,
                true);
            //ChartHelpers.ShowPossibleLaunchesChart(possibleLaunches, false);
            Console.WriteLine((DateTime.UtcNow - testStartTime));
            var heliocentricTransferBurn = heliocentricTransferBurnVector.Length();

            //Uses a hohmann transfer orbit
            //var t = HohmannTransferOrbitTimeToAlignment(ref currentPlanetOrbit, ref targetOrbit);
            //var heliocentricTransferBurn = HohmannTransferOrbit(
            //    sun.StandardGravitationalParameter,
            //    currentPlanetOrbit.SemiMajorAxis,
            //    targetOrbit.SemiMajorAxis).FirstBurn;
            //var coastTime = hohmannCoastTime;

            //Calculate the correct delta V, since the previous calculations does not include speed to escape SOI
            var orbitalRadius = Vector3d.Distance(state.Position, currentPlanet.State.Position);
            var orbitalSpeed = Vector3d.Distance(state.Velocity, currentPlanet.State.Velocity);

            var r0 = orbitalRadius;
            var v0 = Math.Sqrt(heliocentricTransferBurn * heliocentricTransferBurn + (2.0 * currentPlanet.StandardGravitationalParameter) / r0);
            var injectionDeltaV = v0 - orbitalSpeed;

            //Calculate the time when the maneuver should be applied
            testStartTime = DateTime.UtcNow;
            t = CalculatePlanetaryBurnTime(
                simulatorEngine,
                physicsObject,
                currentPlanet,
                objectOrbit,
                currentPlanetOrbit,
                targetOrbit,
                t,
                r0,
                v0,
                injectionDeltaV,
                heliocentricTransferBurn);
            Console.WriteLine((DateTime.UtcNow - testStartTime));

            var injectionState = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                physicsObject,
                physicsObject.State,
                objectOrbit,
                t);

            var injectionPrimaryState = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                physicsObject.PrimaryBody,
                physicsObject.PrimaryBody.State,
                currentPlanetOrbit,
                t);

            //Calculate the prograde vector at the burn
            var burnState = injectionState;
            burnState.MakeRelative(injectionPrimaryState);
            var prograde = burnState.Prograde;

            var bestInjectionBurn = prograde * injectionDeltaV;
            CalculatePlanteryInterceptBurnOrbit(
                simulatorEngine,
                physicsObject,
                ref currentPlanetOrbit,
                injectionState,
                ref injectionPrimaryState,
                ref bestInjectionBurn,
                out var bestInjectionOrbitPosition,
                out var bestHeliocentricOrbitPosition,
                out var leaveSOIState,
                out var timeToLeaveSOI);

            var targetStateAtSOILeave = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                target,
                target.State,
                targetOrbit,
                t + timeToLeaveSOI);

            var targetOrbitAtSOILeave = OrbitPosition.CalculateOrbitPosition(sun, targetStateAtSOILeave);

            //Calculate the mid-course correction burn that will take the spacecraft to the precise encounter
            testStartTime = DateTime.UtcNow;
            InterceptManeuver.Intercept(
                simulatorEngine,
                sun,
                physicsObject,
                leaveSOIState,
                bestHeliocentricOrbitPosition,
                target,
                targetOrbitAtSOILeave,
                MiscHelpers.RoundToDays(coastTime) * 0.75,
                MiscHelpers.RoundToDays(coastTime) * 2.0,
                0,
                MiscHelpers.RoundToDays(coastTime) * 0.5,
                out var midcourseBurnDeltaV,
                out var midcourseBurnTime,
                out var midcourseCoastTime,
                day * 0.5,
                false,
                allowedDeltaV: 150);
            Console.WriteLine((DateTime.UtcNow - testStartTime));

            testStartTime = DateTime.UtcNow;
            var minDistance = OrbitCalculators.ClosestApproach(
                simulatorEngine.KeplerProblemSolver,
                currentPlanet,
                bestHeliocentricOrbitPosition,
                target,
                targetOrbitAtSOILeave,
                deltaTime: 12.0 * 60 * 60.0).Distance;
            Console.WriteLine((DateTime.UtcNow - testStartTime));

            //Console.WriteLine(bestInjectionBurn.Length());
            Console.WriteLine(bestInjectionOrbitPosition);
            Console.WriteLine(bestHeliocentricOrbitPosition);
            //Console.WriteLine(DataFormatter.Format(minDistance, DataUnit.Distance));

            Console.WriteLine("Computed trajectory in: " + (DateTime.UtcNow - startTime));

            var injectionBurn = new OrbitalManeuver(simulatorEngine.TotalTime + t, bestInjectionBurn);
            var midcourseBurn = new OrbitalManeuver(simulatorEngine.TotalTime + timeToLeaveSOI + t + midcourseBurnTime, midcourseBurnDeltaV);
            return OrbitalManeuvers.Sequence(injectionBurn, midcourseBurn);
        }
    }
}
