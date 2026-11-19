# User manual addons
This manual covers all currently available Spline Architect addons.

The currently available official addons are:
- Terrain Tools
- Object Cloning

No other official addons exist at this time.

Addons are separate from the main Spline Architect package and can be deleted from the Addons folder if not needed. Note that removing an addon will also remove all data created by that addon.

## Installation

!> You need to have the base package Spline Architect installed before installing any addon.

Go to Window → Package Manager → My Assets, install your Spline Architect addon, and you're done.
</br></br>
If you've moved the main Spline Architect folder, you will get some errors. This is because the addon assumes the SplineArchitect folder is located in the Assets folder.
You can easily fix this by moving the addon from Assets/SplineArchitect/Addons/YourAddon to Assets/YourPath/SplineArchitect/Addons/YourAddon.

## Updating

!> If you get errors after updating an addon, remember to also check for updates to the base Spline Architect package.

To update, open Unity and go to Window → Package Manager → My Assets, then update the related package.

## Terrain Tools
The terrain tools addon has 3 main features.
- Deform terrain.
- Paint terrain.
- Instantiate objects along a spline.

Terrain deformation and object instantiation has an indestructible work flow. 
Meaning you can always revert back to how it was before.

You can easily create whole worlds by creating mountains, rivers and path in this indestructible workflow.
You do that by combining different noise layers and applying that to your terrain deformation, terrain painting and object instantiation.

### Menus
When you install the Terrain tools addon you will get access to 3 sub menus for the spline.

### Terrain deformation
![Correct mesh](/imgs/menu_terrain.png)<br>
You start deforming the terrain by pressing the "Start deforming" button inside the spline menu, now the terrain will update when you do any changes to the spline.
To stop deform, press the same button again. You can also bake the terrain deformation created by a spline by pressing the “Bake” button.
This will stop any further deformation updates from that spline and apply its deformation to the terrain’s base layer.

The scene needs to be saved before you can start deforming the terrain, because the terrain data is stored in a folder named after the scene (see Terrain data below).

Enable the **Realtime** toggle next to the "Bake" button to update the terrain while you drag control points, sliders or value labels, instead of when you release the mouse button.
This is a global setting that applies to all splines. On large splines, some updates may be skipped while dragging to keep the editor responsive, but the terrain is always updated when you release the mouse button.

If you deform a mountain with multiple paths on it, you should increase the layer value of the paths so Spline Architect knows which spline is on top of the others. 
Otherwise, the paths will disappear when moving the mountain or changing its values.

!> Deforming the terrain at the bottom can cause unwanted deformation buildup. You can fix this by right-clicking on the Terrain component in the Inspector window and press "Raise Terrain by 10%".

#### Deform terrain
- **Deform mode** | Determines how the spline deforms the terrain.
  - **Spline (default)** | Deforms the terrain along the spline.
  - **Fill** | Fills the area inside the spline. The filled area follows the height of the spline, so a flat spline creates a flat area and a sloped spline creates a sloped area. 
  Splines that are not looped are closed with a straight line from the end to the start. The width and blend width are added outside the spline.
  Trim start, trim end, deformation ranges and the Z rotation on control points are only used in Spline mode.
- **Y offset** | Extra height added between the spline and terrain (can be negative).
- **Width** | Width for the terrain deformation.
- **Blend width** | Determines the horizontal extent of terrain deformation, defining the transition area where the terrain shifts from its original to its deformed state. You can also change its curvature by setting different easings.
- **Trim start** | Trims the size of the start of the terrain deformation. Only available in Spline mode.
- **Trim end** | Trims the size of the end of the terrain deformation. Only available in Spline mode.
- **Resolution** | Increase the quality of terrain deformations. Try increasing this value if you get unwanted behaviours when deforming terrain.
- **Layer** | Sets the layer of the spline. Lower layers are deformed before splines with higher layers, so higher layers will always appear on top of lower-layered splines.

#### Deformation noise
- **Noise group** | The noise group you want to be applied to the terrain deformation.
- **Noise space** | What space the noise should originate from.
- **Noise erosion** | Warps the noise in an erosion-like way, either to the right (positive values) or to the left (negative values). This is not a true erosion simulation, but it can produce visually similar results with virtually none of the performance cost of a real calculation.

#### Deformation ranges
- **Range** | The range of the spline that should deform the terrain. You can have up to 16 range values on the same spline.

#### Terrain deformation effects
![Correct mesh](/imgs/menu_terrain_controlpoint.png)<br>
Selecting control points give you access to the menu above, where you can apply different behaviours for each control point on how you want to deform the terrain.

In Fill mode, the scale width and scale blend width use one value instead of separate left and right values, and Z rotation is disabled. 
Saddle skew Y lowers or raises the center of the filled area, positive values create a bowl and negative values create a dome. 
Noise center smooth removes the noise around the deepest point of the filled area.

> If you want to use values outside the slider range, manually enter the desired value in the input field.

#### Runtime terrain deformation
You can also deform the terrain during runtime by calling `spline.DeformTerrain()` and revert it by calling `spline.RevertTerrain()`. 
The deformation uses the same settings as in the Deform terrain menu. See the API Introduction for a script example.

In the editor, changes made to a terrain during Play mode are saved to the terrain's TerrainData asset. 
Because of this, all runtime deformations that have not been reverted are reverted automatically when you exit Play mode.

#### Terrain data
Spline Architect generates binary files during terrain deformation. These files store terrain data, allowing you to work with a non-destructive workflow.
If you delete this data, you may experience unwanted terrain build-up.
These binary files are located inside the 'SplineArchitect-YourSceneName' folder. This folder will automatically be created when necessary.
![Correct mesh](/imgs/data-folder.png)

!> If you rename your scene file you need to rename the SplineArchitect data folder to 'SplineArchitect-YourNewSceneName'.

### Terrain painting
You can have up to 16 paint layers for each spline. 
The order of each layer matters where the layer 1 will be painted first, 2 will be painted second and so on.
You can order them by pressing any of the arrow keys on the specific layer.
You need to have one or more layer palettes assigned to the terrain for creating layers.

![Correct mesh](/imgs/menu_paint.png)
#### General
- **Layer** | The terrain layer pallet you want to paint with.
- **Width** | The painted width that originates from the spline.
- **Offset left** | Offset the painting to the left or right by adding a negative or positive value (the offset is in Unity meters).
- **Strength** | The strength of the layer palette related to other palettes. The value can be above 1 and some times this is preferable.
- **Blend width** | The painted blend width that originates from the spline. 
- **Blend shape** | Applies an easing function that modifies the shape of the blend width.
- **Blend noise** | Applies a noise pattern to the blend width, causing it to vary based on the noise value.

#### Masks
- **Range** | The range of the spline that should be painted.
- **Slope** | Will only paint within this slope value.
- **Slope blend** | How strong the blending of the slope painting will be.
- **Noise** | Applies a noise mask.
- **Invert noise** | Inverts the noise mask.
- **Objects** | Assign a GameObject to this field. All of its child objects will act as a mask, restricting painting to their positions.
- **Probability** | The likelihood that a mask object’s position will be used.
- **Size** | Random size for all object masks. The final size also scales with the original object’s transform.

#### Terrain painting effects
![Correct mesh](/imgs/menu_paint_controlpoint.png)<br>
Selecting control points give you access to the menu above, where you can apply different behaviours for each control point on how you want to paint the terrain.

> If you want to use values outside the slider range, manually enter the desired value in the input field.

### Object instantiation
You can have up to 16 instantiate layers for each spline. 
The order of each layer does not matter. But you can change order if you want.

![Correct mesh](/imgs/menu_instantiate.png)
#### Instances
- **Instance** | Drag a prefab or any GameObject in the hierarchy you want to instantiate. You can create up to 16 instance fields in each layer.
- **Probability** | The probability setting for each instance field. 1 is the highest, if you set it to 0 it will never be instantiated.

#### General
- **Width** | The width of the instantiate are. Originates from the spline.
- **Spacing** | The spacing between each instance.
- **Scale** | Random scale for instances.
- **Offset Y** | Offsets all instances in the Y axel, can be negative.
- **Seed** | A number that controls all randomness. Using the same seed will always generate the same result. Changing the seed produces a different random arrangement.
- **Align normals** | Will align the instance transform with the terrain or whatever the GameObjecet should be instantiated on.
- **Randomness** | The randomness factor.
- **Random rotation** | Random rotation for specific axels.

#### Masks
- **Offset from center** | Skip instancing in a set distance from the center of the spline. 
Can be useful if you want to deform a path in the terrain, but only want to instantiate trees (for example) on the side of the path.
- **Offset from splines** | Of close instances can be other other splines (not the one your instancing from) in the scene.
- **Height** | The height range instances can spawn within. Originates form the spline.
- **Noise** | The noise group you want to use for all instances.
- **Noise range** | The range of the noise value where instances can spawn.
- **Slope** | What slope value instances can be spawned on. Having for example 0 to 20, will only spawn instances on nearly flat to flat ground.
- **Range** | The range of the spline, instances can be spawned from.

#### Object instantiation effects
![Correct mesh](/imgs/menu_instantiate_controlpoint.png)<br>
Selecting control points give you access to the menu above, where you can apply different behaviours for each control point on how you want to instantiate objects along the spline.

> If you want to use values outside the slider range, manually enter the desired value in the input field.

## Object Cloning

Access the Object Cloning sub-menu from the Spline Object menu. To start cloning, add a prefab to any of the prefab fields, then press Clone. 
If you want to adjust individual clones, you can disconnect them by selecting the clone you want to disconnect, opening the Object Cloning menu, and pressing Disconnect. 
You can also disconnect all clones by pressing Disconnect Clones on the clone parent.

When you create new clones by moving the spline, they will inherit settings such as mesh resolution, projection, component mode, and more from existing clones. 
For example, if you want all clones to be projected, select all existing clones and click the **Projection** button. Any new clones will then automatically inherit this setting.

![Correct mesh](/imgs/menu_object_cloning.png)

#### Object Cloning
- **Padding** | Add padding to the start or end of the cloning area (meters).
- **Amount** | Enable the checkbox to the right to set a specific number of clones.
- **Distribution** | Determines how clones are distributed along the spline. The fit options are not used when **Amount** is enabled.
  - **None** | Clones the prefab from start to end without changing its scale or spacing.
  - **Fit using scale** | Scales the clones to fit perfectly along the spline from start to end, while keeping the spacing between them (default).
  - **Fit using spacing** | Keeps the scale of the clones and adjusts the spacing between them so they fit perfectly along the spline from start to end. 
  The relative size of each gap is kept, including gaps from empty prefab fields.
  - **Corrected spacing** | Offsetting non-deformed GameObjects along the X or Y axis can create gaps between them due to how spline space naturally works. 
  This option corrects the spacing so the GameObjects align properly with each other. Should not be used with deformations.
- **Keep prefab link** | If the GameObject originates from a prefab, you can choose to keep its prefab link. 
This means that updating the original prefab will also update all clones created from it. However, keeping prefab links adds some performance overhead to the cloning process.

#### Prefabs
Empty prefab fields can be used to create empty spaces between clones. When an empty field is selected, only the spacing setting will be available. 
These empty fields are also included when randomizing prefabs, allowing you to create randomized gaps between clones.

- **Prefab field** | Create, remove, and reorder prefab fields. You can assign any prefab or GameObject from the scene to these fields. 
- **Offset** | Offsets all clones originating from this prefab along the X, Y, or Z axis. The Z axis applies an incremental offset, while the other axes apply the same offset to all clones.
- **Rotation** | Sets the rotation for all clones.
- **Scale** | Sets the scale for all clones, constrained to all axels using the same axel, or all 3 individual axels.
- **Deform** | Determines whether the clones should be deformed.
- **Incremental UV** | When enabled, applies an incremental UV offset to each clone. You can choose between UV1, UV2, UV3, and UV4, as well as which axes should be affected.
- **Probability** | This setting is only available when **Randomized Prefabs** is enabled. It controls the probability of a prefab being selected relative to the other prefabs.

#### Randomizing
- **Randomize prefabs** | Randomizes the prefab order of all clones, including empty prefab fields with a spacing value greater than 0.
- **Seed** | The seed used to calculate all randomization values.
- **Position** | Randomizes the position offset between the two specified values (meters).
- **Rotation** | Randomizes the rotation offset between the two specified values (degrees).
- **Mirror** | A slider from 0 to 1 that determines the probability of a prefab being mirrored. A value of 1 means a 100% chance.
- **Rotate 180** | A slider from 0 to 1 that determines the probability of a prefab being rotated 180 degrees. A value of 1 means a 100% chance.
- **Strength** | Controls the overall strength of all randomization values. The default value is 1.