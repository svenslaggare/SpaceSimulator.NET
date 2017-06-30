using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;

namespace SpaceSimulator.Common
{
    /// <summary>
    /// Represents a resource managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public interface IRenderingResource2D : IDisposable
    {
        /// <summary>
        /// Updates the internal resource using the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        void Update(DeviceContext deviceContext);
    }

    /// <summary>
    /// Represents an image managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public class RenderingImage2D : IRenderingResource2D
    {
        private readonly int stride;
        private readonly BitmapProperties bitmapProperties;
        private readonly DataStream dataStream;

        private Bitmap currentBitmap;

        /// <summary>
        /// Returns the size of the image
        /// </summary>
        public Size2 Size { get; }

        /// <summary>
        /// Creates a new image from the given file
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        public RenderingImage2D(string fileName)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(fileName))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                this.bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
                this.Size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                this.stride = bitmap.Width * sizeof(int);
                this.dataStream = new DataStream(bitmap.Height * stride, true, true);

                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized 
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        this.dataStream.Write(rgba);
                    }
                }

                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Creates a new image from the given memory pointer
        /// </summary>
        /// <param name="bitmapProperties">The properties for the image</param>
        /// <param name="size">The size of the image</param>
        /// <param name="dataStream">A pointer to the memory</param>
        public RenderingImage2D(BitmapProperties bitmapProperties, Size2 size, DataStream dataStream)
        {
            this.stride = sizeof(int) * size.Width;
            this.bitmapProperties = bitmapProperties;
            this.Size = size;
            this.dataStream = dataStream;
        }

        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        public void ApplyResource(Action<Bitmap> fn)
        {
            fn(this.currentBitmap);
        }

        /// <summary>
        /// Updates the internal image to the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            if (this.currentBitmap != null)
            {
                this.currentBitmap.Dispose();
            }

            this.dataStream.Position = 0;
            this.currentBitmap = new Bitmap(deviceContext, this.Size, this.dataStream, this.stride, this.bitmapProperties);
        }

        /// <summary>
        /// Draws the image at the given position
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position</param>
        public void Draw(DeviceContext deviceContext, Vector2 position)
        {
            deviceContext.DrawBitmap(
                this.currentBitmap,
                1.0f,
                SharpDX.Direct2D1.InterpolationMode.Linear,
                new RectangleF(0, 0, this.Size.Width, this.Size.Height),
                Matrix.Translation(position.X, position.Y, 0));
        }

        public void Dispose()
        {
            this.dataStream.Dispose();

            if (this.currentBitmap != null)
            {
                this.currentBitmap.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents a brush managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public interface IRenderingBrush : IRenderingResource2D
    {
        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        void ApplyResource(Action<Brush> fn);

        /// <summary>
        /// Sets where the brush should be apply
        /// </summary>
        /// <param name="position">The position</param>
        void SetPosition(Vector2 position);
    }

    /// <summary>
    /// Represents a solid color brush managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public class RenderingSolidColorBrush : IRenderingBrush
    {
        private Color color;
        private SolidColorBrush solidColorBrush;

        /// <summary>
        /// Creates a new color brush
        /// </summary>
        /// <param name="color">The color of the brush</param>
        public RenderingSolidColorBrush(Color color)
        {
            this.color = color;
        }

        /// <summary>
        /// The color of the brush
        /// </summary>
        public Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                if (this.solidColorBrush != null)
                {
                    this.solidColorBrush.Color = this.color;
                }
            }
        }

        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        public void ApplyResource(Action<Brush> fn)
        {
            fn(this.solidColorBrush);
        }

        /// <summary>
        /// Sets where the brush should be applied
        /// </summary>
        /// <param name="position">The position</param>
        public void SetPosition(Vector2 position)
        {

        }

        /// <summary>
        /// Updates the internal color brush to the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            if (this.solidColorBrush != null)
            {
                this.solidColorBrush.Dispose();
            }

            this.solidColorBrush = new SolidColorBrush(deviceContext, this.Color);
        }

        /// <summary>
        /// Draws the given text
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="text">The text</param>
        /// <param name="textFormat">The text format</param>
        /// <param name="layoutRectangle">The position and size of the text</param>
        public void DrawText(DeviceContext deviceContext, string text, TextFormat textFormat, RectangleF layoutRectangle)
        {
            deviceContext.DrawText(
                text,
                textFormat,
                layoutRectangle,
                this.solidColorBrush);
        }

        public void Dispose()
        {
            if (this.solidColorBrush != null)
            {
                this.solidColorBrush.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents a linear gradient brush managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public class RenderingLinearGradientBrush : IRenderingBrush
    {
        private readonly Vector2 start;
        private readonly Vector2 stop;
        private readonly GradientStop[] gradientStops;

        private LinearGradientBrush linearGradientBrush;
        private GradientStopCollection gradientStopCollection;

        /// <summary>
        /// Creates a new linear gradient brush
        /// </summary>
        /// <param name="start">The start position</param>
        /// <param name="stop">The stop position</param>
        /// <param name="gradientStops">The gradient stops</param>
        public RenderingLinearGradientBrush(Vector2 start, Vector2 stop, GradientStop[] gradientStops)
        {
            this.start = start;
            this.stop = stop;
            this.gradientStops = gradientStops.ToArray();
        }

        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        public void ApplyResource(Action<Brush> fn)
        {
            fn(this.linearGradientBrush);
        }

        /// <summary>
        /// Sets where the brush should be applied
        /// </summary>
        /// <param name="position">The position</param>
        public void SetPosition(Vector2 position)
        {
            this.linearGradientBrush.Transform = Matrix3x2.Translation(position);
        }

        /// <summary>
        /// Updates the internal color brush to the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            if (this.linearGradientBrush != null)
            {
                this.linearGradientBrush.Dispose();
                this.gradientStopCollection.Dispose();
            }

            this.gradientStopCollection = new GradientStopCollection(deviceContext, this.gradientStops);

            this.linearGradientBrush = new LinearGradientBrush(
                deviceContext,
                new LinearGradientBrushProperties()
                {
                    StartPoint = this.start,
                    EndPoint = this.stop
                }, 
                this.gradientStopCollection);
        }

        public void Dispose()
        {
            if (this.linearGradientBrush != null)
            {
                this.linearGradientBrush.Dispose();
                this.gradientStopCollection.Dispose();
            }
        }
    }

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
