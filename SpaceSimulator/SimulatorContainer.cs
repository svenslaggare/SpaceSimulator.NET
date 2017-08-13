using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

namespace SpaceSimulator
{
    /// <summary>
    /// Represents a container for the simulator
    /// </summary>
    public class SimulatorContainer
    {
        private readonly Device graphicsDevice;

        /// <summary>
        /// Returns the simulator engine
        /// </summary>
        public SimulatorEngine SimulatorEngine { get; }

        /// <summary>
        /// The multiplier for the simulation speed
        /// </summary>
        public int SimulationSpeedMultiplier { get; set; } = 1;

        /// <summary>
        /// Indicates if slow motion mode
        /// </summary>
        public bool IsSlowMotion { get; set; } = false;

        private bool isFrozen = false;
        private bool isPaused = false;

        private PhysicsObject selectedObject;

        /// <summary>
        /// Event for when the selected object changes
        /// </summary>
        public event EventHandler<PhysicsObject> SelectedObjectChanged;

        /// <summary>
        /// The rendering objects
        /// </summary>
        public IList<RenderingObject> RenderingObjects { get; }

        private readonly IDictionary<PhysicsObject, RenderingObject> physicsToRendering = new Dictionary<PhysicsObject, RenderingObject>();

        /// <summary>
        /// Creates a new simulator engine
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="renderingObjects">The rendering objects</param>
        public SimulatorContainer(Device graphicsDevice, SimulatorEngine simulatorEngine, IList<RenderingObject> renderingObjects)
        {
            this.graphicsDevice = graphicsDevice;
            this.SimulatorEngine = simulatorEngine;
            this.RenderingObjects = renderingObjects;

            foreach (var renderingObject in renderingObjects)
            {
                this.physicsToRendering.Add(renderingObject.PhysicsObject, renderingObject);
            }

            this.SimulatorEngine.ObjectAdded += (sender, newObject) =>
            {
                var parentObject = this.physicsToRendering[(PhysicsObject)sender];
                this.AddRenderingObject(parentObject.CreateForSub(newObject));
            };
        }

        /// <summary>
        /// Indicates if the simulator is paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.isPaused || this.isFrozen;
            }
            set
            {
                this.isPaused = value;
            }
        }

        /// <summary>
        /// Indicates if the simulator is frozen
        /// </summary>
        public bool IsFrozen => this.isFrozen;

        /// <summary>
        /// Returns the actual simulation speed
        /// </summary>
        public int ActualSimulationSpeed => this.SimulatorEngine.SimulationSpeed * this.SimulationSpeedMultiplier;

        /// <summary>
        /// The selected object
        /// </summary>
        public PhysicsObject SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                this.selectedObject = value;
                this.SelectedObjectChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Freezes the simulation
        /// </summary>
        public void Freeze()
        {
            this.isFrozen = true;
        }

        /// <summary>
        /// Unfreezes the simulation
        /// </summary>
        public void Unfreeze()
        {
            this.isFrozen = false;
        }

        /// <summary>
        /// Returns the rendering object for the given physics object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        /// <returns>The rendering object or null</returns>
        public RenderingObject GetRenderingObject(PhysicsObject physicsObject)
        {
            if (this.physicsToRendering.TryGetValue(physicsObject, out var renderingObject))
            {
                return renderingObject;
            }

            return null;
        }

        /// <summary>
        /// Adds the given rendering object
        /// </summary>
        /// <param name="renderingObject"></param>
        private void AddRenderingObject(RenderingObject renderingObject)
        {
            this.RenderingObjects.Add(renderingObject);
            this.physicsToRendering.Add(renderingObject.PhysicsObject, renderingObject);
        }

        /// <summary>
        /// Creates and add a rendering object
        /// </summary>
        /// <param name="create">The create function</param>
        public void CreateRenderingObject(Func<Device, RenderingObject> create)
        {
            this.AddRenderingObject(create(this.graphicsDevice));
        }
    }
}
