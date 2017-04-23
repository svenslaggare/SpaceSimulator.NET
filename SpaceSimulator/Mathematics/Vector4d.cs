﻿// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpaceSimulator.Mathematics
{
    /// <summary>
    /// Represents a four dimensional mathematical vector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4d : IEquatable<Vector4d>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref="Vector4d"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4d));

        /// <summary>
        /// A <see cref="Vector4d"/> with all of its components set to zero.
        /// </summary>
        public static readonly Vector4d Zero = new Vector4d();

        /// <summary>
        /// The X unit <see cref="Vector4d"/> (1, 0, 0, 0).
        /// </summary>
        public static readonly Vector4d UnitX = new Vector4d(1.0, 0.0, 0.0, 0.0);

        /// <summary>
        /// The Y unit <see cref="Vector4d"/> (0, 1, 0, 0).
        /// </summary>
        public static readonly Vector4d UnitY = new Vector4d(0.0, 1.0, 0.0, 0.0);

        /// <summary>
        /// The Z unit <see cref="Vector4d"/> (0, 0, 1, 0).
        /// </summary>
        public static readonly Vector4d UnitZ = new Vector4d(0.0, 0.0, 1.0, 0.0);

        /// <summary>
        /// The W unit <see cref="Vector4d"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly Vector4d UnitW = new Vector4d(0.0, 0.0, 0.0, 1.0);

        /// <summary>
        /// A <see cref="Vector4d"/> with all of its components set to one.
        /// </summary>
        public static readonly Vector4d One = new Vector4d(1.0, 1.0, 1.0, 1.0);

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public double X;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public double Y;

        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        public double Z;

        /// <summary>
        /// The W component of the vector.
        /// </summary>
        public double W;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4d"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Vector4d(double value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4d"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the vector.</param>
        /// <param name="y">Initial value for the Y component of the vector.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        public Vector4d(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4d"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        public Vector4d(Vector3d value, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4d"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        //public Vector4d(Vector2 value, double z, double w)
        //{
        //    X = value.X;
        //    Y = value.Y;
        //    Z = z;
        //    W = w;
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4d"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Vector4d(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Vector4.");

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get { return MathUtild.IsOne((X * X) + (Y * Y) + (Z * Z) + (W * W)); }
        }

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get { return X == 0 && Y == 0 && Z == 0 && W == 0; }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Vector4 run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for Vector4 run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="Vector4d.LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double Length()
        {
            return (double)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Vector4d.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize()
        {
            double length = Length();
            if (!MathUtild.IsZero(length))
            {
                double inverse = 1.0 / length;
                X *= inverse;
                Y *= inverse;
                Z *= inverse;
                W *= inverse;
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z, W };
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector4d left, ref Vector4d right, out Vector4d result)
        {
            result = new Vector4d(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4d Add(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <param name="result">The vector with added scalar for each element.</param>
        public static void Add(ref Vector4d left, ref double right, out Vector4d result)
        {
            result = new Vector4d(left.X + right, left.Y + right, left.Z + right, left.W + right);
        }

        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4d Add(Vector4d left, double right)
        {
            return new Vector4d(left.X + right, left.Y + right, left.Z + right, left.W + right);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <param name="result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract(ref Vector4d left, ref Vector4d right, out Vector4d result)
        {
            result = new Vector4d(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4d Subtract(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <param name="result">The vector with subtracted scalar for each element.</param>
        public static void Subtract(ref Vector4d left, ref double right, out Vector4d result)
        {
            result = new Vector4d(left.X - right, left.Y - right, left.Z - right, left.W - right);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector4d Subtract(Vector4d left, double right)
        {
            return new Vector4d(left.X - right, left.Y - right, left.Z - right, left.W - right);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector.</param>
        /// <param name="result">The vector with subtracted scalar for each element.</param>
        public static void Subtract(ref double left, ref Vector4d right, out Vector4d result)
        {
            result = new Vector4d(left - right.X, left - right.Y, left - right.Z, left - right.W);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector.</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector4d Subtract(double left, Vector4d right)
        {
            return new Vector4d(left - right.X, left - right.Y, left - right.Z, left - right.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <param name="result">When the method completes, contains the scaled vector.</param>
        public static void Multiply(ref Vector4d value, double scale, out Vector4d result)
        {
            result = new Vector4d(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d Multiply(Vector4d value, double scale)
        {
            return new Vector4d(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
        }

        /// <summary>
        /// Multiplies a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name="left">The first vector to multiply.</param>
        /// <param name="right">The second vector to multiply.</param>
        /// <param name="result">When the method completes, contains the multiplied vector.</param>
        public static void Multiply(ref Vector4d left, ref Vector4d right, out Vector4d result)
        {
            result = new Vector4d(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Multiplies a vector with another by performing component-wise multiplication.
        /// </summary>
        /// <param name="left">The first vector to multiply.</param>
        /// <param name="right">The second vector to multiply.</param>
        /// <returns>The multiplied vector.</returns>
        public static Vector4d Multiply(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <param name="result">When the method completes, contains the scaled vector.</param>
        public static void Divide(ref Vector4d value, double scale, out Vector4d result)
        {
            result = new Vector4d(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d Divide(Vector4d value, double scale)
        {
            return new Vector4d(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <param name="value">The vector to scale.</param>
        /// <param name="result">When the method completes, contains the scaled vector.</param>
        public static void Divide(double scale, ref Vector4d value, out Vector4d result)
        {
            result = new Vector4d(scale / value.X, scale / value.Y, scale / value.Z, scale / value.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d Divide(double scale, Vector4d value)
        {
            return new Vector4d(scale / value.X, scale / value.Y, scale / value.Z, scale / value.W);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <param name="result">When the method completes, contains a vector facing in the opposite direction.</param>
        public static void Negate(ref Vector4d value, out Vector4d result)
        {
            result = new Vector4d(-value.X, -value.Y, -value.Z, -value.W);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector4d Negate(Vector4d value)
        {
            return new Vector4d(-value.X, -value.Y, -value.Z, -value.W);
        }

        /// <summary>
        /// Returns a <see cref="Vector4d"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        /// <param name="result">When the method completes, contains the 4D Cartesian coordinates of the specified point.</param>
        public static void Barycentric(ref Vector4d value1, ref Vector4d value2, ref Vector4d value3, double amount1, double amount2, out Vector4d result)
        {
            result = new Vector4d((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X)),
                (value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)),
                (value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z)),
                (value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)));
        }

        /// <summary>
        /// Returns a <see cref="Vector4d"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 4D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector4d"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        /// <returns>A new <see cref="Vector4d"/> containing the 4D Cartesian coordinates of the specified point.</returns>
        public static Vector4d Barycentric(Vector4d value1, Vector4d value2, Vector4d value3, double amount1, double amount2)
        {
            Vector4d result;
            Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out result);
            return result;
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Vector4d value, ref Vector4d min, ref Vector4d max, out Vector4d result)
        {
            double x = value.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            double y = value.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            double z = value.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            double w = value.W;
            w = (w > max.W) ? max.W : w;
            w = (w < min.W) ? min.W : w;

            result = new Vector4d(x, y, z, w);
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector4d Clamp(Vector4d value, Vector4d min, Vector4d max)
        {
            Vector4d result;
            Clamp(ref value, ref min, ref max, out result);
            return result;
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">When the method completes, contains the distance between the two vectors.</param>
        /// <remarks>
        /// <see cref="Vector4d.DistanceSquared(ref Vector4d, ref Vector4d, out double)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static void Distance(ref Vector4d value1, ref Vector4d value2, out double result)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            result = (double)Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector4d.DistanceSquared(Vector4d, Vector4d)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static double Distance(Vector4d value1, Vector4d value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            return (double)Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        public static void DistanceSquared(ref Vector4d value1, ref Vector4d value2, out double result)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
        public static double DistanceSquared(Vector4d value1, Vector4d value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="left">First source vector</param>
        /// <param name="right">Second source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two vectors.</param>
        public static void Dot(ref Vector4d left, ref Vector4d right, out double result)
        {
            result = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        /// <returns>The dot product of the two vectors.</returns>
        public static double Dot(Vector4d left, Vector4d right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <param name="result">When the method completes, contains the normalized vector.</param>
        public static void Normalize(ref Vector4d value, out Vector4d result)
        {
            Vector4d temp = value;
            result = temp;
            result.Normalize();
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector4d Normalize(Vector4d value)
        {
            value.Normalize();
            return value;
        }

        /// <summary>
        /// Performs a linear interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Vector4d start, ref Vector4d end, double amount, out Vector4d result)
        {
            result.X = MathUtild.Lerp(start.X, end.X, amount);
            result.Y = MathUtild.Lerp(start.Y, end.Y, amount);
            result.Z = MathUtild.Lerp(start.Z, end.Z, amount);
            result.W = MathUtild.Lerp(start.W, end.W, amount);
        }

        /// <summary>
        /// Performs a linear interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two vectors.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector4d Lerp(Vector4d start, Vector4d end, double amount)
        {
            Vector4d result;
            Lerp(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two vectors.</param>
        public static void SmoothStep(ref Vector4d start, ref Vector4d end, double amount, out Vector4d result)
        {
            amount = MathUtild.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two vectors.</returns>
        public static Vector4d SmoothStep(Vector4d start, Vector4d end, double amount)
        {
            Vector4d result;
            SmoothStep(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position vector.</param>
        /// <param name="tangent1">First source tangent vector.</param>
        /// <param name="value2">Second source position vector.</param>
        /// <param name="tangent2">Second source tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">When the method completes, contains the result of the Hermite spline interpolation.</param>
        public static void Hermite(ref Vector4d value1, ref Vector4d tangent1, ref Vector4d value2, ref Vector4d tangent2, double amount, out Vector4d result)
        {
            double squared = amount * amount;
            double cubed = amount * squared;
            double part1 = ((2.0 * cubed) - (3.0 * squared)) + 1.0;
            double part2 = (-2.0 * cubed) + (3.0 * squared);
            double part3 = (cubed - (2.0 * squared)) + amount;
            double part4 = cubed - squared;

            result = new Vector4d((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
                (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
                (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
                (((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4));
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position vector.</param>
        /// <param name="tangent1">First source tangent vector.</param>
        /// <param name="value2">Second source position vector.</param>
        /// <param name="tangent2">Second source tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static Vector4d Hermite(Vector4d value1, Vector4d tangent1, Vector4d value2, Vector4d tangent2, double amount)
        {
            Vector4d result;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">When the method completes, contains the result of the Catmull-Rom interpolation.</param>
        public static void CatmullRom(ref Vector4d value1, ref Vector4d value2, ref Vector4d value3, ref Vector4d value4, double amount, out Vector4d result)
        {
            double squared = amount * amount;
            double cubed = amount * squared;

            result.X = 0.5f * ((((2.0 * value2.X) + ((-value1.X + value3.X) * amount)) + (((((2.0 * value1.X) - (5.0 * value2.X)) + (4.0 * value3.X)) - value4.X) * squared)) + ((((-value1.X + (3.0 * value2.X)) - (3.0 * value3.X)) + value4.X) * cubed));
            result.Y = 0.5f * ((((2.0 * value2.Y) + ((-value1.Y + value3.Y) * amount)) + (((((2.0 * value1.Y) - (5.0 * value2.Y)) + (4.0 * value3.Y)) - value4.Y) * squared)) + ((((-value1.Y + (3.0 * value2.Y)) - (3.0 * value3.Y)) + value4.Y) * cubed));
            result.Z = 0.5f * ((((2.0 * value2.Z) + ((-value1.Z + value3.Z) * amount)) + (((((2.0 * value1.Z) - (5.0 * value2.Z)) + (4.0 * value3.Z)) - value4.Z) * squared)) + ((((-value1.Z + (3.0 * value2.Z)) - (3.0 * value3.Z)) + value4.Z) * cubed));
            result.W = 0.5f * ((((2.0 * value2.W) + ((-value1.W + value3.W) * amount)) + (((((2.0 * value1.W) - (5.0 * value2.W)) + (4.0 * value3.W)) - value4.W) * squared)) + ((((-value1.W + (3.0 * value2.W)) - (3.0 * value3.W)) + value4.W) * cubed));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A vector that is the result of the Catmull-Rom interpolation.</returns>
        public static Vector4d CatmullRom(Vector4d value1, Vector4d value2, Vector4d value3, Vector4d value4, double amount)
        {
            Vector4d result;
            CatmullRom(ref value1, ref value2, ref value3, ref value4, amount, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector4d left, ref Vector4d right, out Vector4d result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
            result.W = (left.W > right.W) ? left.W : right.W;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector4d Max(Vector4d left, Vector4d right)
        {
            Vector4d result;
            Max(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
        public static void Min(ref Vector4d left, ref Vector4d right, out Vector4d result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
            result.W = (left.W < right.W) ? left.W : right.W;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector4d Min(Vector4d left, Vector4d right)
        {
            Vector4d result;
            Min(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Orthogonalizes a list of vectors.
        /// </summary>
        /// <param name="destination">The list of orthogonalized vectors.</param>
        /// <param name="source">The list of vectors to orthogonalize.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
        /// means that any given vector in the list will be orthogonal to any other given vector in the
        /// list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthogonalize(Vector4d[] destination, params Vector4d[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector4d newvector = source[i];

                for (int r = 0; r < i; ++r)
                {
                    newvector -= (Vector4d.Dot(destination[r], newvector) / Vector4d.Dot(destination[r], destination[r])) * destination[r];
                }

                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Orthonormalizes a list of vectors.
        /// </summary>
        /// <param name="destination">The list of orthonormalized vectors.</param>
        /// <param name="source">The list of vectors to orthonormalize.</param>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all vectors orthogonal to each
        /// other and making all vectors of unit length. This means that any given vector will
        /// be orthogonal to any other given vector in the list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthonormalize(Vector4d[] destination, params Vector4d[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthogonalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
            //q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector4d newvector = source[i];

                for (int r = 0; r < i; ++r)
                {
                    newvector -= Vector4d.Dot(destination[r], newvector) * destination[r];
                }

                newvector.Normalize();
                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Quaterniond"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="Quaterniond"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4d"/>.</param>
        public static void Transform(ref Vector4d vector, ref Quaterniond rotation, out Vector4d result)
        {
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            result = new Vector4d(
                ((vector.X * ((1.0 - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy)),
                ((vector.X * (xy + wz)) + (vector.Y * ((1.0 - xx) - zz))) + (vector.Z * (yz - wx)),
                ((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0 - xx) - yy)),
                vector.W);
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Quaterniond"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="Quaterniond"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4d"/>.</returns>
        public static Vector4d Transform(Vector4d vector, Quaterniond rotation)
        {
            Vector4d result;
            Transform(ref vector, ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Transforms an array of vectors by the given <see cref="Quaterniond"/> rotation.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="rotation">The <see cref="Quaterniond"/> rotation to apply.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector4d[] source, ref Quaterniond rotation, Vector4d[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            double num1 = ((1.0 - yy) - zz);
            double num2 = (xy - wz);
            double num3 = (xz + wy);
            double num4 = (xy + wz);
            double num5 = ((1.0 - xx) - zz);
            double num6 = (yz - wx);
            double num7 = (xz - wy);
            double num8 = (yz + wx);
            double num9 = ((1.0 - xx) - yy);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector4d(
                    ((source[i].X * num1) + (source[i].Y * num2)) + (source[i].Z * num3),
                    ((source[i].X * num4) + (source[i].Y * num5)) + (source[i].Z * num6),
                    ((source[i].X * num7) + (source[i].Y * num8)) + (source[i].Z * num9),
                    source[i].W);
            }
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrixd"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrixd"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4d"/>.</param>
        public static void Transform(ref Vector4d vector, ref Matrixd transform, out Vector4d result)
        {
            result = new Vector4d(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41),
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42),
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43),
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44));
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrixd"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrixd"/>.</param>
        /// <returns>The transformed <see cref="Vector4d"/>.</returns>
        public static Vector4d Transform(Vector4d vector, Matrixd transform)
        {
            Vector4d result;
            Transform(ref vector, ref transform, out result);
            return result;
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix5x4"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix5x4"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4d"/>.</param>
        //public static void Transform(ref Vector4d vector, ref Matrix5x4 transform, out Vector4d result)
        //{
        //    result = new Vector4(
        //        (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41) + transform.M51,
        //        (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42) + transform.M52,
        //        (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43) + transform.M53,
        //        (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44) + transform.M54);
        //}

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix5x4"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix5x4"/>.</param>
        /// <returns>The transformed <see cref="Vector4d"/>.</returns>
        //public static Vector4d Transform(Vector4d vector, Matrix5x4 transform)
        //{
        //    Vector4d result;
        //    Transform(ref vector, ref transform, out result);
        //    return result;
        //}

        /// <summary>
        /// Transforms an array of 4D vectors by the given <see cref="Matrixd"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrixd"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector4d[] source, ref Matrixd transform, Vector4d[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Transform(ref source[i], ref transform, out destination[i]);
            }
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4d operator +(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        /// <summary>
        /// Multiplies a vector with another by performing component-wise multiplication equivalent to <see cref="Multiply(ref Vector4d,ref Vector4d,out Vector4d)"/>.
        /// </summary>
        /// <param name="left">The first vector to multiply.</param>
        /// <param name="right">The second vector to multiply.</param>
        /// <returns>The multiplication of the two vectors.</returns>
        public static Vector4d operator *(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name="value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector4d operator +(Vector4d value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4d operator -(Vector4d left, Vector4d right)
        {
            return new Vector4d(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector4d operator -(Vector4d value)
        {
            return new Vector4d(-value.X, -value.Y, -value.Z, -value.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d operator *(double scale, Vector4d value)
        {
            return new Vector4d(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d operator *(Vector4d value, double scale)
        {
            return new Vector4d(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d operator /(Vector4d value, double scale)
        {
            return new Vector4d(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <param name="value">The vector to scale.</param>        
        /// <returns>The scaled vector.</returns>
        public static Vector4d operator /(double scale, Vector4d value)
        {
            return new Vector4d(scale / value.X, scale / value.Y, scale / value.Z, scale / value.W);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector4d operator /(Vector4d value, Vector4d scale)
        {
            return new Vector4d(value.X / scale.X, value.Y / scale.Y, value.Z / scale.Z, value.W / scale.W);
        }

        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be added on elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4d operator +(Vector4d value, double scalar)
        {
            return new Vector4d(value.X + scalar, value.Y + scalar, value.Z + scalar, value.W + scalar);
        }

        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be added on elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4d operator +(double scalar, Vector4d value)
        {
            return new Vector4d(scalar + value.X, scalar + value.Y, scalar + value.Z, scalar + value.W);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtraced scalar from each element.</returns>
        public static Vector4d operator -(Vector4d value, double scalar)
        {
            return new Vector4d(value.X - scalar, value.Y - scalar, value.Z - scalar, value.W - scalar);
        }

        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtraced scalar from each element.</returns>
        public static Vector4d operator -(double scalar, Vector4d value)
        {
            return new Vector4d(scalar - value.X, scalar - value.Y, scalar - value.Z, scalar - value.W);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Vector4d left, Vector4d right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Vector4d left, Vector4d right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector4d"/> to <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        //public static explicit operator Vector2(Vector4d value)
        //{
        //    return new Vector2(value.X, value.Y);
        //}

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector4d"/> to <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Vector3d(Vector4d value)
        {
            return new Vector3d(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4d"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4d"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4d"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Vector4d other)
        {
            return (MathUtild.NearEqual(other.X, X) &&
                MathUtild.NearEqual(other.Y, Y) &&
                MathUtild.NearEqual(other.Z, Z) &&
                MathUtild.NearEqual(other.W, W));
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4d"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4d"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4d"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Vector4d other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Vector4d))
                return false;

            var strongValue = (Vector4d)value;
            return Equals(ref strongValue);
        }
    }
}
