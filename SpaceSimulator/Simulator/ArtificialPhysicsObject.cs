using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represetns a generic artificial physics object
    /// </summary>
    public abstract class ArtificialPhysicsObject : PhysicsObject
    {
        /// <summary>
        /// Returns the atmospheric properties
        /// </summary>
        public abstract AtmosphericProperties AtmosphericProperties { get; }

        /// <summary>
        /// Creates a new artificial object object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="initialState">The initial state</param>
        /// <param name="initialOrbit">The initial orbit</param>
        public ArtificialPhysicsObject(
            string name,
            double mass,
            NaturalSatelliteObject primaryBody,
            ObjectState initialState,
            Orbit initialOrbit)
            : base(
                  name, 
                  PhysicsObjectType.ArtificialSatellite, 
                  mass,
                  PhysicsFormulas.MomentOfInertiaForSphere(mass, 20.0),
                  0,
                  Vector3d.Zero, 
                  primaryBody, 
                  initialState,
                  initialOrbit)
        {

        }

        /// <summary>
        /// Indicates if the current object intersects the given primary body at the given position
        /// </summary>
        /// <param name="primaryBody">The primary body</param>
        /// <param name="primaryBodyPosition">The position of the primary</param>
        /// <param name="position">The current position of the object</param>
        public override bool Intersects(IPrimaryBodyObject primaryBody, Vector3d primaryBodyPosition, Vector3d position)
        {
            return CollisionHelpers.SphereIntersection(primaryBodyPosition, primaryBody.Radius, position, 10);
        }

        /// <summary>
        /// Updates the reference orbit
        /// </summary>
        public void UpdateReferenceOrbit()
        {
            this.ReferenceState = this.state;
            this.ReferenceOrbit = Orbit.CalculateOrbit(this);
            this.ReferencePrimaryBodyState = this.PrimaryBody.State;
            this.OrbitChanged();
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
            this.UpdateReferenceOrbit();
            this.UsedDeltaV += deltaV.Length();
        }
    }
}
