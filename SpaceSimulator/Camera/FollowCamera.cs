using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Camera
{
    /// <summary>
    /// Represents a follow camera
    /// </summary>
    public sealed class FollowCamera : SpaceCamera
    {
        private Vector3d followForward = Vector3d.ForwardLH;
        private Vector3d followNormal = Vector3d.Up;

        /// <summary>
        /// The follow mode
        /// </summary>
        public enum Mode { FollowNormal, FollowRadial, FollowAscent }

        /// <summary>
        /// The follow mode
        /// </summary>
        public Mode FollowMode { get; }

        /// <summary>
        /// The follow distance
        /// </summary>
        public double Distance { get; set; } = 4240000.0;

        /// <summary>
        /// Creates a new follow camera
        /// </summary>
        /// <param name="followMode">The follow mode</param>
        public FollowCamera(Mode followMode)
        {
            this.FollowMode = followMode;
        }

        public override void UpdateViewMatrix()
        {
            var cameraPosition = this.FocusPosition + (-this.followForward + this.followNormal * 1).Normalized() * this.Distance;
            var targetPosition = this.FocusPosition;

            this.position = this.ToDrawPosition(cameraPosition);
            var targetDrawPosition = this.ToDrawPosition(targetPosition);

            this.up = (this.ToDrawPosition(this.FocusPosition + this.FromDraw(1.0f) * this.followNormal) - this.position).Normalized();
            this.forward = (targetDrawPosition - this.position).Normalized();
            this.right = Vector3.Cross(this.Up, this.Forward);

            this.view = Matrix.LookAtLH(this.position, targetDrawPosition, this.up);
            this.UpdateViewProjection();
        }

        public override void HandleMouseScroll(int delta)
        {
            base.HandleMouseScroll(delta);
            this.Distance += -delta * this.Distance * 0.001;
        }

        public override void SetFocus(PhysicsObject physicsObject)
        {
            base.SetFocus(physicsObject);

            var state = physicsObject.State;
            state.MakeRelative(physicsObject.PrimaryBody.State);

            switch (this.FollowMode)
            {
                case Mode.FollowNormal:
                    this.followForward = state.Prograde;
                    this.followNormal = state.Normal;
                    break;
                case Mode.FollowRadial:
                    this.followForward = state.Prograde;
                    this.followNormal = state.Radial;
                    break;
                case Mode.FollowAscent:
                    //var pseudoRadial = MathHelpers.Normalized(Vector3d.Transform(state.Prograde, Matrix3x3d.RotationY(Math.PI / 2)));
                    //var pseudoNormal = Vector3d.Cross(state.Prograde, pseudoRadial);
                    var sphereNormal = OrbitHelpers.SphereNormal(this.Focus.PrimaryBody, this.Focus.Latitude, this.Focus.Longitude);
                    var sphereTangent = OrbitHelpers.SphereNormal(this.Focus.PrimaryBody, this.Focus.Latitude, this.Focus.Longitude + Math.PI / 2);

                    this.followNormal = sphereNormal;
                    this.followForward = sphereTangent;
                    //this.followNormal = pseudoNormal;
                    //this.followForward = state.Prograde;
                    break;
            }
        }

        public override bool CanSetFocus(PhysicsObject physicsObject)
        {
            return !physicsObject.IsObjectOfReference;
        }
    }
}
