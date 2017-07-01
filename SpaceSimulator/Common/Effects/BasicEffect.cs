using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace SpaceSimulator.Common.Effects
{
    /// <summary>
    /// The vertex format for the basic effect
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BasicVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;

        /// <summary>
        /// Create the input elements
        /// </summary>
        public static InputElement[] CreateInput()
        {
            return new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            };
        }
    };

    /// <summary>
    /// Represents a basic effect
    /// </summary>
    public class BasicEffect : IDisposable
    {
        private readonly Device graphicsDevice;

        private readonly CompilationResult effectByteCode;
        protected readonly Effect effect;
        private readonly EffectTechnique technique;
        private readonly ShaderBytecode shaderBytecode;

        private readonly EffectVectorVariable eyePositionVariable;
        private readonly EffectVectorVariable pointLightSourceVariable;

        private readonly EffectMatrixVariable worldViewProjectionVariable;
        private readonly EffectMatrixVariable worldVariable;
        private readonly EffectMatrixVariable worldInverseTransposeVariable;
        private readonly EffectMatrixVariable textureTransformVariable;

        private readonly EffectVectorVariable fogColor;
        private readonly EffectScalarVariable fogStart;
        private readonly EffectScalarVariable fogRange;

        private readonly EffectVariable dirLightsVariable;
        private readonly EffectVariable materialVariable;
        private readonly EffectShaderResourceVariable diffuseMapVariable;

        private readonly EffectScalarVariable blurSizeXVariable;
        private readonly EffectScalarVariable blurSizeYVariable;

        /// <summary>
        /// The input layout
        /// </summary>
        public InputLayout InputLayout { get; private set; }

        /// <summary>
        /// Creates a new basic effect
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="effectName">The name of the effect file</param>
        /// <param name="techniqueName">The name of the technique to use</param>
        public BasicEffect(Device graphicsDevice, string effectName, string techniqueName)
        {
            this.graphicsDevice = graphicsDevice;

            //Compile the effect
            this.effectByteCode = ShaderBytecode.CompileFromFile(
                effectName,
                "fx_5_0",
                ShaderFlags.None,
                EffectFlags.None,
                include: new IncludeEffect());
            Debug.Write(this.effectByteCode.Message);

            this.effect = new Effect(this.graphicsDevice, effectByteCode);
            this.technique = effect.GetTechniqueByName(techniqueName);
            var pass = technique.GetPassByIndex(0);
            this.shaderBytecode = pass.Description.Signature;

            //Get the variables
            this.eyePositionVariable = this.effect.GetVariableByName("gEyePosW").AsVector();
            this.pointLightSourceVariable = this.effect.GetVariableByName("gPointLightSource").AsVector();

            this.worldViewProjectionVariable = this.effect.GetVariableByName("gWorldViewProj").AsMatrix();
            this.worldVariable = this.effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverseTransposeVariable = this.effect.GetVariableByName("gWorldInvTranspose").AsMatrix();
            this.textureTransformVariable = this.effect.GetVariableByName("gTexTransform").AsMatrix();

            this.fogColor = this.effect.GetVariableByName("gFogColor").AsVector();
            this.fogStart = this.effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.effect.GetVariableByName("gFogRange").AsScalar();

            this.dirLightsVariable = this.effect.GetVariableByName("gDirLights");
            this.materialVariable = this.effect.GetVariableByName("gMaterial");
            this.diffuseMapVariable = this.effect.GetVariableByName("gDiffuseMap").AsShaderResource();

            this.blurSizeXVariable = this.effect.GetVariableByName("gBlurSizeX").AsScalar();
            this.blurSizeYVariable = this.effect.GetVariableByName("gBlurSizeY").AsScalar();
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
        /// Returns the effect passes
        /// </summary>
        public IEnumerable<EffectPass> Passes
        {
            get
            {
                for (int i = 0; i < this.Technique.Description.PassCount; ++i)
                {
                    yield return this.Technique.GetPassByIndex(i);
                }
            }
        }

        /// <summary>
        /// Creates the input layout
        /// </summary>
        /// <param name="elements">The input elements</param>
        public void CreateInputLayout(InputElement[] elements)
        {
            this.InputLayout = new InputLayout(this.graphicsDevice, this.ShaderBytecode, elements);
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
        /// Sets the point light source
        /// </summary>
        /// <param name="position">The source position</param>
        public void SetPointLightSource(Vector3 position)
        {
            this.pointLightSourceVariable.Set(position);
        }

        /// <summary>
        /// Sets the world-view-projection matrix
        /// </summary>
        /// <param name="worldViewProjection">The world-view-projection matrix</param>
        public void SetWorldViewProjection(Matrix worldViewProjection)
		{
			this.worldViewProjectionVariable.SetMatrix(worldViewProjection);
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
		/// <param name="worldInverseTranpose">The world-inverse-transpose matrix</param>
		public void SetWorldInverseTranspose(Matrix worldInverseTranpose)
		{
			this.worldInverseTransposeVariable.SetMatrix(worldInverseTranpose);
		}

        /// <summary>
        /// Sets the transformation matrices
        /// </summary>
        /// <param name="viewProjection">The view-projection matrix</param>
        /// <param name="world">The world matrix</param>
        public void SetTransform(Matrix viewProjection, Matrix world)
        {
            var worldInverseTranspose = world;
            worldInverseTranspose.Invert();
            worldInverseTranspose.Transpose();
            var worldViewProjection = world * viewProjection;

            this.SetWorld(world);
            this.SetWorldInverseTranspose(worldInverseTranspose);
            this.SetWorldViewProjection(worldViewProjection);
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
		/// Sets the diffuse map
		/// </summary>
		/// <param name="diffuseMap">The diffuse map</param>
		public void SetDiffuseMap(ShaderResourceView diffuseMap)
		{
			this.diffuseMapVariable.SetResource(diffuseMap);
		}

        /// <summary>
        /// Sets the blur size x
        /// </summary>
        /// <param name="amount">The amount</param>
        public void SetBlurSizeX(float amount)
        {
            this.blurSizeXVariable.Set(amount);
        }

        /// <summary>
        /// Sets the blur size y
        /// </summary>
        /// <param name="amount">The amount</param>
        public void SetBlurSizeY(float amount)
        {
            this.blurSizeYVariable.Set(amount);
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
		public virtual void Dispose()
		{
			this.effectByteCode.Dispose();
			this.effect.Dispose();
			this.shaderBytecode.Dispose();
			this.technique.Dispose();

            this.eyePositionVariable?.Dispose();
            this.pointLightSourceVariable?.Dispose();

            this.worldViewProjectionVariable?.Dispose();
			this.worldVariable?.Dispose();
			this.worldInverseTransposeVariable?.Dispose();
            this.textureTransformVariable?.Dispose();

            this.fogStart?.Dispose();
            this.fogColor?.Dispose();
            this.fogRange?.Dispose();

            this.dirLightsVariable?.Dispose();
            this.materialVariable?.Dispose();
            this.diffuseMapVariable?.Dispose();

            this.blurSizeXVariable?.Dispose();
            this.blurSizeYVariable?.Dispose();

            this.InputLayout?.Dispose();
        }
	}
}
