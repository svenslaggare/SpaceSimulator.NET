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
	/// Represents a displacement map effect
	/// </summary>
	public class DisplacementMapEffect : IDisposable
	{
		private readonly Device graphicsDevice;

		private readonly CompilationResult effectByteCode;
		private readonly Effect effect;
		private readonly EffectTechnique technique;
		private readonly ShaderBytecode shaderBytecode;

		private readonly EffectMatrixVariable worldViewProjVariable;
		private readonly EffectMatrixVariable viewProjVariable;
		private readonly EffectMatrixVariable worldVariable;
		private readonly EffectMatrixVariable worldInvTransposeVariable;
		private readonly EffectVectorVariable eyePositionVariable;
		private readonly EffectMatrixVariable textureTransformVariable;

		private readonly EffectVectorVariable fogColorVariable;
		private readonly EffectScalarVariable fogStartVariable;
		private readonly EffectScalarVariable fogRangeVariable;

		private readonly EffectScalarVariable heightScaleVariable;
		private readonly EffectScalarVariable minTessDistanceVariable;
		private readonly EffectScalarVariable maxTessDistanceVariable;
		private readonly EffectScalarVariable minTessFactorVariable;
		private readonly EffectScalarVariable maxTessFactorVariable;

		private readonly EffectVariable dirLightsVariable;
		private readonly EffectVariable materialVariable;
		private readonly EffectShaderResourceVariable diffuseMapVariable;
		private readonly EffectShaderResourceVariable cubeMapMapVariable;
		private readonly EffectShaderResourceVariable normalMapMapVariable;

		/// <summary>
		/// Creates a new displacement map effect
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="effectName">The name of the effect file</param>
		/// <param name="techniqueName">The name of the technique to use</param>
		public DisplacementMapEffect(Device graphicsDevice, string effectName, string techniqueName)
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
			this.viewProjVariable = this.effect.GetVariableByName("gViewProj").AsMatrix();
			this.worldVariable = this.effect.GetVariableByName("gWorld").AsMatrix();
			this.worldInvTransposeVariable = this.effect.GetVariableByName("gWorldInvTranspose").AsMatrix();
			this.eyePositionVariable = this.effect.GetVariableByName("gEyePosW").AsVector();
			this.textureTransformVariable = this.effect.GetVariableByName("gTexTransform").AsMatrix();

			this.fogColorVariable = this.effect.GetVariableByName("gFogColor").AsVector();
			this.fogStartVariable = this.effect.GetVariableByName("gFogStart").AsScalar();
			this.fogRangeVariable = this.effect.GetVariableByName("gFogRange").AsScalar();

			this.heightScaleVariable = this.effect.GetVariableByName("gHeightScale").AsScalar();
			this.maxTessDistanceVariable = this.effect.GetVariableByName("gMaxTessDistance").AsScalar();
			this.minTessDistanceVariable = this.effect.GetVariableByName("gMinTessDistance").AsScalar();
			this.minTessFactorVariable = this.effect.GetVariableByName("gMinTessFactor").AsScalar();
			this.maxTessFactorVariable = this.effect.GetVariableByName("gMaxTessFactor").AsScalar();

			this.dirLightsVariable = this.effect.GetVariableByName("gDirLights");
			this.materialVariable = this.effect.GetVariableByName("gMaterial");
			this.diffuseMapVariable = this.effect.GetVariableByName("gDiffuseMap").AsShaderResource();
			this.cubeMapMapVariable = this.effect.GetVariableByName("gCubeMap").AsShaderResource();
			this.normalMapMapVariable = this.effect.GetVariableByName("gNormalMap").AsShaderResource();
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
		/// Sets the view-projection matrix
		/// </summary>
		/// <param name="viewProjection">The view-projection matrix</param>
		public void SetViewProjection(Matrix viewProjection)
		{
			this.viewProjVariable.SetMatrix(viewProjection);
		}

		/// <summary>
		/// Sets the world matrix
		/// </summary>
		/// <param name="world">The worldmatrix</param>
		public void SetWorld(Matrix world)
		{
			this.worldVariable.SetMatrix(world);
		}

		/// <summary>
		/// Sets the world-inverse-transpose matrix
		/// </summary>
		/// <param name="worldInvTranpose">The world-inverse-transpose matrix</param>
		public void SetWorldInvTranspose(Matrix worldInvTranpose)
		{
			this.worldInvTransposeVariable.SetMatrix(worldInvTranpose);
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
		/// Sets the texture transform
		/// </summary>
		/// <param name="transform">The transform</param>
		public void SetTextureTransform(Matrix transform)
		{
			this.textureTransformVariable.SetMatrix(transform);
		}

		/// <summary>
		/// Sets the fog color
		/// </summary>
		/// <param name="color">The color</param>
		public void SetFogColor(Vector4 color)
		{
			this.fogColorVariable.Set(color);
		}

		/// <summary>
		/// Sets the fog start
		/// </summary>
		/// <param name="start">The start</param>
		public void SetFogStart(float start)
		{
			this.fogStartVariable.Set(start);
		}

		/// <summary>
		/// Sets the fog range
		/// </summary>
		/// <param name="range">The range</param>
		public void SetFogRange(float range)
		{
			this.fogRangeVariable.Set(range);
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
		/// Sets the height scale
		/// </summary>
		/// <param name="heightScale">The height scale</param>
		public void SetHeightScale(float heightScale)
		{
			this.heightScaleVariable.Set(heightScale);
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
		/// Sets the diffuse map
		/// </summary>
		/// <param name="cubeMap">The diffuse map</param>
		public void SetDiffuseMap(ShaderResourceView diffuseMap)
		{
			this.diffuseMapVariable.SetResource(diffuseMap);
		}

		/// <summary>
		/// Sets the cube map
		/// </summary>
		/// <param name="cubeMap">The cube map</param>
		public void SetCubeMap(ShaderResourceView cubeMap)
		{
			this.cubeMapMapVariable.SetResource(cubeMap);
		}

		/// <summary>
		/// Sets the normal map
		/// </summary>
		/// <param name="diffuseMap">The normal map</param>
		public void SetNormalMap(ShaderResourceView normalMap)
		{
			this.normalMapMapVariable.SetResource(normalMap);
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

			this.worldViewProjVariable.Dispose();
			this.viewProjVariable.Dispose();
			this.worldVariable.Dispose();
			this.worldInvTransposeVariable.Dispose();
			this.eyePositionVariable.Dispose();
			this.textureTransformVariable.Dispose();

			this.fogStartVariable.Dispose();
			this.fogColorVariable.Dispose();
			this.fogRangeVariable.Dispose();

			this.dirLightsVariable.Dispose();
			this.materialVariable.Dispose();
			this.diffuseMapVariable.Dispose();
			this.cubeMapMapVariable.Dispose();
			this.normalMapMapVariable.Dispose();

			this.heightScaleVariable.Dispose();
			this.minTessDistanceVariable.Dispose();
			this.maxTessDistanceVariable.Dispose();
			this.minTessFactorVariable.Dispose();
			this.maxTessFactorVariable.Dispose();
		}
	}
}
