using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// The type of the physics object
    /// </summary>
    public enum PhysicsObjectType
    {
        /// <summary>
        /// Represents the object of reference (i.e. the basis of the coordinate system, fixed in space)
        /// </summary>
        ObjectOfReference,
        /// <summary>
        /// Represents a natural satellite (planet or moon)
        /// </summary>
        NaturalSatellite,
        /// <summary>
        /// Represents an artificial satellite
        /// </summary>
        ArtificialSatellite,
    }

    /// <summary>
    /// Represents an object affected by physics
    /// </summary>
    public abstract class PhysicsObject : IPhysicsObject
    {
        protected ObjectState state;
        protected ObjectState nextState;

        protected bool orbitChanged = false;
        protected int orbitVersion = 0;

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The mass of the object
        /// </summary>
        public double Mass { get; protected set; }

        /// <summary>
        /// The rotational period
        /// </summary>
        public double RotationalPeriod { get; }

        /// <summary>
        /// The axis-of-rotation
        /// </summary>
        public Vector3d AxisOfRotation { get; }

        /// <summary>
        /// The type of the object
        /// </summary>
        public PhysicsObjectType Type { get; }

        /// <summary>
        /// The object that the current object orbits around
        /// </summary>
        public NaturalSatelliteObject PrimaryBody { get; protected set; }

        /// <summary>
        /// The object that the current object orbits around
        /// </summary>
        IPrimaryBodyObject IPhysicsObject.PrimaryBody => (IPrimaryBodyObject)PrimaryBody;

        /// <summary>
        /// The reference state
        /// </summary>
        public ObjectState ReferenceState { get; protected set; }

        /// <summary>
        /// The reference orbit
        /// </summary>
        public Orbit ReferenceOrbit { get; protected set; }

        /// <summary>
        /// The reference state of the primary body
        /// </summary>
        public ObjectState ReferencePrimaryBodyState { get; protected set; }

        /// <summary>
        /// The amount of delta V used
        /// </summary>
        public double UsedDeltaV { get; protected set; }

        /// <summary>
        /// The target object
        /// </summary>
        public PhysicsObject Target { get; set; }

        /// <summary>
        /// Creates a new physics object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public PhysicsObject(
            string name,
            PhysicsObjectType type,
            double mass,
            double rotationalPeriod,
            Vector3d axisOfRotation,
            NaturalSatelliteObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
        {
            this.Name = name;
            this.Type = type;
            this.Mass = mass;
            this.RotationalPeriod = rotationalPeriod;
            this.AxisOfRotation = axisOfRotation;
            this.state = initialState;
            this.nextState = state;
            this.PrimaryBody = primaryBody;
            this.ReferenceOrbit = initialOrbit;
            this.ReferenceState = initialState;

            if (this.PrimaryBody != null)
            {
                this.ReferencePrimaryBodyState = this.PrimaryBody.State;
            }
        }

        /// <summary>
        /// Returns the state
        /// </summary>
        public ObjectState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// Returns the standard gravitational parameter
        /// </summary>
        public double StandardGravitationalParameter
        {
            get { return this.Mass * Constants.G; }
        }

        /// <summary>
        /// The position
        /// </summary>
        public Vector3d Position
        {
            get { return this.state.Position; }
        }

        /// <summary>
        /// Returns the latitude of the object
        /// </summary>
        public double Latitude
        {
            get
            {
                if (this.PrimaryBody == null)
                {
                    return 0;
                }

                OrbitHelpers.GetCoordinates(this.PrimaryBody, this.Position, out var latitude, out var longitude);
                return latitude;
            }
        }

        /// <summary>
        /// Returns the longitude of the object
        /// </summary>
        public double Longitude
        {
            get
            {
                if (this.PrimaryBody == null)
                {
                    return 0;
                }

                OrbitHelpers.GetCoordinates(this.PrimaryBody, this.Position, out var latitude, out var longitude);
                return longitude;
            }
        }

        /// <summary>
        /// The velocity
        /// </summary>
        public Vector3d Velocity
        {
            get { return this.state.Velocity; }
        }

        /// <summary>
        /// The acceleration
        /// </summary>
        public Vector3d Acceleration
        {
            get { return this.state.Acceleration; }
        }

        /// <summary>
        /// The rotation of the object
        /// </summary>
        public double Rotation
        {
            get { return this.state.Rotation; }
        }

        /// <summary>
        /// Indicates if the object has impacted any object
        /// </summary>
        public bool HasImpacted
        {
            get { return this.state.HasImpacted; }
        }

        /// <summary>
        /// Returns the next state
        /// </summary>
        public ObjectState NextState
        {
            get { return this.nextState; }
        }

        /// <summary>
        /// Indicates if the object is the object of reference
        /// </summary>
        public bool IsObjectOfReference
        {
            get { return this.Type == PhysicsObjectType.ObjectOfReference; }
        }

        /// <summary>
        /// Returns the rotational transform for the object
        /// </summary>
        public Matrix3x3d RotationalTransform
        {
            get { return Matrix3x3d.RotationAxis(this.AxisOfRotation, this.Rotation); }
        }

        /// <summary>
        /// Returns the inverse rotational transform for the object
        /// </summary>
        public Matrix3x3d InverseRotationalTransform
        {
            get { return Matrix3x3d.RotationAxis(this.AxisOfRotation, -this.Rotation); }
        }

        /// <summary>
        /// Returns the sphere-of-influence of the current object
        /// </summary>
        public double? SphereOfInfluence
        {
            get
            {
                if (this.PrimaryBody == null)
                {
                    return null;
                }

                return OrbitFormulas.SphereOfInfluence(
                    this.ReferenceOrbit.SemiMajorAxis,
                    this.Mass,
                    this.PrimaryBody.Mass);
            }
        }

        /// <summary>
        /// Returns the flight path angle
        /// </summary>
        public double FlightPathAngle
        {
            get
            {
                if (this.ReferenceOrbit.IsCircular)
                {
                    return 0.0;
                }

                var r = MathHelpers.SwapYZ(state.Position - this.PrimaryBody.Position);
                var v = MathHelpers.SwapYZ(state.Velocity - this.PrimaryBody.Velocity);
                var h = Vector3d.Cross(r, v);

                return Math.Acos(h.Length() / (r.Length() * v.Length()));
            }
        }

        /// <summary>
        /// Returns the current orbit version
        /// </summary>
        public int OrbitVersion => this.orbitVersion;

        /// <summary>
        /// Sets the next state
        /// </summary>
        /// <param name="nextState">The next state</param>
        public void SetNextState(ObjectState nextState)
        {
            this.nextState = nextState;
        }

        /// <summary>
        /// Indicates if the orbit of the object has changed.
        /// If that this is the case, resets the indication.
        /// </summary>
        public bool HasChangedOrbit()
        {
            var hasChanged = this.orbitChanged;
            this.orbitChanged = false;
            return hasChanged;
        }

        /// <summary>
        /// Marks that the orbit has changed
        /// </summary>
        protected void OrbitChanged()
        {
            this.orbitChanged = true;
            this.orbitVersion++;
        }

        /// <summary>
        /// Applies the burn
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="deltaV">The delta V</param>
        public virtual void ApplyBurn(double totalTime, Vector3d deltaV)
        {
            this.state.Velocity += deltaV;
            this.state.HasImpacted = false;

            this.ReferenceState = this.state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(this);
            this.ReferencePrimaryBodyState = this.PrimaryBody.state;

            this.UsedDeltaV += deltaV.Length();
            this.OrbitChanged();
        }

        /// <summary>
        /// Changes the primary body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        public void ChangePrimaryBody(NaturalSatelliteObject primaryBody)
        {
            this.PrimaryBody = primaryBody;
            this.ReferenceState = this.state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(primaryBody, this.state);
            this.ReferencePrimaryBodyState = primaryBody.state;
            this.OrbitChanged();
        }

        /// <summary>
        /// Checks if the current object has impacted the primary body
        /// </summary>
        /// <param name="time">The current time</param>
        public void CheckImpacted(double time)
        {
            if (this.PrimaryBody != null)
            {
                var radius = 10.0;
                if (this is NaturalSatelliteObject naturalObject)
                {
                    radius = naturalObject.Radius;
                }

                var impacted = CollisionHelpers.SphereIntersection(
                    this.state.Position,
                    radius,
                    this.PrimaryBody.nextState.Position,
                    this.PrimaryBody.Radius);

                //If impacted, move to the edge of the primary body
                if (impacted)
                {
                    var dir = MathHelpers.Normalized(this.state.Position - this.PrimaryBody.nextState.Position);
                    var newDistance = this.PrimaryBody.Radius + radius + 1;
                    this.state.Position = this.PrimaryBody.nextState.Position + dir * newDistance;
                    this.state.Velocity = Vector3d.Zero;
                    this.state.Acceleration = OrbitFormulas.GravityAcceleration(
                        this.PrimaryBody.StandardGravitationalParameter,
                        dir * newDistance);
                    this.state.HasImpacted = true;

                    var referenceState = this.state;
                    referenceState.Time = time;
                    this.ReferenceState = referenceState;
                    this.ReferencePrimaryBodyState = this.PrimaryBody.nextState;
                }
            }
        }

        /// <summary>
        /// Updates the object
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        public virtual void Update(double totalTime, double timeStep)
        {
            this.state = this.nextState;

            if (!this.HasImpacted)
            {
                this.CheckImpacted(totalTime);
            }
        }

        /// <summary>
        /// Returns a string representation of the current object
        /// </summary>
        public override string ToString()
        {
            if (this.PrimaryBody == null)
            {
                return this.Name;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(this.Name);
            stringBuilder.Append(" - alt: " + DataFormatter.Format(this.PrimaryBody.Altitude(this.Position), DataUnit.Distance));
            stringBuilder.Append(", r: " + DataFormatter.Format((this.Position - this.PrimaryBody.Position).Length(), DataUnit.Distance));
            stringBuilder.Append(", v: " + DataFormatter.Format(this.Velocity.Length(), DataUnit.Velocity));
            stringBuilder.Append(", a: " + DataFormatter.Format(this.Acceleration.Length(), DataUnit.Acceleration));

            return stringBuilder.ToString();
        }
    }
}
