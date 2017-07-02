using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Solvers
{
    /// <summary>
    /// Contains helper methods for solvers
    /// </summary>
    public static class SolverHelpers
    {
        /// <summary>
        /// Calculates the rotation of an object
        /// </summary>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="rotation">The current rotation</param>
        /// <param name="time">The amount of time to rotate for</param>
        public static double CalculateRotation(double rotationalPeriod, double rotation, double time)
        {
            if (rotationalPeriod == 0)
            {
                return rotation;
            }

            var rotationalSpeed = (2.0 * Math.PI) / rotationalPeriod;
            return MathHelpers.ClampAngle(rotation + rotationalSpeed * time);
        }

        /// <summary>
        /// Moves an impacted object following the motion of the primary body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialPrimaryBodyState">The initial state of the primary body</param>
        /// <param name="nextPrimaryBodyState">The next state of the primary body</param>
        /// <param name="state">The state</param>
        /// <param name="time">The amount of time to move</param>
        public static ObjectState MoveImpactedObject(
            IPrimaryBodyObject primaryBody,
            ObjectState initialPrimaryBodyState,
            ObjectState nextPrimaryBodyState,
            ObjectState state,
            double time)
        {
            if (primaryBody.RotationalPeriod == 0)
            {
                state.SwapReferenceFrame(initialPrimaryBodyState, nextPrimaryBodyState);
                state.Time += time;
                return state;
            }

            //Calculate the new position
            var r = state.Position - initialPrimaryBodyState.Position;
            OrbitHelpers.GetSphericalCoordinates(r, out var latitude, out var longitude);
            longitude += primaryBody.RotationalSpeed() * time;
            var nextPosition = OrbitHelpers.FromSphericalCoordinates(latitude, longitude, r.Length());

            //Calculate the surface velocity due to rotation of primary body
            var surfaceSpeedDir = Vector3d.Cross(MathHelpers.Normalized(nextPosition), primaryBody.AxisOfRotation);
            surfaceSpeedDir.Normalize();
            var velocity = OrbitHelpers.SurfaceSpeedDueToRotation(primaryBody, Math.PI / 2.0 - latitude) * surfaceSpeedDir;

            return new ObjectState(
                nextPrimaryBodyState.Time + time,
                nextPrimaryBodyState.Position + nextPosition,
                nextPrimaryBodyState.Velocity + velocity,
                state.Acceleration,
                rotation: state.Rotation,
                impacted: state.HasImpacted);
        }

        /// <summary>
        /// Calculates the state after the given amount of time
        /// </summary>
        /// <param name="keplerProblemSolver">A solver for the kepler problem</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="orbit">The orbit</param>
        /// <param name="time">The time</param>
        /// <param name="relative">Indicates if the that should be relative</param>
        /// <param name="nextState">The next state</param>
        /// <param name="nextPrimaryBodyState">The next primary body state</param>
        /// <remarks>This method does not take SOI changes or maneuvers into account.</remarks>
        public static void AfterTime(
            IKeplerProblemSolver keplerProblemSolver,
            IPhysicsObject physicsObject,
            ObjectState state,
            Orbit orbit,
            double time,
            out ObjectState nextState,
            out ObjectState nextPrimaryBodyState,
            bool relative = false)
        {
            var primaryBodyState = new ObjectState();
            if (!orbit.PrimaryBody.IsObjectOfReference)
            {
                var primaryBodyOrbit = Orbit.CalculateOrbit(orbit.PrimaryBody);
                primaryBodyState = AfterTime(
                    keplerProblemSolver,
                    orbit.PrimaryBody,
                    orbit.PrimaryBody.State,
                    primaryBodyOrbit,
                    time,
                    false/*relative*/);
            }

            nextState = keplerProblemSolver.Solve(
                physicsObject,
                orbit.PrimaryBody.State,
                state,
                orbit,
                relative ? new ObjectState() : primaryBodyState,
                time);

            nextPrimaryBodyState = primaryBodyState;
        }

        /// <summary>
        /// Calculates the state after the given amount of time
        /// </summary>
        /// <param name="keplerProblemSolver">A solver for the kepler problem</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="orbit">The orbit</param>
        /// <param name="time">The time</param>
        /// <param name="relative">Indicates if the that should be relative</param>
        /// <remarks>This method does not take SOI changes or maneuvers into account.</remarks>
        public static ObjectState AfterTime(
            IKeplerProblemSolver keplerProblemSolver,
            IPhysicsObject physicsObject,
            ObjectState state,
            Orbit orbit,
            double time,
            bool relative = false)
        {
            AfterTime(
                keplerProblemSolver,
                physicsObject,
                state,
                orbit,
                time,
                out var nextState,
                out var nextPrimaryBodyState,
                relative);
            return nextState;
        }
    }
}
