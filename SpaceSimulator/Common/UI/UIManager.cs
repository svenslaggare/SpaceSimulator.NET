using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Manages the UI elements
    /// </summary>
    public sealed class UIManager : IDisposable
    {
        private readonly IList<UIElement> elements = new List<UIElement>();
        private readonly RenderingManager2D renderingManager2D;

        private readonly RenderingSolidColorBrush debugBrush;

        /// <summary>
        /// Indicates if bounding rectangles are drawn for each element
        /// </summary>
        public bool DrawBoundingRectangles { get; set; }

        /// <summary>
        /// Creates a new UI manager
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        public UIManager(RenderingManager2D renderingManager2D)
        {
            this.renderingManager2D = renderingManager2D;
            this.debugBrush = this.renderingManager2D.CreateSolidColorBrush(Color.Yellow);
        }

        /// <summary>
        /// Adds the given element to the list of elements
        /// </summary>
        /// <param name="element">The element to add</param>
        public void AddElement(UIElement element)
        {
            this.elements.Add(element);
        }

        /// <summary>
        /// Invalidates the UI, forcing all elements to redraw
        /// </summary>
        public void Invalidate()
        {
            foreach (var element in this.elements)
            {
                element.Invalidate();
            }
        }

        /// <summary>
        /// Updates the UI elements
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last update</param>
        public void Update(TimeSpan elapsed)
        {
            foreach (var element in this.elements)
            {
                element.Update(elapsed);
            }
        }

        /// <summary>
        /// Draws the UI elements
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Draw(DeviceContext deviceContext)
        {
            foreach (var element in this.elements)
            {
                element.Draw(deviceContext);

                if (this.DrawBoundingRectangles)
                {
                    this.debugBrush.ApplyResource(brush =>
                    {
                        deviceContext.DrawRectangle(
                            element.BoundingRectangle,
                            brush);
                    });
                }
            }
        }

        /// <summary>
        /// Sets focus to the given element
        /// </summary>
        /// <param name="focusElement">The element</param>
        public void SetFocus(UIElement focusElement)
        {
            foreach (var element in this.elements)
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
        /// Handles when a mouse button is pressed
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="button">Which button that is being pressed</param>
        public void OnMouseButtonDown(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
        {
            UIElement topElement = null;

            foreach (var element in this.elements)
            {
                if (element.BoundingRectangle.Contains(mousePosition.X, mousePosition.Y))
                {
                    if (topElement == null || element.ZOrder >= topElement.ZOrder)
                    {
                        topElement = element;
                    }
                }
            }

            if (topElement != null)
            {
                topElement.HandleClicked(mousePosition, button);
                this.SetFocus(topElement);
            }
            else
            {
                this.SetFocus(null);
            }
        }

        public void Dispose()
        {
            foreach (var element in this.elements)
            {
                element.Dispose();
            }
        }
    }
}
