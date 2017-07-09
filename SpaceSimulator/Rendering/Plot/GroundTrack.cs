using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering.Plot
{
    /// <summary>
    /// Represents a ground track of an object
    /// </summary>
    public sealed class GroundTrack : IDisposable
    {
        private readonly RenderingManager2D renderingManager2D;
        private readonly PhysicsObject physicsObject;

        private Function2D plot;
        private readonly IRenderingBrush currentPositionBrush;
        private RenderingImage2D primaryBodyImage;

        //private readonly Sphere projectedOrbitSphere;

        private Vector2 position;

        private double lastTrackCreated = 0.0;
        private int trackOrbitVersion;
        private readonly TimeSpan trackRefreshRate = TimeSpan.FromSeconds(0.5);
        private DateTime lastTrackCreatedTime;

        /// <summary>
        /// Creates a new ground track of the given object
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="physicsObject">The object to track</param>
        /// <param name="position">The position of the object</param>
        public GroundTrack(RenderingManager2D renderingManager2D, PhysicsObject physicsObject, Vector2 position)
        {
            this.renderingManager2D = renderingManager2D;
            this.physicsObject = physicsObject;

            this.position = position;
            this.CreatePlot();

            this.currentPositionBrush = this.renderingManager2D.CreateSolidColorBrush(Color.Red);
            this.primaryBodyImage = this.renderingManager2D.LoadImage("Content/Textures/Planets/Earth.jpg");
            //this.projectedOrbitSphere = new Sphere(
            //    graphicsDevice,
            //    1.0f,
            //    "Content/Textures/Planets/Satellite.png",
            //    new Material()
            //    {
            //        Ambient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            //        Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            //        Specular = new Vector4(0.6f, 0.6f, 0.6f, 16.0f)
            //    });
        }

        /// <summary>
        /// Returns the width of the ground track
        /// </summary>
        public int Width => this.plot.Width;

        /// <summary>
        /// Returns the height of the ground track
        /// </summary>
        public int Height => this.plot.Height;

        /// <summary>
        /// The position where the ground track is drawn
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                this.plot.Position = value;
            }
        }

        /// <summary>
        /// Creates the plot
        /// </summary>
        private void CreatePlot()
        {
            this.plot = new Function2D(
                this.renderingManager2D,
                this.CalculateTrackPositions(2),
                this.Position,
                Color.Red,
                400,
                200,
                labelAxisX: "Longitude",
                labelAxisY: "Latitude",
                splitIntoMultipleParts: true,
                splitPart: (diffX, diffY) => Math.Abs(diffX) > MathHelpers.Deg2Rad * 10.0f,
                minPosition: new Vector2(-180.0f, -90.0f) * MathHelpers.Deg2Rad,
                maxPosition: new Vector2(180.0f, 90.0f) * MathHelpers.Deg2Rad)
            {
                DrawBackground = false
            };

            this.lastTrackCreated = physicsObject.State.Time;
            this.trackOrbitVersion = this.physicsObject.OrbitVersion;
            this.lastTrackCreatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates the track positions
        /// </summary>
        /// <param name="numOrbits">The number of orbits</param>
        private IList<Vector2> CalculateTrackPositions(int numOrbits)
        {
            var primaryBody = this.physicsObject.PrimaryBody;
            var primaryRotation = primaryBody.Rotation;
            var values = new List<Vector2>();
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.physicsObject);
            var orbit = orbitPosition.Orbit;

            //for (int i = 0; i < numOrbits; i++)
            //{
            //    var prevTimeSincePeriapsis = 0.0;
            //    for (double trueAnomaly = 0; trueAnomaly <= MathUtild.TwoPi; trueAnomaly += MathUtild.Deg2Rad * 5.0)
            //    {
            //        var currentOrbitPosition = new Physics.OrbitPosition(orbit, orbitPosition.TrueAnomaly + trueAnomaly);
            //        var timeSincePeriapsis = orbit.Period - currentOrbitPosition.TimeToPeriapsis();
            //        var deltaTime = timeSincePeriapsis - prevTimeSincePeriapsis;
            //        prevTimeSincePeriapsis = timeSincePeriapsis;

            //        primaryRotation = Physics.Solvers.SolverHelpers.CalculateRotation(primaryBody.RotationalPeriod, primaryRotation, deltaTime);
            //        OrbitHelpers.GetCoordinates(
            //            primaryBody,
            //            primaryRotation,
            //            currentOrbitPosition.CalculateState().Position,
            //            out var latitude,
            //            out var longitude);

            //        values.Add(new Vector2((float)longitude, (float)latitude));
            //    }
            //}

            var deltaTime = 100.0;
            var keplerProblemSolver = new Physics.Solvers.KeplerProblemUniversalVariableSolver();
            for (double t = 0.0; t <= orbit.Period * numOrbits; t += deltaTime)
            {
                primaryRotation = Physics.Solvers.SolverHelpers.CalculateRotation(primaryBody.RotationalPeriod, primaryRotation, deltaTime);
                var nextState = Physics.Solvers.SolverHelpers.AfterTime(
                    keplerProblemSolver,
                    this.physicsObject,
                    this.physicsObject.State,
                    orbit,
                    t);

                Physics.OrbitHelpers.GetCoordinates(primaryBody, primaryRotation, nextState.Position, out var latitude, out var longitude);
                values.Add(new Vector2((float)longitude, (float)latitude));
            }

            return values;
        }

        /// <summary>
        /// Draws the ground track
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Draw(SharpDX.Direct2D1.DeviceContext deviceContext)
        {
            Rendering2DHelpers.BindResources(
                deviceContext,
                this.primaryBodyImage,
                this.currentPositionBrush);

            var timeSinceLastUpdate = this.physicsObject.State.Time - this.lastTrackCreated;
            if ((timeSinceLastUpdate >= this.physicsObject.ReferenceOrbit.Period
                || this.trackOrbitVersion != this.physicsObject.OrbitVersion)
                && DateTime.UtcNow - this.lastTrackCreatedTime >= this.trackRefreshRate)
            {
                Console.WriteLine("Updated ground track.");
                this.CreatePlot();
            }

            var imageScale =
                new Vector2(this.plot.Width, this.plot.Height)
                / new Vector2(this.primaryBodyImage.Size.Width, this.primaryBodyImage.Size.Height);

            this.primaryBodyImage.ApplyResource(image =>
            {
                deviceContext.DrawBitmap(
                    image,
                    1.0f,
                    SharpDX.Direct2D1.InterpolationMode.MultiSampleLinear,
                    Matrix.Scaling(new Vector3(imageScale, 0)) * Matrix.Translation(new Vector3(this.plot.Position, 0)));
            });

            this.plot.Draw(deviceContext);

            this.currentPositionBrush.ApplyResource(brush =>
            {
                var projectedPosition = new Vector2(
                    (float)this.physicsObject.Longitude,
                    (float)this.physicsObject.Latitude);

                var positionRadius = 3.0f;

                deviceContext.FillEllipse(
                    new SharpDX.Direct2D1.Ellipse(
                        this.plot.Position + this.plot.PlotPosition(projectedPosition),
                        positionRadius,
                        positionRadius),
                    brush);
            });
        }

        /// <summary>
        /// Draws the 3D part of the ground track
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="sphereEffect">The effect</param>
        /// <param name="camera">The camera</param>
        //public void Draw3D(SharpDX.Direct3D11.DeviceContext deviceContext, BasicEffect sphereEffect, SpaceCamera camera)
        //{
        //    var projectedPosition = OrbitHelpers.FromCoordinates(this.physicsObject.PrimaryBody, this.physicsObject.Latitude, this.physicsObject.Longitude);

        //    sphereEffect.SetEyePosition(camera.Position);
        //    sphereEffect.SetPointLightSource(camera.ToDrawPosition(Vector3d.Zero));

        //    deviceContext.InputAssembler.InputLayout = sphereEffect.InputLayout;
        //    foreach (var pass in sphereEffect.Passes)
        //    {
        //        this.projectedOrbitSphere.Draw(
        //            deviceContext,
        //            sphereEffect,
        //            pass,
        //            camera,
        //            Matrix.Scaling(0.001f) * Matrix.Translation(camera.ToDrawPosition(projectedPosition)));
        //    }
        //}

        public void Dispose()
        {
            this.plot.Dispose();
            this.currentPositionBrush.Dispose();
            //this.projectedOrbitSphere.Dispose();
        }
    }
}
