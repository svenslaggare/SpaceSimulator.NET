﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// Contains physics constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The universal gravity constant (G) in SI units
        /// </summary>
        public const double G = 6.67408E-11;

        /// <summary>
        /// The astronomical unit in meters
        /// </summary>
        public const double AstronomicalUnit = 149597870700;

        /// <summary>
        /// A sidereal day in seconds
        /// </summary>
        public const double SiderealDay = 23.0 * 60.0 * 60.0 + 56.0 * 60.0 + 4.0916;

        /// <summary>
        /// The standard gravity in m/s^2
        /// </summary>
        public const double StandardGravity = 9.80665;

        /// <summary>
        /// The specific gas constant for the earth
        /// </summary>
        public const double EarthSpecificGasConstant = 287.058;

        /// <summary>
        /// The absolute zero temperature (in C)
        /// </summary>
        public const double AbsoluteZero = -273.15;
    }
}
