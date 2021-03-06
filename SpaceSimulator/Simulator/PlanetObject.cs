﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents a planet physics object
    /// </summary>
    public class PlanetObject : NaturalSatelliteObject
    {
        /// <summary>
        /// The atmospheric model
        /// </summary>
        public IAtmosphericModel AtmosphericModel { get; }

        /// <summary>
        /// Creates a new planet object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="radius">The radius of the object</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="atmosphericModel">The atmospheric model</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public PlanetObject(
            string name,
            PhysicsObjectType type,
            double mass,
            double radius,
            double rotationalPeriod,
            Vector3d axisOfRotation,
            IAtmosphericModel atmosphericModel,
            NaturalSatelliteObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
            : base(name, type, mass, radius, rotationalPeriod, axisOfRotation, primaryBody, initialState, initialOrbit)
        {
            this.AtmosphericModel = atmosphericModel;
        }

        /// <summary>
        /// Calculates the drag force on the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="state">The state of the object</param>
        public (Vector3d, Vector3d) DragOnObject(ArtificialPhysicsObject physicsObject, ref ObjectState state)
        {
            var primaryState = this.State;
            return this.AtmosphericModel.CalculateDrag(
                this,
                ref primaryState,
                physicsObject,
                physicsObject.AtmosphericProperties,
                ref state);
        }

        /// <summary>
        /// Indicates if the given object is inside the atmosphere
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public bool InsideAtmosphere(ArtificialPhysicsObject physicsObject)
        {
            var state = physicsObject.State;
            return this.AtmosphericModel.Inside(this, ref state);
        }
    }
}
