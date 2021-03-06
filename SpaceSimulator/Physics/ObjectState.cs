﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Physics
{
    /// <summary>
    /// The state of an object
    /// </summary>
    public struct ObjectState
    {
        /// <summary>
        /// The time
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The position
        /// </summary>
        public Vector3d Position { get; set; }

        /// <summary>
        /// The velocity
        /// </summary>
        public Vector3d Velocity { get; set; }

        /// <summary>
        /// The orientation
        /// </summary>
        public Quaterniond Orientation { get; set; }

        /// <summary>
        /// The angular momentum
        /// </summary>
        public Vector3d AngularMomentum { get; set; }

        /// <summary>
        /// Indicates if the object has impacted the primary body
        /// </summary>
        public bool HasImpacted { get; set; }

        /// <summary>
        /// Creates a new state
        /// </summary>
        /// <param name="time">The time</param>
        /// <param name="position">The position</param>
        /// <param name="velocity">The velocity</param>
        /// <param name="orientation">The orientation</param>
        /// <param name="angularMomentum">The angular momentum</param>
        /// <param name="impacted">Indicates if the object has impacted the primary body</param>
        public ObjectState(
            double time, 
            Vector3d position, 
            Vector3d velocity, 
            Quaterniond? orientation = null,
            Vector3d? angularMomentum = null,
            bool impacted = false)
        {
            this.Time = time;
            this.Position = position;
            this.Velocity = velocity;
            this.Orientation = orientation ?? Quaterniond.Identity;
            this.AngularMomentum = angularMomentum ?? Vector3d.Zero;
            this.HasImpacted = impacted;
        }

        /// <summary>
        /// Returns the angular velocity
        /// </summary>
        /// <param name="physicsObject">The object for the state</param>
        public Vector3d AngularVelocity(IPhysicsObject physicsObject) => this.AngularMomentum / physicsObject.MomentOfInertia;

        /// <summary>
        /// Returns the prograde vector
        /// </summary>
        public Vector3d Prograde => OrbitHelpers.Prograde(this.Velocity);

        /// <summary>
        /// Returns the retrograde vector
        /// </summary>
        public Vector3d Retrograde => -this.Prograde;

        /// <summary>
        /// Returns the normal vector
        /// </summary>
        public Vector3d Normal => OrbitHelpers.Normal(this.Prograde, this.Radial);

        /// <summary>
        /// Returns the radial vector
        /// </summary>
        public Vector3d Radial => OrbitHelpers.Radial(this.Position);

        /// <summary>
        /// Returns a new state with the given orientation
        /// </summary>
        /// <param name="orientation">The orientation</param>
        public ObjectState WithOrientation(Quaterniond orientation)
        {
            var copy = this;
            copy.Orientation = orientation;
            return copy;
        }

        /// <summary>
        /// Makes the current state relative to the primary body
        /// </summary>
        /// <param name="primaryBodyState">The current state of the primary body</param>
        public void MakeRelative(ObjectState primaryBodyState)
        {
            this.Position -= primaryBodyState.Position;
            this.Velocity -= primaryBodyState.Velocity;
        }

        /// <summary>
        /// Makes the current state absolute to the primary body
        /// </summary>
        /// <param name="primaryBodyState">The current state of the primary body</param>
        public void MakeAbsolute(ObjectState primaryBodyState)
        {
            this.Position += primaryBodyState.Position;
            this.Velocity += primaryBodyState.Velocity;
        }

        /// <summary>
        /// Swaps the reference frame
        /// </summary>
        /// <param name="currentPrimaryBodyState">The state of the current reference frame</param>
        /// <param name="newPrimaryBodyState">The state of the new reference frame</param>
        public void SwapReferenceFrame(ObjectState currentPrimaryBodyState, ObjectState newPrimaryBodyState)
        {
            this.MakeRelative(currentPrimaryBodyState);
            this.MakeAbsolute(newPrimaryBodyState);
        }

        /// <summary>
        /// Calculates the distance to the given state
        /// </summary>
        /// <param name="other">The other state</param>
        public double Distance(ObjectState other)
        {
            return Vector3d.Distance(this.Position, other.Position);
        }
    }
}
