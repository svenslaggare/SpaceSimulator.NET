using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents an object used for rendering
    /// </summary>
    public class RenderingObject : IDisposable
    {
        private readonly Color orbitColor;
        private readonly PhysicsObject physicsObject;

        private IList<Orbit.Point> positions;
        private readonly Orbit renderingOrbit;
        private readonly Sphere renderingSphere;

        private readonly Matrix scalingMatrix;

        /// <summary>
        /// Creates a new rendering object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="orbitColor">The color of the orbit</param>
        /// <param name="textureName">The name of the texture for the sphere model</param>
        /// <param name="physicsObject">The physics object to render</param>
        public RenderingObject(Device graphicsDevice, Color orbitColor, string textureName, PhysicsObject physicsObject)
        {
            this.orbitColor = orbitColor;
            this.physicsObject = physicsObject;

            var defaultMaterial = new Material()
            {
                Ambient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            };

            if (physicsObject.Type != PhysicsObjectType.ObjectOfReference)
            {
                this.positions = OrbitPositions.Create(this.physicsObject.ReferenceOrbit, true);
                this.renderingOrbit = new Orbit(graphicsDevice, this.positions, orbitColor, 6 * 0.25f);
            }

            this.renderingSphere = new Sphere(graphicsDevice, 1.0f, textureName, defaultMaterial);

            var nonRealSize = MathConversionsHelpers.ToDraw(Simulator.SolarSystem.Earth.Radius * 0.01);

            this.scalingMatrix = Matrix.Scaling(physicsObject.IsRealSize ? MathConversionsHelpers.ToDraw(this.physicsObject.Radius) : nonRealSize);
            //this.scalingMatrix = Matrix.Scaling(0.01f);
        }

        /// <summary>
        /// Updates which positions that has been passed
        /// </summary>
        private void UpdatePassedPositions()
        {
            var trueAnomaly = OrbitPosition.CalculateOrbitPosition(this.physicsObject).TrueAnomaly;
            this.renderingOrbit.PassedTrueAnomaly = trueAnomaly;
        }

        /// <summary>
        /// The transform of the primary body
        /// </summary>
        private Matrix PrimaryBodyTransform()
        {
            if (this.physicsObject.PrimaryBody == null)
            {
                return Matrix.Identity;
            }

            return Matrix.Translation(MathConversionsHelpers.ToDrawPosition(this.physicsObject.PrimaryBody.Position));
        }

        /// <summary>
        /// Returns the draw position
        /// </summary>
        private Vector3 DrawPosition
        {
            get { return MathConversionsHelpers.ToDrawPosition(this.physicsObject.Position); }
        }

        /// <summary>
        /// Draws the sphere
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="pass">The effect pass</param>
        private void DrawSphere(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera, EffectPass pass)
        {
            this.renderingSphere.Draw(
                deviceContext,
                planetEffect,
                pass,
                camera,
                this.scalingMatrix * Matrix.RotationY(MathUtil.DegreesToRadians(180.0f)) * Matrix.Translation(this.DrawPosition));
        }

        /// <summary>
        /// Draws the orbit
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="pass">The effect pass</param>
        private void DrawOrbit(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera, EffectPass pass)
        {
            if (this.physicsObject.HasChangedOrbit())
            {
                this.positions = OrbitPositions.Create(this.physicsObject.ReferenceOrbit, true);
                this.renderingOrbit.Update(this.positions);
            }

            this.renderingOrbit.Draw(
                deviceContext,
                orbitEffect,
                pass,
                camera,
                this.PrimaryBodyTransform(),
                this.DrawPosition);
        }

        /// <summary>
        /// Draws the current object.
        /// For maximum performance, consider using the <see cref="Draw(DeviceContext, BasicEffect, OrbitEffect, BaseCamera, IList{RenderingObject})"/> method.
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        public void Draw(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera)
        {
            //Draw planet
            planetEffect.SetEyePosition(camera.Position);

            deviceContext.InputAssembler.InputLayout = planetEffect.InputLayout;
            foreach (var pass in planetEffect.Passes)
            {
                this.DrawSphere(deviceContext, planetEffect, orbitEffect, camera, pass);
            }

            //Draw orbit
            deviceContext.InputAssembler.InputLayout = orbitEffect.InputLayout;
            foreach (var pass in orbitEffect.Passes)
            {
                if (this.renderingOrbit != null)
                {
                    this.UpdatePassedPositions();
                    this.DrawOrbit(deviceContext, planetEffect, orbitEffect, camera, pass);
                }
            }
        }

        /// <summary>
        /// Draws the given rendering objects
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="objects">The objects</param>
        public static void Draw(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera, IList<RenderingObject> objects)
        {
            //Draw planets
            planetEffect.SetEyePosition(camera.Position);

            deviceContext.InputAssembler.InputLayout = planetEffect.InputLayout;
            foreach (var pass in planetEffect.Passes)
            {
                foreach (var currentObject in objects)
                {
                    currentObject.DrawSphere(deviceContext, planetEffect, orbitEffect, camera, pass);
                }
            }

            //Draw orbits
            deviceContext.InputAssembler.InputLayout = orbitEffect.InputLayout;
            foreach (var pass in orbitEffect.Passes)
            {
                foreach (var currentObject in objects)
                {
                    currentObject.UpdatePassedPositions();
                    currentObject.DrawOrbit(deviceContext, planetEffect, orbitEffect, camera, pass);
                }
            }
        }

        public void Dispose()
        {
            if (this.renderingOrbit != null)
            {
                this.renderingOrbit.Dispose();
            }

            this.renderingSphere.Dispose();
        }
    }
}
