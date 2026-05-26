[![NuGet](https://img.shields.io/nuget/v/DigitalRiseModel.MonoGame.svg)](https://www.nuget.org/packages/DigitalRiseModel.MonoGame/)
[![Build & Publish Beta](https://github.com/rds1983/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml/badge.svg)](https://github.com/rds1983/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

<img width="1202" height="832" alt="image" src="https://github.com/user-attachments/assets/614e473f-a153-4fa2-96db-f50fef3dbc41" />

### Overview
DigitalRiseModel is a MonoGame/FNA library that provides an alternative API to XNA's 3D model handling.
DigitalRiseModel has the following features (that XNA lacks):
* Construct 3D models in code
* Load 3D models from GLTF/GLB at runtime
* Skeletal animation
* Create 3D primitives (boxes, spheres, toruses, etc.) at runtime

It is important to note that DigitalRiseModel does not include rendering functionality. That is the responsibility of the developer. 

However, the [Samples](Samples) section demonstrates how this can be done. It implements a simple rendering engine based on XNA stock effects such as BasicEffect and SkinnedEffect.

### Adding Reference For MonoGame
https://www.nuget.org/packages/DigitalRiseModel.MonoGame

### Adding Reference For FNA
Clone the following projects in one folder:
Link|Description
----|-----------
https://github.com/FNA-XNA/FNA|FNA
https://github.com/rds1983/XNAssets|Assets management library
this repo|

Now add each required project's .FNA.Core.csproj to your project

### Usage
Models are loaded through [XNAssets](https://github.com/rds1983/XNAssets).

#### Creating an AssetManager

First, create the AssetManager:
```c#
AssetManager assetManager = AssetManager.CreateFileAssetManager(@"c:\MyGame\Models");
```

#### Loading Models

Load models in GLTF/GLB format:
```c#
DrModel model = assetManager.LoadModel(GraphicsDevice, "myModel.gltf");
```

You can optionally pass `ModelLoadFlags` to control how the model is loaded:
```c#
DrModel model = assetManager.LoadModel(
    GraphicsDevice, 
    "myModel.gltf", 
    ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.ReadableBuffers
);
```

#### Model Load Flags

The following flags can be combined to control model loading behavior:

| Flag | Description |
|------|-------------|
| `None` | No additional options (default). |
| `IgnoreExternalMaterials` | Skip loading materials from external files (e.g., separate texture files referenced in the model). Material structure is still created, but external textures are not loaded. |
| `IgnoreEmbeddedMaterials` | Skip loading materials embedded in the model file (e.g., textures packed inside the GLB file). Material structure is still created, but embedded textures are not loaded. |
| `IgnoreMaterials` | Skip loading all materials (equivalent to `IgnoreExternalMaterials \| IgnoreEmbeddedMaterials`). Material structure is still created, but no textures are loaded. |
| `ReadableBuffers` | Create vertex and index buffers with `BufferUsage.None` instead of `BufferUsage.WriteOnly`, allowing you to read buffer data using `GetData()`. This is useful for analysis or serialization but may have performance implications. |
| `EnsureUVs` | Automatically add a zero-valued UV channel (TextureCoordinate) to meshes that do not have one. This is useful when you need all meshes to have UV coordinates. |

#### DrModel API

The `DrModel` API is similar to the XNA `Model` API, providing access to meshes, bones, animations, and material information.

### Documentation
For detailed information about the skeletal animation API, see [SkeletalAnimationAPI.md](SkeletalAnimationAPI.md).

For detailed information about creating 3D primitives, see [Create3DPrimitivesAPI.md](Create3DPrimitivesAPI.md).

### Samples
The [Samples](Samples) directory is the best way to learn how to work with the library.

Name|Description
----|-----------
BasicEngine|Simple rendering engine that implements tree-like scenes and forward rendering, based on XNA stock effects such as BasicEffect and SkinnedEffect
ModelViewer|Application to load and view 3D models in all supported formats. Skeletal animation is supported
Character|Application that demonstrates how a third-person character controller with skeletal animation can be implemented

### Building From Source For MonoGame
Open DigitalRiseModel.MonoGame.sln in the IDE and run.

### Building From Source For FNA
Clone the following projects in one folder:
Link|Description
----|-----------
https://github.com/FNA-XNA/FNA|FNA
https://github.com/rds1983/XNAssets|Asset management library
https://github.com/FontStashSharp/FontStashSharp|Text rendering library (required for samples)
https://github.com/rds1983/Myra|UI library (required for samples)
this repo|

Open DigitalRiseModel.FNA.Core.sln in the IDE and run.

## Sponsor
If this project is useful for you, you can support development:
- Boosty: https://boosty.to/rds1983
- Telegram Wallet: https://t.me/rds1983

### Crypto

USDT (TON): `UQCQy6tFInPvqinE44zHY4R0rYS3niaBikkqiSyGmyoAMwyO`

TON: `UQCQy6tFInPvqinE44zHY4R0rYS3niaBikkqiSyGmyoAMwyO`

### Credits
* [MonoGame](http://www.monogame.net/)
* [FNA](https://github.com/FNA-XNA/FNA)
* [DigitalRune](https://github.com/DigitalRune/DigitalRune)
* [SharpDX](https://github.com/sharpdx/SharpDX)
* [XNAnimationMG](https://github.com/infinitespace-studios/XNAnimationMG)
