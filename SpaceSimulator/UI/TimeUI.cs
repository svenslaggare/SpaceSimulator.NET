﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using SpaceSimulator.Common;
using SpaceSimulator.Common.Input;
using SpaceSimulator.Helpers;
using SpaceSimulator.Simulator;

namespace SpaceSimulator.UI
{
    /// <summary>
    /// Represents an UI component for managing the time
    /// </summary>
    public class TimeUI : UIComponent
    {
        private readonly int[] simulationSpeeds = new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000 };
        private int simulationSpeedIndex = 0;

        /// <summary>
        /// Creates a new time UI component
        /// </summary>
        /// <param name="renderingManager2D">The rendering manager 2D</param>
        /// <param name="keyboardManager">The keyboard manager</param>
        /// <param name="simulatorContainer">The simulator container</param>
        public TimeUI(RenderingManager2D renderingManager2D, KeyboardManager keyboardManager, SimulatorContainer simulatorContainer)
            : base(renderingManager2D, keyboardManager, simulatorContainer)
        {

        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last frame</param>
        public override void Update(TimeSpan elapsed)
        {
            var deltaSimulationSpeedIndex = 0;
            if (this.KeyboardManager.IsKeyPressed(Key.Period))
            {
                deltaSimulationSpeedIndex = 1;
            }

            if (this.KeyboardManager.IsKeyPressed(Key.Comma))
            {
                deltaSimulationSpeedIndex = -1;
            }

            if (this.KeyboardManager.IsKeyPressed(Key.Pause))
            {
                this.SimulatorContainer.Paused = !this.SimulatorContainer.Paused;
            }

            this.simulationSpeedIndex = MathUtil.Clamp(this.simulationSpeedIndex + deltaSimulationSpeedIndex, 0, this.simulationSpeeds.Length - 1);
            this.SimulatorEngine.SimulationSpeed = this.simulationSpeeds[this.simulationSpeedIndex];
        }

        /// <summary>
        /// Draws the component
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public override void Draw(SharpDX.Direct2D1.DeviceContext deviceContext)
        {
            this.TextColorBrush.DrawText(
                deviceContext,
                $"Simulation speed: {this.SimulatorEngine.SimulationSpeed}x" + (this.SimulatorContainer.Paused ? " (paused)" : ""),
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(UIConstants.OffsetLeft, 5)));

            this.TextColorBrush.DrawText(
                deviceContext,
                "Simulated time: " + DataFormatter.Format(Math.Round(this.SimulatorEngine.TotalTime), DataUnit.Time),
                this.TextFormat,
                this.RenderingManager2D.TextPosition(new Vector2(UIConstants.OffsetLeft, 23)));
        }
    }
}
