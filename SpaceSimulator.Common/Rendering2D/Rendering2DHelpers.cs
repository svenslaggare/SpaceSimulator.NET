using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Common.Rendering2D
{
    /// <summary>
    /// Contains helper methods for 2D rendering
    /// </summary>
    public static class Rendering2DHelpers
    {
        /// <summary>
        /// Binds the resources if unbound for the given resources
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="resources">The resources</param>
        public static void BindResources(SharpDX.Direct2D1.DeviceContext deviceContext, params IRenderingResource2D[] resources)
        {
            foreach (var resource in resources)
            {
                if (!resource.HasBoundResources)
                {
                    resource.Update(deviceContext);
                }
            }
        }
    }
}
