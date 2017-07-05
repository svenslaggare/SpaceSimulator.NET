using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents an UI group
    /// </summary>
    public class UIGroup : UIElement
    {
        private readonly List<UIObject> objects = new List<UIObject>();

        /// <summary>
        /// Creates a new UI group
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="size">The size</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public UIGroup(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            Size2 size,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null) 
            : base(renderingManager2D, name, position, size, positionRelationX, positionRelationY, parent)
        {

        }

        /// <summary>
        /// Returns the objects in the group
        /// </summary>
        public IReadOnlyList<UIObject> Objects => this.objects.AsReadOnly();

        /// <summary>
        /// Adds the given object to the group
        /// </summary>
        /// <param name="uiObject">The object</param>
        public void AddObject(UIObject uiObject)
        {
            this.objects.Add(uiObject);
        }

        /// <summary>
        /// Sets focus to the given object
        /// </summary>
        /// <param name="focusObject">The object</param>
        private void SetFocus(UIObject focusObject)
        {
            foreach (var currentObject in this.objects)
            {
                if (currentObject.HasFocus)
                {
                    currentObject.LostFocus();
                }

                currentObject.HasFocus = false;
            }

            if (focusObject != null)
            {
                focusObject.HasFocus = true;
                focusObject.GotFocus();
            }
        }

        /// <summary>
        /// Selects an element at the given position
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>The object or null</returns>
        private UIObject SelectObject(Vector2 position)
        {
            UIObject topObject = null;

            foreach (var currentObject in this.objects)
            {
                if (currentObject.IsVisible && currentObject.BoundingRectangle.Contains(position.X, position.Y))
                {
                    if (topObject == null || currentObject.ZOrder >= topObject.ZOrder)
                    {
                        topObject = currentObject;
                    }
                }
            }

            return topObject;
        }

        public override void HandleClicked(Vector2 mousePosition, MouseButtons button)
        {
            base.HandleClicked(mousePosition, button);

            var topObject = this.SelectObject(mousePosition);
            if (topObject != null)
            {
                this.SetFocus(topObject);
                topObject.HandleClicked(mousePosition, button);
            }
            else
            {
                this.SetFocus(null);
                Console.WriteLine("Group clicked");
            }
        }

        protected override Vector2 UpdateScreenPosition(Vector2 oldScreenPosition)
        {
            var parentRectangle = this.Parent == null ? this.RenderingManager2D.ScreenRectangle : this.Parent.BoundingRectangle;
            return UIHelpers.CalculateScreenPosition(parentRectangle, this.RelativeBoundingRectangle, this.PositionRelationX, this.PositionRelationY);
        }

        public override void Invalidate()
        {
            base.Invalidate();

            foreach (var currentObject in this.objects)
            {
                currentObject.Invalidate();
            }
        }

        public override void Draw(DeviceContext deviceContext)
        {
            foreach (var currentObject in this.objects)
            {
                if (currentObject.IsVisible)
                {
                    currentObject.Draw(deviceContext);
                }
            }
        }
    }
}
