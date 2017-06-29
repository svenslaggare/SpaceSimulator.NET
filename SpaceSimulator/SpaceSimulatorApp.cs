using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics;
using SpaceSimulator.Physics.Atmosphere;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.UI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator
{
    /// <summary>
    /// The main entry class for the space simulator
    /// </summary>
    public class SpaceSimulatorApp : D3DApp
    {
        private BasicEffect sunEffect;
        private BasicEffect planetEffect;
        private OrbitEffect orbitEffect;

        private RenderingObject referenceRenderingObject;
        private readonly IList<RenderingObject> renderingObjects;
        private readonly SimulatorEngine simulatorEngine;
        private readonly SimulatorContainer simulatorContainer;

        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;
        private readonly IList<UIComponent> uiComponents = new List<UIComponent>();

        //private Plot2D plot2D;

        /// <summary>
        /// Creates a new space simulator application
        /// </summary>
        public SpaceSimulatorApp()
            : base("SpaceSimulator")
        {
            Console.WriteLine("");

            //this.simulatorEngine.SimulationMode = PhysicsSimulationMode.KeplerProblemUniversalVariable;

            this.simulatorEngine.SimulationMode = PhysicsSimulationMode.KeplerProblemUniversalVariable;

            this.OrbitCamera.MinRadius = 0.001f;
            this.OrbitCamera.MaxRadius = 7000.0f * 1000;

            this.uiManager = new UIManager(this.RenderingManager2D)
            {
                DrawBoundingRectangles = false
            };

            this.uiStyle = new UIStyle(this.RenderingManager2D);

            this.simulatorContainer = new SimulatorContainer(this.simulatorEngine);
            this.uiComponents.Add(new TimeUI(this.RenderingManager2D, this.KeyboardManager, this.simulatorContainer));
            this.uiComponents.Add(new SelectedObjectUI(this.RenderingManager2D, this.KeyboardManager, this.simulatorContainer));
            this.uiComponents.Add(new CameraUI(this.RenderingManager2D, this.KeyboardManager, this.simulatorContainer, this.OrbitCamera));
            this.uiComponents.Add(new ManeuverUI(this.RenderingManager2D, this.KeyboardManager, this.simulatorContainer, this.uiManager, this.uiStyle));
        }

        /// <summary>
        /// Returns the orbit camera
        /// </summary>
        private OrbitCamera OrbitCamera
        {
            get { return this.Camera as OrbitCamera; }
        }

        /// <summary>
        /// Creates the effect
        /// </summary>
        private void CreateEffect()
        {
            this.sunEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "NoLightTex");
            this.planetEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "Light1Tex");
            this.orbitEffect = new OrbitEffect(this.GraphicsDevice, "Content/Effects/Orbit.fx", "Orbit");

            this.sunEffect.CreateInputLayout(BasicVertex.CreateInput());
            this.planetEffect.CreateInputLayout(BasicVertex.CreateInput());
            this.orbitEffect.CreateInputLayout(Rendering.OrbitVertex.CreateInput());
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.CreateEffect();

            //var values = new List<Vector2>();
            //var earthAtmosphericModel = new EarthAtmosphericModel();
            //for (double altitude = 0; altitude <= 100E3; altitude += 100)
            //{
            //    values.Add(new Vector2((float)altitude, (float)earthAtmosphericModel.DensityOfAir(altitude)));
            //}
            //this.plot2D = new Plot2D(this.RenderingManager2D, values, Color.Red, 500, 375, labelAxisX: "Altitude", labelAxisY: "Density of air");
        }

        protected override void OnMouseButtonDown(Vector2 mousePosition, MouseButtons button)
        {
            base.OnMouseButtonDown(mousePosition, button);
            this.uiManager.OnMouseButtonDown(mousePosition, button);
        }

        protected override void OnResized()
        {
            base.OnResized();
            this.uiManager.Invalidate();
        }

        /// <summary>
        /// Updates the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Update(TimeSpan elapsed)
        {
            foreach (var component in this.uiComponents)
            {
                component.Update(elapsed);
            }

            this.uiManager.Update(elapsed);

            if (!this.simulatorContainer.Paused)
            {
                this.simulatorEngine.Update();
            }

            foreach (var component in this.uiComponents)
            {
                component.AfterSimulationUpdate();
            }

            foreach (var currentObject in this.renderingObjects)
            {
                currentObject.Update(this.simulatorEngine);
            }

            base.Update(elapsed);
        }

        /// <summary>
        /// Draws the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Draw(TimeSpan elapsed)
        {
            //Clear views
            this.DeviceContext.ClearDepthStencilView(this.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            this.DeviceContext.ClearRenderTargetView(this.RenderView, Color.Black);

            //Draw 3D
            this.referenceRenderingObject.Draw(this.DeviceContext, this.sunEffect, this.orbitEffect, this.Camera);
            RenderingObject.Draw(this.DeviceContext, this.planetEffect, this.orbitEffect, this.Camera, this.renderingObjects);

            //Draw 2D
            this.DeviceContext2D.BeginDraw();

            foreach (var component in this.uiComponents)
            {
                component.Draw(this.DeviceContext2D);
            }

            this.uiManager.Draw(this.DeviceContext2D);
            //this.plot2D.Draw(this.DeviceContext2D, new Vector2(400, 200));
            this.DeviceContext2D.EndDraw();

            //Present
            this.SwapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Disposes the resources
        /// </summary>
        public override void Dispose()
        {
            this.uiManager.Dispose();

            this.sunEffect.Dispose();
            this.planetEffect.Dispose();
            this.orbitEffect.Dispose();

            foreach (var currentObject in this.renderingObjects)
            {
                currentObject.Dispose();
            }

            base.Dispose();
        }
    }
}