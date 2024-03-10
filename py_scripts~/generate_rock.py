import bpy
import sys
from os import environ

sys.path.insert(0, './py_scripts~')
from import_export import export_scene
from value_getters import get_int, get_str, get_float, get_bool

preset = [
    'Default', 'River Rock', 'Asteroid', 'Sandstone', 'Ice', 'Fake Ocean'
].index(get_str('Stone Type'))

x = get_float('scale_X')
y = get_float('scale_Y')
z = get_float('scale_Z')

bpy.ops.mesh.add_mesh_rock(preset_values=str(preset),
                           scale_X=(x, x),
                           scale_Y=(z, z),
                           scale_Z=(y, y),
                           display_detail=get_int('Mesh Density'),
                           user_seed=get_int('seed'),
                           use_random_seed=False,
                           num_of_rocks=get_int('Number of Rocks'))

if get_bool('Smooth'):
  bpy.ops.object.modifier_add(type='SMOOTH')
  bpy.context.object.modifiers["Smooth"].factor = get_float('Smoothness Factor')
  bpy.context.object.modifiers["Smooth"].iterations = 3

bpy.ops.object.convert(target='MESH')
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
dir = '.'.join(environ.get('output').split('.')[:-1])
print(dir)
selected = bpy.context.selected_objects
for index, obj in enumerate(selected):
  bpy.ops.object.select_all(action='DESELECT')
  obj.select_set(True)

  bpy.ops.object.editmode_toggle()
  bpy.ops.uv.cube_project()

  bpy.ops.object.editmode_toggle()

  bpy.context.view_layer.objects.active = obj
  export_scene(file_path=dir + get_str('seed') + "-" + str(index) + ".fbx",
               use_selection=True)
