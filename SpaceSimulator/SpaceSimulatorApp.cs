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
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Physics.Maneuvers;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.UI;
using SpaceSimulator.Rendering.Plot;
using SpaceSimulator.Common.ParticleSystem;

namespace SpaceSimulator
{
    /// <summary>
    /// The main class for the space simulator
    /// </summary>
    public class SpaceSimulatorApp : D3DApp
    {
        private readonly RenderingPasses renderingPasses = new RenderingPasses();

        private readonly SimulatorContainer simulatorContainer;
        private int framesSinceLast = 0;

        private BasicEffect sunEffect;
        private BasicEffect planetNoLightEffect;
        private BasicEffect planetEffect;
        private BasicEffect planetNoTextureEffect;
        private OrbitEffect orbitEffect;
        private OrbitEffect ringEffect;
        private BasicEffect arrowEffect;

        private readonly UIManager uiManager;
        private readonly UIStyle uiStyle;
        private readonly IList<UIComponent> uiComponents = new List<UIComponent>();

        //private ShaderResourceView randomTextureView;
        //private ParticleSystemRender fireParticleSystem;

        /// <summary>
        /// Creates a new space simulator application
        /// </summary>
        public SpaceSimulatorApp()
            : base("SpaceSimulator", new OrbitCamera(0.001f, 10000.0f * 1000), "OrbitCamera")
        {
            Console.WriteLine("");

            var environment = new Environments.SolarSystem(coplanar: false);
            //var environment = new Environments.EarthSystem();
            //var environment = new Environments.Horizons(new DateTime(2017, 8, 1, 0, 0, 0));

            this.simulatorContainer = environment.Create(this.GraphicsDevice);
            //this.SimulatorEngine.SimulationMode = PhysicsSimulationMode.KeplerProblemUniversalVariable;

            this.uiManager = new UIManager(this.RenderingManager2D)
            {
                //DrawBoundingRectangles = true
            };

            this.CameraManager.AddCamera("FollowCameraNormal", new FollowCamera(FollowCamera.Mode.FollowNormal));
            this.CameraManager.AddCamera("FollowCameraRadial", new FollowCamera(FollowCamera.Mode.FollowRadial));
            this.CameraManager.AddCamera("FollowCameraAscent", new FollowCamera(FollowCamera.Mode.FollowAscent));
            //this.CameraManager.AddCamera("GroundCamera", new GroundCamera()
            //{
            //    Latitude = 28.524058 * MathUtild.Deg2Rad * 0,
            //    Longitude = -80.65085 * MathUtild.Deg2Rad
            //});

            this.uiStyle = new UIStyle(this.RenderingManager2D);
        }

        /// <summary>
        /// Returns the simulator engine
        /// </summary>
        private SimulatorEngine SimulatorEngine => this.simulatorContainer.SimulatorEngine;

        /// <summary>
        /// Returns the rendering objects
        /// </summary>
        private IList<RenderingObject> RenderingObjects => this.simulatorContainer.RenderingObjects;

        /// <summary>
        /// Returns the space camera
        /// </summary>
        private SpaceCamera SpaceCamera => this.ActiveCamera as SpaceCamera;

        /// <summary>
        /// Creates the effect
        /// </summary>
        private void CreateEffect()
        {
            this.sunEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "SunTex", BasicVertex.CreateInput());
            this.planetNoLightEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "PlanetNoLightTex", BasicVertex.CreateInput());
            this.planetEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "PlanetTexLight", BasicVertex.CreateInput());
            this.planetNoTextureEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "PlanetLight", BasicVertex.CreateInput());
            this.orbitEffect = new OrbitEffect(this.GraphicsDevice, "Content/Effects/Orbit.fx", "Orbit", Rendering.OrbitVertex.CreateInput());
            this.ringEffect = new OrbitEffect(this.GraphicsDevice, "Content/Effects/Orbit.fx", "PlanetRing", Rendering.OrbitVertex.CreateInput());
            this.arrowEffect = new BasicEffect(this.GraphicsDevice, "Content/Effects/Basic.fx", "Light1", BasicVertex.CreateInput());
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.CreateEffect();

            this.uiComponents.Add(new TimeUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer));

            this.uiComponents.Add(new SelectedObjectUI(
                this.GraphicsDevice,
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer));

            this.uiComponents.Add(new CameraUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.CameraManager,
                this.simulatorContainer,
                (OrbitCamera)this.CameraManager["OrbitCamera"]));

            this.uiComponents.Add(new MainUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer,
                this.uiManager,
                this.uiStyle));

            this.uiComponents.Add(new ManeuverUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer,
                this.uiManager,
                this.uiStyle));

            this.uiComponents.Add(new CreateObjectUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer,
                this.uiManager,
                this.uiStyle));

            this.uiComponents.Add(new OverlayUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.CameraManager,
                this.uiManager,
                this.simulatorContainer,
                (OrbitCamera)this.CameraManager["OrbitCamera"],
                this.planetNoLightEffect,
                this.RenderToTexture));

            this.uiComponents.Add(new SimulatorControlUI(
                this.RenderingManager2D,
                this.KeyboardManager,
                this.MouseManager,
                this.simulatorContainer));

            //var values = new List<Vector2>();
            //var earthAtmosphericModel = new EarthAtmosphericModel();
            //for (double altitude = 0; altitude <= 100E3; altitude += 100)
            //{
            //    values.Add(new Vector2((float)altitude, (float)earthAtmosphericModel.DensityOfAir(altitude)));
            //}
            //this.plot2D = new Plot2D(this.RenderingManager2D, values, Color.Red, 500, 375, labelAxisX: "Altitude", labelAxisY: "Density of air");

            //this.randomTextureView = TextureHelpers.CreateRandomTexture(this.GraphicsDevice);
            //this.fireParticleSystem = new ParticleSystemRender(
            //    500 * 2 * 5,
            //    TextureHelpers.LoadTextureArray(this.GraphicsDevice, this.DeviceContext, new string[] { "Content/Textures/Effects/flare0.png" }),
            //    this.randomTextureView);
            //this.fireParticleSystem.Initialize(this.GraphicsDevice, "Content/Effects/Fire.fx");
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
                if (this.simulatorContainer.IsSlowMotion)
                {
                    this.framesSinceLast++;

                    if (this.framesSinceLast >= 6)
                    {
                        this.SimulatorEngine.Update();
                        this.framesSinceLast = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < this.simulatorContainer.SimulationSpeedMultiplier; i++)
                    {
                        this.SimulatorEngine.Update();
                    }
                }
            }

            foreach (var component in this.uiComponents)
            {
                component.AfterSimulationUpdate();
            }

            foreach (var currentObject in this.RenderingObjects)
            {
                currentObject.Update(this.SimulatorEngine);
            }

            //this.fireParticleSystem.Update(elapsed, this.TotalTime);
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
                RenderingObject.DrawOrbits(deviceContext, this.orbitEffect, this.SpaceCamera, this.RenderingObjects);
            });

            this.renderingPasses.Add2D((deviceContext, deviceContext2D) =>
            {
                foreach (var component in uiComponentsFirstPass)
                {
                    component.Draw(deviceContext2D);
                }
            });

            var selectedObjectUI = this.uiComponents.OfType<SelectedObjectUI>().FirstOrDefault();
            this.renderingPasses.Add3D((deviceContext, deviceContext2D) =>
            {
                RenderingObject.DrawPlanets(
                    deviceContext,
                    this.sunEffect,
                    this.planetEffect,
                    this.ringEffect,
                    this.SpaceCamera,
                    this.RenderingObjects);

                RenderingObject.DrawObjects(
                    deviceContext,
                    this.arrowEffect,
                    //this.planetNoTextureEffect,
                    this.planetEffect,
                    this.arrowEffect,
                    this.SpaceCamera,
                    this.RenderingObjects);

                selectedObjectUI.DrawArrows(deviceContext, this.arrowEffect, this.SpaceCamera);

                //var selectedObject = this.simulatorContainer.SelectedObject;
                //this.fireParticleSystem.SetEmitPosition(this.SpaceCamera.ToDrawPosition(selectedObject.Position));
                //this.fireParticleSystem.SetEmitDirection(MathHelpers.ToFloat(selectedObject.State.Retrograde));
                //this.fireParticleSystem.SetEyePosition(this.ActiveCamera.Position);
                //this.fireParticleSystem.Draw(deviceContext, this.ActiveCamera);

                //var blendFactor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                //deviceContext.Rasterizer.State = null;
                //deviceContext.OutputMerger.SetDepthStencilState(null, 0);
                //deviceContext.OutputMerger.SetBlendState(null, blendFactor, 0xffffffff);
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
            textBuilder.AppendLine($"Camera position: {this.CameraManager.ActiveCamera.Position}");

            foreach (var renderingObject in this.RenderingObjects)
            {
                textBuilder.AppendLine(
                    $"{renderingObject.PhysicsObject.Name} - " +
                    $"Position: {renderingObject.DrawPosition(this.SpaceCamera)}, " +
                    $"ShowSphere: {renderingObject.ShowModel}, " +
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
            this.planetNoTextureEffect.Dispose();
            this.orbitEffect.Dispose();
            this.planetEffect.Dispose();
            this.arrowEffect.Dispose();

            foreach (var renderingObject in this.RenderingObjects)
            {
                renderingObject.Dispose();
            }

            foreach (var component in this.uiComponents)
            {
                component.Dispose();
            }

            base.Dispose();
        }
    }
}