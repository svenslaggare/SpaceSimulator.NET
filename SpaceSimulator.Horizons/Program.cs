using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Integrations;

namespace SpaceSimulator.Horizons
{
    class Program
    {
        static void Main(string[] args)
        {
            var planets = new List<string>()
            {
                //"Mercury",
                //"Venus",
                //"Earth",
                //"Mars",
                "Jupiter",
                //"Saturn",
                //"Uranus",
                //"Neptune",
                //"Pluto",
            };

            var nasaHorizons = new NASAHorizons(referencePlane: ReferencePlane.Ecliptic);
            //var data = nasaHorizons.FetchAllCached(planets, new DateTime(2017, 8, 1, 0, 0, 0), true);

            foreach (var planet in planets)
            {
                //var task = nasaHorizons.FetchData(planet, new DateTime(2017, 8, 1, 0, 0, 0));
                //var task = nasaHorizons.FetchData(planet, new DateTime(2000, 1, 1, 12, 0, 0));
                var task = nasaHorizons.FetchData(planet, null);
                task.Wait();
                var horizonsData = task.Result;
                var physicalProperties = horizonsData.PhysicalProperties;
                var orbitProperties = horizonsData.OrbitProperties;

                Console.WriteLine(planet);
                Console.WriteLine($"Mass = {DataFormatter.Format(physicalProperties.Mass, DataUnit.Mass, useBase10: true)}");
                Console.WriteLine($"MeanRadius = {DataFormatter.Format(physicalProperties.MeanRadius, DataUnit.Distance)}");
                Console.WriteLine($"RotationalPeriod = {DataFormatter.Format(physicalProperties.RotationalPeriod, DataUnit.Time)}");
                Console.WriteLine("");

                Console.WriteLine($"Semi-major axis = {DataFormatter.Format(orbitProperties.SemiMajorAxis, DataUnit.Distance)}");
                Console.WriteLine($"Eccentricity = {orbitProperties.Eccentricity}");
                Console.WriteLine($"Inclination = {DataFormatter.Format(orbitProperties.Inclination, DataUnit.Angle)}");
                Console.WriteLine($"LongitudeOfAscendingNode = {DataFormatter.Format(orbitProperties.LongitudeOfAscendingNode, DataUnit.Angle)}");
                Console.WriteLine($"ArgumentOfPeriapsis = {DataFormatter.Format(orbitProperties.ArgumentOfPeriapsis, DataUnit.Angle)}");
                Console.WriteLine($"TrueAnomaly = {DataFormatter.Format(orbitProperties.TrueAnomaly, DataUnit.Angle)}");
                Console.WriteLine($"MeanAnomaly = {DataFormatter.Format(orbitProperties.MeanAnomaly, DataUnit.Angle)}");
                Console.WriteLine("");
            }

            Console.ReadLine();
        }
    }
}
