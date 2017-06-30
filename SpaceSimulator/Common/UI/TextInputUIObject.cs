using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common.UI
{
    /// <summary>
    /// Manages input text input from the keyboard
    /// </summary>
    public class TextInput
    {
        private readonly KeyboardManager keyboardManager;

        private readonly StringBuilder textBuilder = new StringBuilder();
        private int? caretPosition = null;

        private readonly IDictionary<Key, double> pressedDownTimes = new Dictionary<Key, double>();
        private readonly double holdDownTime;
        private readonly double minTimeBetweenCommand;
        private readonly IDictionary<Key, double> timeSinceCommand = new Dictionary<Key, double>();

        private bool hasCapsLock = false;

        private enum KeyCommandType
        {
            Insert,
            Backspace,
            Delete,
            MoveCaretLeft,
            MoveCaretRight
        }

        /// <summary>
        /// Represents a key command
        /// </summary>
        private class KeyCommand
        {
            /// <summary>
            /// The type
            /// </summary>
            public KeyCommandType Type { get; }

            /// <summary>
            /// The char value
            /// </summary>
            public char Char { get; }

            /// <summary>
            /// Creates a new key command of the give type
            /// </summary>
            /// <param name="type">The type</param>
            public KeyCommand(KeyCommandType type)
            {
                this.Type = type;
            }

            /// <summary>
            /// Creates a new insert command
            /// </summary>
            /// <param name="currentChar">The char</param>
            public KeyCommand(char currentChar)
            {
                this.Type = KeyCommandType.Insert;
                this.Char = currentChar;
            }

            public override string ToString()
            {
                switch (this.Type)
                {
                    case KeyCommandType.Insert:
                        return $"Insert: {this.Char}";
                    default:
                        return this.Type.ToString();
                }
            }
        }

        private readonly IList<(Key, KeyCommand)> keyToCommand = new List<(Key, KeyCommand)>()
        {
            (Key.A, new KeyCommand('a')),
            (Key.B, new KeyCommand('b')),
            (Key.C, new KeyCommand('c')),
            (Key.D, new KeyCommand('d')),
            (Key.E, new KeyCommand('e')),
            (Key.F, new KeyCommand('f')),
            (Key.G, new KeyCommand('g')),
            (Key.H, new KeyCommand('h')),
            (Key.I, new KeyCommand('i')),
            (Key.J, new KeyCommand('j')),
            (Key.K, new KeyCommand('k')),
            (Key.L, new KeyCommand('l')),
            (Key.M, new KeyCommand('m')),
            (Key.N, new KeyCommand('n')),
            (Key.O, new KeyCommand('o')),
            (Key.P, new KeyCommand('p')),
            (Key.R, new KeyCommand('r')),
            (Key.Q, new KeyCommand('q')),
            (Key.S, new KeyCommand('s')),
            (Key.T, new KeyCommand('t')),
            (Key.U, new KeyCommand('u')),
            (Key.V, new KeyCommand('v')),
            (Key.W, new KeyCommand('w')),
            (Key.X, new KeyCommand('x')),
            (Key.Y, new KeyCommand('y')),
            (Key.Z, new KeyCommand('Z')),
            (Key.D0, new KeyCommand('0')),
            (Key.D1, new KeyCommand('1')),
            (Key.D2, new KeyCommand('2')),
            (Key.D3, new KeyCommand('3')),
            (Key.D4, new KeyCommand('4')),
            (Key.D5, new KeyCommand('5')),
            (Key.D6, new KeyCommand('6')),
            (Key.D7, new KeyCommand('7')),
            (Key.D8, new KeyCommand('8')),
            (Key.D9, new KeyCommand('9')),
            (Key.Space, new KeyCommand(' ')),
            (Key.Slash, new KeyCommand('-')),
            (Key.Period, new KeyCommand('.')),
            (Key.Back, new KeyCommand(KeyCommandType.Backspace)),
            (Key.Delete, new KeyCommand(KeyCommandType.Delete)),
            (Key.Left, new KeyCommand(KeyCommandType.MoveCaretLeft)),
            (Key.Right, new KeyCommand(KeyCommandType.MoveCaretRight)),
        };

        /// <summary>
        /// Creates a new text input
        /// </summary>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="holdDownTime">The minimum time a key must be hold down</param>
        /// <param name="minTimeBetweenCommand">The minimum allowed time between commands</param>
        public TextInput(KeyboardManager keyboardManager, double holdDownTime = 0.2, double minTimeBetweenCommand = 30.0 / 1000.0)
        {
            this.keyboardManager = keyboardManager;
            this.holdDownTime = holdDownTime;
            this.minTimeBetweenCommand = minTimeBetweenCommand;
        }

        /// <summary>
        /// The text
        /// </summary>
        public string Text
        {
            get { return this.textBuilder.ToString(); }
            set
            {
                this.textBuilder.Clear();
                this.textBuilder.Append(value);
            }
        }

        /// <summary>
        /// Returns the position of the caret
        /// </summary>
        public int CaretPosition
        {
            get { return this.caretPosition ?? this.textBuilder.Length; }
        }

        /// <summary>
        /// Indicates if the command should produce upper case
        /// </summary>
        private bool IsUpperCase()
        {
            if (this.keyboardManager.IsKeyPressed(Key.Capital))
            {
                this.hasCapsLock = !this.hasCapsLock;
            }

            var isUpperCase = this.hasCapsLock;
            if (this.keyboardManager.IsKeyDown(Key.LeftShift) || this.keyboardManager.IsKeyDown(Key.RightShift))
            {
                isUpperCase = true;
            }

            return isUpperCase;
        }

        /// <summary>
        /// Indicates if the given command should be executed
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="command">The command</param>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        private bool CanExecuteCommand(Key key, KeyCommand command, TimeSpan elapsed)
        {
            var executeCommand = false;

            if (this.keyboardManager.IsKeyDown(key))
            {
                this.pressedDownTimes.TryGetValue(key, out var pressedDownTime);
                pressedDownTime += elapsed.TotalSeconds;
                this.pressedDownTimes[key] = pressedDownTime;

                if (!this.timeSinceCommand.ContainsKey(key))
                {
                    this.timeSinceCommand[key] = 0.0;
                }

                this.timeSinceCommand[key] += elapsed.TotalSeconds;

                if (pressedDownTime >= this.holdDownTime && this.timeSinceCommand[key] >= this.minTimeBetweenCommand)
                {
                    executeCommand = true;
                    this.timeSinceCommand[key] = 0.0;
                }
            }
            else
            {
                this.pressedDownTimes.Remove(key);
            }

            if (this.keyboardManager.IsKeyPressed(key))
            {
                executeCommand = true;
            }

            return executeCommand;
        }

        /// <summary>
        /// Updates the text input
        /// </summary>
        /// <param name="elapsed">The elapsed time</param>
        /// <returns>True if the text was changed</returns>
        public bool Update(TimeSpan elapsed)
        {
            var isUpperCase = this.IsUpperCase();
            var updated = false;

            foreach ((var key, var command) in this.keyToCommand)
            {
                if (this.CanExecuteCommand(key, command, elapsed))
                {
                    switch (command.Type)
                    {
                        case KeyCommandType.Insert:
                            {
                                var currentChar = command.Char;
                                if (isUpperCase)
                                {
                                    currentChar = Char.ToUpper(currentChar);
                                }

                                this.textBuilder.Insert(this.CaretPosition, currentChar);
                                if (this.caretPosition != null)
                                {
                                    this.caretPosition++;
                                }
                            }
                            break;
                        case KeyCommandType.Backspace when this.textBuilder.Length > 0:
                            {
                                var removeCaretPosition = this.CaretPosition - 1;
                                if (removeCaretPosition >= 0)
                                {
                                    this.textBuilder.Remove(removeCaretPosition, 1);
                                    if (this.caretPosition != null)
                                    {
                                        this.caretPosition--;
                                        if (this.caretPosition < 0)
                                        {
                                            this.caretPosition = null;
                                        }
                                    }
                                }
                            }
                            break;
                        case KeyCommandType.Delete when this.textBuilder.Length > 0:
                            {
                                var removeCaretPosition = this.CaretPosition;
                                if (removeCaretPosition >= 0 && removeCaretPosition < this.textBuilder.Length)
                                {
                                    this.textBuilder.Remove(removeCaretPosition, 1);
                                }
                            }
                            break;
                        case KeyCommandType.MoveCaretLeft:
                            {
                                this.caretPosition = this.CaretPosition - 1;
                                if (this.caretPosition < 0)
                                {
                                    this.caretPosition = 0;
                                }

                                updated = true;
                            }
                            break;
                        case KeyCommandType.MoveCaretRight:
                            {
                                this.caretPosition = this.CaretPosition + 1;
                                if (this.caretPosition > this.textBuilder.Length - 1)
                                {
                                    this.caretPosition = null;
                                }

                                updated = true;
                            }
                            break;
                    }

                    updated = true;
                }
            }

            return updated;
        }
    }

    /// <summary>
    /// Represents a text input UI object
    /// </summary>
    public class TextInputUIObject : UIObject
    {
        private readonly KeyboardManager keyboardManager;
        private readonly TextInput textInput;

        private readonly RenderingSolidColorBrush textColorBrush;

        private readonly UIObject backgroundObject;
        private readonly TextUIObject currentInputObject;

        private bool showCaret;
        private double timeSinceCaretAction;
        private readonly double caretHideTime = 0.5;
        private readonly double caretShowTime = 1.0;

        /// <summary>
        /// Creates a new text input UI object
        /// </summary>
        /// <param name="renderingManager2D">The 2D rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="name">The name of the element</param>
        /// <param name="position">The position</param>
        /// <param name="size">The size. If null, then the size of the background object is used.</param>
        /// <param name="positionRelationX">The x-axis position relation</param>
        /// <param name="positionRelationY">The y-axis position relation</param>
        /// <param name="parent">The parent object</param>
        public TextInputUIObject(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            string name,
            Vector2 position,
            Size2 size,
            PositionRelationX positionRelationX = PositionRelationX.Left,
            PositionRelationY positionRelationY = PositionRelationY.Top,
            UIElement parent = null)
            : base(renderingManager2D, name, position, size, positionRelationX, positionRelationY, parent)
        {
            this.keyboardManager = keyboardManager;
            this.textInput = new TextInput(this.keyboardManager);

            var textColor = Color.Black;
            this.textColorBrush = this.RenderingManager2D.CreateSolidColorBrush(textColor);

            this.backgroundObject = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                new Vector2(0, 0),
                size,
                this.RenderingManager2D.CreateSolidColorBrush(new Color(255, 248, 242)),
                this.RenderingManager2D.CreateSolidColorBrush(new Color(160, 160, 160)),
                cornerRadius: 0,
                parent: this);

            this.currentInputObject = new TextUIObject(
                this.RenderingManager2D,
                "CurrentInput",
                new Vector2(5, 5),
                "",
                textColor,
                parent: this);
        }

        /// <summary>
        /// The text
        /// </summary>
        public string Text
        {
            get { return this.textInput.Text; }
            set
            {
                this.textInput.Text = value;
                this.currentInputObject.Text = value;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            this.backgroundObject.Invalidate();
            this.currentInputObject.Invalidate();
        }

        public override void GotFocus()
        {
            base.GotFocus();
            this.showCaret = true;
            this.timeSinceCaretAction = 0;
        }

        public override void Update(TimeSpan elapsed)
        {
            base.Update(elapsed);

            if (this.HasFocus)
            {
                if (this.textInput.Update(elapsed))
                {
                    this.currentInputObject.Text = this.textInput.Text;
                    this.timeSinceCaretAction = 0.0;
                    this.showCaret = true;
                }

                this.timeSinceCaretAction += elapsed.TotalSeconds;
                var caretActionTime = this.showCaret ? this.caretShowTime : this.caretHideTime;
                if (this.timeSinceCaretAction >= caretActionTime)
                {
                    this.showCaret = !this.showCaret;
                    this.timeSinceCaretAction = 0.0;
                }
            }
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.backgroundObject.Draw(deviceContext);
            this.currentInputObject.Draw(deviceContext);

            if (this.HasFocus)
            {
                var textFormat = this.RenderingManager2D.DefaultTextFormat;
                Size2 textSize;
                var caretText = "";
                if (this.textInput.CaretPosition > 0)
                {
                    caretText = this.Text.Substring(0, this.textInput.CaretPosition);
                }

                using (var textLayout = new TextLayout(
                    this.RenderingManager2D.FontFactory,
                    caretText,
                    textFormat,
                    this.RenderingManager2D.ScreenRectangle.Width,
                    this.RenderingManager2D.ScreenRectangle.Height))
                {
                    textSize = new Size2(
                        (int)Math.Round(textLayout.Metrics.Width),
                        (int)Math.Round(textLayout.Metrics.Height));
                }

                if (this.showCaret)
                {
                    this.textColorBrush.DrawText(
                        deviceContext,
                        " |",
                        textFormat,
                        this.RenderingManager2D.TextPosition(this.currentInputObject.ScreenPosition + new Vector2(textSize.Width - 5, -1)));
                }
            }
        }
    }
}
