using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Contains blend states
	/// </summary>
	public class BlendStates : IDisposable
	{
		private readonly BlendState alphaToCoverage;
		private readonly BlendState transparent;
		private readonly BlendState noRenderTargetWrites;

		/// <summary>
		/// Creates new blend states
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		public BlendStates(Device graphicsDevice)
		{
			//Create the alpho to coverge blend state
			var alphaToCovergeDesc = new BlendStateDescription()
			{
				AlphaToCoverageEnable = true,
				IndependentBlendEnable = false,
			};

			alphaToCovergeDesc.RenderTarget[0] = new RenderTargetBlendDescription()
			{
				IsBlendEnabled = false,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};

			this.alphaToCoverage = new BlendState(graphicsDevice, alphaToCovergeDesc);

			//Create the transparent blend state
			var transparentDec = new BlendStateDescription()
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false,
			};

			transparentDec.RenderTarget[0] = new RenderTargetBlendDescription()
			{
				IsBlendEnabled = true,
				SourceBlend = BlendOption.SourceAlpha,
				DestinationBlend = BlendOption.InverseSourceAlpha,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};

			this.transparent = new BlendState(graphicsDevice, transparentDec);

			//Create the no render traget writes blend state
			var noRenderTargetWritesDesc = new BlendStateDescription()
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false
			};
			
			noRenderTargetWritesDesc.RenderTarget[0] = new RenderTargetBlendDescription()
			{
				IsBlendEnabled = false,
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add,
				RenderTargetWriteMask = 0
			};

			this.noRenderTargetWrites = new BlendState(graphicsDevice, noRenderTargetWritesDesc);
		}

		/// <summary>
		/// Returns the alpha to coverge blend state
		/// </summary>
		public BlendState AlphaToCoverge
		{
			get { return this.alphaToCoverage; }
		}

		/// <summary>
		/// Returns the transparent blend state
		/// </summary>
		public BlendState Transparent
		{
			get { return this.transparent; }
		}

		/// <summary>
		/// Returns the no render target writes blend state
		/// </summary>
		public BlendState NoRenderTargetWrites
		{
			get { return this.noRenderTargetWrites; }
		}


		/// <summary>
		/// Disposes the resources
		/// </summary>
		public void Dispose()
		{
			this.alphaToCoverage.Dispose();
			this.transparent.Dispose();
			this.noRenderTargetWrites.Dispose();
		}
	}
}
