using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Common
{
    /// <summary>
    /// The render pass types
    /// </summary>
    public enum RenderingPassType
    {
        Render2D,
        Render3D
    }

    /// <summary>
    /// Represents a function for a render pass
    /// </summary>
    /// <param name="deviceContext">The 3D rendering context</param>
    /// <param name="deviceContext2D">The 2D rendering context</param>
    public delegate void RenderPassFunction(SharpDX.Direct3D11.DeviceContext deviceContext, SharpDX.Direct2D1.DeviceContext deviceContext2D);

    /// <summary>
    /// Represents a rendering pass
    /// </summary>
    public sealed class RenderingPass
    {
        /// <summary>
        /// The type of the rendering pass
        /// </summary>
        public RenderingPassType Type { get; }

        private readonly RenderPassFunction render;

        /// <summary>
        /// Creates a new rendering pass
        /// </summary>
        /// <param name="type">The type of the pass</param>
        /// <param name="render">The function for the render pass</param>
        public RenderingPass(RenderingPassType type, RenderPassFunction render)
        {
            this.Type = type;
            this.render = render;
        }

        /// <summary>
        /// Renders the current pass
        /// </summary>
        /// <param name="deviceContext">The 3D rendering context</param>
        /// <param name="deviceContext2D">The 2D rendering context</param>
        public void Render(SharpDX.Direct3D11.DeviceContext deviceContext, SharpDX.Direct2D1.DeviceContext deviceContext2D)
        {
            if (this.Type == RenderingPassType.Render2D)
            {
                deviceContext2D.BeginDraw();
                this.render(deviceContext, deviceContext2D);
                deviceContext2D.EndDraw();
            }
            else
            {
                this.render(deviceContext, deviceContext2D);
            }
        }
    }

    /// <summary>
    /// Manages rendering passes
    /// </summary>
    public sealed class RenderingPasses
    {
        private readonly IList<RenderingPass> renderingPasses = new List<RenderingPass>();

        /// <summary>
        /// Adds a 2D render pass
        /// </summary>
        /// <param name="render">The function for the render pass</param>
        /// <returns>The created pass</returns>
        public RenderingPass Add2D(RenderPassFunction render)
        {
            var pass = new RenderingPass(RenderingPassType.Render2D, render);
            this.renderingPasses.Add(pass);
            return pass;
        }

        /// <summary>
        /// Adds a 3D render pass
        /// </summary>
        /// <param name="render">The function for the render pass</param>
        /// <returns>The created pass</returns>
        public RenderingPass Add3D(RenderPassFunction render)
        {
            var pass = new RenderingPass(RenderingPassType.Render3D, render);
            this.renderingPasses.Add(pass);
            return pass;
        }

        /// <summary>
        /// Renders all the passes
        /// </summary>
        /// <param name="deviceContext">The 3D rendering context</param>
        /// <param name="deviceContext2D">The 2D rendering context</param>
        public void Render(SharpDX.Direct3D11.DeviceContext deviceContext, SharpDX.Direct2D1.DeviceContext deviceContext2D)
        {
            foreach (var pass in this.renderingPasses)
            {
                pass.Render(deviceContext, deviceContext2D);
            }
        }
    }
}
