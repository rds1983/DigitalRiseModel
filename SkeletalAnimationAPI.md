# Skeletal Animation API

This document describes the skeletal animation system in DigitalRiseModel, designed for smooth skeletal animation playback and advanced blending techniques.

## Overview

The skeletal animation system is built around a hierarchical animation tree architecture that allows for complex animation blending and transitions. The main components are:

- **DrModel**: Holds the static skeleton structure, bones, meshes, and animation clips
- **DrModelInstance**: A runtime instance of a model that maintains bone transformations and can be animated
- **AnimationController**: Manages animation playback, state transitions, and crossfading
- **AnimationBlendNode**: A tree node that blends multiple animations together with weighted layers

The system uses a tree-based evaluation model where animations are composed hierarchically, allowing for flexible animation blending at any level.

## Basic Example

### Creating a Model Instance

```csharp
// Load a model (e.g., Mixamo character)
DrModel characterModel = assetManager.LoadModel(graphicsDevice, "Models/mixamo.gltf");

// Create an instance
DrModelInstance characterInstance = new DrModelInstance(characterModel);
```

### Basic Animation Playback

```csharp
// Create animation controller for the instance
AnimationController controller = new AnimationController(characterInstance);

// Start playing an animation by name
controller.StartClip("Idle", isLooped: true);

// Update animation each frame
controller.Update(gameTime.ElapsedGameTime);
```

### Animation Flags

AnimationFlags control playback behavior:

```csharp
public enum AnimationFlags
{
    None = 0,             // Play once forward (default)
    Looped = 1,           // Loop when time exceeds duration
    PlayBackwards = 2     // Play in reverse
}
```

Usage:

```csharp
// Play once, forward
controller.StartClip("Attack", AnimationFlags.None);

// Play with looping
controller.StartClip("Run", AnimationFlags.Looped);

// Play backward (useful for reversing animations)
controller.StartClip("DrawGreatSword", AnimationFlags.PlayBackwards);

// Combine flags
controller.StartClip("Walk", AnimationFlags.Looped | AnimationFlags.PlayBackwards);
```

## Crossfade

Crossfading provides smooth transitions between animations by blending the outgoing and incoming animations over a specified duration.

### StartClip vs CrossfadeToClip

- **StartClip**: Immediately switches to a new animation with no transition
- **CrossfadeToClip**: Smoothly transitions from the current animation to a new one by blending weights

### Crossfade Example

```csharp
// Start the initial animation
controller.StartClip("Idle", isLooped: true);

// Later, smoothly transition to running over 0.1 seconds
controller.CrossfadeToClip("Run", TimeSpan.FromSeconds(0.1), isLooped: true);

// Transition to an attack animation
controller.CrossfadeToClip("Slash", TimeSpan.FromSeconds(0.2), isLooped: false);

// Crossfade using flags
controller.CrossfadeToClip("Draw", TimeSpan.FromSeconds(0.15), AnimationFlags.None);
```

How it works internally:
1. Creates a temporary AnimationBlendNode with two layers
2. Old clip starts at weight 1.0, new clip at weight 0.0
3. Over the fade duration, weights transition to 0.0 and 1.0
4. When transition completes, the blend node is replaced with the new clip

## AnimationBlendNode

AnimationBlendNode is the foundation for complex animation systems. It blends multiple child animations together by weighted mixing, with support for bone filtering and time offsets.

### Blend Layers

Each layer in a blend node contains:
- **Node**: The animation tree node (another blend or clip)
- **Weight**: Blend weight from 0.0 to 1.0 (controls influence)
- **TimeOffset**: Time offset for the animation playback
- **BoneFilter**: Optional filter restricting which bones this layer affects

### Bone Filters

Bone filters allow a layer to affect only specific bones, useful for:
- Playing upper-body animations while lower-body loops
- Playing lower-body locomotion while upper-body attacks
- Selective animation blending

Example:

```csharp
// Create filters
var topFilter = characterModel.CreateBoneFilter("mixamorig:Spine");
var bottomFilter = characterModel.CreateInverseBoneFilter(topFilter);

// topFilter: Spine and all its descendants (upper body)
// bottomFilter: All bones except those in topFilter (lower body)
```

### Complex Blending Example

```csharp
// Create a blend node for run + slash combination
var runSlashAnimation = new AnimationBlendNode(isLooped: true);

// Lower body: running in place
var runLayer = runSlashAnimation.AddLayer(
    characterModel.Animations["RunGreatSword"], 
    weight: 1.0f
);
runLayer.BoneFilter = bottomFilter;

// Upper body: slash attack
var slashLayer = runSlashAnimation.AddLayer(
    characterModel.Animations["SlashGreatSword"], 
    weight: 1.0f
);
slashLayer.BoneFilter = topFilter;

// Use in controller
controller.CrossfadeToClip(runSlashAnimation, TimeSpan.FromSeconds(0.2));
```

## Character Sample Reference

The `CharacterService` class in the Character sample is a comprehensive example of the animation API in action:

**File**: `Samples/DigitalRiseModel.Samples.Character/CharacterService.cs`

### Key Methods

```csharp
// Starting animations
controller.StartClip(string name, AnimationFlags flags);
controller.StartClip(AnimationClip clip, bool isLooped);
controller.StartClip(AnimationTreeNode node);

// Smooth transitions
controller.CrossfadeToClip(string name, TimeSpan duration, AnimationFlags flags);
controller.CrossfadeToClip(AnimationClip clip, TimeSpan duration, bool isLooped);
controller.CrossfadeToClip(AnimationTreeNode node, TimeSpan duration);

// Playback control
controller.Play();
controller.Pause();
controller.Stop();
controller.Update(TimeSpan elapsed);

// Bone management
drModel.CreateBoneFilter(params string[] names);
drModel.CreateInverseBoneFilter(HashSet<int> excluded);
drModel.FindBoneByName(string name);
```
