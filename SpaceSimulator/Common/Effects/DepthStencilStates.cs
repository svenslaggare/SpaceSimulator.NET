using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Contains depth stencil states
	/// </summary>
	public class DepthStencilStates : IDisposable
	{
		/// <summary>
		/// Represents a mark mirror depth stencil state
		/// </summary>
		public DepthStencilState MarkMirror { get; private set; }

		/// <summary>
		/// Represents a draw reflection stencil state
		/// </summary>
		public DepthStencilState DrawReflection { get; private set; }

		/// <summary>
		/// Represents a no double blend depth stencil state
		/// </summary>
		public DepthStencilState NoDoubleBlend { get; private set; }

		/// <summary>
		/// Represents a less equal depth stencil state
		/// </summary>
		public DepthStencilState LessEqual { get; private set; }

		/// <summary>
		/// Creates new depth stencil states
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		public DepthStencilStates(Device graphicsDevice)
		{
			this.MarkMirror = new DepthStencilState(graphicsDevice, new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.Zero,
				DepthComparison = Comparison.Less,
				IsStencilEnabled = true,
				StencilReadMask = 0xFF,
				StencilWriteMask = 0xFF,
				FrontFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Replace,
					Comparison = Comparison.Always
				},
				BackFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Replace,
					Comparison = Comparison.Always
				}
			});

			this.DrawReflection = new DepthStencilState(graphicsDevice, new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.All,
				DepthComparison = Comparison.Less,
				IsStencilEnabled = true,
				StencilReadMask = 0xFF,
				StencilWriteMask = 0xFF,
				FrontFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Keep,
					Comparison = Comparison.Equal
				},
				BackFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Keep,
					Comparison = Comparison.Equal
				}
			});

			this.NoDoubleBlend = new DepthStencilState(graphicsDevice, new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.All,
				DepthComparison = Comparison.Less,
				IsStencilEnabled = true,
				StencilReadMask = 0xFF,
				StencilWriteMask = 0xFF,
				FrontFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Increment,
					Comparison = Comparison.Equal
				},
				BackFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Keep,
					PassOperation = StencilOperation.Increment,
					Comparison = Comparison.Equal
				}
			});

			this.LessEqual = new DepthStencilState(graphicsDevice, new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthWriteMask = DepthWriteMask.All,
				DepthComparison = Comparison.LessEqual,
				IsStencilEnabled = false
			});
		}

		/// <summary>
		/// Disposes the resources
		/// </summary>
		public void Dispose()
		{
			this.DrawReflection.Dispose();
			this.MarkMirror.Dispose();
			this.NoDoubleBlend.Dispose();
			this.LessEqual.Dispose();
		}
	}
}
