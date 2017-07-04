using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Common.Camera
{
    /// <summary>
    /// Represents a camera used for space
    /// </summary>
    public abstract class SpaceCamera : BaseCamera
    {
        /// <summary>
        /// Sets the scale factor
        /// </summary>
        /// <param name="primaryBody">The primary body to base the scale on</param>
        public void SetScaleFactor(NaturalSatelliteObject primaryBody)
        {
            this.scaleFactor = 1.0 / (5.0 * primaryBody.Radius);
        }
    }
}
