using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Common;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using SpaceSimulator.Helpers;
using SpaceSimulator.Environments;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents an object used for rendering
    /// </summary>
    public sealed class RenderingObject : IDisposable
    {
        private readonly Device graphicsDevice;

        /// <summary>
        /// The object being drawn
        /// </summary>
        public PhysicsObject PhysicsObject { get; }

        private int orbitVersion = 0;
        private readonly Color orbitColor;
        private IList<Orbit.Point> orbitPositions;
        private readonly Orbit renderingOrbit;

        /// <summary>
        /// The model
        /// </summary>
        public IPhysicsObjectModel Model { get; }

        private readonly Rendering.Orbit nextManeuverRenderingOrbit;
        private DateTime nextManeuverRenderingOrbitUpdated;
        private readonly TimeSpan nextManeuverRenderingOrbitUpdateFrequency = TimeSpan.FromSeconds(1.0);
        private bool drawNextManeuver = false;

        private readonly PlanetaryRings rings;

        private readonly TimeSpan orbitUpdateTime = TimeSpan.FromMilliseconds(50.0);
        private DateTime lastOrbitUpdate = new DateTime();
        private bool updateOrbit = false;

        /// <summary>
        /// Indicates if the orbit should be drawn
        /// </summary>
        public bool ShowOrbit { get; set; } = true;

        /// <summary>
        /// Indicates if the model should be drawn
        /// </summary>
        public bool ShowModel { get; set; } = true;

        /// <summary>
        /// Creates a new rendering object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="physicsObject">The physics object to render</param>
        /// <param name="orbitColor">The color of the orbit</param>
        /// <param name="model">The model</param>
        /// <param name="rings">The planetary rings</param>
        public RenderingObject(
            Device graphicsDevice,
            PhysicsObject physicsObject,
            Color orbitColor,
            IPhysicsObjectModel model,
            PlanetaryRings rings = null)
        {
            this.graphicsDevice = graphicsDevice;

            this.PhysicsObject = physicsObject;
            this.orbitColor = orbitColor;
            this.Model = model;

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
                    this.orbitPositions,
                    orbitColor,
                    drawRelativeToFocus);
            }

            this.nextManeuverRenderingOrbit = new Rendering.Orbit(
                graphicsDevice,
                new List<Rendering.Orbit.Point>(),
                new Color(124, 117, 6),
                drawRelativeToFocus);

            if (rings != null && physicsObject is NaturalSatelliteObject naturalSatelliteObject)
            {
                this.rings = rings;
            }
        }

        /// <summary>
        /// Creates a new rendering object
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="physicsObject">The physics object to render</param>
        /// <param name="orbitColor">The color of the orbit</param>
        /// <param name="textureName">The name of the texture</param>
        /// <param name="baseTransform">The base transform<</param>
        /// <param name="rings">The rings</param>
        public RenderingObject(
            Device graphicsDevice,
            PhysicsObject physicsObject,
            Color orbitColor,
            string textureName,
            Matrix? baseTransform = null,
            PlanetaryRings rings = null)
            : this(
                  graphicsDevice,
                  physicsObject,
                  orbitColor,
                  new Sphere(
                      graphicsDevice,
                      1.0f,
                      textureName,
                      Sphere.DefaultMaterial,
                      baseTransform ?? Matrix.Identity),
                  rings)
        {

        }

        /// <summary>
        /// Creates a rendering object for the given physics object assumed to be a part of the current object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        public RenderingObject CreateForSub(PhysicsObject physicsObject)
        {
            if (this.PhysicsObject is RocketObject rocketObject)
            {
                var rocketModel = (Rocket)this.Model;

                return new RenderingObject(
                    this.graphicsDevice,
                    physicsObject,
                    this.orbitColor,
                    rocketModel.CreateSpentStage(rocketObject));
            }

            return new RenderingObject(
                this.graphicsDevice,
                physicsObject,
                this.orbitColor,
                EnvironmentHelpers.BaseDirectory + "Satellite.png");
        }

        /// <summary>
        /// Calculates the orbit positions
        /// </summary>
        private void CalculateOrbitPositions()
        {
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.PhysicsObject);

            if (orbitPosition.Orbit.IsRadialParabolic)
            {
                this.orbitPositions = OrbitPositions.CreateRadialTrajectory(this.PhysicsObject, orbitPosition);
            }
            else if (orbitPosition.Orbit.IsHyperbolic && Math.Abs(orbitPosition.Orbit.Eccentricity - 1.0) <= 1E-2)
            {
                var timeToCrash = OrbitCalculators.TimeToImpact(orbitPosition);
                this.orbitPositions = OrbitPositions.CreateForUnbound(
                    new KeplerProblemUniversalVariableSolver(),
                    this.PhysicsObject,
                    orbitPosition.Orbit,
                    Math.Min(timeToCrash ?? 0.5 * TimeConstants.OneDay, 3.0 * TimeConstants.OneDay));
            }
            else
            {
                this.orbitPositions = OrbitPositions.Create(orbitPosition.Orbit, true, trueAnomaly: orbitPosition.TrueAnomaly);
            }
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
            if (this.PhysicsObject.HasChangedOrbit(ref orbitVersion))
            {
                this.updateOrbit = true;
            }

            if (this.updateOrbit)
            {
                if (DateTime.UtcNow - this.lastOrbitUpdate >= this.orbitUpdateTime)
                {
                    this.CalculateOrbitPositions();
                    this.renderingOrbit.Update(this.orbitPositions);
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
        /// The transform of the primary body
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="relativeToFocus">Indicates if relative to focus</param>
        /// <param name="primaryBody">The primary body</param>
        private Matrix PrimaryBodyTransform(SpaceCamera camera, bool relativeToFocus, NaturalSatelliteObject primaryBody = null)
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

            transform *= Matrix.Translation(camera.ToDrawPosition(primaryBody.Position, relativeToFocus: relativeToFocus));
            return transform;
        }

        /// <summary>
        /// The transform of the primary body
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="renderingOrbit">The rendering orbit</param>
        /// <param name="primaryBody">The primary body</param>
        private Matrix PrimaryBodyTransform(SpaceCamera camera, Orbit renderingOrbit, NaturalSatelliteObject primaryBody = null)
        {
            return this.PrimaryBodyTransform(camera, !renderingOrbit.DrawRelativeToFocus, primaryBody);
        }

        /// <summary>
        /// Returns the draw position
        /// </summary>
        /// <param name="camera">The camera</param>
        public Vector3 DrawPosition(SpaceCamera camera) => camera.ToDrawPosition(this.PhysicsObject.Position);

        /// <summary>
        /// Draws the sphere
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="position">The position to draw at</param>
        public void DrawSphere(DeviceContext deviceContext, BasicEffect planetEffect, EffectPass pass, SpaceCamera camera, Vector3? position = null)
        {
            if (this.Model is Sphere sphere)
            {
                sphere.Draw(
                    deviceContext,
                    planetEffect,
                    pass,
                    camera,
                    sphere.ScalingMatrix(camera, this.PhysicsObject)
                    * sphere.Transform
                    * Matrix.RotationY(-(float)this.PhysicsObject.Rotation)
                    * Matrix.Translation(position ?? this.DrawPosition(camera)));
            }
        }

        /// <summary>
        /// Draws the orbit
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        public void DrawOrbit(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass, SpaceCamera camera)
        {
            if (!this.PhysicsObject.HasImpacted)
            {
                this.renderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    camera,
                    this.PrimaryBodyTransform(camera, this.renderingOrbit),
                    this.DrawPosition(camera));
            }
        }

        /// <summary>
        /// Draws the next maneuver
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        private void DrawNextManeuver(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass, SpaceCamera camera)
        {
            if (this.drawNextManeuver)
            {
                this.nextManeuverRenderingOrbit.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    camera,
                    this.PrimaryBodyTransform(camera, this.nextManeuverRenderingOrbit),
                    this.DrawPosition(camera));
            }
        }

        /// <summary>
        /// Draws the rings
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        private void DrawRings(DeviceContext deviceContext, OrbitEffect orbitEffect, EffectPass pass, SpaceCamera camera)
        {
            if (this.rings != null)
            {
                this.rings.Draw(
                    deviceContext,
                    orbitEffect,
                    pass,
                    camera,
                    this.PrimaryBodyTransform(camera, true, (NaturalSatelliteObject)this.PhysicsObject),
                    this.DrawPosition(camera));
            }
        }

        /// <summary>
        /// Draws planets
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sunEffect">The sun effect</param>
        /// <param name="planetEffect">The planet effect</param>
        /// <param name="ringEffect">The ring effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="objects">The objects</param>
        public static void DrawPlanets(
            DeviceContext deviceContext,
            BasicEffect sunEffect,
            BasicEffect planetEffect,
            OrbitEffect ringEffect,
            SpaceCamera camera,
            IList<RenderingObject> objects)
        {
            //Draw the sun
            if (objects[0].ShowModel)
            {
                sunEffect.SetEyePosition(camera.Position);
                sunEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

                deviceContext.InputAssembler.InputLayout = sunEffect.InputLayout;
                foreach (var pass in sunEffect.Passes)
                {
                    objects[0].DrawSphere(deviceContext, sunEffect, pass, camera);
                }
            }

            //Draw planets
            planetEffect.SetEyePosition(camera.Position);
            planetEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

            deviceContext.InputAssembler.InputLayout = planetEffect.InputLayout;
            foreach (var pass in planetEffect.Passes)
            {
                foreach (var currentObject in objects.Where(obj => obj.PhysicsObject.Type == PhysicsObjectType.NaturalSatellite && obj.ShowModel))
                {
                    currentObject.DrawSphere(deviceContext, planetEffect, pass, camera);
                }
            }

            //Draw planetary rings
            //ringEffect.SetEyePosition(camera.Position);
            //ringEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

            deviceContext.InputAssembler.InputLayout = ringEffect.InputLayout;
            foreach (var pass in ringEffect.Passes)
            {
                foreach (var currentObject in objects.Where(obj => obj.ShowModel))
                {
                    currentObject.DrawRings(deviceContext, ringEffect, pass, camera);
                }
            }
        }

        /// <summary>
        /// Draws artificial objects
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="textureEffect">The texture effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="objects">The objects</param>
        public static void DrawObjects(
            DeviceContext deviceContext,
            BasicEffect effect,
            BasicEffect textureEffect,
            SpaceCamera camera,
            IList<RenderingObject> objects)
        {
            foreach (var pass in effect.Passes)
            {
                foreach (var currentObject in objects.Where(obj => obj.PhysicsObject.Type == PhysicsObjectType.ArtificialSatellite && obj.ShowModel))
                {
                    currentObject.Model.Draw(
                        deviceContext, 
                        currentObject.Model.IsTextured ? textureEffect : effect,
                        camera, 
                        currentObject.PhysicsObject);
                }
            }
        }

        /// <summary>
        /// Draws orbits for the given given rendering
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="orbitEffect">The orbit effect</param>
        /// <param name="camera">The cameraa</param>
        /// <param name="objects">The objects</param>
        public static void DrawOrbits(DeviceContext deviceContext, OrbitEffect orbitEffect, SpaceCamera camera, IList<RenderingObject> objects)
        {
            //Draw orbits
            deviceContext.InputAssembler.InputLayout = orbitEffect.InputLayout;
            foreach (var pass in orbitEffect.Passes)
            {
                foreach (var currentObject in objects.Where(x => x.ShowOrbit))
                {
                    if (currentObject.renderingOrbit != null)
                    {
                        currentObject.UpdatePassedPositions();
                        currentObject.DrawOrbit(deviceContext, orbitEffect, pass, camera);
                        currentObject.DrawNextManeuver(deviceContext, orbitEffect, pass, camera);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Model.Dispose();
            this.renderingOrbit?.Dispose();
            this.nextManeuverRenderingOrbit?.Dispose();
            this.rings?.Dispose();
        }
    }
}
