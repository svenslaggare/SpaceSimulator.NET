using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SpaceSimulator.Common;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Common.Rendering2D;
using SpaceSimulator.Common.UI;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;
using SpaceSimulator.Common.Camera;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing overlays
    /// </summary>
    public class OverlayUI : UIComponent
    {
        private readonly CameraManager cameraManager;
        private readonly UIManager uiManager;

        private readonly OrbitCamera orbitCamera;
        private readonly BasicEffect thumbnailEffect;

        private readonly RenderToTexture renderToTexture;

        private readonly IList<Vector3d> spherePoints = new List<Vector3d>();
        private readonly IList<OverlayObject> overlayObjects = new List<OverlayObject>();

        /// <summary>
        /// Renders to texture
        /// </summary>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="render">The render function</param>
        public delegate RenderingImage2D RenderToTexture(Color backgroundColor, Action<SharpDX.Direct3D11.DeviceContext> render);

        /// <summary>
        /// Object being overlayed
        /// </summary>
        private class OverlayObject : IDisposable, IComparable<OverlayObject>
        {
            /// <summary>
            /// The rendering object
            /// </summary>
            public RenderingObject RenderingObject { get; }

            /// <summary>
            /// The text
            /// </summary>
            public string Text => this.RenderingObject.PhysicsObject.Name;

            /// <summary>
            /// The size of the text
            /// </summary>
            public Size2 TextSize { get; }

            /// <summary>
            /// The thumbnail
            /// </summary>
            public RenderingImage2D Thumbnail { get; set; }

            /// <summary>
            /// The scale which the thumbnail is being drawn with
            /// </summary>
            public float ThumbnailScale { get; } = 1.0f / 32.0f;

            /// <summary>
            /// The size of the drawn thumbnail
            /// </summary>
            public Vector2 ThumbnailSize => this.ThumbnailScale * new Vector2(this.Thumbnail.Size.Width, this.Thumbnail.Size.Height);

            /// <summary>
            /// Indicates if the text is being drawn
            /// </summary>
            public bool DrawText { get; set; }

            /// <summary>
            /// Indicates if the thumbnail is being drawn
            /// </summary>
            public bool DrawThumbnail { get; set; }

            /// <summary>
            /// The draw depth
            /// </summary>
            public double DrawDepth { get; set; }

            /// <summary>
            /// Creates a new overlay object
            /// </summary>
            /// <param name="renderingManager2D">The rendering manager 2D</param>
            /// <param name="renderingObject">The rendering object</param>
            public OverlayObject(RenderingManager2D renderingManager2D, RenderingObject renderingObject)
            {
                this.RenderingObject = renderingObject;

                using (var textLayout = new TextLayout(
                       renderingManager2D.FontFactory,
                       this.Text,
                       renderingManager2D.DefaultTextFormat,
                       renderingManager2D.ScreenRectangle.Width,
                       renderingManager2D.ScreenRectangle.Height))
                {
                    this.TextSize = new Size2(
                        (int)Math.Round(textLayout.Metrics.Width),
                        (int)Math.Round(textLayout.Metrics.Height));
                }
            }

            public void Dispose()
            {
                this.Thumbnail?.Dispose();
            }

            public int CompareTo(OverlayObject other)
            {
                return other.DrawDepth.CompareTo(this.DrawDepth);
            }

            public override string ToString()
            {
                return $"{this.RenderingObject.PhysicsObject.Name}: {this.DrawDepth}";
            }
        }

        /// <summary>
        /// Create a new overlay UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="mouseManager">The mouse manager</param>
        /// <param name="cameraManager">The camera manager</param>
        /// <param name="uiManager">The UI manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        /// <param name="orbitCamera">The camera</param>
        /// <param name="thumbnailEffect">The thumbnail effect</param>
        /// <param name="renderToTexture">Renders to texture</param>
        public OverlayUI(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            CameraManager cameraManager,
            UIManager uiManager,
            SimulatorContainer simulatorContainer,
            OrbitCamera orbitCamera,
            BasicEffect thumbnailEffect,
            RenderToTexture renderToTexture)
            : base(renderingManager2D, keyboardManager, mouseManager, simulatorContainer)
        {
            this.cameraManager = cameraManager;
            this.uiManager = uiManager;

            this.orbitCamera = orbitCamera;
            this.thumbnailEffect = thumbnailEffect;
            this.renderToTexture = renderToTexture;

            GeometryGenerator.CreateSphere(1.0f, 12, 10, out var vertices, out var indices);
            foreach (var vertex in vertices)
            {
                this.spherePoints.Add(MathHelpers.ToDouble(vertex.Position));
            }

            foreach (var currentObject in simulatorContainer.RenderingObjects)
            {
                this.overlayObjects.Add(new OverlayObject(renderingManager2D, currentObject));
            }
        }

        /// <summary>
        /// Creates a thumbnail image for the given object
        /// </summary>
        /// <param name="renderingObject">The rendering object</param>
        private RenderingImage2D CreateThumbnailImage(RenderingObject renderingObject)
        {
            return this.renderToTexture(
                Color.Transparent,
                render: deviceContext =>
                {
                    var overlayCamera = new OrbitCamera(this.orbitCamera);
                    var radius = 1E3;
                    if (renderingObject.PhysicsObject is NaturalSatelliteObject naturalSatelliteObject)
                    {
                        radius = naturalSatelliteObject.Radius;
                        overlayCamera.SetScaleFactor(naturalSatelliteObject);
                    }

                    overlayCamera.Radius = overlayCamera.ToDraw(radius * 3.0);
                    overlayCamera.Phi += MathUtil.DegreesToRadians(60.0f);
                    overlayCamera.UpdateViewMatrix();

                    deviceContext.InputAssembler.InputLayout = this.thumbnailEffect.InputLayout;
                    foreach (var pass in this.thumbnailEffect.Passes)
                    {
                        renderingObject.DrawSphere(
                            deviceContext,
                            this.thumbnailEffect,
                            pass,
                            overlayCamera,
                            position: Vector3.Zero);
                    }
                });
        }

        /// <summary>
        /// Creates the thumbnails
        /// </summary>
        private void CreateThumbnails()
        {
            foreach (var overlayObject in this.overlayObjects)
            {
                overlayObject.Thumbnail = this.CreateThumbnailImage(overlayObject.RenderingObject);
            }
        }

        /// <summary>
        /// Indicates if the orbit camera is the active one
        /// </summary>
        private bool OrbitCameraIsActive => this.orbitCamera == this.cameraManager.ActiveCamera;

        /// <summary>
        /// Selects an object at the given position
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        private void SelectObject(Vector2 mousePosition)
        {
            var selectedUIElement = this.uiManager.SelectElement(mousePosition);
            if (selectedUIElement != null)
            {
                return;
            }

            foreach (var overlayObject in this.overlayObjects)
            {
                var selected = false;
                var screenPosition = this.orbitCamera.Project(overlayObject.RenderingObject.DrawPosition(this.orbitCamera));
                var screenMouseDistance = Vector2.Distance(screenPosition, mousePosition);

                if (overlayObject.DrawThumbnail)
                {
                    selected = screenMouseDistance <= 12.5;
                }
                else
                {
                    if (overlayObject.RenderingObject.PhysicsObject is NaturalSatelliteObject naturalSatelliteObject)
                    {
                        this.GetRenderedSize(naturalSatelliteObject, out var minPosition, out var maxPosition, out var renderedRadius);
                        selected = screenMouseDistance <= renderedRadius;
                    }
                }

                if (selected)
                {
                    this.SimulatorContainer.SelectedObject = overlayObject.RenderingObject.PhysicsObject;
                    break;
                }
            }
        }

        public override void Update(TimeSpan elapsed)
        {
            if (this.OrbitCameraIsActive)
            {
                if (this.MouseManager.IsDoubleClick(MouseButtons.Left))
                {
                    this.SelectObject(this.MouseManager.MousePosition);
                }
            }
        }

        /// <summary>
        /// Returns the rendered size of the given position
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="minPosition">The min position</param>
        /// <param name="maxPosition">The max position</param>
        /// <param name="renderedRadius">The rendered radius</param>
        private void GetRenderedSize(NaturalSatelliteObject physicsObject, out Vector2 minPosition, out Vector2 maxPosition, out float renderedRadius)
        {
            var spherePositions = this.spherePoints.Select(x => physicsObject.Position + physicsObject.Radius * x);

            var screenPositions = spherePositions.Select(x => this.orbitCamera.Project(this.orbitCamera.ToDrawPosition(x))).ToList();
            minPosition = new Vector2(float.MaxValue);
            maxPosition = new Vector2(float.MinValue);

            var center = Vector2.Zero;
            foreach (var position in screenPositions)
            {
                minPosition = Vector2.Min(minPosition, position);
                maxPosition = Vector2.Max(maxPosition, position);
                center += position;
            }

            center /= screenPositions.Count;
            renderedRadius = 0.0f;
            foreach (var position in screenPositions)
            {
                renderedRadius = Math.Max(renderedRadius, (center - position).Length());
            }
        }

        /// <summary>
        /// Determines the visibility of the given overlay object
        /// </summary>
        /// <param name="overlayObject">The overlay object</param>
        private void DetermineOverlayVisiblity(OverlayObject overlayObject)
        {
            var inview = overlayObject.DrawDepth <= 1.0 && overlayObject.DrawDepth >= 0.0;
            overlayObject.DrawText = inview;
            overlayObject.DrawThumbnail = inview;
            var physicsObject = overlayObject.RenderingObject.PhysicsObject;

            if (physicsObject is NaturalSatelliteObject naturalSatelliteObject)
            {
                this.GetRenderedSize(naturalSatelliteObject, out var minPosition, out var maxPosition, out var renderedRadius);
                var screenWidth = maxPosition.X - minPosition.X;
                var screenHeight = maxPosition.Y - minPosition.Y;

                overlayObject.DrawText = screenWidth * screenWidth <= 100.0f;
                var drawnSize = overlayObject.ThumbnailSize;
                overlayObject.DrawThumbnail = inview && screenWidth < drawnSize.X && screenHeight < drawnSize.Y;
                overlayObject.RenderingObject.ShowSphere = !overlayObject.DrawThumbnail;
            }

            if (!physicsObject.IsObjectOfReference)
            {
                var orbitScreenPositions = new List<Vector2>();
                void AddOrbitScreenPosition(double trueAnomaly)
                {
                    var orbitPosition = new Physics.OrbitPosition(physicsObject.ReferenceOrbit, trueAnomaly).CalculateState().Position;
                    orbitScreenPositions.Add(this.orbitCamera.Project(this.orbitCamera.ToDrawPosition(orbitPosition)));
                }

                AddOrbitScreenPosition(0.0);
                AddOrbitScreenPosition(MathUtild.Deg2Rad * 90.0);
                AddOrbitScreenPosition(MathUtild.Deg2Rad * 180.0);
                AddOrbitScreenPosition(MathUtild.Deg2Rad * 270.0);

                var renderedDistance = 0.0;
                foreach (var point1 in orbitScreenPositions)
                {
                    foreach (var point2 in orbitScreenPositions)
                    {
                        renderedDistance = Math.Max(renderedDistance, Vector2.Distance(point1, point2));
                    }
                }

                overlayObject.DrawText = inview && renderedDistance >= 24.0f;
                overlayObject.RenderingObject.ShowOrbit = overlayObject.DrawText;

                if (overlayObject.DrawThumbnail)
                {
                    overlayObject.DrawThumbnail = overlayObject.DrawText;
                }
            }
        }

        public override void BeforeFirstDraw(DeviceContext deviceContext)
        {
            this.CreateThumbnails();
        }

        /// <summary>
        /// Determiens the visibility and draw order
        /// </summary>
        private void DetermineVisibilityAndOrder()
        {
            foreach (var overlayObject in this.overlayObjects)
            {
                var screenPosition = this.orbitCamera.Project(overlayObject.RenderingObject.DrawPosition(this.orbitCamera), out var depth);
                overlayObject.DrawDepth = depth;
            }

            this.overlayObjects.Sort();

            foreach (var overlayObject in this.overlayObjects)
            {
                this.DetermineOverlayVisiblity(overlayObject);
            }
        }

        /// <summary>
        /// Draws the overlay
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        private void DrawOverlay(DeviceContext deviceContext)
        {
            this.DetermineVisibilityAndOrder();

            foreach (var overlayObject in this.overlayObjects)
            {
                var physicsObject = overlayObject.RenderingObject.PhysicsObject;
                var screenPosition = this.orbitCamera.Project(overlayObject.RenderingObject.DrawPosition(this.orbitCamera), out var depth);

                if (overlayObject.DrawThumbnail && physicsObject.Type != PhysicsObjectType.ArtificialSatellite)
                {
                    overlayObject.Thumbnail.ApplyResource(bitmap =>
                    {
                        deviceContext.DrawBitmap(
                            bitmap,
                            1.0f,
                            SharpDX.Direct2D1.InterpolationMode.MultiSampleLinear,
                            new RectangleF(0, 0, overlayObject.Thumbnail.Size.Width, overlayObject.Thumbnail.Size.Height),
                            Matrix.Scaling(overlayObject.ThumbnailScale)
                            * Matrix.Translation(
                                screenPosition.X - overlayObject.ThumbnailSize.X / 2.0f,
                                screenPosition.Y - overlayObject.ThumbnailSize.Y / 2.0f, 0));
                    });
                }

                if (overlayObject.DrawText)
                {
                    this.RenderingManager2D.DefaultSolidColorBrush.DrawText(
                        deviceContext,
                        overlayObject.Text,
                        this.RenderingManager2D.DefaultTextFormat,
                        this.RenderingManager2D.TextPosition(
                            screenPosition - new Vector2(
                                overlayObject.TextSize.Width / 2.0f,
                                overlayObject.TextSize.Height + overlayObject.ThumbnailSize.Y / 2.0f)));
                }
            }

            //var tmpOverlayObject = this.overlayObjects.FirstOrDefault(x => x.RenderingObject.PhysicsObject.Name == "Earth");
            //tmpOverlayObject.Thumbnail.ApplyResource(bitmap =>
            //{
            //    deviceContext.DrawBitmap(
            //        bitmap,
            //        1.0f,
            //        SharpDX.Direct2D1.InterpolationMode.MultiSampleLinear,
            //        new RectangleF(0, 0, tmpOverlayObject.Thumbnail.Size.Width, tmpOverlayObject.Thumbnail.Size.Height),
            //        Matrix.Scaling(0.25f));
            //});
        }

        public override void Draw(DeviceContext deviceContext)
        {
            if (this.OrbitCameraIsActive)
            {
                this.DrawOverlay(deviceContext);
            }
            else
            {
                foreach (var overlayObject in this.overlayObjects)
                {
                    overlayObject.RenderingObject.ShowOrbit = true;
                    overlayObject.RenderingObject.ShowSphere = true;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var overlayObject in this.overlayObjects)
            {
                overlayObject.Dispose();
            }
        }
    }
}
