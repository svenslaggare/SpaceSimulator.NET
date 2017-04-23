using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents a path geometry UI object
    /// </summary>
    /// <remarks>The points used by the paths should only be non-negative.</remarks>
    public class PathGeometryUIObject : UIObject
    {
        private readonly RenderingPathGeometry pathGeometry;
        private readonly IRenderingBrush brush;

        /// <summary>
        /// Creates a new path geometry object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="createGeometry">A function for creating the geometry</param>
        /// <param name="brush">The brush to render the geometry with</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public PathGeometryUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            CreateGeometry createGeometry,
            IRenderingBrush brush,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, Size2.Zero, positionRelationX, positionRelationY, parent)
        {
            this.pathGeometry = renderingManager2D.CreatePathGeometry(createGeometry);
            this.brush = brush;
        }

        public override void Draw(DeviceContext deviceContext)
        {
            if (this.Size == Size2.Zero)
            {
                var size = this.pathGeometry.BoundingRectangle.Size;
                this.Size = new Size2((int)Math.Round(size.Width), (int)Math.Round(size.Height));
            }

            this.brush.ApplyResource(brush =>
            {
                this.pathGeometry.Draw(deviceContext, this.ScreenPosition, brush);
            });
        }
    }
}
