using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Windows;

namespace SpaceSimulator.Common.Rendering2D
{
    /// <summary>
    /// Represents a resource managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public interface IRenderingResource2D : IDisposable
    {
        /// <summary>
        /// Indicates if the internal resources has been bound to a device context
        /// </summary>
        bool HasBoundResources { get; }

        /// <summary>
        /// Updates the internal resource using the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        void Update(DeviceContext deviceContext);
    }

    /// <summary>
    /// Handles rendering in 2D
    /// </summary>
    /// <remarks>
    ///     The purpose of this class is to handle when the device context is recreated due to resizing of window.
    ///     All the resources loaded/created by this class is managed by it - it disposes them when the class is being disposed.
    /// </remarks>
    public class RenderingManager2D : IDisposable
    {
        private readonly RenderForm renderForm;
        private readonly IList<IRenderingResource2D> resources = new List<IRenderingResource2D>();

        /// <summary>
        /// The font factory
        /// </summary>
        public SharpDX.DirectWrite.Factory FontFactory { get; } = new SharpDX.DirectWrite.Factory();

        /// <summary>
        /// The default solid color brush
        /// </summary>
        public RenderingSolidColorBrush DefaultSolidColorBrush { get; }

        /// <summary>
        /// The default text format
        /// </summary>
        public TextFormat DefaultTextFormat { get; }

        /// <summary>
        /// Creates a new 2D rendering manager
        /// </summary>
        /// <param name="renderForm">The render form</param>
        public RenderingManager2D(RenderForm renderForm)
        {
            this.renderForm = renderForm;

            this.DefaultSolidColorBrush = this.CreateSolidColorBrush(Color.Yellow);
            this.DefaultTextFormat = new SharpDX.DirectWrite.TextFormat(this.FontFactory, "Arial", 16)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Justified,
                ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near
            };
        }

        /// <summary>
        /// Returns the screen rectangle
        /// </summary>
        public RectangleF ScreenRectangle
        {
            get { return new RectangleF(0, 0, this.renderForm.ClientSize.Width, this.renderForm.ClientSize.Height); }
        }

        /// <summary>
        /// Adds the given resource to list of resources managed by this class
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <remarks>This resource is managed by this class, and is disposed when the class is disposed.</remarks>
        public void AddResource(IRenderingResource2D resource)
        {
            this.resources.Add(resource);
        }

        /// <summary>
        /// Loads the given image
        /// </summary>
        /// <param name="fileName">The name of the file to load</param>
        /// <returns>The loaded image</returns>
        /// <remarks>This image is managed by this class, and is disposed when the class is disposed.</remarks>
        public RenderingImage2D LoadImage(string fileName)
        {
            var image = new RenderingImage2D(fileName);
            this.resources.Add(image);
            return image;
        }

        /// <summary>
        /// Creates a new solid color brush
        /// </summary>
        /// <param name="color">The color</param>
        /// <returns>The color brush</returns>
        public RenderingSolidColorBrush CreateSolidColorBrush(Color color)
        {
            var solidColorBrush = new RenderingSolidColorBrush(color);
            this.resources.Add(solidColorBrush);
            return solidColorBrush;
        }

        /// <summary>
        /// Creates a new linear gradient brush
        /// </summary>
        /// <param name="start">The start position</param>
        /// <param name="stop">The stop position</param>
        /// <param name="gradientStops">The gradient stops</param>
        /// <returns>The created brush</returns>
        public RenderingLinearGradientBrush CreateLinearGradientBrush(Vector2 start, Vector2 stop, GradientStop[] gradientStops)
        {
            var linearGradientBrush = new RenderingLinearGradientBrush(start, stop, gradientStops);
            this.resources.Add(linearGradientBrush);
            return linearGradientBrush;
        }

        /// <summary>
        /// Creates a new path geometry
        /// </summary>
        /// <param name="createGeometry">A function to create the geometry</param>
        public RenderingPathGeometry CreatePathGeometry(CreateGeometry createGeometry)
        {
            var pathGeometry = new RenderingPathGeometry(createGeometry);
            this.resources.Add(pathGeometry);
            return pathGeometry;
        }

        /// <summary>
        /// Returns a position relative to top-left corner
        /// </summary>
        /// <param name="position">The position</param>
        public RectangleF TextPosition(Vector2 position)
        {
            return new RectangleF(
                position.X,
                position.Y,
                this.renderForm.Width,
                this.renderForm.Height);
        }

        /// <summary>
        /// Updates the resources using the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            foreach (var resource in this.resources)
            {
                resource.Update(deviceContext);
            }
        }

        public void Dispose()
        {
            foreach (var resource in this.resources)
            {
                resource.Dispose();
            }

            this.FontFactory.Dispose();
            this.DefaultTextFormat.Dispose();
        }
    }
}
