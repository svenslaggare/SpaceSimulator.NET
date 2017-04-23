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
    /// Contains method for calculating intercepts
    /// </summary>
    public static class InterceptManeuver
    {
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
        /// Indicates if the given launch is valid
        /// </summary>
        private static bool IsValidLaunch(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject primaryBody,
            ObjectConfig config,
            ref ObjectState launchState,
            ref ObjectState primaryLaunchState,
            double impactCheckDeltaTime,
            double maxImpactCheckTime,
            bool stationary)
        {
            if (MathHelpers.IsNaN(launchState.Velocity))
            {
                return false;
            }

            if (!stationary)
            {
                return true;
            }

            var launchOrbit = Orbit.CalculateOrbit(primaryBody, ref primaryLaunchState, ref launchState);

            for (double t = 0; t <= maxImpactCheckTime; t += impactCheckDeltaTime)
            {
                var nextState = simulatorEngine.KeplerProblemSolver.Solve(
                    config,
                    ref primaryLaunchState,
                    ref launchState,
                    launchOrbit,
                    t);

                if (CollisionHelpers.SphereIntersection(primaryLaunchState.Position, primaryBody.Configuration.Radius, nextState.Position, config.Radius))
                {
                    return false;
                }
            }

            return true; ;
        }

        /// <summary>
        /// Calculates the intercept
        /// </summary>
        private static Vector3d? CalculateIntercept(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject primaryBody,
            ObjectState primaryBodyStateInitial,
            ObjectState primaryBodyStateAtLaunch,
            Orbit primaryBodyOrbit,
            ObjectConfig targetConfig,
            Orbit targetOrbit,
            ObjectState s1,
            ObjectState objectLaunchState,
            double launchTime,
            double interceptTime,
            Func<ObjectState, ObjectState, bool> validLaunch)
        {
            try
            {
                var primaryBodyStateAtIntercept = new ObjectState();
                if (!primaryBody.IsObjectOfReference)
                {
                    primaryBodyStateAtIntercept = simulatorEngine.KeplerProblemSolver.Solve(
                        primaryBody.Configuration,
                        primaryBody.PrimaryBody.State,
                        primaryBodyStateInitial,
                        primaryBodyOrbit,
                        launchTime + interceptTime);
                }

                var targetInterceptState = simulatorEngine.KeplerProblemSolver.Solve(
                    targetConfig,
                    ref primaryBodyStateAtLaunch,
                    ref s1,
                    targetOrbit,
                    ref primaryBodyStateAtIntercept,
                    interceptTime);

                var res1 = simulatorEngine.GaussProblemSolver.Solve(
                    primaryBody,
                    primaryBodyStateAtLaunch,
                    primaryBodyStateAtIntercept,
                    objectLaunchState.Position,
                    targetInterceptState.Position,
                    interceptTime,
                    shortWay: true);

                var res2 = simulatorEngine.GaussProblemSolver.Solve(
                   primaryBody,
                   primaryBodyStateAtLaunch,
                   primaryBodyStateAtIntercept,
                   objectLaunchState.Position,
                   targetInterceptState.Position,
                   interceptTime,
                   shortWay: false);

                bool valid = true;
                var deltaV1 = res1.Velocity1 - objectLaunchState.Velocity;
                var deltaV2 = res2.Velocity1 - objectLaunchState.Velocity;
                var deltaV = Vector3d.Zero;

                if (deltaV1.Length() < deltaV2.Length()
                    && validLaunch(new ObjectState(objectLaunchState.Time, objectLaunchState.Position, res1.Velocity1, Vector3d.Zero), primaryBodyStateAtLaunch))
                {
                    deltaV = deltaV1;
                }
                else
                {
                    deltaV = deltaV2;
                    valid = validLaunch(new ObjectState(objectLaunchState.Time, objectLaunchState.Position, res2.Velocity1, Vector3d.Zero), primaryBodyStateAtLaunch);
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
        /// Computes the optimal intercept (in terms of deltaV) to the current target orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="config">The configuration of the object</param>
        /// <param name="state">The state of the object</param>
        /// <param name="orbit">The orbit of the object</param>
        /// <param name="targetConfig">The configuration of the target object</param>
        /// <param name="targetOrbit">The target orbit</param>
        /// <param name="minInterceptTime">The minimum intercept time</param>
        /// <param name="maxInterceptTime">The maximum intercept time</param>
        /// <param name="minLaunchTime">The minimum launch time</param>
        /// <param name="maxLaunchTime">The maximum launch time</param>
        /// <param name="optimalDeltaV">The optimal delta V</param>
        /// <param name="optimalLaunchTime">The time to apply the optimal maneuver</param>
        /// <param name="optimalInterceptTime">The intercept for the optimal maneuver</param>
        /// <param name="listPossibleLaunches">Indicates if all the possible launches are returned</param>
        /// <param name="deltaTime">The delta time for the search</param>
        /// <param name="allowedDeltaV">Allowed delta V (quits the search if an orbit with less than this value is found)</param>
        /// <returns>The possible launches, or null if listPossibleLaunches = false</returns>
        /// <remarks>This method does not execute the maneuver, only plans it.</remarks>
        public static IList<PossibleLaunch> Intercept(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject primaryBody,
            ObjectConfig config,
            ObjectState state,
            OrbitPosition orbitPosition,
            ObjectConfig targetConfig,
            OrbitPosition targetOrbitPosition,
            double minInterceptTime, double maxInterceptTime, double minLaunchTime, double maxLaunchTime,
            out Vector3d optimalDeltaV, out double optimalLaunchTime, out double optimalInterceptTime,
            double deltaTime, bool listPossibleLaunches,
            double? allowedDeltaV = null)
        {
            var orbit = orbitPosition.Orbit;
            var targetOrbit = targetOrbitPosition.Orbit;

            var stationary = state.Impacted;
            var maxImpactCheckTime = 1000.0;
            var impactCheckDeltaTime = 100.0;

            var bestDeltaV = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            var bestLaunchTime = 0.0;
            var bestInterceptTime = 0.0;
            var foundValid = false;

            var targetInitial = targetOrbitPosition.CalculateState();
            var objectInitial = state;
            var objectOrbit = orbit;

            var primaryBodyOrbit = new Orbit();
            if (!primaryBody.IsObjectOfReference)
            {
                primaryBodyOrbit = Orbit.CalculateOrbit(primaryBody);
            }

            IList<PossibleLaunch> possibleLaunches = null;
            if (listPossibleLaunches)
            {
                possibleLaunches = new List<PossibleLaunch>();
            }

            Func<ObjectState, ObjectState, bool> validLaunch = (primaryLaunchState, launchState) =>
            {
                return IsValidLaunch(simulatorEngine, primaryBody, config, ref launchState, ref primaryLaunchState, impactCheckDeltaTime, maxImpactCheckTime, stationary);
            };

            var primaryBodyStateInitial = primaryBody.State;
            for (var launchTime = minLaunchTime; launchTime <= maxLaunchTime; launchTime += deltaTime)
            {
                //Calculate the launch state
                var primaryBodyStateAtLaunch = new ObjectState();
                if (!primaryBody.IsObjectOfReference)
                {
                    primaryBodyStateAtLaunch = simulatorEngine.KeplerProblemSolver.Solve(
                        primaryBody.Configuration,
                        primaryBody.PrimaryBody.State,
                        primaryBodyStateInitial,
                        primaryBodyOrbit,
                        launchTime);
                }

                var objectLaunchState = simulatorEngine.KeplerProblemSolver.Solve(
                    config,
                    ref primaryBodyStateInitial,
                    ref objectInitial,
                    objectOrbit,
                    ref primaryBodyStateAtLaunch,
                    launchTime);

                var targetLaunchState = simulatorEngine.KeplerProblemSolver.Solve(
                    targetConfig,
                    ref primaryBodyStateInitial,
                    ref targetInitial,
                    targetOrbit,
                    ref primaryBodyStateAtLaunch,
                    launchTime);

                //Calculate the intercept state
                for (var interceptTime = minInterceptTime; interceptTime <= maxInterceptTime; interceptTime += deltaTime)
                {
                    var deltaV = CalculateIntercept(
                        simulatorEngine,
                        primaryBody,
                        primaryBodyStateInitial,
                        primaryBodyStateAtLaunch,
                        primaryBodyOrbit,
                        targetConfig,
                        targetOrbit,
                        targetLaunchState,
                        objectLaunchState,
                        launchTime,
                        interceptTime,
                        validLaunch);

                    if (deltaV != null)
                    {
                        if (listPossibleLaunches)
                        {
                            possibleLaunches.Add(new PossibleLaunch(launchTime, interceptTime, deltaV.Value));
                        }

                        if (deltaV.Value.Length() < bestDeltaV.Length())
                        {
                            bestDeltaV = deltaV.Value;
                            bestLaunchTime = launchTime;
                            bestInterceptTime = interceptTime;
                            foundValid = true;

                            if (allowedDeltaV != null && bestDeltaV.Length() <= allowedDeltaV)
                            {
                                goto exit;
                            }
                        }
                    }
                }
            }

            exit:
            if (foundValid)
            {
                optimalDeltaV = bestDeltaV;
                optimalLaunchTime = bestLaunchTime;
                optimalInterceptTime = bestInterceptTime;
            }
            else
            {
                optimalDeltaV = Vector3d.Zero;
                optimalLaunchTime = 0;
                optimalInterceptTime = 0;
            }

            return possibleLaunches;
        }

        /// <summary>
        /// Computes the optimal intercept (in terms of delta V) to the current target orbit
        /// </summary>
        /// <param name="simulatorEngine">The simulation engine</param>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="targetConfig">The configuration of the target orbit</param>
        /// <param name="targetOrbitPosition">The target orbit position</param>
        /// <param name="minInterceptTime">The minimum intercept time</param>
        /// <param name="maxInterceptTime">The maximum intercept time</param>
        /// <param name="minLaunchTime">The minimum launch time</param>
        /// <param name="maxLaunchTime">The maximum launch time</param>
        /// <param name="deltaTime">The delta time for the search</param>
        /// <returns>The intercept or null, if no intercept is possible</returns>
        public static OrbitalManeuvers Intercept(
            ISimulatorEngine simulatorEngine,
            IPhysicsObject physicsObject,
            ObjectConfig targetConfig,
            OrbitPosition targetOrbitPosition,
            double minInterceptTime, double maxInterceptTime, double minLaunchTime, double maxLaunchTime,
            double deltaTime = 1.0 * 60.0)
        {
            var optimalDeltaV = Vector3d.Zero;
            Intercept(
                 simulatorEngine,
                 physicsObject.PrimaryBody,
                 physicsObject.Configuration,
                 physicsObject.State,
                 OrbitPosition.CalculateOrbitPosition(physicsObject),
                 targetConfig,
                 targetOrbitPosition,
                 minInterceptTime,
                 maxInterceptTime,
                 minLaunchTime,
                 maxLaunchTime,
                 out optimalDeltaV,
                 out var optimalLaunchTime,
                 out var optimalInterceptTime,
                 deltaTime,
                 listPossibleLaunches: false);

            var foundValid = optimalDeltaV != Vector3d.Zero;

            if (foundValid)
            {
                return OrbitalManeuvers.Sequence(
                    OrbitalManeuver.Burn(simulatorEngine, physicsObject, optimalDeltaV, OrbitalManeuverTime.TimeFromNow(optimalLaunchTime)),
                    OrbitalManeuver.Burn(simulatorEngine, physicsObject, Vector3d.Zero, OrbitalManeuverTime.TimeFromNow(optimalLaunchTime + optimalInterceptTime))
                );
            }
            else
            {
                return null;
            }
        }
    }
}
