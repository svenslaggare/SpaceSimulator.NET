using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Integrations
{
    /// <summary>
    /// Holds physical properties
    /// </summary>
    public class PhysicalProperties
    {
        /// <summary>
        /// The mass (in kg)
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The mean radius (in meters)
        /// </summary>
        public double MeanRadius { get; set; }

        /// <summary>
        /// The rotational period (in seconds)
        /// </summary>
        public double RotationalPeriod { get; set; }
    }

    /// <summary>
    /// Holds orbit properties
    /// </summary>
    public class OrbitProperties
    {
        /// <summary>
        /// The time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// The semi-major axis (in meters)
        /// </summary>
        public double SemiMajorAxis { get; set; }

        /// <summary>
        /// The eccentricity
        /// </summary>
        public double Eccentricity { get; set; }

        /// <summary>
        /// The inclination (in radians)
        /// </summary>
        public double Inclination { get; set; }

        /// <summary>
        /// The longitude of ascending node (in radians)
        /// </summary>
        public double LongitudeOfAscendingNode { get; set; }

        /// <summary>
        /// The argument of periapsis (in radians)
        /// </summary>
        public double ArgumentOfPeriapsis { get; set; }

        /// <summary>
        /// The true anomaly (in radians)
        /// </summary>
        public double TrueAnomaly { get; set; }

        /// <summary>
        /// The mean anomaly (in radians)
        /// </summary>
        public double MeanAnomaly { get; set; }
    }

    /// <summary>
    /// The centers of the coordinate systems
    /// </summary>
    public enum CoordinateSystemCenter
    {
        SunBody
    }
    
    /// <summary>
    /// The reference planes
    /// </summary>
    public enum ReferencePlane
    {
        Ecliptic,
        Body
    }

    /// <summary>
    /// Provides integration against NASA's Horizons system
    /// </summary>
    public sealed class NASAHorizons
    {
        private readonly static IDictionary<string, string> planets = new Dictionary<string, string>()
        {
            { "Mercury", "199" },
            { "Venus", "299" },
            { "Earth", "399" },
            { "Mars", "499" },
            { "Jupiter", "599" },
            { "Saturn", "699" },
            { "Uranus", "799" },
            { "Neptune", "899" },
            { "Pluto", "999" },
        };

        private readonly static Regex massRegex;
        private readonly static Regex[] meanRadiusRegex;
        private readonly static Regex[] rotationalPeriodRegex;
        private readonly static Regex rotationalPeriodRegex2;
        private readonly static Regex rotationalPeriodRegex3;

        static NASAHorizons()
        {
            var decimalRegex = "(-?[0-9]+(\\.[0-9]+)?)";
            var equalSignSep = "[a-z\\, \\(\\)]*";

            massRegex = new Regex($"Mass.*(10\\^[0-9]+){equalSignSep}=\\s+{decimalRegex}", RegexOptions.Compiled);
            meanRadiusRegex = new Regex[]
            {
                new Regex($"Mean radius{equalSignSep}=\\s+{decimalRegex}", RegexOptions.Compiled),
                new Regex($"Volumetric mean radius{equalSignSep}=\\s+{decimalRegex}", RegexOptions.Compiled),
                new Regex($"Radius of Pluto, Rp{equalSignSep}=\\s+{decimalRegex}", RegexOptions.Compiled),
            };

            rotationalPeriodRegex = new Regex[]
            {
                new Regex($"Sidereal period{equalSignSep}=\\s+{decimalRegex}\\s+((hr)|d)?"),
                new Regex($"Sidereal rot. period{equalSignSep}=\\s+{decimalRegex}\\s+((hr)|d)?"),
            };

            rotationalPeriodRegex2 = new Regex($"Rotation period{equalSignSep}=\\s+{decimalRegex}h {decimalRegex}m {decimalRegex}s", RegexOptions.Compiled);
            rotationalPeriodRegex3 = new Regex($"Rotation period{equalSignSep}=\\s+{decimalRegex}", RegexOptions.Compiled);
        }

        /// <summary>
        /// Holds data
        /// </summary>
        public class Data
        {
            /// <summary>
            /// The physical properties
            /// </summary>
            public PhysicalProperties PhysicalProperties { get; set; }

            /// <summary>
            /// The orbit properties
            /// </summary>
            public OrbitProperties OrbitProperties { get; set; }
        }

        private readonly CoordinateSystemCenter coordinateSystemCenter;
        private readonly ReferencePlane referencePlane;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="coordinateSystemCenter">The center of the coordinate systems</param>
        /// <param name="referencePlane">The reference plane</param>
        public NASAHorizons(
            CoordinateSystemCenter coordinateSystemCenter = CoordinateSystemCenter.SunBody,
            ReferencePlane referencePlane = ReferencePlane.Body)
        {
            this.coordinateSystemCenter = coordinateSystemCenter;
            this.referencePlane = referencePlane;
        }

        /// <summary>
        /// Returns the object id for the given planet
        /// </summary>
        /// <param name="planet">The planet</param>
        public string GetObjectId(string planet) => planets[planet];

        /// <summary>
        /// Returns the coordinater system center
        /// </summary>
        private string GetCoordinateSystemCenter(CoordinateSystemCenter coordinateSystemCenter)
        {
            switch (coordinateSystemCenter)
            {
                case CoordinateSystemCenter.SunBody:
                    return "500@10";
            }

            return "";
        }

        /// <summary>
        /// Returns the reference plane
        /// </summary>
        private string GetReferencePlane(ReferencePlane referencePlane)
        {
            switch (referencePlane)
            {
                case ReferencePlane.Ecliptic:
                    return "eclip";
                case ReferencePlane.Body:
                    return "body";
            }

            return "";
        }

        /// <summary>
        /// Indicates if the given string matches the given regex
        /// </summary>
        private bool IsMatch(Regex regex, string input, out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }

        /// <summary>
        /// Indicates if the given string matches any of the given regex
        /// </summary>
        private bool AnyMatch(Regex[] regex, string input, out Match match)
        {
            foreach (var current in regex)
            {
                if (IsMatch(current, input, out match))
                {
                    return true;
                }
            }

            match = null;
            return false;
        }

        /// <summary>
        /// Parses the given double
        /// </summary>
        private double Parse(string text)
        {
            return double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parsers the given physical properties lines
        /// </summary>
        /// <param name="lines">The lines</param>
        private PhysicalProperties ParsePhysicalPropertiesLines(IList<string> lines)
        {
            var properties = new PhysicalProperties();
            var afterDynamicalProperties = false;

            foreach (var line in lines)
            {
                if (line.Contains("DYNAMICAL CHARACTERISTICS"))
                {
                    afterDynamicalProperties = true;
                }

                if (!afterDynamicalProperties)
                {
                    if (IsMatch(massRegex, line, out var massMatch))
                    {
                        var power = massMatch.Groups[1].Value.Split('^');
                        var value = massMatch.Groups[2].Value;
                        properties.Mass = Parse(value) * Math.Pow(Parse(power[0]), Parse(power[1]));
                    }

                    if (AnyMatch(meanRadiusRegex, line, out var meanRadiusMatch))
                    {
                        var value = meanRadiusMatch.Groups[1].Value;
                        properties.MeanRadius = Parse(value) * 1000.0;
                    }

                    if (AnyMatch(rotationalPeriodRegex, line, out var rotationalPeriodMatch))
                    {
                        var value = rotationalPeriodMatch.Groups[1].Value;
                        var unit = "hr";
                        if (rotationalPeriodMatch.Groups[3].Success)
                        {
                            unit = rotationalPeriodMatch.Groups[3].Value;
                        }

                        var scale = 1.0;
                        switch (unit)
                        {
                            case "hr":
                                scale = 60.0 * 60.0;
                                break;
                            case "d":
                                scale = 24.0 * 60.0 * 60.0;
                                break;
                        }

                        properties.RotationalPeriod = Parse(value) * scale;
                    }
                    else if (IsMatch(rotationalPeriodRegex2, line, out var rotationalPeriodMatch2))
                    {
                        var hours = Parse(rotationalPeriodMatch2.Groups[1].Value);
                        var minutes = Parse(rotationalPeriodMatch2.Groups[3].Value);
                        var seconds = Parse(rotationalPeriodMatch2.Groups[5].Value);
                        properties.RotationalPeriod = 60.0 * 60.0 * hours + 60.0 * minutes + seconds;
                    }
                    else if (IsMatch(rotationalPeriodRegex3, line, out var rotationalPeriodMatch3))
                    {
                        var hours = Parse(rotationalPeriodMatch3.Groups[1].Value);
                        properties.RotationalPeriod = 60.0 * 60.0 * hours;
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Parses orbit data
        /// </summary>
        /// <param name="orbitData">The orbit data</param>
        private OrbitProperties ParseOrbitData(IList<string> orbitData)
        {
            string IgnoreLast(string input)
            {
                return input.Substring(0, input.Length - 1);
            }

            var time = DateTime.Parse(orbitData[2] + " " + IgnoreLast(orbitData[3]));
           
            var eccentricity = Parse(IgnoreLast(orbitData[4]));
            var periapsisDistance = Parse(IgnoreLast(orbitData[5]));
            var inclination = Parse(IgnoreLast(orbitData[6]));
            var longitudeOfAscendingNode = Parse(IgnoreLast(orbitData[7]));
            var argumentOfPeriapsis = Parse(IgnoreLast(orbitData[8]));
            var timeOfPeriapsis = Parse(IgnoreLast(orbitData[9]));
            var meanMotion = Parse(IgnoreLast(orbitData[10]));
            var meanAnomaly = Parse(IgnoreLast(orbitData[11]));
            var trueAnomaly = Parse(IgnoreLast(orbitData[12]));
            var semiMajorAxis = Parse(IgnoreLast(orbitData[13]));
            var apoapsisDistance = Parse(IgnoreLast(orbitData[14]));
            var period = Parse(IgnoreLast(orbitData[15]));

            return new OrbitProperties()
            {
                Time = time,
                SemiMajorAxis = semiMajorAxis * 1000,
                Eccentricity = eccentricity,
                Inclination = inclination * MathUtild.Deg2Rad,
                LongitudeOfAscendingNode = longitudeOfAscendingNode * MathUtild.Deg2Rad,
                ArgumentOfPeriapsis = argumentOfPeriapsis * MathUtild.Deg2Rad,
                TrueAnomaly = trueAnomaly * MathUtild.Deg2Rad,
                MeanAnomaly = meanAnomaly * MathUtild.Deg2Rad
            };
        }

        /// <summary>
        /// Parses orbit line
        /// </summary>
        /// <param name="line">The line containing the data</param>
        private OrbitProperties ParseOrbitData(string line)
        {
            return this.ParseOrbitData(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Computes the median
        /// </summary>
        /// <param name="values">The values</param>
        /// <remarks>Assumes that values are sorted</remarks>
        private double Median(IList<double> values)
        {
            if (values.Count % 2 == 0)
            {
                return 0.5 * (values[values.Count / 2 - 1] + values[values.Count / 2]);
            }
            else
            {
                return values[values.Count / 2];
            }
        }

        /// <summary>
        /// Calcuilates the mean orbit properties
        /// </summary>
        /// <param name="orbits">The orbits</param>
        private OrbitProperties MeanOrbitProperties(IList<OrbitProperties> orbits)
        {
            var semiMajorAxis = 0.0;
            var eccentricity = 0.0;
            var inclinationX = 0.0;
            var inclinationY = 0.0;
            var longitudeOfAscendingNodeX = 0.0;
            var longitudeOfAscendingNodeY = 0.0;
            var argumentOfPeriapsisX = 0.0;
            var argumentOfPeriapsisY = 0.0;

            foreach (var orbit in orbits)
            {
                semiMajorAxis += orbit.SemiMajorAxis;
                eccentricity += orbit.Eccentricity;

                inclinationX += Math.Cos(orbit.Inclination);
                inclinationY += Math.Sin(orbit.Inclination);

                longitudeOfAscendingNodeX += Math.Cos(orbit.LongitudeOfAscendingNode);
                longitudeOfAscendingNodeY += Math.Sin(orbit.LongitudeOfAscendingNode);

                argumentOfPeriapsisX += Math.Cos(orbit.ArgumentOfPeriapsis);
                argumentOfPeriapsisY += Math.Sin(orbit.ArgumentOfPeriapsis);
            }

            semiMajorAxis /= orbits.Count;
            eccentricity /= orbits.Count;
            inclinationX /= orbits.Count;
            inclinationY /= orbits.Count;
            longitudeOfAscendingNodeX /= orbits.Count;
            longitudeOfAscendingNodeY /= orbits.Count;
            argumentOfPeriapsisX /= orbits.Count;
            argumentOfPeriapsisY /= orbits.Count;

            return new OrbitProperties()
            {
                SemiMajorAxis = semiMajorAxis,
                Eccentricity = eccentricity,
                Inclination = Math.Atan2(inclinationY, inclinationX),
                LongitudeOfAscendingNode = Math.Atan2(longitudeOfAscendingNodeY, longitudeOfAscendingNodeX),
                ArgumentOfPeriapsis = Math.Atan2(argumentOfPeriapsisY, argumentOfPeriapsisX),
            };
        }

        /// <summary>
        /// Fetches data for the given planet at the given time
        /// </summary>
        /// <param name="planet">The name of the planet</param>
        /// <param name="time">The time to fetch. If not specified, calculates the mean elements</param>
        public async Task<Data> FetchData(string planet, DateTime? time = null)
        {
            using (var client = new PrimS.Telnet.Client("horizons.jpl.nasa.gov", 6775, new System.Threading.CancellationToken()))
            {
                async Task SendCommand(string command)
                {
                    await client.WriteLine(command);
                    await client.ReadAsync();
                }

                //Read until we can send command
                while (true)
                {
                    var text = await client.ReadAsync();
                    if (text.Contains("Horizons>"))
                    {
                        break;
                    }
                }

                async Task<PhysicalProperties> GetPhysicalProperties()
                {
                    //Send command to get physical properties
                    await SendCommand(this.GetObjectId(planet));

                    //Get the lines
                    var text = "";
                    while (true)
                    {
                        text += await client.ReadAsync();
                        if (text.Contains("> Select"))
                        {
                            break;
                        }
                    }

                    var lines = text.Split('\n').Select(x => x.Trim()).ToList();

                    var start = -1;
                    var end = -1;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (start == -1 && lines[i].Contains("**********"))
                        {
                            start = i;
                        }
                        else if (start != -1 && lines[i].Contains("**********"))
                        {
                            end = i;
                        }
                    }

                    return this.ParsePhysicalPropertiesLines(lines.GetRange(start + 1, end - start - 1));
                }

                async Task<OrbitProperties> GetOrbitProperties()
                {
                    //Send the command to get the orbit data
                    await SendCommand("E");
                    await SendCommand("e");
                    await SendCommand(this.GetCoordinateSystemCenter(this.coordinateSystemCenter));
                    await SendCommand("y");
                    await SendCommand(this.GetReferencePlane(this.referencePlane));

                    if (time.HasValue)
                    {
                        await SendCommand(time.Value.ToString());
                        await SendCommand(time.Value.AddHours(1).ToString());
                        await SendCommand("1h");
                    }
                    else
                    {
                        await SendCommand(new DateTime(2000 - 100, 1, 1, 12, 0, 0).ToString());
                        await SendCommand(new DateTime(2000, 1, 1, 12, 0, 0).ToString());
                        //var now = new DateTime(2017, 1, 1, 12, 0, 0);
                        //await SendCommand(now.AddYears(-100).ToString());
                        //await SendCommand(now.ToString());
                        await SendCommand("100d");
                    }

                    await SendCommand("n");
                    await SendCommand("J2000");
                    await SendCommand("1");
                    await SendCommand("YES");
                    await SendCommand("YES");
                    await SendCommand("ABS");

                    //Get the lines
                    List<string> orbitLines = null;
                    var text = "";

                    while (true)
                    {
                        text += await client.ReadAsync();
                        if (text.Contains("$$EOE"))
                        {
                            var lines = text.Split('\n').Select(x => x.Trim()).ToList();
                            for (int i = 0; i < lines.Count; i++)
                            {
                                if (lines[i].Contains("$SOE"))
                                {
                                    orbitLines = lines.GetRange(i + 1, lines.Count - i - 1).TakeWhile(x => !x.Contains("$$EOE")).ToList();
                                }
                            }
                            break;
                        }
                    }

                    if (time.HasValue)
                    {
                        return this.ParseOrbitData(orbitLines[0]);
                    }
                    else
                    {
                        var orbits = orbitLines.Select(line => this.ParseOrbitData(line)).ToList();
                        return this.MeanOrbitProperties(orbits);
                    }
                }

                var physicalProperties = await GetPhysicalProperties();
                var orbitProperties = await GetOrbitProperties();
                return new Data()
                {
                    PhysicalProperties = physicalProperties,
                    OrbitProperties = orbitProperties
                };
            }
        }

        /// <summary>
        /// Fetches all the given planets if they are not already cached
        /// </summary>
        /// <param name="planets">The planets</param>
        /// <param name="time">The time</param>
        /// <param name="debug">Indicates if debug information is printed</param>
        public IDictionary<string, Data> FetchAllCached(IList<string> planets, DateTime time, bool debug = false)
        {
            var data = new Dictionary<string, Data>();
            var fileName = $"horizons-{time.ToString("yyyy-MM-dd-HH-mm-ss")}.json";
            if (File.Exists(fileName))
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Data>>(File.ReadAllText(fileName));
            }

            foreach (var planet in planets)
            {
                if (!data.ContainsKey(planet))
                {
                    if (debug)
                    {
                        Console.WriteLine($"Started fetching {planet}...");
                    }

                    var task = this.FetchData(planet, time);
                    task.Wait();
                    data.Add(planet, task.Result);

                    if (debug)
                    {
                        Console.WriteLine($"Fetched fetching {planet}...");
                    }
                }
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(fileName, json);

            return data;
        }
    }
}
