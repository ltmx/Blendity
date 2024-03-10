import bpy
import sys

sys.path.insert(0, './py_scripts~')

from import_export import import_scene, export_scene
from value_getters import get_int, get_float, get_vec, get_bool
from union_loose_parts import separate_loose_parts, union_selected

import_scene()

objs = bpy.context.scene.objects[:]

if get_bool("Join All Objects"):
  bpy.ops.object.select_all(action='SELECT')
  bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
  bpy.ops.object.join()

objs = bpy.context.scene.objects[:]

if get_bool("Union Loose Parts"):
  for obj in objs:
    if obj.type != 'MESH':
      continue

    separate_loose_parts(obj)
    union_selected(obj)

objs = bpy.context.scene.objects[:]

i = 0
for obj in objs:
  if obj.type != 'MESH':
    continue

  ps = obj.modifiers.new("Part", 'PARTICLE_SYSTEM').particle_system
  ps.seed = get_int('seed') + i
  i += 1
  ps.settings.count = get_int('numOfPieces') * 10
  ps.settings.frame_end = 1

  obj.select_set(True)

bpy.ops.object.add_fracture_cell_objects(
    use_debug_redraw=False,
    source_limit=get_int('numOfPieces'),
    source_noise=get_float('noise'),
    cell_scale=get_vec('scaleX', 'scaleY', 'scaleZ'),
    use_smooth_faces=get_bool('smoothFaces'),
    use_sharp_edges=get_bool('sharpEdges'),
    margin=get_float('margin'),
    use_recenter=get_bool('recenterOrigin'))
bpy.ops.object.select_all(action='DESELECT')

for obj in objs:
  if obj.type != 'MESH':
    continue
  obj.select_set(True)

bpy.ops.object.delete(use_global=False)

export_scene()
