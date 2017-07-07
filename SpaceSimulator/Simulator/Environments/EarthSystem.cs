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
using SpaceSimulator.Physics.Rocket;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Simulator.Environments
{
    /// <summary>
    /// Contains an environment for the earth system
    /// </summary>
    public static class EarthSystem
    {
        /// <summary>
        /// Creates a new system
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        public static SimulatorContainer Create(SharpDX.Direct3D11.Device graphicsDevice)
        {
            var baseDir = "Content/Textures/Planets/";

            var earth = new PlanetObject(
                "Earth",
                PhysicsObjectType.ObjectOfReference,
                Simulator.Data.SolarSystemBodies.Earth.Mass,
                Simulator.Data.SolarSystemBodies.Earth.Radius,
                Simulator.Data.SolarSystemBodies.Earth.RotationalPeriod,
                Simulator.Data.SolarSystemBodies.Earth.AxisOfRotation,
                new EarthAtmosphericModel(),
                null,
                new ObjectState(),
                new Physics.Orbit());

            var earthRenderingObject = new RenderingObject(
                graphicsDevice,
                earth,
                Color.Yellow,
                baseDir + "Earth.jpg",
                MathUtil.DegreesToRadians(180.0f));

            var simulatorEngine = new SimulatorEngine(new List<PhysicsObject>() { earth });
            var renderingObjects = new List<RenderingObject>() { earthRenderingObject };

            Func<PhysicsObject, RenderingObject> createRenderingObject = newObject =>
            {
                return new RenderingObject(
                    graphicsDevice,
                    newObject,
                    Color.Yellow,
                    baseDir + "Satellite.png");
            };

            //var moon = simulatorEngine.AddPlanetInOrbit(
            //    "Moon",
            //    PhysicsObjectType.NaturalSatellite,
            //    Simulator.SolarSystem.Moon.CreateConfig(),
            //    new OrbitPosition(Simulator.SolarSystem.Moon.Orbit(earth), 0.0));
            //renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Magenta, baseDir + "Moon.jpg", moon));

            var falcon9Object = simulatorEngine.AddRocket(
                earth,
                "Falcon 9",
                Rockets.CreateFalcon9(4000.0),
                OrbitHelpers.FromCoordinates(earth, 28.524058 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
                //OrbitHelpers.FromCoordinates(earth, 0, -80.65085 * MathUtild.Deg2Rad),
                Vector3d.Zero);
            renderingObjects.Add(new RenderingObject(graphicsDevice, falcon9Object, Color.Yellow, baseDir + "Satellite.png"));

            var falcon9TargetAltitude = 300E3;

            var falcon9TargetOrbit = Physics.Orbit.New(earth, semiMajorAxis: earth.Radius + falcon9TargetAltitude, eccentricity: 0.0);

            //var (bestPitchStart, bestPitchEnd) = AscentControlProgram.CalculateOptimalPitchManeuver(
            //    simulatorEngine.GetSimulator(PhysicsSimulationMode.PerturbationCowell),
            //    falcon9Object,
            //    1E3,
            //    falcon9TargetOrbit);
            var bestPitchStart = 1E3;
            var bestPitchEnd = 15.7875E3;

            falcon9Object.SetControlProgram(new AscentControlProgram(
                falcon9Object,
                falcon9TargetOrbit,
                bestPitchStart,
                bestPitchEnd,
                simulatorEngine.TextOutputWriter));

            falcon9Object.CheckImpacted(0);
            falcon9Object.StartEngine();

            var satellite1 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 1",
                1000,
                new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
                new OrbitPosition(Physics.Orbit.New(earth, semiMajorAxis: Simulator.Data.SolarSystemBodies.Earth.Radius + 300E3), 0.0));
            renderingObjects.Add(new RenderingObject(graphicsDevice, satellite1, Color.Yellow, baseDir + "Satellite.png"));

            var orbitPosition2 = new OrbitPosition(
                Physics.Orbit.New(
                    earth,
                    parameter: 3.0 * Simulator.Data.SolarSystemBodies.Earth.Radius,
                    eccentricity: 0.2,
                    inclination: 30 * MathUtild.Deg2Rad),
                87.2 * MathUtild.Deg2Rad);

            var object2 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 2",
                1000,
                new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
                orbitPosition2);
            renderingObjects.Add(new RenderingObject(graphicsDevice, object2, Color.Yellow, baseDir + "Satellite.png"));

            //var rocketObject = simulatorEngine.AddObject(
            //    PhysicsObjectType.ArtificialSatellite,
            //    earth,
            //    "Rocket 1",
            //    new ObjectConfig(1000),
            //    OrbitHelpers.FromCoordinates(earth, 28.524058 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
            //    //OrbitHelpers.FromCoordinates(earth, 0 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
            //    Vector3d.Zero);
            //rocketObject.CheckImpacted(0);
            //renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", rocketObject));

            //var startTime = DateTime.UtcNow;
            //var maneuvers = InterceptManeuver.Intercept(
            //    simulatorEngine,
            //    rocketObject,
            //    object2.Configuration,
            //    orbitPosition2,
            //    1000,
            //    8000,
            //    0,
            //    8.0 * 60.0 * 60.0);
            //Console.WriteLine($"Computed in {(DateTime.UtcNow - startTime).TotalSeconds} seconds.");
            //simulatorEngine.ScheduleManeuver(rocketObject, maneuvers);

            return new SimulatorContainer(simulatorEngine, renderingObjects, createRenderingObject);
        }
    }
}
