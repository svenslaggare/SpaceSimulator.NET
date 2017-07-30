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
    /// Calculate intercepts between objects
    /// </summary>
    public class InterceptManeuver
    {
        private readonly ISimulatorEngine simulatorEngine;

        private readonly IPrimaryBodyObject primaryBody;
        private readonly Orbit primaryBodyOrbit;

        private readonly IPhysicsObject physicsObject;
        private readonly ObjectState objectState;
        private readonly OrbitPosition objectOrbitPosition;

        private readonly IPhysicsObject target;
        private readonly OrbitPosition targetOrbitPosition;

        private readonly double minInterceptTime;
        private readonly double maxInterceptTime;
        private readonly double minLaunchTime;
        private readonly double maxLaunchTime;
        private readonly double? allowedDeltaV;

        private readonly double deltaTime;

        private readonly bool listPossibleLaunches;

        /// <summary>
        /// Represents a possible launch
        /// </summary>
        public struct PossibleLaunch
        {
            /// <summary>
            /// The start time
            /// </summary>
            public double StartTime { get; }

            /// <summary>
            /// The duration of the maneuver
            /// </summary>
            public double Duration { get; }

            /// <summary>
            /// The arrival time
            /// </summary>
            public double ArrivalTime
            {
                get { return this.StartTime + this.Duration; }
            }

            /// <summary>
            /// The delta velocity
            /// </summary>
            public Vector3d DeltaVelocity { get; }

            /// <summary>
            /// Creates a new possible launch
            /// </summary>
            /// <param name="launchTime">The start time</param>
            /// <param name="launchDuration">The duration of the launch</param>
            /// <param name="deltaVelocity">The delta velocity</param>
            public PossibleLaunch(double launchTime, double launchDuration, Vector3d deltaVelocity)
            {
                this.StartTime = launchTime;
                this.Duration = launchDuration;
                this.DeltaVelocity = deltaVelocity;
            }
        }

        /// <summary>
        /// The possible intercepts
        /// </summary>
        public IList<PossibleLaunch> PossibleIntercepts { get; private set; } = new List<PossibleLaunch>();

        private readonly double maxImpactCheckTime = 1000.0;
        private readonly double impactCheckDeltaTime = 100.0;

        /// <summary>
        /// Creates a new intercept between the given objects
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="orbitPosition">The orbit of the object</param>
        /// <param name="target">The target object</param>
        /// <param name="targetOrbitPosition">The target orbit</param>
        /// <param name="minInterceptDuration">The minimum intercept duration</param>
        /// <param name="maxInterceptDuration">The maximum intercept duration</param>
        /// <param name="minLaunchTime">The minimum launch time</param>
        /// <param name="maxLaunchTime">The maximum launch time</param>
        /// <param name="listPossibleLaunches">Indicates if all the possible launches are returned</param>
        /// <param name="deltaTime">The delta time for the search</param>
        /// <param name="allowedDeltaV">Allowed delta V (quits the search if an orbit with less than this value is found)</param>
        public InterceptManeuver(
            ISimulatorEngine simulatorEngine,
            IPrimaryBodyObject primaryBody,
            IPhysicsObject physicsObject,
            ObjectState state,
            OrbitPosition orbitPosition,
            IPhysicsObject target,
            OrbitPosition targetOrbitPosition,
            double minInterceptDuration,
            double maxInterceptDuration,
            double minLaunchTime,
            double maxLaunchTime,
            double deltaTime,
            bool listPossibleLaunches,
            double? allowedDeltaV = null)
        {
            this.simulatorEngine = simulatorEngine;

            this.primaryBody = primaryBody;
            this.primaryBodyOrbit = new Orbit();
            if (!this.primaryBody.IsObjectOfReference)
            {
                this.primaryBodyOrbit = Orbit.CalculateOrbit(this.primaryBody);
            }

            this.physicsObject = physicsObject;
            this.objectState = state;
            this.objectOrbitPosition = orbitPosition;

            this.target = target;
            this.targetOrbitPosition = targetOrbitPosition;

            this.minInterceptTime = minInterceptDuration;
            this.maxInterceptTime = maxInterceptDuration;
            this.minLaunchTime = minLaunchTime;
            this.maxLaunchTime = maxLaunchTime;
            this.allowedDeltaV = allowedDeltaV;

            this.deltaTime = deltaTime;

            this.listPossibleLaunches = listPossibleLaunches;
        }

        /// <summary>
        /// Indicates if the object is stationary
        /// </summary>
        private bool IsStationary => this.objectState.HasImpacted;

        /// <summary>
        /// The target orbit
        /// </summary>
        private Orbit Orbit => this.objectOrbitPosition.Orbit;

        /// <summary>
        /// The target orbit
        /// </summary>
        private Orbit TargetOrbit => this.targetOrbitPosition.Orbit;

        /// <summary>
        /// Indicates if the given launch is valid
        /// </summary>
        /// <param name="launchState">The launch state</param>
        /// <param name="primaryLaunchState">The state of the primary at launch</param>
        private bool IsValidLaunch(ref ObjectState launchState, ref ObjectState primaryLaunchState)
        {
            if (MathHelpers.IsNaN(launchState.Velocity))
            {
                return false;
            }

            if (!this.IsStationary)
            {
                return true;
            }

            var launchOrbit = Orbit.CalculateOrbit(this.primaryBody, ref primaryLaunchState, ref launchState);

            for (double t = 0.0; t <= this.maxImpactCheckTime; t += this.impactCheckDeltaTime)
            {
                var nextState = simulatorEngine.KeplerProblemSolver.Solve(
                    physicsObject,
                    ref primaryLaunchState,
                    ref launchState,
                    launchOrbit,
                    t);

                if (CollisionHelpers.SphereIntersection(primaryLaunchState.Position, this.primaryBody.Radius, nextState.Position, 10))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates the intercept
        /// </summary>
        /// <param name="primaryBodyStateAtLaunch">The state of the primary body at launch</param>
        /// <param name="primaryBodyStateInitial">The initial state of the primary body</param>
        /// <param name="objectLaunchState">The state of the object at launch</param>
        /// <param name="targetLaunchState">The state of the target at launch</param>
        /// <param name="launchTime">The launch time</param>
        /// <param name="interceptDuration">The duration of the intercept</param>
        private Vector3d? CalculateIntercept(
            ObjectState primaryBodyStateInitial,
            ObjectState primaryBodyStateAtLaunch,
            ObjectState objectLaunchState,
            ObjectState targetLaunchState,
            double launchTime,
            double interceptDuration)
        {
            try
            {
                var primaryBodyStateAtIntercept = new ObjectState();
                if (!this.primaryBody.IsObjectOfReference)
                {
                    primaryBodyStateAtIntercept = this.simulatorEngine.KeplerProblemSolver.Solve(
                        this.primaryBody,
                        this.primaryBody.PrimaryBody.State,
                        primaryBodyStateInitial,
                        this.primaryBodyOrbit,
                        launchTime + interceptDuration);
                }

                var targetInterceptState = this.simulatorEngine.KeplerProblemSolver.Solve(
                    target,
                    ref primaryBodyStateAtLaunch,
                    ref targetLaunchState,
                    this.TargetOrbit,
                    ref primaryBodyStateAtIntercept,
                    interceptDuration);

                var solution1 = simulatorEngine.GaussProblemSolver.Solve(
                    this.primaryBody,
                    primaryBodyStateAtLaunch,
                    primaryBodyStateAtIntercept,
                    objectLaunchState.Position,
                    targetInterceptState.Position,
                    interceptDuration,
                    shortWay: true);

                var solution2 = simulatorEngine.GaussProblemSolver.Solve(
                    this.primaryBody,
                    primaryBodyStateAtLaunch,
                    primaryBodyStateAtIntercept,
                    objectLaunchState.Position,
                    targetInterceptState.Position,
                    interceptDuration,
                    shortWay: false);

                bool valid = true;
                var deltaV1 = solution1.Velocity1 - objectLaunchState.Velocity;
                var deltaV2 = solution2.Velocity1 - objectLaunchState.Velocity;
                var deltaV = Vector3d.Zero;

                var solution1State = new ObjectState(
                    objectLaunchState.Time,
                    objectLaunchState.Position,
                    solution1.Velocity1);

                var solution2State = new ObjectState(
                    objectLaunchState.Time,
                    objectLaunchState.Position,
                    solution2.Velocity1);

                if (deltaV1.Length() < deltaV2.Length() && this.IsValidLaunch(ref solution1State, ref primaryBodyStateAtLaunch))
                {
                    deltaV = deltaV1;
                }
                else
                {
                    deltaV = deltaV2;
                    valid = this.IsValidLaunch(ref solution2State, ref primaryBodyStateAtLaunch);
                }

                if (valid)
                {
                    return deltaV;
                }
            }
            catch
            {
                //Could not be solved!
            }

            return null;
        }

        /// <summary>
        /// Computes the optimal intercept (in terms of deltaV)
        /// </summary>
        public PossibleLaunch Compute()
        {
            var bestLaunch = new PossibleLaunch(0.0, 0.0, new Vector3d(double.MaxValue));
            var foundValid = false;

            var targetInitial = targetOrbitPosition.CalculateState();
            var objectInitial = objectState;

            var primaryBodyStateInitial = this.primaryBody.State;
            for (var launchTime = this.minLaunchTime; launchTime <= this.maxLaunchTime; launchTime += this.deltaTime)
            {
                //Calculate the launch state
                var primaryBodyStateAtLaunch = new ObjectState();
                if (!this.primaryBody.IsObjectOfReference)
                {
                    primaryBodyStateAtLaunch = simulatorEngine.KeplerProblemSolver.Solve(
                        this.primaryBody,
                        this.primaryBody.PrimaryBody.State,
                        primaryBodyStateInitial,
                        this.primaryBodyOrbit,
                        launchTime);
                }

                var objectLaunchState = simulatorEngine.KeplerProblemSolver.Solve(
                    physicsObject,
                    ref primaryBodyStateInitial,
                    ref objectInitial,
                    this.Orbit,
                    ref primaryBodyStateAtLaunch,
                    launchTime);

                var targetLaunchState = simulatorEngine.KeplerProblemSolver.Solve(
                    target,
                    ref primaryBodyStateInitial,
                    ref targetInitial,
                    this.TargetOrbit,
                    ref primaryBodyStateAtLaunch,
                    launchTime);

                //Calculate the intercept state
                for (var interceptDuration = this.minInterceptTime; interceptDuration <= this.maxInterceptTime; interceptDuration += this.deltaTime)
                {
                    var deltaV = this.CalculateIntercept(
                        primaryBodyStateInitial,
                        primaryBodyStateAtLaunch,
                        objectLaunchState,
                        targetLaunchState,
                        launchTime,
                        interceptDuration);

                    if (deltaV != null)
                    {
                        var launch = new PossibleLaunch(launchTime, interceptDuration, deltaV.Value);

                        if (this.listPossibleLaunches)
                        {
                            this.PossibleIntercepts.Add(launch);
                        }

                        if (deltaV.Value.Length() < bestLaunch.DeltaVelocity.Length())
                        {
                            bestLaunch = launch;
                            foundValid = true;

                            if (allowedDeltaV != null && bestLaunch.DeltaVelocity.Length() <= allowedDeltaV)
                            {
                                return bestLaunch;
                            }
                        }
                    }
                }
            }

            if (foundValid)
            {
                return bestLaunch;
            }
            else
            {
                return new PossibleLaunch();
            }
        }

        /// <summary>
        /// Computes the optimal intercept maneuver
        /// </summary>
        /// <returns>The intercept or null, if no intercept is possible</returns>
        public OrbitalManeuvers ComputeManeuver()
        {
            var bestLaunch = this.Compute();
            var foundValid = bestLaunch.DeltaVelocity != Vector3d.Zero;

            if (foundValid)
            {
                return OrbitalManeuvers.Sequence(
                    OrbitalManeuver.Burn(simulatorEngine, physicsObject, bestLaunch.DeltaVelocity, OrbitalManeuverTime.TimeFromNow(bestLaunch.StartTime)),
                    OrbitalManeuver.Burn(simulatorEngine, physicsObject, Vector3d.Zero, OrbitalManeuverTime.TimeFromNow(bestLaunch.ArrivalTime))
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Computes the optimal intercept (in terms of delta V) to the current target orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="target">The target object</param>
        /// <param name="targetOrbitPosition">The target orbit position</param>
        /// <param name="minInterceptDuration">The minimum intercept duration</param>
        /// <param name="maxInterceptDuration">The maximum intercept duration</param>
        /// <param name="minLaunchTime">The minimum launch time</param>
        /// <param name="maxLaunchTime">The maximum launch time</param>
        /// <param name="deltaTime">The delta time for the search</param>
        /// <returns>The intercept or null, if no intercept is possible</returns>
        public static OrbitalManeuvers Intercept(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            IPhysicsObject target,
            OrbitPosition targetOrbitPosition,
            double minInterceptDuration,
            double maxInterceptDuration,
            double minLaunchTime,
            double maxLaunchTime,
            double deltaTime = 1.0 * 60.0)
        {
            var intercept = new InterceptManeuver(
                simulatorEngine,
                physicsObject.PrimaryBody,
                physicsObject,
                physicsObject.State,
                OrbitPosition.CalculateOrbitPosition(physicsObject),
                target,
                targetOrbitPosition,
                minInterceptDuration,
                maxInterceptDuration,
                minLaunchTime,
                maxLaunchTime,
                deltaTime,
                false);
            return intercept.ComputeManeuver();
        }
    }
}
