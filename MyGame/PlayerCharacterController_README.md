# Player Character Controller

This document describes how to use the `PlayerCharacterController` in your Stride game project.

## Overview

The `PlayerCharacterController` is a robust character controller that extends the functionality of the basic camera controller with character-specific features including:

- **Physics-based movement** using Stride's physics system
- **Jump mechanics** with ground detection
- **Animation state management** (Idle, Walk, Run, Jump)
- **Collision handling** to prevent clipping through walls/objects
- **WASD movement controls** 
- **Mouse look camera control**

## Setup Instructions

### 1. Replace BasicCameraController

To use the PlayerCharacterController, replace the `BasicCameraController` component on your Camera entity with `PlayerCharacterController`.

In your scene (.sdscene file), change:
```yaml
6b485efd73db267778a12aabbde731af: !MyGame.BasicCameraController,MyGame
```

to:
```yaml
6b485efd73db267778a12aabbde731af: !MyGame.PlayerCharacterController,MyGame
```

### 2. Add Required Components

For full functionality, your player entity should have:

**Required:**
- `TransformComponent` (automatically present)
- `CameraComponent` (for camera functionality)

**Optional but Recommended:**
- `RigidbodyComponent` (for physics-based movement)
- `StaticColliderComponent` or `RigidbodyComponent` with collider (for collision)
- `AnimationComponent` (for character animations)
- `ModelComponent` (for character model)

### 3. Configure Physics (Optional)

If using physics-based movement:

1. Add a `RigidbodyComponent` to your player entity
2. Set the RigidBodyType to `Dynamic`
3. Add appropriate collider shapes (CapsuleColliderShape recommended for characters)
4. Ensure your ground and walls have collision components

### 4. Setup Animations (Optional)

If using animations:

1. Add an `AnimationComponent` to your player entity
2. Ensure you have the following animations available:
   - "Idle" - for standing still
   - "Walk" - for walking movement
   - "Run" - for running (with shift held)
   - "Jump_Start" - for jumping (can also use "Jump_Loop", "Jump_End")

## Configuration Options

The PlayerCharacterController provides several configurable properties:

### Movement Settings
- **MovementSpeed** (5.0f) - Base movement speed
- **RunSpeedMultiplier** (2.0f) - Speed multiplier when running (shift held)
- **JumpHeight** (1.5f) - How high the player can jump
- **Gravity** (-25.0f) - Gravity force applied when not grounded

### Physics Settings
- **GroundCheckDistance** (0.1f) - Distance to raycast for ground detection

### Camera Settings
- **MouseRotationSpeed** (1.0, 1.0) - Mouse sensitivity for camera rotation
- **KeyboardRotationSpeed** (3.0, 3.0) - Keyboard rotation speed (numpad)
- **TouchRotationSpeed** (1.0, 0.7) - Touch rotation sensitivity

### Input Settings
- **Gamepad** (false) - Enable gamepad support

## Controls

### Keyboard Controls
- **W, A, S, D** - Movement (forward, left, backward, right)
- **Arrow Keys** - Alternative movement
- **Space** - Jump (when grounded)
- **Left/Right Shift** - Run (hold while moving)
- **Right Mouse Button** - Camera look (hold and drag)
- **Numpad 2,4,6,8** - Camera rotation

### Gamepad Controls (when enabled)
- **Left Stick** - Movement
- **Right Stick** - Camera rotation
- **B Button** - Jump
- **A, Left/Right Shoulder** - Run

## Animation States

The controller automatically manages animation states:

1. **Idle** - When not moving and grounded
2. **Walk** - When moving at normal speed
3. **Run** - When moving with shift held
4. **Jump** - When in the air (not grounded)

## Physics Integration

The controller supports both physics-based and transform-based movement:

### With Physics (Recommended)
- Uses `RigidbodyComponent` for realistic movement
- Collision detection prevents clipping
- Proper gravity simulation
- Interaction with other physics objects

### Without Physics (Fallback)
- Direct transform manipulation
- Basic ground detection
- No collision detection
- Lighter performance impact

## Troubleshooting

### Character doesn't move
- Ensure the controller is attached to the correct entity
- Check that input is being received (test with BasicCameraController first)
- Verify MovementSpeed is not set to 0

### Character falls through ground
- Ensure ground has collision components
- Check that physics simulation is running
- Verify GroundCheckDistance is appropriate for your scene scale

### Animations don't play
- Ensure AnimationComponent is attached
- Verify animation names match the expected names
- Check that animation files are properly imported

### Camera doesn't rotate
- Ensure mouse input is enabled
- Check MouseRotationSpeed values
- Verify the camera entity has the controller attached

## Example Entity Setup

Here's an example of a properly configured player entity:

```yaml
Entity:
    Name: Player
    Components:
        TransformComponent:
            Position: {X: 0, Y: 1, Z: 0}
        CameraComponent:
            Projection: Perspective
        PlayerCharacterController:
            MovementSpeed: 5.0
            JumpHeight: 1.5
            MouseRotationSpeed: {X: 1.0, Y: 1.0}
        RigidbodyComponent:
            RigidBodyType: Dynamic
            Mass: 1.0
            ColliderShapes:
                - !CapsuleColliderShapeDesc
                    Height: 2.0
                    Radius: 0.5
        ModelComponent:
            Model: "Models/mannequinModel"
        AnimationComponent:
            Animations:
                Idle: "Animations/Idle"
                Walk: "Animations/Walk"
                Run: "Animations/Run"
                Jump_Start: "Animations/Jump_Start"
```