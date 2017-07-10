using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains common orbits
    /// </summary>
    public static class Orbits
    {
        /// <summary>
        /// Returns a geosynchronous orbit over the given body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="inclination">The inclination</param>
        public static Orbit GeosynchronousOrbit(IPrimaryBodyObject primaryBody, double inclination)
        {
            return Orbit.New(
                primaryBody,
                semiMajorAxis: OrbitFormulas.SemiMajorAxisFromOrbitalPeriod(primaryBody.StandardGravitationalParameter, primaryBody.RotationalPeriod),
                inclination: inclination);
        }

        /// <summary>
        /// Returns a geostationary orbit over the given body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        public static Orbit GeostationaryOrbit(IPrimaryBodyObject primaryBody) => GeosynchronousOrbit(primaryBody, 0.0);
    }
}
