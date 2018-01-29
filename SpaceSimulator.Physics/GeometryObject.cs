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
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.PhysicsTest
{
    /// <summary>
    /// Represents a rendering of a geometry object
    /// </summary>
    public class GeometryObject
    {
        private readonly Device graphicsDevice;

        private readonly BasicVertex[] vertices;
        private readonly Buffer vertexBuffer;
        private readonly VertexBufferBinding vertexBufferBinding;

        private int[] indices;
        private readonly Buffer indexBuffer;

        /// <summary>
        /// Creates a new geometry object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="vertices">The vertices</param>
        /// <param name="indices">The indices</param>
        public GeometryObject(Device graphicsDevice, BasicVertex[] vertices, int[] indices)
        {
            this.graphicsDevice = graphicsDevice;
            this.vertices = vertices;
            this.indices = indices;

            this.vertexBuffer = Buffer.Create(
                this.graphicsDevice,
                BindFlags.VertexBuffer,
                this.vertices);

            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0);

            this.indexBuffer = Buffer.Create(
                this.graphicsDevice,
                BindFlags.IndexBuffer,
                this.indices);
        }

        /// <summary>
        /// Creates a new box objext
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="depth">The depth</param>
        public static GeometryObject Box(Device graphicsDevice, float width, float height, float depth)
        {
            GeometryGenerator.CreateBox(width, height, depth, out var geometryVertices, out var indices);
            var vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                TextureCoordinates = vertex.TextureCoordinates
            }).ToArray();

            return new GeometryObject(graphicsDevice, vertices, indices);
        }

        /// <summary>
        /// Creates a new sphere objext
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius</param>
        public static GeometryObject Sphere(Device graphicsDevice, float radius)
        {
            GeometryGenerator.CreateSphere(radius, 50, 50, out var geometryVertices, out var indices);
            var vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                TextureCoordinates = vertex.TextureCoordinates
            }).ToArray();

            return new GeometryObject(graphicsDevice, vertices, indices);
        }

        /// <summary>
        /// Creates a new plane objext
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="width">The width</param>
        /// <param name="depth">The depth</param>
        public static GeometryObject Plane(Device graphicsDevice, float width, float depth)
        {
            //GeometryGenerator.CreateFullscreenQuad(width, height, depth, out var geometryVertices, out var indices);
            //var vertices = geometryVertices.Select(vertex => new BasicVertex()
            //{
            //    Position = vertex.Position,
            //    Normal = vertex.Normal,
            //    TextureCoordinates = vertex.TextureCoordinates
            //}).ToArray();

            var vertices = new BasicVertex[4];
            var indices = new int[6];

            vertices[0] = new BasicVertex()
            {
                Position = new Vector3(-width, 0.0f, -depth),
                Normal = new Vector3(0.0f, 1.0f, 0.0f),
                TextureCoordinates = new Vector2(0.0f, 1.0f)
            };

            vertices[1] = new BasicVertex()
            {
                Position = new Vector3(-width, 0.0f, depth),
                Normal = new Vector3(0.0f, 1.0f, 0.0f),
                TextureCoordinates = new Vector2(0.0f, 0.0f)
            };

            vertices[2] = new BasicVertex()
            {
                Position = new Vector3(width, 0.0f, depth),
                Normal = new Vector3(0.0f, 1.0f, 0.0f),
                TextureCoordinates = new Vector2(1.0f, 0.0f)
            };

            vertices[3] = new BasicVertex()
            {
                Position = new Vector3(width, 0.0f, -depth),
                Normal = new Vector3(0.0f, 1.0f, 0.0f),
                TextureCoordinates = new Vector2(1.0f, 1.0f)
            };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            return new GeometryObject(graphicsDevice, vertices, indices);
        }

        /// <summary>
        /// Draws the object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="color">The color</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, BaseCamera camera, Matrix world, Color color)
        {
            deviceContext.InputAssembler.InputLayout = effect.InputLayout;
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            effect.SetMaterial(new Material()
            {
                Ambient = color.ToVector4() * 0.25f,
                Diffuse = color.ToVector4(),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            });

            effect.SetEyePosition(camera.Position);
            effect.SetTransform(camera.ViewProjection, world);

            //Draw
            foreach (var pass in effect.Passes)
            {
                pass.Apply(deviceContext);
                deviceContext.DrawIndexed(this.indices.Length, 0, 0);
            }
        }
    }
}
