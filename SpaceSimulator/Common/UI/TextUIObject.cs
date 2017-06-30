using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents a text UI object
    /// </summary>
    public class TextUIObject : UIObject
    {
        private readonly RenderingSolidColorBrush solidColorBrush;

        private string text;
        private Color textColor;

        private readonly TextFormat textFormat;
        private TextLayout textLayout;

        /// <summary>
        /// Creates a new text UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the object</param>
        /// <param name="position">The position of the object</param>
        /// <param name="size">The size of the object</param>
        /// <param name="text">The text</param>
        /// <param name="textColor">The color of the text</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public TextUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            string text,
            Color textColor,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, Size2.Zero, positionRelationX, positionRelationY, parent)
        {
            this.Text = text;
            this.textColor = textColor;
            this.solidColorBrush = renderingManager2D.CreateSolidColorBrush(textColor);

            this.textFormat = new TextFormat(this.RenderingManager2D.FontFactory, "Arial", 16)
            {
                TextAlignment = TextAlignment.Justified,
                ParagraphAlignment = ParagraphAlignment.Near
            };

            this.CalculateTextSize();
        }

        /// <summary>
        /// The text of the object
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;

                if (this.textLayout != null)
                {
                    this.CalculateTextSize();
                }
            }
        }

        /// <summary>
        /// The color of the text
        /// </summary>
        public Color TextColor
        {
            get { return this.textColor; }
            set
            {
                this.textColor = value;
                this.solidColorBrush.Color = value;
            }
        }

        /// <summary>
        /// Calculates the size of the text
        /// </summary>
        private void CalculateTextSize()
        {
            if (this.textLayout != null)
            {
                this.textLayout.Dispose();
            }

            this.textLayout = new TextLayout(
                this.RenderingManager2D.FontFactory,
                this.Text,
                this.textFormat,
                this.RenderingManager2D.ScreenRectangle.Width,
                this.RenderingManager2D.ScreenRectangle.Height);

            this.Size = new Size2(
                (int)Math.Round(this.textLayout.Metrics.Width),
                (int)Math.Round(this.textLayout.Metrics.Height));
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.solidColorBrush.DrawText(
                deviceContext,
                this.Text,
                this.textFormat,
                this.RenderingManager2D.TextPosition(this.ScreenPosition));
        }

        public override void Dispose()
        {
            base.Dispose();
            this.textFormat.Dispose();
            this.textLayout.Dispose();
        }
    }
}
