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
    /// Draws a rocket engine
    /// </summary>
    public sealed class RocketEngine : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly Matrix baseTransform;

        private readonly float mainBodyHeight;
        private readonly float nozzleHeight;
        private readonly float nozzleRadius;

        private readonly Cylinder engineMount;
        private readonly Cylinder engineNozzle;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="baseTransform">The base transform</param>
        /// <param name="mainBodyHeight">The height of the main body</param>
        /// <param name="nozzleHeight">The height of the nozzle</param>
        /// <param name="nozzleRadius">The radius of the nozzle</param>
        public RocketEngine(Device graphicsDevice, Matrix baseTransform, float mainBodyHeight, float nozzleHeight, float nozzleRadius)
        {
            this.graphicsDevice = graphicsDevice;

            this.baseTransform = baseTransform;

            this.mainBodyHeight = mainBodyHeight;
            this.nozzleHeight = nozzleHeight;
            this.nozzleRadius = nozzleRadius;

            var nozzleMinRadius = this.nozzleRadius * 0.3f;
            this.engineMount = new Cylinder(graphicsDevice, nozzleMinRadius, nozzleMinRadius, this.nozzleHeight * 0.5f);
            this.engineNozzle = new Cylinder(graphicsDevice, this.nozzleRadius, nozzleMinRadius, this.nozzleHeight);
        }

        /// <summary>
        /// Returns the transform for the engine
        /// </summary>
        /// <param name="rocketScale">The scale of the rocket</param>
        /// <param name="rocketForward">The forward direction of the rocket</param>
        /// <param name="rocketPosition">The position of the rocket</param>
        /// <param name="thrustDirection">The thurst direction</param>
        public Matrix EngineTransform(
            float rocketScale,
            Vector3 rocketForward,
            Vector3 rocketPosition,
            Vector3 thrustDirection)
        {
            return 
                MathHelpers.FaceDirection(thrustDirection.IsZero ? rocketForward : thrustDirection)
                * Matrix.Translation(-rocketForward * 0.5f * (this.mainBodyHeight))
                * Matrix.Scaling(rocketScale)
                * Matrix.Translation(rocketPosition);
        }

        /// <summary>
        /// Returns the transform for the nozzle
        /// </summary>
        /// <param name="baseEngineTransform">The base transform of the engine</param>
        private Matrix NozzleTransform(Matrix baseEngineTransform)
        {
            return 
                this.baseTransform
                * Matrix.Translation(Vector3.ForwardLH * 0.5f * this.nozzleHeight)
                * baseEngineTransform;
        }

        /// <summary>
        /// Draws a thrust arrow
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The efect</param>
        /// <param name="camera">The camera</param>
        /// <param name="arrow">The arrow</param>
        /// <param name="arrowScale">The scale to draw the arrow at</param>
        /// <param name="baseEngineTranform">The base engine transform</param>
        /// <param name="thrustDirection">The thrust direction</param>
        public void DrawThrustArrow(
            DeviceContext deviceContext,
            BasicEffect effect,
            SpaceCamera camera,
            Arrow arrow,
            float arrowScale,
            Matrix baseEngineTranform,
            Vector3 thrustDirection)
        {
            var engineStartPosition = Vector3.TransformCoordinate(0.1f * Vector3.Down, this.NozzleTransform(baseEngineTranform));
            var engineTargetPosition = engineStartPosition - thrustDirection * arrowScale * (arrow.BaseHeight + arrow.HeadHeight);

            arrow.DrawDirection(
                deviceContext,
                effect,
                camera,
                arrowScale,
                Matrix.Translation(engineTargetPosition),
                Color.Yellow,
                (engineStartPosition - engineTargetPosition).Normalized());
        }

        /// <summary>
        /// Draws the rocket engine
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The efect</param>
        /// <param name="camera">The camera</param>
        /// <param name="baseEngineTransform">The base engine transform</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, Matrix baseEngineTransform)
        {
            var engineMountTransform = this.baseTransform * baseEngineTransform;
            var engineTransform = this.NozzleTransform(baseEngineTransform);

            this.engineMount.Draw(
                deviceContext,
                effect,
                camera,
                engineMountTransform);

            this.engineNozzle.Draw(
                deviceContext,
                effect,
                camera,
                engineTransform);
        }

        public void Dispose()
        {
            this.engineMount.Dispose();
            this.engineNozzle.Dispose();
        }
    }

    /// <summary>
    /// Draws a rocket
    /// </summary>
    public sealed class Rocket : IDisposable, IPhysicsObjectModel
    {
        private readonly Device graphicsDevice;

        private readonly Matrix baseTransform = Matrix.RotationAxis(Vector3.Right, -MathHelpers.Deg2Rad * 90);

        private readonly float radius;
        private readonly float noseConeHeight;
        private readonly float mainBodyHeight;
        private readonly float nozzleHeight;
        private readonly float nozzleRadius;

        private readonly Cylinder noseCone;
        private readonly Cylinder mainBody;
        private readonly RocketEngine engine;

        private readonly Arrow arrow;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the rocket</param>
        /// <param name="noseConeHeight">The height of the nose cone</param>
        /// <param name="mainBodyHeight">The height of the main body</param>
        /// <param name="nozzleHeight">The height of the nozzle</param>
        /// <param name="nozzleRadius">The radius of the nozzle</param>
        public Rocket(Device graphicsDevice, float radius, float noseConeHeight, float mainBodyHeight, float nozzleHeight, float nozzleRadius)
        {
            this.graphicsDevice = graphicsDevice;

            this.radius = radius;
            this.noseConeHeight = noseConeHeight;
            this.mainBodyHeight = mainBodyHeight;
            this.nozzleHeight = nozzleHeight;
            this.nozzleRadius = nozzleRadius;

            this.noseCone = new Cylinder(graphicsDevice, this.radius, 0, this.noseConeHeight);
            this.mainBody = new Cylinder(graphicsDevice, this.radius, this.radius, this.mainBodyHeight);

            var nozzleMinRadius = this.nozzleRadius * 0.3f;
            this.engine = new RocketEngine(graphicsDevice, baseTransform, this.mainBodyHeight, this.nozzleHeight, this.nozzleRadius);

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
        /// Indicates if the effect is textured
        /// </summary>
        public bool IsTextured => false;

        /// <summary>
        /// Draws the given rocket object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="scale">The scale to draw the rocket at</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, RocketObject rocketObject, float scale = 0.01f)
        {
            var arrowScale = camera.ToDraw(1.5E4);

            //Compute transformations
            var position = camera.ToDrawPosition(rocketObject.Position);
            var state = rocketObject.State;
            state.MakeRelative(rocketObject.PrimaryBody.State);

            var forward = MathHelpers.ToFloat(state.Prograde);
            var facing = MathHelpers.FaceDirection(forward);

            var world =
                Matrix.Scaling(scale)
                * facing
                * Matrix.Translation(camera.ToDrawPosition(rocketObject.Position));

            var thrustDirection = MathHelpers.ToFloat(rocketObject.EngineAcceleration().Normalized());
            var baseEngineTransform = this.engine.EngineTransform(
                scale,
                forward,
                camera.ToDrawPosition(rocketObject.Position),
                thrustDirection);

            //Draw thrust arrow
            this.engine.DrawThrustArrow(
                deviceContext,
                effect,
                camera,
                this.arrow,
                arrowScale,
                baseEngineTransform,
                thrustDirection);

            //Draw rocket
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

            this.noseCone.Draw(
                deviceContext,
                effect,
                camera,
                this.baseTransform
                * Matrix.Translation(-Vector3.ForwardLH * 0.5f * (this.mainBodyHeight + this.noseConeHeight))
                * world);

            this.mainBody.Draw(
                deviceContext,
                effect,
                camera,
                this.baseTransform * world);
     
            this.engine.Draw(deviceContext, effect, camera, baseEngineTransform);
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
            if (physicsObject is RocketObject rocketObject)
            {
                this.Draw(deviceContext, effect, camera, rocketObject, scale: camera.ToDraw(3E5));
                //this.Draw(deviceContext, effect, camera, rocketObject, scale: camera.ToDraw(100));
            }
        }

        public void Dispose()
        {
            this.noseCone.Dispose();
            this.mainBody.Dispose();
            this.engine.Dispose();
            this.arrow.Dispose();
        }
    }
}
