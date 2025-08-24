using System;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace MyGame
{
    /// <summary>
    /// A simple test script to validate PlayerCharacterController functionality
    /// </summary>
    public class PlayerCharacterControllerTest : SyncScript
    {
        private PlayerCharacterController playerController;
        private float testTimer = 0f;

        public override void Start()
        {
            base.Start();
            
            // Find the PlayerCharacterController on this or parent entities
            playerController = Entity.Get<PlayerCharacterController>();
            
            if (playerController == null)
            {
                // Try to find it on sibling entities
                var parent = Entity.Transform.Parent;
                if (parent != null)
                {
                    foreach (var child in parent.Children)
                    {
                        playerController = child.Entity.Get<PlayerCharacterController>();
                        if (playerController != null) break;
                    }
                }
            }

            if (playerController != null)
            {
                Log.Info("PlayerCharacterController found and loaded successfully!");
                Log.Info($"Movement Speed: {playerController.MovementSpeed}");
                Log.Info($"Jump Height: {playerController.JumpHeight}");
                Log.Info($"Gravity: {playerController.Gravity}");
            }
            else
            {
                Log.Warning("PlayerCharacterController not found. Make sure it's attached to an entity in the scene.");
            }
        }

        public override void Update()
        {
            if (playerController == null) return;

            testTimer += (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Log periodic status
            if (testTimer >= 5.0f)
            {
                testTimer = 0f;
                LogStatus();
            }
        }

        private void LogStatus()
        {
            var position = Entity.Transform.Position;
            Log.Info($"Player Status - Position: ({position.X:F2}, {position.Y:F2}, {position.Z:F2})");

            // Check if required components are available
            var rigidbody = Entity.Get<Stride.Physics.RigidbodyComponent>();
            var animation = Entity.Get<Stride.Animations.AnimationComponent>();
            var model = Entity.Get<Stride.Rendering.ModelComponent>();

            Log.Info($"Components - Rigidbody: {rigidbody != null}, Animation: {animation != null}, Model: {model != null}");

            // Test input responsiveness
            if (Input.HasKeyboard)
            {
                bool anyMovementInput = Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.A) || 
                                       Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.D);
                
                if (anyMovementInput)
                {
                    Log.Info("Movement input detected!");
                }

                if (Input.IsKeyDown(Keys.Space))
                {
                    Log.Info("Jump input detected!");
                }
            }
        }
    }
}