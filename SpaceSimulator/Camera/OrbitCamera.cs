using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Camera
{
	/// <summary>
	/// Represents an orbit camera
	/// </summary>
	public class OrbitCamera : SpaceCamera
    {
		private float theta;
		private float phi;
		private float radius;
		private float minRadius = 50;
		private float maxRadius = 1000;

        private Vector2 lastMousePosition;

        /// <summary>
        /// The zoom scale factor
        /// </summary>
        private float ZoomScaleFactor => (float)this.scaleFactor;

        /// <summary>
        /// Creates a new orbit camera
        /// </summary>
        /// <param name="minRadius">The minimum radius</param>
        /// <param name="maxRadius">The maximum radius</param>
        public OrbitCamera(float minRadius, float maxRadius)
		{
            this.theta = 1.5f * MathUtil.Pi;
            this.phi = 0.1f * MathUtil.Pi;
            this.minRadius = minRadius;
            this.maxRadius = maxRadius;
            this.radius = minRadius;
        }

        /// <summary>
        /// Creates a new camera that is a copy of the given camera
        /// </summary>
        /// <param name="camera">The camera</param>
        public OrbitCamera(OrbitCamera camera)
            : base(camera)
        {
            this.theta = camera.theta;
            this.phi = camera.phi;
            this.radius = camera.radius;
            this.minRadius = camera.minRadius;
            this.maxRadius = camera.maxRadius;

            this.lastMousePosition = camera.lastMousePosition;
        }

		/// <summary>
		/// The theta angle
		/// </summary>
		public float Theta
		{
			get { return this.theta; }
			set { this.theta = value; }
		}

		/// <summary>
		/// The phi angle
		/// </summary>
		public float Phi
		{
			get { return this.phi; }
			set { this.phi = MathUtil.Clamp(value, 0.1f, MathUtil.Pi - 0.1f); }
		}

		/// <summary>
		/// The radius
		/// </summary>
		public float Radius
		{
			get { return this.radius; }
			set
			{
				this.radius = MathUtil.Clamp(value, this.MinRadius, this.MaxRadius);
			}
		}

        /// <summary>
        /// The max radius
        /// </summary>
        public float MaxRadius
		{
			get { return this.maxRadius; }
			set
			{
				this.maxRadius = value;

				if (this.radius > this.maxRadius)
				{
					this.radius = this.maxRadius;
				}
			}
		}

		/// <summary>
		/// The min radius
		/// </summary>
		public float MinRadius
		{
			get { return this.minRadius; }
			set
			{
				this.minRadius = value;

				if (this.radius < this.minRadius)
				{
					this.radius = this.minRadius;
				}
			}
		}

		/// <summary>
		/// Updates the view matrix
		/// </summary>
		public override void UpdateViewMatrix()
		{
            //Convert Spherical to Cartesian coordinates.
            this.position.X = (float)(this.radius * Math.Sin(this.phi) * Math.Cos(this.theta));
            this.position.Z = (float)(this.radius * Math.Sin(this.phi) * Math.Sin(this.theta));
            this.position.Y = (float)(this.radius * Math.Cos(this.phi));

            this.up = Vector3.Up;
			this.forward = this.position;
			this.forward.Normalize();
			this.right = Vector3.Cross(this.Up, this.Forward);

            var focusPosition = this.ToDrawPosition(this.FocusPosition);
            this.position = this.position + focusPosition;
            this.view = Matrix.LookAtLH(this.position, focusPosition, this.up);
            this.UpdateViewProjection();
		}

        public override void HandleMouseMove(Vector2 position, MouseButtons buttonDown)
        {
            if (buttonDown == System.Windows.Forms.MouseButtons.Left)
            {
                //Make each pixel correspond to a quarter of a degree.
                float dx = MathUtil.DegreesToRadians(0.25f * (float)(position.X - this.lastMousePosition.X));
                float dy = MathUtil.DegreesToRadians(0.25f * (float)(position.Y - this.lastMousePosition.Y));

                //Update angles based on input to orbit camera around box.
                this.theta += dx;
                this.phi += dy;

                //Restrict the angle mPhi.
                this.phi = MathUtil.Clamp(this.phi, 0.1f, MathUtil.Pi - 0.1f);
            }
            else if (buttonDown == System.Windows.Forms.MouseButtons.Right)
            {
                //Make each pixel correspond to 0.2 unit in the scene.
                //float dx = 0.2f * (float)(position.X - this.lastMousePosition.X);
                //float dy = 0.2f * (float)(position.Y - this.lastMousePosition.Y);
                var mouseZoomFactor = this.ZoomScaleFactor * 100.0f * 400.0f;
                float dx = mouseZoomFactor * (float)(position.X - this.lastMousePosition.X);
                float dy = mouseZoomFactor * (float)(position.Y - this.lastMousePosition.Y);

                //Update the camera radius based on input.
                this.radius += dx - dy;

                //Restrict the radius.
                this.radius = MathUtil.Clamp(this.radius, this.minRadius, this.maxRadius);
            }

            this.lastMousePosition = position;
        }

        public override void HandleMouseScroll(int delta)
        {
            this.radius -= delta * 0.005f * this.radius;
            this.radius = MathUtil.Clamp(this.radius, this.minRadius, this.maxRadius);
        }
    }
}
