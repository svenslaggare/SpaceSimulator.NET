using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;

namespace SpaceSimulator.Camera
{
    /// <summary>
    /// Represents a ground based camera
    /// </summary>
    public class GroundCamera : SpaceCamera
    {
        /// <summary>
        /// The latitude of the camera
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude of the camera
        /// </summary>
        public double Longitude { get; set; }

        public override void UpdateViewMatrix()
        {
            var primaryBody = this.Focus.PrimaryBody;

            if (primaryBody != null)
            {
                double? elevation = primaryBody.Radius;
                this.position = this.ToDrawPosition(OrbitHelpers.FromCoordinates(primaryBody, this.Latitude, this.Longitude, elevation));
                var targetPosition = this.ToDrawPosition(this.FocusPosition);

                //this.up = Vector3.Up;
                this.up = MathHelpers.ToFloat(OrbitHelpers.SphereNormal(primaryBody, this.Latitude, this.Longitude, elevation));
                this.look = targetPosition - this.position;
                this.look.Normalize();

                //this.up = Vector3.Up;
                //this.look = MathHelpers.ToFloat(OrbitHelpers.SphereNormal(primaryBody, this.Latitude, this.Longitude));

                this.right = Vector3.Cross(this.Up, this.Look);
                this.view = Matrix.LookAtLH(this.position, this.position + this.look, this.up);
                this.UpdateViewProjection();
            }
        }
    }
}
