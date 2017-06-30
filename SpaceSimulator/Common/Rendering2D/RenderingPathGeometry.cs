using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SpaceSimulator.Common.Rendering2D
{
    /// <summary>
    /// A function for creating the geometry
    /// </summary>
    /// <param name="geometrySink">The geometry sink</param>
    public delegate void CreateGeometry(GeometrySink geometrySink);

    /// <summary>
    /// Represents a path geometry managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public class RenderingPathGeometry : IRenderingResource2D
    {
        private readonly CreateGeometry createGeometry;
        private PathGeometry pathGeometry;

        /// <summary>
        /// Creates a path geometry 
        /// </summary>
        /// <param name="createGeometry">The function to create the path geometry</param>
        public RenderingPathGeometry(CreateGeometry createGeometry)
        {
            this.createGeometry = createGeometry;
        }

        /// <summary>
        /// Creates path geometry for a upwards pointing arrow
        /// </summary>
        /// <param name="scale">The scale of the arrow</param>
        /// <returns>A function to create the geometry</returns>
        public static CreateGeometry UpArrow(float scale)
        {
            return geometrySink =>
            {
                geometrySink.BeginFigure(new Vector2(0.5f, 0.0f), FigureBegin.Filled);
                geometrySink.AddLines(new RawVector2[]
                {
                    new Vector2(0.5f, 0) * scale,
                    new Vector2(1.0f, 1.0f) * scale,
                    new Vector2(0.0f, 1.0f) * scale,
                    new Vector2(0.5f, 0) * scale,
                });
                geometrySink.EndFigure(FigureEnd.Closed);
            };
        }

        /// <summary>
        /// Creates path geometry for a downwards pointing arrow
        /// </summary>
        /// <param name="scale">The scale of the arrow</param>
        /// <returns>A function to create the geometry</returns>
        public static CreateGeometry DownArrow(float scale)
        {
            return geometrySink =>
            {
                geometrySink.BeginFigure(new Vector2(0, 0), FigureBegin.Filled);
                geometrySink.AddLines(new RawVector2[]
                {
                    new Vector2(0, 0) * scale,
                    new Vector2(1.0f, 0) * scale,
                    new Vector2(0.5f, 1.0f) * scale,
                });
                geometrySink.EndFigure(FigureEnd.Closed);
            };
        }

        /// <summary>
        /// Returtns the bounding rectangle
        /// </summary>
        public RectangleF BoundingRectangle
        {
            get
            {
                var bounds = pathGeometry.GetBounds();
                return new RectangleF(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            }
        }

        /// <summary>
        /// Updates the internal color brush to the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            if (this.pathGeometry != null)
            {
                this.pathGeometry.Dispose();
            }

            this.pathGeometry = new PathGeometry(deviceContext.Factory);
            using (var geometrySink = this.pathGeometry.Open())
            {
                this.createGeometry(geometrySink);
                geometrySink.Close();
            }
        }

        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        public void ApplyResource(Action<PathGeometry> fn)
        {
            fn(this.pathGeometry);
        }

        /// <summary>
        /// Draws the geometry filled
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position</param>
        /// <param name="brush">The brush</param>
        public void DrawFilled(DeviceContext deviceContext, Vector2 position, Brush brush)
        {
            var originalTransform = deviceContext.Transform;
            deviceContext.Transform = Matrix3x2.Translation(position);
            deviceContext.FillGeometry(this.pathGeometry, brush);
            deviceContext.Transform = originalTransform;
        }

        /// <summary>
        /// Draws the geometry outlines
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position</param>
        /// <param name="brush">The brush</param>
        public void DrawOutline(DeviceContext deviceContext, Vector2 position, Brush brush)
        {
            var originalTransform = deviceContext.Transform;
            deviceContext.Transform = Matrix3x2.Translation(position);
            deviceContext.DrawGeometry(this.pathGeometry, brush);
            deviceContext.Transform = originalTransform;
        }

        public void Dispose()
        {
            this.pathGeometry?.Dispose();
        }
    }
}
