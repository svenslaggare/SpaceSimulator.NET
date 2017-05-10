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

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the object
        /// </summary>
        public PhysicsObjectType Type { get; }

        /// <summary>
        /// The configuration for the object
        /// </summary>
        public ObjectConfig Configuration { get; protected set; }

        /// <summary>
        /// The object that the current object orbits around
        /// </summary>
        public PhysicsObject PrimaryBody { get; protected set; }

        /// <summary>
        /// The object that the current object orbits around
        /// </summary>
        IPhysicsObject IPhysicsObject.PrimaryBody => (IPhysicsObject)PrimaryBody;

        /// <summary>
        /// Indicates if the object is drawn at real scale
        /// </summary>
        public bool IsRealSize { get; }

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
        /// <param name="config">The configuration for the object</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        /// <param name="isRealSize">Indicates if the size of the drawn object is real</param>
        public PhysicsObject(
            string name,
            PhysicsObjectType type,
            ObjectConfig config,
            PhysicsObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit,
            bool isRealSize = true)
        {
            this.Name = name;
            this.Type = type;
            this.Configuration = config;
            this.state = initialState;
            this.PrimaryBody = primaryBody;
            this.IsRealSize = isRealSize;
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
            get { return this.Configuration.Mass * Constants.G; }
        }

        /// <summary>
        /// The radius
        /// </summary>
        public double Radius
        {
            get { return this.Configuration.Radius; }
        }

        /// <summary>
        /// The mass
        /// </summary>
        public double Mass
        {
            get { return this.Configuration.Mass; }
        }

        /// <summary>
        /// The rotational period (time to complete one rotation around it's axis) of the object
        /// </summary>
        public double RotationalPeriod
        {
            get { return this.Configuration.RotationalPeriod; }
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
        public bool Impacted
        {
            get { return this.state.Impacted; }
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
        /// Returns the axis-of-rotation
        /// </summary>
        public Vector3d AxisOfRotation
        {
            get
            {
                return this.Configuration.AxisOfRotation;
            }
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
        /// Returns the altitude over the current object for the given object
        /// </summary>
        /// <param name="position">The position of the object</param>
        public double Altitude(Vector3d position)
        {
            return (position - this.Position).Length() - this.Radius;
        }

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
        /// If that this is the case, reset the indication.
        /// </summary>
        public bool HasChangedOrbit()
        {
            var hasChanged = this.orbitChanged;
            this.orbitChanged = false;
            return hasChanged;
        }

        /// <summary>
        /// Applies the given delta V
        /// </summary>
        /// <param name="deltaV">The delta V</param>
        public void ApplyDeltaVelocity(double totalTime, Vector3d deltaV)
        {
            this.state.Velocity += deltaV;
            this.state.Impacted = false;

            this.ReferenceState = this.state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(this);
            this.ReferencePrimaryBodyState = this.PrimaryBody.state;

            this.UsedDeltaV += deltaV.Length();
            this.orbitChanged = true;
        }

        /// <summary>
        /// Changes the primary body
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        public void ChangePrimaryBody(PhysicsObject primaryBody)
        {
            this.PrimaryBody = primaryBody;
            this.ReferenceState = this.state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(primaryBody, this.state);
            this.ReferencePrimaryBodyState = primaryBody.state;
            this.orbitChanged = true;
        }

        /// <summary>
        /// Checks if the current object has impacted the primary body
        /// </summary>
        /// <param name="time">The current time</param>
        public void CheckImpacted(double time)
        {
            if (this.PrimaryBody != null)
            {
                var impacted = CollisionHelpers.SphereIntersection(
                    this.state.Position,
                    this.Radius,
                    this.PrimaryBody.NextState.Position,
                    this.PrimaryBody.Radius);

                //If impacted, move to the edge of the primary body
                if (impacted)
                {
                    var dir = MathHelpers.Normalized(this.state.Position - this.PrimaryBody.nextState.Position);
                    var newDistance = this.PrimaryBody.Radius + this.Radius + 1;
                    this.state.Position = this.PrimaryBody.nextState.Position + dir * newDistance;
                    this.state.Velocity = Vector3d.Zero;
                    this.state.Acceleration = OrbitFormulas.GravityAcceleration(
                        this.PrimaryBody.StandardGravitationalParameter,
                        dir * newDistance);
                    this.state.Impacted = true;

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

            if (!this.Impacted)
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
