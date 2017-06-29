﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

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
        /// <param name="camera">The camera</param>
        /// <param name="coplanar">Indicates if all planets lie in the same plane</param>
        /// <returns>Simulator engine, sun rendering object, other rendering objects</returns>
        public static (SimulatorEngine, RenderingObject, IList<RenderingObject>) Create(SharpDX.Direct3D11.Device graphicsDevice, OrbitCamera camera, bool coplanar = false)
        {
            var sun = new PlanetObject(
                "Sun",
                PhysicsObjectType.ObjectOfReference,
                Simulator.SolarSystem.Sun.Mass,
                Simulator.SolarSystem.Sun.Radius,
                Simulator.SolarSystem.Sun.RotationalPeriod,
                Simulator.SolarSystem.Sun.AxisOfRotation,
                new NoAtmosphereModel(),
                null,
                new ObjectState(),
                new Physics.Orbit());
            //camera.SetScaleFactor(sun);

            var simulatorEngine = new SimulatorEngine(new List<PhysicsObject>() { sun });
            var renderingObjects = new List<RenderingObject>();

            PlanetObject AddPlanet(NaturalSatelliteObject primaryBody, string name, SolarSystemBody body, Color color, string textureName)
            {
                var orbit = new OrbitPosition(body.Orbit(primaryBody), 0.0);
                var newObject = simulatorEngine.AddPlanetInOrbit(
                    name,
                    PhysicsObjectType.NaturalSatellite,
                    body.Mass,
                    body.Radius,
                    body.RotationalPeriod,
                    body.AxisOfRotation,
                    new NoAtmosphereModel(),
                    orbit);
                renderingObjects.Add(new RenderingObject(graphicsDevice, camera, color, textureName, newObject));
                return newObject;
            }

            var baseDir = "Content/Textures/Planets/";

            var sunRenderingObject = new RenderingObject(graphicsDevice, camera, Color.Yellow, baseDir + "Sun.jpg", sun);

            AddPlanet(sun, "Mercury", Simulator.SolarSystem.Mercury, Color.Gray, baseDir + "Mercury.png");
            AddPlanet(sun, "Venus", Simulator.SolarSystem.Venus, new Color(255, 89, 0, 255), baseDir + "Venus2.jpg");
            var earth = AddPlanet(sun, "Earth", Simulator.SolarSystem.Earth, Color.Green, baseDir + "Earth.jpg");
            AddPlanet(earth, "Moon", Simulator.SolarSystem.Moon, Color.Magenta, baseDir + "Moon.jpg");
            var mars = AddPlanet(sun, "Mars", Simulator.SolarSystem.Mars, Color.Red, baseDir + "Mars4.png");
            AddPlanet(sun, "Jupiter", Simulator.SolarSystem.Jupiter, new Color(255, 106, 0, 255), baseDir + "Jupiter.jpg");
            AddPlanet(sun, "Saturn", Simulator.SolarSystem.Saturn, new Color(255, 167, 0, 255), baseDir + "Saturn.jpg");
            AddPlanet(sun, "Uranus", Simulator.SolarSystem.Uranus, Color.Blue, baseDir + "Uranus.jpg");
            AddPlanet(sun, "Neptune", Simulator.SolarSystem.Neptune, new Color(0, 148, 255, 255), baseDir + "Neptune.jpg");
            AddPlanet(sun, "Pluto", Simulator.SolarSystem.Pluto, new Color(143, 115, 87, 255), baseDir + "Pluto.png");

            var satellite1 = simulatorEngine.AddSatelliteInOrbit(
                "Satellite 1",
                1000,
                new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
                new OrbitPosition(Physics.Orbit.New(earth, semiMajorAxis: Simulator.SolarSystem.Earth.Radius + 300E3), 0.0));
            renderingObjects.Add(new RenderingObject(graphicsDevice, camera, Color.Yellow, baseDir + "Satellite.png", satellite1));

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

            return (simulatorEngine, sunRenderingObject, renderingObjects);
        }
    }
}