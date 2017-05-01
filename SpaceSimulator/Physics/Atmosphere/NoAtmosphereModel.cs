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
        public Vector3d CalculateDrag(ObjectConfig primaryBodyConfig, ref ObjectState primaryBodyState, AtmosphericProperties properties, ref ObjectState state)
        {
            return Vector3d.Zero;
        }
    }
}
