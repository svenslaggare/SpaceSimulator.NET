using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represents an image UI object
    /// </summary>
    public class ImageUIObject : UIObject
    {
        private readonly RenderingImage2D image;

        /// <summary>
        /// Creates a new image UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="imageName">The name of the image</param>
        /// <param name="size">The size of the object</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public ImageUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            string imageName,
            Size2? size = null,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, size ?? Size2.Zero, positionRelationX, positionRelationY, parent)
        {
            this.image = renderingManager2D.LoadImage(imageName);
            if (size == null)
            {
                this.Size = this.image.Size;
            }
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.image.Draw(deviceContext, this.ScreenPosition);
        }
    }
}
