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
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for creating objects
    /// </summary>
    public class CreateObjectUI : UIComponent
    {
        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;

        private readonly UIGroup createObjectGroup;
        private readonly TextInputUIObject parameterTextInput;
        private readonly TextInputUIObject eccentricityTextInput;
        private readonly TextInputUIObject inclinationTextInput;
        private readonly TextInputUIObject longitudeOfAscendingNodeTextInput;
        private readonly TextInputUIObject argumentOfPeriapsisTextInput;

        /// <summary>
        /// Creates a new create object UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulation container</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="uiStyle">The UI style</param>
        public CreateObjectUI(
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

            var createObjectGroupSize = new Size2(210, 260);

            this.createObjectGroup = new UIGroup(
                this.RenderingManager2D,
                "CreateObjectGroup",
                new Vector2(0, 0),
                createObjectGroupSize,
                PositionRelationX.Center,
                PositionRelationY.Center)
            {
                IsVisible = false
            };
            this.uiManager.AddElement(this.createObjectGroup);

            var createObjectGroupBackground = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                Vector2.Zero,
                createObjectGroupSize,
                this.uiStyle.UIGroupBackgroundBrush,
                this.uiStyle.ButtonBorderBrush,
                parent: this.createObjectGroup);
            this.createObjectGroup.AddObject(createObjectGroupBackground);

            var createObjectBuilder = new UIBuilder(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.uiManager,
                this.uiStyle,
                this.createObjectGroup)
            {
                PositionRelationX = PositionRelationX.Right
            };

            createObjectBuilder.ResetPosition(10, 15);
            createObjectBuilder.TextInputWidth = 80;

            this.parameterTextInput = createObjectBuilder.AddTextInput("ParameterTextInput", "0");
            this.eccentricityTextInput = createObjectBuilder.AddTextInput("EccentricityTextInput", "0");
            this.inclinationTextInput = createObjectBuilder.AddTextInput("InclinationTextInput", "0");
            this.longitudeOfAscendingNodeTextInput = createObjectBuilder.AddTextInput("LongitudeOfAscendingNodeTextInput", "0");
            this.argumentOfPeriapsisTextInput = createObjectBuilder.AddTextInput("ArgumentOfPeriapsisTextInput", "0");

            createObjectBuilder.PositionRelationX = PositionRelationX.Left;
            createObjectBuilder.ResetPosition(10, 15);
            createObjectBuilder.AddText("ParameterText", "Parameter:");
            createObjectBuilder.AddText("EccentricityText", "Eccentricity:");
            createObjectBuilder.AddText("InclinationText", "Inclination:");
            createObjectBuilder.AddText("LongitudeOfAscendingNodeText", "Ω:");
            createObjectBuilder.AddText("ArgumentOfPeriapsisText", "ω:");

            createObjectBuilder.PositionRelationX = PositionRelationX.Center;
            createObjectBuilder.PositionRelationY = PositionRelationY.Bottom;
            createObjectBuilder.ResetPosition(0, 10);
            createObjectBuilder.AddButton("CreateObject", "Create object", this.CreateObject);
        }

        /// <summary>
        /// Creates a new object
        /// </summary>
        private void CreateObject()
        {
            NaturalSatelliteObject primaryBody = null;
            if (this.SelectedObject is NaturalSatelliteObject)
            {
                primaryBody = (NaturalSatelliteObject)this.SelectedObject;
            }
            else
            {
                primaryBody = this.SelectedObject.PrimaryBody;
            }

            var parameter = UIComponentHelpers.ParseDistance(this.parameterTextInput.Text, this.SelectedObject.PrimaryBody);
            var eccentricity = UIComponentHelpers.ParseDouble(this.eccentricityTextInput.Text);
            var inclination = UIComponentHelpers.ParseDouble(this.inclinationTextInput.Text) * MathUtild.Deg2Rad;
            var longitudeOfAscendingNode = UIComponentHelpers.ParseDouble(this.longitudeOfAscendingNodeTextInput.Text) * MathUtild.Deg2Rad;
            var argumentOfPeriapsis = UIComponentHelpers.ParseDouble(this.argumentOfPeriapsisTextInput.Text) * MathUtild.Deg2Rad;

            var orbit = Physics.Orbit.New(
                primaryBody,
                parameter: parameter,
                eccentricity: eccentricity,
                inclination: inclination,
                longitudeOfAscendingNode: longitudeOfAscendingNode,
                argumentOfPeriapsis: argumentOfPeriapsis);

            var satellite = this.SimulatorEngine.AddSatelliteInOrbit(
                "Satellite",
                1000,
                new AtmosphericProperties(AtmosphericFormulas.CircleArea(10), 0.05),
                new OrbitPosition(orbit, 0.0));
            this.SimulatorContainer.CreateRenderingObject(satellite);
        }

        public override void Update(TimeSpan elapsed)
        {

        }

        public override void Draw(DeviceContext deviceContext)
        {

        }
    }
}
