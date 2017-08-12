using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
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

            //this.payload.Draw(
            //    deviceContext,
            //    effect,
            //    camera,
            //    world,
            //    Matrix.Translation(Vector3.BackwardLH * 0.5f * ((currentStage?.MainBodyHeight ?? 0))));

            //if (currentStage != null)
            //{
            //    currentStage.Draw(
            //        deviceContext,
            //        effect,
            //        camera,
            //        scale,
            //        forward,
            //        position,
            //        world,
            //        thrustDirection);
            //}

            var offset = 0.0f;
            int i = 0;
            //foreach (var stage in this.stages)
            foreach (var stage in this.stages.Skip(rocketObject.Stages.CurrentStage.Number))
            {
                var stageWorld =
                    Matrix.Translation(Vector3.BackwardLH * (offset + (offset != 0.0f ? stage.MainBodyHeight * 0.5f : 0.0f)))
                    * Matrix.Scaling(scale)
                    * facing
                    * Matrix.Translation(position);

                //if (i >= rocketObject.Stages.CurrentStage.Number)
                {
                    stage.Draw(
                        deviceContext,
                        effect,
                        camera,
                        scale,
                        forward,
                        position,
                        stageWorld,
                        thrustDirection);
                }

                offset += stage.MainBodyHeight * (offset == 0.0f ? 0.5f : 1.0f);
                i++;
            }

            this.payload.Draw(
                deviceContext,
                effect,
                camera,
                world,
                Matrix.Translation(Vector3.BackwardLH * (offset)));
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
