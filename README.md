[![NuGet](https://img.shields.io/nuget/v/NursiaModel.MonoGame.svg)](https://www.nuget.org/packages/NursiaModel.MonoGame/)
[![Build & Publish Beta](https://github.com/NursiaEngine/NursiaModel/actions/workflows/build-and-publish-beta.yml/badge.svg)](https://github.com/NursiaEngine/NursiaModel/actions/workflows/build-and-publish-beta.yml)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

<img width="1202" height="832" alt="image" src="https://github.com/user-attachments/assets/33d25562-ed14-493c-83dd-ff0ac37e622f" />

### Overview
NursiaModel is MonoGame/FNA library that provides alternative API to XNA's 3D modelling.
NursiaModel has following features(that XNA lacks):
* Construct 3D models in code
* Load 3D models from GLTF/GLB in the run-time
* Skeletal animation
* Create 3D primitives(boxes, spheres, toruses, etc) in the run-time

It's important to note that NursiaModel lacks functionality to render models. That is the responsibility of the developer. 

However [Samples](Samples) section demonstrates how it could be done. It implements simple rendering engine that is based on XNA stock effects such as BasicEffect and SkinnedEffect.

### Adding Reference
NursiaModel consists of following assemblies(click on the name for MonoGame nuget link):
Name|Description
----|-----------
[NursiaModel](https://www.nuget.org/packages/NursiaModel.MonoGame)|Base 3D modelling API and 3D primitives
[NursiaModel.Gltf](https://www.nuget.org/packages/NursiaModel.Gltf.MonoGame)|Loading 3D models from GLTF/GLB

See [this](https://github.com/NursiaEngine/NursiaModel/wiki/Adding-Reference-For-FNA-Project) on how to reference the library in the FNA project.

### Samples
Right now, [Samples](Samples) is the best way to learn how to work with the library.

Name|Description
----|-----------
BasicEngine|Simple rendering engine that implements tree-like scenes and forward rendering, based on XNA stock effects such as BasicEffect and SkinnedEffect
ModelViewer|Application to load and view 3D models in GLTF/GLB format. Skeletal animation is supported
ThirdPerson|Application that demonstrates how simple third person controller could be done

### Building From Source For MonoGame
Open NursiaModel.MonoGame.sln in the IDE and run.

### Building From Source For FNA
Clone following projects in one folder:
Link|Description
----|-----------
https://github.com/FNA-XNA/FNA|FNA
https://github.com/rds1983/XNAssets|Asset management library
https://github.com/FontStashSharp/FontStashSharp|Text rendering library(required for samples)
https://github.com/rds1983/Myra|UI library(required for samples)
this repo|

Open NursiaModel.FNA.Core.sln in the IDE and run.

### Credits
* [MonoGame](http://www.monogame.net/)
* [FNA](https://github.com/FNA-XNA/FNA)
* [DigitalRune](https://github.com/DigitalRune/DigitalRune)
* [SharpDX](https://github.com/sharpdx/SharpDX)
* [XNAnimationMG](https://github.com/infinitespace-studios/XNAnimationMG)
