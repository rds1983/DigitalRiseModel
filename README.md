[![NuGet](https://img.shields.io/nuget/v/DigitalRiseModel.MonoGame.svg)](https://www.nuget.org/packages/DigitalRiseModel.MonoGame/)
[![Build & Publish Beta](https://github.com/DigitalRise3D/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml/badge.svg)](https://github.com/DigitalRiseEngine/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

<img width="1202" height="832" alt="image" src="https://github.com/user-attachments/assets/33d25562-ed14-493c-83dd-ff0ac37e622f" />

### Overview
DigitalRiseModel is MonoGame/FNA library that provides alternative API to XNA's 3D modelling.
DigitalRiseModel has following features(that XNA lacks):
* Construct 3D models in code
* Load 3D models from GLTF/GLB and [G3DJ](https://xoppa.github.io/blog/loading-models-using-libgdx/) in the run-time
* Skeletal animation
* Create 3D primitives(boxes, spheres, toruses, etc) in the run-time

It's important to note that DigitalRiseModel lacks functionality to render models. That is the responsibility of the developer. 

However [Samples](Samples) section demonstrates how it could be done. It implements simple rendering engine that is based on XNA stock effects such as BasicEffect and SkinnedEffect.

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
Now load the model in the GLTF/GLB and [G3DJ](https://xoppa.github.io/blog/loading-models-using-libgdx/) formats:
```c#
NrmModel model = assetManager.LoadModel(GraphicsDevice, "myModel.gltf")
```

### Samples
Right now, [Samples](Samples) is the best way to learn how to work with the library.

Name|Description
----|-----------
BasicEngine|Simple rendering engine that implements tree-like scenes and forward rendering, based on XNA stock effects such as BasicEffect and SkinnedEffect
ModelViewer|Application to load and view 3D models in all supported formats. Skeletal animation is supported
ThirdPerson|Application that demonstrates how simple third person controller could be done

### Building From Source For MonoGame
Open DigitalRiseModel.MonoGame.sln in the IDE and run.

### Building From Source For FNA
Clone following projects in one folder:
Link|Description
----|-----------
https://github.com/FNA-XNA/FNA|FNA
https://github.com/rds1983/XNAssets|Asset management library
https://github.com/FontStashSharp/FontStashSharp|Text rendering library(required for samples)
https://github.com/rds1983/Myra|UI library(required for samples)
this repo|

Open DigitalRiseModel.FNA.Core.sln in the IDE and run.

### Credits
* [MonoGame](http://www.monogame.net/)
* [FNA](https://github.com/FNA-XNA/FNA)
* [DigitalRune](https://github.com/DigitalRune/DigitalRune)
* [SharpDX](https://github.com/sharpdx/SharpDX)
* [XNAnimationMG](https://github.com/infinitespace-studios/XNAnimationMG)
