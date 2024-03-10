import bpy
import sys
from random import Random

sys.path.insert(0, './py_scripts~')

from addon_installer import install_addon
from value_getters import get_int, get_bool
from import_export import export_scene
from union_loose_parts import separate_loose_parts, union_selected

install_addon("./blender~/add_mesh_SpaceshipGenerator-v1.6.5.zip",
              "add_mesh_SpaceshipGenerator")

bpy.ops.add_mesh.generate_spaceship(
    num_hull_segments_min=get_int("num_hull_segments_min"),
    num_hull_segments_max=get_int("num_hull_segments_max"),
    create_asymmetry_segments=get_bool("create_asymmetry_segments"),
    num_asymmetry_segments_min=get_int("num_asymmetry_segments_min"),
    num_asymmetry_segments_max=get_int("num_asymmetry_segments_max"),
    create_face_detail=get_bool("create_face_detail"),
    allow_horizontal_symmetry=get_bool("allow_horizontal_symmetry"),
    allow_vertical_symmetry=get_bool("allow_vertical_symmetry"),
    add_bevel_modifier=False,
    create_materials=get_bool("create_materials"))

obj = separate_loose_parts()

if get_bool("add_bevel_modifier"):
  geom_random = Random()
  bevel_modifier = obj.modifiers.new("Bevel", "BEVEL")
  bevel_modifier.width = geom_random.uniform(5, 20)
  bevel_modifier.offset_type = "PERCENT"
  bevel_modifier.segments = 2
  bevel_modifier.profile = 0.25
  bevel_modifier.limit_method = "NONE"
  bevel_modifier.use_clamp_overlap = False  # no noticeable effect otherwise
  bpy.ops.object.modifier_apply(modifier=bevel_modifier.name)

if get_bool("Union Loose Parts"):
  union_selected(obj)

bpy.ops.object.editmode_toggle()
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project()
bpy.ops.object.editmode_toggle()

export_scene()
