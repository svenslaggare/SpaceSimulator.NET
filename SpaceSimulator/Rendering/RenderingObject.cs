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
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents an object used for rendering
    /// </summary>
    public sealed class RenderingObject : IDisposable
    {
        private readonly Color orbitColor;
        private readonly PhysicsObject physicsObject;

        private IList<Orbit.Point> positions;
        private readonly Orbit renderingOrbit;
        private readonly Sphere renderingSphere;

        private readonly Rendering.Orbit nextManeuverRenderingOrbit;
        private DateTime nextManeuverRenderingOrbitUpdated;
        private readonly TimeSpan nextManeuverRenderingOrbitUpdateFrequency = TimeSpan.FromSeconds(1.0);
        private bool drawNextManeuver = false;

        private readonly Matrix scalingMatrix;

        private readonly TimeSpan orbitUpdateTime = TimeSpan.FromMilliseconds(50.0);
        private DateTime lastOrbitUpdate = new DateTime();
        private bool updateOrbit = false;

        private readonly float baseRotationY;

        /// <summary>
        /// Creates a new rendering object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="orbitColor">The color of the orbit</param>
        /// <param name="textureName">The name of the texture for the sphere model</param>
        /// <param name="physicsObject">The physics object to render</param>
        /// <param name="baseRotationY">The base rotation in the Y-axis</param>
        public RenderingObject(Device graphicsDevice, Color orbitColor, string textureName, PhysicsObject physicsObject, float baseRotationY = 0.0f)
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
                this.CalculateOrbitPositions();
                this.renderingOrbit = new Orbit(graphicsDevice, this.positions, orbitColor, 6 * 0.25f);
            }

            this.renderingSphere = new Sphere(graphicsDevice, 1.0f, textureName, defaultMaterial);

            this.nextManeuverRenderingOrbit = new Rendering.Orbit(graphicsDevice, new List<Rendering.Orbit.Point>(), new Color(124, 117, 6), 3.0f);

            var size = 0.0f;

            if (physicsObject is NaturalSatelliteObject naturalObject)
            {
                size = MathHelpers.ToDraw(naturalObject.Radius);
            }
            else
            {
                size = MathHelpers.ToDraw(Simulator.SolarSystem.Earth.Radius * 0.01);
            }

            this.scalingMatrix = Matrix.Scaling(size);
            this.baseRotationY = baseRotationY;
        }
        
        /// <summary>
        /// Calculates the orbit positions
        /// </summary>
        private void CalculateOrbitPositions()
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.physicsObject);
            this.positions = OrbitPositions.Create(orbitPosition.Orbit, true, trueAnomaly: orbitPosition.TrueAnomaly);
        }
        
        /// <summary>
        /// Updates which positions that has been passed
        /// </summary>
        private void UpdatePassedPositions()
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.physicsObject);
            this.renderingOrbit.PassedTrueAnomaly = orbitPosition.TrueAnomaly;
            this.renderingOrbit.IsBound = orbitPosition.Orbit.IsBound;
        }

        /// <summary>
        /// The transform of the primary body
        /// </summary>
        private Matrix PrimaryBodyTransform()
        {
            var primaryBody = this.physicsObject.PrimaryBody;
            if (primaryBody == null)
            {
                return Matrix.Identity;
            }

            var transform = Matrix.Identity;

            if (this.physicsObject.HasImpacted)
            {
                transform *= Matrix.RotationY(-(float)primaryBody.Rotation);
            }

            transform *= Matrix.Translation(MathHelpers.ToDrawPosition(primaryBody.Position));
            return transform;
        }

        /// <summary>
        /// Returns the draw position
        /// </summary>
        private Vector3 DrawPosition
        {
            get { return MathHelpers.ToDrawPosition(this.physicsObject.Position); }
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
                this.scalingMatrix
                * Matrix.RotationY(this.baseRotationY - (float)this.physicsObject.Rotation)
                * Matrix.Translation(this.DrawPosition));
        }

        /// <summary>
        /// Draws the orbit
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="pass">The effect pass</param>
        private void DrawOrbit(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera, EffectPass pass)
        {
            if (!this.physicsObject.HasImpacted)
            {
                this.renderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    camera,
                    this.PrimaryBodyTransform(),
                    this.DrawPosition);
            }
        }

        /// <summary>
        /// Draws the next maneuver
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="pass">The effect pass</param>
        private void DrawNextManeuver(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect, BaseCamera camera, EffectPass pass)
        {
            if (this.drawNextManeuver)
            {
                this.nextManeuverRenderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    camera,
                    this.PrimaryBodyTransform(),
                    this.DrawPosition);
            }
        }

        /// <summary>
        /// Returns the next maneuver
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        private SimulationManeuever NextManeuver(SimulatorEngine simulatorEngine)
        {
            return simulatorEngine.Maneuvers.FirstOrDefault(x => x.Object == this.physicsObject);
        }

        /// <summary>
        /// Updates the object
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        public void Update(SimulatorEngine simulatorEngine)
        {
            //Orbit
            if (this.physicsObject.HasChangedOrbit())
            {
                this.updateOrbit = true;
            }

            if (this.updateOrbit)
            {
                if (DateTime.UtcNow - this.lastOrbitUpdate >= this.orbitUpdateTime)
                {
                    this.CalculateOrbitPositions();
                    this.renderingOrbit.Update(this.positions);
                    this.updateOrbit = false;
                    this.lastOrbitUpdate = DateTime.UtcNow;
                }
            }

            //Next maneuver
            var nextManeuver = this.NextManeuver(simulatorEngine);
            if (nextManeuver != null)
            {
                var duration = DateTime.UtcNow - this.nextManeuverRenderingOrbitUpdated;
                if (duration >= this.nextManeuverRenderingOrbitUpdateFrequency)
                {
                    var currentOrbit = Physics.Orbit.CalculateOrbit(nextManeuver.Object);

                    var currentState = new ObjectState();
                    var currentPrimaryBodyState = new ObjectState();

                    SolverHelpers.AfterTime(
                        simulatorEngine.KeplerProblemSolver,
                        nextManeuver.Object.Config,
                        nextManeuver.Object.State,
                        currentOrbit,
                        nextManeuver.Maneuver.ManeuverTime - simulatorEngine.TotalTime,
                        out currentState,
                        out currentPrimaryBodyState);

                    currentState.Velocity += nextManeuver.Maneuver.DeltaVelocity;
                    var nextOrbit = Physics.Orbit.CalculateOrbit(nextManeuver.Object.PrimaryBody, ref currentPrimaryBodyState, ref currentState);

                    var positions = OrbitPositions.Create(nextOrbit, true);
                    this.nextManeuverRenderingOrbit.Update(positions);

                    this.nextManeuverRenderingOrbitUpdated = DateTime.UtcNow;
                }

                this.drawNextManeuver = true;
            }
            else
            {
                this.drawNextManeuver = false;
            }
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
                    currentObject.DrawNextManeuver(deviceContext, planetEffect, orbitEffect, camera, pass);
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
