using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Common;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// The vertex format for the orbit effect
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct OrbitVertex
    {
        [FieldOffset(0)]
        public Vector3 Position;

        [FieldOffset(1 * 3 * 4)]
        public Vector3 NextPosition;

        [FieldOffset(2 * 3 * 4)]
        public Vector3 PrevPosition;

        [FieldOffset(3 * 3 * 4)]
        public Vector3 Normal;

        [FieldOffset(4 * 3 * 4)]
        public Vector4 Color;

        [FieldOffset(4 * 4 + 4 * 3 * 4)]
        public Vector4 NextColor;

        /// <summary>
        /// Creates the input elements
        /// </summary>
        public static InputElement[] CreateInput()
        {
            return new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("POSITION", 1, Format.R32G32B32_Float, 1 * 3 * 4, 0),
                new InputElement("POSITION", 2, Format.R32G32B32_Float, 2 * 3 * 4, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 3 * 3 * 4, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 4 * 3 * 4, 0),
                new InputElement("COLOR", 1, Format.R32G32B32A32_Float, 4 * 4 + 4 * 3 * 4, 0),
            };
        }
    };

    /// <summary>
    /// Represents an orbit used for rendering
    /// </summary>
    public class Orbit : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly Color color;
        private readonly float lineWidth;
        private IList<Point> positions;

        private OrbitVertex[] vertices;   
        private Buffer vertexBuffer;

        private RasterizerStates rasterizerStates;
        private BlendStates blendStates;

        /// <summary>
        /// The true anomaly of the last passed position
        /// </summary>
        public double PassedTrueAnomaly { get; set; }

        /// <summary>
        /// Represents a point in the orbit
        /// </summary>
        public struct Point
        {
            /// <summary>
            /// The position
            /// </summary>
            public Vector3 Position { get; }

            /// <summary>
            /// The true anomaly
            /// </summary>
            public double TrueAnomaly { get; }

            public Point(Vector3 position, double trueAnomaly)
            {
                this.Position = position;
                this.TrueAnomaly = trueAnomaly;
            }
        }

        /// <summary>
        /// Creates a new orbit
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="positions">The positions of the points in the orbit</param>
        /// <param name="color">The color of the orbit</param>
        /// <param name="width">The width of the drawn orbit</param>
        public Orbit(Device graphicsDevice, IList<Point> positions, Color color, float width)
        {
            this.graphicsDevice = graphicsDevice;
            this.positions = positions;
            this.UpdateVertices();

            if (positions.Count > 0)
            {
                this.CreateVertexBuffer();
            }

            this.color = color;
            this.lineWidth = width;

            this.rasterizerStates = new RasterizerStates(graphicsDevice);
            this.blendStates = new BlendStates(graphicsDevice);
        }

        /// <summary>
        /// Creates the vertex buffer
        /// </summary>
        private void CreateVertexBuffer()
        {
            this.vertexBuffer = Buffer.Create(
                this.graphicsDevice,
                BindFlags.VertexBuffer,
                this.vertices,
                usage: ResourceUsage.Dynamic,
                accessFlags: CpuAccessFlags.Write);
        }

        /// <summary>
        /// Updates the verticies
        /// </summary>
        private void UpdateVertices()
        {
            var vertices = new List<OrbitVertex>();
            for (int i = 0; i < this.positions.Count; i++)
            {
                var nextIndex = i + 1;
                if (nextIndex >= positions.Count)
                {
                    nextIndex = 0;
                }

                var prevIndex = i - 1;
                if (prevIndex < 0)
                {
                    prevIndex = positions.Count - 1;
                }

                var current = positions[i].Position;
                var next = positions[nextIndex].Position;
                var prev = positions[prevIndex].Position;

                var up = Vector3.Up;
                vertices.Add(new OrbitVertex()
                {
                    Position = current,
                    NextPosition = next,
                    PrevPosition = prev,
                    Normal = up,
                    Color = color.ToVector4()
                });
            }

            this.vertices = vertices.ToArray();
        }

        /// <summary>
        /// Indicates if the given position is passed
        /// </summary>
        /// <param name="index">The index of the position</param>
        private bool IsPassed(int index)
        {
            return this.positions[index].TrueAnomaly <= this.PassedTrueAnomaly;
        }

        /// <summary>
        /// Sets the color of the given vertex and writes to the given stream
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="index">The index of the vertex</param>
        /// <param name="color">The color</param>
        /// <param name="nextColor">The next color</param>
        private void SetVertexColorAndWrite(DataStream stream, int index, Vector4 color, Vector4 nextColor)
        {
            var vertex = this.vertices[index];
            vertex.Color = color;
            vertex.NextColor = nextColor;
            this.vertices[index] = vertex;
            stream.Write(this.vertices[index]);
        }

        /// <summary>
        /// Updates the passed positions
        /// </summary>
        private void UpdatePassedPositions(DeviceContext deviceContext)
        {
            deviceContext.MapSubresource(
                this.vertexBuffer,
                MapMode.WriteDiscard,
                SharpDX.Direct3D11.MapFlags.None,
                out var stream);

            var passedBrightness = 0.7f;

            for (int i = 0; i < this.positions.Count; i++)
            {
                var j = i + 1;
                if (j >= this.positions.Count)
                {
                    j = 0;
                }

                var currentColor = this.color.ToVector4();
                if (this.IsPassed(i))
                {
                    currentColor = RenderingHelpers.ModifyBrightness(this.color, passedBrightness).ToVector4();
                }

                var nextColor = this.color.ToVector4();
                if (this.IsPassed(j))
                {
                    nextColor = RenderingHelpers.ModifyBrightness(this.color, passedBrightness).ToVector4();
                }

                //if (i % 2 == 0)
                //{
                //    currentColor = Vector4.Zero;
                //    nextColor = Vector4.Zero;
                //}

                this.SetVertexColorAndWrite(stream, i, currentColor, nextColor);
            }

            deviceContext.UnmapSubresource(this.vertexBuffer, 0);
        }

        /// <summary>
        /// Calculates the width of an orbit line
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="position">The position of the object (in game space)</param>
        public static float OrbitLineWidth(BaseCamera camera, Vector3 position)
        {
            var width = Vector3.Distance(position, camera.Position) / 200.0f;
            //return MathUtil.Clamp(width, 0.01f, 10.0f);
            return MathUtil.Clamp(width, 0.0001f, 10.0f);
        }

        /// <summary>
        /// Updates the orbit
        /// </summary>
        /// <param name="positions">The new positions</param>
        public void Update(IList<Point> positions)
        {
            if (positions.Count == 0)
            {
                return;
            }

            this.positions = positions;
            this.UpdateVertices();

            if (this.vertexBuffer == null)
            {
                this.CreateVertexBuffer();
            }
            else
            {
                this.vertexBuffer.Dispose();
                this.CreateVertexBuffer();
            }

            this.UpdateVertices();
        }

        /// <summary>
        /// Draws the orbit using the given effect
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="currentPosition">The position of the object in the orbit</param>
        public void Draw(DeviceContext deviceContext, OrbitEffect effect, EffectPass pass, BaseCamera camera, Matrix world, Vector3 currentPosition)
        {
            if (this.vertexBuffer == null)
            {
                return;
            }

            this.UpdatePassedPositions(deviceContext);

            //effect.SetLineWidth(this.lineWidth);
            effect.SetLineWidth(OrbitLineWidth(camera, currentPosition));

            //Set draw type
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            deviceContext.Rasterizer.State = this.rasterizerStates.NoCull;
            deviceContext.OutputMerger.SetBlendState(this.blendStates.Transparent, Color.Black, 0xffffffff);

            //Set buffers
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<OrbitVertex>(), 0));

            //Set per object constants
            effect.SetTransform(camera.ViewProjection, world);

            //Draw
            pass.Apply(deviceContext);
            deviceContext.Draw(this.vertices.Length, 0);
            deviceContext.OutputMerger.SetBlendState(null, Color.Black, 0xffffffff);
        }

        public void Dispose()
        {
            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.Dispose();
            }

            this.rasterizerStates.Dispose();
            this.blendStates.Dispose();
        }
    }
}
