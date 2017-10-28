using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents an UI object
    /// </summary>
    public abstract class UIObject : UIElement
    {
        /// <summary>
        /// The event when the object is clicked by the left mouse button
        /// </summary>
        public event EventHandler<Vector2> LeftMouseButtonClicked;

        /// <summary>
        /// The event when the object is clicked by the right mouse button
        /// </summary>
        public event EventHandler<Vector2> RightMouseButtonClicked;

        /// <summary>
        /// Creates a new UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="size">The size</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public UIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            Size2 size,
            PositionRelationX positionRelationX,
            PositionRelationY positionRelationY,
            UIElement parent)
            : base(renderingManager2D, name, position, size, positionRelationX, positionRelationY, parent)
        {

        }

        protected override Vector2 UpdateScreenPosition(Vector2 oldScreenPosition)
        {
            var parentRectangle = this.Parent == null ? this.RenderingManager2D.ScreenRectangle : this.Parent.BoundingRectangle;
            return UIHelpers.CalculateScreenPosition(parentRectangle, this.RelativeBoundingRectangle, this.PositionRelationX, this.PositionRelationY);
        }

        public override void HandleClicked(Vector2 mousePosition, MouseButtons button)
        {
            base.HandleClicked(mousePosition, button);

            switch (button)
            {
                case MouseButtons.Left:
                    this.LeftMouseButtonClicked?.Invoke(this, mousePosition);
                    break;
                case MouseButtons.Right:
                    this.RightMouseButtonClicked?.Invoke(this, mousePosition);
                    break;
            }
        }
    }
}
