using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSimulator.Helpers;
using SpaceSimulator.Mathematics;

namespace SpaceSimulator.Simulator.Rocket
{
    /// <summary>
    /// Represents a controller for pointing the rocket in a given direction
    /// </summary>
    public sealed class PointInDirectionController
    {
        private readonly RocketObject rocketObject;

        private Vector3d direction;
        private State state;

        private enum State
        {
            KillingRotation,
            Turning,
            Braking,
            Hold,
            HoldBraking
        }
        /// <summary>
        /// Creates a new point in direction controller
        /// </summary>
        /// <param name="rocketObject">The rocket object</param>
        public PointInDirectionController(RocketObject rocketObject)
        {
            this.rocketObject = rocketObject;
        }

        /// <summary>
        /// Sets the point direction
        /// </summary>
        /// <param name="direction">The direction</param>
        public void SetDirection(Vector3d direction)
        {
            var angle = MathHelpers.AngleBetween(direction, this.direction) * MathUtild.Rad2Deg;

            //if (this.direction.IsZero || angle >= 30.0)
            //{
            //    this.state = State.KillingRotation;
            //    Console.WriteLine("Killing rotation...");
            //}
            //else if (angle > 10.0)
            //{
            //    this.state = State.Turning;
            //}

            if (this.direction.IsZero ||angle > 10.0)
            {
                this.state = State.Turning;
            }

            this.direction = direction;
        }

        /// <summary>
        /// Calculates the brake time
        /// </summary>
        /// <param name="torque">The max torque</param>
        private double CalculateBrakeTime(double torque)
        {
            var angularSpeed = this.rocketObject.AngularVelocity.Length();
            var angularAcceleration = torque / this.rocketObject.MomentOfInertia;
            var deaccelerationTime = angularSpeed / angularAcceleration;
            return deaccelerationTime;
        }

        private Vector3d QuaternionError(Quaterniond current, Quaterniond desired)
        {
            var deltaAngle = Vector3d.Zero;

            deltaAngle.X = desired.W * current.X + desired.Z * current.Y - desired.Y * current.Z - desired.X * current.W;
            deltaAngle.Y = -desired.Z * current.X + desired.W * current.Y + desired.X * desired.Z - desired.Y * current.W;
            deltaAngle.Z = desired.Y * current.X - desired.X * current.Y + desired.W * current.Z - desired.Z * current.W;

            return deltaAngle;
        }

        private Quaterniond QuaternionError2(Quaterniond current, Quaterniond desired)
        {
            var lambda = Quaterniond.Dot(current, desired);
            return new Quaterniond(this.QuaternionError(current, desired), lambda);
        }

        /// <summary>
        /// Updates the controller
        /// </summary>
        /// <param name="totalTime">The total time</param>
        /// <param name="timeStep">The time step</param>
        /// <returns>The torque</returns>
        public Vector3d Update(double totalTime, double timeStep)
        {
            if (!this.direction.IsZero)
            {
                var orientation = this.rocketObject.Orientation;
                var currentDirection = Vector3d.Transform(Vector3d.ForwardRH, orientation);
                var desiredOrientation = MathHelpers.FaceDirectionQuaternion(this.direction);

                var rotationAxis = Vector3d.Cross(currentDirection, this.direction).Normalized();
                var angle = MathHelpers.AngleBetween(currentDirection, this.direction);

                const double axisDistanceLimit = 1E-2;
                var rotatesAroundCorrectAxis = Vector3d.Distance(rotationAxis, this.rocketObject.AngularVelocity.Normalized()) <= axisDistanceLimit;
                var inverseRotatesAroundCorrectAxis = Vector3d.Distance(-rotationAxis, this.rocketObject.AngularVelocity.Normalized()) <= axisDistanceLimit;

                var torque = 1E5;
                var holdTorque = 1E5;
                var angularSpeed = this.rocketObject.AngularVelocity.Length();

                //var wololo = desiredOrientation * orientation.Conjugated();
                //wololo = Quaterniond.RotationAxis(wololo.Axis, wololo.Angle * 0.001 * timeStep);
                //this.rocketObject.SetOrientation((wololo * orientation).Normalized());
                //return Vector3d.Zero;

                switch (this.state)
                {
                    case State.KillingRotation:
                        {
                            if (this.rocketObject.AngularVelocity.Length() <= 1E-6)
                            {
                                this.state = State.Turning;
                                Console.WriteLine("Killed rotation");
                            }
                            else
                            {
                                return -this.rocketObject.AngularVelocity * 1E6;
                            }
                        }
                        break;
                    case State.Turning:
                        {
                            var deaccelerationTime = this.CalculateBrakeTime(torque);
                            var averageAngularSpeed = angularSpeed / 2.0;
                            var brakeDistance = deaccelerationTime * averageAngularSpeed;

                            if (angle < brakeDistance)
                            {
                                this.state = State.Braking;
                                Console.WriteLine("Braking");
                                return -rotationAxis * torque;
                            }

                            return rotationAxis * torque;
                        }
                    case State.Braking:
                        {
                            if (angularSpeed < 1E-5)
                            {
                                this.state = State.Hold;
                                Console.WriteLine("Holding");
                                return Vector3d.Zero;
                            }

                            var deaccelerationTime = this.CalculateBrakeTime(torque);
                            var brakeTorque = (angularSpeed / deaccelerationTime) * this.rocketObject.MomentOfInertia;
                            return -this.rocketObject.AngularVelocity.Normalized() * torque;
                        }
                    case State.Hold:
                        {
                            if (angle * MathUtild.Rad2Deg >= 10.0)
                            {
                                this.state = State.Turning;
                            }

                            var deaccelerationTime = this.CalculateBrakeTime(holdTorque);
                            var averageAngularSpeed = angularSpeed / 2.0;
                            var brakeDistance = deaccelerationTime * averageAngularSpeed;

                            var turnDirection = rotationAxis;
                            if (angle < brakeDistance)
                            {
                                turnDirection = -rotationAxis;
                                this.state = State.HoldBraking;
                            }

                            //var nextAngularSpeed = ((this.rocketObject.State.AngularMomentum + turnDirection * holdTorque * timeStep) / this.rocketObject.MomentOfInertia).Length();
                            //if (nextAngularSpeed * MathUtild.Rad2Deg >= 0.06 && nextAngularSpeed > angularSpeed)
                            //{
                            //    return Vector3d.Zero;
                            //}

                            return turnDirection * holdTorque;
                        }
                    case State.HoldBraking:
                        {
                            if (angle * MathUtild.Rad2Deg >= 10.0)
                            {
                                this.state = State.Turning;
                            }

                            //var nextAngularSpeed = ((this.rocketObject.State.AngularMomentum + -this.rocketObject.AngularVelocity.Normalized() * holdTorque * timeStep) 
                            //                       / this.rocketObject.MomentOfInertia).Length();
                            //if (nextAngularSpeed * MathUtild.Rad2Deg >= 0.06 && nextAngularSpeed > angularSpeed)
                            //{
                            //    return Vector3d.Zero;
                            //}

                            if (angularSpeed < 1E-6)
                            {
                                this.state = State.Hold;
                                return Vector3d.Zero;
                            }

                            var deaccelerationTime = this.CalculateBrakeTime(holdTorque);
                            var brakeTorque = (angularSpeed / deaccelerationTime) * this.rocketObject.MomentOfInertia;
                            return -this.rocketObject.AngularVelocity.Normalized() * holdTorque;
                        }
                }
            }

            return Vector3d.Zero;
        }
    }
}
