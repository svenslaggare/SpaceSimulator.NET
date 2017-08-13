using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.Environments
{
    /// <summary>
    /// Contains an environment for the earth system
    /// </summary>
    public sealed class EarthSystem : IEnvironment
    {
        /// <summary>
        /// Creates a new system
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        public SimulatorContainer Create(SharpDX.Direct3D11.Device graphicsDevice)
        {
            var baseDir = EnvironmentHelpers.BaseDirectory;

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
                Matrix.RotationY(MathUtil.DegreesToRadians(180.0f)));

            var simulatorEngine = new SimulatorEngine(new List<PhysicsObject>() { earth });
            var renderingObjects = new List<RenderingObject>() { earthRenderingObject };

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
            renderingObjects.Add(new RenderingObject(
                graphicsDevice, 
                falcon9Object, 
                Color.Yellow,
                Rendering.Rocket.CreateFalcon9(graphicsDevice)));

            var falcon9TargetAltitude = 300E3;
            //var falcon9TargetAltitude = OrbitFormulas.SemiMajorAxisFromOrbitalPeriod(
            //    earth.StandardGravitationalParameter,
            //    earth.RotationalPeriod) - earth.Radius;

            var falcon9TargetOrbit = Physics.Orbit.New(earth, semiMajorAxis: earth.Radius + falcon9TargetAltitude, eccentricity: 0.0);

            //var (bestPitchStart, bestPitchEnd) = AscentProgram.CalculateOptimalPitchManeuver(
            //    simulatorEngine.GetSimulator(PhysicsSimulationMode.PerturbationCowell),
            //    falcon9Object,
            //    1E3,
            //    falcon9TargetOrbit);
            //var bestPitchStart = 1E3;
            //var bestPitchEnd = 2.8E3;

            var bestPitchStart = 1E3;
            var bestPitchEnd = 2E3;

            falcon9Object.SetControlProgram(new AscentProgram(
                falcon9Object,
                falcon9TargetOrbit,
                bestPitchStart,
                bestPitchEnd,
                simulatorEngine.TextOutputWriter));

            falcon9Object.CheckImpacted(0);
            falcon9Object.StartProgram();

            //var satellite1 = simulatorEngine.AddSatelliteInOrbit(
            //    "Satellite 1",
            //    1000,
            //    new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
            //    new OrbitPosition(
            //        Physics.Orbit.New(
            //            earth,
            //            semiMajorAxis: earth.Radius + 300E3,
            //            //semiMajorAxis: OrbitFormulas.SemiMajorAxisFromOrbitalPeriod(earth.StandardGravitationalParameter, Constants.SiderealDay),
            //            //longitudeOfAscendingNode: 45.0 * MathUtild.Deg2Rad,
            //            inclination: 0.0 * MathUtild.Deg2Rad),
            //        0.0));
            //renderingObjects.Add(new RenderingObject(graphicsDevice, satellite1, Color.Yellow, baseDir + "Satellite.png"));

            //var orbitPosition2 = new OrbitPosition(
            //  Physics.Orbit.New(
            //      earth,
            //      parameter: earth.Radius + 300E3,
            //      inclination: 45.0 * MathUtild.Deg2Rad),
            //  0.0);

            //var satellite2 = simulatorEngine.AddSatelliteInOrbit(
            //    "Satellite 2",
            //    1000,
            //    new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
            //    orbitPosition2);

            //renderingObjects.Add(new RenderingObject(graphicsDevice, satellite2, Color.Yellow, baseDir + "Satellite.png"));

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

            return new SimulatorContainer(graphicsDevice, simulatorEngine, renderingObjects);
        }
    }
}
