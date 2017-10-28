using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Contains rasterizer states
	/// </summary>
	public class RasterizerStates : IDisposable
	{
		/// <summary>
		/// Represents a wireframe rasterizer state
		/// </summary>
		public RasterizerState Wireframe { get; private set; }

		/// <summary>
		/// Represents a no cull rasterizer state
		/// </summary>
		public RasterizerState NoCull { get; private set; }

		/// <summary>
		/// Represents a cull clockwise rasterizer state
		/// </summary>
		public RasterizerState CullClockwise { get; private set; }

		/// <summary>
		/// Creates new rasterizer states
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		public RasterizerStates(Device graphicsDevice)
		{
			this.Wireframe = new RasterizerState(graphicsDevice, new RasterizerStateDescription()
			{
				FillMode = FillMode.Wireframe,
				CullMode = CullMode.Back,
				IsFrontCounterClockwise = false,
				IsDepthClipEnabled = true
			});

			this.NoCull = new RasterizerState(graphicsDevice, new RasterizerStateDescription()
			{
				FillMode = FillMode.Solid,
				CullMode = CullMode.None,
				IsFrontCounterClockwise = false,
				IsDepthClipEnabled = true
			});

			this.CullClockwise = new RasterizerState(graphicsDevice, new RasterizerStateDescription()
			{
				FillMode = FillMode.Solid,
				CullMode = CullMode.Back,
				IsFrontCounterClockwise = true,
				IsDepthClipEnabled = true
			});
		}

		/// <summary>
		/// Disposes resources
		/// </summary>
		public void Dispose()
		{
			this.CullClockwise.Dispose();
			this.NoCull.Dispose();
			this.Wireframe.Dispose();
		}
	}
}
