using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Camera;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.Common.ParticleSystem
{
    /// <summary>
    /// The particle vertex
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleVertex
    {
        public Vector3 InitialPosition;
        public Vector3 InitialVelocity;
        public Vector2 Size;
        public float Age;
        public uint Type;
    }

    /// <summary>
    /// Represents a particle system
    /// </summary>
    public class ParticleSystemRender : IDisposable
    {
        private readonly int maxParticles;
        private bool isFirstRun = true;

        private float gameTime;
        private float timeStep;
        private float age;

        private Vector3 eyePosition;
        private Vector3 emitPosition;
        private Vector3 emitDirection = new Vector3(0, 1, 0);

        private Buffer initVertexBuffer;
        private Buffer drawVertexBuffer;
        private Buffer streamOutVertexBuffer;

        private readonly ShaderResourceView textureArrayView;
        private readonly ShaderResourceView randomTextureView;

        private ParticleSystemEffect effect;
        private InputLayout inputLayout;

        private EffectTechnique streamOutTechnique;
        private EffectTechnique drawTechnique;

        /// <summary>
        /// Creates a new particle system
        /// </summary>
        /// <param name="maxParticles">The maximum number of particles</param>
        /// <param name="textureArrayView">The texture view</param>
        /// <param name="randomTextureView">The random texture view</param>
        public ParticleSystemRender(int maxParticles, ShaderResourceView textureArrayView, ShaderResourceView randomTextureView)
        {
            this.maxParticles = maxParticles;
            this.textureArrayView = textureArrayView;
            this.randomTextureView = randomTextureView;
        }

        /// <summary>
        /// Returns the age
        /// </summary>
        public float Age
        {
            get { return this.age; }
        }

        /// <summary>
        /// Sets the eye position
        /// </summary>
        /// <param name="eyePosition">The eye position</param>
        public void SetEyePosition(Vector3 eyePosition)
        {
            this.eyePosition = eyePosition;
        }

        /// <summary>
        /// Sets the emit position
        /// </summary>
        /// <param name="emitPosition">The emit position</param>
        public void SetEmitPosition(Vector3 emitPosition)
        {
            this.emitPosition = emitPosition;
        }

        /// <summary>
        /// Sets the emit direction
        /// </summary>
        /// <param name="emitDirection">The emit direction</param>
        public void SetEmitDirection(Vector3 emitDirection)
        {
            this.emitDirection = emitDirection;
        }

        /// <summary>
        /// Updates the particle system
        /// </summary>
        /// <param name="elapsed">The elapsed time</param>
        /// <param name="gameTime">The game time</param>
        public void Update(TimeSpan elapsed, TimeSpan gameTime)
        {
            this.gameTime = (float)gameTime.TotalSeconds;
            this.timeStep = (float)elapsed.TotalSeconds;
            this.age += this.timeStep;
        }

        /// <summary>
        /// Resets the particle system
        /// </summary>
        public void Reset()
        {
            this.isFirstRun = true;
            this.age = 0.0f;
        }

        /// <summary>
        /// Initializes the particle system
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="effectName">The name of the effect name</param>
        public void Initialize(Device graphicsDevice, string effectName)
        {
            this.effect = new ParticleSystemEffect(graphicsDevice, effectName, "StreamOutTech");
            this.inputLayout = new InputLayout(graphicsDevice, this.effect.ShaderBytecode, new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
                new InputElement("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0),
                new InputElement("SIZE", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0),
                new InputElement("AGE", 0, SharpDX.DXGI.Format.R32_Float, 32, 0),
                new InputElement("TYPE", 0, SharpDX.DXGI.Format.R32_UInt, 36, 0),
            });

            this.streamOutTechnique = this.effect.GetTechnique("StreamOutTech");
            this.drawTechnique = this.effect.GetTechnique("DrawTech");

            var bufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Utilities.SizeOf<ParticleVertex>(),
                BindFlags = BindFlags.VertexBuffer
            };

            this.initVertexBuffer = new Buffer(graphicsDevice, bufferDesc);

            bufferDesc.SizeInBytes = Utilities.SizeOf<ParticleVertex>() * this.maxParticles;
            bufferDesc.BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput;

            this.drawVertexBuffer = new Buffer(graphicsDevice, bufferDesc);
            this.streamOutVertexBuffer = new Buffer(graphicsDevice, bufferDesc);

            this.effect.SetTextureMapArray(this.textureArrayView);
            this.effect.SetRandomTexture(this.randomTextureView);
        }

        /// <summary>
        /// Swapws the given values
        /// </summary>
        private void Swap<T>(ref T x, ref T y)
        {
            T tmp = x;
            x = y;
            y = tmp;
        }

        /// <summary>
        /// Draws the particle system
        /// </summary>
        /// <param name="context">The device context</param>
        /// <param name="camera">The camera</param>
        public void Draw(DeviceContext context, BaseCamera camera)
        {
            var viewProjection = Matrix.Scaling(0.01f) * camera.View * camera.Projection;
            //var viewProjection = camera.View * camera.Projection;

            //Set constants.
            this.effect.SetViewProjection(viewProjection);
            this.effect.SetGameTime(this.gameTime);
            this.effect.SetTimeStep(this.timeStep);
            this.effect.SetEyePosition(this.eyePosition);
            this.effect.SetEmitPosition(this.emitPosition);
            this.effect.SetEmitDirection(this.emitDirection);

            context.InputAssembler.InputLayout = this.inputLayout;
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;

            int stride = Utilities.SizeOf<ParticleVertex>();

            //On the first pass, use the initialization VB.  Otherwise, use
            //the VB that contains the current particle list.
            if (this.isFirstRun)
            {
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.initVertexBuffer, stride, 0));
            }
            else
            {
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.drawVertexBuffer, stride, 0));
            }

            //Draw the current particle list using stream-out only to update them.  
            //The updated vertices are streamed-out to the target VB. 
            context.StreamOutput.SetTarget(this.streamOutVertexBuffer, 0);

            for (int i = 0; i < this.streamOutTechnique.Description.PassCount; i++)
            {
                var pass = this.streamOutTechnique.GetPassByIndex(i);
                pass.Apply(context);

                if (this.isFirstRun)
                {
                    context.Draw(1, 0);
                    this.isFirstRun = false;
                }
                else
                {
                    context.DrawAuto();
                }
            }

            //Done streaming-out--unbind the vertex buffer
            context.StreamOutput.SetTargets(new StreamOutputBufferBinding[]
            {
                new StreamOutputBufferBinding()
            });

            //Swap the buffers
            this.Swap(ref this.drawVertexBuffer, ref this.streamOutVertexBuffer);

            //Draw the updated particle system we just streamed-out. 
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.drawVertexBuffer, stride, 0));

            for (int i = 0; i < this.drawTechnique.Description.PassCount; i++)
            {
                var pass = this.drawTechnique.GetPassByIndex(i);
                pass.Apply(context);
                context.DrawAuto();
            }
        }

        /// <summary>
        /// Disposes the resources
        /// </summary>
        public void Dispose()
        {
            this.effect.Dispose();
            this.inputLayout.Dispose();
            this.drawTechnique.Dispose();
            this.streamOutTechnique.Dispose();

            this.initVertexBuffer.Dispose();
            this.drawVertexBuffer.Dispose();
            this.streamOutVertexBuffer.Dispose();

            this.textureArrayView.Dispose();
        }
    }
}
