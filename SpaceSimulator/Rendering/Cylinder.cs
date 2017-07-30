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
    /// Draws a cylinder
    /// </summary>
    public sealed class Cylinder : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly BasicVertex[] vertices;
        private readonly Buffer vertexBuffer;
        private readonly VertexBufferBinding vertexBufferBinding;

        private readonly int[] indices;
        private readonly Buffer indexBuffer;

        /// <summary>
        /// Creates a new cylinder
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="bottomRadius">The bottom radius</param>
        /// <param name="topRadius">The top radius</param>
        /// <param name="height">The height of the cylinder</param>
        public Cylinder(Device graphicsDevice, float bottomRadius, float topRadius, float height)
        {
            this.graphicsDevice = graphicsDevice;

            GeometryGenerator.CreateCylinder(bottomRadius, topRadius, height, 50, 50, out var geometryVertices, out var this.indices);
            this.vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                TextureCoordinates = vertex.TextureCoordinates
            }).ToArray();

            this.vertexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.VertexBuffer,
                vertices);

            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0);

            this.indexBuffer = Buffer.Create(
                graphicsDevice,
                BindFlags.IndexBuffer,
                this.indices);
        }

        /// <summary>
        /// Draws the cylinder
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        public void DrawPart(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, Matrix world)
        {
            effect.SetTransform(camera.ViewProjection, world);

            //Set input assembler
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            //Draw
            foreach (var pass in effect.Passes)
            {
                pass.Apply(deviceContext);
                deviceContext.DrawIndexed(this.vertices.Length, 0, 0);
            }
        }

        public void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }
    }
}
