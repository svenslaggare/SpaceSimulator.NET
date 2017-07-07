using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;

namespace SpaceSimulator.Common.Camera
{
    /// <summary>
    /// Represents a base camera
    /// </summary>
    public abstract class BaseCamera
    {
        protected Vector3 position;
        protected Vector3 right;
        protected Vector3 up;
        protected Vector3 look;

        private float nearZ;
        private float farZ;
        protected float viewportWidth;
        protected float viewportHeight;
        private float fovY;
        private float nearWindowHeight;
        private float farWindowHeight;

        protected Matrix view;
        protected Matrix projection;
        private Matrix viewProjection;

        /// <summary>
        /// Creates a new base camera
        /// </summary>
        public BaseCamera()
        {
            this.right = Vector3.Right;
            this.up = Vector3.Up;
            this.look = Vector3.ForwardLH;
        }

        /// <summary>
        /// Creates a new camera that is a copy of the given camera
        /// </summary>
        /// <param name="camera">The camera</param>
        public BaseCamera(BaseCamera camera)
        {
            this.position = camera.position;
            this.right = camera.right;
            this.up = camera.up;
            this.look = camera.look;

            this.nearZ = camera.nearZ;
            this.farZ = camera.farZ;
            this.viewportWidth = camera.viewportWidth;
            this.viewportHeight = camera.viewportHeight;
            this.fovY = camera.fovY;
            this.nearWindowHeight = camera.nearWindowHeight;
            this.farWindowHeight = camera.farWindowHeight;

            this.view = camera.view;
            this.projection = camera.projection;
            this.viewProjection = camera.viewProjection;
        }

        /// <summary>
        /// The position of the camera
        /// </summary>
        public virtual Vector3 Position
        {
            get { return this.position; }
        }

        /// <summary>
        /// The right vector
        /// </summary>
        public Vector3 Right
        {
            get { return this.right; }
        }

        /// <summary>
        /// The up vector
        /// </summary>
        public Vector3 Up
        {
            get { return this.up; }
        }

        /// <summary>
        /// The look vector
        /// </summary>
        public Vector3 Look
        {
            get { return this.look; }
        }

        /// <summary>
        /// Returns the near plane
        /// </summary>
        public float NearPlane
        {
            get { return this.nearZ; }
        }

        /// <summary>
        /// Returns the far plane
        /// </summary>
        public float FarPlane
        {
            get { return this.farZ; }
        }

        /// <summary>
        /// Returns the aspect ratio
        /// </summary>
        public float AspectRatio => this.viewportWidth / this.viewportHeight;

        /// <summary>
        /// Returns the field of view
        /// </summary>
        public Vector2 FieldOfView
        {
            get
            {
                float halfWidth = 0.5f * this.NearWindowWidth;
                return new Vector2(2.0f * (float)Math.Atan(halfWidth / nearZ), this.fovY);
            }
        }

        /// <summary>
        /// Returns the near window width
        /// </summary>
        public float NearWindowWidth
        {
            get { return this.AspectRatio * this.nearWindowHeight; }
        }

        /// <summary>
        /// Returns the near window height
        /// </summary>
        public float NearWindowHeight
        {
            get { return this.nearWindowHeight; }
        }

        /// <summary>
        /// Returns the far window width
        /// </summary>
        public float FarWindowWidth
        {
            get { return this.AspectRatio * this.farWindowHeight; }
        }

        /// <summary>
        /// Returns the far window height
        /// </summary>
        public float FarWindowHeight
        {
            get { return this.farWindowHeight; }
        }

        /// <summary>
        /// Returns the view matrix
        /// </summary>
        public Matrix View
        {
            get { return this.view; }
        }

        /// <summary>
        /// Returns the projection matrix
        /// </summary>
        public Matrix Projection
        {
            get { return this.projection; }
        }

        /// <summary>
        /// Returns the combined view-projection matrix
        /// </summary>
        public Matrix ViewProjection
        {
            get { return this.viewProjection; }
        }

        /// <summary>
        /// Sets the frustum
        /// </summary>
        /// <param name="fovY">The field of view in the y-direction</param>
        /// <param name="viewportWidth">The width of the viewport</param>
        /// <param name="viewportHeight">The height of the viewport</param>
        /// <param name="nearPlane">The near plane</param>
        /// <param name="farPlane">The far plane</param>
        public virtual void SetLens(float fovY, float viewportWidth, float viewportHeight, float nearPlane, float farPlane)
        {
            this.fovY = fovY;
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            this.nearZ = nearPlane;
            this.farZ = farPlane;

            this.nearWindowHeight = 2.0f * this.nearZ * (float)Math.Tan(0.5f * this.fovY);
            this.farWindowHeight = 2.0f * this.farZ * (float)Math.Tan(0.5f * this.fovY);

            this.projection = Matrix.PerspectiveFovLH(this.fovY, this.AspectRatio, this.nearZ, this.farZ);
            this.UpdateViewProjection();
        }

        /// <summary>
        /// Defines the camera space
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="target">The target</param>
        /// <param name="up">The up</param>
        public virtual void LookAt(Vector3 position, Vector3 target, Vector3 up)
        {
            this.look = target - position;
            this.look.Normalize();

            this.right = Vector3.Cross(up, this.look);
            this.right.Normalize();

            this.up = Vector3.Cross(this.look, this.Right);
        }

        /// <summary>
        /// Updates the view-projection matrix
        /// </summary>
        protected void UpdateViewProjection()
        {
            this.viewProjection = this.view * this.projection;
        }

        /// <summary>
        /// Updates the view matrix
        /// </summary>
        public abstract void UpdateViewMatrix();

        /// <summary>
        /// Handles the keyboard
        /// </summary>
        /// <param name="keyboardState">The keyboard state</param>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public virtual void HandleKeyboard(KeyboardState keyboardState, TimeSpan elapsed)
        {

        }

        /// <summary>
        /// Handles when the mouse move
        /// </summary>
        /// <param name="position">The position of the mouse</param>
        /// <param name="buttonDown">The down mouse button</param>
        public virtual void HandleMouseMove(Vector2 position, System.Windows.Forms.MouseButtons buttonDown)
        {

        }

        /// <summary>
		/// Handles when the mouse button is down
		/// </summary>
		/// <param name="position">The position of the mouse</param>
		/// <param name="buttonDown">The down mouse button</param>
		public virtual void HandleMouseDown(Vector2 position, System.Windows.Forms.MouseButtons buttonDown)
        {

        }

        /// <summary>
        /// Handles when the mouse is scrolled
        /// </summary>
        /// <param name="delta">The delta</param>
        public virtual void HandleMouseScroll(int delta)
        {

        }

        /// <summary>
        /// Projects the given position in world space to screen space
        /// </summary>
        /// <param name="position">The position in world space</param>
        /// <returns>The position in screen space</returns>
        public Vector2 Project(Vector3 position)
        {
            return this.Project(position, out var depth);
        }

        /// <summary>
        /// Projects the given position in world space to screen space
        /// </summary>
        /// <param name="position">The position in world space</param>
        /// <param name="depth">The depth of the position</param>
        /// <returns>The position in screen space</returns>
        public Vector2 Project(Vector3 position, out double depth)
        {
            var screenPosition = Vector3.TransformCoordinate(position, this.ViewProjection);
            depth = screenPosition.Z;
            //screenPosition /= screenPosition.Z;

            screenPosition.X = this.viewportWidth * (screenPosition.X + 1.0f) / 2.0f;
            screenPosition.Y = this.viewportHeight * (1.0f - ((screenPosition.Y + 1.0f) / 2.0f));
            return new Vector2(screenPosition.X, screenPosition.Y);
        }

    }
}
