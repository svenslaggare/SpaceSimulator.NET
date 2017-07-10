using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Common.UI;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for the main UI
    /// </summary>
    public class MainUI : UIComponent
    {
        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;

        private UIGroup maneuverGroup;
        private UIGroup ascentGroup;
        private UIGroup createObjectGroup;

        /// <summary>
        /// Creates a new main UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulation container</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="uiStyle">The UI style</param>
        public MainUI(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            SimulatorContainer simulatorContainer,
            UIManager uiManager,
            UIStyle uiStyle)
            : base(renderingManager2D, keyboardManager, mouseManager, simulatorContainer)
        {
            this.uiManager = uiManager;
            this.uiStyle = uiStyle;

            var numMainMenuElements = 4;
            var mainMenuSize = new Size2(
                UIBuilder.DefaultButtonWidth + 30,
                (int)(UIBuilder.DefaultButtonHeight * numMainMenuElements + 50));

            var mainMenuUIGroup = new UIGroup(
                this.RenderingManager2D,
                "MainGroup",
                new Vector2(0, 0),
                mainMenuSize,
                PositionRelationX.Right,
                PositionRelationY.Center);
            this.uiManager.AddElement(mainMenuUIGroup);

            var mainMenuBackground = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                new Vector2(0, 0),
                new Size2(mainMenuSize.Width + 5, mainMenuSize.Height),
                this.uiStyle.UIGroupBackgroundBrush,
                this.uiStyle.ButtonBorderBrush,
                parent: mainMenuUIGroup);
            mainMenuUIGroup.AddObject(mainMenuBackground);

            var mainMenuBuilder = new UIBuilder(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.uiManager,
                this.uiStyle,
                mainMenuUIGroup);

            mainMenuBuilder.ResetPosition(5, 10);
            mainMenuBuilder.PositionRelationX = PositionRelationX.Center;

            mainMenuBuilder.AddButton(
                "AbortManeuverButton",
                "Abort maneuver",
                this.AbortManeuver);

            mainMenuBuilder.AddButton(
               "ShowManeuversButton",
               "Show maneuvers",
               this.ShowManeuvers);

            mainMenuBuilder.AddButton(
                "ShowAscentButton",
                "Show ascent",
                this.ShowAscent);

            mainMenuBuilder.AddButton(
                "ShowCreateObjectButton",
                "Show create object",
                this.ShowCreateObject);
        }

        /// <summary>
        /// Aborts the most recent maneuver
        /// </summary>
        private void AbortManeuver()
        {
            if (this.SimulatorEngine.Maneuvers.Count > 0)
            {
                this.SimulatorEngine.AbortManeuver(this.SimulatorEngine.Maneuvers.First());
            }
        }

        /// <summary>
        /// Returns the maneuver group
        /// </summary>
        private UIGroup ManeuverGroup
        {
            get
            {
                if (this.maneuverGroup == null)
                {
                    this.maneuverGroup = this.uiManager.FindElement("ManeuverGroup") as UIGroup;
                }

                return this.maneuverGroup;
            }
        }

        /// <summary>
        /// Returns the ascent group
        /// </summary>
        private UIGroup AscentGroup
        {
            get
            {
                if (this.ascentGroup == null)
                {
                    this.ascentGroup = this.uiManager.FindElement("AscentGroup") as UIGroup;
                }

                return this.ascentGroup;
            }
        }

        /// <summary>
        /// Returns the create object group
        /// </summary>
        private UIGroup CreateObjectGroup
        {
            get
            {
                if (this.createObjectGroup == null)
                {
                    this.createObjectGroup = this.uiManager.FindElement("CreateObjectGroup") as UIGroup;
                }

                return this.createObjectGroup;
            }
        }

        /// <summary>
        /// Toggles visibility for the given group
        /// </summary>
        /// <param name="group">The group</param>
        private void ToggleVisibility(UIGroup group)
        {
            group.IsVisible = !group.IsVisible;

            if (this.ManeuverGroup != group)
            {
                this.ManeuverGroup.IsVisible = false;
            }

            if (this.AscentGroup != group)
            {
                this.AscentGroup.IsVisible = false;
            }

            if (this.CreateObjectGroup != group)
            {
                this.CreateObjectGroup.IsVisible = false;
            }
        }

        /// <summary>
        /// Shows the maneuvers
        /// </summary>
        private void ShowManeuvers()
        {
            this.ToggleVisibility(this.ManeuverGroup);
        }

        /// <summary>
        /// Shows the ascent menu
        /// </summary>
        private void ShowAscent()
        {
            this.ToggleVisibility(this.AscentGroup);
        }

        /// <summary>
        /// Shows the create object menu
        /// </summary>
        private void ShowCreateObject()
        {
            this.ToggleVisibility(this.CreateObjectGroup);
        }

        public override void Update(TimeSpan elapsed)
        {

        }

        public override void Draw(DeviceContext deviceContext)
        {
            
        }
    }
}
