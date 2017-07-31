using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SpaceSimulator.Common;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Mathematics;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents a rendering of a sphere (planet, moon, etc.)
    /// </summary>
    public sealed class Sphere : IDisposable, IPhysicsObjectModel
    {
        private readonly BasicVertex[] vertices;
        private readonly int[] indices;

        private readonly Buffer vertexBuffer;
        private readonly VertexBufferBinding vertexBufferBinding;
        private readonly Buffer indexBuffer;

        private readonly Texture2D texture;
        private readonly ShaderResourceView textureView;

        /// <summary>
        /// The material
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// The name of the texture
        /// </summary>
        public string TextureName { get; }

        /// <summary>
        /// The transform before world transform
        /// </summary>
        public Matrix Transform { get; }

        /// <summary>
        /// Creates a new spehre
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="textureName">The path of the texture to use</param>
        /// <param name="material">The material</param>
        /// <param name="transform">Transform before world transform</param>
        public Sphere(Device graphicsDevice, float radius, string textureName, Material material, Matrix transform)
        {
            this.TextureName = textureName;

            GeometryGenerator.CreateSphere(radius, 50, 50, out var sphereVertices, out this.indices);
            this.vertices = new BasicVertex[sphereVertices.Length];

            for (int i = 0; i < sphereVertices.Length; i++)
            {
                var geometryVertex = sphereVertices[i];
                var vertex = new BasicVertex()
                {
                    Position = geometryVertex.Position,
                    Normal = geometryVertex.Normal,
                    TextureCoordinates = geometryVertex.TextureCoordinates
                };
                this.vertices[i] = vertex;
            }

            this.vertexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.VertexBuffer,
                this.vertices);

            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0);

            this.indexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.IndexBuffer,
                this.indices);

            this.texture = TextureHelpers.FromFile(graphicsDevice, textureName);
            this.textureView = new ShaderResourceView(graphicsDevice, this.texture);
            this.Material = material;

            this.Transform = transform;
        }

        /// <summary>
        /// Indicates if the effect is textured
        /// </summary>
        public bool IsTextured => true;

        /// <summary>
        /// Draws the sphere using the given effect
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, EffectPass pass, SpaceCamera camera, Matrix world)
        {
            //Set draw type
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Set buffers
            deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            //Set per object constants
            effect.SetTransform(camera.ViewProjection, world);
            effect.SetMaterial(this.Material);
            effect.SetTextureTransform(Matrix.Identity);
            effect.SetDiffuseMap(this.textureView);

            //Draw
            pass.Apply(deviceContext);
            deviceContext.DrawIndexed(this.indices.Length, 0, 0);
        }

        /// <summary>
        /// Returns the scaling matrix
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The object</param>
        private Matrix ScalingMatrix(SpaceCamera camera, PhysicsObject physicsObject)
        {
            var size = 0.0f;
            if (physicsObject is NaturalSatelliteObject naturalObject)
            {
                size = camera.ToDraw(naturalObject.Radius);
            }
            else
            {
                size = camera.ToDraw(Simulator.Data.SolarSystemBodies.Earth.Radius * 0.01);
            }

            return Matrix.Scaling(size);
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
            effect.SetEyePosition(camera.Position);
            effect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));
            deviceContext.InputAssembler.InputLayout = effect.InputLayout;

            var world =
                this.ScalingMatrix(camera, physicsObject)
                * this.Transform
                * Matrix.RotationY(-(float)physicsObject.Rotation)
                * Matrix.Translation(camera.ToDrawPosition(physicsObject.Position));

            foreach (var pass in effect.Passes)
            {
                this.Draw(
                    deviceContext,
                    effect,
                    pass,
                    camera,
                    world);
            }
        }

        public void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
            this.texture.Dispose();
            this.texture.Dispose();
        }
    }
}
