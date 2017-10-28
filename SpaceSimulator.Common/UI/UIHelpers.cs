using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Contains UI helper methods
    /// </summary>
    public static class UIHelpers
    {
        /// <summary>
        /// Calculates the screen position for the given object
        /// </summary>
        /// <param name="parentRectangle">The parent rectangle</param>
        /// <param name="objectRectangle">The object rectangle</param>
        /// <param name="positionRelationX">The position relation in the x-axis</param>
        /// <param name="positionRelationY">The position relation in the y-axis</param>
        public static Vector2 CalculateScreenPosition(RectangleF parentRectangle, RectangleF objectRectangle, PositionRelationX positionRelationX, PositionRelationY positionRelationY)
        {
            var screenPosition = Vector2.Zero;

            switch (positionRelationX)
            {
                case PositionRelationX.Left:
                    screenPosition.X = parentRectangle.X + objectRectangle.X;
                    break;
                case PositionRelationX.Center:
                    screenPosition.X = (parentRectangle.X + parentRectangle.Width * 0.5f - objectRectangle.Width * 0.5f) + objectRectangle.X;
                    break;
                case PositionRelationX.Right:
                    screenPosition.X = parentRectangle.Right - objectRectangle.Right;
                    break;
            }

            switch (positionRelationY)
            {
                case PositionRelationY.Top:
                    screenPosition.Y = parentRectangle.Y + objectRectangle.Y;
                    break;
                case PositionRelationY.Center:
                    screenPosition.Y = (parentRectangle.Y + parentRectangle.Height * 0.5f - objectRectangle.Height * 0.5f) + objectRectangle.Y;
                    break;
                case PositionRelationY.Bottom:
                    screenPosition.Y = parentRectangle.Bottom - objectRectangle.Bottom;
                    break;
            }

            return screenPosition;
        }

        /// <summary>
        /// Sets focus to the given element
        /// </summary>
        /// <param name="elements">The elements</param>
        /// <param name="focusElement">The element</param>
        public static void SetFocus<T>(IList<T> elements, T focusElement) where T : UIElement
        {
            foreach (var element in elements)
            {
                if (element.HasFocus)
                {
                    element.LostFocus();
                }

                element.HasFocus = false;
            }

            if (focusElement != null)
            {
                focusElement.HasFocus = true;
                focusElement.GotFocus();
            }
        }

        /// <summary>
        /// Selects an element at the given position
        /// </summary>
        /// <param name="elements">The elements</param>
        /// <param name="position">The position</param>
        /// <returns>The element or null</returns>
        public static T SelectElement<T>(IList<T> elements, Vector2 position) where T : UIElement
        {
            T topElement = null;

            foreach (var element in elements)
            {
                if (element.IsVisible && element.BoundingRectangle.Contains(position.X, position.Y))
                {
                    if (topElement == null || element.ZOrder >= topElement.ZOrder)
                    {
                        topElement = element;
                    }
                }
            }

            return topElement;
        }
    }
}
