import bpy
import sys
from os import environ

sys.path.insert(0, './py_scripts~')
from import_export import export_scene
from value_getters import get_int, get_str, get_float, get_bool


def separate_loose_parts(obj=None):
  if obj is None:
    # Select the object with loose parts
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    obj = bpy.context.active_object
  else:
    bpy.context.view_layer.objects.active = obj

  # Deselect all objects
  bpy.ops.object.select_all(action='DESELECT')

  # Select all loose parts of the object
  bpy.ops.object.mode_set(mode='EDIT')
  bpy.ops.mesh.select_mode(use_extend=False, use_expand=False, type='VERT')
  bpy.ops.mesh.select_all(action='SELECT')
  bpy.ops.mesh.separate(type='LOOSE')

  # Switch back to Object Mode
  bpy.ops.object.mode_set(mode='OBJECT')

  return obj


def union_selected(main_obj):
  # Apply the Boolean Union modifier to combine all objects into one
  for ob in bpy.context.selected_objects:
    if ob.type == 'MESH' and ob != main_obj:
      mod = main_obj.modifiers.new(name="Boolean", type='BOOLEAN')
      mod.operation = 'UNION'
      mod.object = ob
      bpy.ops.object.modifier_apply(modifier=mod.name)

  # Clean up loose parts
  for ob in bpy.context.selected_objects:
    if ob != main_obj:
      bpy.data.objects.remove(ob, do_unlink=True)
