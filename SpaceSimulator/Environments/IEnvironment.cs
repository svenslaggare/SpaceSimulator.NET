using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Environments
{
    /// <summary>
    /// Represents an environment
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Creates a new environment
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        SimulatorContainer Create(SharpDX.Direct3D11.Device graphicsDevice);
    }
}
