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
    /// Represents an atmospheric model for the earth
    /// </summary>
    /// <remarks>See <see cref="https://www.grc.nasa.gov/www/k-12/airplane/atmosmet.html"/></remarks>
    public sealed class EarthAtmosphericModel : IAtmosphericModel
    {
        /// <summary>
        /// The altitude when the atmosphere ends
        /// </summary>
        public static readonly double EndOfAtmosphereAltitude = 300E3;

        /// <summary>
        /// Calculates the pressure and temperature at the given altitude
        /// </summary>
        /// <param name="altitude">The altitude</param>
        /// <returns>(Pressure, Temperature)</returns>
        public (double, double) PressureAndTemperature(double altitude)
        {
            double temperature = 0.0;
            double pressure = 0.0;

            if (altitude < 11000)
            {
                temperature = 15.04 - 0.00649 * altitude;
                pressure = 101.29 * Math.Pow((temperature + 273.1) / 288.08, 5.256);
            }
            else if (altitude >= 11000 && altitude < 25000)
            {
                temperature = -56.46;
                pressure = 22.65 * Math.Exp(1.73 - 0.000157 * altitude);
            }
            else
            {
                temperature = -131.21 + 0.00299 * altitude;
                pressure = 2.488 * Math.Pow((temperature + 273.1) / 216.6, -11.388);
            }

            pressure *= 1000;
            temperature = temperature + Math.Abs(Constants.AbsoluteZero);

            return (pressure, temperature);
        }

        /// <summary>
        /// Calculates the density of the air at the given altitude
        /// </summary>
        /// <param name="altitude">The altitude</param>
        public double DensityOfAir(double altitude)
        {
            (var pressure, var temperature) = PressureAndTemperature(altitude);
            return AtmosphericFormulas.DensityOfAir(pressure, temperature);
        }

        /// <summary>
        /// Indicates if the given object is inside the atmosphere
        /// </summary>
        /// <param name="primaryBodyConfig">The configuration of the primary body</param>
        /// <param name="primaryBodyState">The state of the primary body</param>
        /// <param name="state">The state of the object</param>
        public bool Inside(ObjectConfig primaryBodyConfig, ref ObjectState primaryBodyState, ref ObjectState state)
        {
            var altitude = OrbitFormulas.Altitude(primaryBodyState.Position, primaryBodyConfig.Radius, state.Position);
            return altitude < EndOfAtmosphereAltitude;
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
            var v = state.Velocity - primaryBodyState.Velocity;
            var altitude = OrbitFormulas.Altitude(primaryBodyState.Position, primaryBodyConfig.Radius, state.Position);
            if (altitude >= EndOfAtmosphereAltitude)
            {
                return Vector3d.Zero;
            }

            var densityOfAir = this.DensityOfAir(altitude);
            return AtmosphericFormulas.Drag(v, densityOfAir, properties.ReferenceArea, properties.DragCoefficient);
        }
    }
}
