using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SpaceSimulator.Camera;
using SpaceSimulator.Common.Effects;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents a model for a physics object
    /// </summary>
    public interface IPhysicsObjectModel : IDisposable
    {
        /// <summary>
        /// Indicates if the effect is textured
        /// </summary>
        bool IsTextured { get; }

        /// <summary>
        /// Draws the given object
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="effect">The effect</param>
        /// <param name="arrowEffect">The arrow effect</param>
        /// <param name="camera">The camera</param>
        /// <param name="physicsObject">The physics object</param>
        void Draw(DeviceContext deviceContext, BasicEffect effect, BasicEffect arrowEffect, SpaceCamera camera, PhysicsObject physicsObject);
    }
}
