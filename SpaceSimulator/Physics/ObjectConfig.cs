using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// The configuration for a physics object
    /// </summary>
    public sealed class ObjectConfig
    {
        /// <summary>
        /// The mass
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// The rotational period (time to complete one rotation around its axis) of the object
        /// </summary>
        public double RotationalPeriod { get; }

        /// <summary>
        /// The axis-of-rotation
        /// </summary>
        public Vector3d AxisOfRotation { get; }

        private static readonly ObjectConfig empty = new ObjectConfig(0);

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        /// <param name="mass">The mass</param>
        /// <param name="rotationalPeriod">The rotational period </param>
        /// <param name="axisOfRotation">The axis-of-rotation, defaults to Vector3d.Up</param>
        public ObjectConfig(double mass, double rotationalPeriod = 0, Vector3d? axisOfRotation = null)
        {
            this.Mass = mass;
            this.RotationalPeriod = rotationalPeriod;
            this.AxisOfRotation = axisOfRotation ?? Vector3d.Up;
        }

        /// <summary>
        /// Returns the rotational speed in radians/second
        /// </summary>
        public double RotationalSpeed
        {
            get { return (2.0 * Math.PI) / this.RotationalPeriod; }
        }

        /// <summary>
        /// Returns an empty configuration
        /// </summary>
        public static ObjectConfig Empty
        {
            get { return empty; }
        }

        /// <summary>
        /// Changes the mass of the object
        /// </summary>
        /// <param name="newMass">The new mass</param>
        /// <returns>A new configuration with the changed mass</returns>
        public ObjectConfig WithMass(double newMass)
        {
            return new ObjectConfig(newMass, this.RotationalPeriod, this.AxisOfRotation);
        }
    }
}
