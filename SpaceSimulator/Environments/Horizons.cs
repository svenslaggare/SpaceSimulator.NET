using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Integrations;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Environments
{
    /// <summary>
    /// Contains an environment with data obtained from NASA Horizons system
    /// </summary>
    public class Horizons : IEnvironment
    {
        /// <summary>
        /// Contains predefined object data
        /// </summary>
        public class ObjectData
        {
            /// <summary>
            /// The texture for the object
            /// </summary>
            public string TextureName { get; }

            /// <summary>
            /// The base transform
            /// </summary>
            public Matrix BaseTransform { get; }

            /// <summary>
            /// The physical properties
            /// </summary>
            public PhysicalProperties PhysicalProperties { get; }

            /// <summary>
            /// The axis-of-rotation
            /// </summary>
            public Vector3d AxisOfRotation { get; }

            /// <summary>
            /// The atmospheric model
            /// </summary>
            public IAtmosphericModel AtmosphericModel { get; }

            /// <summary>
            /// The color of the orbit
            /// </summary>
            public Color OrbitColor { get; }

            /// <summary>
            /// Creates new object data
            /// </summary>
            /// <param name="textureName">The texture for the object</param>
            /// <param name="baseTransform">The base transform</param>
            /// <param name="physicalProperties">The physical properties</param>
            /// <param name="axisOfRotation">The axis-of-rotation</param>
            /// <param name="atmosphericModel">The atmospheric model</param>
            /// <param name="orbitColor">The color of the orbit</param>
            public ObjectData(
                string textureName,
                Matrix baseTransform,
                PhysicalProperties physicalProperties,
                Vector3d axisOfRotation,
                IAtmosphericModel atmosphericModel,
                Color orbitColor)
            {
                this.TextureName = textureName;
                this.BaseTransform = baseTransform;
                this.PhysicalProperties = physicalProperties;
                this.AxisOfRotation = axisOfRotation;
                this.AtmosphericModel = atmosphericModel;
                this.OrbitColor = orbitColor;
            }
        }

        private readonly IDictionary<string, ObjectData> predefinedData = new Dictionary<string, ObjectData>();
        private readonly DateTime time;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="time">The time to base the data on</param>
        public Horizons(DateTime time)
        {
            this.time = time;

            var baseDir = EnvironmentHelpers.BaseDirectory;
            var baseTransform = Matrix.RotationY(180.0f * MathHelpers.Deg2Rad);

            this.predefinedData.Add(
                "Mercury",
                new ObjectData(
                    baseDir + "Mercury.png",
                    baseTransform,
                    this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Mercury),
                    Vector3d.Up,
                    new NoAtmosphereModel(),
                    Color.Gray));

            this.predefinedData.Add(
              "Venus",
              new ObjectData(
                  baseDir + "Venus2.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Venus),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  new Color(255, 89, 0, 255)));

            this.predefinedData.Add(
              "Earth",
              new ObjectData(
                  baseDir + "Earth.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Earth),
                  Vector3d.Up,
                  new EarthAtmosphericModel(),
                  Color.Green));

            this.predefinedData.Add(
              "Mars",
              new ObjectData(
                  baseDir + "Mars4.png",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Mars),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  Color.Red));

            this.predefinedData.Add(
              "Jupiter",
              new ObjectData(
                  baseDir + "Jupiter.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Jupiter),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  new Color(255, 106, 0, 255)));

            this.predefinedData.Add(
              "Saturn",
              new ObjectData(
                  baseDir + "Saturn.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Saturn),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  new Color(255, 167, 0, 255)));

            this.predefinedData.Add(
              "Uranus",
              new ObjectData(
                  baseDir + "Uranus.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Uranus),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  Color.Blue));

            this.predefinedData.Add(
              "Neptune",
              new ObjectData(
                  baseDir + "Neptune.jpg",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Neptune),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  new Color(0, 148, 255, 255)));

            this.predefinedData.Add(
              "Pluto",
              new ObjectData(
                  baseDir + "Pluto.png",
                  baseTransform,
                  this.GetPhysicalProperties(Simulator.Data.SolarSystemBodies.Pluto),
                  Vector3d.Up,
                  new NoAtmosphereModel(),
                  new Color(143, 115, 87, 255)));
        }

        /// <summary>
        /// Gets the physical properties for the given body
        /// </summary>
        /// <param name="body">The body</param>
        private PhysicalProperties GetPhysicalProperties(Simulator.Data.SolarSystemBody body)
        {
            return new PhysicalProperties()
            {
                Mass = body.Mass,
                MeanRadius = body.MeanRadius,
                RotationalPeriod = body.RotationalPeriod
            };
        }

        /// <summary>
        /// Creates a new system
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        public SimulatorContainer Create(Device graphicsDevice)
        {
            var baseDir = EnvironmentHelpers.BaseDirectory;
            var sun = new PlanetObject(
                "Sun",
                PhysicsObjectType.ObjectOfReference,
                Simulator.Data.SolarSystemBodies.Sun.Mass,
                Simulator.Data.SolarSystemBodies.Sun.Radius,
                Simulator.Data.SolarSystemBodies.Sun.RotationalPeriod,
                Simulator.Data.SolarSystemBodies.Sun.AxisOfRotation,
                new NoAtmosphereModel(),
                null,
                new ObjectState(),
                new Physics.Orbit());

            var simulatorEngine = new SimulatorEngine(new List<PhysicsObject>() { sun });
            var renderingObjects = new List<RenderingObject>
            {
                new RenderingObject(graphicsDevice, sun, Color.Yellow, baseDir + "Sun.jpg")
            };

            var planets = new List<string>()
            {
                "Mercury",
                "Venus",
                "Earth",
                "Mars",
                "Jupiter",
                "Saturn",
                "Uranus",
                "Neptune",
                "Pluto",
            };

            var nasaHorizons = new NASAHorizons();
            System.Threading.SynchronizationContext.SetSynchronizationContext(null);
            var horizonsData = nasaHorizons.FetchAllCached(planets, this.time);

            foreach (var planet in planets)
            {
                var horizonsPlanetData = horizonsData[planet];
                var physicalProperties = horizonsPlanetData.PhysicalProperties;
                var orbitProperties = horizonsPlanetData.OrbitProperties;

                var orbit = new OrbitPosition(
                    Physics.Orbit.New(
                        sun,
                        semiMajorAxis: orbitProperties.SemiMajorAxis,
                        eccentricity: orbitProperties.Eccentricity,
                        inclination: orbitProperties.Inclination,
                        longitudeOfAscendingNode: orbitProperties.LongitudeOfAscendingNode,
                        argumentOfPeriapsis: orbitProperties.ArgumentOfPeriapsis),
                    orbitProperties.TrueAnomaly);

                var axisOfRotation = Vector3d.Up;
                IAtmosphericModel atmosphericModel = new NoAtmosphereModel();
                var orbitColor = Color.Yellow;
                var textureName = baseDir + "Satellite.png";
                var baseTransform = Matrix.Identity;

                if (this.predefinedData.TryGetValue(planet, out var planetData))
                {
                    physicalProperties = planetData.PhysicalProperties;
                    axisOfRotation = planetData.AxisOfRotation;
                    atmosphericModel = planetData.AtmosphericModel;
                    orbitColor = planetData.OrbitColor;
                    textureName = planetData.TextureName;
                    baseTransform = planetData.BaseTransform;
                }

                var newObject = simulatorEngine.AddPlanetInOrbit(
                    planet,
                    physicalProperties.Mass,
                    physicalProperties.MeanRadius,
                    physicalProperties.RotationalPeriod,
                    axisOfRotation,
                    atmosphericModel,
                    orbit);

                renderingObjects.Add(new RenderingObject(
                    graphicsDevice,
                    newObject,
                    orbitColor,
                    textureName,
                    baseTransform: baseTransform));
            }

            Func<PhysicsObject, RenderingObject> createRenderingObject = newObject =>
            {
                return new RenderingObject(
                    graphicsDevice,
                    newObject,
                    Color.Yellow,
                    baseDir + "Satellite.png");
            };

            return new SimulatorContainer(simulatorEngine, renderingObjects, createRenderingObject);
        }
    }
}
