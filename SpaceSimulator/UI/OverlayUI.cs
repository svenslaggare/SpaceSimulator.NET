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
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Common.Models;
using SpaceSimulator.Mathematics;
using SpaceSimulator.Rendering;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing overlays
    /// </summary>
    public class OverlayUI : UIComponent
    {
        private readonly OrbitCamera camera;
        private readonly BasicEffect thumbnailEffect;
        private readonly D3DApp d3dApplication;

        private readonly IList<Vector3d> spherePoints = new List<Vector3d>();
        private readonly IList<OverlayObject> overlayObjects = new List<OverlayObject>();

        /// <summary>
        /// Object being overlayed
        /// </summary>
        private class OverlayObject : IDisposable
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
            /// Indicates if the thumbnail is being drawn
            /// </summary>
            public bool DrawThumbnail { get; set; }

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
        }

        /// <summary>
        /// Create a new overlay UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        /// <param name="camera">The camera</param>
        /// <param name="thumbnailEffect">The thumbnail effect</param>
        /// <param name="d3dApplication">The D3D application</param>
        public OverlayUI(
            RenderingManager2D renderingManager2D,
            KeyboardManager keyboardManager,
            SimulatorContainer simulatorContainer,
            OrbitCamera camera,
            BasicEffect thumbnailEffect,
            D3DApp d3dApplication)
            : base(renderingManager2D, keyboardManager, simulatorContainer)
        {
            this.camera = camera;
            this.thumbnailEffect = thumbnailEffect;
            this.d3dApplication = d3dApplication;

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
            return this.d3dApplication.RenderToTexture(
                Color.Transparent,
                render: deviceContext =>
                {
                    var radius = 1E3;
                    if (renderingObject.PhysicsObject is NaturalSatelliteObject naturalSatelliteObject)
                    {
                        radius = naturalSatelliteObject.Radius;
                    }

                    this.thumbnailEffect.SetEyePosition(this.camera.Position);
                    this.thumbnailEffect.SetPointLightSource(this.camera.ToDrawPosition(Vector3d.Zero));

                    var originalRadius = this.camera.Radius;
                    var originalPhi = this.camera.Phi;

                    this.camera.Radius = this.camera.ToDraw(radius * 3.0);
                    this.camera.Phi += MathUtil.DegreesToRadians(60.0f);
                    this.camera.UpdateViewMatrix();

                    deviceContext.InputAssembler.InputLayout = this.thumbnailEffect.InputLayout;
                    foreach (var pass in this.thumbnailEffect.Passes)
                    {
                        renderingObject.DrawSphere(
                            deviceContext,
                            this.thumbnailEffect,
                            this.camera,
                            pass,
                            position: Vector3.Zero);
                    }

                    this.camera.Radius = originalRadius;
                    this.camera.Phi = originalPhi;
                    this.camera.UpdateViewMatrix();
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

        public override void Update(TimeSpan elapsed)
        {

        }

        public override void OnMouseButtonDown(Vector2 mousePosition, MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                foreach (var overlayObject in this.overlayObjects)
                {
                    var selected = false;
                    var screenPosition = this.camera.Project(overlayObject.RenderingObject.DrawPosition);
                    var screenMouseDistance = Vector2.Distance(screenPosition, this.d3dApplication.MousePosition);

                    if (overlayObject.DrawThumbnail)
                    {
                        selected = screenMouseDistance <= 12.5;
                    }
                    else
                    {
                        if (overlayObject.RenderingObject.PhysicsObject is NaturalSatelliteObject naturalSatelliteObject)
                        {
                            this.GetRenderedMinAndMax(naturalSatelliteObject, out var minPosition, out var maxPosition, out var renderedRadius);
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
        }

        /// <summary>
        /// Returns the rendered min and max positions
        /// </summary>
        /// <param name="physicsObject">The physics object</param>
        /// <param name="minPosition">The min position</param>
        /// <param name="maxPosition">The max position</param>
        /// <param name="renderedRadius">The rendered radius</param>
        private void GetRenderedMinAndMax(NaturalSatelliteObject physicsObject, out Vector2 minPosition, out Vector2 maxPosition, out float renderedRadius)
        {
            var spherePositions = this.spherePoints.Select(x => physicsObject.Position + physicsObject.Radius * x);

            var screenPositions = spherePositions.Select(x => this.camera.Project(this.camera.ToDrawPosition(x))).ToList();
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

        public override void BeforeFirstDraw(DeviceContext deviceContext)
        {
            this.CreateThumbnails();
        }

        public override void Draw(DeviceContext deviceContext)
        {
            foreach (var overlayObject in this.overlayObjects)
            {
                var screenPosition = this.camera.Project(overlayObject.RenderingObject.DrawPosition);
                var drawText = true;
                var drawThumbnail = true;

                var scaling = 1.0f / 32.0f;
                //var scaling = 1.0f;
                var originalWidth = overlayObject.Thumbnail.Size.Width;
                var originalHeight = overlayObject.Thumbnail.Size.Height;
                var width = originalWidth * scaling;
                var height = originalHeight * scaling;

                //var center = screenPosition;
                //var radius = 12.0f;

                if (overlayObject.RenderingObject.PhysicsObject is NaturalSatelliteObject naturalSatelliteObject)
                {
                    this.GetRenderedMinAndMax(naturalSatelliteObject, out var minPosition, out var maxPosition, out var renderedRadius);
                    var screenWidth = maxPosition.X - minPosition.X;
                    var screenHeight = maxPosition.Y - minPosition.Y;

                    drawText = screenWidth * screenWidth <= 100.0f;
                    drawThumbnail = screenWidth < width && screenHeight < height;

                    //radius = (maxPosition - minPosition).Length() * 0.36f;
                    //radius = renderedRadius;
                }

                //this.RenderingManager2D.DefaultSolidColorBrush.Color = new Color(255, 0, 0, 128);
                //this.RenderingManager2D.DefaultSolidColorBrush.ApplyResource(brush =>
                //{
                //    deviceContext.FillEllipse(
                //        new Ellipse(center, radius, radius),
                //        brush);
                //});

                var physicsObject = overlayObject.RenderingObject.PhysicsObject;
                if (drawThumbnail 
                    && (physicsObject.PrimaryBody == this.SimulatorEngine.ObjectOfReference || physicsObject.IsObjectOfReference))
                {
                    overlayObject.Thumbnail.ApplyResource(bitmap =>
                    {
                        deviceContext.DrawBitmap(
                            bitmap,
                            1.0f,
                            SharpDX.Direct2D1.InterpolationMode.MultiSampleLinear,
                            new RectangleF(0, 0, originalWidth, originalHeight),
                            Matrix.Scaling(scaling) * Matrix.Translation(screenPosition.X - width / 2.0f, screenPosition.Y - height / 2.0f, 0));
                    });
                }

                overlayObject.DrawThumbnail = drawThumbnail;

                if (drawText)
                {
                    this.RenderingManager2D.DefaultSolidColorBrush.DrawText(
                        deviceContext,
                        overlayObject.Text,
                        this.RenderingManager2D.DefaultTextFormat,
                        this.RenderingManager2D.TextPosition(
                            screenPosition - new Vector2(
                                overlayObject.TextSize.Width / 2.0f,
                                overlayObject.TextSize.Height + height / 2.0f)));
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
