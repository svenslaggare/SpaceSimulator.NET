﻿using System;
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
        /// The follow distance
        /// </summary>
        public double Distance { get; set; } = 4240000.0;

        public override void UpdateViewMatrix()
        {
            var cameraPosition = this.FocusPosition + MathHelpers.Normalized(-this.followForward + this.followNormal * 1) * this.Distance;
            var targetPosition = this.FocusPosition;

            this.position = this.ToDrawPosition(cameraPosition);
            var targetDrawPosition = this.ToDrawPosition(targetPosition);

            this.up = MathHelpers.Normalized(this.ToDrawPosition(this.FocusPosition + this.FromDraw(1.0f) * this.followNormal) - this.position);
            this.look = MathHelpers.Normalized(targetDrawPosition - this.position);
            this.right = Vector3.Cross(this.Up, this.Look);

            this.view = Matrix.LookAtLH(this.position, targetDrawPosition, this.up);
            this.UpdateViewProjection();
        }

        public override void HandleMouseScroll(int delta)
        {
            base.HandleMouseScroll(delta);
            //this.Distance += delta * 1E3;
            this.Distance += -delta * this.Distance * 0.001;
        }

        public override void SetFocus(PhysicsObject physicsObject)
        {
            base.SetFocus(physicsObject);

            var state = physicsObject.State;
            state.MakeRelative(physicsObject.PrimaryBody.State);

            this.followForward = state.Prograde;

            if (state.Prograde.Equals(state.Radial))
            {
                var pseudoRadial = MathHelpers.Normalized(Vector3d.Transform(state.Prograde, Matrix3x3d.RotationY(Math.PI / 2)));
                var pseudoNormal = Vector3d.Cross(state.Prograde, pseudoRadial);
                var sphereNormal = OrbitHelpers.SphereNormal(this.Focus.PrimaryBody, this.Focus.Latitude, this.Focus.Longitude);
                var sphereTangent = OrbitHelpers.SphereNormal(this.Focus.PrimaryBody, this.Focus.Latitude, this.Focus.Longitude + Math.PI / 2);

                this.followNormal = sphereNormal;
                this.followForward = sphereTangent;
            }
            else
            {
                this.followNormal = state.Radial;
                //this.followNormal = state.Normal;
            }
        }

        public override bool CanSetFocus(PhysicsObject physicsObject)
        {
            return !physicsObject.IsObjectOfReference;
        }
    }
}
