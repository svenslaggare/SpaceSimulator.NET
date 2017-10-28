using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common.Effects
{
    /// <summary>
    /// Represents a base class for an effect class
    /// </summary>
    public abstract class BaseEffect : IDisposable
    {
        protected readonly Device graphicsDevice;

        private readonly CompilationResult effectByteCode;
        protected readonly Effect effect;
        private readonly EffectTechnique technique;
        private readonly ShaderBytecode shaderBytecode;

        /// <summary>
        /// Creates a new base effect
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="effectName">The name of the effect file</param>
        /// <param name="techniqueName">The name of the technique to use</param>
        public BaseEffect(Device graphicsDevice, string effectName, string techniqueName)
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
        }
    }
}
