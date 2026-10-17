import bpy, sys, math
args=sys.argv[sys.argv.index("--")+1:]
out, path = args[0], args[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
bpy.ops.import_scene.fbx(filepath=path)
scene.render.engine='BLENDER_WORKBENCH'; scene.display.shading.light='FLAT'; scene.display.shading.color_type='TEXTURE'
scene.render.film_transparent=True
scene.render.image_settings.color_mode='RGBA'
scene.render.resolution_x = scene.render.resolution_y = 768
cam=bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam")); scene.collection.objects.link(cam); scene.camera=cam
cam.data.type='ORTHO'; cam.data.ortho_scale=1.18
cam.location=(0,-8,1.38); cam.rotation_euler=(math.radians(90),0,0)
scene.render.filepath=out; bpy.ops.render.render(write_still=True)
