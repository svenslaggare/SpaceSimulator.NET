using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing maneuvers
    /// </summary>
    public class ManeuverUI : UIComponent
    {
        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;

        private readonly TextInputUIObject changePeriapsisTextInput;
        private readonly TextInputUIObject changeApoapsisTextInput;
        private readonly TextInputUIObject changeInclinationTextInput;

        private readonly TextInputUIObject thurstAmountTextInput;
        private readonly TextInputUIObject hohmannTransferRadiusTextInput;

        private readonly ListBoxUIObject rendevouzTargetList;
        private readonly ListBoxUIObject planetaryRendevouzTargetList;

        /// <summary>
        /// Creates a new maneuver UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="simulatorContainer">The simulation container</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="uiStyle">The UI style</param>
        public ManeuverUI(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, SimulatorContainer simulatorContainer, UIManager uiManager, UIStyle uiStyle)
            : base(renderingManager2D, keyboardManager, simulatorContainer)
        {
            this.uiManager = uiManager;
            this.uiStyle = uiStyle;

            var startPosY = 400.0f;
            var posY = startPosY;
            var deltaY = -40.0f;
            var offsetRight = 0;
            var inputWidth = 150;
            var buttonWidth = 155;
            var buttonHeight = 30;
            var positionRelationX = PositionRelationX.Right;
            var positionRelationY = PositionRelationY.Bottom;

            Vector2 NextPosition(bool button)
            {
                return new Vector2(offsetRight + 10.0f + (button ? - 2.5f : 0), posY += deltaY);
            }

            ButtonUIObject CreateButton(string name, string text)
            {
                return new ButtonUIObject(
                    this.RenderingManager2D,
                    name,
                    NextPosition(true),
                    parent => this.uiStyle.CreateButtonBackground(new Size2(buttonWidth, buttonHeight), parent: parent),
                    text,
                    Color.Yellow,
                    positionRelationX: positionRelationX,
                    positionRelationY: positionRelationY);
            }

            void AddButton(string name, string text, Action leftMouseClick)
            {
                var button = CreateButton(name, text);
                this.uiManager.AddElement(button);
                button.LeftMouseButtonClicked += (sender, e) =>
                {
                    try
                    {
                        leftMouseClick();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                };
            }

            TextInputUIObject AddButtonAndTextInput(string buttonName, string buttonText, string textInputName, string textInputDefaultText, Action leftMouseClick)
            {
                var textInput = new TextInputUIObject(
                    this.RenderingManager2D,
                    this.KeyboardManager,
                    textInputName,
                    NextPosition(false),
                    new Size2(inputWidth, buttonHeight),
                    positionRelationX: positionRelationX,
                    positionRelationY: positionRelationY)
                {
                    Text = textInputDefaultText
                };
                this.uiManager.AddElement(textInput);

                AddButton(buttonName, buttonText, leftMouseClick);

                return textInput;
            }

            ListBoxUIObject AddButtonAndListBox(string buttonName, string buttonText, string listName, Action leftMouseClick)
            {
                var listBox = new ListBoxUIObject(
                    this.RenderingManager2D,
                    listName,
                    NextPosition(false),
                    inputWidth,
                    new List<ListBoxUIObject.Item>(),
                    positionRelationX: positionRelationX,
                    positionRelationY: positionRelationY);
                this.uiManager.AddElement(listBox);

                AddButton(buttonName, buttonText, leftMouseClick);
                return listBox;
            }

            offsetRight = buttonWidth + 30;
            posY = startPosY;

            AddButton("AbortManeuverButton", "Abort maneuver", this.AbortManeuver);

            this.changePeriapsisTextInput = AddButtonAndTextInput(
                "ChangePeriapsisButton",
                "Change periapsis",
                "ChangePeriapsisTextInput",
                "0",
                this.ChangePeriapsis);

            this.changeApoapsisTextInput = AddButtonAndTextInput(
                "ChangeApoapsisButton",
                "Change apoapsis",
                "ChangeApoapsisTextInput",
                "0",
                this.ChangeApoapsis);

            this.changeInclinationTextInput = AddButtonAndTextInput(
                "ChangeInclinationButton",
                "Change inclination",
                "ChangeInclinationTextInput",
                "0",
                this.ChangeInclination);

            offsetRight = 20;
            posY = startPosY;

            this.thurstAmountTextInput = AddButtonAndTextInput(
                "ApplyThrustButton",
                "Apply Thrust",
                "ThrustAmountTextInput",
                "100P",
                this.ApplyThrust);

            this.hohmannTransferRadiusTextInput = AddButtonAndTextInput(
                "HohmannTransferButton",
                "Hohmann transfer",
                "HohmannTransferRadiusTextInput",
                "3",
                this.HohmannTransfer);

            this.rendevouzTargetList = AddButtonAndListBox("RendevouzButton", "Rendevouz", "RendevouzTargetList", this.Rendevouz);
            this.UpdateRendevouzTargetList(this.SelectedObject);

            this.planetaryRendevouzTargetList = AddButtonAndListBox("PlanetaryRendevouzButton", "Planetary rendevouz", "RendevouzTargetList", this.PlanetaryRendevouz);
            this.UpdatePlanetaryRendevouzTargetList(this.SelectedObject);

            this.SimulatorContainer.SelectedObjectChanged += (sender, e) =>
            {
                this.UpdateRendevouzTargetList(e);
                this.UpdatePlanetaryRendevouzTargetList(e);
            };
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
        /// Changes the periapsis
        /// </summary>
        private void ChangePeriapsis()
        {
            var newPeriapsis = double.Parse(this.changePeriapsisTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                BasicManeuver.ChangePeriapsis(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    newPeriapsis * SolarSystem.Earth.Radius));
        }

        /// <summary>
        /// Changes the apoapsis
        /// </summary>
        private void ChangeApoapsis()
        {
            var newApoapsis = double.Parse(this.changeApoapsisTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                BasicManeuver.ChangeApoapsis(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    newApoapsis * SolarSystem.Earth.Radius));
        }

        /// <summary>
        /// Changes the inclination
        /// </summary>
        private void ChangeInclination()
        {
            var newInclination = double.Parse(this.changeInclinationTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                BasicManeuver.ChangeInclination(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    newInclination * MathUtild.Deg2Rad));
        }

        /// <summary>
        /// Applies thrust
        /// </summary>
        private void ApplyThrust()
        {
            var deltaV = DataFormatter.ParseDeltaVelocity(
                this.SelectedObject,
                this.thurstAmountTextInput.Text);

            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                OrbitalManeuver.Burn(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    deltaV,
                    OrbitalManeuverTime.Now()));
        }

        /// <summary>
        /// Applies a hohmann transfer
        /// </summary>
        private void HohmannTransfer()
        {
            var newRadius = double.Parse(this.hohmannTransferRadiusTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            var state = this.SelectedObject.State;
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.SelectedObject);

            this.SimulatorEngine.ScheduleManeuver( this.SelectedObject,
                HohmannTransferOrbit.Create(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    ref state,
                    ref orbitPosition,
                    newRadius * SolarSystem.Earth.Radius,
                    OrbitalManeuverTime.Periapsis()));
        }

        /// <summary>
        /// The rendevouz maneuver
        /// </summary>
        private void Rendevouz()
        {
            var targetObject = (PhysicsObject)this.rendevouzTargetList.SelectedItem?.Tag;

            if (targetObject != null)
            {
                var targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(targetObject);
                var manevuer = RendevouzManeuver.Rendevouz(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    ref targetOrbitPosition);
                this.SimulatorEngine.ScheduleManeuver(this.SelectedObject, manevuer);
            }
        }

        /// <summary>
        /// The planetary rendevouz maneuver
        /// </summary>
        private void PlanetaryRendevouz()
        {
            var targetObject = (PhysicsObject)planetaryRendevouzTargetList.SelectedItem?.Tag;

            if (targetObject != null)
            {
                var maneuver = InterplanetaryManeuver.PlanetaryTransfer(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    targetObject);
                this.SimulatorEngine.ScheduleManeuver(this.SelectedObject, maneuver);
            }
        }

        /// <summary>
        /// Updates the rendevouz target list
        /// </summary>
        /// <param name="selectedObject">The current selected object</param>
        private void UpdateRendevouzTargetList(PhysicsObject selectedObject)
        {
            var validTargets = this.SimulatorEngine.Objects
                   .Where(x => x.Type == Simulator.PhysicsObjectType.ArtificialSatellite && x != selectedObject)
                   .ToList();

            this.rendevouzTargetList.SetItems(validTargets.Select(x => new ListBoxUIObject.Item(x.Name, x)).ToList());
        }

        /// <summary>
        /// Updates the planetary rendevouz target list
        /// </summary>
        /// <param name="selectedObject">The current selected object</param>
        private void UpdatePlanetaryRendevouzTargetList(PhysicsObject selectedObject)
        {
            if (selectedObject.Type != PhysicsObjectType.ObjectOfReference)
            {
                var validTargets = this.SimulatorEngine.Objects
                       .Where(x => x.Type == Simulator.PhysicsObjectType.NaturalSatellite
                                   && x != selectedObject
                                   && x != selectedObject.PrimaryBody
                                   && x.PrimaryBody == selectedObject.PrimaryBody.PrimaryBody)
                       .ToList();

                this.planetaryRendevouzTargetList.SetItems(validTargets.Select(x => new ListBoxUIObject.Item(x.Name, x)).ToList());
            }
            else
            {
                this.planetaryRendevouzTargetList.SetItems(new List<ListBoxUIObject.Item>());
            }
        }

        public override void Update(TimeSpan elapsed)
        {

        }

        /// <summary>
        /// Draws the info text for the maneuvers
        /// </summary>
        /// <param name="deviceContex">The device context</param>
        private void DrawManeuverTexts(DeviceContext deviceContext)
        {
            var posY = 10;
            var posX = this.RenderingManager2D.ScreenRectangle.Width - 500;
            
            var outputBuilder = new StringBuilder();
            foreach (var maneuver in this.SimulatorEngine.Maneuvers)
            {
                var timeLeft = maneuver.Maneuver.ManeuverTime - this.SimulatorEngine.TotalTime;
                var dv = maneuver.Maneuver.DeltaVelocity.Length();

                var decimals = 2;
                var P = DataFormatter.Format(maneuver.ProgradeComponent, DataUnit.Velocity, decimals);
                var N = DataFormatter.Format(maneuver.NormalComponent, DataUnit.Velocity, decimals);
                var R = DataFormatter.Format(maneuver.RadialComponent, DataUnit.Velocity, decimals);

                var mainString = maneuver.Object.Name + " - time left: " + DataFormatter.Format(timeLeft, DataUnit.Time, 0);

                if (maneuver.Maneuver.DeltaVelocity != Vector3d.Zero)
                {
                    outputBuilder.AppendLine(mainString + ", Δv: " + DataFormatter.Format(dv, DataUnit.Velocity, decimals));
                    outputBuilder.AppendLine("(P: " + P + ", N: " + N + ", R: " + R + ")");
                }
                else
                {
                    outputBuilder.AppendLine(mainString);
                }
            }

            foreach (var currentEvent in this.SimulatorEngine.Events)
            {
                var timeLeft = currentEvent.Time - this.SimulatorEngine.TotalTime;
                var mainString = currentEvent.Object.Name + ": " + currentEvent.Type + " - time left: " + DataFormatter.Format(timeLeft, DataUnit.Time, 0);
                outputBuilder.AppendLine(mainString);
            }

            this.TextColorBrush.DrawText(
                deviceContext,
                outputBuilder.ToString(),
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(posX, posY)));
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.DrawManeuverTexts(deviceContext);
        }
    }
}
