Polygon Tool
---------------------------
Created by David Clark 2011


Controls:
- Add a 'Polygon' component to a game object - a mesh renderer, mesh filter, and mesh collider will be added automatically
- The polygon is created by extruding a shape that you create by placing points on a plane. This plane is along the local x and y axis.
- When you select 'Add points' or 'Modify points', other objects in the scene wont be selectable. You also wont be able to use the camera widget in the top right
  - To select other objects or modify the camera by its widget, hold down alt
- Pressing Control + W will toggle wether you want the selected object to have its wireframe showing

Modify mode:
- Pressing Delete on the keyboard will delete the selected point
- Left click to select a point
- Drag a point to move it
- Hold left click to create a selection box
- Hold control to select more than one point by toggling if its selected or not
- Hold control and shift while dragging a box to deselect points

Add mode:
- The aqua blue line represents the line that will be 'split' when you add an extra point. Clicking a point in modify mode will change this line.
- Left click to place a point
- Left click and hold to see the 'elastic band' to see where the point would go

Undos:
- Pressing undo and redo will undo any changes to points such as moving, adding or deleting
- It will NOT undo selection changes



