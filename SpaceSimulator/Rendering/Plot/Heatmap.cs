﻿using System;
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
        private readonly IList<ColorRange> colorScheme;
        private readonly IList<HeatmapValue> values = new List<HeatmapValue>();
        private readonly RenderingImage2D heatmapImage;

        private readonly int minimumValueCrossSize;
        private readonly bool showMinimumValue;

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
        /// <param name="colorScheme">The color scheme</param>
        /// <param name="values">The values</param>
        /// <param name="showMinimumValue">Indicates if the minimum value is shown as a cross</param>
        /// <param name="minimumValueCrossSize">The size of the minimum value cross</param>
        public Heatmap(RenderingManager2D renderingManager2D, IList<ColorRange> colorScheme, IList<HeatmapValue> values, bool showMinimumValue, int minimumValueCrossSize = 5)
        {
            this.renderingManager2D = renderingManager2D;
            this.colorScheme = colorScheme;
            this.values = values;

            this.showMinimumValue = showMinimumValue;
            this.minimumValueCrossSize = minimumValueCrossSize;

            this.heatmapImage = this.CreateImage();
            this.renderingManager2D.AddResource(this.heatmapImage);
        }

        /// <summary>
        /// Creates a delta V chart
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="possibleLaunches">The possible launches</param>
        /// <param name="deltaVLimit">The maximum allowed delta V</param>
        public static Heatmap CreateDeltaVChart(RenderingManager2D renderingManager2D, IList<Physics.Maneuvers.InterceptManeuver.PossibleLaunch> possibleLaunches, double deltaVLimit = 30E3)
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
                    //new Vector2d(possibleLaunch.ArrivalTime, possibleLaunch.StartTime),
                    deltaV));

                minValue = Math.Min(minValue, deltaV);
                maxValue = Math.Max(maxValue, deltaV);
            }

            return new Heatmap(
                renderingManager2D,
                Heatmap.DeltaVColorScheme(minValue, maxValue),
                new List<Heatmap.HeatmapValue>(values),
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
        /// Creates the heatmap image
        /// </summary>
        private RenderingImage2D CreateImage()
        {
            //Find the min and max of the values
            var minPosition = new Vector2d(double.MaxValue);
            var maxPosition = new Vector2d(double.MinValue);
            var minIntensity = double.MaxValue;
            var maxIntensity = double.MinValue;
            var minIntensityPosition = Vector2d.Zero;
            var maxIntensityPosition = Vector2d.Zero;
            var deltaPosition = Vector2d.Zero;

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

            //Create the memory
            var width = (int)Math.Ceiling((maxPosition.X - minPosition.X) / deltaPosition.X);
            var height = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / deltaPosition.Y);
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
                var x = (int)((value.Position.X - minPosition.X) / deltaPosition.X);
                var y = (int)((value.Position.Y - minPosition.Y) / deltaPosition.Y);
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
        /// Draws the heatmap at the given position
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position to draw at</param>
        public void Draw(DeviceContext deviceContext, Vector2 position)
        {
            if (!this.heatmapImage.HasBoundResources)
            {
                this.heatmapImage.Update(deviceContext);
            }

            this.heatmapImage.Draw(deviceContext, position);
        }
    }
}