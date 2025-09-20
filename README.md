[![NuGet](https://img.shields.io/nuget/v/DigitalRiseModel.svg)](https://www.nuget.org/packages/DigitalRiseModel/)
[![Build & Publish Beta](https://github.com/DigitalRiseEngine/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml/badge.svg)](https://github.com/DigitalRiseEngine/DigitalRiseModel/actions/workflows/build-and-publish-beta.yml)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

<img width="1202" height="832" alt="image" src="https://github.com/user-attachments/assets/33d25562-ed14-493c-83dd-ff0ac37e622f" />

### Overview
DigitalRiseModel is MonoGame/FNA library that provides alternative API to XNA's 3D modelling.
DigitalRiseModel has following features(that XNA lacks):
* Construct 3d models in code
* Load 3d models from GLTF/GLB in the run-time
* Skeletal animation
* Create primitives(boxes, spheres, toruses, etc) in the run-time

It's important to note that DigitalRiseModel lacks functionality to render models. That responsibility of the developer. However [Samples] section demonstrates how it could be done. It implements very simple rendering engine that is based on XNA stock effects such as BasicEffect and SkinnedEffect.


