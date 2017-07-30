using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Mathematics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Draws an arrow
    /// </summary>
    public sealed class Arrow : IDisposable
    {
        private readonly Device graphicsDevice;

        /// <summary>
        /// The height of the base
        /// </summary>
        public float BaseHeight { get; }

        /// <summary>
        /// The height of the head
        /// </summary>
        public float HeadHeight { get; }

        private readonly Cylinder arrowBase;
        private readonly Cylinder arrowHead;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new arrow
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the arrow</param>
        /// <param name="baseHeight">The height of the base</param>
        /// <param name="headHeight">The height of the head</param>
        public Arrow(Device graphicsDevice, float radius, float baseHeight, float headHeight)
        {
            this.graphicsDevice = graphicsDevice;
            this.BaseHeight = baseHeight;
            this.HeadHeight = headHeight;

            this.arrowBase = new Cylinder(graphicsDevice, radius, radius, baseHeight);
            this.arrowHead = new Cylinder(graphicsDevice, radius * 2.0f, 0, headHeight);

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
        /// Draws the arrow
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="color">The color of the arrow</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, BaseCamera camera, Matrix world, Color color)
        {
            //Set object constants
            effect.SetMaterial(new Material()
            {
                Ambient = color.ToVector4() * 0.25f,
                Diffuse = color.ToVector4(),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            });

            effect.SetEyePosition(camera.Position);
            this.directionalLights[0].Direction = (-camera.Look + 0.2f * camera.Up).Normalized();
            //this.directionalLights[0].Direction = MathHelpers.Normalized(-camera.Up);
            effect.SetDirectionalLights(this.directionalLights);

            deviceContext.InputAssembler.InputLayout = effect.InputLayout;

            var offset = Matrix.Translation(Vector3.Up * this.BaseHeight * 0.5f);
            var rotation = Matrix.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);
            var offsetRotationWorld = offset * rotation * world;

            this.arrowHead.Draw(
                deviceContext,
                effect,
                camera,
                offset * Matrix.Translation(Vector3.Up * this.HeadHeight * 0.5f) * offsetRotationWorld);

            this.arrowBase.Draw(
                deviceContext,
                effect,
                camera,
                offsetRotationWorld);
        }

        /// <summary>
        /// Draws an arrow in the given direction
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="scale">The scale of arrow</param>
        /// <param name="world">The world matrix</param>
        /// <param name="color">The color of the arrow</param>
        /// <param name="direction">The direction</param>
        public void DrawDirection(
            DeviceContext deviceContext,
            BasicEffect effect,
            BaseCamera camera,
            float scale,
            Matrix world,
            Color color,
            Vector3 direction)
        {
            this.Draw(
                deviceContext,
                effect,
                camera,
                Matrix.Scaling(scale) * MathHelpers.FaceDirection(direction, MathHelpers.Normal(direction)) * world,
                color);
        }

        /// <summary>
        /// Draws the given basis
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="scale">The scale of arrow</param>
        /// <param name="world">The world matrix</param>
        /// <param name="forward">The forward vector</param>
        /// <param name="up">The up vector</param>
        /// <param name="forwardColor">The color for the forward axis</param>
        /// <param name="upColor">The color for the up axis</param>
        /// <param name="rightColor">The color for the right axis</param>
        public void DrawBasis(
            DeviceContext deviceContext,
            BasicEffect effect,
            BaseCamera camera,
            float scale,
            Matrix world,
            Vector3 forward,
            Vector3 up,
            Color forwardColor,
            Color upColor,
            Color rightColor)
        {
            if (up.Length() <= 0.0001)
            {
                up = MathHelpers.Normal(forward);
            }

            var right = Vector3.Cross(forward, up);
            var scaling = Matrix.Scaling(scale);
            this.Draw(deviceContext, effect, camera, scaling * MathHelpers.FaceDirection(forward, up, right) * world, forwardColor);
            this.Draw(deviceContext, effect, camera, scaling * MathHelpers.FaceDirection(up, forward, right) * world, upColor);
            this.Draw(deviceContext, effect, camera, scaling * MathHelpers.FaceDirection(right, up, forward) * world, rightColor);
        }

        public void Dispose()
        {
            this.arrowBase.Dispose();
            this.arrowHead.Dispose();
        }
    }
}
