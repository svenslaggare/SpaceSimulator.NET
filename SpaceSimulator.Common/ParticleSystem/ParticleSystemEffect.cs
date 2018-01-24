using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Effects;

namespace SpaceSimulator.Common.ParticleSystem
{
    /// <summary>
    /// Represents a particle system effect
    /// </summary>
    public class ParticleSystemEffect : BaseEffect
    {
        private readonly EffectMatrixVariable viewProjVariable;
        private readonly EffectScalarVariable gameTimeVariable;
        private readonly EffectScalarVariable timeStepVariable;
        private readonly EffectVectorVariable eyePositionVariable;
        private readonly EffectVectorVariable emitPositionVariable;
        private readonly EffectVectorVariable emitDirectionVariable;

        private readonly EffectShaderResourceVariable textureMapArrayVariable;
        private readonly EffectShaderResourceVariable randomTextureVariable;

        /// <summary>
        /// Creates a new particle system effect
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="effectName">The name of the effect file</param>
        /// <param name="techniqueName">The name of the technique to use</param>
        public ParticleSystemEffect(Device graphicsDevice, string effectName, string techniqueName)
            : base(graphicsDevice, effectName, techniqueName)
        {
            //Get the variables
            this.viewProjVariable = this.effect.GetVariableByName("gViewProj").AsMatrix();
            this.gameTimeVariable = this.effect.GetVariableByName("gGameTime").AsScalar();
            this.timeStepVariable = this.effect.GetVariableByName("gTimeStep").AsScalar();
            this.eyePositionVariable = this.effect.GetVariableByName("gEyePosW").AsVector();
            this.emitPositionVariable = this.effect.GetVariableByName("gEmitPosW").AsVector();
            this.emitDirectionVariable = this.effect.GetVariableByName("gEmitDirW").AsVector();
            this.textureMapArrayVariable = this.effect.GetVariableByName("gTexArray").AsShaderResource();
            this.randomTextureVariable = this.effect.GetVariableByName("gRandomTex").AsShaderResource();
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
        /// Sets the game time
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public void SetGameTime(float gameTime)
        {
            this.gameTimeVariable.Set(gameTime);
        }

        /// <summary>
        /// Sets the time step
        /// </summary>
        /// <param name="timeStep">The time step</param>
        public void SetTimeStep(float timeStep)
        {
            this.timeStepVariable.Set(timeStep);
        }

        /// <summary>
        /// Sets the eye position
        /// </summary>
        /// <param name="eyePosition">The eye position</param>
        public void SetEyePosition(Vector3 eyePosition)
        {
            this.eyePositionVariable.Set(eyePosition);
        }

        /// <summary>
        /// Sets the emit position
        /// </summary>
        /// <param name="emitPosition">The emit position</param>
        public void SetEmitPosition(Vector3 emitPosition)
        {
            this.emitPositionVariable.Set(emitPosition);
        }

        /// <summary>
        /// Sets the emit direction
        /// </summary>
        /// <param name="emitDirection">The emit direction</param>
        public void SetEmitDirection(Vector3 emitDirection)
        {
            this.emitDirectionVariable.Set(emitDirection);
        }

        /// <summary>
        /// Sets the texture map array
        /// </summary>
        /// <param name="textureMapArray">The texture map array</param>
        public void SetTextureMapArray(ShaderResourceView textureMapArray)
        {
            this.textureMapArrayVariable.SetResource(textureMapArray);
        }

        /// <summary>
        /// Sets the diffuse map
        /// </summary>
        /// <param name="randomTexture">The random texture</param>
        public void SetRandomTexture(ShaderResourceView randomTexture)
        {
            this.randomTextureVariable.SetResource(randomTexture);
        }

        /// <summary>
        /// Disposes the resources
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.viewProjVariable.Dispose();
            this.gameTimeVariable.Dispose();
            this.timeStepVariable.Dispose();
            this.eyePositionVariable.Dispose();
            this.emitPositionVariable.Dispose();
            this.emitDirectionVariable.Dispose();
            this.textureMapArrayVariable.Dispose();
            this.randomTextureVariable.Dispose();
        }
    }
}
