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
    /// Represents a spent rocket stage
    /// </summary>
    public sealed class SpentRocketStage : IPhysicsObjectModel
    {
        private readonly Device graphicsDevice;

        private readonly Cylinder mainBody;
        //private readonly RocketEngine engine;
        private readonly RocketEngineCluster engines;

        private readonly DirectionalLight[] directionalLights;

        /// <summary>
        /// Creates a new spent rocket stage
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="rocket">The rocket</param>
        public SpentRocketStage(Device graphicsDevice, Rocket rocket)
        {
            this.graphicsDevice = graphicsDevice;

            this.mainBody = new Cylinder(graphicsDevice, rocket.Radius, rocket.Radius, rocket.MainBodyHeight, true);
            this.engines = rocket.Engines.Clone();

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
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object</param>
        public void Draw(DeviceContext deviceContext, BasicEffect effect, SpaceCamera camera, PhysicsObject physicsObject)
        {
            var scale = camera.ToDraw(3E5);

            //Compute transformations
            var position = camera.ToDrawPosition(physicsObject.Position);
            var state = physicsObject.State;
            state.MakeRelative(physicsObject.PrimaryBody.State);

            var forward = MathHelpers.ToFloat(state.Prograde);
            var facing = MathHelpers.FaceDirection(forward);

            var world =
                Matrix.Scaling(scale)
                * facing
                * Matrix.Translation(camera.ToDrawPosition(physicsObject.Position));

            //Set effect parameters
            effect.SetMaterial(Rocket.DefaultMaterial(Color.Gray));
            effect.SetEyePosition(camera.Position);
            this.directionalLights[0].Direction = (camera.ToDrawPosition(Vector3d.Zero) - camera.Position).Normalized();
            effect.SetDirectionalLights(this.directionalLights);
            deviceContext.InputAssembler.InputLayout = effect.InputLayout;

            //Draw
            this.mainBody.Draw(
                deviceContext,
                effect,
                camera,
                world);

            this.engines.Draw(
                deviceContext,
                effect,
                camera,
                scale,
                forward,
                position,
                forward);
        }

        public void Dispose()
        {
            this.mainBody.Dispose();
        }
    }
}
