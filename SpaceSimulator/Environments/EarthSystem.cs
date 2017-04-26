﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Physics.Rocket;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Environments
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
        /// <returns>Simulator engine, earth rendering object, other rendering objects</returns>
        public static (SimulatorEngine, RenderingObject, IList<RenderingObject>) Create(SharpDX.Direct3D11.Device graphicsDevice)
        {
            var baseDir = "Content/Textures/Planets/";

            var earth = new PlanetObject(
                "Earth",
                PhysicsObjectType.ObjectOfReference,
                Simulator.SolarSystem.Earth.CreateConfig(),
                null,
                new ObjectState(),
                new Physics.Orbit());
            var earthRenderingObject = new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Earth.jpg", earth);

            var simulatorEngine = new SimulatorEngine(new List<PhysicsObject>() { earth });
            var renderingObjects = new List<RenderingObject>();

            var moon = simulatorEngine.AddPlanetInOrbit(
                "Moon",
                PhysicsObjectType.NaturalSatellite,
                Simulator.SolarSystem.Moon.CreateConfig(),
                new OrbitPosition(Simulator.SolarSystem.Moon.Orbit(earth), 0.0));
            renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Magenta, baseDir + "Moon.jpg", moon));

            //var falcon9TargetAltitude = 250E3;
            var falcon9TargetAltitude = 300E3;
            //var falcon9TargetAltitude = 1000E3;

            //var falcon9TargetOrbit = Physics.Orbit.New(earth, semiMajorAxis: earth.Radius + falcon9TargetAltitude, eccentricity: 0.0);
            //var falcon9Object = simulatorEngine.AddRocketObject(
            //    earth,
            //    "Falcon 9",
            //    10,
            //    RocketStages.New(
            //        RocketEngine.CreateFromBurnTime(9, 845E3, 282, 10000, 162),
            //        RocketEngine.CreateFromBurnTime(1, 934E3, 348, 5000, 397)
            //    ),
            //    OrbitHelpers.FromCoordinates(earth, 28.524058 * MathUtild.Deg2Rad, -80.65085 * MathUtild.Deg2Rad),
            //    //OrbitHelpers.FromCoordinates(earth, 0, -80.65085 * MathUtild.Deg2Rad),
            //    Vector3d.Zero);
            //renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", falcon9Object));

            //var (bestPitchStart, bestPitchEnd) = AscentControlProgram.CalculateOptimalPitchManeuver(
            //    simulatorEngine.GetSimulator(PhysicsSimulationMode.PerturbationCowell),
            //    falcon9Object,
            //    2E3,
            //    falcon9TargetOrbit);
            //falcon9Object.SetControlProgram(new AscentControlProgram(falcon9Object, falcon9TargetOrbit, bestPitchStart, bestPitchEnd));
            //falcon9Object.CheckImpacted(0);
            //falcon9Object.StartEngine();

            var satellite1 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 1",
                new ObjectConfig(10, 1000),
                new OrbitPosition(Physics.Orbit.New(earth, semiMajorAxis: Simulator.SolarSystem.Earth.Radius + 300E3), 0.0));
            renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", satellite1));
            //simulatorEngine.ScheduleManeuver(
            //    satellite1,
            //    HohmannTransferOrbit.Create(simulatorEngine, satellite1, Simulator.SolarSystem.Earth.Radius * 2.0, OrbitalManeuverTime.TimeFromNow(10 * 60.0)));

            var orbitPosition2 = new OrbitPosition(
                Physics.Orbit.New(earth, parameter: 3.0 * Simulator.SolarSystem.Earth.Radius),
                87.2 * MathUtild.Deg2Rad);

            var object2 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 2",
                new ObjectConfig(10, 1000),
                orbitPosition2);
            renderingObjects.Add(new RenderingObject(graphicsDevice, Color.Yellow, baseDir + "Satellite.png", object2));

            //var rocketObject = simulatorEngine.AddObject(
            //    PhysicsObjectType.ArtificialSatellite,
            //    earth,
            //    "Rocket 1",
            //    new ObjectConfig(10, 1000),
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

            return (simulatorEngine, earthRenderingObject, renderingObjects);
        }
    }
}
