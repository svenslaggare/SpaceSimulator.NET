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
    /// Calculates transfers between planets
    /// </summary>
    public sealed class PlanetaryTransfer
    {
        private readonly ISimulatorEngine simulatorEngine;
        private readonly IPhysicsObject physicsObject;
        private readonly IPhysicsObject target;

        private readonly IPrimaryBodyObject sun;
        private readonly IPrimaryBodyObject currentPlanet;

        private readonly OrbitPosition currentPlanetOrbitPosition;
        private readonly OrbitPosition targetOrbitPosition;
        private readonly OrbitPosition physicsObjectOrbitPosition;

        /// <summary>
        /// The possible departure burns
        /// </summary>
        public IList<InterceptManeuver.PossibleLaunch> PossibleDepartureBurns { get; private set; } = new List<InterceptManeuver.PossibleLaunch>();

        private double injectionToDepatureOrbitTime;
        private double heliocentricTransferBurn;
        private double heliocentricTransferBurnCoastTime;

        private ObjectState leaveSOIState;
        private double timeToLeaveSOI;
        private OrbitPosition targetOrbitPositionAtSOILeave;

        private OrbitPosition bestHeliocentricOrbitPosition;
        private Vector3d bestInjectionBurn;

        private Vector3d midcourseBurnDeltaV;
        private double midcourseBurnTime;

        /// <summary>
        /// Createws a new planet transfer
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="physicsObject">The object to transfer</param>
        /// <param name="target">The object to transfer to</param>
        public PlanetaryTransfer(ISimulatorEngine simulatorEngine, IPhysicsObject physicsObject, IPhysicsObject target)
        {
            this.simulatorEngine = simulatorEngine;
            this.physicsObject = physicsObject;
            this.target = target;

            if (!this.target.PrimaryBody.IsObjectOfReference)
            {
                throw new ArgumentException("The target must be a planet.");
            }

            this.sun = this.physicsObject.PrimaryBody.PrimaryBody;
            this.currentPlanet = this.physicsObject.PrimaryBody;

            if (this.target == this.currentPlanet)
            {
                throw new ArgumentException("The target cannot be the same as the current planet.");
            }

            this.currentPlanetOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.currentPlanet);
            this.targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.target);
            this.physicsObjectOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.physicsObject);
        }

        /// <summary>
        /// The current orbit of the planet that the current object is orbiting around
        /// </summary>
        private Orbit CurrentPlanetOrbit => this.currentPlanetOrbitPosition.Orbit;

        /// <summary>
        /// The current orbit of the target
        /// </summary>
        private Orbit TargetOrbit => this.targetOrbitPosition.Orbit;

        /// <summary>
        /// The current orbit of the object
        /// </summary>
        private Orbit PhysicsObjectOrbit => this.physicsObjectOrbitPosition.Orbit;

        /// <summary>
        /// Calculates the heliocentric transfer orbit
        /// </summary>
        /// <param name="minCoastRatio">The minimum coast ratio (in Hohmann coast time)</param>
        /// <param name="maxCoastRatio">The maximum coast ratio (in Hohmann coast time)</param>
        /// <param name="maxLaunchTime">The maximum number of launch time (in synodic periods)</param>
        /// <param name="deltaTime">The delta time</param>
        public void CalculateHeliocentricTransferOrbit(
            double minCoastRatio = 0.5,
            double maxCoastRatio = 2.0,
            double maxLaunchTime = 1.0,
            double deltaTime = TimeConstants.OneDay)
        {
            var hohmannCoastTime = MiscHelpers.RoundToDays(HohmannTransferOrbit.CalculateBurn(
                this.sun.StandardGravitationalParameter,
                this.CurrentPlanetOrbit.SemiMajorAxis,
                this.TargetOrbit.SemiMajorAxis).CoastTime);

            var synodicPeriod = OrbitFormulas.SynodicPeriod(this.CurrentPlanetOrbit.Period, this.TargetOrbit.Period);

            var helicentricIntercept = new InterceptManeuver(
                this.simulatorEngine,
                this.sun,
                this.currentPlanet,
                this.currentPlanet.State,
                this.currentPlanetOrbitPosition,
                this.target,
                this.targetOrbitPosition,
                hohmannCoastTime * minCoastRatio,
                hohmannCoastTime * maxCoastRatio,
                0,
                MiscHelpers.RoundToDays(synodicPeriod) * maxLaunchTime,
                deltaTime,
                listPossibleLaunches: true);
            var optimalIntercept = helicentricIntercept.Compute();

            this.PossibleDepartureBurns = helicentricIntercept.PossibleIntercepts;
            this.injectionToDepatureOrbitTime = optimalIntercept.StartTime;
            this.heliocentricTransferBurnCoastTime = optimalIntercept.Duration;
            this.heliocentricTransferBurn = optimalIntercept.DeltaVelocity.Length();
        }

        /// <summary>
        /// Calculates the heliocentric transfer orbit using a hohmann orbit
        /// </summary>
        public void CalculateHeliocentricTransferUsingHohmannOrbit()
        {
            var currentPlanetOrbitPosition = this.currentPlanetOrbitPosition;
            var targetOrbitPosition = this.targetOrbitPosition;

            this.injectionToDepatureOrbitTime = HohmannTransferOrbit.TimeToAlignment(ref currentPlanetOrbitPosition, ref targetOrbitPosition);
            var heliocentricTransferBurn = HohmannTransferOrbit.CalculateBurn(
                this.sun.StandardGravitationalParameter,
                this.CurrentPlanetOrbit.SemiMajorAxis,
                this.TargetOrbit.SemiMajorAxis);
            this.heliocentricTransferBurnCoastTime = heliocentricTransferBurn.CoastTime;
            this.heliocentricTransferBurn = heliocentricTransferBurn.FirstBurn;
        }

        /// <summary>
        /// Sets the helicentric transfer orbit
        /// </summary>
        /// <param name="injectionToDepatureOrbitTime">The time when the object should inject to the depature orbit</param>
        /// <param name="coastTime">The coast time</param>
        /// <param name="heliocentricTransferBurn">The delta V for the burn</param>
        public void SetHeliocentricTransferOrbit(double injectionToDepatureOrbitTime, double coastTime, double heliocentricTransferBurn)
        {
            this.injectionToDepatureOrbitTime = injectionToDepatureOrbitTime;
            this.heliocentricTransferBurnCoastTime = coastTime;
            this.heliocentricTransferBurn = heliocentricTransferBurn;
        }

        /// <summary>
        /// Calculates the planetary intercept orbit
        /// </summary>
        /// <param name="injectionState">The state of at the injection point</param>
        /// <param name="injectionPrimaryState">The state of the primary body at the injection point</param>
        private void CalculatePlanteryInterceptBurnOrbit(ObjectState injectionState, ref ObjectState injectionPrimaryState)
        {
            //Calculate the injection orbit (escape orbit from primary body)
            injectionState.Velocity += this.bestInjectionBurn;
            var injectionOrbit = OrbitPosition.CalculateOrbitPosition(this.physicsObject.PrimaryBody, ref injectionPrimaryState, ref injectionState);
            injectionOrbit.TrueAnomaly = 0;

            //Calculate the state when the object leaves the SOI
            this.timeToLeaveSOI = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(injectionOrbit) ?? 0;

            var leaveSOIPrimaryState = simulatorEngine.KeplerProblemSolver.Solve(
                this.physicsObject.PrimaryBody,
                this.sun.State,
                injectionPrimaryState,
                this.CurrentPlanetOrbit,
                this.timeToLeaveSOI);

            this.leaveSOIState = simulatorEngine.KeplerProblemSolver.Solve(
                this.physicsObject,
                injectionPrimaryState,
                injectionState,
                injectionOrbit.Orbit,
                leaveSOIPrimaryState,
                this.timeToLeaveSOI);

            //Calculate the final heliocentric transfer orbit
            this.bestHeliocentricOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.sun, ref leaveSOIState);
        }

        /// <summary>
        /// Calculates the correct burn time for a planetary intercept
        /// </summary>
        /// <param name="alignmentTime">The hohmann alignment time</param>
        /// <param name="distance">The relative distance of the object</param>
        /// <param name="injectionSpeed">The injection speed</param>
        private double CalculatePlanteryInterceptBurnTime(double alignmentTime, double distance, double injectionSpeed)
        {
            var planetMu = this.currentPlanet.StandardGravitationalParameter;
            var E = (injectionSpeed * injectionSpeed * 0.5) - planetMu / distance;
            var h = distance * injectionSpeed;
            var e = Math.Sqrt(1 + (2 * E * h * h) / (planetMu * planetMu));
            var requiredEjectionAngle = Math.Acos(-1.0 / e);

            var t = alignmentTime;
            var w = Math.Sqrt(planetMu / Math.Pow(distance, 3));

            var transferDir = 1;
            if (this.TargetOrbit.SemiMajorAxis < this.CurrentPlanetOrbit.SemiMajorAxis)
            {
                transferDir = -1;
            }

            //Compute the time
            var injectionPrimaryState = simulatorEngine.KeplerProblemSolver.Solve(
                this.currentPlanet,
                this.sun.State,
                this.currentPlanet.State,
                this.CurrentPlanetOrbit,
                alignmentTime);

            var injectionState = simulatorEngine.KeplerProblemSolver.Solve(
                this.physicsObject,
                this.currentPlanet.State,
                this.physicsObject.State,
                this.PhysicsObjectOrbit,
                injectionPrimaryState,
                alignmentTime);

            var currentEjectionAngle = OrbitHelpers.AngleToPrograde(
                injectionPrimaryState.Position,
                injectionPrimaryState.Velocity * transferDir,
                injectionState.Position);

            var deltaTime = (currentEjectionAngle - requiredEjectionAngle) / w;
            t = alignmentTime + deltaTime;
            return t;
        }

        /// <summary>
        /// Calculates the injection to the departure orbit
        /// </summary>
        public void CalculateInjectionToDepatureOrbit()
        {
            //Calculate the correct delta V, since the previous calculations does not include speed to escape SOI
            var state = this.physicsObject.State;
            var orbitalRadius = Vector3d.Distance(state.Position, currentPlanet.State.Position);
            var orbitalSpeed = Vector3d.Distance(state.Velocity, currentPlanet.State.Velocity);

            var r0 = orbitalRadius;
            var v0 = Math.Sqrt(this.heliocentricTransferBurn * this.heliocentricTransferBurn + (2.0 * this.currentPlanet.StandardGravitationalParameter) / r0);
            var injectionDeltaV = v0 - orbitalSpeed;

            //Calculate the departure time
            this.injectionToDepatureOrbitTime = this.CalculatePlanteryInterceptBurnTime(this.injectionToDepatureOrbitTime, r0, v0);

            //Calculate the complete intercept orbit
            var injectionState = SolverHelpers.AfterTime(
                this.simulatorEngine.KeplerProblemSolver,
                this.physicsObject,
                this.physicsObject.State,
                this.PhysicsObjectOrbit,
                this.injectionToDepatureOrbitTime);

            var injectionPrimaryState = SolverHelpers.AfterTime(
                this.simulatorEngine.KeplerProblemSolver,
                this.physicsObject.PrimaryBody,
                this.physicsObject.PrimaryBody.State,
                this.CurrentPlanetOrbit,
                this.injectionToDepatureOrbitTime);

            var burnState = injectionState;
            burnState.MakeRelative(injectionPrimaryState);
            var prograde = burnState.Prograde;

            this.bestInjectionBurn = prograde * injectionDeltaV;
            this.CalculatePlanteryInterceptBurnOrbit(injectionState, ref injectionPrimaryState);

            var targetStateAtSOILeave = SolverHelpers.AfterTime(
                this.simulatorEngine.KeplerProblemSolver,
                this.target,
                this.target.State,
                this.TargetOrbit,
                this.injectionToDepatureOrbitTime + timeToLeaveSOI);

            this.targetOrbitPositionAtSOILeave = OrbitPosition.CalculateOrbitPosition(this.sun, targetStateAtSOILeave);
        }

        /// <summary>
        /// Calculates the mid-course correction
        /// </summary>
        /// <param name="minCoastRatio">The minimum coast ratio (in Hohmann coast time)</param>
        /// <param name="maxCoastRatio">The maximum coast ratio (in Hohmann coast time)</param>
        /// <param name="maxLaunchTime">The maximum number of launch time (in Hohmann coast time)</param>
        /// <param name="deltaTime">The delta time</param>
        public void CalculateMidcourseCorrection(
            double minCoastRatio = 0.75,
            double maxCoastRatio = 2.0,
            double maxLaunchTime = 0.5,
            double deltaTime = 0.5 * TimeConstants.OneDay)
        {
            var midcourseIntercept = new InterceptManeuver(
                this.simulatorEngine,
                this.sun,
                this.physicsObject,
                this.leaveSOIState,
                this.bestHeliocentricOrbitPosition,
                this.target,
                this.targetOrbitPositionAtSOILeave,
                MiscHelpers.RoundToDays(this.heliocentricTransferBurnCoastTime) * minCoastRatio,
                MiscHelpers.RoundToDays(this.heliocentricTransferBurnCoastTime) * maxCoastRatio,
                0,
                MiscHelpers.RoundToDays(this.heliocentricTransferBurnCoastTime) * maxLaunchTime,
                deltaTime,
                listPossibleLaunches: false,
                allowedDeltaV: 150);
            var midcourseManeuver = midcourseIntercept.Compute();
            this.midcourseBurnDeltaV = midcourseManeuver.DeltaVelocity;
            this.midcourseBurnTime = midcourseManeuver.StartTime;
        }

        /// <summary>
        /// Computes the maneuvers (assuming everything else has been calculated)
        /// </summary>
        public OrbitalManeuvers ComputeManeuvers()
        {
            var injectionBurnTime = simulatorEngine.TotalTime + this.injectionToDepatureOrbitTime;
            var injectionBurn = new OrbitalManeuver(
                injectionBurnTime,
                this.bestInjectionBurn);

            var midcourseBurn = new OrbitalManeuver(
                injectionBurnTime + this.timeToLeaveSOI + this.midcourseBurnTime,
                this.midcourseBurnDeltaV);

            return OrbitalManeuvers.Sequence(injectionBurn, midcourseBurn);
        }

        /// <summary>
        /// Computes the transfer maneuver
        /// </summary>
        public OrbitalManeuvers Compute()
        {
            var startTime = DateTime.UtcNow;

            using (new Timing("Calculated helicentric transfer orbit in {0}."))
            {
                this.CalculateHeliocentricTransferOrbit();
                //this.CalculateHeliocentricTransferUsingHohmannOrbit();
            }

            this.CalculateInjectionToDepatureOrbit();

            using (new Timing("Calculated mid-course correction burn in {0}."))
            {
                this.CalculateMidcourseCorrection();
            }

            var totalDeltaV = DataFormatter.Format(bestInjectionBurn.Length() + midcourseBurnDeltaV.Length(), DataUnit.Velocity);
            Console.WriteLine($"Computed trajectory in {(DateTime.UtcNow - startTime)}, deltaV: {totalDeltaV}");
            return this.ComputeManeuvers();
        }
    }
}
