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

        private readonly Matrix baseTransform = Matrix.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);
        private readonly Matrix3x3 baseTransform3x3 = Matrix3x3.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);

        /// <summary>
        /// Creates a new cylinder
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="bottomRadius">The bottom radius</param>
        /// <param name="topRadius">The top radius</param>
        /// <param name="height">The height of the cylinder</param>
        /// <param name="transform">Indicates if the cylinder is transformed such that its default orientation is facing forwards</param>
        public Cylinder(Device graphicsDevice, float bottomRadius, float topRadius, float height, bool transform = false)
        {
            this.graphicsDevice = graphicsDevice;

            GeometryGenerator.CreateCylinder(bottomRadius, topRadius, height, 50, 50, out var geometryVertices, out this.indices);
            this.vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = transform ? Vector3.TransformCoordinate(vertex.Position, this.baseTransform) : vertex.Position,
                Normal = transform ? Vector3.Transform(vertex.Normal, this.baseTransform3x3).Normalized() : vertex.Normal,
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
        public void Draw(DeviceContext deviceContext, BasicEffect effect, BaseCamera camera, Matrix world)
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
                deviceContext.DrawIndexed(this.indices.Length, 0, 0);
            }
        }

        public void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }
    }
}
