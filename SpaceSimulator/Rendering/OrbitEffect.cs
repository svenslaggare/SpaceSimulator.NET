using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Effects;
using Device = SharpDX.Direct3D11.Device;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents an orbit effect
    /// </summary>
    public class OrbitEffect : BasicEffect
    {
        private readonly EffectScalarVariable lineWidth;

        /// <summary>
        /// Creates a new basic effect
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="effectName">The name of the effect file</param>
        /// <param name="techniqueName">The name of the technique to use</param>
        public OrbitEffect(Device graphicsDevice, string effectName, string techniqueName)
            : base(graphicsDevice, effectName, techniqueName)
        {
            this.lineWidth = this.effect.GetVariableByName("gLineWidth").AsScalar();
        }

        /// <summary>
        /// Sets the line width
        /// </summary>
        /// <param name="lineWidth">The line width</param>
        public void SetLineWidth(float lineWidth)
        {
            this.lineWidth.Set(lineWidth);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.lineWidth.Dispose();
        }
    }
}
