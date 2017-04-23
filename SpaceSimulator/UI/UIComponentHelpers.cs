using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Contains helper methods for UI components
    /// </summary>
    public static class UIComponentHelpers
    {
        /// <summary>
        /// Selects an object using up and down keys
        /// </summary>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="objects">The objects</param>
        /// <param name="index">The current index</param>
        /// <param name="keyUp">The up key</param>
        /// <param name="keyDown">The down key</param>
        /// <param name="changed">Indicates if the object was changed</param>
        /// <returns>The selected object</returns>
        public static PhysicsObject SelectObjectUpAndDown(KeyboardManager keyboardManager, IList<PhysicsObject> objects, ref int index, Key keyUp, Key keyDown, out bool changed)
        {
            var deltaIndex = 0;
            if (keyboardManager.IsKeyPressed(keyUp))
            {
                deltaIndex = -1;
            }

            if (keyboardManager.IsKeyPressed(keyDown))
            {
                deltaIndex = 1;
            }

            if (deltaIndex != 0)
            {
                index += deltaIndex;
                if (index < 0)
                {
                    index = objects.Count - 1;
                }

                if (index >= objects.Count)
                {
                    index = 0;
                }
            }

            changed = deltaIndex != 0;
            return objects[index];
        }
    }
}
