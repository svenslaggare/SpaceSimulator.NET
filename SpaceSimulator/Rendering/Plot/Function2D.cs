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
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Rendering.Plot
{
    /// <summary>
    /// Represents a rendering of 2D function
    /// </summary>
    public sealed class Function2D : IDisposable
    {
        private readonly RenderingManager2D renderingManager2D;

        /// <summary>
        /// The position where the figure is drawn at
        /// </summary>
        public Vector2 Position { get; set; }

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

        private readonly Vector2 minPosition;
        private readonly Vector2 maxPosition;
        private readonly Vector2 scale;

        /// <summary>
        /// Indicates if the background is drawn
        /// </summary>
        public bool DrawBackground { get; set; } = true;

        /// <summary>
        /// Creates a new plot of the given function values
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="values">The values</param>
        /// <param name="position">The position to draw the figure at</param>
        /// <param name="color">The color</param>
        /// <param name="width">The width of the plot</param>
        /// <param name="height">The height of the plot</param>
        /// <param name="labelAxisX">The label for the x-axis</param>
        /// <param name="labelAxisY">The label for the y-axis</param>
        /// <param name="splitIntoMultipleParts">Indicates if the function should be split into multiple parts</param>
        /// <param name="splitPart">Determines how the parts should be split</param>
        /// <param name="minPosition">The minimum position. Defaults to auto.</param>
        /// <param name="maxPosition">The maximum position. Defaults to auto.</param>
        public Function2D(
            RenderingManager2D renderingManager2D,
            IList<Vector2> values,
            Vector2 position,
            Color color,
            int width,
            int height,
            string labelAxisX = "",
            string labelAxisY = "",
            bool splitIntoMultipleParts = false,
            Func<double, double, bool> splitPart = null,
            Vector2? minPosition = null,
            Vector2? maxPosition = null)
        {
            this.Position = position;
            this.width = width;
            this.height = height;

            this.maxPosition = new Vector2(float.MinValue);
            this.minPosition = new Vector2(float.MaxValue);

            if (minPosition != null)
            {
                this.minPosition = minPosition.Value;
            }

            if (maxPosition != null)
            {
                this.maxPosition = maxPosition.Value;
            }

            if (!maxPosition.HasValue || !minPosition.HasValue)
            {
                foreach (var point in values)
                {
                    if (maxPosition == null)
                    {
                        this.maxPosition = Vector2.Max(this.maxPosition, point);
                    }

                    if (minPosition == null)
                    {
                        this.minPosition = Vector2.Min(this.minPosition, point);
                    }
                }
            }

            var range = this.maxPosition - this.minPosition;
            this.scale = new Vector2(this.width, this.height) / range;

            RawVector2 TransformPoint(Vector2 point)
            {
                var transformed = this.PlotPosition(point);
                return new RawVector2(transformed.X, transformed.Y);
            }

            this.renderingManager2D = renderingManager2D;
            this.linePathGeometry = this.renderingManager2D.CreatePathGeometry(geometrySink =>
            {
                if (splitIntoMultipleParts)
                {
                    var i = 0;
                    while (i < values.Count)
                    {
                        geometrySink.BeginFigure(TransformPoint(values[i]), FigureBegin.Filled);
                        var prev = values[i];
                        var end = values.Count;

                        for (int j = i + 1; j < values.Count; j++)
                        {
                            var value = values[j];
                            var diffX = value.X - prev.X;
                            var diffY = value.Y - prev.Y;

                            if (splitPart(diffX, diffY))
                            {
                                end = j;
                                break;
                            }

                            prev = value;
                        }

                        //Console.WriteLine($"{i}-{end}");
                        geometrySink.AddLines(values.GetRange(i, end - i).Select(point => TransformPoint(point)).ToArray());
                        i += end - i;

                        geometrySink.EndFigure(FigureEnd.Open);
                    }
                }
                else
                {
                    var firstPoint = TransformPoint(values[0]);
                    geometrySink.BeginFigure(firstPoint, FigureBegin.Filled);
                    geometrySink.AddLines(values.Select(point => TransformPoint(point)).ToArray());
                    geometrySink.EndFigure(FigureEnd.Open);
                }
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
        /// The width of the figure
        /// </summary>
        public int Width => this.width;

        /// <summary>
        /// The height of the figure
        /// </summary>
        public int Height => this.height;

        /// <summary>
        /// Returns the plot position for the given value
        /// </summary>
        /// <param name="value">The value</param>
        public Vector2 PlotPosition(Vector2 value)
        {
            value -= this.minPosition;
            return new RawVector2(value.X * this.scale.X, -value.Y * this.scale.Y + this.height);
        }

        /// <summary>
        /// Draws the plot
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Draw(DeviceContext deviceContext)
        {
            Rendering2DHelpers.BindResources(
                deviceContext,
                this.linePathGeometry,
                this.figureBackgroundBorderBrush,
                this.figureBackgroundBrush,
                this.figureBrush,
                this.textBrush);

            var figureAreaRectangle = new RectangleF(this.Position.X, this.Position.Y, this.width, this.height);

            if (this.DrawBackground)
            {
                this.figureBackgroundBrush.ApplyResource(brush =>
                {
                    deviceContext.FillRectangle(figureAreaRectangle, brush);
                });
            }

            this.figureBackgroundBorderBrush.ApplyResource(brush =>
            {
                deviceContext.DrawRectangle(figureAreaRectangle, brush);
            });

            this.figureBrush.ApplyResource(brush =>
            {
                this.linePathGeometry.DrawOutline(deviceContext, this.Position, brush);
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

        /// <summary>
        /// Releases the resources managed by the <see cref="RenderingManager2D"/> class
        /// </summary>
        public void ReleaseResources()
        {
            this.renderingManager2D.RemoveResource(this.linePathGeometry);
            this.renderingManager2D.RemoveResource(this.figureBackgroundBrush);
            this.renderingManager2D.RemoveResource(this.figureBackgroundBorderBrush);
            this.renderingManager2D.RemoveResource(this.figureBrush);
            this.renderingManager2D.RemoveResource(this.textBrush);
        }

        public void Dispose()
        {
            this.horizontalTextFormat.Dispose();
            this.verticalTextFormat.Dispose();
        }
    }
}
