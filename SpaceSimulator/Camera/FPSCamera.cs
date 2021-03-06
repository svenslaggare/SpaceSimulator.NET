﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;

namespace SpaceSimulator.Camera
{
	/// <summary>
	/// Represents a first person camera
	/// </summary>
	public class FPSCamera : SpaceCamera
    {
		private Vector2 lastMousePosition = Vector2.Zero;

		/// <summary>
		/// The speed of the camera
		/// </summary>
		public float Speed { get; set; }

		/// <summary>
		/// Creates a new FPS camera
		/// </summary>
		public FPSCamera()
		{
			this.Speed = 20;
		}

		/// <summary>
		/// The position of the camera
		/// </summary>
		public new Vector3 Position
		{
			get { return this.position; }
			set { this.position = value; }
		}

		/// <summary>
		/// Strafes the camera by the given amount
		/// </summary>
		/// <param name="amount">The amount/param>
		public void Strafe(float amount)
		{
			this.position += amount * this.Right;
		}

		/// <summary>
		/// Walk the camera by the given amount
		/// </summary>
		/// <param name="amount">The amount</param>
		public void Walk(float amount)
		{
			this.position += amount * this.Forward;
		}

		/// <summary>
		/// Rotates up/down around the right vector
		/// </summary>
		/// <param name="angle">The angle to rotate</param>
		public void Pitch(float angle)
		{
			var rotation = Matrix.RotationAxis(this.Right, angle);
			this.up = Vector3.TransformNormal(this.Up, rotation);
			this.forward = Vector3.TransformNormal(this.Forward, rotation);
		}

		/// <summary>
		/// Rotates the basis vector around the y-axis
		/// </summary>
		/// <param name="angle">The angle</param>
		public void RotateY(float angle)
		{
			var rotation = Matrix.RotationY(angle);
			this.right = Vector3.TransformNormal(this.Right, rotation);
			this.up = Vector3.TransformNormal(this.Up, rotation);
			this.forward = Vector3.TransformNormal(this.Forward, rotation);
		}

		/// <summary>
		/// Updates the view matrix
		/// </summary>
		public override void UpdateViewMatrix()
		{
			this.forward.Normalize();

			this.up = Vector3.Cross(this.Forward, this.Right);
			this.up.Normalize();

			this.right = Vector3.Cross(this.Up, this.Forward);

			this.view = Matrix.LookAtLH(this.Position, this.Position + this.Forward, this.Up);
            this.UpdateViewProjection();
		}

		/// <summary>
		/// Handles the keyboard
		/// </summary>
		/// <param name="keyboardState">The keyboard state</param>
		/// <param name="elapsed">The elapsed time since the last frame</param>
		public override void HandleKeyboard(KeyboardState keyboardState, TimeSpan elapsed)
		{
			//Keyboard
			float speed = this.Speed * (float)elapsed.TotalSeconds;

			if (keyboardState.IsPressed(Key.W))
			{
				this.Walk(speed);
			}

			if (keyboardState.IsPressed(Key.S))
			{
				this.Walk(-speed);
			}

			if (keyboardState.IsPressed(Key.D))
			{
				this.Strafe(speed);
			}

			if (keyboardState.IsPressed(Key.A))
			{
				this.Strafe(-speed);
			}

            if (keyboardState.IsPressed(Key.Up))
            {
                this.position += speed * this.Up;
            }

            if (keyboardState.IsPressed(Key.Down))
            {
                this.position -= speed * this.Up;
            }


            this.UpdateViewMatrix();
		}

		/// <summary>
		/// Handles when the mouse move
		/// </summary>
		/// <param name="position">The position of the mouse</param>
		/// <param name="buttonDown">The down mouse button</param>
		public override void HandleMouseMove(Vector2 position, System.Windows.Forms.MouseButtons buttonDown)
		{
			if (buttonDown == System.Windows.Forms.MouseButtons.Left)
			{
				//Make each pixel correspond to a quarter of a degree.
				float dx = MathUtil.DegreesToRadians(0.25f * (float)(position.X - this.lastMousePosition.X));
				float dy = MathUtil.DegreesToRadians(0.25f * (float)(position.Y - this.lastMousePosition.Y));

				this.Pitch(dy);
				this.RotateY(dx);
			}

			this.lastMousePosition = position;
		}
	}
}
