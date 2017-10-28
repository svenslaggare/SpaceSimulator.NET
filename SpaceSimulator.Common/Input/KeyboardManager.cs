using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace SpaceSimulator.Common.Input
{
    /// <summary>
    /// Represents a keyboard manager
    /// </summary>
    public sealed class KeyboardManager : IDisposable
    {
        private readonly Keyboard keyboard;

        private bool canReceiveInput = true;

        private KeyboardState keyboardState = new KeyboardState();
        private KeyboardState prevKeyboardState = new KeyboardState();

        /// <summary>
        /// Creates a new keyboard manager
        /// </summary>
        /// <param name="directInput">The direct input object</param>
        public KeyboardManager(DirectInput directInput)
        {
            this.keyboard = new Keyboard(directInput);
            this.keyboard.Acquire();
        }

        /// <summary>
        /// Returns the current state of the keyboard
        /// </summary>
        public KeyboardState State
        {
            get { return this.keyboardState; }
        }

        /// <summary>
        /// Updates the keyboard manager
        /// </summary>
        /// <param name="canReceiveInput">Indicates if the manager can receive input</param>
        public void Update(bool canReceiveInput)
        {
            this.canReceiveInput = canReceiveInput;

            this.prevKeyboardState = this.keyboardState;
            this.keyboardState = this.keyboard.GetCurrentState();
            //Console.WriteLine(string.Join(", ", this.keyboardState.PressedKeys));
        }

        /// <summary>
        /// Indicates if the key is up
        /// </summary>
        /// <param name="key">The key</param>
        public bool IsKeyUp(Key key)
        {
            if (!this.canReceiveInput)
            {
                return false;
            }

            return !this.keyboardState.IsPressed(key);
        }

        /// <summary>
        /// Indicates if the given key is down
        /// </summary>
        /// <param name="key">The key</param>
        public bool IsKeyDown(Key key)
        {
            if (!this.canReceiveInput)
            {
                return false;
            }

            return this.keyboardState.IsPressed(key);
        }

        /// <summary>
        /// Indicates if the given key is pressed
        /// </summary>
        /// <param name="key">The key</param>
        public bool IsKeyPressed(Key key)
        {
            if (!this.canReceiveInput)
            {
                return false;
            }

            return !this.prevKeyboardState.IsPressed(key) && this.keyboardState.IsPressed(key);
        }

        public void Dispose()
        {
            this.keyboard.Dispose();
        }
    }
}
