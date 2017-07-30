using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Camera;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Rendering;
using SpaceSimulator.Rendering.Plot;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represent an UI component for the selected object
    /// </summary>
    public class SelectedObjectUI : UIComponent
    {
        private int selectedObjectIndex;

        private readonly Arrow arrow;

        private readonly IDictionary<PhysicsObject, GroundTrack> groundTracks = new Dictionary<PhysicsObject, GroundTrack>();
        private readonly bool showGroundTracks = true;

        private OrbitCalculators.ApproachData closestApproachData;
        private DateTime lastClosestApproachUpdate;
        private readonly TimeSpan closestApproachUpdateTime = TimeSpan.FromSeconds(0.75);

        /// <summary>
        /// Creates a new selected object UI component
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        public SelectedObjectUI(
            SharpDX.Direct3D11.Device graphicsDevice,
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            SimulatorContainer simulatorContainer)
            : base(renderingManager2D, keyboardManager, mouseManager, simulatorContainer)
        {
            if (this.SelectedObject == null)
            {
                //for (int i = 0; i < this.SimulatorEngine.Objects.Count; i++)
                //{
                //    var currentObject = this.SimulatorEngine.Objects[i];
                //    if (currentObject.Type == PhysicsObjectType.ArtificialSatellite)
                //    {
                //        this.selectedObjectIndex = i;
                //        break;
                //    }
                //}
                this.selectedObjectIndex = this.SimulatorEngine.Objects.Count - 1;

                this.SimulatorContainer.SelectedObject = this.SimulatorEngine.Objects[this.selectedObjectIndex];
            }

            this.SimulatorContainer.SelectedObjectChanged += SelectedObjectChanged;

            this.arrow = new Arrow(graphicsDevice, 0.25f, 10.0f, 2.0f);
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

            this.lastClosestApproachUpdate = new DateTime();
        }

        /// <summary>
        /// Computes the closest approach
        /// </summary>
        private void ComputeClosestApproach()
        {
            if (this.SelectedObject.Type == PhysicsObjectType.ArtificialSatellite
                && this.SelectedObject.Target != null
                && this.SelectedObject.PrimaryBody == this.SelectedObject.Target.PrimaryBody)
            {
                if (DateTime.UtcNow - this.lastClosestApproachUpdate >= this.closestApproachUpdateTime)
                {
                    this.closestApproachData = OrbitCalculators.ClosestApproach(
                        this.SimulatorEngine.KeplerProblemSolver,
                        this.SelectedObject,
                        OrbitPosition.CalculateOrbitPosition(this.SelectedObject),
                        this.SelectedObject.Target,
                        OrbitPosition.CalculateOrbitPosition(this.SelectedObject.Target));
                    this.lastClosestApproachUpdate = DateTime.UtcNow;
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
                SharpDX.DirectInput.Key.PageDown,
                SharpDX.DirectInput.Key.PageUp,
                out var changed);

            if (changed)
            {
                this.SimulatorContainer.SelectedObject = newSelectedObject;
                this.lastClosestApproachUpdate = new DateTime();
            }

            this.ComputeClosestApproach();
        }

        /// <summary>
        /// Draws the ground track
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        private void DrawGroundTrack(DeviceContext deviceContext)
        {
            if (!this.showGroundTracks)
            {
                return;
            }

            if (this.SelectedObject.Type == PhysicsObjectType.ArtificialSatellite
                && this.SelectedObject.ReferenceOrbit.IsBound)
            {
                if (!this.groundTracks.TryGetValue(this.SelectedObject, out var groundTrack))
                {
                    var primaryBodyRenderingObject = this.SimulatorContainer.GetRenderingObject(this.SelectedObject.PrimaryBody);

                    groundTrack = new GroundTrack(
                        this.RenderingManager2D,
                        this.SimulatorEngine.KeplerProblemSolver,
                        this.SelectedObject,
                        ((Sphere)primaryBodyRenderingObject.Model).TextureName,
                        Vector2.Zero);

                    this.groundTracks.Add(this.SelectedObject, groundTrack);
                }

                groundTrack.Position = UIHelpers.CalculateScreenPosition(
                    this.RenderingManager2D.ScreenRectangle,
                    new RectangleF(0, 0, groundTrack.Width, groundTrack.Height),
                    PositionRelationX.Right,
                    PositionRelationY.Top);

                groundTrack.Draw(deviceContext);
            }
        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Draw(DeviceContext deviceContext)
        {
            this.DrawGroundTrack(deviceContext);

            var selectedObjectOrbitPosition = new OrbitPosition();

            if (this.SelectedObject.PrimaryBody != null)
            {
                selectedObjectOrbitPosition = OrbitPosition.CalculateOrbitPosition(this.SelectedObject.PrimaryBody, this.SelectedObject.State);
            }
            else
            {
                selectedObjectOrbitPosition = new OrbitPosition(new Physics.Orbit(), 0.0);
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
                    calculateClosestApproach: true,
                    closestApproach: this.closestApproachData);
            }

            this.TextColorBrush.DrawText(
                deviceContext,
                selectedObjectText,
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(UIConstants.OffsetLeft, 90)));
        }

        /// <summary>
        /// Draws the arrows
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="arrowEffect">The arrow effect</param>
        /// <param name="camera">The camera</param>
        public void DrawArrows(SharpDX.Direct3D11.DeviceContext deviceContext, BasicEffect arrowEffect, SpaceCamera camera)
        {
            if (this.SelectedObject.Type == PhysicsObjectType.ArtificialSatellite)
            {
                var state = this.SelectedObject.State;
                state.MakeRelative(this.SelectedObject.PrimaryBody.State);

                var position = camera.ToDrawPosition(this.SelectedObject.Position);
                var targetPosition = camera.ToDrawPosition(this.SelectedObject.Position + camera.FromDraw(1) * state.Prograde);
                var upPosition = camera.ToDrawPosition(this.SelectedObject.Position + camera.FromDraw(1) * state.Normal);

                this.arrow.DrawBasis(
                    deviceContext,
                    arrowEffect,
                    camera,
                    camera.ToDraw(2.5E4),
                    Matrix.Translation(position),
                    (targetPosition - position).Normalized(),
                    (upPosition - position).Normalized(),
                    Color.Red,
                    Color.Blue,
                    Color.Green);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.arrow.Dispose();

            foreach (var groundTrack in this.groundTracks.Values)
            {
                groundTrack.Dispose();
            }
        }
    }
}
