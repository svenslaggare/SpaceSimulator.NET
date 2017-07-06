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

        private readonly ObjectState state;

        private readonly OrbitPosition currentPlanetOrbitPosition;
        private readonly OrbitPosition targetOrbitPosition;
        private readonly OrbitPosition physicsObjectOrbitPosition;

        private readonly IPrimaryBodyObject sun;
        private readonly IPrimaryBodyObject currentPlanet;

        /// <summary>
        /// The possible departure burns
        /// </summary>
        public IList<InterceptManeuver.PossibleLaunch> PossibleDepartureBurns { get; private set; } = new List<InterceptManeuver.PossibleLaunch>();

        private double heliocentricTransferBurn;
        private double injectionToDepatureOrbitTime;
        private double coastTime;

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

            if (!target.PrimaryBody.IsObjectOfReference)
            {
                throw new ArgumentException("The target must be a planet.");
            }

            this.sun = physicsObject.PrimaryBody.PrimaryBody;
            this.currentPlanet = physicsObject.PrimaryBody;

            if (target == currentPlanet)
            {
                throw new ArgumentException("The target cannot be the same as the current planet.");
            }

            this.state = physicsObject.State;

            this.currentPlanetOrbitPosition = OrbitPosition.CalculateOrbitPosition(currentPlanet);
            this.targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(target);
            this.physicsObjectOrbitPosition = OrbitPosition.CalculateOrbitPosition(physicsObject);
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
                sun.StandardGravitationalParameter,
                CurrentPlanetOrbit.SemiMajorAxis,
                TargetOrbit.SemiMajorAxis).CoastTime);

            var synodicPeriod = OrbitFormulas.SynodicPeriod(this.CurrentPlanetOrbit.Period, this.TargetOrbit.Period);
            this.PossibleDepartureBurns = InterceptManeuver.Intercept(
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
                out var heliocentricTransferBurnVector,
                out this.injectionToDepatureOrbitTime,
                out this.coastTime,
                deltaTime,
                true);

            this.heliocentricTransferBurn = heliocentricTransferBurnVector.Length();
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
            this.coastTime = heliocentricTransferBurn.CoastTime;
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
            this.coastTime = coastTime;
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
            var injectionOrbit = OrbitPosition.CalculateOrbitPosition(physicsObject.PrimaryBody, ref injectionPrimaryState, ref injectionState);
            injectionOrbit.TrueAnomaly = 0;

            //Calculate the state when the object leaves the SOI
            timeToLeaveSOI = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(injectionOrbit) ?? 0;

            var leaveSOIPrimaryState = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject.PrimaryBody,
                sun.State,
                injectionPrimaryState,
                this.CurrentPlanetOrbit,
                timeToLeaveSOI);

            leaveSOIState = simulatorEngine.KeplerProblemSolver.Solve(
                physicsObject,
                injectionPrimaryState,
                injectionState,
                injectionOrbit.Orbit,
                leaveSOIPrimaryState,
                timeToLeaveSOI);

            //Calculate the final heliocentric transfer orbit
            this.bestHeliocentricOrbitPosition = OrbitPosition.CalculateOrbitPosition(sun, ref leaveSOIState);
        }

        /// <summary>
        /// Calculates the correct burn time for a planetary intercept
        /// </summary>
        /// <param name="alignmentTime">The hohmann alignment time</param>
        /// <param name="r0">The relative distance of the object</param>
        /// <param name="v0">The injection speed</param>
        /// <param name="injectionDeltaV">The delta V for the injection burn</param>
        /// <param name="hyperbolicExcessSpeed">The hyperbolic excess speed</param>
        private double CalculatePlanteryInterceptBurnTime(
            double alignmentTime,
            double r0,
            double v0,
            double injectionDeltaV,
            double hyperbolicExcessSpeed)
        {
            var planetMu = this.currentPlanet.StandardGravitationalParameter;
            var E = (v0 * v0 * 0.5) - planetMu / r0;
            var h = r0 * v0;
            var e = Math.Sqrt(1 + (2 * E * h * h) / (planetMu * planetMu));
            var requiredEjectionAngle = Math.Acos(-1.0 / e);

            var t = alignmentTime;
            var w = Math.Sqrt(planetMu / Math.Pow(r0, 3));

            var transferDir = 1;
            if (TargetOrbit.SemiMajorAxis < CurrentPlanetOrbit.SemiMajorAxis)
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

            var tn = (currentEjectionAngle - requiredEjectionAngle) / w;
            t = alignmentTime + tn;

            return t;
        }

        /// <summary>
        /// Calculates the injection to the departure orbit
        /// </summary>
        public void CalculateInjectionToDepatureOrbit()
        {
            //Calculate the correct delta V, since the previous calculations does not include speed to escape SOI
            var orbitalRadius = Vector3d.Distance(state.Position, currentPlanet.State.Position);
            var orbitalSpeed = Vector3d.Distance(state.Velocity, currentPlanet.State.Velocity);

            var r0 = orbitalRadius;
            var v0 = Math.Sqrt(heliocentricTransferBurn * heliocentricTransferBurn + (2.0 * currentPlanet.StandardGravitationalParameter) / r0);
            var injectionDeltaV = v0 - orbitalSpeed;

            //Calculate the departure time
            this.injectionToDepatureOrbitTime = this.CalculatePlanteryInterceptBurnTime(
                this.injectionToDepatureOrbitTime,
                r0,
                v0,
                injectionDeltaV,
                heliocentricTransferBurn);

            //Calculate the complete intercept orbit
            var injectionState = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                physicsObject,
                physicsObject.State,
                PhysicsObjectOrbit,
                this.injectionToDepatureOrbitTime);

            var injectionPrimaryState = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                physicsObject.PrimaryBody,
                physicsObject.PrimaryBody.State,
                CurrentPlanetOrbit,
                this.injectionToDepatureOrbitTime);

            var burnState = injectionState;
            burnState.MakeRelative(injectionPrimaryState);
            var prograde = burnState.Prograde;

            this.bestInjectionBurn = prograde * injectionDeltaV;
            this.CalculatePlanteryInterceptBurnOrbit(injectionState, ref injectionPrimaryState);

            var targetStateAtSOILeave = SolverHelpers.AfterTime(
                simulatorEngine.KeplerProblemSolver,
                target,
                target.State,
                TargetOrbit,
                this.injectionToDepatureOrbitTime + timeToLeaveSOI);

            this.targetOrbitPositionAtSOILeave = OrbitPosition.CalculateOrbitPosition(sun, targetStateAtSOILeave);
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
            InterceptManeuver.Intercept(
                this.simulatorEngine,
                this.sun,
                this.physicsObject,
                this.leaveSOIState,
                this.bestHeliocentricOrbitPosition,
                this.target,
                this.targetOrbitPositionAtSOILeave,
                MiscHelpers.RoundToDays(coastTime) * minCoastRatio,
                MiscHelpers.RoundToDays(coastTime) * maxCoastRatio,
                0,
                MiscHelpers.RoundToDays(coastTime) * maxLaunchTime,
                out this.midcourseBurnDeltaV,
                out this.midcourseBurnTime,
                out var midcourseCoastTime,
                deltaTime,
                false,
                allowedDeltaV: 150);
        }

        /// <summary>
        /// Computes the maneuvers (assuming everything else has been calculated)
        /// </summary>
        public OrbitalManeuvers ComputeManeuvers()
        {
            var injectionBurn = new OrbitalManeuver(
                simulatorEngine.TotalTime + this.injectionToDepatureOrbitTime,
                this.bestInjectionBurn);

            var midcourseBurn = new OrbitalManeuver(
                simulatorEngine.TotalTime + this.timeToLeaveSOI + this.injectionToDepatureOrbitTime + this.midcourseBurnTime,
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
