using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents a natural satellite object
    /// </summary>
    public abstract class NaturalSatelliteObject : PhysicsObject, IPrimaryBodyObject
    {
        /// <summary>
        /// The radius
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Creates a new natrual satellite object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="radius">The radius of the object</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public NaturalSatelliteObject(
            string name,
            PhysicsObjectType type,
            double mass,
            double radius,
            double rotationalPeriod,
            Vector3d axisOfRotation,
            NaturalSatelliteObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
            : base(name, type, mass, rotationalPeriod, axisOfRotation, primaryBody, initialState, initialOrbit)
        {
            this.Radius = radius;
        }

        /// <summary>
        /// Returns the altitude over the current object for the given object
        /// </summary>
        /// <param name="position">The position of the object</param>
        public double Altitude(Vector3d position)
        {
            return (position - this.Position).Length() - this.Radius;
        }
    }
}
