using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains physics formulas
    /// </summary>
    public static class PhysicsFormulas
    {
        /// <summary>
        /// Calculates the moment-of-inertia for a sphere
        /// </summary>
        /// <param name="mass">The mass</param>
        /// <param name="radius">The radius</param>
        public static double MomentOfInertiaForSphere(double mass, double radius)
        {
            return (2.0 / 5.0) * mass * radius * radius;
        }
    }
}
