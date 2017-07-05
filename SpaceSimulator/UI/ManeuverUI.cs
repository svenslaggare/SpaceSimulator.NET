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
using SpaceSimulator.Simulator.Rocket;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing maneuvers
    /// </summary>
    public class ManeuverUI : UIComponent
    {
        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;

        private readonly UIGroup maneuverUIGroup;

        private readonly TextInputUIObject thurstAmountTextInput;
        private readonly TextInputUIObject changePeriapsisTextInput;
        private readonly TextInputUIObject changeApoapsisTextInput;
        private readonly TextInputUIObject changeInclinationTextInput;

        private readonly TextInputUIObject hohmannTransferRadiusTextInput;
        private readonly ListBoxUIObject interceptTargetList;
        private readonly ListBoxUIObject rendevouzTargetList;
        private readonly ListBoxUIObject planetaryRendevouzTargetList;

        private readonly UIGroup ascentUIGroup;

        private readonly TextInputUIObject ascentTargetAltitudeTextInput;

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

            var buttonWidth = 155;
            var buttonHeight = 30;

            var startPosY = -20.0f;
            var posY = startPosY;
            var deltaY = 40.0f;
            var offsetRight = buttonWidth + 40;
            var inputWidth = 150;
            var maneuverObjectsPositionRelationX = PositionRelationX.Right;
            var maneuverObjectsPositionRelationY = PositionRelationY.Top;

            #region Helpers
            Vector2 NextPosition(bool button)
            {
                return new Vector2(offsetRight + 10.0f + (button ? - 2.5f : 0), posY += deltaY);
            }

            ButtonUIObject CreateButton(string name, Vector2 position, PositionRelationX positionRelationX, PositionRelationY positionRelationY, string text, UIElement parent = null)
            {
                return new ButtonUIObject(
                    this.RenderingManager2D,
                    name,
                    position,
                    createParent => this.uiStyle.CreateButtonBackground(new Size2(buttonWidth, buttonHeight), parent: createParent),
                    text,
                    Color.Yellow,
                    positionRelationX: positionRelationX,
                    positionRelationY: positionRelationY,
                    parent: parent);
            }

            void AddElement(UIElement parent, UIObject newObject)
            {
                if (parent is UIGroup uiGroup)
                {
                    uiGroup.AddObject(newObject);
                }
                else
                {
                    this.uiManager.AddElement(newObject);
                }
            }

            void AddButton(string name, Vector2 position, PositionRelationX positionRelationX, PositionRelationY positionRelationY, string text, Action leftMouseClick, UIElement parent)
            {
                var button = CreateButton(name, position, positionRelationX, positionRelationY, text, parent);
                AddElement(parent, button);

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

            TextInputUIObject AddButtonAndTextInput(string buttonName, string buttonText, string textInputName, string textInputDefaultText, Action leftMouseClick, UIElement parent)
            {
                var textInput = new TextInputUIObject(
                    this.RenderingManager2D,
                    this.KeyboardManager,
                    textInputName,
                    NextPosition(false),
                    new Size2(inputWidth, buttonHeight),
                    positionRelationX: maneuverObjectsPositionRelationX,
                    positionRelationY: maneuverObjectsPositionRelationY,
                    parent: parent)
                {
                    Text = textInputDefaultText
                };
                AddElement(parent, textInput);

                AddButton(buttonName, NextPosition(true), maneuverObjectsPositionRelationX, maneuverObjectsPositionRelationY, buttonText, leftMouseClick, parent);

                return textInput;
            }

            ListBoxUIObject AddButtonAndListBox(string buttonName, string buttonText, string listName, Action leftMouseClick, UIElement parent)
            {
                var listBox = new ListBoxUIObject(
                    this.RenderingManager2D,
                    listName,
                    NextPosition(false),
                    inputWidth,
                    new List<ListBoxUIObject.Item>(),
                    positionRelationX: maneuverObjectsPositionRelationX,
                    positionRelationY: maneuverObjectsPositionRelationY,
                    parent: parent);
                AddElement(parent, listBox);

                AddButton(buttonName, NextPosition(true), maneuverObjectsPositionRelationX, maneuverObjectsPositionRelationY, buttonText, leftMouseClick, parent);
                return listBox;
            }
            #endregion

            #region MainMenu
            var mainMenuPositionRelationX = PositionRelationX.Center;
            var mainMenuPositionRelationY = PositionRelationY.Top;
            var mainMenuSize = new Size2(buttonWidth + 30, (int)(buttonHeight + deltaY * 0.4) * 3 - 5);
            var mainMenuUIGroup = new UIGroup(
                this.RenderingManager2D,
                "ManeuverMainMenu",
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

            posY = 10.0f;
            AddButton(
                "AbortManeuverButton",
                new Vector2(0, posY),
                mainMenuPositionRelationX,
                mainMenuPositionRelationY,
                "Abort maneuver",
                this.AbortManeuver,
                mainMenuUIGroup);

            AddButton(
               "ShowManeuversButton",
               new Vector2(0, posY += deltaY),
               mainMenuPositionRelationX,
               mainMenuPositionRelationY,
               "Show maneuvers",
               this.ShowManeuvers,
               mainMenuUIGroup);

            AddButton(
                "ShowAscentButton",
                new Vector2(0, posY += deltaY),
                mainMenuPositionRelationX,
                mainMenuPositionRelationY,
                "Show ascent",
                this.ShowAscent,
                mainMenuUIGroup);
            #endregion

            #region Maneuvers
            var maneuverUIGroupSize = new Size2(380, 350);
            posY = startPosY;

            this.maneuverUIGroup = new UIGroup(
                this.RenderingManager2D,
                "ManeuverUIGroup",
                new Vector2(0, 0),
                maneuverUIGroupSize,
                PositionRelationX.Center,
                PositionRelationY.Center);
            this.uiManager.AddElement(this.maneuverUIGroup);
            this.maneuverUIGroup.IsVisible = false;

            var maneuverUIGroupBackground = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                Vector2.Zero,
                maneuverUIGroupSize,
                this.uiStyle.UIGroupBackgroundBrush,
                this.uiStyle.ButtonBorderBrush,
                parent: this.maneuverUIGroup);
            this.maneuverUIGroup.AddObject(maneuverUIGroupBackground);

            this.thurstAmountTextInput = AddButtonAndTextInput(
                "ApplyThrustButton",
                "Apply Thrust",
                "ThrustAmountTextInput",
                "100P",
                this.ApplyThrust,
                this.maneuverUIGroup);

            this.changePeriapsisTextInput = AddButtonAndTextInput(
                "ChangePeriapsisButton",
                "Change periapsis",
                "ChangePeriapsisTextInput",
                "0",
                this.ChangePeriapsis,
                this.maneuverUIGroup);

            this.changeApoapsisTextInput = AddButtonAndTextInput(
                "ChangeApoapsisButton",
                "Change apoapsis",
                "ChangeApoapsisTextInput",
                "0",
                this.ChangeApoapsis,
                this.maneuverUIGroup);

            this.changeInclinationTextInput = AddButtonAndTextInput(
                "ChangeInclinationButton",
                "Change inclination",
                "ChangeInclinationTextInput",
                "0",
                this.ChangeInclination,
                this.maneuverUIGroup);

            offsetRight = 20;
            posY = startPosY;

            this.hohmannTransferRadiusTextInput = AddButtonAndTextInput(
                "HohmannTransferButton",
                "Hohmann transfer",
                "HohmannTransferRadiusTextInput",
                "0",
                this.HohmannTransfer,
                this.maneuverUIGroup);

            this.interceptTargetList = AddButtonAndListBox(
                "InterceptButton",
                "Intercept",
                "InterceptTargetList",
                this.Intercept,
                this.maneuverUIGroup);
            this.UpdateInterceptTargetList(this.SelectedObject);

            this.rendevouzTargetList = AddButtonAndListBox(
                "RendevouzButton",
                "Rendevouz",
                "RendevouzTargetList",
                this.Rendevouz,
                this.maneuverUIGroup);
            this.UpdateRendevouzTargetList(this.SelectedObject);

            this.planetaryRendevouzTargetList = AddButtonAndListBox(
                "PlanetaryRendevouzButton",
                "Planetary rendevouz",
                "RendevouzTargetList",
                this.PlanetaryRendevouz,
                this.maneuverUIGroup);
            this.UpdatePlanetaryRendevouzTargetList(this.SelectedObject);

            this.SimulatorContainer.SelectedObjectChanged += (sender, args) =>
            {
                this.UpdateRendevouzTargetList(args);
                this.UpdatePlanetaryRendevouzTargetList(args);
                this.UpdateInterceptTargetList(args);
            };
            #endregion

            #region Ascent
            var ascentUIGroupSize = new Size2(200, 110);
            posY = startPosY;

            this.ascentUIGroup = new UIGroup(
                this.RenderingManager2D,
                "ManeuverUIGroup",
                new Vector2(0, 0),
                ascentUIGroupSize,
                PositionRelationX.Center,
                PositionRelationY.Center);
            this.uiManager.AddElement(this.ascentUIGroup);
            this.ascentUIGroup.IsVisible = false;

            var ascentUIGroupBackground = new RectangleUIObject(
                this.RenderingManager2D,
                "Background",
                Vector2.Zero,
                ascentUIGroupSize,
                this.uiStyle.UIGroupBackgroundBrush,
                this.uiStyle.ButtonBorderBrush,
                parent: this.ascentUIGroup);
            this.ascentUIGroup.AddObject(ascentUIGroupBackground);

            this.ascentTargetAltitudeTextInput = AddButtonAndTextInput(
                "AscendToOrbitButton",
                "Start ascent",
                "AscentTargetAltitudeTextInput",
                "300 km",
                this.AscendToOrbit,
                this.ascentUIGroup);
            #endregion
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

        enum ManeuverMenu
        {
            Manuevers,
            Ascent
        }

        /// <summary>
        /// Shows the given menu
        /// </summary>
        /// <param name="maneuverMenu">The menu</param>
        private void ShowMenu(ManeuverMenu maneuverMenu)
        {
            switch (maneuverMenu)
            {
                case ManeuverMenu.Manuevers:
                    this.maneuverUIGroup.IsVisible = !this.maneuverUIGroup.IsVisible;
                    this.ascentUIGroup.IsVisible = false;
                    break;
                case ManeuverMenu.Ascent:
                    this.ascentUIGroup.IsVisible = !this.ascentUIGroup.IsVisible;
                    this.maneuverUIGroup.IsVisible = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Shows the maneuvers
        /// </summary>
        private void ShowManeuvers()
        {
            this.ShowMenu(ManeuverMenu.Manuevers);
        }

        /// <summary>
        /// Shows the ascent menu
        /// </summary>
        private void ShowAscent()
        {
            this.ShowMenu(ManeuverMenu.Ascent);
        }

        /// <summary>
        /// Parses the given distance string
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="primaryBody">The primary body</param>
        /// <returns>The distance in meters</returns>
        private double ParseDistance(string text, NaturalSatelliteObject primaryBody = null)
        {
            var numericPart = "";
            var unitPart = "";
            foreach (var currentChar in text)
            {
                if (char.IsDigit(currentChar) && unitPart == "")
                {
                    numericPart += currentChar;
                }

                if (char.IsLetter(currentChar))
                {
                    unitPart += currentChar;
                }
            }

            var unitScaleFactor = 1.0;
            switch (unitPart)
            {
                case "er":
                case "ER":
                    unitScaleFactor = SolarSystemBodies.Earth.Radius;
                    break;
                case "sr":
                case "SR":
                    unitScaleFactor = SolarSystemBodies.Sun.Radius;
                    break;
                case "au":
                case "AU":
                    unitScaleFactor = Constants.AstronomicalUnit;
                    break;
                case "km":
                    unitScaleFactor = 1E3;
                    break;
                case "Mm":
                    unitScaleFactor = 1E6;
                    break;
                case "Gm":
                    unitScaleFactor = 1E9;
                    break;
            }

            var offset = 0.0;
            if (primaryBody != null && primaryBody.Name != "Sun")
            {
                offset = primaryBody.Radius;
            }

            return double.Parse(numericPart, System.Globalization.CultureInfo.InvariantCulture) * unitScaleFactor + offset;
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
        /// Changes the periapsis
        /// </summary>
        private void ChangePeriapsis()
        {
            //var newPeriapsis = double.Parse(this.changePeriapsisTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            var newPeriapsis = this.ParseDistance(this.changePeriapsisTextInput.Text, this.SelectedObject.PrimaryBody);
            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                BasicManeuver.ChangePeriapsis(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    newPeriapsis));
        }
 
        /// <summary>
        /// Changes the apoapsis
        /// </summary>
        private void ChangeApoapsis()
        {
            //var newApoapsis = double.Parse(this.changeApoapsisTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            var newApoapsis = this.ParseDistance(this.changeApoapsisTextInput.Text, this.SelectedObject.PrimaryBody);
            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                BasicManeuver.ChangeApoapsis(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    newApoapsis));
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
        /// Applies a hohmann transfer
        /// </summary>
        private void HohmannTransfer()
        {
            //var newRadius = double.Parse(this.hohmannTransferRadiusTextInput.Text, System.Globalization.CultureInfo.InvariantCulture);
            var newRadius = this.ParseDistance(this.hohmannTransferRadiusTextInput.Text, this.SelectedObject.PrimaryBody);
            var state = this.SelectedObject.State;
            var orbitPosition = OrbitPosition.CalculateOrbitPosition(this.SelectedObject);

            this.SimulatorEngine.ScheduleManeuver(this.SelectedObject,
                HohmannTransferOrbit.Create(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    ref state,
                    ref orbitPosition,
                    newRadius,
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

        /// <summary>
        /// Ascends to orbit
        /// </summary>
        private void AscendToOrbit()
        {
            if (this.SelectedObject.HasImpacted && this.SelectedObject is RocketObject rocketObject)
            {
                var targetAltitude = this.ParseDistance(this.ascentTargetAltitudeTextInput.Text, this.SelectedObject.PrimaryBody);
                var targetOrbit = Physics.Orbit.New(this.SelectedObject.PrimaryBody, semiMajorAxis: targetAltitude, eccentricity: 0.0);

                var bestPitchStart = 2E3;
                var bestPitchEnd = 12.8625E3;
                rocketObject.SetControlProgram(new AscentControlProgram(
                    rocketObject,
                    targetOrbit,
                    bestPitchStart,
                    bestPitchEnd,
                    this.SimulatorEngine.TextOutputWriter));

                rocketObject.StartEngine();
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
