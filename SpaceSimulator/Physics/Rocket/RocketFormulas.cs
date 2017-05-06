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
        public static double CalculateMassFlowRate(double thrust, double specificImpulse)
        {
            return thrust / (Constants.StandardGravity * specificImpulse);
        }
    }
}
