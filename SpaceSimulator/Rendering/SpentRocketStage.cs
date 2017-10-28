using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents a spent rocket stage
    /// </summary>
    public sealed class SpentRocketStage : IPhysicsObjectModel
    {
        private readonly Device graphicsDevice;
        private readonly RocketStage stage;
        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new spent rocket stage
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="stage">The rocket stage</param>
        public SpentRocketStage(Device graphicsDevice, RocketStage stage)
        {
            this.graphicsDevice = graphicsDevice;
            this.stage = stage;

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
        /// Draws the given object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="arrowEffect">The arrow effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, BasicEffect arrowEffect, SpaceCamera camera, PhysicsObject physicsObject)
        {
            if (physicsObject is RocketObject rocketObject)
            {
                var scale = camera.ToDraw(3E5);

                //Compute transformations
                (var position, var forward, var facing, var world) = Rocket.ComputeTransformations(camera, rocketObject, scale);

                //Set effect parameters
                Rocket.SetEffectParameters(deviceContext, effect, camera, this.directionalLights);

                //Draw
                this.stage.Draw(
                    deviceContext,
                    effect,
                    camera,
                    scale,
                    forward,
                    position,
                    world,
                    forward);
            }
        }

        public void Dispose()
        {
            this.stage.Dispose();
        }
    }
}
