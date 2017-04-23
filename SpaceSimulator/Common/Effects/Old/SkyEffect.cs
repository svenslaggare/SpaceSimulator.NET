using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Effects;
using Device = SharpDX.Direct3D11.Device;

namespace SpaceSimulator.Common.Old
{
	/// <summary>
	/// Represents a sky effect
	/// </summary>
	public class SkyEffect : IDisposable
	{
		private readonly Device graphicsDevice;

		private readonly CompilationResult effectByteCode;
		private readonly Effect effect;
		private readonly EffectTechnique technique;
		private readonly ShaderBytecode shaderBytecode;

		private readonly EffectMatrixVariable worldViewProjVariable;
		private readonly EffectShaderResourceVariable cubeMapVariable;

		/// <summary>
		/// Creates a new sky effect
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="effectName">The name of the effect file</param>
		/// <param name="techniqueName">The name of the technique to use</param>
		public SkyEffect(Device graphicsDevice, string effectName, string techniqueName)
		{
			this.graphicsDevice = graphicsDevice;

			//Compile the effect
			this.effectByteCode = ShaderBytecode.CompileFromFile(
				effectName,
				"fx_5_0",
				ShaderFlags.None,
				EffectFlags.None,
				include: new IncludeEffect());

			this.effect = new Effect(this.graphicsDevice, effectByteCode);
			this.technique = effect.GetTechniqueByName(techniqueName);
			var pass = technique.GetPassByIndex(0);
			this.shaderBytecode = pass.Description.Signature;

			//Get the variables
			this.worldViewProjVariable = this.effect.GetVariableByName("gWorldViewProj").AsMatrix();
			this.cubeMapVariable = this.effect.GetVariableByName("gCubeMap").AsShaderResource();
		}

		/// <summary>
		/// Returns the technique
		/// </summary>
		public EffectTechnique Technique
		{
			get { return this.technique; }
		}

		/// <summary>
		/// Returns the shader bytecode
		/// </summary>
		public ShaderBytecode ShaderBytecode
		{
			get { return this.shaderBytecode; }
		}

		/// <summary>
		/// Sets the world-view-projection matrix
		/// </summary>
		/// <param name="worldViewProjection">The world-view-projection matrix</param>
		public void SetWorldViewProjection(Matrix worldViewProjection)
		{
			this.worldViewProjVariable.SetMatrix(worldViewProjection);
		}

		/// <summary>
		/// Sets the cube map
		/// </summary>
		/// <param name="cubeMap">The diffuse map</param>
		public void SetCubeMap(ShaderResourceView cubeMap)
		{
			this.cubeMapVariable.SetResource(cubeMap);
		}

		/// <summary>
		/// Disposes the resources
		/// </summary>
		public void Dispose()
		{
			this.effectByteCode.Dispose();
			this.effect.Dispose();
			this.shaderBytecode.Dispose();
			this.technique.Dispose();

			this.worldViewProjVariable.Dispose();
			this.cubeMapVariable.Dispose();
		}
	}
}
