using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;
using SpaceSimulator.Common.Camera;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing the camera
    /// </summary>
    public class CameraUI : UIComponent
    {
        private readonly CameraManager cameraManager;
        private readonly OrbitCamera orbitCamera;

        private PhysicsObject focusObject;
        private int focusObjectIndex = 0;

        private int cameraIndex = 0;
        
        /// <summary>
        /// Creates a new camera UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="cameraManager">The camera manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        /// <param name="orbitCamera">The orbit camera</param>
        public CameraUI(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            CameraManager cameraManager,
            SimulatorContainer simulatorContainer,
            OrbitCamera orbitCamera)
            : base(renderingManager2D, keyboardManager, mouseManager, simulatorContainer)
        {
            this.cameraManager = cameraManager;
            this.orbitCamera = orbitCamera;
            this.focusObjectIndex = this.SimulatorEngine.Objects.Count - 1;
            this.focusObject = this.SimulatorEngine.Objects[this.focusObjectIndex];

            foreach (var spaceCamera in this.cameraManager.Cameras.OfType<SpaceCamera>())
            {
                spaceCamera.SetScaleFactor(SimulatorContainer.SimulatorEngine.ObjectOfReference);
                spaceCamera.SetFocus(this.focusObject);
            }

            this.orbitCamera.Radius = this.GetStartRadius();
        }

        /// <summary>
        /// Sets the scale factor based on the current focus object
        /// </summary>
        private void SetScaleFactorFromFocusObject()
        {
            if (this.focusObject.Type != PhysicsObjectType.ArtificialSatellite)
            {
                this.orbitCamera.SetScaleFactor((NaturalSatelliteObject)this.focusObject);
            }
            else
            {
                this.orbitCamera.SetScaleFactor(this.focusObject.PrimaryBody);
            }
        }

        /// <summary>
        /// Returns the start radius of the camera
        /// </summary>
        private float GetStartRadius()
        {
            if (this.focusObject is NaturalSatelliteObject naturalObject)
            {
                return this.orbitCamera.ToDraw(naturalObject.Radius * 5.0);
            }
            else
            {
                return this.orbitCamera.ToDraw(2.5E6);
            }
        }

        /// <summary>
        /// Indicates if the given object can be set as focus
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        private bool CanSetFocus(PhysicsObject physicsObject)
        {
            if (this.cameraManager.ActiveCamera is SpaceCamera spaceCamera)
            {
                return spaceCamera.CanSetFocus(physicsObject);
            }

            return true;
        }

        /// <summary>
        /// Sets a valid focus object based on the given camera
        /// </summary>
        /// <param name="camera">The camera</param>
        private void SetValidFocus(SpaceCamera camera)
        {
            for (int i = 0; i < this.SimulatorEngine.Objects.Count; i++)
            {
                var objectIndex = MathHelpers.Mod(this.focusObjectIndex + i, this.SimulatorEngine.Objects.Count);
                var currentObject = this.SimulatorEngine.Objects[objectIndex];

                if (this.CanSetFocus(currentObject))
                {
                    this.focusObjectIndex = objectIndex;
                    this.focusObject = currentObject;
                    break;
                }
            }

            camera.SetFocus(this.focusObject);
        }

        public override void Update(TimeSpan elapsed)
        {
            this.focusObject = UIComponentHelpers.SelectObjectUpAndDown(
                this.KeyboardManager,
                this.SimulatorEngine.Objects,
                ref this.focusObjectIndex,
                SharpDX.DirectInput.Key.End,
                SharpDX.DirectInput.Key.Home,
                out var changed,
                this.CanSetFocus);

            //{
            //    if (this.cameraManager.ActiveCamera is SpaceCamera spaceCamera)
            //    {
            //        spaceCamera.SetScaleFactor2(this.SimulatorEngine.Objects);
            //    }
            //}

            if (changed)
            {
                //this.SetScaleFactorFromFocusObject();
                this.orbitCamera.Radius = this.GetStartRadius();
            }

            if (this.KeyboardManager.IsKeyPressed(SharpDX.DirectInput.Key.C))
            {
                MathHelpers.ModIncrease(ref this.cameraIndex, this.cameraManager.CameraNames.Count);
                var spaceCamera = this.cameraManager.SetActiveCamera(this.cameraManager.CameraNames[this.cameraIndex]) as SpaceCamera;
                this.SetValidFocus(spaceCamera);
            }
        }

        public override void AfterSimulationUpdate()
        {
            var spaceCamera = this.cameraManager.ActiveCamera as SpaceCamera;
            spaceCamera.SetFocus(this.focusObject);
        }

        public override void Draw(DeviceContext deviceContext)
        {
            this.TextColorBrush.DrawText(
                deviceContext,
                $"Focused object: {this.focusObject.Name} ({this.cameraManager.ActiveCameraName})",
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(UIConstants.OffsetLeft, 43)));
        }
    }
}
