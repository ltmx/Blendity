import bpy
import sys
import math

sys.path.insert(0, './py_scripts~')

from import_export import import_scene, export_scene
from value_getters import get_float, get_int

import_scene()

bpy.context.scene.snow.coverage = get_int('Coverage %')
bpy.context.scene.snow.height = get_float('Height')

bpy.ops.object.select_all(action='SELECT')
objs = bpy.context.selected_objects[:]
for obj in objs:
  obj.rotation_euler.x += math.radians(get_float('Rotate X'))
  obj.rotation_euler.y += math.radians(get_float('Rotate Z'))
  obj.rotation_euler.z += math.radians(get_float('Rotate Y'))

bpy.ops.snow.create()

objs = bpy.context.selected_objects[:]
bpy.ops.object.select_all(action='DESELECT')

for obj in objs:
  obj.select_set(True)
  bpy.context.view_layer.objects.active = obj
  bpy.ops.object.modifier_remove(modifier="Subdiv")
  bpy.context.object.modifiers["Decimate"].ratio = 1 - get_float(
      'Mesh Reduction')

export_scene()
