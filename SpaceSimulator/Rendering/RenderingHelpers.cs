using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMine.ColorSpaces;
using SharpDX;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Helper methods used for rendering
    /// </summary>
    public static class RenderingHelpers
    {
        /// <summary>
        /// Modifies the brightness of the given color
        /// </summary>
        /// <param name="color">The color</param>
        /// <param name="brightness">The new brightness</param>
        public static Color ModifyBrightness(Color color, float brightness)
        {
            var hsv = (new Rgb() { R = color.R, G = color.G, B = color.B }).To<Hsv>();
            hsv.V = brightness;
            var rgb = hsv.To<Rgb>();
            return new Color((byte)rgb.R, (byte)rgb.G, (byte)rgb.B, color.A);
        }
    }
}
