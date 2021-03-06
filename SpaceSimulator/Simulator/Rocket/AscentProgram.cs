﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;
using SpaceSimulator.Simulator.OrbitSimulators;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a control program for ascent
    /// </summary>
    public class AscentProgram : BaseControlProgram
    {
        private readonly Orbit targetOrbit;
        private readonly ITextOutputWriter textOutputWriter;

        private State state;

        private readonly double pitchManeuverStartAltitude;
        private readonly double pitchManeuverStopAltitude;

        private bool pitchStarted = false;
        private bool pitchCompleted = false;

        private DateTime lastTime;

        /// <summary>
        /// Creates a new ascent control program
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="targetOrbit">The target orbit</param>
        /// <param name="pitchEndAltitude">The altitude when to start the pitch maneuver</param>
        /// <param name="pitchStartAltitude">The altitude to stop the pitch maneuver</param>
        /// <param name="textOutputWriter">The text output writer</param>
        public AscentProgram(RocketObject rocketObject, Orbit targetOrbit, double pitchStartAltitude, double pitchEndAltitude, ITextOutputWriter textOutputWriter)
            : base(rocketObject)
        {
            this.targetOrbit = targetOrbit;
            this.state = State.InitialAscent;
            this.textOutputWriter = textOutputWriter;

            this.pitchManeuverStartAltitude = pitchStartAltitude;
            this.pitchManeuverStopAltitude = pitchEndAltitude;
        }

        /// <summary>
        /// Calculates the horizontal thrust direction
        /// </summary>
        private Vector3d HorizontalThrustDirection()
        {
            return OrbitHelpers.SphereNormal(this.rocketObject.PrimaryBody, this.rocketObject.Latitude, this.rocketObject.Longitude + MathUtild.Pi * 0.5);
        }

        /// <summary>
        /// Calculates the vertical thrust direction
        /// </summary>
        private Vector3d VerticalThrustDirection()
        {
            return OrbitHelpers.SphereNormal(this.rocketObject.PrimaryBody, this.rocketObject.Latitude, this.rocketObject.Longitude);
        }

        public enum State
        {
            InitialAscent,
            Turning,
            Coast,
            Circularizing,
            InOrbit,
            Failed
        }

        /// <summary>
        /// Indicates if the program is completed
        /// </summary>
        public override bool IsCompleted
        {
            get { return this.state == State.InOrbit || this.state == State.Failed; }
        }

        /// <summary>
        /// Starts the program
        /// </summary>
        /// <param name="totalTime">The current time</param>
        public override void Start(double totalTime)
        {
            this.absoluteThrustDirection = this.VerticalThrustDirection();
        }

        /// <summary>
        /// Logs the status of the ascent program
        /// </summary>
        /// <param name="message">The message</param>
        private void LogStatus(string message)
        {
            textOutputWriter.WriteLine(this.rocketObject.Name, message);
        }

        public override void Update(double totalTime, double timeStep)
        {
            var prograde = (this.rocketObject.Velocity - this.rocketObject.PrimaryBody.Velocity).Normalized();
            var gravityAccelerationDir = (this.rocketObject.Position - this.rocketObject.PrimaryBody.Position).Normalized();
            this.absoluteThrustDirection = prograde;

            switch (this.state)
            {
                case State.InitialAscent:
                    if (this.rocketObject.PrimaryBody.Altitude(this.rocketObject.Position) >= this.pitchManeuverStartAltitude && this.state == State.InitialAscent)
                    {
                        this.state = State.Turning;
                        this.LogStatus("Gravity turn started.");
                    }
                    break;
                case State.Turning:
                    var altitude = this.rocketObject.PrimaryBody.Altitude(this.rocketObject.Position);
                    var currentOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.rocketObject);

                    if (altitude >= this.pitchManeuverStartAltitude && altitude <= this.pitchManeuverStopAltitude)
                    {
                        //var turnAmount = 0.70;
                        //this.ThrustDirection = Vector3d.Transform(
                        //    this.HorizontalThrustDirection(),
                        //    Matrix3x3d.RotationY(MathUtild.Deg2Rad * turnAmount * 90.0));

                        var radial = Vector3d.Transform(prograde, Matrix3x3d.RotationY(MathUtild.Pi / 2));
                        var pitchAxis = Vector3d.Cross(radial, prograde);
                        var turnAngle = 4.0 * MathUtild.Deg2Rad;
                        this.absoluteThrustDirection = Vector3d.Transform(prograde, Matrix3x3d.RotationAxis(pitchAxis, turnAngle));

                        if (!this.pitchStarted)
                        {
                            this.LogStatus("Pitch maneuver started.");
                            this.pitchStarted = true;
                        }
                    }
                    else
                    {
                        if (!this.pitchCompleted)
                        {
                            this.LogStatus("Pitch maneuver completed.");
                            this.pitchCompleted = true;
                        }

                        if (altitude >= 0.9 * currentOrbitPosition.Orbit.RelativeApoapsis && currentOrbitPosition.TimeToApoapsis() <= 100.0)
                        {
                            this.absoluteThrustDirection = (prograde + 0.1 * gravityAccelerationDir).Normalized();
                        }
                        else
                        {
                            this.absoluteThrustDirection = prograde;
                        }

                        //var minTime = 200.0;
                        //var turnSpeed = 0.1;
                        //if (altitude >= 0.9 * currentOrbitPosition.Orbit.RelativeApoapsis && currentOrbitPosition.TimeToApoapsis() <= minTime)
                        //{
                        //    var turnAmount = (1.0 - currentOrbitPosition.TimeToApoapsis() / minTime) * (1.0 - turnSpeed) + turnSpeed;
                        //    this.ThrustDirection = (prograde + 0.1 * turnAmount * gravityAccelerationDir).Normalized();
                        //}
                        //else
                        //{
                        //    this.ThrustDirection = prograde;
                        //}
                    }

                    if (currentOrbitPosition.TrueAnomaly > 190.0 * MathUtild.Deg2Rad && currentOrbitPosition.TimeToApoapsis() >= currentOrbitPosition.Orbit.Period * 0.8)
                    {
                        this.state = State.Failed;
                        this.LogStatus("Failed to reach orbit.");
                    }

                    if (currentOrbitPosition.Orbit.IsBound && currentOrbitPosition.Orbit.Apoapsis >= this.targetOrbit.Apoapsis)
                    {
                        this.rocketObject.StopEngines();
                        this.state = State.Coast;
                        this.LogStatus("Engine shutdown.");
                        this.LogStatus("Coasting.");
                    }

                    if (this.rocketObject.CurrentStage.Number == 0 
                        && this.rocketObject.CurrentStage.FuelMassRemainingRatio <= 0.1)
                    {
                        this.rocketObject.Stage(spentStageObject =>
                        {
                            spentStageObject.StartProgram(new LandingProgram(spentStageObject, this.textOutputWriter));
                        });
                    }
                    break;
                case State.Coast:
                    if (OrbitPosition.CalculateOrbitPosition(this.rocketObject).TimeToApoapsis() <= 10.0)
                    {
                        this.rocketObject.StartEngines();
                        this.state = State.Circularizing;
                        this.LogStatus("Engine started.");
                        this.LogStatus("Circularizing.");
                    }
                    break;
                case State.Circularizing:
                    var currentOrbit = OrbitPosition.CalculateOrbitPosition(this.rocketObject);
                    if (Math.Abs(currentOrbit.Orbit.Eccentricity - this.targetOrbit.Eccentricity) <= 0.01 &&
                        OrbitPosition.CalculateOrbitPosition(this.rocketObject).Orbit.RelativePeriapsis >= this.targetOrbit.RelativePeriapsis * 0.99)
                    {
                        this.rocketObject.StopEngines();
                        this.LogStatus("Engine shutdown.");
                        this.LogStatus("In orbit.");
                        this.LogStatus(DataFormatter.Format(totalTime, DataUnit.Time));
                        this.LogStatus(DataFormatter.Format(this.rocketObject.Mass, DataUnit.Mass, useBase10: true));
                        this.state = State.InOrbit;
                        //this.rocketObject.Stage();
                    }
                    break;
            }

            this.SetFaceThrustDirection();
            //this.FaceDirection(totalTime, this.absoluteThrustDirection);
            //this.FaceDirection(totalTime, prograde);

            //var gimbalAngle = MathHelpers.AngleBetween(this.ThrustDirection, Vector3d.ForwardRH);
            //if (Math.Abs(gimbalAngle) >= 1E-4)
            //{
            //    var relativeThrustDirection = this.ThrustDirection;
            //    var sign = Math.Sign(gimbalAngle);
            //    this.absoluteThrustDirection = RocketHelpers.RelativeToAbsoluteThrustDirection(
            //        this.rocketObject,
            //        Vector3d.Transform(relativeThrustDirection, Quaterniond.RotationAxis(Vector3d.Up, sign * 0.1 * MathUtild.Deg2Rad)));
            //}

            //if ((DateTime.UtcNow - this.lastTime).TotalSeconds >= 0.25 && this.rocketObject.IsEngineRunning)
            //{
            //    //var thrustAngle = MathHelpers.AngleBetween(this.absoluteThrustDirection, MathHelpers.Normalized(prograde));
            //    //if (thrustAngle >= 1E-4)
            //    //{
            //    //    this.LogStatus($"Thrust angle: {MathUtild.Rad2Deg * thrustAngle}");
            //    //    this.lastTime = DateTime.UtcNow;
            //    //}

            //    var gimbalAngle = MathHelpers.AngleBetween(this.ThrustDirection, Vector3d.ForwardRH);
            //    if (gimbalAngle >= 1E-4)
            //    {
            //        this.LogStatus($"Gimbal angle: {MathUtild.Rad2Deg * gimbalAngle}");
            //        this.lastTime = DateTime.UtcNow;
            //    }
            //}
        }

        /// <summary>
        /// Simulates the ascent of the given object using the given pitch maneuver
        /// </summary>
        /// <param name="orbitSimulator">The orbit simulator</param>
        /// <param name="rocketObject">The rocket object</param>
        /// <param name="targetOrbit">The target object</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="pitchStart">The start of the pitch maneuver</param>
        /// <param name="pitchEnd">The end of the pitch maneuver</param>
        /// <returns>(totalTime, mass, orbit)</returns>
        private static (double, double, Orbit) SimulateAscent(IOrbitSimulator orbitSimulator, RocketObject rocketObject, Orbit targetOrbit, double timeStep, double pitchStart, double pitchEnd)
        {
            var textOutputWriter = new NullTextOutputWriter();
            var currentObject = new RocketObject(
                rocketObject.Name,
                rocketObject.Mass,
                rocketObject.PrimaryBody,
                rocketObject.State,
                rocketObject.ReferenceOrbit,
                rocketObject.Stages.Clone(),
                textOutputWriter);

            var ascentProgram = new AscentProgram(currentObject, targetOrbit, pitchStart, pitchEnd, textOutputWriter);
            currentObject.SetControlProgram(ascentProgram);

            currentObject.CheckImpacted(rocketObject.State.Time);
            currentObject.StartProgram();

            var otherObjects = new List<PhysicsObject>();
            var maxTime = rocketObject.State.Time + 2.0 * 60.0 * 60.0;
            double totalTime = 0.0;
            for (totalTime = rocketObject.State.Time; totalTime <= maxTime; totalTime += timeStep)
            {
                orbitSimulator.Update(totalTime, timeStep, currentObject);
                currentObject.Update(totalTime, timeStep);

                if (ascentProgram.IsCompleted)
                {
                    break;
                }
            }

            return (totalTime, currentObject.Mass, OrbitPosition.CalculateOrbitPosition(currentObject).Orbit);
        }

        /// <summary>
        /// Selects the best ascent
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="mass">The mass when reached orbit</param>
        /// <param name="orbit">The reached orbit</param>
        /// <param name="targetOrbit">The target orbit</param>
        /// <param name="pitchStart">The start of the pitch maneuver</param>
        /// <param name="pitchEnd">The end of the pitch maneuver</param>
        /// <param name="bestPitchStart">The best start of pitch</param>
        /// <param name="bestPitchEnd">The best end of pitch</param>
        /// <param name="bestMass">The mass of the best maneuver</param>
        /// <param name="bestTime">The time of the best maneuver</param>
        /// <returns>True if valid else false</returns>
        private static bool SelectBestAscent(
            double totalTime,
            double mass,
            Orbit orbit,
            Orbit targetOrbit,
            double pitchStart,
            double pitchEnd,
            ref double bestPitchStart,
            ref double bestPitchEnd,
            ref double bestMass,
            ref double bestTime)
        {
            if (Math.Abs(orbit.Eccentricity - targetOrbit.Eccentricity) < 1E-2 && Math.Abs(orbit.SemiMajorAxis - targetOrbit.SemiMajorAxis) < 10E3)
            {
                if (mass > bestMass)
                {
                    bestPitchStart = pitchStart;
                    bestPitchEnd = pitchEnd;
                    bestTime = totalTime;
                    bestMass = mass;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the optimal pitch maneuver in parallel parallized on the start
        /// </summary>
        private static (double, double, double, double) CalculateOptimalPitchManeuverParallelStart(
            IOrbitSimulator orbitSimulator,
            RocketObject rocketObject,
            double minAltitude,
            Orbit targetOrbit,
            double deltaAltitude,
            double timeStep,
            double maxPitchStart,
            double maxPitchEnd,
            int numTasks)
        {
            var bestPitchStart = 0.0;
            var bestPitchEnd = 0.0;
            var bestMass = 0.0;
            var bestTime = 0.0;

            var tasks = new List<Task<(double, double, double, double)>>();

            var pitchStartRange = (maxPitchStart - minAltitude) / numTasks;
            for (int i = 0; i < numTasks; i++)
            {
                var pitchStartStart = minAltitude + (pitchStartRange / numTasks) * i;
                var pitchStartEnd = minAltitude + (pitchStartRange / numTasks) * (i + 1);

                tasks.Add(new Task<(double, double, double, double)>(state =>
                {
                    var stateTuple = ((double, double))state;
                    var taskBestPitchStart = 0.0;
                    var taskBestPitchEnd = 0.0;
                    var taskBestMass = 0.0;
                    var taskBestTime = 0.0;

                    for (double pitchStart = stateTuple.Item1; pitchStart <= stateTuple.Item2; pitchStart += deltaAltitude)
                    {
                        for (double pitchEnd = pitchStart + deltaAltitude; pitchEnd <= maxPitchEnd; pitchEnd += deltaAltitude)
                        {
                            (var totalTime, var mass, var orbit) = SimulateAscent(orbitSimulator, rocketObject, targetOrbit, timeStep, pitchStart, pitchEnd);
                            SelectBestAscent(totalTime, mass, orbit, targetOrbit, pitchStart, pitchEnd, ref taskBestPitchStart, ref taskBestPitchEnd, ref taskBestMass, ref taskBestTime);
                        }
                    }

                    return (taskBestPitchStart, taskBestPitchEnd, taskBestMass, taskBestTime);
                }, (pitchStartStart, pitchStartEnd)));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WhenAll(tasks).Wait();

            foreach (var task in tasks)
            {
                if (task.Result.Item4 > bestMass)
                {
                    (bestPitchStart, bestPitchEnd, bestMass, bestTime) = task.Result;
                }
            }

            return (bestPitchStart, bestPitchEnd, bestMass, bestTime);
        }

        /// <summary>
        /// Calculates the optimal pitch maneuver in parallel parallized on the end
        /// </summary>
        private static (double, double, double, double) CalculateOptimalPitchManeuverParallelEnd(
            IOrbitSimulator orbitSimulator,
            RocketObject rocketObject,
            double minAltitude,
            Orbit targetOrbit,
            double deltaAltitude,
            double timeStep,
            double maxPitchStart,
            double maxPitchEnd,
            int numTasks)
        {
            var bestPitchStart = 0.0;
            var bestPitchEnd = 0.0;
            var bestMass = 0.0;
            var bestTime = 0.0;

            for (double pitchStart = minAltitude; pitchStart <= maxPitchStart; pitchStart += deltaAltitude)
            {
                var pitchEndRange = maxPitchEnd - (pitchStart + deltaAltitude) / numTasks;
                var tasks = new List<Task<(double, double, double, double)>>();

                for (int i = 0; i < numTasks; i++)
                {
                    var pitchEndStart = pitchStart + deltaAltitude + (pitchEndRange / numTasks) * i;
                    var pitchEndEnd = pitchStart + deltaAltitude + (pitchEndRange / numTasks) * (i + 1);

                    tasks.Add(new Task<(double, double, double, double)>(state =>
                    {
                        var stateTuple = ((double, double))state;
                        var taskBestPitchStart = 0.0;
                        var taskBestPitchEnd = 0.0;
                        var taskBestMass = 0.0;
                        var taskBestTime = 0.0;

                        for (double pitchEnd = pitchEndStart; pitchEnd <= pitchEndEnd; pitchEnd += deltaAltitude)
                        {
                            (var totalTime, var mass, var orbit) = SimulateAscent(orbitSimulator, rocketObject, targetOrbit, timeStep, pitchStart, pitchEnd);
                            SelectBestAscent(totalTime, mass, orbit, targetOrbit, pitchStart, pitchEnd, ref taskBestPitchStart, ref taskBestPitchEnd, ref taskBestMass, ref taskBestTime);
                        }

                        return (taskBestPitchStart, taskBestPitchEnd, taskBestMass, taskBestTime);
                    }, (pitchEndStart, pitchEndEnd)));
                }

                foreach (var task in tasks)
                {
                    task.Start();
                }

                Task.WhenAll(tasks).Wait();

                foreach (var task in tasks)
                {
                    if (task.Result.Item4 > bestMass)
                    {
                        (bestPitchStart, bestPitchEnd, bestMass, bestTime) = task.Result;
                    }
                }
            }

            return (bestPitchStart, bestPitchEnd, bestMass, bestTime);
        }

        /// <summary>
        /// Calculates the opitmal start and end of the pitch maneuver
        /// </summary>
        /// <param name="orbitSimulator">The orbit simulator</param>
        /// <param name="rocketObject">The rocket object to calculate for</param>
        /// <param name="minAltitude">The minimum start of the pitch maneuver</param>
        /// <param name="targetOrbit">The target orbit</param>
        public static (double, double) CalculateOptimalPitchManeuver(IOrbitSimulator orbitSimulator, RocketObject rocketObject, double minAltitude, Orbit targetOrbit)
        {
            var bestPitchStart = 0.0;
            var bestPitchEnd = 0.0;
            var bestMass = 0.0;
            var bestTime = 0.0;

            var deltaAltitude = 200.0;
            var timeStep = 0.02;
            timeStep *= 2.0;

            //var maxPitchStart = Math.Max(6E3, minAltitude);
            var maxPitchStart = minAltitude;
            var maxPitchEnd = Math.Max(18E3, minAltitude);

            var startTime = DateTime.UtcNow;
            var numTasks = 8;

            //for (double pitchStart = minAltitude; pitchStart <= maxPitchStart; pitchStart += deltaAltitude)
            //{
            //    for (double pitchEnd = pitchStart + deltaAltitude; pitchEnd <= maxPitchEnd; pitchEnd += deltaAltitude)
            //    {
            //        (var totalTime, var mass, var orbit) = SimulateAscent(orbitSimulator, rocketObject, targetOrbit, timeStep, pitchStart, pitchEnd);

            //        if (SelectBestAscent(totalTime, mass, orbit, targetOrbit, pitchStart, pitchEnd, ref bestPitchStart, ref bestPitchEnd, ref bestMass, ref bestTime))
            //        {
            //            //goto done;
            //        }
            //    }
            //}

            //Parallelize on pitch end
            (bestPitchStart, bestPitchEnd, bestMass, bestTime) = CalculateOptimalPitchManeuverParallelEnd(
                orbitSimulator,
                rocketObject,
                minAltitude,
                targetOrbit,
                deltaAltitude,
                timeStep,
                maxPitchStart,
                maxPitchEnd,
                numTasks);

            //Parallelize on pitch start
            //(bestPitchStart, bestPitchEnd, bestMass, bestTime) = CalculateOptimalPitchManeuverParallelStart(
            //    orbitSimulator,
            //    rocketObject,
            //    minAltitude,
            //    targetOrbit,
            //    deltaAltitude,
            //    timeStep,
            //    maxPitchStart,
            //    maxPitchEnd,
            //    numTasks);

            Console.WriteLine($"Optimal ascent maneuver computed in {(DateTime.UtcNow - startTime).TotalSeconds} seconds.");
            Console.WriteLine("Start pitch maneuver: " + DataFormatter.Format(bestPitchStart, DataUnit.Distance));
            Console.WriteLine("End pitch maneuver: " + DataFormatter.Format(bestPitchEnd, DataUnit.Distance));
            Console.WriteLine("Mass: " + DataFormatter.Format(bestMass, DataUnit.Mass, useBase10: true));
            Console.WriteLine("Reached orbit in: " + DataFormatter.Format(bestTime, DataUnit.Time));

            return (bestPitchStart, bestPitchEnd);
        }
    }
}
