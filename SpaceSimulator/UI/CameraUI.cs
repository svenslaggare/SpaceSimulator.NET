using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing the camera
    /// </summary>
    public class CameraUI : UIComponent
    {
        private readonly OrbitCamera orbitCamera;

        private PhysicsObject focusObject;
        private int focusObjectIndex = 0;
        
        /// <summary>
        /// Creates a new camera UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        /// <param name="orbitCamera">The orbit camera</param>
        public CameraUI(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, SimulatorContainer simulatorContainer, OrbitCamera orbitCamera)
            : base(renderingManager2D, keyboardManager, simulatorContainer)
        {
            this.orbitCamera = orbitCamera;
            this.focusObject = this.SimulatorEngine.Objects[this.focusObjectIndex];

            //this.SetScaleFactorFromFocusObject();
            this.orbitCamera.SetScaleFactor(SimulatorContainer.SimulatorEngine.ObjectOfReference);
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

        public override void Update(TimeSpan elapsed)
        {
            this.focusObject = UIComponentHelpers.SelectObjectUpAndDown(
                this.KeyboardManager,
                this.SimulatorEngine.Objects,
                ref this.focusObjectIndex,
                SharpDX.DirectInput.Key.Home,
                SharpDX.DirectInput.Key.End,
                out var changed);

            if (changed)
            {
                this.SetScaleFactorFromFocusObject();
                this.orbitCamera.Radius = this.GetStartRadius();
            }
        }

        public override void AfterSimulationUpdate()
        {
            this.orbitCamera.Focus = this.focusObject.Position;
        }

        public override void Draw(DeviceContext deviceContext)
        {
            
        }
    }
}
