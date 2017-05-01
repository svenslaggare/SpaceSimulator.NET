using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics.Atmosphere
{
    /// <summary>
    /// Contains formulas for atmospheric models
    /// </summary>
    public static class AtmosphericFormulas
    {
        /// <summary>
        /// Calculates the drag force using the drag equation
        /// </summary>
        /// <param name="velocity">The velcoity (in m/s)</param>
        /// <param name="densityOfAir">The density of the air (kg/m^3)</param>
        /// <param name="referenceArea">The reference area (m)</param>
        /// <param name="dragCoefficient">The drag coefficent</param>
        /// <returns>The drag force, in opposite direction of the velocity vector</returns>
        public static Vector3d Drag(Vector3d velocity, double densityOfAir, double referenceArea, double dragCoefficient)
        {
            return -0.5 * densityOfAir * velocity * velocity.Length() * dragCoefficient * referenceArea;
        }

        /// <summary>
        /// Calculates the density of the air
        /// </summary>
        /// <param name="pressure">The pressure (in Pa)</param>
        /// <param name="temperature">The temperature (in K)</param>
        public static double DensityOfAir(double pressure, double temperature)
        {
            return pressure / (Constants.EarthSpecificGasConstant * temperature);
        }

        /// <summary>
        /// Returns the area of a rocket cone nose
        /// </summary>
        /// <param name="baseRadius">The radius of the base</param>
        /// <param name="height">The height of the nose</param>
        public static double ConeNoseSurfaceArea(double baseRadius, double height)
        {
            return Math.PI * baseRadius * Math.Sqrt(baseRadius * baseRadius + height * height);
        }
    }
}
