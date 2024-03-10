import bpy

def install_addon(path,addon_name):
  bpy.ops.preferences.addon_install(filepath=path)
  bpy.ops.preferences.addon_enable(module=addon_name)
