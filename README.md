# RadianceCascadesUnity
A simple 2D implementation of Radiance Cascades by Alexander Sannikov in Unity
Radiance Cascades is a smart approach to both 2D and 3D Realtime Global Illumination
The idea is to cast a finite amout of rays in an effective way to get the best approximation of the radiance in each pixel of the scene

In this small post I will cover a bit about the implementation process only and my project structure
If you wanna know more about the actual concept here is the original paper : [Radiance Cascades](https://drive.google.com/file/d/1L6v1_7HY2X-LV3Ofb6oyTIxgEaP4LOI6/view)

The project is based on the 2D Built-In RenderPipeline in Unity 2020.3.15f2 but I'm sure you can upgrade it to any other version of Unity or any other RenderPipeline without getting any errors
This is a bare implementation of the technic, it doesn't include any Ringing fix or Sky radiance ...
I just wanted to present a clear example of the concept

The project is divided into two sections:
* SDF Generation: it generates the drawing sdf and store it in a RenderTexture
* Cascades Compute: which uses this texture to generate the radiance field

The Cascades Compute is done through 3 major steps:

# Computing Cascades
As I said, this approch relies on casting the rays in an effective pattern to approcimate the radiance
I recommend watching this video made by SimonDev which talks about this in a more detailed way : https://youtu.be/3so7xdZHKxw?si=kkuR7l4EZW4KxlvQ
What we usually does is casting a constant number of ray for each pixel like this example : [ShaderToy by DanielHopper](https://www.shadertoy.com/view/4ftXzS) and this is very costing in performance

Let's say that our screen resolution is 1920x1080 and the dimensions of the probes in the cascade0 is 2x2
(in the project I changed the resolution of the cascades for the sake of performance in mobile tests but let's assume that the cascade resolution is the same as screen resolution for simplicity)
so we will have 960x540 probe (which is 518400 probes in total), for each probe we will cast 4 rays in 4 different angles
To store the result of casting those rays, we can use a Texture that have exactly the same resolution of Screen, so each 4 pixels represents a probe and each pixel in those probes represents an angle, and what we store in the texture is the result of casting a ray in that angle (the ray origin is the center of the probe)
The same goes for upper cascades:

