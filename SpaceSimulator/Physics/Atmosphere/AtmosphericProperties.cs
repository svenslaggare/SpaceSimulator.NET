using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Physics.Atmosphere
{
    /// <summary>
    /// Contains the atmospheric properties for an object
    /// </summary>
    public sealed class AtmosphericProperties
    {
        /// <summary>
        /// The reference area
        /// </summary>
        public double ReferenceArea { get; }

        /// <summary>
        /// The drag coefficient
        /// </summary>
        public double DragCoefficient { get; }

        /// <summary>
        /// Creates new atmospheric properties for an object
        /// </summary>
        /// <param name="referenceArea">The reference area</param>
        /// <param name="dragCoefficient">The drag coefficient</param>
        public AtmosphericProperties(double referenceArea, double dragCoefficient)
        {
            this.ReferenceArea = referenceArea;
            this.DragCoefficient = dragCoefficient;
        }
    }
}
