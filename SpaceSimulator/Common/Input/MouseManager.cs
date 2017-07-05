using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;

namespace SpaceSimulator.Common.Input
{
    /// <summary>
    /// Represents a mouse manager
    /// </summary>
    public sealed class MouseManager : IDisposable
    {
        private readonly Mouse mouse;

        private MouseState mouseState = new MouseState();
        private MouseState prevMouseState = new MouseState();

        private readonly DateTime[] lastButtonPressedTime = new DateTime[3];

        /// <summary>
        /// Returns the position of the mouse on the screen
        /// </summary>
        public Vector2 MousePosition { get; private set; }

        /// <summary>
        /// Creates a new mouse manager
        /// </summary>
        /// <param name="directInput">The direct input object</param>
        public MouseManager(DirectInput directInput)
        {
            this.mouse = new Mouse(directInput);
            this.mouse.Acquire();
        }

        /// <summary>
        /// Updates the state
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        public void Update(Vector2 mousePosition)
        {
            var timeNow = DateTime.UtcNow;
            if (this.IsButtonPressed(System.Windows.Forms.MouseButtons.Left))
            {
                this.lastButtonPressedTime[0] = timeNow;
            }

            if (this.IsButtonPressed(System.Windows.Forms.MouseButtons.Right))
            {
                this.lastButtonPressedTime[1] = timeNow;
            }

            if (this.IsButtonPressed(System.Windows.Forms.MouseButtons.Middle))
            {
                this.lastButtonPressedTime[2] = timeNow;
            }

            this.MousePosition = mousePosition;
            this.prevMouseState = this.mouseState;
            this.mouseState = this.mouse.GetCurrentState();
        }

        /// <summary>
        /// Indicates if the given button is being pressed down
        /// </summary>
        /// <param name="mouseState">The state</param>
        /// <param name="button">The button</param>
        private bool IsButtonDown(MouseState mouseState, System.Windows.Forms.MouseButtons button)
        {
            switch (button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    return mouseState.Buttons[0];
                case System.Windows.Forms.MouseButtons.Right:
                    return mouseState.Buttons[1];
                case System.Windows.Forms.MouseButtons.Middle:
                    return mouseState.Buttons[2];
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates if the given button is being pressed down
        /// </summary>
        /// <param name="button">The button</param>
        public bool IsButtonDown(System.Windows.Forms.MouseButtons button)
        {
            return this.IsButtonDown(this.mouseState, button);
        }

        /// <summary>
        /// Indicates if the given button is being pressed up
        /// </summary>
        /// <param name="button">The button</param>
        public bool IsButtonUp(System.Windows.Forms.MouseButtons button)
        {
            return !this.IsButtonDown(button);
        }

        /// <summary>
        /// Indicates if the given button was pressed
        /// </summary>
        /// <param name="button">The button</param>
        public bool IsButtonPressed(System.Windows.Forms.MouseButtons button)
        {
            return this.IsButtonDown(this.prevMouseState, button) && !this.IsButtonDown(this.mouseState, button);
        }

        /// <summary>
        /// Indicates if the given button has been double clicked
        /// </summary>
        /// <param name="button">The button</param>
        /// <param name="minDoubleClickTime">The minimum double click time (in ms)</param>
        /// <param name="maxDoubleClickTime">The maximum double click time (in ms)</param>
        public bool IsDoubleClick(System.Windows.Forms.MouseButtons button, double minDoubleClickTime = 0.0, double maxDoubleClickTime = 200.0)
        {
            var timeSinceClick = new TimeSpan();
            switch (button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    timeSinceClick = DateTime.UtcNow - this.lastButtonPressedTime[0];
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    timeSinceClick = DateTime.UtcNow - this.lastButtonPressedTime[1];
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    timeSinceClick = DateTime.UtcNow - this.lastButtonPressedTime[2];
                    break;
            }

            return this.IsButtonPressed(button)
                && timeSinceClick.TotalMilliseconds >= minDoubleClickTime
                && timeSinceClick.TotalMilliseconds <= maxDoubleClickTime;
        }

        public void Dispose()
        {
            this.mouse.Dispose();
        }
    }
}
