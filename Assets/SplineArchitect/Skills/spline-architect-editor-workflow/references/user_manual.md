# User manual
This is the official user manual for the Unity editor tool Spline Architect (base package). Documentation for optional addons is provided in a separate Addons Manual.

> Having trouble migrating to Spline Architect 2.1.0? See the [Migration Guide](migration_guide.md)

## Installation

> Works with <b>Unity 2022</b> and <b>6000</b>.

!> Minimum required version is **Unity 2022.3.62**.

Open your unity editor and go to Window → Package Manager → My Assets, and install Spline Architect.
After that you need to press Install/Upgrade in the Warning window. This will install the collection package (or do nothing if its already installed).  
![Spline Architect Logo](/imgs/have_dependency.png)

## Updating

!> If you encounter errors after updating, remove the entire Spline Architect folder and re-import the package(s). Also check for updates to any of your addons!

To update, open Unity and go to Window → Package Manager → My Assets, then update the related package.
After updating, you’ll see the same Warning window that appeared when first installing Spline Architect. Press Install/Upgrade.

## Create splines and control points
You find the "Spline Architect - Create" button in the regular Tools menu. 
When its enabled you can either create new splines or new control points on the selected spline.

Delete control points by selecting them and pressing "Delete" or "Cmd + Backspace" on Mac computers.

**Unity 6000**<br>
![Create spline button](/imgs/create_spline_button_6000.png)

**Unity 2022**<br>
![Create spline button](/imgs/create_spline_button_2022.png)

## Scale control points
Select a control point and use Unity's Scale tool. The X and Y handles change the scale of the control point, which scales the deformations at that control point. 
The Z handle scales the length of both tangents.

If you select multiple control points and set Unity's tool handle position to **Center**, the Scale tool will instead scale the positions of all selected control points from their center.

## Create deformation
Parent the object to any Spline or Spline Object. 
This will automatically generate a new mesh derived from the original mesh that the GameObject had in its Mesh Filter or/and Mesh Collider component.
Now you can move, delete, duplicate, rotate, and scale the deformation freely on the spline.

If the mesh looks blocky or incorrect, increase the mesh resolution. Go to the SplineObject menu, and under Mesh Settings increase the Resolution value.
![Correct mesh](/imgs/mesh_resolution.jpg)

## Create follower
Parent the GameObject to an Spline or Spline Object. After that, select the Game Object and change type to Follower.

![Change type](/imgs/change_to_follower.png)

## Snapping
You can snap and align deformations by enable Unity’s snapping feature.

## Animations
You can animate any deformation or follower using Unity's built in animations system or script your own behaviours. 
If you see the small orange icon in the upper left corner (in the Spline Object menu), you need to enable read/write access for that mesh (only for deformations). 
Otherwise, you can't animate it.  
![Realtime deformation](/imgs/cant_realtime_deform.png)  
Find the mesh in your project folder --> check the Read/Write box --> press the apply button.  
![Read write](/imgs/read_write.png)

!> **Note:** Remember that the component option on the spline and SplineObject needs to be set to Active. Otherwise, you won’t be able to use them in your built game. More about this below.

## Children of Deformations
You can parent a deformation or follower to another deformation, and it will be updated and deformed within that deformation’s local space. 
This can be useful for animating a train with different wagons.
You can animate the train using Unity’s built-in animation system or through scripting, parent them to the train, and all the wagons will follow. 

**Note:** A deformation can be an empty GameObject.

## Children of Followers
Parenting a deformation or follower to another follower will make the object behave like a normal GameObject that ignores the spline space.

## Toolbar/Menus
Spline Architect has its own toolbar, menus and selection system. You have different menus depending on what you have selected. 
Splines, control points and deformations all have their own with different submenus and settings. 
Toggle between submenus with the bottom buttons. Some menu items have yellow icons. 
You can hover over these icons to get information about the specific menu item. You access the menus trough the toolbar, where you also find some other features.

Nearly all functionality in Spline Architect can be found within these menus/toolbar. The exceptions are how you adjust snapping and creating new splines.

Below, we’ll go through the toolbar and each menu, and explain all the features.

!> **Note:** If you can't see the Toolbar you need to enable it in Unity's overlay menu. ![Overlay menu](/imgs/overlay_menu.png)  


### Toolbar
![Toolbar](/imgs/toolbar.png)  
From the toolbar you have access to all Spline Architect menus and some other features.

**Note:** If you are using Unity 2022 you may want to dock the Toolbar because of a visual bug (the bug has no implications on any features).

**Buttons:**
- **Control panel menus** | Access the spline, spline object and control point menus. You can toggle them by pressing (alt + h).
- **Settings menu** | Access the settings menu that contains various user settings.
- **Info menu** | Access the info menu where you have links to documentations, can see addons installed and more.
- **Plane system** | Toggle the visibility of the plane system, which can be displayed either as a grid or as a regular plane depending on the selected type. 
A small dropdown menu provides access to additional plane system settings.
- **Handle type** | Change between Mirrored, Continuous, broken and auto handle types.
- **Hide splines** | Controls spline visibility in the Scene view.
  - **Show All Splines (default)** | Displays all splines.
  - **Hide Non-Selected Splines** | Hides all splines except the selected spline.
  - **Hide Non-Selected Splines and Occlude Selected Spline** | Hides all non-selected splines and allows the selected spline to be occluded by other GameObjects.
Disabling gizmos will hide all splines, even the selected spline.
- **Show normals** | Toggle normals on or off.

### Settings menu
![Settings](/imgs/menu_settings.png)  

**Settings:**
- **Spline line resolution** | The resolution on all lines that draws splines.
- **Spline view distance** | Splines will not be drawn beyond this distance.
- **Normal spacing** | How many normals will be drawn for every 100 meters of a spline.
- **Normal length** | The length of the normals displayed along the spline.
- **Control point size** | Change the size of all control points on splines.
- **Control point scale distance** | Sets the maximum distance control points will scale to.
- **Deformation performance** | A higher value will force deformation jobs to run over fewer frames.
- **Link indicators** | Turn link indicators on/off, small dots shown in the Scene view over linked control points.
- **Created splines occluded** | Determines if newly created splines are occluded or not.

**Menus:**
- **Info icons** | Turn on/off info icons (yellow (i) icons).
- **Floating menus** | Turn floating menus on/off. When floating menus are enabled, you can move all Spline Architect windows freely. This can be useful if you have undocked the Scene view.
- **Horizontal layout** | Determines the layout of the control panel menus.
- **Submenus on top** | Determines whether submenu buttons are displayed at the top or bottom of the window.

### Info menu
![Info](/imgs/menu_info.png)  

**Info:**
- **Version** | Current Spline Architect version.
- **Splines** | How many splines are currently in the open scene(s).
- **Length** | Total length of all splines.
- **LInes drawn** | How many lines Spline Architect will draw to display all splines.
- **Temp data** | Currently only used by the Terrain Tools addon. Displays how much temporary data is being used and how much disk space you have left. Restarting the editor will remove unnecessary data.

### Plane system menu
![Info](/imgs/menu_grid.png)  
All settings for the plane system. The plane system provides a visible working area that can be displayed either as a grid or as a regular plane. In grid mode, control points will snap to the grid.

**Buttons:**
- **Type** | Determines how the plane system is displayed and behaves.
  - **Plane** | Displays a regular plane as the working area.
  - **Grid** | Displays a grid as the working area. In this mode, all control points will snap to the grid.
- **Color** | Change the color.
- **Size** | Change the size of the grid. Only visible when the plane type is set to Grid.
- **Occluded** | Determines whether the plane should be occluded by other GameObjects.
- **Distance labels** | Show or hide distance labels.

### Spline menu - deformation
![Spline deformation](/imgs/menu_deformation.png)  
This panel appears when a Spline is selected, allowing you to configure various settings related to the splines mesh deformation feature.

**Top buttons:**
- **Toggle loop** | Toggle loop for the selected Spline.
- **Select and center Spline transform** | Selects the spline’s transform and centers it to the center of all control points. 
If the spline transform is already selected, all control points on the spline will be selected instead.
- **Reverse control points** | Reverses the order of all control points.
- **Join selected Splines** | Joins all currently selected splines. This button is only available when two or more splines are selected.
- **Align control points** | Aligns all selected control points into a straight line. Only available when two or more anchors are selected.

**General:**
- **Components** | Determines how the Spline and SplineObject components are handled during runtime.
  - **Active** | Keeps all spline data and runtime functionality enabled. This allows the spline and SplineObjects to be used for animations and other runtime logic in the built game.
  - **Inactive** | Keeps the components in the build, but removes all spline data during the Start call on the Spline component. Useful if you only need to generate meshes during scene load.
  - **Remove from Build** | Completely removes the Spline and SplineObject components from the built game.
- **Scale profile** | Control the scale of the spline from start to end using an Animation Curve.
- **Noise group** | The noise group you want to be applied to mesh deformations and animations.
- **Normal resolution** | The resolution of cached normals.
- **Spline resolution** | The resolution of the spline. If you’re working with very long splines you might need to increase this value.
**Spline rendering:**
- **Render in game** | Draws the spline in Play mode and in the built game. Useful for debugging when you need to see the spline at runtime.
- **Occluded** | Determines whether the spline is always visible or can be occluded by other GameObjects.
- **Color** | Change the color of the spline’s control points and unselected line. The selected spline will always display with a yellow line.
- **Width** | The width of the spline. A larger width also increases the spline’s selection area. 
Values other than 1 require a less efficient spline rendering system. See [Spline rendering](user_manual?id=spline-rendering) for more information.
**Deformation scheduling:**
- **Start** | Specifies whether the deformation job starts in **Update**, **Late Update**, or is **disabled** so you can trigger it manually.
- **End** | Specifies whether the deformation job ends in **Update**, **Late Update**, or is **disabled** so you can handle completion manually.
- **Job interval** | The number of frames the deformation job runs before it completes.
Increasing this can greatly reduce performance cost and is especially useful for background animations.
- **Initial delay** | The number of frames to wait after the spline is initialized before the deformation job starts.
If you want two splines to update on alternating frames, set the Initial Delay to 1 on one of them so they do not complete on the same frame, which can improve performance.
- **Frame spreading** | Spreads SplineObjects across multiple frames based on the selected value. 
This can greatly increase performance when animating many SplineObjects along splines. Primarily intended for followers, 
but can also be used for deformations as long as they do not need to update coherently.

### Spline menu - noise
![Spline noise](/imgs/menu_noise.png)  
Will appear when a Spline is selected and you have pressed the bottom noise button.
Displays all the noise settings that will change the behaviour on mesh deformations and animations.

- **Noise** | Choose between 8 different noise effects.
- **Group** | Set the group for the noise layer. You can use the same group for multiple layers and stack noise layers together to create various effects.

### Spline menu - info
![Spline info](/imgs/menu_spline_info.png)  
General info about the Spline, size in memory, length, and so on.

### Spline object menu - general
![Spline object general](/imgs/menu_spline_object_general.png)
![Spline object general](/imgs/menu_spline_object_general_follower.png)  
One of these menus appear when a Spline Object is selected. 
The left menu appears when the deformation type is set, and the right menu appears when the follower type is set. 
At the top, you’ll find all buttons related to the selected spline object. 

**Top buttons:**
- **Snap** | You can snap your deformations to specific points or the control points on the spline. It has 3 modes: **disabled**, **snap to control points**, **snap to point**.
- **Project to surface** | Projects the deformed mesh or follower transform onto the surface below. Unity Calculated normals must be used when projecting deformations onto surfaces, otherwise the deformation will appear incorrect.
The raycast originates from the spline transform’s downward direction, allowing projection to work correctly on walls, roofs, and other orientations when the spline transform is properly aligned with the surface.
- **Align to end** | If enabled, the Spline Object will originate from the end of the spline instead of the start.
- **To center** | Reset the object’s X and Y position to 0 and its Euler rotation to (0, 0, 0).
- **Auto type** | Will automatically change types between deformation and follower. For example, if you have a road where parts of it is a straight line. 
All deformations on that straight line will switch to type follower instead and not be deformed.
This works really well with the "Align control points" feature and is also very good for performance and memory usage.

**General:**
- **Position** | Set the object’s position in spline space. 
- **Rotation** | Set the object’s rotation in spline space. 
- **Rotation mode** | Only visible when the type is set to Follower. Rotation Mode has four different options:
  - **Follow Spline (default)** | The SplineObject follows the spline’s rotation.  
  - **Lock Upwards** | Locks the SplineObject to the spline transform’s up direction while otherwise following the spline’s rotation.  
  - **Lock Upwards Keep Z Rotation** | Works the same as Lock Upwards, but also applies the Z rotation values from the control points.  
  - **Follow Projection** | Aligns the follower’s rotation to the projected surface normal. 
- **Component** | Determines how the SplineObject component is handled during runtime.
  - **Active** | The SplineObject remains fully active and can be used for animations and runtime logic in the built game.
  - **Inactivate after scene load** | Keeps the component in the build, but automatically inactivates it during the Start call. Useful if you only need the generated meshes during scene load.
  - **Remove from build** | Completely removes the component from the built game.

  **Note:** If you change this value on a SplineObject hierarchy, the same component setting will be applied to the entire hierarchy.

- **Type** | Changes the SplineObject type.
  - **None** | Behaves like a normal GameObject and does not interact with the spline.
  - **Follower** | Follows the spline without being deformed.
  - **Deformation** | Deforms the mesh along the spline if the GameObject contains a MeshFilter or MeshCollider.

**Mesh settings:**
- **UV offset** | Offsets the UVs along the X or Y axis. You can also choose between UV1, UV2, UV3, and UV4 and set an individual offset for each channel.
- **Resolution** | Increases the level of detail of a mesh when it is deformed along a spline, 
adding vertices and triangles to the mesh to make it appear smoother.
This does not modify the original mesh, only the instance mesh created from it.
It only adds detail; you cannot reduce the mesh’s detail level below the original geometry.
Setting Resolution to 0 uses the original mesh geometry without adding any extra detail.
- **Resolution mode** | The dropdown to the right of the resolution value. Determines how the resolution adds detail to the mesh.
  - **Subdivide all axels (Default)** | Subdivides the triangles of the mesh on all axes.
  - **Subdivide z axel** | Only subdivides the triangles based on their length along the Z axis, improving smoothness while keeping vertex count lower.
  Requires the mesh’s Z axis to align with the spline direction.
  - **Subdivide x axel** | Works the same as Subdivide z axel, but along the X axis.
  - **Edge loops along z** | Adds evenly spaced edge loops along the Z axis. The resolution value is the number of edge loops. 
  Existing edge loops are reused and flat quads stay as quads, which keeps the vertex count low. Requires the mesh’s Z axis to align with the spline direction.
  - **Edge loops along x** | Works the same as Edge loops along z, but along the X axis.
- **Auto** | Enables auto resolution. The mesh resolution increases based on how much the spline bends and twists under the mesh. Stretching from snapping is also taken into account.
Enabling this option reveals an additional setting called **Auto Bias**. Higher values increase the resolution and a value of 0 adds no extra resolution. 
The slider goes from 0 to 2, but you can enter values up to 10 in the input field.
- **Skip tangents** | Skips recalculating mesh tangents, which can improve performance if tangents are not required by your shaders or other rendering effects.
- **Normals** | Determines how normals are generated for the deformation.
  - **Spline Based (Default)** | The recommended and most performant normal calculation method for most use cases.
  - **Cylinder Based** | Best suited for cylindrical shapes such as pipes or cables.
  - **Unity Calculated** | Uses Unity’s built-in normal calculation method.
  - **Unity Calculated (Seamless)** | Similar to Unity Calculated, but ignores overlapping vertices to create seamless normals.
  - **Don’t Calculate** | Disables normal calculation entirely.
- **Mesh** | How to handle the deformed mesh: You can generate it during runtime, save it in the scene file, or save it only in your build.

**Snap settings:**
</br>Snap settings can be toggled between three modes: Off, Snap to Control Points, and Snap to a specific point on the spline.
You can cycle through these modes by pressing the Snap button multiple times.
- **Snap length** | How long the deformations start and end points can snap to a control point or object on the spline.
- **Snap Offset** | Offset the snapping position for the start or end of the deformation.
- **Spline point** | Snaps to a specific point on the spline, measured in meters.

**Advanced settings:**
- **Project to surface mask** | Determines which LayerMask should be used when projecting this SplineObject onto a surface.
- **Mirror** | Mirrors the deformation. Only available when the type is set to Deformation. 
You can achieve a similar effect by using negative scaling, but this may cause collision issues in some physics systems.
- **Lock position** | Only visible when set to type Follower. A value from 0 to 1. At 1, the SplineObject will not follow the spline’s position but will only adjust according to the spline’s rotation. Values between 0 and 1 blend between following and locking the position.


### Spline object menu - info
![Spline object info](/imgs/menu_spline_object_info.png)  
General info about the Spline Object.

### Control point menu - general
![Control point general](/imgs/menu_control_point_general.png)  
This menu appears when a control point is selected.

**Top buttons:**
- **Link to closest anchor or spline connector** | Links the selected control point to the closest anchor on any spline or closest Spline Connector.
- **Project to spline plane** | Projects the control point(s) to the surface on the splines plane.
- **Project to surface** | Projects the selected control point(s) downward onto a surface. The spline transform’s downward direction is used, 
so rotating the spline transform allows segments to be projected onto walls, roofs, or other oriented surfaces.
- **Next control point** | Move selection to the next control point.
- **Prev control point** | Move selection to the previous control point.
- **Split spline** | Split the Spline into two at the selected control point.
- **Align tangents** | Only available on linked control points, will align all linked control points.

**General:**
- **Position** | World position for the selected control point.
- **Z position** | The control points position on the spline. The z position is also equivalent to the spline length at that specific point.
- **Length** | The length of the segment.
- **Static** | Sets the segment to static, meaning cached normals will not be calculated for this segment. This can improve performance if you move control points at runtime. 
However, static segments do not work well with loops or similar shapes, so you should avoid using this option in those cases.
- **Type** | Change between Spline and Line type. Creating new control points will have the type of the closest control point.

**Links:** If you have linked the control point, all links will be displayed in this section. 
You can also toggle each link on/off, when doing so the link will be ignored when unsing API functions as: spline.FindLinkCrossingsNonAlloc().

### Control point menu - deformation
![Control point deformation](/imgs/menu_control_point_deformation.png)  
Will appear when a control point is selected and the bottom deformation sub menu button has been pressed. 
Has different effects that can be applied to deformations.

> If you want to use values outside the slider range, manually enter the desired value in the input field.

## Components
**Spline**  
Add this component to any GameObject to create a Spline. However, the most common way to create one is by using the Create Spline tool button.

**Spline Object**  
This component is automatically added when you attach a GameObject as a child of a spline. 
You can also add it manually to any GameObject, even if it is not yet placed on a spline, for pre-configuration.

**Spline Connector**
You can connect a splines control point to the GameObject that has this component.
This is useful for animating splines using Unity’s animation system or for creating road intersections.
Moving the SplineConnector GameObject will update all connected splines. 

If a SplineConnector is only connected to splines with the component setting "Remove from build", the connector will also be removed. 
If the connector has no connections at all, it will be included in the build.

## Shortcuts
Spline Architect has several shortcuts that can be changed in Unity’s Shortcut Manager (Edit --> Shortcuts).
Some shortcuts also have additional features that are only available when using specific key combinations, see below.

**Snap control points to surfaces** - Select a control point, hold down **Shift + Ctrl**, and then click the control point again to activate surface snapping. 
Now, as you move the control point, it will snap to and follow the surface of any object with a collider.

**Match nearest spline direction** - Holding **Ctrl** while creating control points will automatically align the new control point with the closest spline.

**Match adjacent control point height** - Holding **Ctrl + Shift** while creating control points will create the new control point at the same height as the adjacent control point.

**Change indicator size** - Holding **Shift** and **scrolling** down or up when creating new control points will adjust the size of the indicator. 
On **Mac**, you need to hold **Shift** and press **Arrow Up** or **Arrow Down** for the same effect.

## Unity Splines Conversion
Spline Architect does not use Unity splines as its spline foundation, 
but you can convert any Unity spline to a Spline Architect spline and the other way around.

If you have the Unity Splines package installed, right-clicking on a SplineContainer or a Spline Architect spline component 
will show options like Convert to Unity Spline and Convert to Spline Architect Spline.
You also have an option for matching a Spline Architect spline to a Unity spline.

## Spline rendering
Spline Architect has two different systems for rendering splines on the screen. 
The first system allows you to change the width of a spline, 
while the second is significantly more efficient and can handle thousands of visible splines at the same time.

The more efficient rendering system only supports a spline width of 1. 
Setting the width to any other value will automatically use the less efficient rendering system.  

Depending on your hardware, the less efficient system can typically handle around 100–500 visible splines, 
while the more efficient system is around 400% faster and can handle approximately 500–2,500 visible splines.

## Mesh pooling
Spline Architect tracks all deformed meshes. 
For example, if a Mesh Filter and a Mesh Collider on the same Game Object use the same original mesh, only one mesh will be created and used.
If you have two different meshes on a collider and mesh filter it will pool two different meshes.

