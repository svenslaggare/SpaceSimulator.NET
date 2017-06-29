﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Mathematics
{
    /// <summary>
    /// Contain helper methods for math
    /// </summary>
    public static class MathHelpers
    {
        private static readonly double stumpffFunctionZeroLimit = 1E-3;

        /// <summary>
        /// Indicates if any component of the given vector is NaN
        /// </summary>
        /// <param name="vector">The vector</param>
        public static bool IsNaN(Vector3 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z);
        }

        /// <summary>
        /// Indicates if any component of the given vector is NaN
        /// </summary>
        /// <param name="vector">The vector</param>
        public static bool IsNaN(Vector3d vector)
        {
            return double.IsNaN(vector.X) || double.IsNaN(vector.Y) || double.IsNaN(vector.Z);
        }

        /// <summary>
        /// Indicates if any component of the given vector is infinity
        /// </summary>
        /// <param name="vector">The vector</param>
        public static bool IsInfinity(Vector3 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z);
        }

        /// <summary>
        /// Indicates if any component of the given vector is infinity
        /// </summary>
        /// <param name="vector">The vector</param>
        public static bool IsInfinity(Vector3d vector)
        {
            return double.IsInfinity(vector.X) || double.IsInfinity(vector.Y) || double.IsInfinity(vector.Z);
        }

        /// <summary>
        /// Asserts that the given vector is not NaN
        /// </summary>
        /// <param name="vector">The vector</param>
        public static void AssertNotNaN(Vector3 vector)
        {
            if (IsNaN(vector))
            {
                throw new InvalidOperationException("The vector is not valid (" + vector + "):");
            }
        }

        /// <summary>
        /// Ignores small values of the given vector
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3 IgnoreSmallValues(Vector3 vector, float epsilon = 1E-7f)
        {
            if (Math.Abs(vector.X) < epsilon)
            {
                vector.X = 0;
            }

            if (Math.Abs(vector.Y) < epsilon)
            {
                vector.Y = 0;
            }

            if (Math.Abs(vector.Z) < epsilon)
            {
                vector.Z = 0;
            }

            return vector;
        }

        /// <summary>
        /// Ignores small values of the given vector
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d IgnoreSmallValues(Vector3d vector, double epsilon = 1E-10)
        {
            if (Math.Abs(vector.X) < epsilon)
            {
                vector.X = 0;
            }

            if (Math.Abs(vector.Y) < epsilon)
            {
                vector.Y = 0;
            }

            if (Math.Abs(vector.Z) < epsilon)
            {
                vector.Z = 0;
            }

            return vector;
        }

        /// <summary>
        /// Normalizes the given vector
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d Normalized(Vector3d vector)
        {
            vector.Normalize();
            return vector;
        }

        /// <summary>
        /// Computes the C function
        /// </summary>
        /// <param name="z">The value</param>
        public static double C(double z)
        {
            if (Math.Abs(z) < stumpffFunctionZeroLimit)
            {
                return (1 / 2.0) - (z / 24.0) + ((z * z) / 720.0) - ((z * z * z) / 40320.0);
            }
            else if (z > 0)
            {
                return (1 - Math.Cos(Math.Sqrt(z))) / z;
            }
            else if (z < 0)
            {
                return (1 - Math.Cosh(Math.Sqrt(-z))) / z;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Computes the S function
        /// </summary>
        /// <param name="z">The value</param>
        public static double S(double z)
        {
            if (Math.Abs(z) < stumpffFunctionZeroLimit)
            {
                return (1 / 6.0) - (z / 120.0) + ((z * z) / 5040.0) - ((z * z * z) / 362880.0);
            }
            else if (z > 0)
            {
                var sqrtZ = Math.Sqrt(z);
                return (sqrtZ - Math.Sin(sqrtZ)) / Math.Sqrt(z * z * z);
            }
            else if (z < 0)
            {
                var sqrtZ = Math.Sqrt(-z);
                return (Math.Sinh(sqrtZ) - sqrtZ) / Math.Sqrt(-z * z * z);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the angle between the vectors u and v
        /// </summary>
        /// <param name="u">The first vector</param>
        /// <param name="v">The second vector</param>
        public static double AngleBetween(Vector3d u, Vector3d v)
        {
            if (u == v)
            {
                return 0;
            }

            //return Math.Acos(Vector3d.Dot(Normalized(u), Normalized(v)));
            return Math.Atan2(Vector3d.Cross(u, v).Length(), Vector3d.Dot(u, v));
        }

        /// <summary>
        /// Returns the angle between the vectors u and v, with the sign determined by the normal to the plane of u and v
        /// </summary>
        /// <param name="u">The first vector</param>
        /// <param name="v">The second vector</param>
        /// <param name="n">The normal vector</param>
        public static double AngleBetween(Vector3d u, Vector3d v, Vector3d n)
        {
            if (u == v)
            {
                return 0;
            }

            var angle = Math.Acos(Vector3d.Dot(MathHelpers.Normalized(u), MathHelpers.Normalized(v)));
            var cross = Vector3d.Cross(u, v);
            return angle * Math.Sign(Vector3d.Dot(n, cross));
        }

        /// <summary>
        /// Clamps the given angle to [0, 2pi]
        /// </summary>
        /// <param name="angle">The angle</param>
        public static double ClampAngle(double angle)
        {
            var twoPi = 2 * Math.PI;
            while (angle < 0)
            {
                angle += twoPi;
            }

            while (angle > twoPi)
            {
                angle -= twoPi;
            }

            return angle;
        }

        /// <summary>
        /// Wraps the given value to the range [min, max]
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <param name="value">The value</param>
        public static double Clamp(double min, double max, double value)
        {
            if (value > max)
            {
                return max;
            }
            else if (value < min)
            {
                return min;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Calculates the inverse hyperbolic cosine function
        /// </summary>
        /// <param name="x">The value to calculate for</param>
        public static double Acosh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x - 1));
        }

        /// <summary>
        /// Calculates the minimum angle difference
        /// </summary>
        /// <param name="angle">The current angle</param>
        /// <param name="requiredAngle">The required angle</param>
        public static double CalculateMinAngleDifference(double angle, double requiredAngle)
        {
            //var delta = requiredAngle - angle;

            //if (delta < 0)
            //{
            //    delta += 2 * Mathf.PI;
            //}

            //if (Math.Abs(delta - 2 * Math.PI) < Math.Abs(delta))
            //{
            //    if (Math.Abs(delta + 2 * Math.PI) < Math.Abs(delta - 2 * Math.PI))
            //    {
            //        delta += 2 * Math.PI;
            //    }
            //    else
            //    {
            //        delta -= 2 * Math.PI;
            //    }
            //}
            //else if (Math.Abs(delta + 2 * Math.PI) < Math.Abs(delta))
            //{
            //    delta += 2 * Math.PI;
            //}

            //return delta;

            var delta = requiredAngle - angle;
            if (delta > Math.PI)
            {
                delta -= 2.0 * Math.PI; 
            }

            if (delta < -Math.PI)
            {
                delta += 2.0 * Math.PI;
            }

            return delta;
        }

        /// <summary>
        /// Converts between the given vector types
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3 ToFloat(Vector3d vector)
        {
            return new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        /// <summary>
        /// Converts between the given vector types
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d ToDouble(Vector3 vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Swaps the y and z components
        /// </summary>
        /// <param name="vector">The vector</param>
        public static Vector3d SwapYZ(Vector3d vector)
        {
            return new Vector3d(vector.X, vector.Z, vector.Y);
        }

        /// <summary>
        /// Moves the rectangle by the given amount
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <param name="amount">The amount</param>
        /// <returns>The moved rectangle</returns>
        public static RectangleF Move(this RectangleF rectangle, Vector2 amount)
        {
            var currentCopy = rectangle;
            currentCopy.Offset(amount);
            return currentCopy;
        }
    }
}