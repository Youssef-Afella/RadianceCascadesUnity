# RadianceCascades Unity
A simple 2D implementation of Radiance Cascades by Alexander Sannikov in Unity<br/>
Radiance Cascades is a smart approach to both 2D and 3D Realtime Global Illumination<br/>
The idea is to cast a finite amout of rays in an effective way to get the best approximation of the radiance in each pixel of the scene<br/>
![Capture1](https://github.com/user-attachments/assets/f1d4755c-bf38-4af3-99c6-e9aefe41bb8f)

In this small post I will cover a bit about the implementation process only<br/>
If you wanna know more about the actual concept here is the original paper : [Radiance Cascades](https://drive.google.com/file/d/1L6v1_7HY2X-LV3Ofb6oyTIxgEaP4LOI6/view)

The project is based on the 2D Built-In RenderPipeline in Unity 2020.3.15f2 but I'm sure you can upgrade it to any other version of Unity or any other RenderPipeline without getting any errors<br/>

This is a bare implementation of the technic, it doesn't include any Ringing fix or Dark Areas Fix ...<br/>
I just wanted to present a clear example of the concept<br/>

The project is divided into two sections:
* SDF Generation: it generates the drawing sdf and store it in a RenderTexture
* Cascades Compute: which uses this texture to generate the Radiance field

The Cascades Compute is done through 3 major steps:

# Computing Cascades
As I said, this approch relies on casting the rays in an effective pattern to approcimate the radiance<br/>
I recommend watching this video made by SimonDev which talks about this in a more detailed way : https://youtu.be/3so7xdZHKxw?si=kkuR7l4EZW4KxlvQ<br/>
What we usually does is casting a constant number of ray for each pixel like this example : [ShaderToy by DanielHopper](https://www.shadertoy.com/view/4ftXzS) and this is VERY costing in performance<br/>

So how do we compute:
Let's say that our screen resolution is 1920x1080 and the dimensions of the probes in the cascade0 is 2x2<br/>
(in the project I changed the resolution of the cascades for the sake of performance in mobile tests but let's assume that the cascade resolution is the same as screen resolution for simplicity)<br/>
so we will have 960x540 probe (which is 518400 probes in total), for each probe we will cast 4 rays in 4 different angles.<br/>

To store the result of casting those rays, we can use a Texture that have exactly the same resolution of Screen, so each 4 pixels represents a probe and each pixel in those probes represents an angle, and what we store in the texture is the result of casting a ray in that angle (the ray origin is the center of the probe)<br/>
The same goes for upper cascades (for cascade1 : dim=4, numOfAngles=16):<br/>
![Im0](https://github.com/user-attachments/assets/52978085-ed39-435c-b941-d22c48ede0f2)
Yaazarai has a better visualisation of this in his repo : https://github.com/Yaazarai/RadianceCascades/tree/main<br/>
(There is another small detail I haven't talkedabout but you will find it in the project)

There is one other thing, we need to make sure to cast the rays in the apropriate Range<br/>
Check Tmpvar article, it has the best visualisation of that : https://tmpvar.com/poc/radiance-cascades/

We do this process for a number of levels and we end up have a stack of textures like this:<br/>
![Im3](https://github.com/user-attachments/assets/68868a48-331c-409d-8eee-8b00b7118b99)

And those are your final cascades ! (there is a formula to know exactly how many cascades you need but it wasn't really working for me)<br/>
You can see how the ray length is increasing with each level also the position where it starts

# Merging Cascades
Now here is the important part, we need to merge our cascades in a way that let them converge to the final radiance.<br/>
The merging starts from the upper cascades to the lower ones (we merge the cascade with level n+1 with n). And the result of merging the two cascades is stored in the lower one to be used in the next interation<br/>

So how do we merge:
Let's say we want to merge the cascade1 with cascade0<br/>
For each probe in the lowerCascade (cascade0) we find the 4 nearest probes to it in the upperCascade (cascade1)<br/>
So now we have 1 lowerProbe(with 4 angles) and 4 upperProbes(with 16 angles each)<br/>
Let's take one of those angles in the lowerProbe and for each upperProbe we find the 4 nearest angles to the one we selected and average the radiance value from those 4 angles<br/>
Now we end up having 4 values from the upperProbes for this one angle from the lowerProbe<br/>
Finally, we bilinearly interpolate between these 4 values based on the lowerProbe position inside the upperProbes<br/>

After calculating the radiance value from the upper probes in this way we need to compose it with the radiance that already exist in the lower probe using this formula :<br/>
_**lowerRadiance.rgb += upperRadiance.rgb * lowerRadiance.a;**_<br/>
_**lowerRadiance.a *= upperRadiance.a;**_<br/>
Here is a schema for what we just covered :<br/>
![Im4](https://github.com/user-attachments/assets/1b207e93-0683-4223-8b53-0af1ad578441)
In pixels it should be like this :<br/>
![Im5](https://github.com/user-attachments/assets/ae3b0d26-564b-4375-b24a-7654cf07705f)

# Radiance Field
With our way of merging, the cascade0 should be holding now all the merged cascades<br/>
The last processing we do is for each probe in the cascade0 we loopover the pixels in this probe and average their values (we sum the color of the pixels in the probe and divide them by 4 in our case)<br/>
This gonna cause the resolution to be halfed so you need to upscale the final result

And there you have it, a cheap way to calculate the Global Illumination in Realtime!
![Capture](https://github.com/user-attachments/assets/d2379d12-7d62-48f5-94f7-630513240805)

# To be added:
* Fixing Rigning Artifact
* Fixing the Dark Areas (you can see it around the screen)
* Adding Sky Radiance
* Transforming the Unity scene to an SDF map (or have a way of implementing it directly into the Unity scene)
* Expanding to 3D (the main idea of this whole thing)

# Resources:
- [Paper by Alexander Sannikov](https://drive.google.com/file/d/1L6v1_7HY2X-LV3Ofb6oyTIxgEaP4LOI6/view)
- [SimonDev Video](https://youtu.be/3so7xdZHKxw?si=Kop3WY-9n88FcHjS) and [Web Implementation](https://github.com/simondevyoutube/Shaders_RadianceCascades)
- [GM Shaders Article](https://mini.gmshaders.com/p/radiance-cascades), they have also a great [Article](https://mini.gmshaders.com/p/radiance-cascades2) on how to optimize the process a lot
- [Tmpvar Visualizations](https://tmpvar.com/poc/radiance-cascades/)
