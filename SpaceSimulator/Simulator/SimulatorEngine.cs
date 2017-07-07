using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SharpDX;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Physics.Rocket;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Simulator.OrbitSimulators;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// The simulation modes
    /// </summary>
    public enum PhysicsSimulationMode
    {
        PerturbationCowell,
        KeplerProblemUniversalVariable
    }

    /// <summary>
    /// The type of the simulation event
    /// </summary>
    public enum SimulationEventType
    {
        SphereOfInfluenceChange,
        Crash
    }

    /// <summary>
    /// Represents a simulation event
    /// </summary>
    public sealed class SimulationEvent : IComparable<SimulationEvent>
    {
        /// <summary>
        /// The type of the event
        /// </summary>
        public SimulationEventType Type { get; }

        /// <summary>
        /// The object
        /// </summary>
        public PhysicsObject Object { get; }

        /// <summary>
        /// The time of the event
        /// </summary>
        public double Time { get; }

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="type">The type of the event</param>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="time">The time</param>
        public SimulationEvent(SimulationEventType type, PhysicsObject physicsObject, double time)
        {
            this.Type = type;
            this.Object = physicsObject;
            this.Time = time;
        }

        public int CompareTo(SimulationEvent other)
        {
            return this.Time.CompareTo(other.Time);
        }

        public override string ToString()
        {
            string eventType = "";
            switch (this.Type)
            {
                case SimulationEventType.SphereOfInfluenceChange:
                    eventType = "Change of sphere-of-influence";
                    break;
                case SimulationEventType.Crash:
                    eventType = "Crash";
                    break;
            }

            return this.Object.Name + ": " + eventType + " at " + DataFormatter.Format(this.Time, DataUnit.Time);
        }
    }

    /// <summary>
    /// Represents a maneuver for the simulator
    /// </summary>
    public sealed class SimulationManeuever : IComparable<SimulationManeuever>
    {
        /// <summary>
        /// The object
        /// </summary>
        public PhysicsObject Object { get; }

        /// <summary>
        /// The maneuver
        /// </summary>
        public OrbitalManeuver Maneuver { get; }

        /// <summary>
        /// The prograde component of the delta V
        /// </summary>
        public double Prograde { get; private set; }

        /// <summary>
        /// The normal component of the delta V
        /// </summary>
        public double Normal { get; private set; }

        /// <summary>
        /// The radial component of the delta V
        /// </summary>
        public double Radial { get; private set; }

        /// <summary>
        /// Creates a new maneuver
        /// </summary>
        /// <param name="physicsObject">The object to apply for</param>
        /// <param name="manuever">The maneuver</param>
        public SimulationManeuever(PhysicsObject physicsObject, OrbitalManeuver manuever)
        {
            this.Object = physicsObject;
            this.Maneuver = manuever;
        }

        /// <summary>
        /// Computes the maneuver components
        /// </summary>
        /// <param name="stateAtBurn">The state of the object at the burn</param>
        public void ComputeComponents(ref ObjectState stateAtBurn)
        {
            (this.Prograde, this.Normal, this.Radial)
                = OrbitHelpers.ComputeVelocityComponents(ref stateAtBurn, this.Maneuver.DeltaVelocity);
        }

        public int CompareTo(SimulationManeuever other)
        {
            return this.Maneuver.ManeuverTime.CompareTo(other.Maneuver.ManeuverTime);
        }

        public override string ToString()
        {
            return this.Object.Name + " - " + this.Maneuver.ToString();
        }
    }

    /// <summary>
    /// Represents the simulation engine
    /// </summary>
    public sealed class SimulatorEngine : ISimulatorEngine
    {
        private readonly IList<PhysicsObject> objects = new List<PhysicsObject>();
        private readonly IList<PhysicsObject> naturalObjects = new List<PhysicsObject>();
        private readonly IList<PhysicsObject> newObjects = new List<PhysicsObject>();
        private NaturalSatelliteObject objectOfReference;

        private double totalTime;
        private double timeStep = 0.02;
        private readonly IList<double> subTimeIntervals = new List<double>();

        private readonly INumericIntegrator numericIntegrator = new RungeKutta4Integrator();
        private readonly IKeplerProblemSolver keplerProblemSolver = new KeplerProblemUniversalVariableSolver();
        private readonly IGaussProblemSolver gaussProblemSolver = new GaussProblemUniversalVariableSolver();

        private PhysicsSimulationMode simulationMode;
        private readonly IDictionary<PhysicsSimulationMode, IOrbitSimulator> orbitSimulators = new Dictionary<PhysicsSimulationMode, IOrbitSimulator>();
        private IOrbitSimulator currentOrbitSimulator;

        private readonly IList<SimulationEvent> events = new List<SimulationEvent>();
        private readonly IList<SimulationEvent> executedEvents = new List<SimulationEvent>();
        private readonly IDictionary<PhysicsObject, SimulationEvent> soiChanges = new Dictionary<PhysicsObject, SimulationEvent>();
        private bool addedEvent = false;

        private readonly IList<SimulationManeuever> maneuvers = new List<SimulationManeuever>();
        private readonly IList<SimulationManeuever> executedManeuvers = new List<SimulationManeuever>();
        private readonly IDictionary<PhysicsObject, double> sphereOfInfluences = new Dictionary<PhysicsObject, double>();
        private readonly double maneuverTimeEpsilon = 1E-6;

        /// <summary>
        /// Fires when a new object is added
        /// </summary>
        public event EventHandler<PhysicsObject> ObjectAdded;

        /// <summary>
        /// The text output writer
        /// </summary>
        public ITextOutputWriter TextOutputWriter { get; } = new ConsoleTextOutputWriter();

        /// <summary>
        /// The simulation speed
        /// </summary>
        public int SimulationSpeed { get; set; }

        /// <summary>
        /// Creates a new simulator engine
        /// </summary>
        /// <param name="objects">The objects</param>
        public SimulatorEngine(IList<PhysicsObject> objects)
        {
            foreach (var current in objects)
            {
                this.AddObject(current);
            }

            this.orbitSimulators.Add(PhysicsSimulationMode.PerturbationCowell, new CowellSimulator(this.numericIntegrator));
            this.orbitSimulators.Add(PhysicsSimulationMode.KeplerProblemUniversalVariable, new TwoBodySimulator(this.keplerProblemSolver));
            this.SimulationMode = PhysicsSimulationMode.PerturbationCowell;
        }

        /// <summary>
        /// Returns the numeric integrator
        /// </summary>
        public INumericIntegrator NumericIntegrator
        {
            get { return this.numericIntegrator; }
        }

        /// <summary>
        /// Returns the kepler problem solver
        /// </summary>
        public IKeplerProblemSolver KeplerProblemSolver
        {
            get { return this.keplerProblemSolver; }
        }

        /// <summary>
        /// Returns the gauss problem solver
        /// </summary>
        public IGaussProblemSolver GaussProblemSolver
        {
            get { return this.gaussProblemSolver; }
        }


        /// <summary>
        /// The simulation mode
        /// </summary>
        public PhysicsSimulationMode SimulationMode
        {
            get { return this.simulationMode; }
            set
            {
                this.simulationMode = value;
                this.currentOrbitSimulator = this.orbitSimulators[value];
            }
        }

        /// <summary>
        /// Returns the objects
        /// </summary>
        public IList<PhysicsObject> Objects
        {
            get { return this.objects; }
        }

        /// <summary>
        /// Returns the object of reference
        /// </summary>
        public NaturalSatelliteObject ObjectOfReference
        {
            get
            {
                if (this.objectOfReference == null)
                {
                    this.objectOfReference = (NaturalSatelliteObject)this.Objects.FirstOrDefault(x => x.IsObjectOfReference);
                }

                return this.objectOfReference;
            }
        }

        /// <summary>
        /// Returns the total time the system has been running
        /// </summary>
        public double TotalTime
        {
            get { return this.totalTime; }
        }

        /// <summary>
        /// Returns the total time the system has been running as a formatted string
        /// </summary>
        public string TotalTimeString
        {
            get { return DataFormatter.Format(this.totalTime, DataUnit.Time, 0); }
        }

        /// <summary>
        /// Returns the scheduled maneuvers
        /// </summary>
        public IList<SimulationManeuever> Maneuvers
        {
            get { return this.maneuvers; }
        }

        /// <summary>
        /// Returns the events
        /// </summary>
        public IList<SimulationEvent> Events
        {
            get { return this.events; }
        }

        /// <summary>
        /// Returns the given orbit simulator
        /// </summary>
        /// <param name="mode">The simulator</param>
        public IOrbitSimulator GetSimulator(PhysicsSimulationMode mode)
        {
            return this.orbitSimulators[mode];
        }

        /// <summary>
        /// Adds the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        private void AddObject(PhysicsObject physicsObject)
        {
            this.objects.Add(physicsObject);

            if (physicsObject.Type != PhysicsObjectType.ArtificialSatellite)
            {
                this.naturalObjects.Add(physicsObject);
            }

            this.CalculateSphereOfInfluence(physicsObject);
        }

        /// <summary>
        /// Adds the given object to the list of objects to add
        /// </summary>
        /// <param name="physicsObject">The object</param>
        private void AddNewObject(PhysicsObject physicsObject)
        {
            this.newObjects.Add(physicsObject);
        }

        /// <summary>
        /// Adds a planet object in the given orbit
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="radius">The radius of the object</param>
        /// <param name="rotationalPeriod">The rotational period</param>
        /// <param name="axisOfRotation">The axis-of-rotation</param>
        /// <param name="atmosphericModel">The atmospheric model</param>
        /// <param name="orbitPosition">The position in the orbit</param>
        /// <returns>The created object</returns>
        public PlanetObject AddPlanetInOrbit(
            string name,
            double mass,
            double radius,
            double rotationalPeriod,
            Vector3d axisOfRotation,
            IAtmosphericModel atmosphericModel,
            OrbitPosition orbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            var primaryBodyState = orbit.PrimaryBody.State;
            var newObject = new PlanetObject(
                name,
                PhysicsObjectType.NaturalSatellite,
                mass,
                radius,
                rotationalPeriod,
                axisOfRotation,
                atmosphericModel,
                (NaturalSatelliteObject)orbit.PrimaryBody,
                orbitPosition.CalculateState(ref primaryBodyState),
                orbitPosition.Orbit);

            this.AddObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Adds the given satellite object
        /// </summary>
        /// <param name="primaryBody">The object to orbit around</param>
        /// <param name="name">The name of the object</param>
        /// <param name="mass">The mass of the satellite</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        /// <param name="position">The initial position</param>
        /// <param name="velocity">The initial velocity</param>
        /// <returns>The created object</returns>
        public SatelliteObject AddSatellite(
            NaturalSatelliteObject primaryBody,
            string name,
            double mass,
            AtmosphericProperties atmosphericProperties,
            Vector3d position,
            Vector3d velocity)
        {
            var initialState = new ObjectState(this.totalTime, position, velocity, Vector3d.Zero);

            var newObject = new SatelliteObject(
                name,
                mass,
                atmosphericProperties,
                primaryBody,
                initialState,
                Orbit.CalculateOrbit(primaryBody, ref initialState));

            this.AddObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Adds a satellite in given orbit
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        /// <param name="orbitPosition">The position in the orbit</param>
        /// <returns>The created object</returns>
        public PhysicsObject AddSatelliteInOrbit(
            string name,
            double mass,
            AtmosphericProperties atmosphericProperties,
            OrbitPosition orbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            var primaryBodyState = orbit.PrimaryBody.State;
            var newObject = new SatelliteObject(
                name,
                mass,
                atmosphericProperties,
                (NaturalSatelliteObject)orbit.PrimaryBody,
                orbitPosition.CalculateState(ref primaryBodyState),
                orbitPosition.Orbit);

            this.AddObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Adds the given rocket object
        /// </summary>
        /// <param name="primaryBody">The object to orbit around</param>
        /// <param name="name">The name of the object</param>
        /// <param name="rocketStages">The rocket stages</param>
        /// <param name="position">The initial position</param>
        /// <param name="velocity">The initial velocity</param>
        /// <returns>The created object</returns>
        public RocketObject AddRocket(
            NaturalSatelliteObject primaryBody,
            string name,
            RocketStages rocketStages,
            Vector3d position,
            Vector3d velocity)
        {
            var initialState = new ObjectState(this.totalTime, position, velocity, Vector3d.Zero);

            var newObject = new RocketObject(
                name,
                rocketStages.InitialTotalMass,
                primaryBody,
                initialState,
                Orbit.CalculateOrbit(primaryBody, ref initialState),
                rocketStages,
                this.TextOutputWriter);

            this.AddObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Adds a rocket object in given orbit
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="atmosphericProperties">The atmospheric properties</param>
        /// <param name="rocketStages">The rocket stages</param>
        /// <param name="orbitPosition">The position in the orbit</param>
        /// <returns>The created object</returns>
        public RocketObject AddRocketInOrbit(
            string name,
            AtmosphericProperties atmosphericProperties,
            RocketStages rocketStages,
            OrbitPosition orbitPosition)
        {
            var orbit = orbitPosition.Orbit;
            var primaryBodyState = orbit.PrimaryBody.State;
            var newObject = new RocketObject(
                name,
                rocketStages.InitialTotalMass,
                (NaturalSatelliteObject)orbit.PrimaryBody,
                orbitPosition.CalculateState(ref primaryBodyState),
                orbitPosition.Orbit,
                rocketStages,
                this.TextOutputWriter);

            this.AddObject(newObject);
            return newObject;
        }

        /// <summary>
        /// Adds the given event
        /// </summary>
        /// <param name="newEvent">The event</param>
        private void AddEvent(SimulationEvent newEvent)
        {
            this.events.Add(newEvent);
        }

        /// <summary>
        /// Handles events
        /// </summary>
        private void HandleEvents()
        {
            if (this.events.Count > 0)
            {
                foreach (var currentEvent in this.events)
                {
                    if (this.totalTime >= currentEvent.Time - this.maneuverTimeEpsilon)
                    {
                        this.executedEvents.Add(currentEvent);
                        //Debug.Log(currentEvent.Type + ": " + (this.totalTime - currentEvent.Time));
                    }
                }

                if (this.executedEvents.Count > 0)
                {
                    foreach (var currentEvent in this.executedEvents)
                    {
                        this.events.Remove(currentEvent);
                    }

                    this.executedEvents.Clear();
                }
            }
        }

        /// <summary>
        /// Calculates the sphere of influence for the given object
        /// </summary>
        /// <param name="currentObject">The object</param>
        private void CalculateSphereOfInfluence(PhysicsObject currentObject)
        {
            if (currentObject.Type == PhysicsObjectType.NaturalSatellite)
            {
                var semiMajorAxis = Orbit.CalculateOrbit(currentObject).SemiMajorAxis;
                var soi = OrbitFormulas.SphereOfInfluence(semiMajorAxis, currentObject.Mass, currentObject.PrimaryBody.Mass);
                this.sphereOfInfluences.Add(currentObject, soi);
            }
        }

        /// <summary>
        /// Changes the sphere of influences
        /// </summary>
        private void ChangeSOI()
        {
            foreach (var object1 in this.objects)
            {
                //if (object1.Type == PhysicsObjectType.ArtificialSatellite && !object1.HasImpacted)
                if (object1.Type == PhysicsObjectType.ArtificialSatellite)
                {
                    //Change primary bodys
                    var withinSOI = false;
                    NaturalSatelliteObject primaryBody = null;
                    double? minChangeTime = null;

                    foreach (var object2 in this.objects.Where(x => x.Type == PhysicsObjectType.NaturalSatellite && x != object1))
                    {
                        var soi = this.sphereOfInfluences[object2];
                        var distance = (object1.Position - object2.Position).Length();

                        if (distance < soi)
                        {
                            withinSOI = true;
                            primaryBody = (NaturalSatelliteObject)object2;
                        }

                        if (object1.PrimaryBody == object2.PrimaryBody)
                        {
                            //If a SOI change is likely, add as an event
                            if (OrbitHelpers.SOIChangeLikely(object1.ReferenceOrbit, object2.ReferenceOrbit, soi))
                            {
                                var enterOrbit = OrbitPosition.CalculateOrbitPosition((NaturalSatelliteObject)object2, object1.State);
                                var soiChangeTime = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(enterOrbit);
                                if (soiChangeTime != null && soiChangeTime > 0 && (minChangeTime == null || soiChangeTime < minChangeTime))
                                {
                                    minChangeTime = soiChangeTime;
                                }
                            }
                        }
                    }

                    //Default to object of reference
                    if (!withinSOI)
                    {
                        primaryBody = this.ObjectOfReference;
                    }

                    if (object1.PrimaryBody != primaryBody)
                    {
                        object1.ChangePrimaryBody(primaryBody);

                        var timeToImpact = OrbitCalculators.TimeToImpact(OrbitPosition.CalculateOrbitPosition(object1));
                        if (timeToImpact != null)
                        {
                            this.AddEvent(new SimulationEvent(SimulationEventType.Crash, object1, this.totalTime + timeToImpact ?? 0));
                        }
                    }
                    else if (minChangeTime != null)
                    {
                        this.AddSOIChange(object1, minChangeTime ?? 0);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a SOI change
        /// </summary>
        /// <param name="physicsObject">The object to add for</param>
        /// <param name="soiChangeTime">The time of the SOI change</param>
        private void AddSOIChange(PhysicsObject physicsObject, double soiChangeTime)
        {
            if (soiChangeTime <= 60)
            {
                return;
            }

            var soiEvent = new SimulationEvent(
                SimulationEventType.SphereOfInfluenceChange,
                physicsObject,
                this.totalTime + soiChangeTime);

            if (this.soiChanges.ContainsKey(physicsObject))
            {
                this.events.Remove(this.soiChanges[physicsObject]);
            }

            this.soiChanges[physicsObject] = soiEvent;
            this.AddEvent(soiEvent);
        }

        /// <summary>
        /// Computes the maneuver components
        /// </summary>
        /// <param name="maneuver">The maneuver</param>
        private void ComputeManeuverComponents(SimulationManeuever maneuver)
        {
            //Calculate the state at the burn
            var orbit = Orbit.CalculateOrbit(maneuver.Object);
            var stateAtBurn = this.AfterTime(
                maneuver.Object,
                maneuver.Object.State,
                orbit,
                maneuver.Maneuver.ManeuverTime - this.TotalTime,
                true);

            maneuver.ComputeComponents(ref stateAtBurn);
        }

        /// <summary>
        /// Adds the given maneuver
        /// </summary>
        /// <param name="maneuver">The maneuver</param>
        private void AddManeuver(SimulationManeuever maneuver)
        {
            this.maneuvers.Add(maneuver);
            this.maneuvers.Sort();
        }

        /// <summary>
        /// Schedules the given maneuver for the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="maneuver">The maneuver</param>
        public void ScheduleManeuver(PhysicsObject physicsObject, OrbitalManeuver maneuver)
        {
            var newManeuver = new SimulationManeuever(physicsObject, maneuver);
            this.ComputeManeuverComponents(newManeuver);
            this.AddManeuver(newManeuver);
        }

        /// <summary>
        /// Schedules the given maneuvers for the given object
        /// </summary>
        /// <param name="physicsObject">The object</param>
        /// <param name="maneuvers">The maneuvers</param>
        public void ScheduleManeuver(PhysicsObject physicsObject, OrbitalManeuvers maneuvers)
        {
            foreach (var maneuver in maneuvers)
            {
                this.ScheduleManeuver(physicsObject, maneuver);
            }
        }

        /// <summary>
        /// Aborts the given maneuver
        /// </summary>
        /// <param name="maneuver">The maneuver</param>
        public void AbortManeuver(SimulationManeuever maneuver)
        {
            this.maneuvers.Remove(maneuver);
        }

        /// <summary>
        /// Tries to apply the maneuvers
        /// </summary>
        private void ApplyManeuvers()
        {
            if (this.maneuvers.Count > 0)
            {
                foreach (var maneuver in this.maneuvers)
                {
                    if (this.totalTime >= maneuver.Maneuver.ManeuverTime - this.maneuverTimeEpsilon)
                    {
                        this.executedManeuvers.Add(maneuver);
                        maneuver.Object.ApplyBurn(this.totalTime, maneuver.Maneuver.DeltaVelocity);

                        //If the current maneuver leads to a SOI change/crash, mark it
                        if (maneuver.Maneuver.DeltaVelocity != Vector3d.Zero)
                        {
                            var objectOrbitPosition = OrbitPosition.CalculateOrbitPosition(maneuver.Object);

                            var added = false;
                            if (maneuver.Object.ReferenceOrbit.IsUnbound)
                            {
                                var soiChangeTime = OrbitCalculators.TimeToLeaveSphereOfInfluenceUnboundOrbit(objectOrbitPosition);

                                if (soiChangeTime != null)
                                {
                                    this.AddSOIChange(maneuver.Object, soiChangeTime ?? 0);
                                    added = true;
                                }
                            }

                            var timeToImpact = OrbitCalculators.TimeToImpact(objectOrbitPosition);
                            if (timeToImpact != null)
                            {
                                this.AddEvent(new SimulationEvent(SimulationEventType.Crash, maneuver.Object, this.totalTime + timeToImpact ?? 0));
                                added = true;
                            }

                            if (added)
                            {
                                if (this.SimulationMode == PhysicsSimulationMode.KeplerProblemUniversalVariable)
                                {
                                    this.addedEvent = true;
                                }
                            }
                        }
                    }
                }

                if (this.executedManeuvers.Count > 0)
                {
                    foreach (var maneuver in this.executedManeuvers)
                    {
                        this.maneuvers.Remove(maneuver);
                    }
                    this.executedManeuvers.Clear();

                    //Recompute the delta V components
                    foreach (var maneuver in this.maneuvers)
                    {
                        this.ComputeManeuverComponents(maneuver);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the next states
        /// </summary>
        /// <param name="deltaTime">The delta time</param>
        private void CalculateNextStates(double deltaTime)
        {
            foreach (var currentObject in this.objects)
            {
                if (!currentObject.IsObjectOfReference)
                {
                    this.currentOrbitSimulator.Update(this.totalTime, deltaTime, currentObject, this.AddNewObject);

                    //Set state to relative to handle update of the state of the primary body
                    var nextState = currentObject.NextState;
                    nextState.MakeRelative(currentObject.PrimaryBody.State);
                    currentObject.SetNextState(nextState);
                }
                else
                {
                    var nextState = currentObject.NextState;
                    nextState.Time += deltaTime;

                    currentObject.SetNextState(nextState.WithRotation(SolverHelpers.CalculateRotation(
                        currentObject.RotationalPeriod,
                        currentObject.NextState.Rotation,
                        deltaTime)));
                }
            }

            //Set the absolute state for all objects
            foreach (var currentObject in this.objects)
            {
                if (!currentObject.IsObjectOfReference)
                {
                    var nextState = currentObject.NextState;
                    nextState.MakeAbsolute(currentObject.PrimaryBody.NextState);
                    currentObject.SetNextState(nextState);     
                }
            }

            //Add new objects
            if (this.newObjects.Count > 0)
            {
                foreach (var newObject in this.newObjects)
                {
                    this.AddObject(newObject);
                    this.ObjectAdded?.Invoke(this, newObject);
                }

                this.newObjects.Clear();
            }
        }

        /// <summary>
        /// Updates the physics system using the given amount of steps
        /// </summary>
        /// <param name="deltaTime">The delta time</param>
        /// <param name="numSteps">The number of steps</param>
        private void UpdateStep(double deltaTime, int numSteps)
        {
            //Calculate the time step & and the number of simulation steps
            var timeStep = 0.0;
            var numSimulationSteps = 0;

            switch (this.SimulationMode)
            {
                case PhysicsSimulationMode.PerturbationCowell:
                    timeStep = deltaTime;
                    numSimulationSteps = numSteps;
                    break;
                case PhysicsSimulationMode.KeplerProblemUniversalVariable:
                    timeStep = deltaTime * numSteps;
                    numSimulationSteps = 1;
                    break;
            }

            //Simulate
            for (int i = 0; i < numSimulationSteps; i++)
            {
                this.ApplyManeuvers();
                this.HandleEvents();

                if (this.addedEvent)
                {
                    break;
                }

                //Calculate the next states
                this.CalculateNextStates(deltaTime);

                //Update state
                foreach (var currentObject in this.objects)
                {
                    currentObject.Update(this.totalTime, timeStep);
                }

                this.totalTime += timeStep;

                if (this.SimulationMode == PhysicsSimulationMode.KeplerProblemUniversalVariable)
                {
                    this.ChangeSOI();
                }
            }

            if (this.SimulationMode != PhysicsSimulationMode.KeplerProblemUniversalVariable)
            {
                this.ChangeSOI();
            }
        }

        /// <summary>
        /// Updates for an integrator
        /// </summary>
        /// <param name="numSteps">The number of simulatino steps</param>
        private void UpdateIntegrator(int numSteps)
        {
            this.UpdateStep(this.timeStep, numSteps);
        }

        /// <summary>
        /// Determines the number of sub-intervals
        /// </summary>
        /// <param name="time">The amount of time to simulate for</param>
        private void DetermineSubIntervals(double time)
        {
            this.subTimeIntervals.Clear();

            foreach (var maneuver in this.maneuvers)
            {
                var timeLeft = maneuver.Maneuver.ManeuverTime - this.TotalTime;
                if (timeLeft < time && timeLeft >= 0)
                {
                    this.subTimeIntervals.Add(maneuver.Maneuver.ManeuverTime);
                }
            }

            foreach (var currentEvent in this.events)
            {
                var timeLeft = currentEvent.Time - this.TotalTime;
                if (timeLeft < time && timeLeft >= 0)
                {
                    this.subTimeIntervals.Add(currentEvent.Time);
                }
            }

            this.subTimeIntervals.Add(this.TotalTime + time);

            //Sort, as the maneuvers might not be ordered on time.
            this.subTimeIntervals.Sort();
        }

        /// <summary>
        /// Updates for a Kepler solver
        /// </summary>
        /// <param name="time">The amount of time to simulate for</param>
        private void UpdateKeplerSolver(double time)
        {
            //Determine the of sub-intervals
            this.DetermineSubIntervals(time);

            var lastStart = this.totalTime;
            var restart = false;
            var amountSimulated = 0.0;
            foreach (var startTime in this.subTimeIntervals)
            {
                var intervalAmount = startTime - lastStart;
                this.UpdateStep(intervalAmount, 1);

                if (this.addedEvent)
                {
                    restart = true;
                    break;
                }

                lastStart = startTime;
                amountSimulated += this.totalTime - startTime;
            }

            this.addedEvent = false;

            //If a maneuver was added during an interval, restart
            if (restart)
            {
                var t = time - amountSimulated;
                this.UpdateKeplerSolver(t);
            }
        }

        /// <summary>
        /// Advances the physics system the given amount of time
        /// </summary>
        /// <param name="duration">The duration</param>
        public void Advance(TimeSpan duration)
        {
            switch (this.simulationMode)
            {
                case PhysicsSimulationMode.PerturbationCowell:
                    this.UpdateIntegrator((int)Math.Ceiling(duration.TotalSeconds / this.timeStep));
                    break;
                case PhysicsSimulationMode.KeplerProblemUniversalVariable:
                    this.UpdateKeplerSolver(duration.TotalSeconds);
                    break;
            }
        }

        /// <summary>
        /// Updates the physics system
        /// </summary>
        public void Update()
        {
            switch (this.simulationMode)
            {
                case PhysicsSimulationMode.PerturbationCowell:
                    this.UpdateIntegrator(this.SimulationSpeed);
                    break;
                case PhysicsSimulationMode.KeplerProblemUniversalVariable:
                    this.UpdateKeplerSolver(this.timeStep * this.SimulationSpeed);
                    break;
            }
        }

        /// <summary>
        /// Calculates the state after the given amount of time
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="state">The state</param>
        /// <param name="orbit">The orbit</param>
        /// <param name="time">The time</param>
        /// <param name="relative">Indicates if the that should be relative</param>
        /// <remarks>This method does not take SOI changes or maneuvers into account.</remarks>
        public ObjectState AfterTime(
            IPhysicsObject physicsObject,
            ObjectState state,
            Orbit orbit,
            double time,
            bool relative = false)
        {
            return SolverHelpers.AfterTime(this.keplerProblemSolver, physicsObject, state, orbit, time, relative);
        }
    }
}
