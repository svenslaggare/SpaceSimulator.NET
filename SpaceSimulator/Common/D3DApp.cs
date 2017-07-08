using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Diagnostics;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Windows;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Rendering2D;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace SpaceSimulator.Common
{
    /// <summary>
    /// Represents a D3D application
    /// </summary>
    public abstract class D3DApp : IDisposable
    {
        private readonly string title;

        private readonly RenderForm renderForm;
        private SharpDX.Direct3D11.Device graphicsDevice;
        private SwapChain swapChain;
        private SharpDX.Direct3D11.DeviceContext deviceContext;
        //private DeviceDebug graphicsDeviceDebug;

        private SharpDX.DXGI.Factory factory;

        private readonly int swapChainBufferCount = 1;

        //private readonly Format backbufferFormat = Format.R8G8B8A8_UNorm;
        private readonly Format backBufferFormat = Format.B8G8R8A8_UNorm;

        private SampleDescription sampleDescription;

        private bool resized = true;

        private Texture2D backBuffer;
        private RenderTargetView backBufferRenderView;
        private Texture2D depthBuffer;
        private DepthStencilView backBufferDepthView;

        private SharpDX.Direct2D1.DeviceContext deviceContext2D;
        private RenderingManager2D renderingManager2D;

        //private BaseCamera camera;
        private readonly CameraManager cameraManager;

        private readonly DirectInput directInput;
        private readonly KeyboardManager keyboardManager;
        private readonly MouseManager mouseManager;

        /// <summary>
        /// The position of the mouse
        /// </summary>
        public Vector2 MousePosition { get; private set; }

        private RenderToTexture renderToTexture;

        private int frameCount;
        private float elapsed;
        private float fps;
        private Stopwatch totalTimeWatch;

        /// <summary>
        /// Creates a new D3D application
        /// </summary>
        /// <param name="title">The title of the application</param>
        /// <param name="camera">The default camera</param>
        /// <param name="cameraName">The name of the default camera</param>
        public D3DApp(string title, BaseCamera camera, string cameraName = "Default")
        {
            this.title = title;
            this.renderForm = new RenderForm(title)
            {
                ClientSize = new System.Drawing.Size(1440, 900)
            };

            this.renderingManager2D = new RenderingManager2D(this.renderForm);

            this.cameraManager = new CameraManager();
            this.cameraManager.AddCamera(cameraName, camera, true);

            this.directInput = new DirectInput();
            this.keyboardManager = new KeyboardManager(this.directInput);
            this.mouseManager = new MouseManager(this.directInput);
            this.CreateDevice();
        }

        /// <summary>
        /// Returns the render form
        /// </summary>
        public RenderForm RenderForm
        {
            get { return this.renderForm; }
        }

        /// <summary>
        /// Returns the graphics device
        /// </summary>
        public SharpDX.Direct3D11.Device GraphicsDevice
        {
            get { return this.graphicsDevice; }
        }

        /// <summary>
        /// Returns the swap chain
        /// </summary>
        public SwapChain SwapChain
        {
            get { return this.swapChain; }
        }

        /// <summary>
        /// Returns the D3D device context
        /// </summary>
        public SharpDX.Direct3D11.DeviceContext DeviceContext
        {
            get { return this.deviceContext; }
        }

        /// <summary>
        /// Returns the render view
        /// </summary>
        public RenderTargetView BackBufferRenderView
        {
            get { return this.backBufferRenderView; }
        }

        /// <summary>
        /// Returns the depth view
        /// </summary>
        public DepthStencilView BackBufferDepthView
        {
            get { return this.backBufferDepthView; }
        }

        /// <summary>
        /// Returns the description for the back buffer
        /// </summary>
        public Texture2DDescription BackBufferDescription => this.backBuffer.Description;

        /// <summary>
        /// Returns the description for the depth back buffer
        /// </summary>
        public Texture2DDescription BackBufferDepthDescription => this.depthBuffer.Description;

        /// <summary>
        /// Returns the D2D context
        /// </summary>
        public SharpDX.Direct2D1.DeviceContext DeviceContext2D
        {
            get { return this.deviceContext2D; }
        }

        /// <summary>
        /// Returns the rendering 2D manager
        /// </summary>
        public RenderingManager2D RenderingManager2D
        {
            get { return this.renderingManager2D; }
        }

        /// <summary>
        /// Returns the active camera
        /// </summary>
        public BaseCamera ActiveCamera
        {
            get { return this.cameraManager.ActiveCamera; }
        }

        /// <summary>
        /// Returns the camera manager
        /// </summary>
        public CameraManager CameraManager
        {
            get { return this.cameraManager; }
        }

        /// <summary>
        /// Returns the keyboard manager
        /// </summary>
        public KeyboardManager KeyboardManager
        {
            get { return this.keyboardManager; }
        }

        /// <summary>
        /// Returns the mouse manager
        /// </summary>
        public MouseManager MouseManager
        {
            get { return this.mouseManager; }
        }

        /// <summary>
        /// Returns the total time
        /// </summary>
        public TimeSpan TotalTime
        {
            get { return this.totalTimeWatch.Elapsed; }
        }

        /// <summary>
        /// Renders to texture
        /// </summary>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="render">The render function</param>
        public RenderingImage2D RenderToTexture(Color backgroundColor, Action<SharpDX.Direct3D11.DeviceContext> render)
        {
            if (this.renderToTexture == null)
            {
                this.renderToTexture = new RenderToTexture(
                    this.GraphicsDevice,
                    this.BackBufferDescription,
                    this.BackBufferDepthDescription);
            }

            var image = this.renderToTexture.Render(
                this.deviceContext,
                this.BackBufferRenderView,
                this.backBufferDepthView,
                backgroundColor,
                render);
            this.RenderingManager2D.AddResource(image);
            return image;
        }

        /// <summary>
        /// Creates the device
        /// </summary>
        private void CreateDevice()
        {
            this.sampleDescription = new SampleDescription(4, 0);
            //this.sampleDescription = new SampleDescription(1, 0);

            //Create the device and swap chain
            var swapChainDescription = new SwapChainDescription()
            {
                BufferCount = this.swapChainBufferCount,
                ModeDescription = new ModeDescription(
                    this.renderForm.ClientSize.Width,
                    this.renderForm.ClientSize.Height,
                    new Rational(60, 1),
                    this.backBufferFormat),
                IsWindowed = true,
                OutputHandle = this.renderForm.Handle,
                SampleDescription = this.sampleDescription,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
                //DeviceCreationFlags.BgraSupport,
                swapChainDescription,
                out this.graphicsDevice,
                out this.swapChain);

            //this.graphicsDeviceDebug = new DeviceDebug(this.graphicsDevice);

            this.deviceContext = this.graphicsDevice.ImmediateContext;

            // Ignore all windows events
            this.factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            this.factory.MakeWindowAssociation(this.renderForm.Handle, WindowAssociationFlags.IgnoreAll);
        }

        /// <summary>
        /// Resizes the render buffers
        /// </summary>
        /// <param name="isFirstTime">Indicates if this is the first time the buffers are resized</param>
        private void ResizeRenderBuffers(bool isFirstTime = false)
        {
            //Dispose all previous allocated resources
            Utilities.Dispose(ref this.backBuffer);
            Utilities.Dispose(ref this.backBufferRenderView);
            Utilities.Dispose(ref this.depthBuffer);
            Utilities.Dispose(ref this.backBufferDepthView);

            Utilities.Dispose(ref this.deviceContext2D);

            //Resize the backbuffer
            this.swapChain.ResizeBuffers(
                this.swapChainBufferCount,
                this.renderForm.ClientSize.Width,
                this.renderForm.ClientSize.Height,
                this.backBufferFormat,
                SwapChainFlags.None);

            //Get the backbuffer from the swapchain
            this.backBuffer = Texture2D.FromSwapChain<Texture2D>(this.swapChain, 0);

            //Renderview on the backbuffer
            this.backBufferRenderView = new RenderTargetView(this.graphicsDevice, this.backBuffer);

            //Create the depth buffer
            this.depthBuffer = new Texture2D(this.graphicsDevice, new Texture2DDescription()
            {
                Format = Format.D24_UNorm_S8_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = this.renderForm.ClientSize.Width,
                Height = this.renderForm.ClientSize.Height,
                SampleDescription = this.sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            //Create the depth buffer view
            this.backBufferDepthView = new DepthStencilView(this.graphicsDevice, this.depthBuffer);

            //Setup targets and viewport for rendering
            this.deviceContext.Rasterizer.SetViewport(new Viewport(
                0,
                0,
                this.renderForm.ClientSize.Width,
                this.renderForm.ClientSize.Height,
                0.0f,
                1.0f));

            this.deviceContext.OutputMerger.SetTargets(backBufferDepthView, backBufferRenderView);

            this.renderForm.Resize += (sender, e) =>
            {
                //When the window is minimized, the window is resized with a size of zero
                if (!this.renderForm.ClientSize.IsEmpty)
                {
                    this.resized = true;
                }
            };

            using (var backBuffer = this.swapChain.GetBackBuffer<Surface>(0))
            {
                this.deviceContext2D = new SharpDX.Direct2D1.DeviceContext(backBuffer);
            }

            if (!isFirstTime)
            {
                this.renderingManager2D.Update(this.deviceContext2D);
            }
        }

        /// <summary>
        /// Called when the window is resized
        /// </summary>
        protected virtual void OnResized()
        {
            this.cameraManager.SetProjection(
                0.25f * MathUtil.Pi,
                this.RenderForm.ClientSize.Width,
                this.RenderForm.ClientSize.Height,
                0.001f,
                100000.0f);
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        public virtual void Initialize()
        {
            this.RenderForm.MouseDown += (sender, args) =>
            {
                this.OnMouseButtonDown(new Vector2(args.X, args.Y), args.Button);
            };

            this.RenderForm.MouseMove += (sender, args) =>
            {
                this.OnMouseMove(new Vector2(args.X, args.Y), args.Button);
            };

            this.RenderForm.MouseWheel += (sender, args) =>
            {
                this.OnMouseScroll(args.Delta);
            };
        }

        /// <summary>
        /// Loads the content
        /// </summary>
        /// <remarks>Compred to <see cref="Initialize"/> this is called after the device context has been created</remarks>
        public virtual void LoadContent()
        {

        }

        /// <summary>
        /// The mouse moved
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="button">The state of the buttons</param>
        protected virtual void OnMouseMove(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
        {
            this.MousePosition = mousePosition;
            this.ActiveCamera.HandleMouseMove(mousePosition, button);
        }

        /// <summary>
        /// A mouse button is pressed down
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="button">The state of the buttons</param>
        protected virtual void OnMouseButtonDown(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
        {
            this.ActiveCamera.HandleMouseDown(mousePosition, button);
        }

        /// <summary>
        /// The mouse was scrolled
        /// </summary>
        /// <param name="delta">the delta</param>
        protected virtual void OnMouseScroll(int delta)
        {
            this.ActiveCamera.HandleMouseScroll(delta);
        }

        /// <summary>
        /// Updates the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public virtual void Update(TimeSpan elapsed)
        {            
            this.keyboardManager.Update(this.renderForm.Focused);
            this.mouseManager.Update(this.renderForm.Focused, this.MousePosition);
            this.ActiveCamera.HandleKeyboard(this.keyboardManager.State, elapsed);
            this.ActiveCamera.UpdateViewMatrix();
        }

        /// <summary>
        /// Called before the first frame is drawn
        /// </summary>
        public virtual void BeforeFirstDraw()
        {

        }

        /// <summary>
        /// Draws the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public abstract void Draw(TimeSpan elapsed);

        /// <summary>
        /// Runs the application
        /// </summary>
        public void Run()
        {
            this.Initialize();

            var clock = new Stopwatch();
            var elapsed = new TimeSpan();

            this.totalTimeWatch = new Stopwatch();
            this.totalTimeWatch.Start();
            var hasLoaded = false;
            var firstFrame = true;

            RenderLoop.Run(this.renderForm, () =>
            {
                if (this.resized)
                {
                    this.ResizeRenderBuffers(!hasLoaded);
                    this.OnResized();
                    this.resized = false;

                    if (!hasLoaded)
                    {
                        this.LoadContent();
                        hasLoaded = true;
                    }
                }

                this.elapsed += (float)elapsed.TotalMilliseconds;

                if (this.elapsed > 1000.0f)
                {
                    this.fps = this.frameCount / (this.elapsed / 1000.0f);
                    this.elapsed = 0;
                    this.frameCount = 0;
                }
                else
                {
                    this.frameCount++;
                }

                this.renderForm.Text = this.title + " FPS: " + this.fps;
                this.Update(elapsed);

                if (firstFrame)
                {
                    this.BeforeFirstDraw();
                    this.renderingManager2D.Update(this.deviceContext2D);
                    firstFrame = false;
                }

                this.Draw(elapsed);

                elapsed = clock.Elapsed;
                clock.Restart();
            });
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public virtual void Dispose()
        {
            this.depthBuffer.Dispose();
            this.backBufferDepthView.Dispose();
            this.backBufferRenderView.Dispose();
            this.backBuffer.Dispose();

            this.deviceContext.ClearState();
            this.deviceContext.Flush();
            this.swapChain.Dispose();
            this.graphicsDevice.Dispose();
            this.renderForm.Dispose();
            this.factory.Dispose();

            this.deviceContext2D.Dispose();
            this.renderingManager2D.Dispose();

            this.keyboardManager.Dispose();
            this.mouseManager.Dispose();
            this.directInput.Dispose();
        }
    }
}
