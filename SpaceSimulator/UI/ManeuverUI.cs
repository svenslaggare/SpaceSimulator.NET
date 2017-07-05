using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Physics.Solvers;
using SpaceSimulator.Rendering;
using SpaceSimulator.Rendering.Plot;
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

        private readonly ListBoxUIObject interceptTargetList;
        private readonly ListBoxUIObject rendevouzTargetList;
        private readonly ListBoxUIObject planetaryRendevouzTargetList;

        private Heatmap deltaVChart;
        private bool showDeltaVChart = false;

        /// <summary>
        /// Creates a new maneuver UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulation container</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="uiStyle">The UI style</param>
        public ManeuverUI(
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

            var startPosY = 500.0f;
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
                        if (!this.SimulatorContainer.IsFrozen)
                        {
                            leftMouseClick();
                        }
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

            this.interceptTargetList = AddButtonAndListBox("InterceptButton", "Intercept", "InterceptTargetList", this.Intercept);
            this.UpdateInterceptTargetList(this.SelectedObject);

            this.rendevouzTargetList = AddButtonAndListBox("RendevouzButton", "Rendevouz", "RendevouzTargetList", this.Rendevouz);
            this.UpdateRendevouzTargetList(this.SelectedObject);

            this.planetaryRendevouzTargetList = AddButtonAndListBox("PlanetaryRendevouzButton", "Planetary rendevouz", "RendevouzTargetList", this.PlanetaryRendevouz);
            this.UpdatePlanetaryRendevouzTargetList(this.SelectedObject);

            this.SimulatorContainer.SelectedObjectChanged += (sender, e) =>
            {
                this.UpdateRendevouzTargetList(e);
                this.UpdatePlanetaryRendevouzTargetList(e);
                this.UpdateInterceptTargetList(e);
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
                    newPeriapsis * SolarSystemBodies.Earth.Radius));
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
                    newApoapsis * SolarSystemBodies.Earth.Radius));
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
                    newRadius * SolarSystemBodies.Earth.Radius,
                    OrbitalManeuverTime.Periapsis()));
        }

        /// <summary>
        /// Calculates an orbital intercept
        /// </summary>
        /// <param name="selectedObject">The selected object</param>
        /// <param name="targetOrbitPosition">The target orbit position</param>
        private void OrbitalIntercept(PhysicsObject selectedObject, ref OrbitPosition targetOrbitPosition)
        {
            var hohmannCoastTime = MiscHelpers.RoundToDays(HohmannTransferOrbit.CalculateBurn(
                selectedObject.PrimaryBody.StandardGravitationalParameter,
                selectedObject.ReferenceOrbit.SemiMajorAxis,
                targetOrbitPosition.Orbit.SemiMajorAxis).CoastTime);

            var synodicPeriod = OrbitFormulas.SynodicPeriod(
                selectedObject.ReferenceOrbit.Period,
                targetOrbitPosition.Orbit.Period);

            var deltaTime = MathHelpers.Clamp(1000, 2.0 * 24 * 60 * 60, synodicPeriod / 1000.0);

            this.SimulatorEngine.ScheduleManeuver(
                selectedObject,
                InterceptManeuver.Intercept(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    selectedObject.Target,
                    targetOrbitPosition,
                    hohmannCoastTime * 0.5,
                    hohmannCoastTime * 2.0,
                    0,
                    synodicPeriod * 1.25,
                    deltaTime));
        }

        /// <summary>
        /// Calculates a ground intercept
        /// </summary>
        /// <param name="selectedObject">The selected object</param>
        /// <param name="targetOrbitPosition">The target orbit position</param>
        private void GroundIntercept(PhysicsObject selectedObject, ref OrbitPosition targetOrbitPosition)
        {
            var deltaTime = 600.0;
            if (targetOrbitPosition.Orbit.Period > 24.0 * 60 * 60.0)
            {
                deltaTime = MathHelpers.Clamp(600.0, 24.0 * 60.0 * 60.0, targetOrbitPosition.Orbit.Period / (24.0 * 6.0));
            }

            this.SimulatorEngine.ScheduleManeuver(
                this.SelectedObject,
                InterceptManeuver.Intercept(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    selectedObject.Target,
                    targetOrbitPosition,
                    targetOrbitPosition.Orbit.Period * 0.1,
                    targetOrbitPosition.Orbit.Period,
                    0,
                    targetOrbitPosition.Orbit.Period * 3,
                    deltaTime));
        }

        /// <summary>
        /// Intercepts with an object
        /// </summary>
        private void Intercept()
        {
            this.SelectedObject.Target = (PhysicsObject)this.interceptTargetList.SelectedItem?.Tag;

            var startTime = DateTime.UtcNow;
            if (this.SelectedObject.Target != null)
            {
                var targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.SelectedObject.Target);

                if (this.SelectedObject.HasImpacted)
                {
                    this.GroundIntercept(this.SelectedObject, ref targetOrbitPosition);
                }
                else
                {
                    this.OrbitalIntercept(this.SelectedObject, ref targetOrbitPosition);
                }
            }
            Console.WriteLine("Computed intercept in: " + (DateTime.UtcNow - startTime));
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
                this.SelectedObject.Target = targetObject;
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
                //var maneuver = InterplanetaryManeuver.PlanetaryTransfer(
                //    this.SimulatorEngine,
                //    this.SelectedObject,
                //    targetObject,
                //    out var possibleDepartureBurns);

                //this.SimulatorEngine.ScheduleManeuver(this.SelectedObject, maneuver);
                //this.SelectedObject.Target = targetObject;

                //this.showDeltaVChart = true;
                //this.deltaVChart = PlotHeatmap.CreateDeltaVChart(this.RenderingManager2D, possibleDepartureBurns);

                this.SimulatorContainer.Freeze();
                var task = new Task(state =>
                {
                    var stateTuple = ((PhysicsObject, PhysicsObject))state;
                    var maneuver = InterplanetaryManeuver.PlanetaryTransfer(
                        this.SimulatorEngine,
                        stateTuple.Item1,
                        stateTuple.Item2,
                        out var possibleDepartureBurns);

                    this.ScheduleMain(() =>
                    {
                        this.SimulatorEngine.ScheduleManeuver(stateTuple.Item1, maneuver);
                        this.SelectedObject.Target = stateTuple.Item2;

                        this.showDeltaVChart = true;
                        this.deltaVChart = Heatmap.CreateDeltaVChart(this.RenderingManager2D, possibleDepartureBurns);

                        this.SimulatorContainer.Unfreeze();
                    });
                }, (this.SelectedObject, targetObject));
                task.Start();
            }
        }

        /// <summary>
        /// Updates the intercept target list
        /// </summary>
        /// <param name="selectedObject">The current selected object</param>
        private void UpdateInterceptTargetList(PhysicsObject selectedObject)
        {
            var validTargets = this.SimulatorEngine.Objects
                   .Where(x =>
                            !x.IsObjectOfReference
                            && x != selectedObject
                            && x.PrimaryBody == selectedObject.PrimaryBody)
                   .ToList();

            this.interceptTargetList.SetItems(validTargets.Select(x => new ListBoxUIObject.Item(x.Name, x)).ToList());
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
            this.RunScheduledTasks();
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
                var P = DataFormatter.Format(maneuver.Prograde, DataUnit.Velocity, decimals);
                var N = DataFormatter.Format(maneuver.Normal, DataUnit.Velocity, decimals);
                var R = DataFormatter.Format(maneuver.Radial, DataUnit.Velocity, decimals);

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

            if (this.showDeltaVChart)
            {
                this.deltaVChart.Draw(deviceContext, new Vector2(400, 50));
            }
        }
    }
}
