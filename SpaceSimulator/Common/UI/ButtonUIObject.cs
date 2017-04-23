using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents a button UI object
    /// </summary>
    public class ButtonUIObject : UIObject
    {
        private readonly UIObject backgroundObject;
        private readonly TextUIObject textObject;

        /// <summary>
        /// Creates a new UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="size">The size. If null, then the size of the background object is used.</param>
        /// <param name="createBackgroundObject">Function for creating the background object. This object is supplied as parameter, and should be set as the parent.</param>
        /// <param name="buttonText">The text of the button</param>
        /// <param name="buttonTextColor">The color of the text</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public ButtonUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            Func<UIObject, UIObject> createBackgroundObject,
            string buttonText,
            Color buttonTextColor,
            Size2? size = null,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, size ?? Size2.Zero, positionRelationX, positionRelationY, parent)
        {
            this.backgroundObject = createBackgroundObject(this);
            this.textObject = new TextUIObject(
                renderingManager2D,
                "Text",
                Vector2.Zero,
                buttonText,
                buttonTextColor,
                parent: this,
                positionRelationX: PositionRelationX.Center,
                positionRelationY: PositionRelationY.Center);

            if (size == null)
            {
                this.Size = this.backgroundObject.Size;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            this.backgroundObject.Invalidate();
            this.textObject.Invalidate();
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.backgroundObject.Draw(deviceContext);
            this.textObject.Draw(deviceContext);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.backgroundObject.Dispose();
            this.textObject.Dispose();
        }
    }
}
