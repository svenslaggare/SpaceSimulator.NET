using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Helpers;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represent an UI component for the selected object
    /// </summary>
    public class SelectedObjectUI : UIComponent
    {
        private int selectedObjectIndex;

        /// <summary>
        /// Creates a new selected object UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        public SelectedObjectUI(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, SimulatorContainer simulatorContainer)
            : base(renderingManager2D, keyboardManager, simulatorContainer)
        {
            if (this.SelectedObject == null)
            {
                for (int i = 0; i < this.SimulatorEngine.Objects.Count; i++)
                {
                    var currentObject = this.SimulatorEngine.Objects[i];
                    if (currentObject.Type == PhysicsObjectType.ArtificialSatellite)
                    {
                        this.selectedObjectIndex = i;
                        break;
                    }
                }

                this.SimulatorContainer.SelectedObject = this.SimulatorEngine.Objects[this.selectedObjectIndex];
            }

            this.SimulatorContainer.SelectedObjectChanged += SelectedObjectChanged;
        }

        /// <summary>
        /// Handles when the selected object is changed
        /// </summary>
        /// <param name="newObject">The new selected object</param>
        private void SelectedObjectChanged(object sender, PhysicsObject newObject)
        {
            for (int i = 0; i < this.SimulatorEngine.Objects.Count; i++)
            {
                if (this.SimulatorEngine.Objects[i] == newObject)
                {
                    this.selectedObjectIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Draws the component
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public override void Update(TimeSpan elapsed)
        {
            var newSelectedObject = UIComponentHelpers.SelectObjectUpAndDown(
                this.KeyboardManager,
                this.SimulatorEngine.Objects,
                ref this.selectedObjectIndex,
                SharpDX.DirectInput.Key.PageUp,
                SharpDX.DirectInput.Key.PageDown,
                out var changed);

            if (changed)
            {
                this.SimulatorContainer.SelectedObject = newSelectedObject;
            }
        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Draw(DeviceContext deviceContext)
        {
            //var selectedObject = this.SimulatorEngine.Objects.FirstOrDefault(x => x.Type == PhysicsObjectType.ArtificialSatellite);
            var selectedObjectOrbitPosition = new OrbitPosition();

            if (this.SelectedObject.PrimaryBody != null)
            {
                selectedObjectOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.SelectedObject.PrimaryBody, this.SelectedObject.State);
            }
            else
            {
                selectedObjectOrbitPosition = new OrbitPosition(new Orbit(), 0.0);
            }

            var selectedObjectText = OrbitTextInformation.FullInformation(
                this.SelectedObject.PrimaryBody,
                this.SelectedObject.State,
                selectedObjectOrbitPosition,
                this.SelectedObject);

            var target = this.SelectedObject.Target;
            if (target != null)
            {
                var targetState = target.State;
                var targetOrbitPosition = OrbitPosition.CalculateOrbitPosition(target);

                selectedObjectText += Environment.NewLine + OrbitTextInformation.TargetInformation(
                    this.SimulatorEngine,
                    this.SelectedObject,
                    this.SelectedObject.State,
                    selectedObjectOrbitPosition,
                    target,
                    target.State,
                    targetOrbitPosition,
                    calculateClosestApproach: false);
            }

            this.TextColorBrush.DrawText(
                deviceContext,
                selectedObjectText,
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(UIConstants.OffsetLeft, 90)));
        }
    }
}
