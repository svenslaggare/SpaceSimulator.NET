using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.UI;

namespace SpaceSimulator
{
    /// <summary>
    /// The main entry class for the space simulator
    /// </summary>
    public class SpaceSimulatorApp : D3DApp
    {
        private readonly RenderingPasses renderingPasses = new RenderingPasses();

        private BasicEffect sunEffect;
        private BasicEffect planetNoLightEffect;
        private BasicEffect planetEffect;
        private OrbitEffect orbitEffect;

        private readonly IList<RenderingObject> renderingObjects;
        private readonly SimulatorEngine simulatorEngine;
        private readonly SimulatorContainer simulatorContainer;

        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;
        private readonly IList<UIComponent> uiComponents = new List<UIComponent>();

        /// <summary>
        /// Creates a new space simulator application
        /// </summary>
        public SpaceSimulatorApp()
            : base("SpaceSimulator")
        {
            Console.WriteLine("");

            (this.simulatorEngine, this.renderingObjects) = Simulator.Environments.SolarSystem.Create(this.GraphicsDevice, this.OrbitCamera, false);
            this.simulatorEngine.SimulationMode = PhysicsSimulationMode.KeplerProblemUniversalVariable;

            //(this.simulatorEngine, this.renderingObjects) = Simulator.Environments.EarthSystem.Create(this.GraphicsDevice, this.OrbitCamera);
            //this.simulatorEngine.SimulationMode = PhysicsSimulationMode.KeplerProblemUniversalVariable;

            this.OrbitCamera.MinRadius = 0.001f;
            //this.OrbitCamera.MaxRadius = 15000.0f;
            this.OrbitCamera.MaxRadius = 10000.0f;

            this.uiManager = new UIManager(this.RenderingManager2D)
            {
                //DrawBoundingRectangles = true
            };

            this.uiStyle = new UIStyle(this.RenderingManager2D);

            this.simulatorContainer = new SimulatorContainer(this.simulatorEngine, this.renderingObjects);
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
            this.sunEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "SunTex");
            this.planetNoLightEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "NoLightTex");
            this.planetEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "Light1Tex");
            this.orbitEffect = new OrbitEffect(this.GraphicsDevice, "Content/Effects/Orbit.fx", "Orbit");

            this.sunEffect.CreateInputLayout(BasicVertex.CreateInput());
            this.planetEffect.CreateInputLayout(BasicVertex.CreateInput());
            this.planetNoLightEffect.CreateInputLayout(BasicVertex.CreateInput());
            this.orbitEffect.CreateInputLayout(Rendering.OrbitVertex.CreateInput());
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.CreateEffect();

            this.uiComponents.Add(new TimeUI(this.RenderingManager2D, this.KeyboardManager, this.MouseManager, this.simulatorContainer));
            this.uiComponents.Add(new SelectedObjectUI(this.RenderingManager2D, this.KeyboardManager, this.MouseManager, this.simulatorContainer));
            this.uiComponents.Add(new CameraUI(this.RenderingManager2D, this.KeyboardManager, this.MouseManager, this.simulatorContainer, this.OrbitCamera));
            this.uiComponents.Add(new ManeuverUI(this.RenderingManager2D, this.KeyboardManager, this.MouseManager, this.simulatorContainer, this.uiManager, this.uiStyle));
            this.uiComponents.Add(new OverlayUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.uiManager,
                this.simulatorContainer,
                this.OrbitCamera,
                this.planetNoLightEffect,
                this.RenderToTexture));

            //var values = new List<Vector2>();
            //var earthAtmosphericModel = new EarthAtmosphericModel();
            //for (double altitude = 0; altitude <= 100E3; altitude += 100)
            //{
            //    values.Add(new Vector2((float)altitude, (float)earthAtmosphericModel.DensityOfAir(altitude)));
            //}
            //this.plot2D = new Plot2D(this.RenderingManager2D, values, Color.Red, 500, 375, labelAxisX: "Altitude", labelAxisY: "Density of air");

            //var satellit1 = this.simulatorEngine.Objects[2];
            //var satellit2 = this.simulatorEngine.Objects[3];
            //var satellite1Orbit = Physics.OrbitPosition.CalculateOrbitPosition(satellit1);
            //var satellite2Orbit = Physics.OrbitPosition.CalculateOrbitPosition(satellit2);
            //var synodicPeriod = Physics.OrbitFormulas.SynodicPeriod(satellite1Orbit.Orbit.Period, satellite2Orbit.Orbit.Period);
            //var possibleLaunches = InterceptManeuver.Intercept(
            //    this.simulatorEngine,
            //    satellit1.PrimaryBody,
            //    satellit1,
            //    satellit1.State,
            //    satellite1Orbit,
            //    satellit2,
            //    satellite2Orbit,
            //    600,
            //    10000,
            //    0,
            //    8.0 * 60.0 * 60.0,
            //    out var optimtalDeltaV,
            //    out var optimalLaunchTime,
            //    out var optimalInterceptTime,
            //    60.0,
            //    true);

            //this.deltaVChart = PlotHeatmap.CreateDeltaVChart(this.RenderingManager2D, possibleLaunches);
        }

        protected override void OnMouseButtonDown(Vector2 mousePosition, MouseButtons button)
        {
            base.OnMouseButtonDown(mousePosition, button);
            this.uiManager.OnMouseButtonDown(mousePosition, button);

            foreach (var component in this.uiComponents)
            {
                component.OnMouseButtonDown(mousePosition, button);
            }
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
            base.Update(elapsed);

            foreach (var component in this.uiComponents)
            {
                component.Update(elapsed);
            }

            this.uiManager.Update(elapsed);

            if (!this.simulatorContainer.IsPaused)
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
        }

        public override void BeforeFirstDraw()
        {
            foreach (var component in this.uiComponents)
            {
                component.BeforeFirstDraw(this.DeviceContext2D);
            }

            //Setup rendering passes
            (var uiComponentsFirstPass, var uiComponentsSecondPass) = this.uiComponents.Split(component => component is OverlayUI);

            this.renderingPasses.Add3D((deviceContext, deviceContext2D) =>
            {
                RenderingObject.DrawOrbits(deviceContext, this.orbitEffect, this.Camera, this.renderingObjects);
            });

            this.renderingPasses.Add2D((deviceContext, deviceContext2D) =>
            {
                foreach (var component in uiComponentsFirstPass)
                {
                    component.Draw(deviceContext2D);
                }
            });

            this.renderingPasses.Add3D((deviceContext, deviceContext2D) =>
            {
                RenderingObject.DrawSpheres(
                    deviceContext,
                    this.sunEffect,
                    this.planetEffect,
                    this.orbitEffect,
                    this.Camera,
                    this.renderingObjects);
            });

            this.renderingPasses.Add2D((deviceContext, deviceContext2D) =>
            {
                foreach (var component in uiComponentsSecondPass)
                {
                    component.Draw(deviceContext2D);
                }

                this.uiManager.Draw(deviceContext2D);
                //this.DrawDebug();
            });
        }

        /// <summary>
        /// Draws debug information
        /// </summary>
        private void DrawDebug()
        {
            var textBuilder = new StringBuilder();

            foreach (var renderingObject in this.renderingObjects)
            {
                textBuilder.AppendLine(
                    $"{renderingObject.PhysicsObject.Name} - " +
                    $"Position: {renderingObject.DrawPosition(this.Camera)}, " +
                    $"ShowSphere: {renderingObject.ShowSphere}, " +
                    $"ShowOrbit: {renderingObject.ShowOrbit}");
            }

            this.RenderingManager2D.DefaultSolidColorBrush.DrawText(
                this.DeviceContext2D,
                textBuilder.ToString(),
                this.RenderingManager2D.DefaultTextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(700, 50)));
        }

        /// <summary>
        /// Draws the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Draw(TimeSpan elapsed)
        {
            //Clear views
            this.DeviceContext.ClearDepthStencilView(this.BackBufferDepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            this.DeviceContext.ClearRenderTargetView(this.BackBufferRenderView, Color.Black);

            this.sunEffect.SetBlurSizeX(1 * (float)Math.Sin(0.1d * this.TotalTime.TotalSeconds));
            this.sunEffect.SetBlurSizeY(1 * (float)Math.Cos(0.1d * this.TotalTime.TotalSeconds));

            //Render
            this.renderingPasses.Render(this.DeviceContext, this.DeviceContext2D);

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
            this.planetNoLightEffect.Dispose();
            this.orbitEffect.Dispose();

            foreach (var currentObject in this.renderingObjects)
            {
                currentObject.Dispose();
            }

            foreach (var component in this.uiComponents)
            {
                component.Dispose();
            }

            base.Dispose();
        }
    }
}