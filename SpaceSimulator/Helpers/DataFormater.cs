﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;
using SpaceSimulator.Simulator.Data;

namespace SpaceSimulator.Helpers
{
    /// <summary>
    /// Represents a data unit
    /// </summary>
    public enum DataUnit
    {
        /// <summary>
        /// No unit
        /// </summary>
        NoUnit,
        /// <summary>
        /// The time in seconds
        /// </summary>
        Time,
        /// <summary>
        /// The distance in meters
        /// </summary>
        Distance,
        /// <summary>
        /// The velocity in meter/second
        /// </summary>
        Velocity,
        /// <summary>
        /// The acceleration in meter/second^2
        /// </summary>
        Acceleration,
        /// <summary>
        /// The force in newtons
        /// </summary>
        Force,
        /// <summary>
        /// The angle in degrees
        /// </summary>
        Angle,
        /// <summary>
        /// The angle in x° y' y''
        /// </summary>
        DegreesAndMinutesAndSeconds,
        /// <summary>
        /// The angle in latitude
        /// </summary>
        Latitude,
        /// <summary>
        /// The angle in longitude
        /// </summary>
        Longitude,
        /// <summary>
        /// The distance in earth radii
        /// </summary>
        EarthRadii,
        /// <summary>
        /// The mass in earth masses
        /// </summary>
        EarthMass,
        /// <summary>
        /// The distance in astronomical units
        /// </summary>
        AstronomicalUnits,
        /// <summary>
        /// The mass in sun masses
        /// </summary>
        SolarMass,
        /// <summary>
        /// The mass in kg
        /// </summary>
        Mass,
        /// <summary>
        /// The density in kg/m^3
        /// </summary>
        Density,
        /// <summary>
        /// The pressure in Pascal
        /// </summary>
        Pressure,
        /// <summary>
        /// The temperature in celsius
        /// </summary>
        TemperatureCelsius
    }

    /// <summary>
    /// Represents a data formatter
    /// </summary>
    /// <remarks>All values assumed to be in SI units, and angles in radians.</remarks>
    public static class DataFormatter
    {
        private static readonly IList<KeyValuePair<string, double>> negativePrefixes = new List<KeyValuePair<string, double>>
        {
           new KeyValuePair<string, double>("µ", 1E-6),
           new KeyValuePair<string, double>("m", 1E-3),
           new KeyValuePair<string, double>("c", 1E-2),
           new KeyValuePair<string, double>("", 0),
        };

        private static readonly IList<KeyValuePair<string, double>> positivePrefixes = new List<KeyValuePair<string, double>>
        {
           new KeyValuePair<string, double>("", 0),
           new KeyValuePair<string, double>("k", 1E3),
           new KeyValuePair<string, double>("M", 1E6),
           new KeyValuePair<string, double>("G", 1E9),
           new KeyValuePair<string, double>("T", 1E12),
           new KeyValuePair<string, double>("P", 1E15),
           new KeyValuePair<string, double>("E", 1E18),
           new KeyValuePair<string, double>("Z", 1E21),
        };

        private static readonly IDictionary<DataUnit, string> unitNames = new Dictionary<DataUnit, string>()
        {
            { DataUnit.Time, "s" },
            { DataUnit.Distance, "m" },
            { DataUnit.Velocity, "m/s" },
            { DataUnit.Acceleration, "m/s²" },
            { DataUnit.Force, "N" },
            { DataUnit.Mass, "kg" },
            { DataUnit.Density, "kg/m³" },
            { DataUnit.Pressure, "Pa" },
            //{ DataUnit.Temperature, "C" }
        };

        private static readonly IDictionary<DataUnit, Func<double, int, string>> nonPrefix = new Dictionary<DataUnit, Func<double, int, string>>()
        {
            { DataUnit.Angle, (value, decimals) => Math.Round(MathUtild.Rad2Deg * value, decimals) + "°" },
            { DataUnit.DegreesAndMinutesAndSeconds, (value, decimals) =>
            {
                var valueInDegrees = value * MathUtild.Rad2Deg;
                var degrees = Math.Floor(valueInDegrees);
                var minutes = Math.Floor(60.0 * (valueInDegrees - degrees));
                var seconds = 3600.0 * (valueInDegrees - degrees) - 60.0 * minutes;
                return degrees + "°" + " " + minutes + "′" + " " + Math.Round(seconds, decimals) + "″";
            } },
            { DataUnit.Latitude, (value, decimals) => Format(Math.Abs(value), DataUnit.DegreesAndMinutesAndSeconds, decimals) + " " + (value > 0 ? "N" : "S") },
            { DataUnit.Longitude, (value, decimals) => Format(Math.Abs(value), DataUnit.DegreesAndMinutesAndSeconds, decimals) + " " + (value > 0 ? "E" : "W") },
            { DataUnit.EarthRadii, (value, decimals) => Math.Round(value / SolarSystemBodies.Earth.Radius, decimals) + " ER" },
            { DataUnit.EarthMass, (value, decimals) => Math.Round(value / SolarSystemBodies.Earth.Mass, decimals) + " EM" },
            { DataUnit.SolarMass, (value, decimals) => Math.Round(value / SolarSystemBodies.Sun.Mass, decimals) + " SM" },
            { DataUnit.AstronomicalUnits, (value, decimals) => Math.Round(value / Constants.AstronomicalUnit, decimals) + " AU" },
            { DataUnit.TemperatureCelsius, (value, decimals) => Math.Round(value - Math.Abs(Constants.AbsoluteZero), decimals) + " °C" }
        };

        private const int defaultNumDecimals = 4;

        /// <summary>
        /// Formats and logs to the unity console the given data
        /// </summary>
        /// <param name="value">The numeric value</param>
        /// <param name="unit">The unit</param>
        /// <param name="numDecimals">The number of decimals</param>
        public static void Log(double value, DataUnit unit, int numDecimals = defaultNumDecimals)
        {
            Console.WriteLine(Format(value, unit, numDecimals));
        }

        /// <summary>
        /// Formats the given data
        /// </summary>
        /// <param name="value">The numeric value</param>
        /// <param name="unit">The unit</param>
        /// <param name="numDecimals">The number of decimals</param>
        /// <param name="useBase10">Indicates if base 10 is used instead of unit prefixes.</param>
        public static string Format(double value, DataUnit unit, int numDecimals = defaultNumDecimals, bool useBase10 = false)
        {
            //Handle special values
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(value))
            {
                return "∞";
            }

            if (double.IsPositiveInfinity(value))
            {
                return "-∞";
            }

            //Handle units that don't use prefixes
            switch (unit)
            {
                case DataUnit.Time:
                    if (value >= 1000.0 * 365.0 * 24.0 * 60.0 * 60.0)
                    {
                        return "N/A";
                    }

                    return TimeSpan.FromSeconds(Math.Round(value, numDecimals)).ToString();
                case DataUnit.NoUnit:
                    return Math.Round(value, numDecimals) + "";
                default:
                    break;
            }

            if (nonPrefix.ContainsKey(unit))
            {
                return nonPrefix[unit](value, numDecimals);
            }

            //Handle units with prefixes
            var prefix = new KeyValuePair<string, double>();
            var prefixSpace = true;

            var absValue = Math.Abs(value);
            if (!useBase10)
            {
                if (absValue < 1 && absValue != 0.0)
                {
                    var power = Math.Round(Math.Log10(absValue));
                    prefix = negativePrefixes[0];
                    foreach (var currentPrefix in negativePrefixes)
                    {
                        var bestPower = Math.Log10(prefix.Value);
                        var bestPowerDiff = Math.Abs(bestPower - power);
                        var currentPower = Math.Log10(currentPrefix.Value);
                        var currentPowerDiff = Math.Abs(currentPower - power);

                        if (currentPowerDiff < bestPowerDiff)
                        {
                            prefix = currentPrefix;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < positivePrefixes.Count; i++)
                    {
                        var j = i + 1;
                        if (j < positivePrefixes.Count)
                        {
                            if (absValue >= positivePrefixes[i].Value && absValue < positivePrefixes[j].Value)
                            {
                                prefix = positivePrefixes[i];
                                break;
                            }
                        }
                        else
                        {
                            prefix = positivePrefixes[positivePrefixes.Count - 1];
                        }
                    }
                }
            }
            else
            {
                var power = Math.Floor(Math.Log10(absValue));
                if (power != 0)
                {
                    prefix = new KeyValuePair<string, double>($"E{power} ", Math.Pow(10, power));
                    prefixSpace = false;
                }
                else
                {
                    prefixSpace = true;
                }
            }

            unitNames.TryGetValue(unit, out var unitString);

            var prefixValue = Math.Round((value / (prefix.Value > 0 ? prefix.Value : 1)), numDecimals);
            if (prefixValue == 0)
            {
                prefix = new KeyValuePair<string, double>("", 1);
            }

            return prefixValue + (prefixSpace ? " " : "") + prefix.Key + unitString;
        }

        /// <summary>
        /// Parses the deltaV for the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="value">The value</param>
        public static Vector3d ParseDeltaVelocity(PhysicsObject physicsObject, string value)
        {
            var deltaV = new Vector3d();
            var state = physicsObject.State;
            state.Velocity -= physicsObject.PrimaryBody.Velocity;
            state.Position -= physicsObject.PrimaryBody.Position;
            if (state.Velocity == Vector3d.Zero)
            {
                state.Velocity = Vector3d.ForwardLH;
            }

            foreach (var part in value.Split(' '))
            {
                var sub = part.Substring(0, part.Length - 1);
                var amount = double.Parse(sub, System.Globalization.CultureInfo.InvariantCulture);

                switch (part.Last())
                {
                    case 'P':
                        deltaV += amount * state.Prograde;
                        break;
                    case 'N':
                        deltaV += amount * state.Normal;
                        break;
                    case 'R':
                        deltaV += amount * state.Radial;
                        break;
                }
            }

            return deltaV;
        }

        /// <summary>
        /// Formats the given vector
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="numDecimals">The number of decimals</param>
        public static string Format(Vector3d value, int numDecimals = 2)
        {
            value.X = Math.Round(value.X, numDecimals);
            value.Y = Math.Round(value.Y, numDecimals);
            value.Z = Math.Round(value.Z, numDecimals);
            return $"X: {value.X}, Y: {value.Y}, Z: {value.Z}";
        }
    }
}
