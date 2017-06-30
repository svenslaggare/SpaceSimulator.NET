using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SpaceSimulator.Common.Rendering2D;

namespace SpaceSimulator.Common
{
    /// <summary>
    /// Handles rendering the scene to a texture
    /// </summary>
    public sealed class RenderToTexture : IDisposable
    {
        private readonly SharpDX.Direct3D11.Device graphicsDevice;

        private readonly Texture2D texture;
        private readonly RenderTargetView textureRenderTargetView;

        private readonly Texture2D depthBuffer;
        private readonly DepthStencilView depthStencilView;

        private readonly Texture2D multisampleCopyTexture;
        private readonly Texture2D copyTexture;

        /// <summary>
        /// Creates a new render to texture
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="backBufferDescription">The description for the back buffer</param>
        /// <param name="backBufferDepthDescription">The description for the depth of the back buffer</param>
        public RenderToTexture(SharpDX.Direct3D11.Device graphicsDevice, Texture2DDescription backBufferDescription, Texture2DDescription backBufferDepthDescription)
        {
            this.graphicsDevice = graphicsDevice;

            var textureDescription = backBufferDescription;
            textureDescription.BindFlags = BindFlags.RenderTarget;
            this.texture = new Texture2D(this.graphicsDevice, textureDescription);
            this.textureRenderTargetView = new RenderTargetView(this.graphicsDevice, this.texture);

            this.depthBuffer = new Texture2D(this.graphicsDevice, backBufferDepthDescription);
            this.depthStencilView = new DepthStencilView(this.graphicsDevice, this.depthBuffer);

            var multisampleCopyTextureDescription = textureDescription;
            multisampleCopyTextureDescription.BindFlags = BindFlags.None;
            multisampleCopyTextureDescription.Usage = ResourceUsage.Default;
            multisampleCopyTextureDescription.SampleDescription = new SampleDescription(1, 0);
            this.multisampleCopyTexture = new Texture2D(this.graphicsDevice, multisampleCopyTextureDescription);

            var copyTextureDescription = textureDescription;
            copyTextureDescription.CpuAccessFlags = CpuAccessFlags.Read;
            copyTextureDescription.BindFlags = BindFlags.None;
            copyTextureDescription.Usage = ResourceUsage.Staging;
            copyTextureDescription.SampleDescription = new SampleDescription(1, 0);
            this.copyTexture = new Texture2D(this.graphicsDevice, copyTextureDescription);
        }

        /// <summary>
        /// Renders to texture
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="backBufferRenderView">The render view for the back buffer</param>
        /// <param name="backBufferDepthView">The depth view for the back buffer</param>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="render">The render function</param>
        public RenderingImage2D Render(
            SharpDX.Direct3D11.DeviceContext deviceContext,
            RenderTargetView backBufferRenderView,
            DepthStencilView backBufferDepthView,
            Color backgroundColor,
            Action<SharpDX.Direct3D11.DeviceContext> render)
        {
            //Render to texture
            //deviceContext.ClearRenderTargetView(this.textureRenderTargetView, backgroundColor);
            deviceContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.OutputMerger.SetRenderTargets(this.depthStencilView, this.textureRenderTargetView);
            deviceContext.OutputMerger.SetRenderTargets(this.textureRenderTargetView);
            render(deviceContext);
            deviceContext.OutputMerger.SetRenderTargets(backBufferDepthView, backBufferRenderView);

            //Copy to texture
            //deviceContext.CopyResource(this.texture, this.copyTexture);
            deviceContext.ResolveSubresource(this.texture, 0, this.multisampleCopyTexture, 0, Format.B8G8R8A8_UNorm);
            deviceContext.CopyResource(this.multisampleCopyTexture, this.copyTexture);

            var dataBox = deviceContext.MapSubresource(
                this.copyTexture,
                0,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out var dataStream);

            var copyDataStream = new DataStream(
                this.texture.Description.Width * this.texture.Description.Height * sizeof(int),
                true,
                true);
            dataStream.CopyTo(copyDataStream);
            dataStream.Seek(0, System.IO.SeekOrigin.Begin);
            deviceContext.UnmapSubresource(copyTexture, 0);

            //var bitmap = new Bitmap(
            //    deviceContext2D,
            //    new Size2(this.texture.Description.Width, this.texture.Description.Height),
            //    dataStream,
            //    this.texture.Description.Width * sizeof(int),
            //    new BitmapProperties(new PixelFormat(this.texture.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied)));

            return new RenderingImage2D(
                new BitmapProperties(new PixelFormat(this.texture.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied)),
                new Size2(this.texture.Description.Width, this.texture.Description.Height),
                copyDataStream);
        }

        public void Dispose()
        {
            this.texture.Dispose();
            this.textureRenderTargetView.Dispose();
            this.depthBuffer.Dispose();
            this.depthStencilView.Dispose();
            this.copyTexture.Dispose();
            this.multisampleCopyTexture.Dispose();
        }
    }
}
