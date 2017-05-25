using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Solvers;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains methods for calculating various things for orbits
    /// </summary>
    public static class OrbitCalculators
    {
        /// <summary>
        /// Contains data for an approach
        /// </summary>
        public struct ApproachData
        {
            /// <summary>
            /// The distance
            /// </summary>
            public double Distance { get; set; }

            /// <summary>
            /// The time
            /// </summary>
            public double Time { get; set; }
        }

        /// <summary>
        /// Calculates the closest approach between the given orbits
        /// </summary>
        /// <param name="keplerProblemSolver">A solver for the kepler problem</param>
        /// <param name="object1">The first object</param>
        /// <param name="orbitPosition1">The first orbit</param>
        /// <param name="object2">The second object</param>
        /// <param name="orbitPosition2">The second orbit</param>
        /// <param name="deltaTime">The delta time</param>
        /// <exception cref="ArgumentException">If the primary bodies are not the same.</exception>
        public static ApproachData ClosestApproach(
            IKeplerProblemSolver keplerProblemSolver,
            IPhysicsObject object1,
            OrbitPosition orbitPosition1,
            IPhysicsObject object2,
            OrbitPosition orbitPosition2,
            double deltaTime = 600.0 * 30)
        {
            var orbit1 = orbitPosition1.Orbit;
            var orbit2 = orbitPosition2.Orbit;

            if (orbit1.PrimaryBody != orbit2.PrimaryBody)
            {
                throw new ArgumentException("Both orbits must be around a common primary body.");
            }

            var minDist = double.MaxValue;
            var maxChangeRate = double.MinValue;
            var minTime = 0.0;

            var T1 = orbit1.Period;
            var T2 = orbit2.Period;
            var synodicPeriod = OrbitFormulas.SynodicPeriod(T1, T2);
            if (synodicPeriod == 0)
            {
                return new ApproachData();
            }

            if (orbit1.IsUnbound || orbit2.IsUnbound)
            {
                synodicPeriod = 24.0 * 60.0 * 60.0;
            }

            if (deltaTime == -1)
            {
                deltaTime = synodicPeriod / 2000.0;
            }

            var initPrimaryState = orbit1.PrimaryBody.State;
            var totalTime = orbit1.PrimaryBody.State.Time;
            var initState1 = orbitPosition1.CalculateState(ref initPrimaryState);
            var initState2 = orbitPosition2.CalculateState(ref initPrimaryState);

            var t = 0.0;
            var prevDistance = 0.0;
            while (t <= synodicPeriod)
            {
                var s1 = keplerProblemSolver.Solve(
                    object1,
                    ref initPrimaryState,
                    ref initState1,
                    orbit1,
                    t);

                var s2 = keplerProblemSolver.Solve(
                    object2,
                    ref initPrimaryState,
                    ref initState2,
                    orbit2,
                    t);

                var dist = Vector3d.Distance(s1.Position, s2.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTime = totalTime + t;
                }

                //Adjust the delta time 
                var stepRate = 1.0;
                var changeRate = (prevDistance - dist) / deltaTime;

                if (changeRate > maxChangeRate)
                {
                    maxChangeRate = changeRate;
                }

                if (changeRate < 0)
                {
                    stepRate = MathUtild.Lerp(1.0, 50.0, Math.Abs(changeRate) / maxChangeRate);
                }

                t += stepRate * deltaTime;
                prevDistance = dist;
            }
            //Debug.Log((DateTime.UtcNow - startTime) + ": " + DataFormatter.Format(deltaTime, DataUnit.Time));

            return new ApproachData()
            {
                Distance = minDist,
                Time = minTime
            };
        }

        /// <summary>
        /// Calculates the time to leave the current sphere-of-influence for an unbound orbit
        /// </summary>
        /// <param name="orbit">The current orbit</param>
        /// <returns>The solution if exists</returns>
        public static double? TimeToLeaveSphereOfInfluenceUnboundOrbit(OrbitPosition orbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            if (orbit.IsUnbound)
            {
                //If we are really close to full rotation, switch to zero to avoid problems with negative time.
                if (2.0 * Math.PI - orbitPosition.TrueAnomaly <= 1E-5)
                {
                    orbitPosition.TrueAnomaly = 0.0;
                }

                var soiObject = orbit.PrimaryBody;
                var nextSoiObject = soiObject.PrimaryBody;
                if (nextSoiObject != null)
                {
                    var soi = OrbitFormulas.SphereOfInfluence(
                        Orbit.CalculateOrbit(soiObject).SemiMajorAxis,
                        soiObject.Mass,
                        nextSoiObject.Mass);

                    var found = OrbitFormulas.TrueAnomalyAt(
                        soi,
                        orbit.Parameter,
                        orbit.Eccentricity,
                        out var trueAnomaly1,
                        out var trueAnomaly2);

                    if (found)
                    {
                        var leaveAngle = 0.0;
                        if (Math.Abs(orbitPosition.TrueAnomaly - trueAnomaly1) < Math.Abs(orbitPosition.TrueAnomaly - trueAnomaly2))
                        {
                            leaveAngle = trueAnomaly1;
                        }
                        else
                        {
                            leaveAngle = trueAnomaly2;
                        }

                        var timeToLeave = orbitPosition.TimeToTrueAnomaly(leaveAngle);
                        if (timeToLeave > 0)
                        {
                            return timeToLeave;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates the time to impact for the given orbit
        /// </summary>
        /// <param name="orbit">The current orbit</param>
        /// <returns>The time, if the object will impact</returns>
        public static double? TimeToImpact(OrbitPosition orbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            if (orbit.Periapsis <= orbit.PrimaryBody.Radius)
            {
                if (OrbitFormulas.TrueAnomalyAt(orbit.PrimaryBody.Radius, orbit.Parameter, orbit.Eccentricity, out var trueAnomaly1, out var trueAnomaly2))
                {
                    var impactAngle = 0.0;
                    if (Math.Abs(orbitPosition.TrueAnomaly - trueAnomaly1) < Math.Abs(orbitPosition.TrueAnomaly - trueAnomaly2))
                    {
                        impactAngle = trueAnomaly1;
                    }
                    else
                    {
                        impactAngle = trueAnomaly2;
                    }

                    return orbitPosition.TimeToTrueAnomaly(impactAngle);
                }
            }

            return null;
        }
    }
}
