using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Common.UI;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Contains the default UI style
    /// </summary>
    public class UIStyle
    {
        private readonly RenderingManager2D renderingManager2D;

        /// <summary>
        /// The button background brush
        /// </summary>
        public IRenderingBrush ButtonBackgroundBrush { get; }

        /// <summary>
        /// The button border brush
        /// </summary>
        public IRenderingBrush ButtonBorderBrush { get; }

        /// <summary>
        /// The color of the button text
        /// </summary>
        public Color ButtonTextColor { get; }

        /// <summary>
        /// Creates a new UI style
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        public UIStyle(RenderingManager2D renderingManager2D)
        {
            this.renderingManager2D = renderingManager2D;

            this.ButtonBackgroundBrush = this.renderingManager2D.CreateLinearGradientBrush(
                new Vector2(),
                new Vector2(0, 30),
                new GradientStop[]
                {
                    new GradientStop() { Color = new Color(6, 127, 251), Position = 0.0f },
                    new GradientStop() { Color = new Color(5, 65, 231), Position = 1.0f },
                });
            this.ButtonBorderBrush = this.renderingManager2D.CreateSolidColorBrush(new Color(255, 255, 255));
            this.ButtonTextColor = Color.Yellow;
        }

        /// <summary>
        /// Creates a new button background object
        /// </summary>
        /// <param name="size">The size of the object</param>
        /// <param name="parent">The parent</param>
        public UIObject CreateButtonBackground(Size2 size, UIObject parent = null)
        {
            return new RectangleUIObject(
                this.renderingManager2D,
                "Background",
                Vector2.Zero,
                size,
                this.ButtonBackgroundBrush,
                this.ButtonBorderBrush,
                parent: parent);
        }
    }
}
