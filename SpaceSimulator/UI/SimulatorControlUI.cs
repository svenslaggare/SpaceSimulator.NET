using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for controlling the simulator
    /// </summary>
    public class SimulatorControlUI : UIComponent
    {
        /// <summary>
        /// Creates a new simulator controll UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        public SimulatorControlUI(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            SimulatorContainer simulatorContainer) 
            : base(renderingManager2D, keyboardManager, mouseManager, simulatorContainer)
        {

        }

        public override void Update(TimeSpan elapsed)
        {
            if (this.KeyboardManager.IsKeyPressed(SharpDX.DirectInput.Key.F1))
            {
                this.SimulatorEngine.SimulationMode = Simulator.PhysicsSimulationMode.PerturbationCowell;
            }

            if (this.KeyboardManager.IsKeyPressed(SharpDX.DirectInput.Key.F2))
            {
                this.SimulatorEngine.SimulationMode = Simulator.PhysicsSimulationMode.KeplerProblemUniversalVariable;
            }

            if (this.KeyboardManager.IsKeyPressed(SharpDX.DirectInput.Key.F3))
            {
                this.SimulatorEngine.SimulationMode = Simulator.PhysicsSimulationMode.Hybrid;
            }
        }

        public override void Draw(DeviceContext deviceContext)
        {

        }
    }
}
