using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Represens a list box UI object
    /// </summary>
    public class ListBoxUIObject : UIObject
    {
        private readonly IList<Item> items;
        private int selectedItemIndex;

        private readonly UIObject backgroundObject;
        private readonly TextUIObject selectedItemTextObject;
        private readonly UIObject upArrowObject;
        private readonly UIObject downArrowObject;

        private const int boxHeight = 30;

        /// <summary>
        /// Represents an item
        /// </summary>
        public class Item
        {
            /// <summary>
            /// The text of the item
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// A tag for the item
            /// </summary>
            public object Tag { get; }

            /// <summary>
            /// Creates a new item
            /// </summary>
            /// <param name="text">The text of the item</param>
            /// <param name="tag">The tag</param>
            public Item(string text, object tag)
            {
                this.Text = text;
                this.Tag = tag;
            }
        }

        /// <summary>
        /// Creates a new list box UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="items">The items</param>
        /// <param name="width">The width of the box</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public ListBoxUIObject(
            RenderingManager2D renderingManager2D,
            string name,
            Vector2 position,
            int width,
            IList<Item> items,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, new Size2(width, boxHeight), positionRelationX, positionRelationY, parent)
        {
            this.items = new List<Item>(items);

            this.backgroundObject = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                Vector2.Zero,
                this.Size,
                this.RenderingManager2D.CreateSolidColorBrush(new Color(255, 248, 242)),
                this.RenderingManager2D.CreateSolidColorBrush(new Color(160, 160, 160)),
                cornerRadius: 0,
                parent: this);

            this.selectedItemTextObject = new TextUIObject(
                this.RenderingManager2D,
                "Text",
                new Vector2(5, 5),
                this.SelectedItem?.Text ?? "",
                Color.Black,
                parent: this);

            var scale = 12f;
            var controlOffsetRight = scale + 3;
            var controlOffsetTop = 2;
            var controlBrush = this.RenderingManager2D.CreateSolidColorBrush(Color.Black);

            this.upArrowObject = new PathGeometryUIObject(
                this.RenderingManager2D,
                "Up",
                new Vector2(this.Size.Width - controlOffsetRight, controlOffsetTop),
                RenderingPathGeometry.UpArrow(scale),
                controlBrush,
                parent: this);

            this.downArrowObject = new PathGeometryUIObject(
                this.RenderingManager2D,
                "Down",
                new Vector2(this.Size.Width - controlOffsetRight, controlOffsetTop + scale + 2),
                RenderingPathGeometry.DownArrow(scale),
                controlBrush,
                parent: this);
        }

        /// <summary>
        /// The index of the selected item
        /// </summary>
        public int SelectedItemIndex
        {
            get { return this.selectedItemIndex; }
            set
            {
                if (value >= 0 && value < this.items.Count)
                {
                    this.selectedItemIndex = value;
                    this.selectedItemTextObject.Text = this.SelectedItem?.Text ?? "";
                }
            }
        }

        /// <summary>
        /// Returns the number of items
        /// </summary>
        public int ItemCount
        {
            get { return this.items.Count; }
        }

        /// <summary>
        /// Returns the selected item
        /// </summary>
        public Item SelectedItem
        {
            get
            {
                if (this.items.Count == 0)
                {
                    return null;
                }

                return this.items[this.selectedItemIndex];
            }
        }

        /// <summary>
        /// Sets the items
        /// </summary>
        /// <param name="items">The new items</param>
        public void SetItems(IList<Item> items)
        {
            this.items.Clear();
            foreach (var item in items)
            {
                this.items.Add(item);
            }

            this.SelectedItemIndex = 0;
        }

        public override void HandleClicked(Vector2 mousePosition, MouseButtons button)
        {
            base.HandleClicked(mousePosition, button);

            var deltaIndex = 0;
            if (this.upArrowObject.BoundingRectangle.Contains(mousePosition))
            {
                deltaIndex -= 1;
            }
            else if (this.downArrowObject.BoundingRectangle.Contains(mousePosition))
            {
                deltaIndex += 1;
            }

            if (deltaIndex != 0)
            {
                this.SelectedItemIndex += deltaIndex;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            this.backgroundObject.Invalidate();
            this.selectedItemTextObject.Invalidate();
            this.upArrowObject.Invalidate();
            this.downArrowObject.Invalidate();
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.backgroundObject.Draw(deviceContext);
            this.selectedItemTextObject.Draw(deviceContext);
            this.upArrowObject.Draw(deviceContext);
            this.downArrowObject.Draw(deviceContext);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.backgroundObject.Dispose();
            this.selectedItemTextObject.Dispose();
            this.upArrowObject.Dispose();
            this.downArrowObject.Dispose();
        }
    }
}
