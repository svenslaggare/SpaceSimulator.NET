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
    /// Represents a rectangle UI object
    /// </summary>
    public class RectangleUIObject : UIObject
    {
        private readonly IRenderingBrush fillBrush;
        private readonly IRenderingBrush borderBrush;
        private readonly float cornerRadius;

        /// <summary>
        /// Creates a new rectangle UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="image">The name of the image</param>
        /// <param name="size">The size of the object</param>
        /// <param name="fillBrush">The fill brush</param>
        /// <param name="borderBrush">The border brush</param>
        /// <param name="cornerRadius">The corner radius</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public RectangleUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            Size2 size,
            IRenderingBrush fillBrush,
            IRenderingBrush borderBrush,
            float cornerRadius = 5.0f,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, size, positionRelationX, positionRelationY, parent)
        {
            this.fillBrush = fillBrush;
            this.borderBrush = borderBrush;
            this.cornerRadius = cornerRadius;
        }

        public override void Draw(DeviceContext deviceContext)
        {
            var roundedRectangle = new RoundedRectangle()
            {
                Rect = this.BoundingRectangle,
                RadiusX = this.cornerRadius,
                RadiusY = this.cornerRadius
            };

            this.fillBrush.ApplyResource(brush =>
            {
                this.fillBrush.SetPosition(this.ScreenPosition);
                deviceContext.FillRoundedRectangle(
                    roundedRectangle,
                    brush);
            });

            this.borderBrush.ApplyResource(brush =>
            {
                this.borderBrush.SetPosition(this.ScreenPosition);
                deviceContext.DrawRoundedRectangle(
                    roundedRectangle,
                    brush,
                    1.5f);
            });
        }
    }
}
