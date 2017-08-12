using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Camera
{
    /// <summary>
    /// Represents a camera used for space
    /// </summary>
    public abstract class SpaceCamera : BaseCamera
    {
        protected double scaleFactor = 1.0;

        /// <summary>
        /// The object being focused
        /// </summary>
        public PhysicsObject Focus { get; private set; }

        /// <summary>
        /// Creates a new space camera
        /// </summary>
        public SpaceCamera()
        {

        }

        /// <summary>
        /// The focus object
        /// </summary>
        public Vector3d FocusPosition => this.Focus?.Position ?? Vector3d.Zero;

        /// <summary>
        /// Creates a new camera that is a copy of the given camera
        /// </summary>
        /// <param name="camera">The camera</param>
        public SpaceCamera(SpaceCamera camera)
            : base(camera)
        {
            this.scaleFactor = camera.scaleFactor;
            this.Focus = camera.Focus;
        }

        /// <summary>
        /// Sets the scale factor
        /// </summary>
        /// <param name="primaryBody">The primary body to base the scale on</param>
        public virtual void SetScaleFactor(NaturalSatelliteObject primaryBody)
        {
            this.scaleFactor = 1.0 / (5.0 * primaryBody.Radius);
            //this.scaleFactor = 0.0001;
        }

        /// <summary>
        /// Converts the given scalar in world scale to draw scale
        /// </summary>
        /// <param name="world">The world scalar</param>
        public virtual float ToDraw(double world)
        {
            return (float)(this.scaleFactor * world);
        }

        /// <summary>
        /// Converts the given scalar in draw scale to world scale
        /// </summary>
        /// <param name="draw">The draw scalar</param>
        /// <remarks>Precision is lost when converting between draw/world </remarks>
        public virtual double FromDraw(float draw)
        {
            return (double)(draw / this.scaleFactor);
        }

        /// <summary>
        /// Converts the given position in the world position to a draw position
        /// </summary>
        /// <param name="worldPosition">The position in the world</param>
        /// <param name="relativeToFocus">Indicates if the position is relative to the focus</param>
        public virtual Vector3 ToDrawPosition(Vector3d worldPosition, bool relativeToFocus = true)
        {
            if (relativeToFocus)
            {
                worldPosition -= this.FocusPosition;
            }

            return MathHelpers.ToFloat(this.scaleFactor * worldPosition);
        }

        /// <summary>
        /// Sets the focus to the given object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public virtual void SetFocus(PhysicsObject physicsObject)
        {
            this.Focus = physicsObject;
        }

        /// <summary>
        /// Indicates if the given object can be set as focus
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public virtual bool CanSetFocus(PhysicsObject physicsObject) => true;
    }
}
