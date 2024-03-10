import bpy
import sys
from os import environ

sys.path.insert(0, './py_scripts~')

from value_getters import get_float_in_range, get_int, get_bool, get_int_in_range
from import_export import export_scene

bpy.context.object.modifiers["GeometryNodes"]["Input_87"] = get_int("seed")

width = get_int_in_range("Width") - 1
bpy.context.object.modifiers["GeometryNodes"]["Input_5"] = width
length = get_int_in_range("Length") - 1
bpy.context.object.modifiers["GeometryNodes"]["Input_7"] = length
stories = get_int_in_range("Stories") - 1
bpy.context.object.modifiers["GeometryNodes"]["Input_13"] = stories
bpy.context.object.modifiers["GeometryNodes"]["Input_69"] = 1 if get_bool(
    "With Balcony") else 0
bpy.context.object.modifiers["GeometryNodes"]["Input_35"] = get_int_in_range(
    "Entrance Every x Column")
bpy.context.object.modifiers["GeometryNodes"]["Input_37"] = get_int_in_range(
    "Entrance Position Offset")
bpy.context.object.modifiers["GeometryNodes"]["Input_43"] = get_int_in_range(
    "Balcony Every x Column")
bpy.context.object.modifiers["GeometryNodes"]["Input_45"] = get_int_in_range(
    "Balcony Position Offset")
bpy.context.object.modifiers["GeometryNodes"]["Input_39"] = get_int_in_range(
    "Wide Window Every x Column")
bpy.context.object.modifiers["GeometryNodes"]["Input_41"] = get_int_in_range(
    "Wide Window Position Offset")

bpy.context.object.modifiers["GeometryNodes"]["Input_63"] = get_int_in_range(
    "Roof Extra Every x Column")
bpy.context.object.modifiers["GeometryNodes"]["Input_59"] = 1 if get_bool(
    "Add Roof Top") else 0
bpy.context.object.modifiers["GeometryNodes"]["Input_61"] = get_float_in_range(
    "Roof Top Height")
bpy.context.object.modifiers["GeometryNodes"]["Input_71"] = get_float_in_range(
    "Antennas")
bpy.context.object.modifiers["GeometryNodes"]["Input_75"] = get_int_in_range(
    "Water Pipe Every x Column")
bpy.context.object.modifiers["GeometryNodes"]["Input_89"] = get_float_in_range(
    "Extras on Balcony")

bpy.context.object.modifiers["GeometryNodes"].show_viewport = True

bpy.ops.object.modifier_apply(modifier="GeometryNodes")

# bpy.ops.object.shade_flat()
bpy.context.object.data.use_auto_smooth = True
bpy.context.object.data.auto_smooth_angle = 1.785398

bpy.ops.object.editmode_toggle()

bpy.ops.mesh.select_all(action='DESELECT')
bpy.context.object.active_material_index = 6
bpy.ops.object.material_slot_select()
bpy.ops.mesh.select_more()
bpy.ops.mesh.select_more()
bpy.ops.mesh.select_more()
bpy.ops.mesh.select_more()
bpy.ops.mesh.select_more()
bpy.ops.mesh.normals_make_consistent(inside=False)

bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.remove_doubles(threshold=0.005)

bpy.ops.uv.cube_project()
bpy.ops.object.editmode_toggle()

file_name = environ.get("output").replace(
    "DIMENSIONS",
    str(width + 1) + "x" + str(length + 1) + "x" + str(stories + 1))
environ["output"] = file_name

export_scene(use_selection=True)
