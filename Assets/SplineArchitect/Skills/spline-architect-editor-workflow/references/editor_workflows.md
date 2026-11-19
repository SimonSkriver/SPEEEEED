# Editor workflows
This document contains the official scripts for the Spline Architect tutorial videos and general videos.
These scripts describe real editor workflows and other relevant information for Spline Architect and its Terrain Tools and Object Cloning addon.

The tutorials covered in this file:
- Quick Start Tutorial (Base Package)
- Terrain Tools Tutorial (Terrain Tools)

The general videos in this file:
- Why Isn't Spline Architect Built on Unity Splines? (Base Package + Addons)

## Quick Start Tutorial (Base Package)
Use this video script for understanding some of the general workflows in the base version of Spline Architect.
Link: https://www.youtube.com/watch?v=vlKJGL2eVKU

This is a quick start tutorial showing how to install and use Spline Architect in Unity 6.
Open the Package Manager, go to My Assets, find Spline Architect, and import it into your project.
If you don’t see the Spline Architect toolbar you can enable it in the overlay menu.

### Setup
To create a spline, press this button. In Unity 2022 the button is located inside a dropdown menu. Now we can create new splines or add control points to the currently selected spline.

### Menus and create spline
Since we’ve selected a control point, two menus are available if we press this button. The first menu contains all settings for the spline, and the second menu contains all settings for the selected control point.
You can switch the menu layout from vertical to horizontal, toggle them on and off with a shortcut, and enable floating menus if you want to move them freely.

### Deform meshes
To deform a GameObject, simply add it as a child to the spline. You can now move, rotate, scale, and duplicate it freely. As you can see, the deformations are not aligned. Align them by pressing the to center button. 
You can change the scale using the Transform’s scale fields, but the position and rotation fields in the Transform will not work. You need to use the position and rotation fields in the Spline Architect menu.
To align multiple deformations, enable the grid snapping feature. If you don’t want to deform a GameObject, change its type to Follower.
Deform a MeshColliders by simply adding a MeshCollider component to the GameObject.

### Create road intersection
Let’s create a road intersection using Spline Connectors. We will use this road piece as our intersection and add one Spline Connector for each road we want to connect. 
The intersection mesh is exactly 8 meters wide and 8 meters long, so each connector should be positioned 4 meters from the center of the mesh, let's add the spline connectors. 
And add the 4 meter offset to each connector. Now we select the control point on the spline you want to connect. As you move it, you will see 4 red dots and 1 white dot. 
The red dots indicate each spline connector, the white dot indicates the closest spline connector. That’s the one the spline will connect to when you press this connect button. 
Let's connect the roads. As you can see the roads are not aligned correctly. We also need to align the rotation of the spline connector. 
Perfect, now we can move the intersection mesh freely, and all attached roads will move with it.

### Spline types
The Spline Type setting changes how the spline behaves. The Static spline is perfect for train tracks, roads and similar things. For more complex shapes like loops, switch to Dynamic.

### Center transform to spline
If you select the spline, you will see that the transform is not centered on the spline. This can make it harder to scale and rotate. Click the 'Select and Center Spline' button to fix this.

## Terrain Tools Tutorial (Addon)
Use this video script for understanding some of the workflows in the Terrain Tools addon.
Link: https://www.youtube.com/watch?v=9Lmiq4WWmgc

### Introduction
In this tutorial, I will show you how the Terrain Tools addon for Spline Architect works. How you can create mountains, paths, 
and more with a non-destructive workflow that works with Unity’s built in terrain system.

### Setup
First, create a terrain. The terrain has a minimum and maximum height range, and right now we are at the bottom. 
Which means that we can’t deform anything below the terrain. It’s also very important to have raised the terrain when deforming splines on top of other splines. 
Else you can get unwanted terrain built up. To fix this, select the Terrain GameObject in the Hierarchy. 
Right-click the Terrain component and raise the terrain by 10%. Now we are good to go.

### Deform Terrain Basics
Create a spline. In the Spline Architect control panel, go to the Deform Terrain section. This is where most of the settings for how the spline deforms the terrain are located. 
Enable terrain deformation by pressing the Start Deforming button. Now the spline will automatically 
update the terrain when you move it or change any values. You can also adjust the terrain on individual control points. 
Select a control point, then press the terrain icon. Here you can scale, rotate, apply saddle skew, and more.

### Creating Mountains and Paths
Let’s create a mountain. Increase the blend width so the mountain gets a proper size. Next, add a noise layer. We have eight different noise types to choose from.
FBM noise and Hybrid Multi Fractal noise work best for mountains. Increase the scale. You can also adjust the noise erosion value. Now we have something that looks like a mountain.
Next, let’s make a path. But there is a problem. If we update the mountain, the path will disappear. To fix this, the path spline needs a higher Layer value than the mountain spline. 
Splines with higher Layer values are always applied on top of lower ones. Create a second path and set its Layer value higher than the first path. Now it will update correctly.
If two splines have the same Layer value, the last one you deform will appear on top. You can also parent the path splines to the mountain spline. Now, when you move the mountain, all paths will move with it.
Since Spline Architect works with Unity’s terrain system, you can use the stamp feature to stamp a mountain and then deform paths along it.

### Fixing intersections
Here we have two intersections where multiple splines deform very uneven terrain. Because of that there is an issue. One intersection looks correct, while the other does not.
If we move the incorrect intersection, it fixes itself, but then the other one becomes incorrect. And if we move that one, the first breaks again, and this keeps repeating. So how do we fix this?
The easiest way is to use Unity’s Set Height tool and create a flat base around the intersection. But we can also solve this in a non-destructive way, which makes it easier to edit later.
Create a new spline and parent it to the intersection mesh. Use this spline as a base and set its Layer value to 0. Then set all other splines to a higher layer.
Now you can move both intersections and they will stay correct. Depending on your use case you might also need to trim the end of the spline that's connected to the intersection and also reduce the scale for the width and blend width on the control point connected to the intersection.

### Unity Terrain Tools Editing and Bake
Another useful thing to know is what happens when you raise or lower the terrain directly on a spline. The spline will automatically correct itself so it stays on top of any changes made with Unity’s terrain tools.
You can’t change the spline’s terrain width because it will always correct itself, but you can change the blend width.
For example, if we raise the terrain on the side of the spline, you can see that after the spline updates, the terrain is raised along with it.
Also remember that any changes you make with Unity’s terrain tools around splines will affect the base layer. 
If we paint on the spline and then press Stop Deforming, you can see that the terrain has been raised underneath it in the base layer.
If you want to raise or lower the terrain without the spline correcting itself, you can press the Bake button on the spline. 
This will apply the spline changes directly to the base layer, but it will remove the non-destructive workflow.

### Pain terrain
Now I will show you how to use the Paint Terrain feature. First, go to the Paint Terrain section on the spline.
Here you will see a warning icon saying that no terrain layers are assigned. So the first step is to assign terrain layers to the terrain.
Select the Terrain GameObject in the Hierarchy. In the Terrain component, go to the Paint Terrain section. Set the active tool to Paint Textures. 
Then press Edit Terrain Layers and add a layer. If you are using multiple terrains, make sure they all have the same terrain layers in the same order.
Now let’s paint the path. Go back to the spline menu and open the Paint Terrain section, then press Add Layer. You can have up to 16 paint layers on a single spline. 
The order matters, and the layers are painted from top to bottom. Next, choose a texture in the Layer option. Increase the width and blend width, then press Paint.
If we undo this, we can also add noise to the paint. Go to the Noise section on the spline and press Create Layer. Then increase the noise scale.
You can see that the noise also affects the terrain deformation. If you want to remove that effect, set the Noise Group to None.
Then go back to the Paint Terrain section and set Blend Noise to A. This applies the noise only to the blend width, which gives more texture variation. Press Paint again.
Now we also want to paint the mountain. Add a paint layer just like before. This time, adjust the Slope value and the Slope Blend value, then press Paint.
For more variation, add another paint layer. On this layer, use a small slope range to create an outline around the cliff texture. Then press Paint.

### Instantiating GameObjects
And now to the last and shortest part of this tutorial: instantiating GameObjects along splines.
Go to the Instantiate GameObjects section on the spline and create a layer. You can have up to 16 layers on each spline.
Now we add tree prefabs to the instance fields. Then increase the offset splines value. This value determines how close instantiations can be to other splines. 
And in this case we don't want any trees on the path carved along the mountain. 
Next Increase the width, spacing, apply random rotation on the y axel and increase the random scale value. Then press Instantiate.
But we only want to spawn trees on less steep areas, so adjust the slope setting. Then press Instantiate again.
