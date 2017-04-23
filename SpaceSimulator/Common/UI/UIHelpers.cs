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
    }
}
