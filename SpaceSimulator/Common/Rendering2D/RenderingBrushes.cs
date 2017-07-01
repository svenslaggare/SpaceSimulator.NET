using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace SpaceSimulator.Common.Rendering2D
{
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
        /// Indicates if the internal resources has been bound to a device context
        /// </summary>
        public bool HasBoundResources => this.solidColorBrush != null;

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
        /// Indicates if the internal resources has been bound to a device context
        /// </summary>
        public bool HasBoundResources => this.linearGradientBrush != null;

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

}
