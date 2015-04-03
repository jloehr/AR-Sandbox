# AR-Sandbox

Visually augmented sandbox using Kinect and Unity3D.
Inspiried by [SARndbox](http://idav.ucdavis.edu/~okreylos/ResDev/SARndbox/)

See the Project in action on [Youtube](https://www.youtube.com/playlist?list=PLiYkNDKSkCd4wgPixZNUtXTdhxdEhq7Vk).
Otherwise there are some images in the project "Images" folder.

## Dependency 
* [Unity3D 4.6.0](http://unity3d.com/)
* Kinect SDK 1.8
* Unity 3D Kinect Pugin by [Carnegie Mellon University](http://wiki.etc.cmu.edu/unity3d/index.php/Microsoft_Kinect_-_Microsoft_SDK)
* [AForge.NET](http://www.aforgenet.com/)

The Plugin as well as AForge are already included in the project.

## Content

The base directory of this repository is the Unity3D project.
In the "Project Images" folder are some additional photos andoots.

## Project Content

The main scene of this project is "DepthMesh".

### DepthMesh GameObject

In this scene the "DepthMesh" GameObject represents the Sandbox through a Mesh.

The "DepthMesh" script creates the Mesh and updates it with the data from the Kinect. 
The properties Width, Height and Offset are used for cropping.
MinDepth and MaxDepth are used to limit the Kinect input values.
MeshHeight is the height of the Mesh in the Unity Space.

The Mesh is positioned at the origin of the Unity Space (0, 0, 0) and streches on the X,Y-Plane in the positiv direction.
The Mesh height is in Z-Axis direction. The Mesh is facing in +Z-Axis direction, whereas the camera is looking at -Z-Axis Direction.

### SharkPopulation GameObject

The "SharkPopulation" GameObject is a simple feature to have some shark swimming through the water layer.
The "SharkController" has some properties to adjust.
SharkLayer and SharkLayerWidth defines where the Shark are. Should be somewhere around the water level of the Shader.
And SharksPerSquare and MaxSharks are the values for the amount of Sharks created.

### Shader

Use the default Unity Defuse Shader to see the Mesh in Unity with some shadowing.
Vertex Color was our old testing Shader for colors.
Terrain Shader is our self written Shader to display the Mesh with actual Textures.
You can adjust the Terrain Shader by setting the Water Level (World Space) and the Layer Width, as well as the Blending between two layer.

## Setup

AForge needs the System.Drawing.dll. Copy it from System32 into the project folder and into the Unity Editor folder.

For the Kinect Plugin see their site for instructions.
However have the Kinect SDK 1.8 installed and the Kinect pluged in before startung Unity3D.

Arrange the Projector and the Kinect so they cover the sandbox. We used a giant mirror on the wall (see the Project Images).
Open the "DepthMesh" scene in Unity and check "Maximize at Play" button. Hit play and get the "Play" View on the Projector.

## Calibration

Originally we tried to use QR-Marker. Since they didn't work with our mirror, we only have trivial manual calibration.
The MinDepth and MaxValue values as well as the Terrain Shader values are depending on the distance of the Kinect to the Sandbox.

### Cropping

Place 4 Clicks in the Play-View clockwise, starting Top-Left, to crop the Mesh.
Otherwise you can adjust the cropping values directly on the DepthMesh component on the DepthMesh GameObject.

### Positioning the Image

Use WASD to move the Mesh on the screen. With QE you can scale the Mesh up and down.
(In fact you move the camera and adjust its size)
