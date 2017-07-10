using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Common.UI;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Builds an UI
    /// </summary>
    public sealed class UIBuilder
    {
        private readonly RenderingManager2D renderingManager2D;
        private readonly KeyboardManager keyboardManager;
        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;
        private readonly UIGroup parent;

        /// <summary>
        /// How much the y position is changed for each new element
        /// </summary>
        public float DeltaY { get; set; } = 40.0f;

        /// <summary>
        /// The position relation in the x-axis
        /// </summary>
        public PositionRelationX PositionRelationX { get; set; } = PositionRelationX.Left;

        /// <summary>
        /// The position relation in the y-axis
        /// </summary>
        public PositionRelationY PositionRelationY { get; set; } = PositionRelationY.Top;

        /// <summary>
        /// The default button width
        /// </summary>
        public const int DefaultButtonWidth = 155;

        /// <summary>
        /// The default button height
        /// </summary>
        public const int DefaultButtonHeight = 30;

        /// <summary>
        /// The width of a button
        /// </summary>
        public int ButtonWidth { get; set; } = DefaultButtonWidth;

        /// <summary>
        /// The height of a button
        /// </summary>
        public int ButtonHeight { get; set; } = DefaultButtonHeight;

        /// <summary>
        /// The width of a text input
        /// </summary>
        public int TextInputWidth { get; set; } = 150;

        private bool isFirst = true;
        private float currentPositionX = 0;
        private float currentPositionY = 0;

        /// <summary>
        /// Creates a new UI builder with the given parent
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="parent">The parent</param>
        /// <param name="uiStyle">The UI style</param>
        public UIBuilder(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, UIManager uiManager, UIStyle uiStyle, UIGroup parent)
        {
            this.renderingManager2D = renderingManager2D;
            this.keyboardManager = keyboardManager;
            this.uiManager = uiManager;
            this.uiStyle = uiStyle;
            this.parent = parent;
        }

        /// <summary>
        /// Resets the position to the given
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        public void ResetPosition(float x, float y)
        {
            this.currentPositionX = x;
            this.currentPositionY = y;
            this.isFirst = true;
        }

        /// <summary>
        /// Returns the next position
        /// </summary>
        /// <param name="isButton">Indicates if button</param>
        private Vector2 NextPosition(bool isButton)
        {
            if (!this.isFirst)
            {
                this.currentPositionY += this.DeltaY;
            }

            var nextPosition = new Vector2(
                this.currentPositionX + (isButton ? -2.5f : 0),
                this.currentPositionY);

            this.isFirst = false;
            return nextPosition;
        }

        /// <summary>
        /// Adds a new object
        /// </summary>
        /// <param name="newObject">The new object</param>
        private void AddObject(UIObject newObject)
        {
            if (this.parent != null)
            {
                this.parent.AddObject(newObject);
            }
            else
            {
                this.uiManager.AddElement(newObject);
            }
        }

        /// <summary>
        /// Adds a new button
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="text">The text of the button</param>
        /// <param name="leftMouseClick">The left mouse click action</param>
        public void AddButton(string name, string text, Action leftMouseClick)
        {
            var button = new ButtonUIObject(
                this.renderingManager2D,
                name,
                this.NextPosition(true),
                createParent => this.uiStyle.CreateButtonBackground(new Size2(this.ButtonWidth, this.ButtonHeight), parent: createParent),
                text,
                Color.Yellow,
                positionRelationX: this.PositionRelationX,
                positionRelationY: this.PositionRelationY,
                parent: this.parent);

            this.AddObject(button);

            button.LeftMouseButtonClicked += (sender, args) =>
            {
                leftMouseClick();
            };
        }

        /// <summary>
        /// Adds a text input
        /// </summary>
        /// <param name="name">The name of the text input</param>
        /// <param name="defaultText">The default text</param>
        public TextInputUIObject AddTextInput(string name, string defaultText)
        {
            var textInput = new TextInputUIObject(
                this.renderingManager2D,
                this.keyboardManager,
                name,
                this.NextPosition(false),
                new Size2(this.TextInputWidth, this.ButtonHeight),
                positionRelationX: this.PositionRelationX,
                positionRelationY: this.PositionRelationY,
                parent: this.parent)
            {
                Text = defaultText
            };

            this.AddObject(textInput);
            return textInput;
        }

        /// <summary>
        /// Adds a button and text input
        /// </summary>
        /// <param name="buttonName">The name of the button</param>
        /// <param name="buttonText">The text of the button</param>
        /// <param name="textInputName">The name of the text input</param>
        /// <param name="textInputDefaultText">The default text of the text input</param>
        /// <param name="buttonLeftMouseClick">The left mouse click for the button</param>
        public TextInputUIObject AddButtonAndTextInput(string buttonName, string buttonText, string textInputName, string textInputDefaultText, Action buttonLeftMouseClick)
        {
            var textInput = this.AddTextInput(textInputName, textInputDefaultText);
            this.AddButton(buttonName, buttonText, buttonLeftMouseClick);
            return textInput;
        }

        /// <summary>
        /// Adds a button and a list box
        /// </summary>
        /// <param name="buttonName">The name of the button</param>
        /// <param name="buttonText">The text of the button</param>
        /// <param name="listName">The name of the list box</param>
        /// <param name="buttonLeftMouseClick">The left mouse click for the button</param>
        public ListBoxUIObject AddButtonAndListBox(string buttonName, string buttonText, string listName, Action buttonLeftMouseClick)
        {
            var listBox = new ListBoxUIObject(
                this.renderingManager2D,
                listName,
                this.NextPosition(false),
                this.TextInputWidth,
                new List<ListBoxUIObject.Item>(),
                positionRelationX: this.PositionRelationX,
                positionRelationY: this.PositionRelationY,
                parent: parent);
            this.AddObject(listBox);

            this.AddButton(buttonName, buttonText, buttonLeftMouseClick);
            return listBox;
        }

        /// <summary>
        /// Adds a text object
        /// </summary>
        /// <param name="textName">The name of the object</param>
        /// <param name="text">The text</param>
        public void AddText(string textName, string text)
        {
            var textObject = new TextUIObject(
                this.renderingManager2D,
                textName,
                this.NextPosition(false),
                text,
                Color.Yellow,
                parent: parent);
            this.AddObject(textObject);
        }
    }
}
