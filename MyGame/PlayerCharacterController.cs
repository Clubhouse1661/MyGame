using System;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using Stride.Animations;

namespace MyGame
{
    /// <summary>
    /// A player character controller that handles movement, jumping, and animation with physics-based gameplay.
    /// </summary>
    /// <remarks>
    /// Extends the BasicCameraController functionality with character-specific features including:
    /// - Physics-based movement using character controller
    /// - Jump mechanics with ground detection
    /// - Animation state management (Idle, Walk, Run, Jump)
    /// - Collision handling to prevent clipping
    /// </remarks>
    public class PlayerCharacterController : SyncScript
    {
        private const float MaximumPitch = MathUtil.PiOverTwo * 0.99f;

        // Movement and camera state
        private Vector3 upVector;
        private Vector3 movement;
        private float yaw;
        private float pitch;
        private bool isGrounded;
        private float verticalVelocity;

        // Animation state
        private AnimationState currentAnimationState = AnimationState.Idle;
        private AnimationComponent animationComponent;
        
        // Physics components  
        private RigidbodyComponent rigidbodyComponent;

        public enum AnimationState
        {
            Idle,
            Walk,
            Run,
            Jump
        }

        #region Configuration Properties

        [Display("Movement")]
        public bool Gamepad { get; set; } = false;

        [Display("Movement Speed")]
        public float MovementSpeed { get; set; } = 5.0f;

        [Display("Run Speed Multiplier")]
        public float RunSpeedMultiplier { get; set; } = 2.0f;

        [Display("Jump Height")]
        public float JumpHeight { get; set; } = 1.5f;

        [Display("Gravity")]
        public float Gravity { get; set; } = -25.0f;

        [Display("Ground Detection Distance")]
        public float GroundCheckDistance { get; set; } = 0.1f;

        [Display("Camera")]
        public Vector2 MouseRotationSpeed { get; set; } = new Vector2(1.0f, 1.0f);

        [Display("Mouse Sensitivity")]
        public Vector2 KeyboardRotationSpeed { get; set; } = new Vector2(3.0f);

        [Display("Touch Controls")]
        public Vector2 TouchRotationSpeed { get; set; } = new Vector2(1.0f, 0.7f);

        #endregion

        public override void Start()
        {
            base.Start();

            // Default up-direction
            upVector = Vector3.UnitY;

            // Get physics components
            rigidbodyComponent = Entity.Get<RigidbodyComponent>();
            animationComponent = Entity.Get<AnimationComponent>();

            // If no rigidbody is present, we'll use transform-based movement as fallback
            if (rigidbodyComponent != null)
            {
                // Configure rigidbody for character movement
                rigidbodyComponent.RigidBodyType = RigidBodyTypes.Dynamic;
                rigidbodyComponent.Mass = 1.0f;
            }

            // Configure touch input
            if (!Platform.IsWindowsDesktop)
            {
                Input.Gestures.Add(new GestureConfigDrag());
                Input.Gestures.Add(new GestureConfigComposite());
            }

            // Initialize animation state
            if (animationComponent != null)
            {
                PlayAnimation(AnimationState.Idle);
            }
        }

        public override void Update()
        {
            ProcessInput();
            UpdatePhysics();
            UpdateAnimations();
            UpdateCameraTransform();
        }

        private void ProcessInput()
        {
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            movement = Vector3.Zero;
            yaw = 0f;
            pitch = 0f;

            // Check if player is grounded
            CheckGroundedState();

            // Keyboard and Gamepad based movement
            ProcessMovementInput(deltaTime);

            // Mouse and touch rotation
            ProcessRotationInput(deltaTime);

            // Jump input
            ProcessJumpInput();
        }

        private void ProcessMovementInput(float deltaTime)
        {
            Vector3 inputDirection = Vector3.Zero;
            bool isRunning = false;

            if (Gamepad && Input.HasGamePad)
            {
                GamePadState padState = Input.DefaultGamePad.State;
                inputDirection.Z += padState.LeftThumb.Y;
                inputDirection.X += padState.LeftThumb.X;

                // Check for run button
                isRunning = (padState.Buttons & (GamePadButton.A | GamePadButton.LeftShoulder | GamePadButton.RightShoulder)) != 0;
            }

            if (Input.HasKeyboard)
            {
                // WASD movement
                if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
                    inputDirection.Z += 1;
                if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
                    inputDirection.Z -= 1;
                if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
                    inputDirection.X -= 1;
                if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
                    inputDirection.X += 1;

                // Check for run (shift key)
                isRunning = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
            }

            // Normalize input direction
            if (inputDirection.Length() > 1f)
            {
                inputDirection = Vector3.Normalize(inputDirection);
            }

            // Calculate movement speed
            float currentSpeed = MovementSpeed;
            if (isRunning && inputDirection.Length() > 0)
            {
                currentSpeed *= RunSpeedMultiplier;
            }

            // Apply movement relative to camera direction
            movement = inputDirection * currentSpeed;

            // Update animation state based on movement
            UpdateMovementAnimationState(inputDirection.Length(), isRunning);
        }

        private void ProcessRotationInput(float deltaTime)
        {
            // Keyboard rotation
            if (Input.HasKeyboard)
            {
                Vector2 rotation = Vector2.Zero;
                
                if (Input.IsKeyDown(Keys.NumPad2)) rotation.X += 1;
                if (Input.IsKeyDown(Keys.NumPad8)) rotation.X -= 1;
                if (Input.IsKeyDown(Keys.NumPad4)) rotation.Y += 1;
                if (Input.IsKeyDown(Keys.NumPad6)) rotation.Y -= 1;

                if (rotation.Length() > 1f)
                    rotation = Vector2.Normalize(rotation);

                rotation *= KeyboardRotationSpeed * deltaTime;
                pitch += rotation.X;
                yaw += rotation.Y;
            }

            // Mouse rotation
            if (Input.HasMouse)
            {
                if (Input.IsMouseButtonDown(MouseButton.Right))
                {
                    Input.LockMousePosition();
                    Game.IsMouseVisible = false;

                    yaw -= Input.MouseDelta.X * MouseRotationSpeed.X;
                    pitch -= Input.MouseDelta.Y * MouseRotationSpeed.Y;
                }
                else
                {
                    Input.UnlockMousePosition();
                    Game.IsMouseVisible = true;
                }
            }

            // Handle touch gestures
            foreach (var gestureEvent in Input.GestureEvents)
            {
                if (gestureEvent.Type == GestureType.Drag)
                {
                    var drag = (GestureEventDrag)gestureEvent;
                    var dragDistance = drag.DeltaTranslation;
                    yaw = -dragDistance.X * TouchRotationSpeed.X;
                    pitch = -dragDistance.Y * TouchRotationSpeed.Y;
                }
            }
        }

        private void ProcessJumpInput()
        {
            if (Input.HasKeyboard && Input.IsKeyPressed(Keys.Space) && isGrounded)
            {
                Jump();
            }

            if (Gamepad && Input.HasGamePad)
            {
                GamePadState padState = Input.DefaultGamePad.State;
                if ((padState.Buttons & GamePadButton.B) != 0 && isGrounded)
                {
                    Jump();
                }
            }
        }

        private void Jump()
        {
            if (!isGrounded || JumpHeight <= 0) return;

            // Calculate jump velocity to reach desired height
            float jumpVelocity = MathF.Sqrt(2 * -Gravity * JumpHeight);
            verticalVelocity = jumpVelocity;
            isGrounded = false;

            // Play jump animation
            if (animationComponent != null)
            {
                PlayAnimation(AnimationState.Jump);
            }
        }

        private void CheckGroundedState()
        {
            // Perform raycast downward to check if player is on ground
            var startPosition = Entity.Transform.WorldMatrix.TranslationVector;
            var endPosition = startPosition + Vector3.UnitY * -GroundCheckDistance;

            // Use physics simulation for raycast
            var simulation = this.GetSimulation();
            if (simulation != null)
            {
                var hitResult = simulation.Raycast(startPosition, endPosition);
                isGrounded = hitResult.Succeeded;
            }
            else
            {
                // Fallback: assume grounded if Y position is near zero
                isGrounded = startPosition.Y <= 0.1f;
            }
        }

        private void UpdatePhysics()
        {
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Apply gravity if not grounded
            if (!isGrounded)
            {
                verticalVelocity += Gravity * deltaTime;
            }
            else
            {
                verticalVelocity = 0f;
            }

            // Calculate final movement vector
            Vector3 finalMovement = movement;
            finalMovement.Y = verticalVelocity * deltaTime;

            // Transform movement to world space relative to camera direction
            var cameraTransform = Entity.Transform.WorldMatrix;
            var forward = Vector3.Normalize(new Vector3(cameraTransform.Forward.X, 0, cameraTransform.Forward.Z));
            var right = Vector3.Cross(forward, Vector3.UnitY);

            Vector3 worldMovement = (forward * finalMovement.Z + right * finalMovement.X) + Vector3.UnitY * finalMovement.Y;

            // Apply movement through physics or transform
            if (rigidbodyComponent != null)
            {
                // Use physics-based movement
                var currentVelocity = rigidbodyComponent.LinearVelocity;
                rigidbodyComponent.LinearVelocity = new Vector3(worldMovement.X / deltaTime, currentVelocity.Y + finalMovement.Y / deltaTime, worldMovement.Z / deltaTime);
            }
            else
            {
                // Fallback to transform-based movement
                Entity.Transform.Position += worldMovement;
            }
        }

        private void UpdateMovementAnimationState(float inputMagnitude, bool isRunning)
        {
            AnimationState newState = currentAnimationState;

            if (!isGrounded)
            {
                newState = AnimationState.Jump;
            }
            else if (inputMagnitude > 0.1f)
            {
                newState = isRunning ? AnimationState.Run : AnimationState.Walk;
            }
            else
            {
                newState = AnimationState.Idle;
            }

            if (newState != currentAnimationState)
            {
                PlayAnimation(newState);
            }
        }

        private void UpdateAnimations()
        {
            // Animation updates are handled by the animation component
            // Additional custom animation logic can be added here if needed
        }

        private void PlayAnimation(AnimationState state)
        {
            if (animationComponent == null) return;

            currentAnimationState = state;

            // Map animation states to animation names
            string animationName = state switch
            {
                AnimationState.Idle => "Idle",
                AnimationState.Walk => "Walk", 
                AnimationState.Run => "Run",
                AnimationState.Jump => "Jump_Start", // Could cycle through Jump_Start, Jump_Loop, Jump_End
                _ => "Idle"
            };

            // Play the animation with error handling
            try
            {
                if (animationComponent.Animations.ContainsKey(animationName))
                {
                    animationComponent.Play(animationName);
                }
                else
                {
                    // Try alternative animation names
                    var alternatives = new[] { "idle", "Idle", "default", "Default" };
                    foreach (var alt in alternatives)
                    {
                        if (animationComponent.Animations.ContainsKey(alt))
                        {
                            animationComponent.Play(alt);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the game
                Console.WriteLine($"Animation error: {ex.Message}");
            }
        }

        private void UpdateCameraTransform()
        {
            // Get the local coordinate system
            var rotation = Matrix.RotationQuaternion(Entity.Transform.Rotation);

            // Enforce the global up-vector by adjusting the local x-axis
            var right = Vector3.Cross(rotation.Forward, upVector);
            var up = Vector3.Cross(right, rotation.Forward);

            // Stabilize
            right.Normalize();
            up.Normalize();

            // Adjust pitch. Prevent it from exceeding up and down facing.
            var currentPitch = MathUtil.PiOverTwo - MathF.Acos(Vector3.Dot(rotation.Forward, upVector));
            pitch = MathUtil.Clamp(currentPitch + pitch, -MaximumPitch, MaximumPitch) - currentPitch;

            // Apply rotation - Yaw around global up-vector, pitch in local space
            Entity.Transform.Rotation *= Quaternion.RotationAxis(right, pitch) * Quaternion.RotationAxis(upVector, yaw);
        }
    }
}