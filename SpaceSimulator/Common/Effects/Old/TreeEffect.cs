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
	/// Represents a tree effect
	/// </summary>
	public class TreeEffect : IDisposable
	{
		private readonly Device graphicsDevice;

		private readonly CompilationResult effectByteCode;
		private readonly Effect effect;
		private readonly EffectTechnique technique;
		private readonly ShaderBytecode shaderBytecode;

		private readonly EffectMatrixVariable viewProjVariable;
		private readonly EffectVectorVariable eyePositionVariable;

		private readonly EffectVectorVariable fogColor;
		private readonly EffectScalarVariable fogStart;
		private readonly EffectScalarVariable fogRange;

		private readonly EffectVariable dirLightsVariable;
		private readonly EffectVariable materialVariable;
		private readonly EffectShaderResourceVariable treeMapArray;

		/// <summary>
		/// Creates a new tree effect
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="effectName">The name of the effect file</param>
		/// <param name="techniqueName">The name of the technique to use</param>
		public TreeEffect(Device graphicsDevice, string effectName, string techniqueName)
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
			this.viewProjVariable = this.effect.GetVariableByName("gViewProj").AsMatrix();
			this.eyePositionVariable = this.effect.GetVariableByName("gEyePosW").AsVector();

			this.fogColor = this.effect.GetVariableByName("gFogColor").AsVector();
			this.fogStart = this.effect.GetVariableByName("gFogStart").AsScalar();
			this.fogRange = this.effect.GetVariableByName("gFogRange").AsScalar();

			this.dirLightsVariable = this.effect.GetVariableByName("gDirLights");
			this.materialVariable = this.effect.GetVariableByName("gMaterial");
			this.treeMapArray = this.effect.GetVariableByName("gTreeMapArray").AsShaderResource();
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
		/// Sets the view-projection matrix
		/// </summary>
		/// <param name="viewProjection">The view-projection matrix</param>
		public void SetViewProjection(Matrix viewProjection)
		{
			this.viewProjVariable.SetMatrix(viewProjection);
		}

		/// <summary>
		/// Sets the eye position
		/// </summary>
		/// <param name="position">The eye position</param>
		public void SetEyePosition(Vector3 position)
		{
			this.eyePositionVariable.Set(position);
		}

		/// <summary>
		/// Sets the fog color
		/// </summary>
		/// <param name="color">The color</param>
		public void SetFogColor(Vector4 color)
		{
			this.fogColor.Set(color);
		}

		/// <summary>
		/// Sets the fog start
		/// </summary>
		/// <param name="start">The start</param>
		public void SetFogStart(float start)
		{
			this.fogStart.Set(start);
		}

		/// <summary>
		/// Sets the fog range
		/// </summary>
		/// <param name="range">The range</param>
		public void SetFogRange(float range)
		{
			this.fogRange.Set(range);
		}

		/// <summary>
		/// Sets the material
		/// </summary>
		/// <param name="material">The material</param>
		public void SetMaterial(Material material)
		{
			this.materialVariable.SetStruct(material);
		}
	
		/// <summary>
		/// Sets the directional lights
		/// </summary>
		/// <param name="lights">The directional lights</param>
		public void SetDirectionalLights(DirectionalLight[] lights)
		{
			this.dirLightsVariable.SetStructArray(lights);
		}

		/// <summary>
		/// Sets the tree map array
		/// </summary>
		/// <param name="treeMapArray">The tree map array</param>
		public void SetTreeMapArray(ShaderResourceView treeMapArray)
		{
			this.treeMapArray.SetResource(treeMapArray);
		}

		/// <summary>
		/// Returns the given technique
		/// </summary>
		/// <param name="name">The name of the technique</param>
		public EffectTechnique GetTechnique(string name)
		{
			return this.effect.GetTechniqueByName(name);
		}

		/// <summary>
		/// Disposes the resources
		/// </summary>
		public void Dispose()
		{
			this.effectByteCode.Dispose();
			this.effect.Dispose();
			this.shaderBytecode.Dispose();

			this.viewProjVariable.Dispose();
			this.eyePositionVariable.Dispose();

			this.fogStart.Dispose();
			this.fogColor.Dispose();
			this.fogRange.Dispose();

			this.dirLightsVariable.Dispose();
			this.materialVariable.Dispose();
			this.treeMapArray.Dispose();
		}
	}
}
