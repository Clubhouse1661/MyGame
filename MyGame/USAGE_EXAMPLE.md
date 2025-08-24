## PlayerCharacterController Usage Example

This example shows how to replace the BasicCameraController with the new PlayerCharacterController in your MainScene.

### Step 1: Backup Original Scene
Before making changes, backup your original MainScene.sdscene file.

### Step 2: Modify Scene Configuration

In your `Assets/MainScene.sdscene` file, find the Camera entity section and replace:

```yaml
6b485efd73db267778a12aabbde731af: !MyGame.BasicCameraController,MyGame
    Id: b158c6ae-5c1c-43c5-a200-02bdaf2e6606
    Gamepad: false
    KeyboardMovementSpeed: {X: 5.0, Y: 5.0, Z: 5.0}
    TouchMovementSpeed: {X: 0.7, Y: 0.7, Z: 0.3}
    SpeedFactor: 5.0
    KeyboardRotationSpeed: {X: 3.0, Y: 3.0}
    MouseRotationSpeed: {X: 1.0, Y: 1.0}
    TouchRotationSpeed: {X: 1.0, Y: 0.7}
```

With:

```yaml
6b485efd73db267778a12aabbde731af: !MyGame.PlayerCharacterController,MyGame
    Id: b158c6ae-5c1c-43c5-a200-02bdaf2e6606
    Gamepad: false
    MovementSpeed: 5.0
    RunSpeedMultiplier: 2.0
    JumpHeight: 1.5
    Gravity: -25.0
    GroundCheckDistance: 0.1
    MouseRotationSpeed: {X: 1.0, Y: 1.0}
    KeyboardRotationSpeed: {X: 3.0, Y: 3.0}
    TouchRotationSpeed: {X: 1.0, Y: 0.7}
```

### Step 3: Test Basic Movement

After making this change, you should be able to:

1. **Move**: Use WASD keys to move around
2. **Run**: Hold Shift while moving to run faster
3. **Jump**: Press Space to jump (basic jump without full physics)
4. **Camera**: Hold right mouse button and move mouse to look around

### Step 4: Add Physics (Optional)

For full physics support, add these components to your Camera entity:

```yaml
# Add RigidbodyComponent for physics-based movement
rigidBodyComponent: !RigidbodyComponent
    Id: [generate-new-id]
    RigidBodyType: Dynamic
    Mass: 1.0
    ColliderShapes:
        - !CapsuleColliderShapeDesc
            Height: 1.8
            Radius: 0.3

# Add collision shape for the ground (if not already present)
# On your Ground entity, add:
staticColliderComponent: !StaticColliderComponent
    Id: [generate-new-id]
    ColliderShapes:
        - !BoxColliderShapeDesc
            Size: {X: 10.0, Y: 0.1, Z: 10.0}
```

### Step 5: Add Character Model and Animations (Optional)

To use a character model instead of just a camera:

```yaml
# Add ModelComponent to display character
modelComponent: !ModelComponent
    Id: [generate-new-id]
    Model: Models/mannequinModel

# Add AnimationComponent for character animations
animationComponent: !AnimationComponent
    Id: [generate-new-id]
    Animations:
        Idle: Animations/Idle
        Walk: Animations/Walk
        Run: Animations/Run
        Jump_Start: Animations/Jump_Start
```

### Controls Summary

- **WASD / Arrow Keys**: Move
- **Space**: Jump
- **Shift**: Run (while moving)
- **Right Mouse + Move**: Camera look
- **Numpad 2,4,6,8**: Camera rotation (alternative)

### Troubleshooting

1. **Character doesn't jump**: Make sure there's a ground surface to detect. Without physics, the ground detection falls back to Y position check.

2. **Movement feels strange**: Adjust MovementSpeed, RunSpeedMultiplier values in the controller configuration.

3. **Camera too sensitive**: Reduce MouseRotationSpeed values.

4. **Physics issues**: Ensure both player and ground have appropriate collision components.

This setup provides a solid foundation for a character controller that can be extended with additional features as needed.