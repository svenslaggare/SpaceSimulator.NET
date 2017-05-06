using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Atmosphere
{
    /// <summary>
    /// Represents an atmospheric model for a body without any atmosphere
    /// </summary>
    public sealed class NoAtmosphereModel : IAtmosphericModel
    {
        /// <summary>
        /// Calculates the pressure and temperature at the given altitude
        /// </summary>
        /// <param name="altitude">The altitude</param>
        /// <returns>(Pressure, Temperature)</returns>
        public (double, double) PressureAndTemperature(double altitude)
        {
            return (0, 0);
        }

        /// <summary>
        /// Indicates if the given object is inside the atmosphere
        /// </summary>
        /// <param name="primaryBodyConfig">The configuration of the primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="state">The state of the object</param>
        public bool Inside(ObjectConfig primaryBodyConfig, ref ObjectState primaryBodyState, ref ObjectState state)
        {
            return false;
        }

        /// <summary>
        /// Calculates the drag of the given object
        /// </summary>
        /// <param name="primaryBodyConfig">The configuration of the primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="properties">The atmospheric properties of the object</param>
        /// <param name="state">The state of the object</param>
        /// <returns>The drag force</returns>
        public Vector3d CalculateDrag(ObjectConfig primaryBodyConfig, ref ObjectState primaryBodyState, AtmosphericProperties properties, ref ObjectState state)
        {
            return Vector3d.Zero;
        }
    }
}
