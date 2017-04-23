using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Creates a new simulator engine
        /// </summary>
        /// <param name="simulatorEngine">The simulator engine</param>
        public SimulatorContainer(SimulatorEngine simulatorEngine)
        {
            this.SimulatorEngine = simulatorEngine;
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
    }
}
