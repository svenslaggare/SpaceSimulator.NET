using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Rendering.Plot
{
    /// <summary>
    /// Plots a heatmap
    /// </summary>
    public sealed class Heatmap
    {
        private readonly RenderingManager2D renderingManager2D;

        /// <summary>
        /// The position where the figure is drawn at
        /// </summary>
        public Vector2 Position { get; set; }

        private Vector2 mousePosition;

        private readonly IList<HeatmapValue> values = new List<HeatmapValue>();

        private Vector2d minPosition;
        private Vector2d maxPosition;
        private double minIntensity;
        private double maxIntensity;
        private Vector2d minIntensityPosition;
        private Vector2d maxIntensityPosition;
        private Vector2d deltaPosition;
        private HeatmapValue?[,] valueMatrix;

        private readonly IList<ColorRange> colorScheme;
        private readonly RenderingImage2D heatmapImage;
        private readonly int minimumValueCrossSize;
        private readonly bool showMinimumValue;
        private readonly Func<HeatmapValue, string> formatHoverValue;

        /// <summary>
        /// Represents a range of colors
        /// </summary>
        public struct ColorRange
        {
            /// <summary>
            /// The minimum value
            /// </summary>
            public double Min { get; }

            /// <summary>
            /// The maximum value
            /// </summary>
            public double Max { get; }

            /// <summary>
            /// The color for the minimum value
            /// </summary>
            public Color MinColor { get; }

            /// <summary>
            /// The color for the maximum value
            /// </summary>
            public Color MaxColor { get; }

            /// <summary>
            /// Creates a new color range
            /// </summary>
            /// <param name="min">The minimum value</param>
            /// <param name="max">The maximum value</param>
            /// <param name="minColor">The color for the minimum value</param>
            /// <param name="maxColor">The color for the maximum value</param>
            public ColorRange(double min, double max, Color minColor, Color maxColor)
            {
                this.Min = min;
                this.Max = max;
                this.MinColor = minColor;
                this.MaxColor = maxColor;
            }

            public override string ToString()
            {
                return "min: " + Min + ", max: " + Max + ", min color: " + MinColor + ", max color: " + MaxColor;
            }
        }

        /// <summary>
        /// Represents a value for the heatmap
        /// </summary>
        public struct HeatmapValue
        {
            /// <summary>
            /// The position
            /// </summary>
            public Vector2d Position { get; }

            /// <summary>
            /// The intensity of the value
            /// </summary>
            public double Intensity { get; }

            /// <summary>
            /// Creates a new heatmap value
            /// </summary>
            /// <param name="position">The position</param>
            /// <param name="intensity">The intensity</param>
            public HeatmapValue(Vector2d position, double intensity)
            {
                this.Position = position;
                this.Intensity = intensity;
            }
        }

        /// <summary>
        /// Creates a new heatmap plotter using the given color scheme
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="position">The position to draw the figure at</param>
        /// <param name="colorScheme">The color scheme</param>
        /// <param name="values">The values</param>
        /// <param name="formatHoverValue">Formats the hover value</param>
        /// <param name="showMinimumValue">Indicates if the minimum value is shown as a cross</param>
        /// <param name="minimumValueCrossSize">The size of the minimum value cross</param>
        public Heatmap(
            RenderingManager2D renderingManager2D,
            Vector2 position,
            IList<ColorRange> colorScheme,
            IList<HeatmapValue> values,
            Func<HeatmapValue, string> formatHoverValue,
            bool showMinimumValue,
            int minimumValueCrossSize = 5)
        {
            this.renderingManager2D = renderingManager2D;
            this.Position = position;

            this.colorScheme = colorScheme;
            this.values = values;

            this.formatHoverValue = formatHoverValue;
            this.showMinimumValue = showMinimumValue;
            this.minimumValueCrossSize = minimumValueCrossSize;

            this.heatmapImage = this.CreateImage();
            this.renderingManager2D.AddResource(this.heatmapImage);
        }

        /// <summary>
        /// Creates a delta V chart
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="position">The position to draw at</param>
        /// <param name="possibleLaunches">The possible launches</param>
        /// <param name="deltaVLimit">The maximum allowed delta V</param>
        public static Heatmap CreateDeltaVChart(
            RenderingManager2D renderingManager2D,
            Vector2 position,
            IList<Physics.Maneuvers.InterceptManeuver.PossibleLaunch> possibleLaunches,
            double deltaVLimit = 30E3)
        {
            var minValue = double.MaxValue;
            var maxValue = double.MinValue;
            var values = new List<Heatmap.HeatmapValue>();

            foreach (var possibleLaunch in possibleLaunches)
            {
                var deltaV = possibleLaunch.DeltaVelocity.Length();
                deltaV = Math.Min(deltaV, deltaVLimit);

                values.Add(new Heatmap.HeatmapValue(
                    new Vector2d(possibleLaunch.Duration, possibleLaunch.StartTime),
                    deltaV));

                minValue = Math.Min(minValue, deltaV);
                maxValue = Math.Max(maxValue, deltaV);
            }

            return new Heatmap(
                renderingManager2D,
                position,
                Heatmap.DeltaVColorScheme(minValue, maxValue),
                new List<Heatmap.HeatmapValue>(values),
                value => $"Depature time: {DataFormatter.Format(value.Position.Y, DataUnit.Time)}, " +
                         $"duration: {DataFormatter.Format(value.Position.X, DataUnit.Time)}, " +
                         $"Δv: {DataFormatter.Format(value.Intensity, DataUnit.Velocity, numDecimals: 2)}",
                true);
        }

        /// <summary>
        /// Creates the color scheme for delta V heatmaps
        /// </summary>
        /// <param name="min">The min value</param>
        /// <param name="max">The max value</param>
        /// <param name="optimalRegionSize">The size of the optimal deltaV region</param>
        public static IList<ColorRange> DeltaVColorScheme(double min, double max, double optimalRegionSize = 100.0)
        {
            var stepSize = (max - min) / 5.0;

            var currentMin = min - stepSize + optimalRegionSize;
            var currentMax = min + optimalRegionSize;

            var lightBlue = new Color(0, 148.0f / 255, 1.0f);
            var blue = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            var cyan = Color.Cyan;
            var red = Color.Red;
            var green = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            var yellow = new Color(1.0f, 0.92f, 0.016f, 1.0f);

            return new List<ColorRange>()
            {
                new ColorRange(min, min + optimalRegionSize, blue, lightBlue),
                new ColorRange(currentMin += stepSize, currentMax += stepSize, lightBlue, cyan),
                new ColorRange(currentMin += stepSize, currentMax += stepSize, cyan, green),
                new ColorRange(currentMin += stepSize, currentMax += stepSize, green, yellow),
                new ColorRange(currentMin += stepSize, currentMax += stepSize, yellow, red),
                new ColorRange(currentMin += stepSize, max, red, red),
            };
        }

        /// <summary>
        /// Determines the min and maximum positions/intensities of the values
        /// </summary>
        private void DetermineMinAndMax()
        {
            //Find the min and max of the values
            this.minPosition = new Vector2d(double.MaxValue);
            this.maxPosition = new Vector2d(double.MinValue);
            this.minIntensity = double.MaxValue;
            this.maxIntensity = double.MinValue;
            this.minIntensityPosition = Vector2d.Zero;
            this.maxIntensityPosition = Vector2d.Zero;
            this.deltaPosition = Vector2d.Zero;

            var prevValue = this.values[0];
            foreach (var value in this.values)
            {
                minPosition = Vector2d.Min(minPosition, value.Position);
                maxPosition = Vector2d.Max(maxPosition, value.Position);
                minIntensity = Math.Min(minIntensity, value.Intensity);
                maxIntensity = Math.Max(maxIntensity, value.Intensity);

                if (value.Intensity == minIntensity)
                {
                    minIntensityPosition = value.Position;
                }

                if (value.Intensity == maxIntensity)
                {
                    maxIntensityPosition = value.Position;
                }

                if (deltaPosition.X == 0)
                {
                    var deltaX = value.Position.X - prevValue.Position.X;
                    if (deltaX != 0.0)
                    {
                        deltaPosition.X = deltaX;
                    }
                }

                if (deltaPosition.Y == 0)
                {
                    var deltaY = value.Position.Y - prevValue.Position.Y;
                    if (deltaY != 0.0)
                    {
                        deltaPosition.Y = deltaY;
                    }
                }

                prevValue = value;
            }
        }

        /// <summary>
        /// Returns the color for the given value
        /// </summary>
        /// <param name="value">The value</param>
        private Color GetColor(double value)
        {
            foreach (var color in this.colorScheme)
            {
                if (value >= color.Min && value <= color.Max)
                {
                    return Color.Lerp(
                        color.MinColor,
                        color.MaxColor,
                        (float)MiscHelpers.RangeNormalize(color.Min, color.Max, value));
                }
            }

            return Color.Gray;
        }

        /// <summary>
        /// Returns the pixel position
        /// </summary>
        /// <param name="position">The position in the heatmap</param>
        private (int, int) GetPixelPosition(Vector2d position)
        {
            var x = (int)((position.X - this.minPosition.X) / this.deltaPosition.X);
            var y = (int)((position.Y - this.minPosition.Y) / this.deltaPosition.Y);
            return (x, y);
        }

        /// <summary>
        /// Creates the heatmap image
        /// </summary>
        private RenderingImage2D CreateImage()
        {
            this.DetermineMinAndMax();

            var width = (int)Math.Ceiling((maxPosition.X - minPosition.X) / deltaPosition.X);
            var height = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / deltaPosition.Y);

            //Create the heatmap matrix
            this.valueMatrix = new HeatmapValue?[width, height];
            foreach (var value in this.values)
            {
                (var x, var y) = this.GetPixelPosition(value.Position);
                if (x >= 0 && y >= 0 && x < width && y < height)
                {
                    this.valueMatrix[x, y] = value;
                }
            }

            //Create the memory
            var dataStream = new DataStream(sizeof(int) * width * height, true, true);

            void SetPixel(int x, int y, Color color)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    int rgba = color.R | (color.G << 8) | (color.B << 16) | (color.A << 24);
                    dataStream.Position = (y * width + x) * sizeof(int);
                    dataStream.Write(rgba);
                }
            }

            //Set to default color
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SetPixel(x, y, Color.Gray);
                }
            }

            var minIntensityPixelPosition = Point.Zero;
            var maxIntensityPixelPosition = Point.Zero;

            //Fill in heatmap
            foreach (var value in this.values)
            {
                (var x, var y) = this.GetPixelPosition(value.Position);
                SetPixel(x, y, this.GetColor(value.Intensity));

                if (value.Intensity == minIntensity)
                {
                    minIntensityPixelPosition = new Point(x, y);
                }

                if (value.Intensity == maxIntensity)
                {
                    maxIntensityPixelPosition = new Point(x, y);
                }
            }

            //Add a cross for the optimal
            if (this.showMinimumValue)
            {
                for (int i = -this.minimumValueCrossSize / 2; i <= this.minimumValueCrossSize / 2; i++)
                {
                    SetPixel((minIntensityPixelPosition.X + i), minIntensityPixelPosition.Y, Color.White);
                    SetPixel(minIntensityPixelPosition.X, (minIntensityPixelPosition.Y + i), Color.White);
                }
            }

            dataStream.Position = 0;
            return new RenderingImage2D(
                new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)),
                new Size2(width, height),
                dataStream);
        }

        /// <summary>
        /// Sets the position of the mouse
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        public void SetMousePosition(Vector2 mousePosition)
        {
            this.mousePosition = mousePosition;
        }

        /// <summary>
        /// Draws the heatmap at the given position
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Draw(DeviceContext deviceContext)
        {
            if (!this.heatmapImage.HasBoundResources)
            {
                this.heatmapImage.Update(deviceContext);
            }

            this.heatmapImage.Draw(deviceContext, this.Position);

            var hoverPosition = (Point)(this.mousePosition - this.Position);

            if (hoverPosition.X >= 0
                && hoverPosition.Y >= 0
                && hoverPosition.X < this.heatmapImage.Size.Width
                && hoverPosition.Y < this.heatmapImage.Size.Height)
            {
                var hoverValue = this.valueMatrix[hoverPosition.X, hoverPosition.Y];

                if (hoverValue.HasValue)
                {
                    var textFormat = this.renderingManager2D.DefaultTextFormat;
                    var hoverValueString = this.formatHoverValue(hoverValue.Value);
                    var textSize = this.renderingManager2D.TextSize(textFormat, hoverValueString);

                    this.renderingManager2D.DefaultSolidColorBrush.DrawText(
                        deviceContext,
                        hoverValueString,
                        textFormat,
                        this.renderingManager2D.TextPosition(this.Position + new Vector2(
                            this.heatmapImage.Size.Width / 2 - textSize.Width / 2,
                            this.heatmapImage.Size.Height)));
                }
            }
        }

        /// <summary>
        /// Releases the resources managed by the <see cref="RenderingManager2D"/> class
        /// </summary>
        public void ReleaseResources()
        {
            this.renderingManager2D.RemoveResource(this.heatmapImage);
        }
    }
}
