using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectWrite;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component
    /// </summary>
    public abstract class UIComponent : IDisposable
    {
        /// <summary>
        /// The rendering manager 2D
        /// </summary>
        protected RenderingManager2D RenderingManager2D { get; }

        /// <summary>
        /// The keyboard manager
        /// </summary>
        protected KeyboardManager KeyboardManager { get; }

        /// <summary>
        /// The mouse manager
        /// </summary>
        protected MouseManager MouseManager { get; }

        /// <summary>
        /// The simulator container
        /// </summary>
        protected SimulatorContainer SimulatorContainer { get; }

        private readonly Queue<Action> mainThreadQueue = new Queue<Action>();

        /// <summary>
        /// Creates a new base UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        public UIComponent(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, MouseManager mouseManager, SimulatorContainer simulatorContainer)
        {
            this.RenderingManager2D = renderingManager2D;
            this.KeyboardManager = keyboardManager;
            this.MouseManager = mouseManager;
            this.SimulatorContainer = simulatorContainer;
        }

        /// <summary>
        /// Returns the simulator engine
        /// </summary>
        protected SimulatorEngine SimulatorEngine => this.SimulatorContainer.SimulatorEngine;

        /// <summary>
        /// Returns the text color brush
        /// </summary>
        protected RenderingSolidColorBrush TextColorBrush => this.RenderingManager2D.DefaultSolidColorBrush;

        /// <summary>
        /// Returns the text format
        /// </summary>
        protected TextFormat TextFormat => this.RenderingManager2D.DefaultTextFormat;

        /// <summary>
        /// Returns the selected object
        /// </summary>
        protected PhysicsObject SelectedObject => this.SimulatorContainer.SelectedObject;

        /// <summary>
        /// Runs the given task on the main thread
        /// </summary>
        /// <param name="action">The action</param>
        protected void ScheduleMain(Action action)
        {
            lock (this.mainThreadQueue)
            {
                this.mainThreadQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Runs the scheduled tasks
        /// </summary>
        protected void RunScheduledTasks()
        {
            lock (this.mainThreadQueue)
            {
                while (this.mainThreadQueue.Count > 0)
                {
                    var task = this.mainThreadQueue.Dequeue();
                    task();
                }
            }
        }

        /// <summary>
        /// Handles when a mouse button is pressed
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="button">Which button that is being pressed</param>
        public virtual void OnMouseButtonDown(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
        {

        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public abstract void Update(TimeSpan elapsed);

        /// <summary>
        /// Called after when the simulator has been updated
        /// </summary>
        public virtual void AfterSimulationUpdate()
        {

        }

        /// <summary>
        /// Called before the first frame is drawn
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public virtual void BeforeFirstDraw(SharpDX.Direct2D1.DeviceContext deviceContext)
        {

        }

        /// <summary>
        /// Draws the component after the 3D
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public abstract void Draw(SharpDX.Direct2D1.DeviceContext deviceContext);

        public virtual void Dispose()
        {
           
        }
    }
}
