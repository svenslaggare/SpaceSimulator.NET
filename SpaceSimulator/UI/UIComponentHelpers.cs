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
        /// <param name="validObject">Predicate to determine if the object is valid</param>
        /// <param name="deltaMultiplier">The detlta multiplier</param>
        /// <returns>The selected object</returns>
        public static PhysicsObject SelectObjectUpAndDown
            (KeyboardManager keyboardManager,
            IList<PhysicsObject> objects,
            ref int index,
            Key keyUp,
            Key keyDown,
            out bool changed,
            Predicate<PhysicsObject> validObject = null,
            int deltaMultiplier = 1)
        {
            if (deltaMultiplier > objects.Count)
            {
                changed = false;
                return null;
            }

            var deltaIndex = 0;
            if (keyboardManager.IsKeyPressed(keyUp))
            {
                deltaIndex = -1 * deltaMultiplier;
            }

            if (keyboardManager.IsKeyPressed(keyDown))
            {
                deltaIndex = 1 * deltaMultiplier;
            }

            var newIndex = index + deltaIndex;
            if (deltaIndex != 0)
            {
                newIndex = newIndex % objects.Count;
                if (newIndex < 0)
                {
                    newIndex += objects.Count;
                }
            }

            if (deltaIndex != 0)
            {
                var isValid = validObject != null ? validObject(objects[newIndex]) : true;

                if (isValid)
                {
                    changed = true;
                    index = newIndex;
                }
                else
                {
                    return SelectObjectUpAndDown(
                        keyboardManager,
                        objects,
                        ref index,
                        keyUp,
                        keyDown,
                        out changed,
                        validObject,
                        deltaMultiplier + 1);
                }
            }
            else
            {
                changed = false;
            }

            return objects[index];
        }
    }
}
