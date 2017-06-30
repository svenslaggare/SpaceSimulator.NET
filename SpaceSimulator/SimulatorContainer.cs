using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Indicates if the simulator is paused
        /// </summary>
        public bool Paused { get; set; }

        private PhysicsObject selectedObject;

        /// <summary>
        /// Event for when the selected object changes
        /// </summary>
        public event EventHandler<PhysicsObject> SelectedObjectChanged;

        private readonly RenderingObject referenceRenderingObject;
        private readonly IList<RenderingObject> otherRenderingObjects;

        /// <summary>
        /// Creates a new simulator engine
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        /// <param name="referenceRenderingObject">The rendering object for the reference object</param>
        /// <param name="otherRenderingObjects">The other rendering objects</param>
        public SimulatorContainer(SimulatorEngine simulatorEngine, RenderingObject referenceRenderingObject, IList<RenderingObject> otherRenderingObjects)
        {
            this.SimulatorEngine = simulatorEngine;
            this.referenceRenderingObject = referenceRenderingObject;
            this.otherRenderingObjects = otherRenderingObjects;
        }

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
        /// Returns the rendering objects
        /// </summary>
        public IEnumerable<RenderingObject> RenderingObjects
        {
            get
            {
                yield return this.referenceRenderingObject;

                foreach (var currentObject in this.otherRenderingObjects)
                {
                    yield return currentObject;
                }
            }
        }
    }
}
