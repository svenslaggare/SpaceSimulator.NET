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
		private RenderTargetView renderView;
		private Texture2D depthBuffer;
		private DepthStencilView depthView;

        private SharpDX.Direct2D1.DeviceContext deviceContext2D;
        private RenderingManager2D renderingManager2D;
    
        private BaseCamera camera;
        private KeyboardManager keyboardManager;

        private int frameCount;
		private float elapsed;
		private float fps;
		private Stopwatch totalTimeWatch;

		/// <summary>
		/// Creates a new D3D application
		/// </summary>
		/// <param name="title">The title of the application</param>
		public D3DApp(string title)
		{
			this.title = title;
            this.renderForm = new RenderForm(title)
            {
                //ClientSize = new System.Drawing.Size(1280, 720)
                ClientSize = new System.Drawing.Size(1440, 900)
            };

            this.renderingManager2D = new RenderingManager2D(this.renderForm);

            this.camera = new OrbitCamera();
            this.keyboardManager = new KeyboardManager();
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
		protected SwapChain SwapChain
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
		protected RenderTargetView RenderView
		{
			get { return this.renderView; }
		}

		/// <summary>
		/// Returns the depth view
		/// </summary>
		protected DepthStencilView DepthView
		{
			get { return this.depthView; }
		}

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
        protected RenderingManager2D RenderingManager2D
        {
            get { return this.renderingManager2D; }
        }

        /// <summary>
        /// Returns the camera
        /// </summary>
        protected BaseCamera Camera
        {
            get { return this.camera; }
        }

        /// <summary>
        /// Returns the keyboard manager
        /// </summary>
        protected KeyboardManager KeyboardManager
		{
			get { return this.keyboardManager; }
		}

		/// <summary>
		/// Returns the total time
		/// </summary>
		public TimeSpan TotalTime
		{
			get { return this.totalTimeWatch.Elapsed; }
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
                //DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
                DeviceCreationFlags.BgraSupport,
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
        private void ResizeRenderBuffers()
		{
			//Dispose all previous allocated resources
			Utilities.Dispose(ref this.backBuffer);
			Utilities.Dispose(ref this.renderView);
			Utilities.Dispose(ref this.depthBuffer);
			Utilities.Dispose(ref this.depthView);

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
			this.renderView = new RenderTargetView(this.graphicsDevice, this.backBuffer);

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
            this.depthView = new DepthStencilView(this.graphicsDevice, this.depthBuffer);
            
            //Setup targets and viewport for rendering
            this.deviceContext.Rasterizer.SetViewport(new Viewport(
                0,
                0,
                this.renderForm.ClientSize.Width,
                this.renderForm.ClientSize.Height,
                0.0f,
                1.0f));

            this.deviceContext.OutputMerger.SetTargets(depthView, renderView);

            this.renderForm.Resize += (sender, e) =>
            {
                //When the window is minimized, the window is resized with a size of zero
                if (!this.renderForm.ClientSize.IsEmpty)
                {
                    this.resized = true;
                }
            };

            using (var backBuffer = swapChain.GetBackBuffer<Surface>(0))
            {
                this.deviceContext2D = new SharpDX.Direct2D1.DeviceContext(backBuffer);
            }

            this.renderingManager2D.Update(this.deviceContext2D);
        }

        /// <summary>
        /// Called when the window is resized
        /// </summary>
        protected virtual void OnResized()
		{
            this.camera.SetLens(
                0.25f * MathUtil.Pi,
                this.RenderForm.ClientSize.Width / (float)this.RenderForm.ClientSize.Height,
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
				//this.lastMousePosition.X = args.X;
				//this.lastMousePosition.Y = args.Y;
			};

			this.RenderForm.MouseMove += (sender, args) =>
			{
				this.OnMouseMove(new Vector2(args.X, args.Y), args.Button);
				//this.lastMousePosition.X = args.X;
				//this.lastMousePosition.Y = args.Y;
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
            this.camera.HandleMouseMove(mousePosition, button);
		}

		/// <summary>
		/// A mouse button is pressed down
		/// </summary>
		/// <param name="mousePosition">The position of the mouse</param>
		/// <param name="button">The state of the buttons</param>
		protected virtual void OnMouseButtonDown(Vector2 mousePosition, System.Windows.Forms.MouseButtons button)
		{
            this.camera.HandleMouseDown(mousePosition, button);
		}

        /// <summary>
        /// The mouse was scrolled
        /// </summary>
        /// <param name="delta">the delta</param>
        protected virtual void OnMouseScroll(int delta)
        {
            this.camera.HandleMouseScroll(delta);
        }

        /// <summary>
        /// Updates the application
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public virtual void Update(TimeSpan elapsed)
		{
            this.keyboardManager.Update();
            this.camera.HandleKeyboard(this.keyboardManager.State, elapsed);
            this.camera.UpdateViewMatrix();
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

			RenderLoop.Run(this.renderForm, () =>
			{
				if (this.resized)
				{
                    this.ResizeRenderBuffers();
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
			this.depthView.Dispose();
			this.renderView.Dispose();
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
		}
	}
}
