using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

namespace SpaceSimulator
{
    /// <summary>
    /// Represents a container for the simulator
    /// </summary>
    public class SimulatorContainer
    {
        /// <summary>
        /// Returns the simulator engine
        /// </summary>
        public SimulatorEngine SimulatorEngine { get; }

        private bool isFrozen = false;
        private bool isPaused = false;

        private PhysicsObject selectedObject;

        private readonly Func<PhysicsObject, RenderingObject> createRenderingObject;

        /// <summary>
        /// Event for when the selected object changes
        /// </summary>
        public event EventHandler<PhysicsObject> SelectedObjectChanged;

        /// <summary>
        /// The rendering objects
        /// </summary>
        public IList<RenderingObject> RenderingObjects { get; }

        /// <summary>
        /// Creates a new simulator engine
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="renderingObjects">The rendering objects</param>
        /// <param name="createRenderingObject">Creates a rendering object at runtime</param>
        public SimulatorContainer(SimulatorEngine simulatorEngine, IList<RenderingObject> renderingObjects, Func<PhysicsObject, RenderingObject> createRenderingObject)
        {
            this.SimulatorEngine = simulatorEngine;
            this.RenderingObjects = renderingObjects;

            if (createRenderingObject != null)
            {
                this.createRenderingObject = createRenderingObject;

                this.SimulatorEngine.ObjectAdded += (sender, newObject) =>
                {
                    this.RenderingObjects.Add(this.createRenderingObject(newObject));
                };
            }
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
        /// Adds a rendering object for the given object
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        public void AddRenderingObject(PhysicsObject physicsObject)
        {
            if (this.createRenderingObject != null)
            {
                var renderingObject = this.createRenderingObject(physicsObject);
                this.RenderingObjects.Add(renderingObject);
            }
        }
    }
}
