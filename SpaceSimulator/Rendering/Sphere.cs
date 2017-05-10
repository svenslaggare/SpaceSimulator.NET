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
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Mathematics;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents a rendering of a sphere (planet, moon, etc.)
    /// </summary>
    public sealed class Sphere : IDisposable
    {
        private readonly BasicVertex[] vertices;
        private readonly int[] indices;

        private readonly Buffer vertexBuffer;
        private readonly Buffer indexBuffer;

        private readonly Texture2D texture;
        private readonly ShaderResourceView textureView;

        private readonly Material material;

        /// <summary>
        /// Creates a new spehre
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="textureFile">The path of the texture to use</param>
        /// <param name="material">The material</param>
        public Sphere(Device graphicsDevice, float radius, string textureFile, Material material)
        {
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

            this.indexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.IndexBuffer,
                this.indices);

            this.texture = TextureHelpers.FromFile(graphicsDevice, textureFile);
            this.textureView = new ShaderResourceView(graphicsDevice, this.texture);
            this.material = material;
        }

        /// <summary>
        /// Draws the sphere using the given effect
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, EffectPass pass, BaseCamera camera, Matrix world)
        {
            //Set draw type
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Set buffers
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            //Set per object constants
            effect.SetTransform(camera.ViewProjection, world);
            effect.SetMaterial(this.material);
            effect.SetTextureTransform(Matrix.Identity);
            effect.SetDiffuseMap(this.textureView);

            //Draw
            pass.Apply(deviceContext);
            deviceContext.DrawIndexed(this.indices.Length, 0, 0);
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
