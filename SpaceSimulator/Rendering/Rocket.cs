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
    public sealed class Rocket : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly float radius;
        private readonly float noseConeHeight;
        private readonly float baseHeight;
        private readonly float engineHeight;

        private readonly BasicVertex[] noseConeVertices;
        private readonly Buffer noseConeVertexBuffer;
        private readonly VertexBufferBinding noseConeVertexBufferBinding;
        private readonly int[] noseConeIndices;
        private readonly Buffer noseConeIndexBuffer;

        private readonly BasicVertex[] baseVertices;
        private readonly Buffer baseVertexBuffer;
        private readonly VertexBufferBinding baseVertexBufferBinding;
        private readonly int[] baseIndices;
        private readonly Buffer baseIndexBuffer;

        private readonly BasicVertex[] engineVertices;
        private readonly Buffer engineVertexBuffer;
        private readonly VertexBufferBinding engineVertexBufferBinding;
        private readonly int[] engineIndices;
        private readonly Buffer engineIndexBuffer;

        private readonly Arrow arrow;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the rocket</param>
        /// <param name="noseConeHeight">The height of the nose cone</param>
        /// <param name="baseHeight">The height of the base</param>
        /// <param name="engineHeight">The height of the nozzle</param>
        public Rocket(Device graphicsDevice, float radius, float noseConeHeight, float baseHeight, float engineHeight)
        {
            this.graphicsDevice = graphicsDevice;
            this.radius = radius;
            this.noseConeHeight = noseConeHeight;
            this.baseHeight = baseHeight;
            this.engineHeight = engineHeight;

            (this.noseConeVertices, this.noseConeVertexBuffer, this.noseConeVertexBufferBinding, this.noseConeIndices, this.noseConeIndexBuffer) = 
                this.CreateGeometry(this.radius, 0, this.noseConeHeight);

            (this.baseVertices, this.baseVertexBuffer, this.baseVertexBufferBinding, this.baseIndices, this.baseIndexBuffer) = 
                this.CreateGeometry(this.radius, this.radius, this.baseHeight);

            (this.engineVertices, this.engineVertexBuffer, this.engineVertexBufferBinding, this.engineIndices, this.engineIndexBuffer) = 
                this.CreateGeometry(this.radius, radius * 0.3f, this.engineHeight);

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
        /// Creates the geometry
        /// </summary>
        private (BasicVertex[], Buffer, VertexBufferBinding, int[], Buffer) CreateGeometry(float bottomRadius, float topRadius, float height)
        {
            GeometryGenerator.CreateCylinder(bottomRadius, topRadius, height, 50, 50, out var geometryVertices, out var indices);
            var vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                TextureCoordinates = vertex.TextureCoordinates
            }).ToArray();

            var vertexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.VertexBuffer,
                vertices);

            var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0);

            var indexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.IndexBuffer,
                indices);

            return (vertices, vertexBuffer, vertexBufferBinding, indices, indexBuffer);
        }

        /// <summary>
        /// Draws the given part
        /// </summary>
        private void DrawPart(
            DeviceContext deviceContext,
            BasicEffect effect,
            VertexBufferBinding vertexBufferBinding,
            Buffer indexBuffer,
            int count,
            SpaceCamera camera,
            Matrix world)
        {
            effect.SetTransform(camera.ViewProjection, world);

            //Set input assembler
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            //Draw
            foreach (var pass in effect.Passes)
            {
                pass.Apply(deviceContext);
                deviceContext.DrawIndexed(count, 0, 0);
            }
        }

        /// <summary>
        /// Draws the rocket
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, Matrix world)
        {
            //Set object constants
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

            var rotation = Matrix.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);
            var offsetRotationWorld = rotation * world;

            this.DrawPart(
                deviceContext,
                effect,
                this.noseConeVertexBufferBinding,
                this.noseConeIndexBuffer,
                this.noseConeIndices.Length,
                camera,
                Matrix.Translation(Vector3.Up * this.baseHeight * 0.5f)
                * Matrix.Translation(Vector3.Up * this.noseConeHeight * 0.5f)
                * offsetRotationWorld);

            this.DrawPart(
                deviceContext,
                effect,
                this.baseVertexBufferBinding,
                this.baseIndexBuffer,
                this.baseIndices.Length,
                camera,
                offsetRotationWorld);

            this.DrawPart(
                deviceContext,
                effect,
                this.engineVertexBufferBinding,
                this.engineIndexBuffer,
                this.engineIndices.Length,
                camera,
                Matrix.Translation(Vector3.Down * this.baseHeight * 0.5f)
                * Matrix.Translation(Vector3.Down * this.engineHeight * 0.5f)
                * offsetRotationWorld);
        }

        /// <summary>
        /// Draws the given rocket object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="rocketObject">The rocket object</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, RocketObject rocketObject)
        {
            var position = camera.ToDrawPosition(rocketObject.Position);
            var targetPosition = camera.ToDrawPosition(rocketObject.Position + camera.FromDraw(1) * rocketObject.State.Prograde);
            var upPosition = camera.ToDrawPosition(rocketObject.Position + camera.FromDraw(1) * rocketObject.State.Normal);

            //var facing = MathHelpers.FaceDirection(
            //    MathHelpers.Normalized(targetPosition - position),
            //    MathHelpers.Normalized(upPosition - position));
            var forward = (targetPosition - position).Normalized();
            var facing = MathHelpers.FaceDirection(forward, MathHelpers.Normal(forward));

            var scale = 0.01f;
            var world =
                facing
                * Matrix.Scaling(scale)
                * Matrix.Translation(camera.ToDrawPosition(rocketObject.Position));

            var arrowScale = camera.ToDraw(2.5E4);
            var thrustDirection = MathHelpers.ToFloat(rocketObject.EngineAcceleration().Normalized());

            var engineStartPosition = position - forward * scale * 0.5f * (this.baseHeight + this.engineHeight + 0.2f);
            var engineTargetPosition = engineStartPosition - thrustDirection * arrowScale * (this.arrow.BaseHeight + this.arrow.HeadHeight);

            //this.arrow.DrawDirection(
            //    deviceContext,
            //    effect,
            //    camera,
            //    arrowScale,
            //    Matrix.Translation(position - forward * scale * 0.5f * (this.baseHeight + this.engineHeight)),
            //    Color.Yellow,
            //    -thrustDirection);

            this.arrow.DrawDirection(
                deviceContext,
                effect,
                camera,
                arrowScale,
                Matrix.Translation(engineTargetPosition),
                Color.Yellow,
                thrustDirection);

            this.Draw(deviceContext, effect, camera, world);
        }

        public void Dispose()
        {
            this.noseConeVertexBuffer.Dispose();
            this.noseConeIndexBuffer.Dispose();

            this.baseVertexBuffer.Dispose();
            this.baseIndexBuffer.Dispose();

            this.engineVertexBuffer.Dispose();
            this.engineIndexBuffer.Dispose();

            this.arrow.Dispose();
        }
    }
}
