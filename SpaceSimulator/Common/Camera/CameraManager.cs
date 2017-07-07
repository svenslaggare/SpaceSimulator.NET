using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Common.Camera
{
    /// <summary>
    /// Manages cameras
    /// </summary>
    public sealed class CameraManager
    {
        private readonly IDictionary<string, BaseCamera> cameras = new Dictionary<string, BaseCamera>();

        /// <summary>
        /// The active camera
        /// </summary>
        public BaseCamera ActiveCamera { get; private set; }

        /// <summary>
        /// Creates a new camera manager
        /// </summary>
        public CameraManager()
        {

        }

        /// <summary>
        /// Returns the cameras
        /// </summary>
        public IEnumerable<BaseCamera> Cameras => this.cameras.Values;

        /// <summary>
        /// Adds the given camera
        /// </summary>
        /// <param name="name">The name of the camera</param>
        /// <param name="camera">The camera</param>
        /// <param name="setActive">Indicates if the camera should be set as the active one</param>
        public void AddCamera(string name, BaseCamera camera, bool setActive = false)
        {
            this.cameras.Add(name, camera);

            if (setActive)
            {
                this.ActiveCamera = camera;
            }
        }

        /// <summary>
        /// Sets the active camera
        /// </summary>
        /// <param name="name">The name of the camera</param>
        /// <exception cref="KeyNotFoundException">If the camera does not exist</exception>
        public void SetActiveCamera(string name)
        {
            if (this.cameras.TryGetValue(name, out var camera))
            {
                this.ActiveCamera = camera;
            }
            else
            {
                throw new KeyNotFoundException("The camera does not exist.");
            }
        }

        /// <summary>
        /// Returns a camera with the given name
        /// </summary>
        /// <param name="name">The name of the camera</param>
        /// <returns>The camera or null</returns>
        public BaseCamera GetCamera(string name)
        {
            if (this.cameras.TryGetValue(name, out var camera))
            {
                return camera;
            }

            return null;
        }

        /// <summary>
        /// Returns a camera with the given name
        /// </summary>
        /// <param name="name">The name of the camera</param>
        /// <returns>The camera or null</returns>
        public BaseCamera this[string name] => this.GetCamera(name);

        /// <summary>
        /// Sets the projection matrix for all cameras
        /// </summary>
        /// <param name="fovY">The field of view in the y-direction</param>
        /// <param name="viewportWidth">The width of the viewport</param>
        /// <param name="viewportHeight">The height of the viewport</param>
        /// <param name="nearPlane">The near plane</param>
        /// <param name="farPlane">The far plane</param>
        public void SetProjection(float fovY, float viewportWidth, float viewportHeight, float nearPlane, float farPlane)
        {
            foreach (var camera in this.cameras.Values)
            {
                camera.SetProjection(fovY, viewportWidth, viewportHeight, nearPlane, farPlane);
            }
        }
    }
}
