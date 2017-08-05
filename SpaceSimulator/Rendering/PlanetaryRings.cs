using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Camera;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.Rendering
{
    /// <summary>
    /// Represents planetary rings
    /// </summary>
    public sealed class PlanetaryRings : IDisposable
    {
        private readonly Rendering.Orbit renderingOrbit;

        /// <summary>
        /// The color of the ring
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// The radius of the rings (from the planet)
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// The width of the rings
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Creates new rings
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="planet">The planet the rings are around</param>
        /// <param name="color">The color of the rings</param>
        /// <param name="radius">The radius of the rings</param>
        /// <param name="width">The width of the rings</param>
        public PlanetaryRings(Device graphicsDevice, NaturalSatelliteObject planet, Color color, double radius, double width)
        {
            this.Color = color;
            this.Radius = radius;
            this.Width = width;

            var positions = OrbitPositions.Create(Physics.Orbit.New(planet, semiMajorAxis: this.Radius), true);

            this.renderingOrbit = new Rendering.Orbit(
                graphicsDevice,
                positions,
                this.Color,
                false)
            {
                IsBound = true,
                ChangeBrightnessForPassed = false,
                RotateToFaceCamera = false
            };
        }

        /// <summary>
        /// Draws the rings
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="ringEffect">The ring effect</param>
        /// <param name="pass">The effect pass</param>
        /// <param name="camera">The camera</param>
        /// <param name="world">The world matrix</param>
        /// <param name="position">The position to draw at</param>
        public void Draw(DeviceContext deviceContext, OrbitEffect ringEffect, EffectPass pass, SpaceCamera camera, Matrix world, Vector3 position)
        {
            this.renderingOrbit.Draw(
                deviceContext,
                ringEffect,
                pass,
                camera,
                world,
                position,
                lineWidth: camera.ToDraw(this.Width));
        }

        public void Dispose()
        {
            this.renderingOrbit.Dispose();
        }
    }
}
