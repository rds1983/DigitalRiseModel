# Create 3D Primitives API

This document describes the 3D primitives system in DigitalRiseModel, designed for creating geometric shapes at runtime without needing to model them in external tools.

## Overview

The 3D primitives system provides a set of static methods to generate common geometric shapes with customizable parameters. Each primitive is returned as either a `DrMeshPart` (a single mesh component) or a `DrMesh` (a complete mesh that can be used directly). The main components are:

- **MeshPrimitives**: Static class containing factory methods for creating primitives
- **DrMeshPart**: A single mesh part containing vertices and indices
- **DrMesh**: A mesh that wraps one or more mesh parts

All primitive creation methods take a `GraphicsDevice` parameter and return XNA-compatible vertex and index buffers suitable for rendering.

## Common Parameters

Most primitive creation methods share these parameters:

- **graphicsDevice**: The XNA `GraphicsDevice` used to create vertex and index buffers
- **uScale**: Scales the U texture coordinates (horizontal). Default is 1.0f
- **vScale**: Scales the V texture coordinates (vertical). Default is 1.0f
- **toLeftHanded**: If true, transforms vertices and indices to left-handed coordinate system. Default is false
- **tessellation**: Controls the level of detail for smooth primitives (sphere, cone, etc.). Higher values = smoother surfaces but more vertices

## Basic Example

### Creating a Simple Box

```csharp
// Create a box with dimensions 2x2x2
var boxMesh = MeshPrimitives.CreateBoxMesh(graphicsDevice, new Vector3(2, 2, 2));

// Create just the mesh part if you need more control
var boxPart = MeshPrimitives.CreateBoxMeshPart(graphicsDevice, new Vector3(2, 2, 2));
```

### Creating a Sphere

```csharp
// Create a sphere with radius 1.0
var sphereMesh = MeshPrimitives.CreateSphereMesh(graphicsDevice, radius: 1.0f);

// With custom tessellation for smoother appearance
var smoothSphere = MeshPrimitives.CreateSphereMesh(graphicsDevice, radius: 1.0f, tessellation: 32);
```

## Available Primitives

### Box / Cube

Creates a rectangular box with six faces.

```csharp
// Create box mesh
DrMesh CreateBoxMesh(GraphicsDevice graphicsDevice, Vector3 size, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

// Create just the mesh part
DrMeshPart CreateBoxMeshPart(GraphicsDevice graphicsDevice, Vector3 size, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `size`: Dimensions of the box (width, height, depth)

**Example:**
```csharp
// Create a 3x2x4 box
var boxMesh = MeshPrimitives.CreateBoxMesh(graphicsDevice, new Vector3(3, 2, 4));
```

### Sphere

Creates a smooth spherical mesh using tessellation.

```csharp
DrMesh CreateSphereMesh(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateSphereMeshPart(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `radius`: Radius of the sphere
- `tessellation`: Resolution of the sphere surface. Minimum is 3. Default 16 provides good balance

**Example:**
```csharp
// Create a detailed sphere
var sphereMesh = MeshPrimitives.CreateSphereMesh(graphicsDevice, radius: 2.0f, tessellation: 32);
```

### Torus (Donut)

Creates a torus with major and minor radii.

```csharp
DrMesh CreateTorusMesh(GraphicsDevice graphicsDevice, float majorRadius = 0.5f, 
    float minorRadius = 0.16666f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateTorusMeshPart(GraphicsDevice graphicsDevice, float majorRadius = 0.5f, 
    float minorRadius = 0.16666f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `majorRadius`: Radius of the main ring
- `minorRadius`: Radius of the tube
- `tessellation`: Resolution. Minimum is 3

**Example:**
```csharp
// Create a torus with major radius 2.0 and tube radius 0.5
var torusMesh = MeshPrimitives.CreateTorusMesh(graphicsDevice, majorRadius: 2.0f, minorRadius: 0.5f);
```

### Cylinder

Creates a cylindrical mesh with optional top/bottom caps.

```csharp
DrMesh CreateCylinderMesh(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateCylinderMeshPart(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `height`: Height of the cylinder
- `diameter`: Diameter of the circular base
- `tessellation`: Number of sides around the cylinder. Minimum is 3

**Example:**
```csharp
// Create a cylinder 3 units tall with diameter 1
var cylinderMesh = MeshPrimitives.CreateCylinderMesh(graphicsDevice, height: 3.0f, diameter: 1.0f);
```

### Cone

Creates a cone shape.

```csharp
DrMesh CreateConeMesh(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateConeMeshPart(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 32, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `height`: Height of the cone
- `diameter`: Diameter of the base
- `tessellation`: Number of sides around the cone. Minimum is 3

**Example:**
```csharp
// Create a cone 2 units tall with base diameter 1
var coneMesh = MeshPrimitives.CreateConeMesh(graphicsDevice, height: 2.0f, diameter: 1.0f);
```

### Capsule

Creates a capsule shape (cylinder with hemispherical ends).

```csharp
DrMesh CreateCapsuleMesh(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 16, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateCapsuleMeshPart(GraphicsDevice graphicsDevice, float height = 1.0f, 
    float diameter = 1.0f, int tessellation = 16, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `height`: Total height including the hemispheres
- `diameter`: Diameter of the cylindrical body
- `tessellation`: Resolution of the hemisphere ends

**Example:**
```csharp
// Create a capsule shape for a character collision representation
var capsuleMesh = MeshPrimitives.CreateCapsuleMesh(graphicsDevice, height: 2.0f, diameter: 0.8f);
```

### Plane

Creates a flat rectangular plane.

```csharp
DrMesh CreatePlaneMesh(GraphicsDevice graphicsDevice, Vector2 size, 
    int tessellationX = 1, int tessellationY = 1, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreatePlaneMeshPart(GraphicsDevice graphicsDevice, Vector2 size, 
    int tessellationX = 1, int tessellationY = 1, 
    float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `size`: Width and height of the plane
- `tessellationX`: Subdivision count along X axis
- `tessellationY`: Subdivision count along Y axis

**Example:**
```csharp
// Create a 10x10 ground plane
var planeMesh = MeshPrimitives.CreatePlaneMesh(graphicsDevice, new Vector2(10, 10));

// Create a subdivided plane for deformation or detailed terrain
var detailedPlane = MeshPrimitives.CreatePlaneMesh(graphicsDevice, new Vector2(10, 10), 
    tessellationX: 10, tessellationY: 10);
```

### Disc

Creates a flat disc (similar to a cylinder with height 0).

```csharp
DrMesh CreateDiscMesh(GraphicsDevice graphicsDevice, float diameter = 1.0f, 
    int tessellation = 32, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateDiscMeshPart(GraphicsDevice graphicsDevice, float diameter = 1.0f, 
    int tessellation = 32, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `diameter`: Diameter of the disc
- `tessellation`: Number of segments around the perimeter

**Example:**
```csharp
// Create a circular platform
var discMesh = MeshPrimitives.CreateDiscMesh(graphicsDevice, diameter: 5.0f);
```

### Teapot

Creates the famous Utah teapot mesh (primarily for testing and demonstration).

```csharp
DrMesh CreateTeapotMesh(GraphicsDevice graphicsDevice, float size = 1.0f, 
    int tessellation = 8, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateTeapotMeshPart(GraphicsDevice graphicsDevice, float size = 1.0f, 
    int tessellation = 8, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `size`: Scale factor for the teapot
- `tessellation`: Resolution of the curved surfaces

**Example:**
```csharp
// Create a teapot for testing
var teapotMesh = MeshPrimitives.CreateTeapotMesh(graphicsDevice, size: 2.0f);
```

### GeoSphere

Creates a sphere using geodesic subdivision (icosahedron-based), resulting in more uniform vertex distribution.

```csharp
DrMesh CreateGeoSphereMesh(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 3, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateGeoSphereMeshPart(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 3, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `radius`: Radius of the sphere
- `tessellation`: Subdivision level. Higher values = smoother. Minimum is 1

**Example:**
```csharp
// Create a geosphere with better uniform distribution
var geoSphereMesh = MeshPrimitives.CreateGeoSphereMesh(graphicsDevice, radius: 1.0f, tessellation: 4);
```

### Hemisphere

Creates half of a sphere.

```csharp
DrMesh CreateHemisphereMesh(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)

DrMeshPart CreateHemisphereMeshPart(GraphicsDevice graphicsDevice, float radius = 0.5f, 
    int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
```

**Parameters:**
- `radius`: Radius of the hemisphere
- `tessellation`: Resolution of the surface

**Example:**
```csharp
// Create a dome shape
var hemisphereMesh = MeshPrimitives.CreateHemisphereMesh(graphicsDevice, radius: 3.0f);
```

## Line Primitives

Line primitives create wireframe versions of shapes for visualization and debugging:

- **CreateBoxLinesMesh/Part**: Wireframe box
- **CreatePlaneLinesMesh/Part**: Wireframe plane
- **CreateCircleLinesMesh/Part**: Circle outline
- **CreateConeLinesMesh/Part**: Wireframe cone
- **CreateCylinderLinesMesh/Part**: Wireframe cylinder
- **CreateHemisphereLinesMesh/Part**: Wireframe hemisphere

These use the same parameters as their solid counterparts but produce line primitives instead of filled meshes.

```csharp
// Example: Create a wireframe box for debugging
var debugBox = MeshPrimitives.CreateBoxLinesMesh(graphicsDevice, new Vector3(2, 2, 2));
```

## Texture Coordinates

Most primitives support texture coordinate scaling via `uScale` and `vScale`:

```csharp
// Create a box with texture coordinates scaled 2x2 (repeats texture 4 times)
var boxMesh = MeshPrimitives.CreateBoxMesh(graphicsDevice, 
    new Vector3(1, 1, 1), 
    uScale: 2.0f, 
    vScale: 2.0f);
```

## Coordinate Systems

By default, primitives are created in right-handed coordinate system (XNA standard). Use `toLeftHanded: true` if your rendering system requires left-handed coordinates:

```csharp
// Create sphere for left-handed rendering
var leftHandedSphere = MeshPrimitives.CreateSphereMesh(graphicsDevice, 
    radius: 1.0f, 
    toLeftHanded: true);
```

## Performance Considerations

- **Tessellation/Complexity**: Higher tessellation values create smoother surfaces but increase vertex count. Use appropriate values for your use case
- **Creation Time**: Primitive creation is reasonably fast but should typically be done at load time, not per-frame
- **Memory**: Each primitive stores all vertices and indices in GPU buffers. Reuse mesh instances instead of creating new ones each frame
- **DrMesh vs DrMeshPart**: Use `DrMesh` for complete, ready-to-render meshes. Use `DrMeshPart` when you need finer control or plan to combine multiple parts

## Advanced Usage

### Combining Primitives

You can create complex shapes by combining multiple primitives:

```csharp
// Create a simple robot: box body + sphere head + cylinder arms
var bodyMesh = MeshPrimitives.CreateBoxMesh(graphicsDevice, new Vector3(1, 2, 0.5f));
var headMesh = MeshPrimitives.CreateSphereMesh(graphicsDevice, radius: 0.4f);
var armMesh = MeshPrimitives.CreateCylinderMesh(graphicsDevice, height: 1.5f, diameter: 0.2f);

// Position and render each part with appropriate transformations
```

### Creating Procedural Environments

Use primitives to generate simple test environments:

```csharp
// Create a simple test scene
var ground = MeshPrimitives.CreatePlaneMesh(graphicsDevice, new Vector2(100, 100), 
    tessellationX: 20, tessellationY: 20);

var obstacles = new[] {
    MeshPrimitives.CreateBoxMesh(graphicsDevice, new Vector3(2, 2, 2)),
    MeshPrimitives.CreateSphereMesh(graphicsDevice, radius: 1.5f),
    MeshPrimitives.CreateCylinderMesh(graphicsDevice, height: 3, diameter: 1)
};
```

### Debug Visualization

Use wireframe primitives for debugging:

```csharp
// Visualize bounding boxes
var boundingBoxVisualization = MeshPrimitives.CreateBoxLinesMesh(graphicsDevice, boundingBoxSize);

// Visualize collision shapes
var collisionShape = MeshPrimitives.CreateCapsuleLinesMesh(graphicsDevice, characterHeight, characterWidth);
```

## Character Sample Reference

The samples demonstrate practical use of primitives:

**Files**: 
- `Samples/DigitalRiseModel.Samples.BasicEngine/` - Contains rendering code for primitives
- `Samples/DigitalRiseModel.Samples.ModelViewer/` - Shows how to render created meshes

Both samples implement rendering using XNA's BasicEffect or SkinnedEffect, showing how to properly render the primitives created by this API.

## Coordinate System Details

Primitives follow these conventions:

- **Axes**: X (right), Y (up), Z (forward) in right-handed system
- **Centered**: Most primitives are centered at the origin
- **Normals**: Calculated for proper lighting (point outward from surface)
- **Texture Mapping**: U (horizontal), V (vertical) with coordinates from 0-1 (before scaling)
