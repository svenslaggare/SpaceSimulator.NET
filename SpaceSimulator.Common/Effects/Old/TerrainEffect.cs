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
	/// Represents a terrain effect
	/// </summary>
	public class TerrainEffect : IDisposable
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

		private readonly EffectScalarVariable minTessDistanceVariable;
		private readonly EffectScalarVariable maxTessDistanceVariable;
		private readonly EffectScalarVariable minTessFactorVariable;
		private readonly EffectScalarVariable maxTessFactorVariable;

		private readonly EffectScalarVariable texelCellSpaceUVariable;
		private readonly EffectScalarVariable texelCellSpaceVVariable;
		private readonly EffectScalarVariable worldCellSpaceVariable;
		private readonly EffectVectorVariable worldFrustumPlanesVariable;

		private readonly EffectVariable dirLightsVariable;
		private readonly EffectVariable materialVariable;

		private readonly EffectShaderResourceVariable layerMapArrayMapVariable;
		private readonly EffectShaderResourceVariable blendMapVariable;
		private readonly EffectShaderResourceVariable heightMapVariable;

		/// <summary>
		/// Creates a new terrain effect
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="effectName">The name of the effect file</param>
		/// <param name="techniqueName">The name of the technique to use</param>
		public TerrainEffect(Device graphicsDevice, string effectName, string techniqueName)
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

			this.minTessDistanceVariable = this.effect.GetVariableByName("gMinDist").AsScalar();
			this.maxTessDistanceVariable = this.effect.GetVariableByName("gMaxDist").AsScalar();
			this.minTessFactorVariable = this.effect.GetVariableByName("gMinTess").AsScalar();
			this.maxTessFactorVariable = this.effect.GetVariableByName("gMaxTess").AsScalar();
			this.texelCellSpaceUVariable = this.effect.GetVariableByName("gTexelCellSpaceU").AsScalar();
			this.texelCellSpaceVVariable = this.effect.GetVariableByName("gTexelCellSpaceV").AsScalar();
			this.worldCellSpaceVariable = this.effect.GetVariableByName("gWorldCellSpace").AsScalar();
			this.worldFrustumPlanesVariable = this.effect.GetVariableByName("gWorldFrustumPlanes").AsVector();

			this.dirLightsVariable = this.effect.GetVariableByName("gDirLights");
			this.materialVariable = this.effect.GetVariableByName("gMaterial");

			this.layerMapArrayMapVariable = this.effect.GetVariableByName("gLayerMapArray").AsShaderResource();
			this.blendMapVariable = this.effect.GetVariableByName("gBlendMap").AsShaderResource();
			this.heightMapVariable = this.effect.GetVariableByName("gHeightMap").AsShaderResource();
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
		/// Sets the minimum tess distance
		/// </summary>
		/// <param name="minTessDistance">The minimum tess distance</param>
		public void SetMinTessDistance(float minTessDistance)
		{
			this.minTessDistanceVariable.Set(minTessDistance);
		}

		/// <summary>
		/// Sets the maxium tess distance
		/// </summary>
		/// <param name="maxTessDistance">The maximum tess distance</param>
		public void SetMaxTessDistance(float maxTessDistance)
		{
			this.maxTessDistanceVariable.Set(maxTessDistance);
		}

		/// <summary>
		/// Sets the minimum tess factor
		/// </summary>
		/// <param name="factor">The factor</param>
		public void SetMinTessFactor(float factor)
		{
			this.minTessFactorVariable.Set(factor);
		}

		/// <summary>
		/// Sets the maximum tess factor
		/// </summary>
		/// <param name="factor">The factor</param>
		public void SetMaxTessFactor(float factor)
		{
			this.maxTessFactorVariable.Set(factor);
		}

		/// <summary>
		/// Sets the texel cell space U
		/// </summary>
		/// <param name="texelCellSpaceU">The texel cell space U</param>
		public void SetTexelCellSpaceU(float texelCellSpaceU)
		{
			this.texelCellSpaceUVariable.Set(texelCellSpaceU);
		}

		/// <summary>
		/// Sets the texel cell space V
		/// </summary>
		/// <param name="texelCellSpaceV">The texel cell space V</param>
		public void SetTexelCellSpaceV(float texelCellSpaceV)
		{
			this.texelCellSpaceVVariable.Set(texelCellSpaceV);
		}

		/// <summary>
		/// Sets the world cepp sace
		/// </summary>
		/// <param name="worldCellSpace">The world cell space</param>
		public void SetWorldCellSpace(float worldCellSpace)
		{
			this.worldCellSpaceVariable.Set(worldCellSpace);
		}

		/// <summary>
		/// Sets the world frustum planes
		/// </summary>
		/// <param name="worldFrustumPlanes">The world frustum planes</param>
		public void SetWorldFrustumPlanes(Vector4[] worldFrustumPlanes)
		{
			this.worldFrustumPlanesVariable.Set(worldFrustumPlanes);
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
        /// Sets the height map
        /// </summary>
        /// <param name="heightMap">The height map</param>
        public void SetHeightMap(ShaderResourceView heightMap)
		{
			this.heightMapVariable.SetResource(heightMap);
		}

		/// <summary>
		/// Sets the layer map array
		/// </summary>
		/// <param name="layerMapArray">The layer map map</param>
		public void SetLayerMapArray(ShaderResourceView layerMapArray)
		{
			this.layerMapArrayMapVariable.SetResource(layerMapArray);
		}

        /// <summary>
        /// Sets the blend map
        /// </summary>
        /// <param name="blendMap">The blend map</param>
        public void SetBlendMap(ShaderResourceView blendMap)
		{
			this.blendMapVariable.SetResource(blendMap);
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
		/// Returns an effect variable of the given name
		/// </summary>
		/// <param name="name">The name of the variable</param>
		public EffectVariable GetVariable(string name)
		{
			return this.effect.GetVariableByName(name);
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

			this.viewProjVariable.Dispose();
			this.eyePositionVariable.Dispose();

			this.fogStart.Dispose();
			this.fogColor.Dispose();
			this.fogRange.Dispose();

			this.minTessDistanceVariable.Dispose();
			this.maxTessDistanceVariable.Dispose();
			this.minTessFactorVariable.Dispose();
			this.maxTessFactorVariable.Dispose();
			this.texelCellSpaceUVariable.Dispose();
			this.texelCellSpaceVVariable.Dispose();
			this.worldCellSpaceVariable.Dispose();
			this.worldFrustumPlanesVariable.Dispose();

			this.dirLightsVariable.Dispose();
			this.materialVariable.Dispose();

			this.layerMapArrayMapVariable.Dispose();
			this.blendMapVariable.Dispose();
			this.heightMapVariable.Dispose();
		}
	}
}
