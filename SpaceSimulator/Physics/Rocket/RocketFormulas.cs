using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics.Rocket
{
    /// <summary>
    /// Contains fornulas for rockets
    /// </summary>
    public static class RocketFormulas
    {
        /// <summary>
        /// Calculates the mass flow rate (kg/s)
        /// </summary>
        /// <param name="thrust">The thrust (in newtons)</param>
        /// <param name="specificImpulse">The specific impulse (in seconds)</param>
        public static double MassFlowRate(double thrust, double specificImpulse)
        {
            return thrust / (Constants.StandardGravity * specificImpulse);
        }

        /// <summary>
        /// Calculates the effective exhaust velocity
        /// </summary>
        /// <param name="specificImpulse">The specific impluse (in seconds)</param>
        public static double EffectiveExhaustVelocity(double specificImpulse)
        {
            return specificImpulse * Constants.StandardGravity;
        }

        /// <summary>
        /// Calculates the delta V produced by the given burn
        /// </summary>
        /// <param name="effectiveExhaustVelocity">The effective exhaust velocity</param>
        /// <param name="beforeMass">The mass before the burn</param>
        /// <param name="afterMass">The mass after the burn</param>
        public static double DeltaV(double effectiveExhaustVelocity, double beforeMass, double afterMass)
        {
            return effectiveExhaustVelocity * Math.Log(beforeMass / afterMass);
        }
    }
}
