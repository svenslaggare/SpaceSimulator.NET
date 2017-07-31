using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Simulator;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Draws a rocket
    /// </summary>
    public sealed class Rocket : IDisposable, IPhysicsObjectModel
    {
        private readonly Device graphicsDevice;

        private readonly float radius;
        private readonly float noseConeHeight;
        private readonly float mainBodyHeight;
        private readonly float nozzleHeight;

        private readonly Cylinder noseCone;
        private readonly Cylinder mainBody;
        private readonly Cylinder engineMount;
        private readonly Cylinder engineNozzle;

        private readonly Arrow arrow;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the rocket</param>
        /// <param name="noseConeHeight">The height of the nose cone</param>
        /// <param name="mainBodyHeight">The height of the main body</param>
        /// <param name="nozzleHeight">The height of the nozzle</param>
        public Rocket(Device graphicsDevice, float radius, float noseConeHeight, float mainBodyHeight, float nozzleHeight)
        {
            this.graphicsDevice = graphicsDevice;

            this.radius = radius;
            this.noseConeHeight = noseConeHeight;
            this.mainBodyHeight = mainBodyHeight;
            this.nozzleHeight = nozzleHeight;

            this.noseCone = new Cylinder(graphicsDevice, this.radius, 0, this.noseConeHeight);
            this.mainBody = new Cylinder(graphicsDevice, this.radius, this.radius, this.mainBodyHeight);
            this.engineMount = new Cylinder(graphicsDevice, this.radius * 0.3f, this.radius * 0.3f, this.nozzleHeight * 0.75f);
            this.engineNozzle = new Cylinder(graphicsDevice, this.radius, this.radius * 0.3f, this.nozzleHeight);

            this.arrow = new Arrow(graphicsDevice, 0.25f, 10.0f, 2.0f);

            this.directionalLights = new DirectionalLight[]
            {
                new DirectionalLight()
                {
                    Ambient = Vector4.One,
                    Diffuse = Vector4.One,
                    Specular = Vector4.Zero,
                    Direction = Vector3.Up
                }
            };
        }

        /// <summary>
        /// Indicates if the effect is textured
        /// </summary>
        public bool IsTextured => false;

        /// <summary>
        /// Draws the given rocket object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="scale">The scale to draw the rocket at</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, RocketObject rocketObject, float scale = 0.01f)
        {
            //Compute transformations
            var position = camera.ToDrawPosition(rocketObject.Position);
            var targetPosition = camera.ToDrawPosition(rocketObject.Position + camera.FromDraw(1) * rocketObject.State.Prograde);
            var forward = (targetPosition - position).Normalized();
            var facing = MathHelpers.FaceDirection(forward);

            var world =
                Matrix.Scaling(scale)
                * facing
                * Matrix.Translation(camera.ToDrawPosition(rocketObject.Position));
            var rotation = Matrix.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);

            //Draw thrust arrow
            var arrowScale = camera.ToDraw(2.5E4);
            var thrustDirection = MathHelpers.ToFloat(rocketObject.EngineAcceleration().Normalized());

            var engineStartPosition = position - forward * scale * 0.5f * (this.mainBodyHeight + this.nozzleHeight * 1 + 0.2f * 1);
            var engineTargetPosition = engineStartPosition - thrustDirection * arrowScale * (this.arrow.BaseHeight + this.arrow.HeadHeight);

            this.arrow.DrawDirection(
                deviceContext,
                effect,
                camera,
                arrowScale,
                Matrix.Translation(engineTargetPosition),
                Color.Yellow,
                (engineStartPosition - engineTargetPosition).Normalized());

            //Draw rocket
            var color = Color.Gray;
            effect.SetMaterial(new Material()
            {
                Ambient = color.ToVector4() * 0.25f,
                Diffuse = color.ToVector4(),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            });

            effect.SetEyePosition(camera.Position);
            this.directionalLights[0].Direction = (camera.ToDrawPosition(Vector3d.Zero) - camera.Position).Normalized();
            effect.SetDirectionalLights(this.directionalLights);
            deviceContext.InputAssembler.InputLayout = effect.InputLayout;

            this.noseCone.Draw(
                deviceContext,
                effect,
                camera,
                rotation
                * Matrix.Translation(-Vector3.ForwardLH * 0.5f * (this.mainBodyHeight + this.noseConeHeight))
                * world);

            this.mainBody.Draw(
                deviceContext,
                effect,
                camera,
                rotation * world);

            this.engineMount.Draw(
                deviceContext,
                effect,
                camera,
                rotation
                * Matrix.Translation(Vector3.ForwardLH * 0.5f * (this.mainBodyHeight + this.nozzleHeight * 0.75f))
                * world);

            this.engineNozzle.Draw(
                deviceContext,
                effect,
                camera,
                rotation
                * Matrix.Translation(Vector3.ForwardLH * 0.5f * this.nozzleHeight)
                * MathHelpers.FaceDirection(thrustDirection.IsZero ? forward : thrustDirection)
                * Matrix.Translation(-forward * 0.5f * (this.mainBodyHeight))
                * Matrix.Scaling(scale)
                * Matrix.Translation(camera.ToDrawPosition(rocketObject.Position)));
        }

        /// <summary>
        /// Draws the given object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, PhysicsObject physicsObject)
        {
            if (physicsObject is RocketObject rocketObject)
            {
                this.Draw(deviceContext, effect, camera, rocketObject);
            }
        }

        public void Dispose()
        {
            this.noseCone.Dispose();
            this.mainBody.Dispose();
            this.engineMount.Dispose();
            this.engineNozzle.Dispose();
            this.arrow.Dispose();
        }
    }
}
