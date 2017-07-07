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
        /// The focus position
        /// </summary>
        public Vector3d Focus { get; set; }

        /// <summary>
        /// Creates a new space camera
        /// </summary>
        public SpaceCamera()
        {

        }

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
        /// Converts the given position in the world position to a draw position
        /// </summary>
        /// <param name="worldPosition">The position in the world</param>
        /// <param name="relativeToFocus">Indicates if the position is relative to the focus</param>
        public virtual Vector3 ToDrawPosition(Vector3d worldPosition, bool relativeToFocus = true)
        {
            if (relativeToFocus)
            {
                worldPosition -= this.Focus;
            }

            return MathHelpers.ToFloat(this.scaleFactor * worldPosition);
        }
    }
}
