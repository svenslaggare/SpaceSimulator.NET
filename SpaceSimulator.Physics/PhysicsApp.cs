using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SpaceSimulator.Camera;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Mathematics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.PhysicsTest
{
    public class PhysicsApp : D3DApp
    {
        private BasicVertex[] vertices;
        private Buffer vertexBuffer;
        private VertexBufferBinding vertexBufferBinding;

        private int[] indices;
        private Buffer indexBuffer;

        private BasicEffect effect;

        private RenderingSolidColorBrush colorBrush;

        private RungeKutta4Integrator integrator = new RungeKutta4Integrator();
        private double totalTime = 0.0;
        private readonly double deltaTime = 1.0 / 60.0;

        private ObjectState state = new ObjectState(0.0, 10.0, CalculateMomentOfIntertia(10.0, 1.0), Vector3d.Zero, Vector3d.Zero);

        public PhysicsApp()
            : base("Physics", new FPSCamera())
        {

        }

        private static double CalculateMomentOfIntertia(double mass, double side)
        {
            return (1.0 / 6.0) * (mass * side * side);
        }

        public override void LoadContent()
        {
            GeometryGenerator.CreateBox(1, 1, 1, out var geometryVertices, out this.indices);
            this.vertices = geometryVertices.Select(vertex => new BasicVertex()
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                TextureCoordinates = vertex.TextureCoordinates
            }).ToArray();

            this.vertexBuffer = Buffer.Create(
                this.GraphicsDevice,
                BindFlags.VertexBuffer,
                this.vertices);

            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<BasicVertex>(), 0);

            this.indexBuffer = Buffer.Create(
                this.GraphicsDevice,
                BindFlags.IndexBuffer,
                this.indices);

            this.effect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "Light0", BasicVertex.CreateInput());

            this.colorBrush = this.RenderingManager2D.CreateSolidColorBrush(Color.Yellow);

            base.LoadContent();
        }

        public override void Update(TimeSpan elapsed)
        {
            base.Update(elapsed);

            this.integrator.Solve(ref this.state, this.totalTime, this.deltaTime, (ref IntegratorState integratorState, ref ObjectState state) =>
            {
                var acceleration = Vector3d.Zero;
                var torque = Vector3d.Zero;

                if (state.Position.Y > 0.0)
                {
                    acceleration += 9.81 * Vector3d.Down;
                }

                if (this.KeyboardManager.IsKeyDown(SharpDX.DirectInput.Key.Space))
                {
                    acceleration += 10.0 * Vector3d.Up;
                    var applyPoint = state.Position + new Vector3d(0.1, 0.5, 0.0);

                    //var applyPoint = state.Position + new Vector3d(0, 1.0, 0.5);
                    //acceleration = (applyPoint - state.Position).Normalized() * 10;

                    var relativeApplyPoint = applyPoint - state.Position;
                    torque = Vector3d.Cross(acceleration * state.Mass, relativeApplyPoint);

                    //Console.WriteLine(acceleration);
                    //Console.WriteLine(relativeApplyPoint);
                    //Console.WriteLine(torque);
                    //Console.WriteLine("");

                    //torque = new Vector3d(0.5, 1, 0.37);
                }

                return new AccelerationState(acceleration, torque, 0.0);
            });

            if (this.state.Position.Y < 0)
            {
                var position = this.state.Position;
                var velocity = this.state.Velocity;

                position.Y = 0;
                velocity.Y = 0;

                this.state.Position = position;
                this.state.Velocity = velocity;
            }

            //if (!this.state.Velocity.IsZero)
            //{
            //    var orientation = this.state.Orientation * Quaterniond.RotationAxis(Vector3d.ForwardLH, MathUtild.Deg2Rad * 5.0f);
            //    orientation.Normalize();
            //    this.state.Orientation = orientation;
            //}

            this.totalTime += this.deltaTime;
        }

        public override void Draw(TimeSpan elapsed)
        {
            //Clear views
            this.DeviceContext.ClearDepthStencilView(this.BackBufferDepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            this.DeviceContext.ClearRenderTargetView(this.BackBufferRenderView, Color.CornflowerBlue);

            //Draw
            var camera = this.ActiveCamera;
            var color = Color.Gray;
            var world =
                Matrix.RotationQuaternion(MathHelpers.ToFloat(this.state.Orientation)) 
                * Matrix.Translation(MathHelpers.ToFloat(this.state.Position));

            //Set input assembler
            this.DeviceContext.InputAssembler.InputLayout = effect.InputLayout;
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            effect.SetMaterial(new Material()
            {
                Ambient = color.ToVector4() * 0.25f,
                Diffuse = color.ToVector4(),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            });

            effect.SetEyePosition(camera.Position);
            effect.SetTransform(camera.ViewProjection, world);
            //effect.SetPointLightSource(Vector3.Zero);

            //Draw
            foreach (var pass in effect.Passes)
            {
                pass.Apply(this.DeviceContext);
                this.DeviceContext.DrawIndexed(this.indices.Length, 0, 0);
            }

            this.DeviceContext2D.BeginDraw();

            this.colorBrush.DrawText(
                this.DeviceContext2D,
                $"Position: {this.state.Position}, " +
                $"Velocity: {this.state.Velocity}, " +
                $"Orientation: {Math.Round(MathUtild.Rad2Deg * this.state.Orientation.Angle, 2)} @ {this.state.Orientation.Axis}, " +
                $"Angular velocity: {Math.Round(MathUtild.Rad2Deg * this.state.AngularVelocity.Length(), 2)} @ {this.state.AngularVelocity.Normalized()}",
                this.RenderingManager2D.DefaultTextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(10, 10)));

            this.DeviceContext2D.EndDraw();

            //Present
            this.SwapChain.Present(1, PresentFlags.None);
        }
    }
}
