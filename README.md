[![NuGet](https://img.shields.io/nuget/v/DigitalRiseModel.MonoGame.svg)](https://www.nuget.org/packages/DigitalRiseModel.MonoGame/)
[![Build & Publish Beta](https://github.com/rds1983/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml/badge.svg)](https://github.com/rds1983/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

<img width="1202" height="832" alt="image" src="https://github.com/user-attachments/assets/614e473f-a153-4fa2-96db-f50fef3dbc41" />

### Overview
DigitalRiseModel is a MonoGame/FNA library that provides an alternative API to XNA's 3D modelling.
DigitalRiseModel has the following features (that XNA lacks):
* Construct 3D models in code
* Load 3D models from GLTF/GLB at runtime
* Skeletal animation
* Create 3D primitives (boxes, spheres, toruses, etc.) at runtime

It's important to note that DigitalRiseModel does not include rendering functionality. That is the responsibility of the developer. 

However, the [Samples](Samples) section demonstrates how this can be done. It implements a simple rendering engine based on XNA stock effects such as BasicEffect and SkinnedEffect.

### Adding Reference For MonoGame
https://www.nuget.org/packages/DigitalRiseModel.MonoGame

### Adding Reference For FNA
Clone following projects in one folder:
Link|Description
----|-----------
https://github.com/FNA-XNA/FNA|FNA
https://github.com/rds1983/XNAssets|Assets management library
this repo|

Now add every required project .FNA.Core.csproj to your project

### Usage
Models are loaded through [XNAssets](https://github.com/rds1983/XNAssets).

Firstly create the AssetManager:
```c#
AssetManager assetManager = AssetManager.CreateFileAssetManager(@"c:\MyGame\Models");
```
Now load the model in the GLTF/GLB format:
```c#
DrModel model = assetManager.LoadModel(GraphicsDevice, "myModel.gltf")
```
The DrModel API is quite similar to the XNA Model API.

### Documentation
For detailed information about the skeletal animation API, see [SkeletalAnimationAPI.md](SkeletalAnimationAPI.md).

### Samples
The [Samples](Samples) directory is the best way to learn how to work with the library.

Name|Description
----|-----------
BasicEngine|Simple rendering engine that implements tree-like scenes and forward rendering, based on XNA stock effects such as BasicEffect and SkinnedEffect
ModelViewer|Application to load and view 3D models in all supported formats. Skeletal animation is supported
Character|Application that demonstrates how a third-person character controller with skeletal animation can be implemented

<<<<<<< HEAD
=======
### Documentation
For detailed information about the skeletal animation API, see [SkeletalAnimationAPI.md](SkeletalAnimationAPI.md).
For detailed information about creating 3D primitives, see [Create3DPrimitivesAPI.md](Create3DPrimitivesAPI.md).

>>>>>>> 3fa114b (Docs updates)
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

### Credits
* [MonoGame](http://www.monogame.net/)
* [FNA](https://github.com/FNA-XNA/FNA)
* [DigitalRune](https://github.com/DigitalRune/DigitalRune)
* [SharpDX](https://github.com/sharpdx/SharpDX)
* [XNAnimationMG](https://github.com/infinitespace-studios/XNAnimationMG)
