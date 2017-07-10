using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Camera;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.Simulator.Data;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Simulator.Environments
{
    /// <summary>
    /// Contains an environment for the solar system
    /// </summary>
    public static class SolarSystem
    {
        /// <summary>
        /// Creates a new system
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="coplanar">Indicates if all planets lie in the same plane</param>
        public static SimulatorContainer Create(SharpDX.Direct3D11.Device graphicsDevice, bool coplanar = false)
        {
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
            var renderingObjects = new List<RenderingObject>();
            var baseDir = "Content/Textures/Planets/";

            Func<PhysicsObject, RenderingObject> createRenderingObject = newObject =>
            {
                return new RenderingObject(
                    graphicsDevice,
                    newObject,
                    Color.Yellow,
                    baseDir + "Satellite.png");
            };

            PlanetObject AddPlanet(
                NaturalSatelliteObject primaryBody,
                string name,
                SolarSystemBody body,
                Color color,
                string textureName,
                float baseRotationY = 180.0f * MathHelpers.Deg2Rad,
                Color? ringColor = null,
                double ringRadius = 0)
            {
                var orbit = new OrbitPosition(body.Orbit(primaryBody), 0.0);

                if (coplanar)
                {
                    orbit = orbit.Set(inclination: 0, longitudeOfAscendingNode: 0, argumentOfPeriapsis: 0);
                }

                var newObject = simulatorEngine.AddPlanetInOrbit(
                    name,
                    body.Mass,
                    body.Radius,
                    body.RotationalPeriod,
                    body.AxisOfRotation,
                    body.AtmosphericModel,
                    orbit);

                renderingObjects.Add(new RenderingObject(
                    graphicsDevice,
                    newObject,
                    color,
                    textureName,
                    baseRotationY: baseRotationY,
                    ringColor: ringColor,
                    ringRadius: ringRadius));
                return newObject;
            }

            var sunRenderingObject = new RenderingObject(graphicsDevice, sun, Color.Yellow, baseDir + "Sun.jpg");
            renderingObjects.Add(sunRenderingObject);

            AddPlanet(sun, "Mercury", Simulator.Data.SolarSystemBodies.Mercury, Color.Gray, baseDir + "Mercury.png");
            AddPlanet(sun, "Venus", Simulator.Data.SolarSystemBodies.Venus, new Color(255, 89, 0, 255), baseDir + "Venus2.jpg");
            var earth = AddPlanet(sun, "Earth", Simulator.Data.SolarSystemBodies.Earth, Color.Green, baseDir + "Earth.jpg");
            AddPlanet(earth, "Moon", Simulator.Data.SolarSystemBodies.Moon, Color.Magenta, baseDir + "Moon.jpg");
            var mars = AddPlanet(sun, "Mars", Simulator.Data.SolarSystemBodies.Mars, Color.Red, baseDir + "Mars4.png");
            var jupiter = AddPlanet(sun, "Jupiter", Simulator.Data.SolarSystemBodies.Jupiter, new Color(255, 106, 0, 255), baseDir + "Jupiter.jpg");
            var saturn = AddPlanet(
                sun,
                "Saturn",
                Simulator.Data.SolarSystemBodies.Saturn,
                new Color(255, 167, 0, 255),
                baseDir + "Saturn.jpg",
                ringColor: new Color(255, 227, 107),
                ringRadius: 1.4 * Simulator.Data.SolarSystemBodies.Saturn.Radius);

            AddPlanet(sun, "Uranus", Simulator.Data.SolarSystemBodies.Uranus, Color.Blue, baseDir + "Uranus.jpg");
            AddPlanet(sun, "Neptune", Simulator.Data.SolarSystemBodies.Neptune, new Color(0, 148, 255, 255), baseDir + "Neptune.jpg");
            var pluto = AddPlanet(sun, "Pluto", Simulator.Data.SolarSystemBodies.Pluto, new Color(143, 115, 87, 255), baseDir + "Pluto.png");

            var satellite1 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 1",
                1000,
                new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
                new OrbitPosition(Physics.Orbit.New(earth, semiMajorAxis: earth.Radius + 300E3), 0.0));
            renderingObjects.Add(new RenderingObject(graphicsDevice, satellite1, Color.Yellow, baseDir + "Satellite.png"));

            //var rocketObject = simulatorEngine.AddSatellite(
            //    earth,
            //    "Rocket 1",
            //    1000,
            //    new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
            //    OrbitHelpers.FromCoordinates(earth, 28.524058 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
            //    //OrbitHelpers.FromCoordinates(earth, 0 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
            //    Vector3d.Zero);
            //rocketObject.CheckImpacted(0);
            //renderingObjects.Add(new RenderingObject(graphicsDevice, camera, rocketObject, Color.Yellow, baseDir + "Satellite.png"));

            //var falcon9TargetAltitude = 300E3;

            //var falcon9TargetOrbit = Physics.Orbit.New(earth, semiMajorAxis: earth.Radius + falcon9TargetAltitude, eccentricity: 0.0);
            var falcon9Object = simulatorEngine.AddRocket(
                earth,
                "Falcon 9",
                Rockets.CreateFalcon9(4000.0),
                OrbitHelpers.FromCoordinates(earth, 28.524058 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
                //OrbitHelpers.FromCoordinates(earth, 0, -80.65085 * MathUtild.Deg2Rad),
                Vector3d.Zero);
            renderingObjects.Add(new RenderingObject(graphicsDevice, falcon9Object, Color.Yellow, baseDir + "Satellite.png"));

            //var bestPitchStart = 2E3;
            //var bestPitchEnd = 12.8625E3;
            //falcon9Object.SetControlProgram(new AscentControlProgram(
            //    falcon9Object,
            //    falcon9TargetOrbit,
            //    bestPitchStart,
            //    bestPitchEnd,
            //    simulatorEngine.TextOutputWriter));

            falcon9Object.CheckImpacted(0);
            //falcon9Object.StartEngine();

            //var satellite2 = simulatorEngine.AddObjectInOrbit(
            //    "Satellite 2",
            //    new ObjectConfig(1000),
            //    new OrbitPosition(Physics.Orbit.New(earth, parameter: Simulator.SolarSystem.Earth.Radius * 2.0, eccentricity: 1.05), MathUtild.Deg2Rad * -50.0),
            //    isRealSize: false);
            //renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", satellite2));

            //var satellite3 = simulatorEngine.AddObjectInOrbit(
            //    "Satellite 3",
            //    new ObjectConfig(1000),
            //    new OrbitPosition(
            //        Physics.Orbit.New(
            //            mars,
            //            parameter: 1.1984E3,
            //            eccentricity: 1.0004,
            //            inclination: 22.2203,
            //            longitudeOfAscendingNode: 10.278,
            //            argumentOfPeriapsis: 210.5102),
            //        MathUtild.Deg2Rad * 181.7195),
            //    isRealSize: false);
            //renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", satellite3));

            return new SimulatorContainer(simulatorEngine, renderingObjects, createRenderingObject);
        }
    }
}
