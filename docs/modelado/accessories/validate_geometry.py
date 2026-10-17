"""Validate actual Blender mesh topology, UVs, materials and finite coordinates."""
import bpy
import bmesh
import json
import math
from pathlib import Path

root=Path(__file__).resolve().parent
report=[]
for item in json.loads((root/'manifest.json').read_text()):
    name=item['name']
    bpy.ops.wm.open_mainfile(filepath=str(root/name/(name+'.blend')))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    problems=[]
    for obj in meshes:
        bm=bmesh.new();bm.from_mesh(obj.data)
        boundary=sum(e.is_boundary for e in bm.edges)
        nonmanifold=sum(not e.is_manifold for e in bm.edges)
        degenerate=sum(f.calc_area()<1e-12 for f in bm.faces)
        if nonmanifold or degenerate:problems.append({'part':obj.name,'boundary':boundary,'nonmanifold':nonmanifold,'degenerate':degenerate})
        assert all(math.isfinite(v) for vertex in obj.data.vertices for v in vertex.co)
        assert obj.data.uv_layers and obj.data.materials
        bm.free()
    report.append({'name':name,'mesh_parts':len(meshes),'problems':problems})
(root/'geometry-validation.json').write_text(json.dumps(report,indent=2))
print('VALIDATION',json.dumps(report),flush=True)
