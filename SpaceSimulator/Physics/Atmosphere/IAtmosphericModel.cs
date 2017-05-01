using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Atmosphere
{
    /// <summary>
    /// Represents an atmospheric model
    /// </summary>
    public interface IAtmosphericModel
    {
        /// <summary>
        /// Calculates the drag force of the given object
        /// </summary>
        /// <param name="primaryBodyConfig">The configuration of the primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="properties">The atmospheric properties of the object</param>
        /// <param name="state">The state of the object</param>
        /// <returns>The drag force</returns>
        Vector3d CalculateDrag(ObjectConfig primaryBodyConfig, ref ObjectState primaryBodyState, AtmosphericProperties properties, ref ObjectState state);
    }
}
