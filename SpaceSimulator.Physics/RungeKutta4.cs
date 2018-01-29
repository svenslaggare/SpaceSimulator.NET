using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.PhysicsTest
{
    /// <summary>
    /// The state of an object
    /// </summary>
    public struct ObjectState
    {
        /// <summary>
        /// The time
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The mass
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The moment-of-inertia
        /// </summary>
        public double MomentOfInertia { get; set; }

        /// <summary>
        /// The position
        /// </summary>
        public Vector3d Position { get; set; }

        /// <summary>
        /// The velocity
        /// </summary>
        public Vector3d Velocity { get; set; }

        /// <summary>
        /// The orientation
        /// </summary>
        public Quaterniond Orientation { get; set; }

        /// <summary>
        /// The angular momentum
        /// </summary>
        public Vector3d AngularMomentum { get; set; }

        /// <summary>
        /// Creates a new state
        /// </summary>
        /// <param name="time">The time</param>
        /// <param name="mass">The mass of the object</param>
        /// <param name="momentOfInertia">The moment of intertia</param>
        /// <param name="position">The position</param>
        /// <param name="velocity">The velocity</param>
        /// <param name="orientation">The orientation</param>
        /// <param name="angularMomentum">The angular momentum</param>
        public ObjectState(double time, double mass, double momentOfInertia, Vector3d position, Vector3d velocity, Quaterniond? orientation = null, Vector3d? angularMomentum = null)
        {
            this.Time = time;
            this.Mass = mass;
            this.MomentOfInertia = momentOfInertia;
            this.Position = position;
            this.Velocity = velocity;
            this.Orientation = orientation ?? Quaterniond.Identity;
            this.AngularMomentum = angularMomentum ?? Vector3d.Zero;
        }

        /// <summary>
        /// Returns the angular velocity
        /// </summary>
        public Vector3d AngularVelocity => this.AngularMomentum / this.MomentOfInertia;

        /// <summary>
        /// Calculates the distance to the given state
        /// </summary>
        /// <param name="other">The other state</param>
        public double Distance(ObjectState other)
        {
            return Vector3d.Distance(this.Position, other.Position);
        }
    }

    /// <summary>
    /// The acceleration state
    /// </summary>
    public struct AccelerationState
    {
        /// <summary>
        /// The acceleration
        /// </summary>
        public Vector3d Acceleration { get; set; }

        /// <summary>
        /// The torque
        /// </summary>
        public Vector3d Torque { get; set; }

        /// <summary>
        /// The change in mass of the object
        /// </summary>
        public double DeltaMass { get; set; }

        /// <summary>
        /// Creates a new acceleration state
        /// </summary>
        /// <param name="acceleration">The acceleration</param>
        /// <param name="torque">The torque</param>
        /// <param name="deltaMass">The change in mass of the object</param>
        public AccelerationState(Vector3d acceleration, Vector3d torque, double deltaMass)
        {
            this.Acceleration = acceleration;
            this.Torque = torque;
            this.DeltaMass = deltaMass;
        }
    }

    /// <summary>
    /// The state of the integrator
    /// </summary>
    public struct IntegratorState
    {
        /// <summary>
        /// The mass of the object
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The total time
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// The time step
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Creates a new integrator state
        /// </summary>
        /// <param name="mass">The mass of the object</param>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        public IntegratorState(double mass, double totalTime, double timeStep)
        {
            this.Mass = mass;
            this.TotalTime = totalTime;
            this.TimeStep = timeStep;
        }
    }

    /// <summary>
    /// Calculates the acceleration at the given state
    /// </summary>
    /// <param name="integratorState">The state of the integrator</param>
    /// <param name="state">The state of the object</param>
    public delegate AccelerationState CalculateAcceleration(ref IntegratorState integratorState, ref ObjectState state);

    /// <summary>
    /// Represents a numeric integrator using the Runge-Kutta method of order 4
    /// </summary>
    public class RungeKutta4Integrator
    {
        private static DerivativeState zero = new DerivativeState();

        /// <summary>
        /// Holds the state for the derivatives
        /// </summary>
        private struct DerivativeState
        {
            /// <summary>
            /// The velocity
            /// </summary>
            public Vector3d Velocity { get; set; }

            /// <summary>
            /// The acceleration
            /// </summary>
            public Vector3d Acceleration { get; set; }

            /// <summary>
            /// The spin
            /// </summary>
            public Quaterniond Spin { get; set; }

            /// <summary>
            /// The torque
            /// </summary>
            public Vector3d Torque { get; set; }

            /// <summary>
            /// Creates a new derivative state
            /// </summary>
            /// <param name="velocity">The velocity</param>
            /// <param name="acceleration">The acceleration</param>
            /// <param name="spin">The spin</param>
            /// <param name="torque">The torque</param>
            public DerivativeState(Vector3d velocity, Vector3d acceleration, Quaterniond spin, Vector3d torque)
            {
                this.Velocity = velocity;
                this.Acceleration = acceleration;
                this.Spin = spin;
                this.Torque = torque;
            }
        }

        /// <summary>
        /// Calculates the spin (time derivative of the orientation)
        /// </summary>
        /// <param name="orientation">The orientation</param>
        /// <param name="angularVelocity">The angular velocity</param>
        private Quaterniond CalculateSpin(Quaterniond orientation, Vector3d angularVelocity)
        {
            return 0.5 * new Quaterniond(angularVelocity.X, angularVelocity.Y, angularVelocity.Z, 0) * orientation;
        }

        /// <summary>
        /// Evaluates the state at the given time
        /// </summary>
        private DerivativeState Evaluate(
            ref ObjectState initial,
            double totalTime,
            double deltaTime,
            ref DerivativeState derivative,
            CalculateAcceleration calculateAcceleration)
        {
            var integratorState = new IntegratorState(initial.Mass, totalTime, deltaTime);

            var state = new ObjectState(
                initial.Time + deltaTime,
                initial.Mass,
                initial.MomentOfInertia,
                initial.Position + derivative.Velocity * deltaTime,
                initial.Velocity + derivative.Acceleration * deltaTime,
                initial.Orientation + derivative.Spin * deltaTime,
                initial.AngularMomentum + derivative.Torque * deltaTime);

            var accelerationState = calculateAcceleration(ref integratorState, ref state);
            state.Mass += accelerationState.DeltaMass;

            return new DerivativeState(
                state.Velocity,
                accelerationState.Acceleration,
                this.CalculateSpin(state.Orientation, state.AngularVelocity), 
                accelerationState.Torque);
        }

        /// <summary>
        /// Solves the given integration problem
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="totalTime">The total time</param>
        /// <param name="deltaTime">The delta time</param>
        /// <param name="calculateAcceleration">Calculates the acceleration</param>
        public ObjectState Solve(
            ref ObjectState state,
            double totalTime,
            double deltaTime,
            CalculateAcceleration calculateAcceleration)
        {
            var k0 = this.Evaluate(ref state, totalTime, 0, ref zero, calculateAcceleration);
            var k1 = this.Evaluate(ref state, totalTime + deltaTime * 0.5, deltaTime * 0.5, ref k0, calculateAcceleration);
            var k2 = this.Evaluate(ref state, totalTime + deltaTime * 0.5, deltaTime * 0.5, ref k1, calculateAcceleration);
            var k3 = this.Evaluate(ref state, totalTime + deltaTime, deltaTime, ref k2, calculateAcceleration);

            var velocity = (1.0 / 6.0) * (k0.Velocity + 2 * (k1.Velocity + k2.Velocity) + k3.Velocity);
            var acceleration = (1.0 / 6.0) * (k0.Acceleration + 2 * (k1.Acceleration + k2.Acceleration) + k3.Acceleration);

            var spin = (1.0 / 6.0) * (k0.Spin + 2 * (k1.Spin + k2.Spin) + k3.Spin);
            var torque = (1.0 / 6.0) * (k0.Torque + 2 * (k1.Torque + k2.Torque) + k3.Torque);

            state.Time += deltaTime;
            state.Position += velocity * deltaTime;
            state.Velocity += acceleration * deltaTime;
            state.Orientation = (state.Orientation + spin * deltaTime).Normalized();
            state.AngularMomentum += torque * deltaTime;
            return state;
        }
    }
}
