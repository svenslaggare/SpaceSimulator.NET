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

        private readonly float mainBodyHeight;
        private readonly float nozzleHeight;
        private readonly float nozzleRadius;

        private readonly Cylinder engineMount;
        private readonly Cylinder engineNozzle;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="mainBodyHeight">The height of the main body</param>
        /// <param name="nozzleHeight">The height of the nozzle</param>
        /// <param name="nozzleRadius">The radius of the nozzle</param>
        public RocketEngine(Device graphicsDevice, float mainBodyHeight, float nozzleHeight, float nozzleRadius)
        {
            this.graphicsDevice = graphicsDevice;

            this.mainBodyHeight = mainBodyHeight;
            this.nozzleHeight = nozzleHeight;
            this.nozzleRadius = nozzleRadius;

            var nozzleMinRadius = this.nozzleRadius * 0.3f;
            this.engineMount = new Cylinder(graphicsDevice, nozzleMinRadius, nozzleMinRadius, this.nozzleHeight * 0.5f, true);
            this.engineNozzle = new Cylinder(graphicsDevice, this.nozzleRadius, nozzleMinRadius, this.nozzleHeight, true);
        }

        /// <summary>
        /// Clones the current rocket engine
        /// </summary>
        public RocketEngine Clone()
        {
            return new RocketEngine(this.graphicsDevice, this.mainBodyHeight, this.nozzleHeight, this.nozzleRadius);
        }

        /// <summary>
        /// Returns the transform for the engine
        /// </summary>
        /// <param name="rocketScale">The scale of the rocket</param>
        /// <param name="rocketForward">The forward direction of the rocket</param>
        /// <param name="rocketPosition">The position of the rocket</param>
        /// <param name="thrustDirection">The thurst direction</param>
        /// <param name="offset">The offset of the engine</param>
        public Matrix EngineTransform(float rocketScale, Vector3 rocketForward, Vector3 rocketPosition, Vector3 thrustDirection, Vector2 offset = new Vector2())
        {
            return 
                Matrix.Translation(Vector3.Up * offset.X + Vector3.Right * offset.Y)
                * MathHelpers.FaceDirection(thrustDirection.IsZero ? rocketForward : thrustDirection)
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
            return Matrix.Translation(Vector3.ForwardLH * 0.5f * this.nozzleHeight) * baseEngineTransform;
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
            var engineStartPosition = Vector3.TransformCoordinate(0.1f * Vector3.ForwardLH, this.NozzleTransform(baseEngineTranform));
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
            var engineMountTransform = baseEngineTransform;
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
    /// Represents a cluster of rocket engines of the same type
    /// </summary>
    public sealed class RocketEngineCluster : IDisposable
    {
        /// <summary>
        /// The base engine
        /// </summary>
        public RocketEngine Engine { get; }

        private readonly IList<Vector2> positions;

        /// <summary>
        /// Creates a new cluster of rocket engines
        /// </summary>
        /// <param name="engine">The engine</param>
        /// <param name="positions">The positions of the engines. If null, then one at the center</param>
        public RocketEngineCluster(RocketEngine engine, IList<Vector2> positions = null)
        {
            this.Engine = engine;
            this.positions = positions != null ? new List<Vector2>(positions) : new List<Vector2>() { Vector2.Zero };
        }

        /// <summary>
        /// Clones the current engine cluster
        /// </summary>
        public RocketEngineCluster Clone()
        {
            return new RocketEngineCluster(
                this.Engine.Clone(), 
                new List<Vector2>(this.positions));
        }

        /// <summary>
        /// Draw the given engine cluster
        /// </summary>
        /// <param name="deviceContext">The device cluster</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="scale">The scale of the rocket</param>
        /// <param name="forward">The forward direction of the rocket</param>
        /// <param name="position">The position of the rocket</param>
        /// <param name="thrustDirection">The thurst direction of the rocket</param>
        public void Draw(
            DeviceContext deviceContext,
            BasicEffect effect,
            SpaceCamera camera, 
            float scale, 
            Vector3 forward, 
            Vector3 position,
            Vector3 thrustDirection)
        {
            foreach (var enginePosition in this.positions)
            {
                var engineTransform = this.Engine.EngineTransform(
                    scale,
                    forward,
                    position,
                    thrustDirection,
                    offset: enginePosition);

                this.Engine.Draw(deviceContext, effect, camera, engineTransform);
            }
        }

        /// <summary>
        /// Draws a center thrust arrow instead of seperate
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="arrow">The arrow</param>
        /// <param name="arrowScale">The scale of the arrow</param>
        /// <param name="rocketScale">The scale of the rocket</param>
        /// <param name="rocketForward">The forward direction of the rocket</param>
        /// <param name="rocketPosition">The position of the rocket</param>
        /// <param name="thrustDirection">The direction of the thurst</param>
        public void DrawCenterThrustArrow(
            DeviceContext deviceContext,
            BasicEffect effect,
            SpaceCamera camera,
            Arrow arrow,
            float arrowScale,
            float rocketScale,
            Vector3 rocketForward,
            Vector3 rocketPosition,
            Vector3 thrustDirection)
        {
            var engineTransform = this.Engine.EngineTransform(
                rocketScale,
                rocketForward,
                rocketPosition,
                thrustDirection);

            this.Engine.DrawThrustArrow(
                deviceContext,
                effect,
                camera,
                arrow,
                arrowScale,
                engineTransform,
                thrustDirection);
        }

        /// <summary>
        /// Draws the thurst arrows for the engiens
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="arrow">The arrow</param>
        /// <param name="arrowScale">The scale of the arrow</param>
        /// <param name="rocketScale">The scale of the rocket</param>
        /// <param name="rocketForward">The forward direction of the rocket</param>
        /// <param name="rocketPosition">The position of the rocket</param>
        /// <param name="thrustDirection">The direction of the thurst</param>
        public void DrawThrustArrows(
            DeviceContext deviceContext,
            BasicEffect effect,
            SpaceCamera camera,
            Arrow arrow,
            float arrowScale,
            float rocketScale,
            Vector3 rocketForward,
            Vector3 rocketPosition,
            Vector3 thrustDirection)
        {
            foreach (var enginePosition in this.positions)
            {
                var engineTransform = this.Engine.EngineTransform(
                    rocketScale,
                    rocketForward,
                    rocketPosition,
                    thrustDirection,
                    offset: enginePosition);

                this.Engine.DrawThrustArrow(
                    deviceContext,
                    effect,
                    camera,
                    arrow,
                    arrowScale,
                    engineTransform,
                    thrustDirection);
            }
        }

        public void Dispose()
        {
            this.Engine.Dispose();
        }
    }

    /// <summary>
    /// Represents a rocket stage
    /// </summary>
    public sealed class RocketStage : IDisposable
    {
        private readonly Device graphicsDevice;
        
        /// <summary>
        /// The radius of the rocket
        /// </summary>
        public float MainBodyRadius { get; }

        /// <summary>
        /// The radius of the main body
        /// </summary>
        public float MainBodyHeight { get; }

        private readonly Cylinder mainBody;

        /// <summary>
        /// The rocket engines
        /// </summary>
        public RocketEngineCluster Engines { get; }

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius of the rocket</param>
        /// <param name="mainBodyHeight">The height of the main body</param>
        /// <param name="engines">The engines</param>
        public RocketStage(Device graphicsDevice, float radius, float mainBodyHeight, RocketEngineCluster engines)
        {
            this.graphicsDevice = graphicsDevice;
            this.MainBodyRadius = radius;
            this.MainBodyHeight = mainBodyHeight;
            this.mainBody = new Cylinder(graphicsDevice, radius, radius, mainBodyHeight, true);
            this.Engines = engines;
        }

        /// <summary>
        /// Clones the current stage
        /// </summary>
        public RocketStage Clone()
        {
            return new RocketStage(
                this.graphicsDevice,
                this.MainBodyRadius,
                this.MainBodyHeight,
                this.Engines.Clone());
        }

        /// <summary>
        /// Draw the stage
        /// </summary>
        /// <param name="deviceContext">The device cluster</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="scale">The scale of the rocket</param>
        /// <param name="forward">The forward direction of the rocket</param>
        /// <param name="position">The position of the rocket</param>
        /// <param name="world">The world matrix</param>
        /// <param name="thrustDirection">The thurst direction of the rocket</param>
        public void Draw(
            DeviceContext deviceContext,
            BasicEffect effect,
            SpaceCamera camera,
            float scale,
            Vector3 forward,
            Vector3 position,
            Matrix world,
            Vector3 thrustDirection)
        {
            this.mainBody.Draw(
                deviceContext,
                effect,
                camera,
                world);

            this.Engines.Draw(
                deviceContext,
                effect,
                camera,
                scale,
                forward,
                position,
                thrustDirection);
        }

        public void Dispose()
        {
            this.mainBody.Dispose();
            this.Engines.Dispose();
        }
    }

    /// <summary>
    /// Represents a payload
    /// </summary>
    public abstract class Payload : IDisposable
    {
        /// <summary>
        /// Draws the payload
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="offset">The offset within the rocket</param>
        public abstract void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, Matrix world, Matrix offset);

        public abstract void Dispose();
    }

    /// <summary>
    /// Represents a nose cone
    /// </summary>
    public sealed class NoseCone : Payload
    {
        private readonly Cylinder model;
        private readonly float radius;
        private readonly float height;

        /// <summary>
        /// Creates a new nose cone
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="radius">The radius</param>
        /// <param name="height">The height</param>
        public NoseCone(Device graphicsDevice, float radius, float height)
        {
            this.radius = radius;
            this.height = height;
            this.model = new Cylinder(graphicsDevice, radius, 0, height, true);
        }

        public override void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, Matrix world, Matrix offset)
        {
            this.model.Draw(
               deviceContext,
               effect,
               camera,
               offset * Matrix.Translation(Vector3.BackwardLH * 0.5f * this.height) * world);
        }

        public override void Dispose()
        {
            this.model.Dispose();
        }
    }

    /// <summary>
    /// Draws a rocket
    /// </summary>
    public sealed class Rocket : IDisposable, IPhysicsObjectModel
    {
        private readonly Device graphicsDevice;

        private readonly IList<RocketStage> stages;
        private readonly Payload payload;

        private readonly Arrow arrow;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="stages">The stages</param>
        /// <param name="payload">The payload</param>
        public Rocket(Device graphicsDevice, IList<RocketStage> stages, Payload payload)
        {
            this.graphicsDevice = graphicsDevice;

            this.stages = new List<RocketStage>(stages);
            this.payload = payload;

            this.arrow = new Arrow(graphicsDevice, 0.2f, 10.0f, 2.0f);

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
        /// Creates a new Falcon 9 rocket
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        public static Rocket CreateFalcon9(Device graphicsDevice)
        {
            var mainBodyRadius = 0.1f;
            var noseConeHeight = 0.2f;
            var firstStageHeight = 1.0f;
            var secondStageHeight = 0.25f;

            var firstStageSdeEngines = 8;
            var firstStageEnginePositions = new List<Vector2>
            {
                Vector2.Zero
            };

            for (int i = 0; i < firstStageSdeEngines; i++)
            {
                var angle = ((float)i / firstStageSdeEngines) * MathUtil.TwoPi;
                firstStageEnginePositions.Add(0.75f * mainBodyRadius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
            }

            var payload = new NoseCone(
                graphicsDevice,
                mainBodyRadius,
                noseConeHeight);

            var firstStage = new RocketStage(
                graphicsDevice,
                mainBodyRadius,
                firstStageHeight,
                new RocketEngineCluster(
                    new RocketEngine(graphicsDevice, firstStageHeight, 0.1f * 0.5f, 0.075f * 0.35f),
                    firstStageEnginePositions));

            var secondStage = new RocketStage(
                graphicsDevice,
                mainBodyRadius,
                secondStageHeight,
                new RocketEngineCluster(new RocketEngine(graphicsDevice, secondStageHeight, 0.1f * 0.75f, 0.075f * 0.75f)));

            return new Rocket(
                graphicsDevice,
                new List<RocketStage>() { firstStage, secondStage },
                payload);
        }

        /// <summary>
        /// Returns the current stage
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="stageNumber">The stage number</param>
        private RocketStage CurrentStage(RocketObject rocketObject, int? stageNumber = null)
        {
            if (stageNumber == null)
            {
                stageNumber = rocketObject.Stages.CurrentStage.Number;
            }

            if (stageNumber < this.stages.Count)
            {
                return this.stages[stageNumber.Value];
            }

            return null;
        }

        /// <summary>
        /// Creates a model for a spent stage
        /// </summary>
        /// <param name="rocketObject">The current rocket object</param>
        public IPhysicsObjectModel CreateSpentStage(RocketObject rocketObject)
        {
            return new SpentRocketStage(
                this.graphicsDevice,
                this.CurrentStage(
                    rocketObject,
                    rocketObject.Stages.CurrentStage.Number - 1).Clone());
        }

        /// <summary>
        /// Returns the default material for a rocket part
        /// </summary>
        /// <param name="color">The color</param>
        public static Material DefaultMaterial(Color color) => new Material()
        {
            Ambient = color.ToVector4() * 0.25f,
            Diffuse = color.ToVector4(),
            Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
        };

        /// <summary>
        /// Indicates if the effect is textured
        /// </summary>
        public bool IsTextured => false;

        /// <summary>
        /// Computes transformations and vectors
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="scale">The scale</param>
        public static (Vector3 position, Vector3 forward, Matrix facing, Matrix world) ComputeTransformations(
            SpaceCamera camera, 
            PhysicsObject physicsObject,
            float scale)
        {
            var position = camera.ToDrawPosition(physicsObject.Position);
            var state = physicsObject.State;
            state.MakeRelative(physicsObject.PrimaryBody.State);

            var forward = MathHelpers.ToFloat(state.Prograde);
            var facing = MathHelpers.FaceDirection(forward);

            var world = Matrix.Scaling(scale) * facing * Matrix.Translation(position);

            return (position, forward, facing, world);
        }

        /// <summary>
        /// Sets the effect parameters
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="directionalLights">The directional lights</param>
        public static void SetEffectParameters(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, DirectionalLight[] directionalLights)
        {
            effect.SetMaterial(DefaultMaterial(Color.Gray));

            effect.SetEyePosition(camera.Position);

            directionalLights[0].Direction = (camera.ToDrawPosition(Vector3d.Zero) - camera.Position).Normalized();
            //this.directionalLights[0].Direction = (position - camera.ToDrawPosition(Vector3d.Zero)).Normalized();
            effect.SetDirectionalLights(directionalLights);

            deviceContext.InputAssembler.InputLayout = effect.InputLayout;
        }

        /// <summary>
        /// Draws the given rocket object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="scale">The scale to draw the rocket at</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, RocketObject rocketObject, float scale)
        {
            var arrowScale = camera.ToDraw(1.5E4);

            //Compute transformations
            (var position, var forward, var facing, var world) = ComputeTransformations(camera, rocketObject, scale);
            var thrustDirection = MathHelpers.ToFloat(rocketObject.EngineAcceleration().Normalized());

            var currentStage = this.CurrentStage(rocketObject);

            //Draw thrust arrow
            if (currentStage != null)
            {
                currentStage.Engines.DrawCenterThrustArrow(
                    deviceContext,
                    effect,
                    camera,
                    this.arrow,
                    arrowScale,
                    scale,
                    forward,
                    position,
                    thrustDirection);
            }

            //Draw rocket
            SetEffectParameters(deviceContext, effect, camera, this.directionalLights);

            this.payload.Draw(
                deviceContext,
                effect,
                camera,
                world,
                Matrix.Translation(Vector3.BackwardLH * 0.5f * ((currentStage?.MainBodyHeight ?? 0))));

            if (currentStage != null)
            {
                currentStage.Draw(
                    deviceContext,
                    effect,
                    camera,
                    scale,
                    forward,
                    position,
                    world,
                    thrustDirection);
            }
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
            }
        }

        public void Dispose()
        {
            foreach (var stage in this.stages)
            {
                stage.Dispose();
            }

            this.payload.Dispose();
            this.arrow.Dispose();
        }
    }
}
