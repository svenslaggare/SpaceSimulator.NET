using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// How the x position is relative to the parent
    /// </summary>
    public enum PositionRelationX
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,
        /// <summary>
        /// Center
        /// </summary>
        Center,
        /// <summary>
        /// Right
        /// </summary>
        Right
    }

    /// <summary>
    /// How the y position is relative to the parent
    /// </summary>
    public enum PositionRelationY
    {
        /// <summary>
        /// Top
        /// </summary>
        Top,
        /// <summary>
        /// Center
        /// </summary>
        Center,
        /// <summary>
        /// Bottom
        /// </summary>
        Bottom,
    }

    /// <summary>
    /// Represents an UI element
    /// </summary>
    public abstract class UIElement : IDisposable
    {
        /// <summary>
        /// The 2D rendering manager
        /// </summary>
        protected RenderingManager2D RenderingManager2D { get; }

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The parent element
        /// </summary>
        public UIElement Parent { get; }

        private Vector2 position;
        private PositionRelationX positionRelationX;
        private PositionRelationY positionRelationY;
        private Vector2 screenPosition;

        private Size2 size;

        private bool updateScreenPosition = true;

        /// <summary>
        /// The Z-order of the element
        /// </summary>
        public int ZOrder { get; set; }

        /// <summary>
        /// Indicates if the element has focus
        /// </summary>
        public bool HasFocus { get; set; } = false;

        /// <summary>
        /// Indicates if the object is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Creates a new UI element
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="size">The size</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public UIElement(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            Size2 size,
            PositionRelationX positionRelationX,
            PositionRelationY positionRelationY,
            UIElement parent)
        {
            this.RenderingManager2D = renderingManager2D;
            this.Name = name;
            this.position = position;
            this.size = size;
            this.positionRelationX = positionRelationX;
            this.positionRelationY = positionRelationY;
            this.Parent = parent;
        }

        /// <summary>
        /// The position
        /// </summary>
        public Vector2 Position
        {
            get { return this.position; }
            set
            {
                this.position = value;
                this.updateScreenPosition = true;
            }
        }

        /// <summary>
        /// How the position is related to the parent in the x-axis
        /// </summary>
        public PositionRelationX PositionRelationX
        {
            get { return this.positionRelationX; }
            set
            {
                this.positionRelationX = value;
                this.updateScreenPosition = true;
            }
        }

        /// <summary>
        /// How the position is related to the parent in the y-axis
        /// </summary>
        public PositionRelationY PositionRelationY
        {
            get { return this.positionRelationY; }
            set
            {
                this.positionRelationY = value;
                this.updateScreenPosition = true;
            }
        }

        /// <summary>
        /// Returns the actual position on the screen
        /// </summary>
        public Vector2 ScreenPosition
        {
            get
            {
                if (this.updateScreenPosition)
                {
                    this.screenPosition = this.UpdateScreenPosition(this.screenPosition);
                    this.updateScreenPosition = false;
                }

                return this.screenPosition;
            }
        }

        /// <summary>
        /// The size of the element
        /// </summary>
        public Size2 Size
        {
            get { return this.size; }
            protected set
            {
                this.size = value;
                this.updateScreenPosition = true;
            }
        }

        /// <summary>
        /// Returns the bounding rectangle for the element
        /// </summary>
        public RectangleF BoundingRectangle
        {
            get
            {
                return new RectangleF(this.ScreenPosition.X, this.ScreenPosition.Y, this.Size.Width, this.Size.Height);
            }
        }

        /// <summary>
        /// Returns the relative bounding rectangle for the element
        /// </summary>
        public RectangleF RelativeBoundingRectangle
        {
            get
            {
                return new RectangleF(this.Position.X, this.Position.Y, this.Size.Width, this.Size.Height);
            }
        }

        /// <summary>
        /// Updates the screen position
        /// </summary>
        /// <param name="oldScreenPosition">The old screen position</param>
        /// <returns>The new screen position</returns>
        protected abstract Vector2 UpdateScreenPosition(Vector2 oldScreenPosition);

        /// <summary>
        /// Handles when the element is clicked
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="button">Which button that is being pressed</param>
        public virtual void HandleClicked(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
        {

        }

        /// <summary>
        /// Invalidates the element, forcing the screen position to be recomputed
        /// </summary>
        public virtual void Invalidate()
        {
            this.updateScreenPosition = true;
        }

        /// <summary>
        /// Handles when the object got focus
        /// </summary>
        public virtual void GotFocus()
        {

        }

        /// <summary>
        /// Handles when the object lost focus
        /// </summary>
        public virtual void LostFocus()
        {

        }

        /// <summary>
        /// Updates the element
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last update</param>
        public virtual void Update(TimeSpan elapsed)
        {

        }

        /// <summary>
        /// Draws the element
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public abstract void Draw(DeviceContext deviceContext);

        public virtual void Dispose()
        {
            
        }
    }
}
