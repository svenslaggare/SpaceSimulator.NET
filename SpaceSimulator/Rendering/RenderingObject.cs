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
        private readonly BaseCamera camera;

        /// <summary>
        /// The object being drawn
        /// </summary>
        public PhysicsObject PhysicsObject { get; }

        private readonly Color orbitColor;

        private IList<Orbit.Point> positions;
        private readonly Orbit renderingOrbit;
        private readonly Sphere renderingSphere;

        private readonly Rendering.Orbit nextManeuverRenderingOrbit;
        private DateTime nextManeuverRenderingOrbitUpdated;
        private readonly TimeSpan nextManeuverRenderingOrbitUpdateFrequency = TimeSpan.FromSeconds(1.0);
        private bool drawNextManeuver = false;

        private readonly Rendering.Orbit ringRenderingOrbit;

        private readonly TimeSpan orbitUpdateTime = TimeSpan.FromMilliseconds(50.0);
        private DateTime lastOrbitUpdate = new DateTime();
        private bool updateOrbit = false;

        private readonly float baseRotationY;

        /// <summary>
        /// Indicates if the sphere should be drawn
        /// </summary>
        public bool ShowSphere { get; set; } = true;

        /// <summary>
        /// Creates a new rendering object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object to render</param>
        /// <param name="orbitColor">The color of the orbit</param>
        /// <param name="textureName">The name of the texture for the sphere model</param>
        /// <param name="baseRotationY">The base rotation in the Y-axis</param>
        /// <param name="ringColor">The color of the rings</param>
        /// <param name="ringRadius">The radius of the rings</param>
        public RenderingObject(
            Device graphicsDevice,
            BaseCamera camera,
            PhysicsObject physicsObject,
            Color orbitColor,
            string textureName,
            float baseRotationY = 0.0f,
            Color? ringColor = null,
            double ringRadius = 0.0)
        {
            this.camera = camera;
            this.PhysicsObject = physicsObject;

            this.orbitColor = orbitColor;

            var defaultMaterial = new Material()
            {
                Ambient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            };

            var drawRelativeToFocus = physicsObject.Type != PhysicsObjectType.ArtificialSatellite;
            drawRelativeToFocus = false;
            if (physicsObject.PrimaryBody != null && physicsObject.PrimaryBody.IsObjectOfReference)
            {
                drawRelativeToFocus = true;
            }

            if (!physicsObject.IsObjectOfReference)
            {
                this.CalculateOrbitPositions();
                this.renderingOrbit = new Orbit(
                    graphicsDevice,
                    this.camera,
                    this.positions,
                    orbitColor,
                    drawRelativeToFocus);
            }

            this.renderingSphere = new Sphere(graphicsDevice, 1.0f, textureName, defaultMaterial);

            this.nextManeuverRenderingOrbit = new Rendering.Orbit(
                graphicsDevice,
                this.camera,
                new List<Rendering.Orbit.Point>(),
                new Color(124, 117, 6),
                drawRelativeToFocus);

            this.baseRotationY = baseRotationY;

            if (ringColor.HasValue && physicsObject is NaturalSatelliteObject naturalSatelliteObject)
            {
                var ringPositions = OrbitPositions.Create(
                    Physics.Orbit.New(
                        naturalSatelliteObject,
                        semiMajorAxis: ringRadius),
                    true);

                this.ringRenderingOrbit = new Rendering.Orbit(
                    graphicsDevice,
                    this.camera,
                    ringPositions,
                    ringColor.Value,
                    false)
                {
                    IsBound = true,
                    ChangeBrightnessForPassed = false
                };
            }
        }
        
        /// <summary>
        /// Returns the scaling matrix
        /// </summary>
        private Matrix ScalingMatrix
        {
            get
            {
                var size = 0.0f;
                if (this.PhysicsObject is NaturalSatelliteObject naturalObject)
                {
                    size = this.camera.ToDraw(naturalObject.Radius);
                }
                else
                {
                    size = this.camera.ToDraw(Simulator.SolarSystemBodies.Earth.Radius * 0.01);
                }

                return Matrix.Scaling(size);
            }
        }

        /// <summary>
        /// Calculates the orbit positions
        /// </summary>
        private void CalculateOrbitPositions()
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.PhysicsObject);
            this.positions = OrbitPositions.Create(orbitPosition.Orbit, true, trueAnomaly: orbitPosition.TrueAnomaly);
        }
        
        /// <summary>
        /// Updates which positions that has been passed
        /// </summary>
        private void UpdatePassedPositions()
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.PhysicsObject);
            this.renderingOrbit.PassedTrueAnomaly = orbitPosition.TrueAnomaly;
            this.renderingOrbit.IsBound = orbitPosition.Orbit.IsBound;
        }

        /// <summary>
        /// The transform of the primary body
        /// </summary>
        /// <param name="renderingOrbit">The rendering orbit</param>
        /// <param name="primaryBody">The primary body</param>
        private Matrix PrimaryBodyTransform(Orbit renderingOrbit, NaturalSatelliteObject primaryBody = null)
        {
            primaryBody = primaryBody ?? this.PhysicsObject.PrimaryBody;
            if (primaryBody == null)
            {
                return Matrix.Identity;
            }

            var transform = Matrix.Identity;
            if (this.PhysicsObject.HasImpacted)
            {
                transform *= Matrix.RotationY(-(float)primaryBody.Rotation);
            }

            transform *= Matrix.Translation(this.camera.ToDrawPosition(primaryBody.Position, relativeToFocus: !renderingOrbit.DrawRelativeToFocus));
            return transform;
        }

        /// <summary>
        /// Returns the draw position
        /// </summary>
        public Vector3 DrawPosition
        {
            get { return this.camera.ToDrawPosition(this.PhysicsObject.Position); }
        }

        /// <summary>
        /// Draws the sphere
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="position">The position to draw at.</param>
        public void DrawSphere(DeviceContext deviceContext, BasicEffect planetEffect, EffectPass pass, Vector3? position = null)
        {
            this.renderingSphere.Draw(
                deviceContext,
                planetEffect,
                pass,
                camera,
                this.ScalingMatrix
                * Matrix.RotationY(this.baseRotationY - (float)this.PhysicsObject.Rotation)
                * Matrix.Translation(position ?? this.DrawPosition));
        }

        /// <summary>
        /// Draws the orbit
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        public void DrawOrbit(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass)
        {
            if (!this.PhysicsObject.HasImpacted)
            {
                this.renderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    this.PrimaryBodyTransform(this.renderingOrbit),
                    this.DrawPosition);
            }
        }

        /// <summary>
        /// Draws the next maneuver
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        private void DrawNextManeuver(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass)
        {
            if (this.drawNextManeuver)
            {
                this.nextManeuverRenderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    this.PrimaryBodyTransform(this.nextManeuverRenderingOrbit),
                    this.DrawPosition);
            }
        }

        /// <summary>
        /// Draws the rings
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        private void DrawRings(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass)
        {
            if (this.ringRenderingOrbit != null)
            {
                this.ringRenderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    this.PrimaryBodyTransform(this.ringRenderingOrbit, (NaturalSatelliteObject)this.PhysicsObject),
                    this.DrawPosition,
                    lineWidth: 0.03f);
            }
        }

        /// <summary>
        /// Returns the next maneuver
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        private SimulationManeuever NextManeuver(SimulatorEngine simulatorEngine)
        {
            return simulatorEngine.Maneuvers.FirstOrDefault(x => x.Object == this.PhysicsObject);
        }

        /// <summary>
        /// Updates the object
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        public void Update(SimulatorEngine simulatorEngine)
        {
            //Orbit
            if (this.PhysicsObject.HasChangedOrbit())
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
                        nextManeuver.Object,
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
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="orbitEffect">The orbit effect</param>
        public void Draw(DeviceContext deviceContext, BasicEffect planetEffect, OrbitEffect orbitEffect)
        {
            //Draw planet
            planetEffect.SetEyePosition(camera.Position);
            planetEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

            deviceContext.InputAssembler.InputLayout = planetEffect.InputLayout;
            foreach (var pass in planetEffect.Passes)
            {
                this.DrawSphere(deviceContext, planetEffect, pass);
            }

            //Draw orbit
            deviceContext.InputAssembler.InputLayout = orbitEffect.InputLayout;
            foreach (var pass in orbitEffect.Passes)
            {
                if (this.renderingOrbit != null)
                {
                    this.UpdatePassedPositions();
                    this.DrawOrbit(deviceContext, orbitEffect, pass);
                    this.DrawNextManeuver(deviceContext, orbitEffect, pass);
                    this.DrawRings(deviceContext, orbitEffect, pass);
                }
            }
        }

        /// <summary>
        /// Draws spheres for the given rendering objects
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="ringEffect">The ring effect</param>
        /// <param name="objects">The objects</param>
        public static void DrawSpheres(DeviceContext deviceContext, BasicEffect sunEffect, BasicEffect planetEffect, OrbitEffect ringEffect, IList<RenderingObject> objects)
        {
            var camera = objects[0].camera;

            //Draw the sun
            if (objects[0].ShowSphere)
            {
                sunEffect.SetEyePosition(camera.Position);
                sunEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

                deviceContext.InputAssembler.InputLayout = sunEffect.InputLayout;
                foreach (var pass in sunEffect.Passes)
                {
                    objects[0].DrawSphere(deviceContext, sunEffect, pass);
                }
            }

            //Draw planets
            planetEffect.SetEyePosition(camera.Position);
            planetEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

            deviceContext.InputAssembler.InputLayout = planetEffect.InputLayout;
            foreach (var pass in planetEffect.Passes)
            {
                foreach (var currentObject in objects.Where(x => !x.PhysicsObject.IsObjectOfReference && x.ShowSphere))
                {
                    currentObject.DrawSphere(deviceContext, planetEffect, pass);
                }
            }

            //Draw planetary rings
            deviceContext.InputAssembler.InputLayout = ringEffect.InputLayout;
            foreach (var pass in ringEffect.Passes)
            {
                foreach (var currentObject in objects)
                {
                    if (currentObject.renderingOrbit != null)
                    {
                        currentObject.DrawRings(deviceContext, ringEffect, pass);
                    }
                }
            }
        }

        /// <summary>
        /// Draws orbits for the given given rendering
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="objects">The objects</param>
        public static void DrawOrbits(DeviceContext deviceContext, OrbitEffect orbitEffect, IList<RenderingObject> objects)
        {
            var camera = objects[0].camera;

            //Draw orbits
            deviceContext.InputAssembler.InputLayout = orbitEffect.InputLayout;
            foreach (var pass in orbitEffect.Passes)
            {
                foreach (var currentObject in objects)
                {
                    if (currentObject.renderingOrbit != null)
                    {
                        currentObject.UpdatePassedPositions();
                        currentObject.DrawOrbit(deviceContext, orbitEffect, pass);
                        currentObject.DrawNextManeuver(deviceContext, orbitEffect, pass);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.renderingSphere.Dispose();
            this.renderingOrbit?.Dispose();
            this.nextManeuverRenderingOrbit?.Dispose();
            this.ringRenderingOrbit?.Dispose();
        }
    }
}
