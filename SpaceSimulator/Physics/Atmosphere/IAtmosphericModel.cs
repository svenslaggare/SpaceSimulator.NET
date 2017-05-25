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
        /// Calculates the pressure and temperature at the given altitude
        /// </summary>
        /// <param name="altitude">The altitude</param>
        /// <returns>(Pressure, Temperature)</returns>
        (double, double) PressureAndTemperature(double altitude);

        /// <summary>
        /// Indicates if the given object is inside the atmosphere
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="state">The state of the object</param>
        bool Inside(IPrimaryBodyObject primaryBody, ref ObjectState primaryBodyState, ref ObjectState state);

        /// <summary>
        /// Calculates the drag force of the given object
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="properties">The atmospheric properties of the object</param>
        /// <param name="state">The state of the object</param>
        /// <returns>The drag force</returns>
        Vector3d CalculateDrag(IPrimaryBodyObject primaryBody, ref ObjectState primaryBodyState, AtmosphericProperties properties, ref ObjectState state);
    }

    /// <summary>
    /// Contains extensions methods for the <see cref="IAtmosphericModel"/> interface
    /// </summary>
    public static class IAtmosphericModelExtensions
    {
        /// <summary>
        /// Indicates if the given object is inside the atmosphere
        /// </summary>
        /// <param name="model">The current model</param>
        /// <param name="primaryBody">The body that the atmosphere is applied to</param>
        /// <param name="state">The state of the object</param>
        public static bool Inside(this IAtmosphericModel model, IPrimaryBodyObject primaryBody, ref ObjectState state)
        {
            var primaryBodyState = primaryBody.State;
            return model.Inside(primaryBody, ref primaryBodyState, ref state);
        }
    }
}
