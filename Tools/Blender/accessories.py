"""Prepara los accesorios del usuario (y el micrófono del presentador) tal cual: sin tocar polígonos ni texturas.
Solo los pone a su tamaño real y lleva el origen a su punto de enganche, con el frente hacia -Y y arriba +Z (como los
personajes), y los exporta a FBX con los mismos ajustes que los personajes.
Uso: blender -b -P accessories.py -- <carpeta con las descargas> <carpeta de salida de accesorios> <carpeta del micro> [carpetas]"""
import bpy, sys, os, shutil
import numpy as np
from mathutils import Vector, Matrix

argv = sys.argv[sys.argv.index("--") + 1:]
SRC, OUT, MIC_OUT = argv[0], argv[1], argv[2]
ONLY = set(argv[3:])  # Opcional: solo estas carpetas.

# Nombre de la carpeta descargada -> (nombre en el proyecto, enganche, medida en metros y de qué eje).
# Enganches: base (centro de abajo), center (centro), front (cara delantera, donde la montura), back (cara de atrás,
# la que toca el cuerpo), wings (cara delantera, que toca la espalda, a la altura del broche).
ITEMS = {
    "Crown": ("Crown", "base", 0.46, "x"),
    "TopHat": ("TopHat", "base", 0.60, "x"),
    "Halo": ("Halo", "center", 0.50, "x"),
    "StarGlasses": ("StarGlasses", "front", 0.62, "x"),
    "SunGlasses": ("SunGlasses", "front", 0.58, "x"),
    "Mask": ("Mask", "front", 0.68, "x"),
    "Mustache": ("Mustache", "back", 0.30, "x"),
    "BowTie": ("BowTie", "back", 0.26, "x"),
    "AngelWings": ("AngelWings", "wings", 1.10, "x"),
    "DragonWings": ("DragonWings", "wings", 1.15, "x"),
    "Mic": ("Microphone", "base", 0.34, "z"),
}

def pivot_of(c, kind):
    mn, mx = c.min(0), c.max(0); ctr = (mn + mx) / 2
    if kind == "base":
        return np.array([ctr[0], ctr[1], mn[2]])
    if kind == "center":
        return ctr
    if kind == "front":
        # La montura (lo que queda delante): su centro en altura; las patillas van hacia atrás.
        depth = mx[1] - mn[1]
        part = c[c[:, 1] < mn[1] + 0.18 * depth]
        return np.array([ctr[0], mn[1], (part[:, 2].min() + part[:, 2].max()) / 2])
    if kind == "back":
        return np.array([ctr[0], mx[1], ctr[2]])
    if kind == "wings":
        width = mx[0] - mn[0]
        clasp = c[np.abs(c[:, 0] - ctr[0]) < 0.08 * width]
        return np.array([ctr[0], mn[1], (clasp[:, 2].min() + clasp[:, 2].max()) / 2])
    raise ValueError(kind)

for folder, (name, kind, size, axis) in ITEMS.items():
    if ONLY and folder not in ONLY:
        continue
    path = os.path.join(SRC, folder, "base_basic_shaded.fbx")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=path)
    meshes = [o for o in bpy.data.objects if o.type == 'MESH']
    for o in list(bpy.data.objects):
        if o.type != 'MESH':
            bpy.data.objects.remove(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes: o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1: bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    obj.data.transform(obj.matrix_world); obj.matrix_world = Matrix()
    obj.name = name; obj.data.name = name
    c = np.empty(len(obj.data.vertices) * 3); obj.data.vertices.foreach_get("co", c); c = c.reshape(-1, 3)
    extent = c.max(0) - c.min(0)
    scale = size / extent["xyz".index(axis)]
    pivot = pivot_of(c, kind)
    obj.data.transform(Matrix.Diagonal((scale, scale, scale, 1)) @ Matrix.Translation(Vector((-pivot).tolist())))
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)

    # Material con su nombre (Unity lo sustituye por el suyo: base negra y shaded.png como emisión).
    tex = None
    for mat in obj.data.materials:
        if mat and mat.use_nodes:
            for n in mat.node_tree.nodes:
                if n.type == 'TEX_IMAGE': tex = n.image
    mat = bpy.data.materials.new(f"{name}_Mat"); mat.use_nodes = True
    if tex is not None:
        t = mat.node_tree.nodes.new("ShaderNodeTexImage"); t.image = tex
        mat.node_tree.links.new(t.outputs[0], mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"])
    obj.data.materials.clear(); obj.data.materials.append(mat)

    dest = MIC_OUT if folder == "Mic" else os.path.join(OUT, name)
    os.makedirs(dest, exist_ok=True)
    bpy.ops.object.select_all(action='DESELECT'); obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(dest, f"{name}.fbx"), use_selection=True, object_types={'MESH'},
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=True, axis_forward='-Z', axis_up='Y',
        mesh_smooth_type='FACE', use_mesh_modifiers=False, path_mode='STRIP', embed_textures=False, bake_anim=False)
    shutil.copyfile(os.path.join(SRC, folder, "shaded.png"), os.path.join(dest, "shaded.png"))
    size_now = (c.max(0) - c.min(0)) * scale
    print(f"ACCESSORY {name}: {tris} triángulos, tamaño {np.round(size_now, 3)} m, enganche {kind}")
