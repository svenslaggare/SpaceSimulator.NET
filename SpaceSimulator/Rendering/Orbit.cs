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
using SpaceSimulator.Camera;
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
    public sealed class Orbit : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly Color color;
        private IList<Point> positions;

        private OrbitVertex[] vertices;   
        private Buffer vertexBuffer;
        private VertexBufferBinding vertexBufferBinding;

        /// <summary>
        /// Indicates if the positions are relative to the focus
        /// </summary>
        public bool DrawRelativeToFocus { get; }

        private RasterizerStates rasterizerStates;
        private BlendStates blendStates;

        /// <summary>
        /// The true anomaly of the last passed position
        /// </summary>
        public double PassedTrueAnomaly { get; set; }

        /// <summary>
        /// Indicates if the brightness is changed for passed positions
        /// </summary>
        public bool ChangeBrightnessForPassed { get; set; } = true;

        /// <summary>
        /// Indicates if the orbit is bound
        /// </summary>
        public bool IsBound { get; set; } = true;

        /// <summary>
        /// Indicates if the orbit is rotated to face the camera
        /// </summary>
        public bool RotateToFaceCamera { get; set; } = true;

        /// <summary>
        /// The material
        /// </summary>
        public Material? Material { get; set; } = null;

        /// <summary>
        /// Represents a point in the orbit
        /// </summary>
        public struct Point
        {
            /// <summary>
            /// The position
            /// </summary>
            public Vector3d Position { get; }

            /// <summary>
            /// The true anomaly
            /// </summary>
            public double TrueAnomaly { get; }

            public Point(Vector3d position, double trueAnomaly)
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
        /// <param name="drawRelativeToFocus">Indicates if the positions are relative to the focus</param>
        public Orbit(Device graphicsDevice, IList<Point> positions, Color color, bool drawRelativeToFocus)
        {
            this.graphicsDevice = graphicsDevice;

            this.Update(positions);
            this.color = color;
            this.DrawRelativeToFocus = drawRelativeToFocus;

            this.rasterizerStates = new RasterizerStates(graphicsDevice);
            this.blendStates = new BlendStates(graphicsDevice);
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
        /// Updates the given vertex and writes to the given stream
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="index">The index of the vertex</param>
        /// <param name="newVertex">The new vertex</param>
        private void SetVerteAndWrite(DataStream stream, int index, OrbitVertex newVertex)
        {
            this.vertices[index] = newVertex;
            stream.Write(this.vertices[index]);
        }

        /// <summary>
        /// Updates the vertices
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="camera">The camera</param>
        private void UpdateVertices(DeviceContext deviceContext, SpaceCamera camera)
        {
            deviceContext.MapSubresource(
                this.vertexBuffer,
                MapMode.WriteDiscard,
                SharpDX.Direct3D11.MapFlags.None,
                out var stream);

            var passedBrightness = 0.7f;

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

                var currentColor = this.color.ToVector4();
                if (this.IsPassed(i) && this.ChangeBrightnessForPassed)
                {
                    currentColor = RenderingHelpers.ModifyBrightness(this.color, passedBrightness).ToVector4();
                }

                var nextColor = this.color.ToVector4();
                if (this.IsPassed(nextIndex) && this.ChangeBrightnessForPassed)
                {
                    nextColor = RenderingHelpers.ModifyBrightness(this.color, passedBrightness).ToVector4();
                }

                var vertex = this.vertices[i];
                var current = this.positions[i].Position;
                var prev = this.positions[prevIndex].Position;
                var next = this.positions[nextIndex].Position;

                var up = Vector3d.Cross(next - current, current);
                up.Normalize();

                vertex.Position = camera.ToDrawPosition(current, this.DrawRelativeToFocus);
                vertex.NextPosition = camera.ToDrawPosition(next, this.DrawRelativeToFocus);
                vertex.PrevPosition = camera.ToDrawPosition(prev, this.DrawRelativeToFocus);

                if (this.RotateToFaceCamera)
                {
                    vertex.Normal = camera.Forward;
                }
                else
                {
                    vertex.Normal = Vector3.Cross(vertex.NextPosition - vertex.Position, vertex.Position);
                    vertex.Normal.Normalize();
                }

                vertex.Color = currentColor;
                vertex.NextColor = nextColor;
                this.SetVerteAndWrite(stream, i, vertex);
            }

            deviceContext.UnmapSubresource(this.vertexBuffer, 0);
        }

        /// <summary>
        /// Calculates the width of an orbit line segment
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="position">The position of the object (in game space)</param>
        public static float OrbitLineWidth(SpaceCamera camera, Vector3 position)
        {
            //var width = Vector3.Distance(position, camera.Position) / 200.0f;
            //return MathUtil.Clamp(width, 0.0001f, 100.0f) / 1.5f;
            var width = 0.005f * Vector3.Distance(position, camera.Position);
            return width;
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

            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<OrbitVertex>(), 0);
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
            var beforeVerticiesCount = this.vertices != null ? this.vertices.Length : 0;
            this.vertices = new OrbitVertex[this.positions.Count];

            if (this.vertexBuffer == null)
            {
                this.CreateVertexBuffer();
            }
            else if (this.vertices.Length != beforeVerticiesCount)
            {
                this.vertexBuffer.Dispose();
                this.CreateVertexBuffer();
            }
        }

        /// <summary>
        /// Draws the orbit using the given effect
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="objectPosition">The position of the object in the orbit</param>
        /// <param name="lineWidth">The width of the orbit line</param>
        public void Draw(DeviceContext deviceContext, OrbitEffect effect, EffectPass pass, SpaceCamera camera, Matrix world, Vector3 objectPosition, float? lineWidth = null)
        {
            if (this.vertexBuffer == null)
            {
                return;
            }

            this.UpdateVertices(deviceContext, camera);

            //Set draw type
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            deviceContext.Rasterizer.State = this.rasterizerStates.NoCull;
            deviceContext.OutputMerger.SetBlendState(this.blendStates.Transparent, Color.Black, 0xffffffff);

            //Set buffers
            deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);

            //Set per object constants
            effect.SetTransform(camera.ViewProjection, world);
            effect.SetLineWidth((lineWidth ?? OrbitLineWidth(camera, objectPosition)));
            if (this.Material != null)
            {
                effect.SetMaterial(this.Material.Value);
            }

            //Draw
            pass.Apply(deviceContext);
            deviceContext.Draw(this.IsBound ? this.vertices.Length : (this.vertices.Length - 1), 0);
            deviceContext.OutputMerger.SetBlendState(null, Color.Black, 0xffffffff);
        }

        public void Dispose()
        {
            this.vertexBuffer?.Dispose();
            this.rasterizerStates.Dispose();
            this.blendStates.Dispose();
        }
    }
}
