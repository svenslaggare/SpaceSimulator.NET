using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.Camera
{
    /// <summary>
    /// Contains helper methods for cameras
    /// </summary>
    public static class CameraHelpers
    {
        /// <summary>
        /// Extract the frustum planes from given view projection matrix
        /// </summary>
        /// <param name="viewProjection">The view projection matrix</param>
        public static Vector4[] ExtractFrustumPlanes(Matrix viewProjection)
        {
            var planes = new Vector4[6];

            // Left
            planes[0] = new Vector4(
                viewProjection[0, 3] + viewProjection[0, 0],
                viewProjection[1, 3] + viewProjection[1, 0],
                viewProjection[2, 3] + viewProjection[2, 0],
                viewProjection[3, 3] + viewProjection[3, 0]);

            //Right
            planes[1] = new Vector4(
                viewProjection[0, 3] - viewProjection[0, 0],
                viewProjection[1, 3] - viewProjection[1, 0],
                viewProjection[2, 3] - viewProjection[2, 0],
                viewProjection[3, 3] - viewProjection[3, 0]);

            //Bottom
            planes[2] = new Vector4(
                viewProjection[0, 3] + viewProjection[0, 1],
                viewProjection[1, 3] + viewProjection[1, 1],
                viewProjection[2, 3] + viewProjection[2, 1],
                viewProjection[3, 3] + viewProjection[3, 1]);

            //Top
            planes[3] = new Vector4(
                viewProjection[0, 3] - viewProjection[0, 1],
                viewProjection[1, 3] - viewProjection[1, 1],
                viewProjection[2, 3] - viewProjection[2, 1],
                viewProjection[3, 3] - viewProjection[3, 1]);

            //Near
            planes[4] = new Vector4(
                viewProjection[0, 2],
                viewProjection[1, 2],
                viewProjection[2, 2],
                viewProjection[3, 2]);

            //Far
            planes[5] = new Vector4(
                viewProjection[0, 3] - viewProjection[0, 2],
                viewProjection[1, 3] - viewProjection[1, 2],
                viewProjection[2, 3] - viewProjection[2, 2],
                viewProjection[3, 3] - viewProjection[3, 2]);

            //Normalize the plane equations.
            for (int i = 0; i < 6; ++i)
            {
                var plane = Plane.Normalize(new Plane(planes[i].X, planes[i].Y, planes[i].Z, planes[i].W));
                planes[i] = new Vector4(plane.Normal, plane.D);
            }

            return planes;
        }
    }
}
