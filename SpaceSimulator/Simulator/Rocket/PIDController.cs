using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a PID controller
    /// </summary>
    public sealed class PIDController
    {
        private readonly double proportionalGain;
        private readonly double integralGain;
        private readonly double derivativeGain;

        private double accumulatedIntegralError = 0.0;
        private double prevError = 0.0;
        private bool hasPrevious = false;

        /// <summary>
        /// Creates a new PID controller
        /// </summary>
        /// <param name="proportionalGain">The proportional gain</param>
        /// <param name="integralGain">The integral gain</param>
        /// <param name="derivativeGain">The derivative gain</param>
        public PIDController(double proportionalGain, double integralGain, double derivativeGain)
        {
            this.proportionalGain = proportionalGain;
            this.integralGain = integralGain;
            this.derivativeGain = derivativeGain;
        }

        /// <summary>
        /// Computes the command
        /// </summary>
        /// <param name="error">The error</param>
        /// <param name="duration">The duration</param>
        public double ComputeCommand(double error, double duration)
        {
            var derivative = 0.0;
            if (this.hasPrevious)
            {
                derivative = (error - this.prevError) / duration;
                this.accumulatedIntegralError += error * duration;
            }

            var command = this.proportionalGain * error + this.integralGain * this.accumulatedIntegralError * this.derivativeGain * derivative;

            this.hasPrevious = true;
            this.prevError = error;
            return command;
        }
    }
}
