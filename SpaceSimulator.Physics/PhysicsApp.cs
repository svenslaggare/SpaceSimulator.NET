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
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Atmosphere;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.PhysicsTest
{
    public class PhysicsApp : D3DApp
    {
        private GeometryObject groundPlane;
        private GeometryObject boxObject;
        private GeometryObject forceOrigin;
        private GeometryObject forceTarget;

        private BasicEffect effect;

        private RenderingSolidColorBrush colorBrush;
        private RasterizerStates rasterizerStates;

        private RungeKutta4Integrator integrator = new RungeKutta4Integrator();
        private double totalTime = 0.0;
        private readonly double deltaTime = 1.0 / 60.0;

        private readonly double side = 1.0;
        private ObjectState state;

        private Vector3d forceOriginPosition = new Vector3d(0, 0, -2);
        private Vector3d forceTargetPosition = new Vector3d(0.5, 0.5, -0.5);

        //private Vector3d forceOriginPosition = new Vector3d(-0.5, 0, -2);
        //private Vector3d forceTargetPosition = new Vector3d(-0.5, 0.0, -0.5);

        //private Vector3d forceOriginPosition = new Vector3d(0, -2.5, 0);
        //private Vector3d forceTargetPosition = new Vector3d(0.1, -0.5, 0.0);

        public PhysicsApp()
            : base("Physics", new FPSCamera())
        {
            var mass = 10.0;
            this.state = new ObjectState(0.0, mass, CalculateMomentOfIntertia(mass, side), Vector3d.Zero, Vector3d.Zero);
        }

        private static double CalculateMomentOfIntertia(double mass, double side)
        {
            return (1.0 / 6.0) * (mass * side * side);
        }

        public override void LoadContent()
        {
            this.groundPlane = GeometryObject.Plane(this.GraphicsDevice, 1.0f, 1.0f);
            this.boxObject = GeometryObject.Box(this.GraphicsDevice, 1, 1, 1);
            this.forceOrigin = GeometryObject.Sphere(this.GraphicsDevice, 1);
            this.forceTarget = GeometryObject.Sphere(this.GraphicsDevice, 1);

            this.effect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "Light0", BasicVertex.CreateInput());

            this.colorBrush = this.RenderingManager2D.CreateSolidColorBrush(Color.Yellow);

            this.rasterizerStates = new RasterizerStates(this.GraphicsDevice);
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

                var dragForce = AtmosphericFormulas.Drag(state.Velocity, 1.225, this.side * this.side, 1.0);

                if (this.KeyboardManager.IsKeyDown(SharpDX.DirectInput.Key.Space))
                {
                    //acceleration += 10.0 * Vector3d.Up;
                    //var applyPoint = state.Position + new Vector3d(0.1, 0.5, 0.0);

                    var applyPoint = this.forceTargetPosition;
                    acceleration = (this.forceTargetPosition - this.forceOriginPosition).Normalized() * 10;

                    var relativeApplyPoint = applyPoint - state.Position;
                    torque = -Vector3d.Cross(acceleration * state.Mass, relativeApplyPoint);
                }

                acceleration += dragForce / state.Mass;
                torque += AtmosphericFormulas.AngularDrag(state.AngularVelocity, 1.225, this.side * this.side, 1.0);

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
            this.DeviceContext.Rasterizer.State = this.rasterizerStates.NoCull;

            //Draw
            var world =
                Matrix.RotationQuaternion(MathHelpers.ToFloat(this.state.Orientation)) 
                * Matrix.Translation(MathHelpers.ToFloat(this.state.Position));

            //this.groundPlane.Draw(
            //    this.DeviceContext,
            //    this.effect,
            //    this.ActiveCamera,
            //    Matrix.Scaling(100.0f),
            //    Color.Green);

            this.boxObject.Draw(
                this.DeviceContext,
                this.effect,
                this.ActiveCamera,
                world,
                Color.Gray);

            this.forceOrigin.Draw(
                this.DeviceContext,
                this.effect,
                this.ActiveCamera,
                Matrix.Scaling(0.01f) * Matrix.Translation(MathHelpers.ToFloat(this.forceOriginPosition)),
                Color.Yellow);

            this.forceTarget.Draw(
                this.DeviceContext,
                this.effect,
                this.ActiveCamera,
                Matrix.Scaling(0.01f) * Matrix.Translation(MathHelpers.ToFloat(this.forceTargetPosition)),
                Color.Red);

            this.DeviceContext2D.BeginDraw();

            this.colorBrush.DrawText(
                this.DeviceContext2D,
                $"Position: {DataFormatter.Format(this.state.Position)}, " +
                $"Velocity: {DataFormatter.Format(this.state.Velocity)}, " +
                $"Orientation: {Math.Round(MathUtild.Rad2Deg * this.state.Orientation.Angle, 2)} @ {DataFormatter.Format(this.state.Orientation.Axis)}, " +
                $"Angular velocity: {Math.Round(MathUtild.Rad2Deg * this.state.AngularVelocity.Length(), 2)} @ {DataFormatter.Format(this.state.AngularVelocity.Normalized())}",
                this.RenderingManager2D.DefaultTextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(10, 10)));

            this.DeviceContext2D.EndDraw();

            //Present
            this.SwapChain.Present(1, PresentFlags.None);
        }
    }
}
