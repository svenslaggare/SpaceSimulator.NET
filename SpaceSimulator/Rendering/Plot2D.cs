using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents a rendering of 2D function
    /// </summary>
    public sealed class Plot2D : IDisposable
    {
        private readonly RenderingManager2D renderingManager2D;

        private readonly RenderingPathGeometry linePathGeometry;
        private readonly IRenderingBrush figureBackgroundBrush;
        private readonly IRenderingBrush figureBackgroundBorderBrush;
        private readonly IRenderingBrush figureBrush;

        private readonly IRenderingBrush textBrush;
        private readonly TextFormat horizontalTextFormat;
        private readonly TextFormat verticalTextFormat;

        private readonly string labelAxisX;
        private readonly string labelAxisY;

        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Creates a new plot of the given function values
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="values">The values</param>
        /// <param name="color">The color</param>
        /// <param name="width">The width of the plot</param>
        /// <param name="height">The height of the plot</param>
        /// <param name="labelAxisX">The label for the x-axis</param>
        /// <param name="labelAxisY">The label for the y-axis</param>
        public Plot2D(RenderingManager2D renderingManager2D, IList<Vector2> values, Color color, int width, int height, string labelAxisX = "", string labelAxisY = "")
        {
            this.width = width;
            this.height = height;

            var maxPosition = new Vector2(float.MinValue);
            var minPosition = new Vector2(float.MaxValue);
            foreach (var point in values)
            {
                maxPosition = Vector2.Max(maxPosition, point);
                minPosition = Vector2.Min(minPosition, point);
            }

            var range = maxPosition - minPosition;
            var scale = new Vector2(width, height) / range;

            RawVector2 TransformPoint(Vector2 point)
            {
                return new RawVector2(point.X * scale.X, -point.Y * scale.Y + height);
            }

            this.renderingManager2D = renderingManager2D;
            this.linePathGeometry = this.renderingManager2D.CreatePathGeometry(geometrySink =>
            {
                geometrySink.BeginFigure(TransformPoint(values[0]), FigureBegin.Filled);
                geometrySink.AddLines(values.Select(point => TransformPoint(point)).ToArray());
                geometrySink.EndFigure(FigureEnd.Open);
            });

            this.figureBackgroundBrush = this.renderingManager2D.CreateSolidColorBrush(Color.White);
            this.figureBackgroundBorderBrush = this.renderingManager2D.CreateSolidColorBrush(Color.Gray);
            this.figureBrush = this.renderingManager2D.CreateSolidColorBrush(color);

            this.labelAxisX = labelAxisX;
            this.labelAxisY = labelAxisY;
            this.textBrush = this.renderingManager2D.CreateSolidColorBrush(Color.Yellow);
            this.horizontalTextFormat = new TextFormat(this.renderingManager2D.FontFactory, "Arial", 16)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Far
            };

            this.verticalTextFormat = new TextFormat(this.renderingManager2D.FontFactory, "Arial", 16)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Near,
                ReadingDirection = ReadingDirection.TopToBottom,
                FlowDirection = FlowDirection.LeftToRight,
            };
        }

        /// <summary>
        /// Draws the plot
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position to draw the figure at</param>
        public void Draw(DeviceContext deviceContext, Vector2 position)
        {
            var figureAreaRectangle = new RectangleF(position.X, position.Y, this.width, this.height);

            this.figureBackgroundBrush.ApplyResource(brush =>
            {
                deviceContext.FillRectangle(figureAreaRectangle, brush);
            });

            this.figureBackgroundBorderBrush.ApplyResource(brush =>
            {
                deviceContext.DrawRectangle(figureAreaRectangle, brush);
            });

            this.figureBrush.ApplyResource(brush =>
            {
                this.linePathGeometry.DrawOutline(deviceContext, position, brush);
            });

            this.textBrush.ApplyResource(brush =>
            {
                deviceContext.DrawText(
                    this.labelAxisX,
                    this.horizontalTextFormat,
                    figureAreaRectangle.Move(new Vector2(0, 20)),
                    brush);

                deviceContext.DrawText(
                    this.labelAxisY,
                    this.verticalTextFormat,
                    figureAreaRectangle.Move(new Vector2(-20, 0)),
                    brush);
            });
        }

        public void Dispose()
        {
            this.horizontalTextFormat.Dispose();
            this.verticalTextFormat.Dispose();
        }
    }
}
