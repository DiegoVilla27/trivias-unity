"""Riggea un personaje tal cual (sin tocar malla ni textura): esqueleto humano de Unity a partir de su geometría,
pesos calculados en una copia ligera y traspasados al modelo completo, pose T y FBX listo para Unity.
Uso: blender -b -P rig_character.py -- <base_basic_shaded.fbx> <salida.fbx> <nombre> <renders_prefijo>"""
import bpy, sys, math
import numpy as np
from mathutils import Vector, Matrix
src, out, name, renders = sys.argv[sys.argv.index("--")+1:][:4]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=src)
full=[o for o in bpy.data.objects if o.type=='MESH'][0]
for o in list(bpy.data.objects):
    if o is not full: bpy.data.objects.remove(o)
full.data.transform(full.matrix_world); full.matrix_world=Matrix()
full.name="Body"
c=np.empty(len(full.data.vertices)*3); full.data.vertices.foreach_get("co",c); c=c.reshape(-1,3)
top=c[:,2].max()
ymid=float(np.median(c[:,1]))

def slab(z0,z1,x0,x1):
    s=c[(c[:,2]>=z0)&(c[:,2]<z1)&(c[:,0]>=x0)&(c[:,0]<=x1)]
    return s.mean(0) if len(s) else None

# Entrepierna: donde las perneras se juntan (vértices cerca de x=0 en el centro).
crotch=0.38
for z in np.arange(0.15,0.8,0.005):
    s=c[(c[:,2]>=z)&(c[:,2]<z+0.005)&(np.abs(c[:,1]-ymid)<0.12)]
    if len(s) and np.sum(np.abs(s[:,0])<0.012)>0: crotch=float(z); break

def arm_chain(sgn):
    """Brazo: punta de la mano y línea del brazo por centroides de las secciones separadas del tronco."""
    side=c[(c[:,0]*sgn>0.42)&(c[:,2]>0.15)]
    hand_bottom=float(side[:,2].min())
    pts=[]
    edge=0.25
    for z in np.arange(hand_bottom+0.03, hand_bottom+0.45, 0.02):
        s=c[(c[:,2]>=z)&(c[:,2]<z+0.02)&(c[:,0]*sgn>0.05)]
        if len(s)<10: continue
        xs=np.sort(s[:,0]*sgn); g=np.diff(xs); i=int(np.argmax(g))
        if g[i]<0.006: break  # el brazo ya toca el tronco
        arm=s[s[:,0]*sgn>xs[i]+1e-4]
        pts.append(arm.mean(0))
        edge=float(xs[i])
    pts=np.array(pts)
    # Recta por mínimos cuadrados en (x, z) en función de la altura.
    A=np.vstack([pts[:,2],np.ones(len(pts))]).T
    kx=np.linalg.lstsq(A,pts[:,0],rcond=None)[0]; ky=np.linalg.lstsq(A,pts[:,1],rcond=None)[0]
    at=lambda z: Vector((kx[0]*z+kx[1], ky[0]*z+ky[1], z))
    top_z=float(pts[:,2].max())
    # Hombro: donde la línea del brazo llega al costado del tronco (sube por encima de donde la manga toca el cuerpo).
    shoulder_z=(edge*0.85*sgn-kx[1])/kx[0] if abs(kx[0])>1e-4 else top_z+0.1
    shoulder_z=float(np.clip(shoulder_z, top_z+0.04, top_z+0.3))
    return hand_bottom, top_z, at, shoulder_z

L=arm_chain(1); R=arm_chain(-1)
armpit=(L[1]+R[1])/2
shoulder_z=(L[3]+R[3])/2
hips=crotch+0.035
neck=shoulder_z+0.07
chest=shoulder_z-0.06
spine=(hips+chest)/2
trunk=slab(crotch+0.05, armpit, -0.2, 0.2)
cy=float(trunk[1]) if trunk is not None else ymid
J={}
for side,sgn,(hand_bottom,top_z,at,_) in (("Left",1,L),("Right",-1,R)):
    shoulder=at(shoulder_z); shoulder.x=max(shoulder.x*sgn,0.18)*sgn
    wrist=at(hand_bottom+0.17); hand_end=at(hand_bottom+0.02)
    elbow=(shoulder+wrist)/2
    leg=slab(crotch-0.15, crotch-0.03, *((0.02,0.45) if sgn>0 else (-0.45,-0.02)))
    ankle_z=max(0.1, crotch*0.31)
    foot=c[(c[:,2]<0.08)&(c[:,0]*sgn>0.02)]
    toe_y=float(foot[:,1].min())
    knee=Vector((leg[0],leg[1],(crotch+ankle_z)/2)); ankle=Vector((leg[0],leg[1]+0.02,ankle_z))
    toe=Vector((leg[0],ankle.y+(toe_y-ankle.y)*0.7,0.05)); toe_end=Vector((leg[0],toe_y,0.04))
    J[side]=dict(UpperLeg=(Vector((leg[0],leg[1],hips-0.02)),knee),LowerLeg=(knee,ankle),Foot=(ankle,toe),Toes=(toe,toe_end),
                 Shoulder=(Vector((0.08*sgn,cy,shoulder_z+0.02)),shoulder),UpperArm=(shoulder,elbow),LowerArm=(elbow,wrist),Hand=(wrist,hand_end))
print(f"RIG {name}: entrepierna {crotch:.3f} axila {armpit:.3f} hombro {shoulder_z:.3f} manos {L[0]:.3f}/{R[0]:.3f} altura {top:.3f}")

arm=bpy.data.armatures.new("ContestantRig"); rig=bpy.data.objects.new("ContestantRig",arm)
bpy.context.scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='EDIT')
eb=arm.edit_bones
def bone(n,h,t,parent=None):
    b=eb.new(n); b.head=h; b.tail=t
    if parent: b.parent=eb[parent]; b.use_connect=False
P=lambda z: Vector((0.0,cy,z))
bone("Hips",P(hips),P(spine)); bone("Spine",P(spine),P(chest),"Hips"); bone("Chest",P(chest),P(neck),"Spine")
bone("Neck",P(neck),P(neck+0.08),"Chest"); bone("Head",P(neck+0.08),P(top-0.06),"Neck")
for side in ("Left","Right"):
    j=J[side]
    bone(f"{side}UpperLeg",*j["UpperLeg"],"Hips"); bone(f"{side}LowerLeg",*j["LowerLeg"],f"{side}UpperLeg")
    bone(f"{side}Foot",*j["Foot"],f"{side}LowerLeg"); bone(f"{side}Toes",*j["Toes"],f"{side}Foot")
    bone(f"{side}Shoulder",*j["Shoulder"],"Chest"); bone(f"{side}UpperArm",*j["UpperArm"],f"{side}Shoulder")
    bone(f"{side}LowerArm",*j["LowerArm"],f"{side}UpperArm"); bone(f"{side}Hand",*j["Hand"],f"{side}LowerArm")
bpy.ops.object.mode_set(mode='OBJECT')

# Pesos: calor en una copia ligera (20 %), traspasados punto a punto al modelo completo (que no se toca).
proxy=full.copy(); proxy.data=full.data.copy(); proxy.name="Proxy"; bpy.context.scene.collection.objects.link(proxy)
bpy.ops.object.select_all(action='DESELECT'); proxy.select_set(True); bpy.context.view_layer.objects.active=proxy
dec=proxy.modifiers.new("Decimate","DECIMATE"); dec.ratio=0.2; bpy.ops.object.modifier_apply(modifier="Decimate")
bpy.ops.object.select_all(action='DESELECT'); proxy.select_set(True); rig.select_set(True); bpy.context.view_layer.objects.active=rig
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
for g in proxy.vertex_groups: full.vertex_groups.new(name=g.name)
bpy.ops.object.select_all(action='DESELECT'); full.select_set(True); bpy.context.view_layer.objects.active=full
dt=full.modifiers.new("Weights","DATA_TRANSFER"); dt.object=proxy
dt.use_vert_data=True; dt.data_types_verts={'VGROUP_WEIGHTS'}; dt.vert_mapping='POLYINTERP_NEAREST'
dt.layers_vgroup_select_src='ALL'; dt.layers_vgroup_select_dst='NAME'
bpy.ops.object.modifier_apply(modifier="Weights")
bpy.data.objects.remove(proxy, do_unlink=True)
unweighted=sum(1 for v in full.data.vertices if not any(g.weight>0.001 for g in v.groups))
print(f"WEIGHTS {name}: sin peso {unweighted} de {len(full.data.vertices)}")
m=full.modifiers.new("Armature","ARMATURE"); m.object=rig
full.parent=rig

# Pose T: brazos a lo largo de ±X y esa pasa a ser la de reposo.
bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='POSE')
def aim(pb, d):
    bpy.context.view_layer.update()
    cur=(pb.tail-pb.head).normalized(); q=cur.rotation_difference(d.normalized())
    mm=pb.matrix.copy(); loc=mm.to_translation()
    pb.matrix=Matrix.Translation(loc) @ (q.to_matrix().to_4x4() @ Matrix.Translation(-loc) @ mm)
    bpy.context.view_layer.update()
for side,s in (("Left",1),("Right",-1)):
    for n in ("UpperArm","LowerArm","Hand"): aim(rig.pose.bones[f"{side}{n}"], Vector((s,0,0)))
bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='DESELECT'); bpy.context.view_layer.objects.active=full
bpy.ops.object.modifier_apply(modifier="Armature")
bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='POSE')
bpy.ops.pose.select_all(action='SELECT'); bpy.ops.pose.armature_apply(selected=False); bpy.ops.object.mode_set(mode='OBJECT')
m=full.modifiers.new("Armature","ARMATURE"); m.object=rig

# Material con su nombre (Unity lo sustituye por el suyo: base negra y shaded.png como emisión).
tex=None
for mat in full.data.materials:
    if mat and mat.use_nodes:
        for n in mat.node_tree.nodes:
            if n.type=='TEX_IMAGE': tex=n.image
mat=bpy.data.materials.new(f"{name}_Body"); mat.use_nodes=True
if tex is not None:
    t=mat.node_tree.nodes.new("ShaderNodeTexImage"); t.image=tex
    mat.node_tree.links.new(t.outputs[0], mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"])
full.data.materials.clear(); full.data.materials.append(mat)

# Comprobación: pose T y un paso de andar.
scene=bpy.context.scene
scene.render.engine='BLENDER_WORKBENCH'; scene.display.shading.light='FLAT'; scene.display.shading.color_type='TEXTURE'
scene.render.resolution_x, scene.render.resolution_y = 900, 600
cam=bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam")); scene.collection.objects.link(cam); scene.camera=cam
cam.data.type='ORTHO'; cam.data.ortho_scale=2.9; cam.location=(0,-6,0.95); cam.rotation_euler=(math.radians(90),0,0)
scene.render.filepath=f"{renders}_tpose.png"; bpy.ops.render.render(write_still=True)
bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='POSE')
pb=rig.pose.bones
def rot(n,axis,deg):
    b=pb[n]; b.rotation_mode='XYZ'; e=list(b.rotation_euler); e["XYZ".index(axis)]+=math.radians(deg); b.rotation_euler=e
rot("LeftUpperLeg","X",35); rot("LeftLowerLeg","X",-45); rot("RightUpperLeg","X",-25)
rot("LeftUpperArm","Z",65); rot("LeftLowerArm","X",-60); rot("RightUpperArm","Z",-65); rot("Head","Z",25)
bpy.ops.object.mode_set(mode='OBJECT')
cam.location=(-3.5,-4.5,0.95); cam.rotation_euler=(math.radians(90),0,math.radians(-38))
scene.render.filepath=f"{renders}_posed.png"; bpy.ops.render.render(write_still=True)
bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='POSE')
for b in pb: b.rotation_euler=(0,0,0); b.rotation_quaternion=(1,0,0,0)
bpy.ops.object.mode_set(mode='OBJECT')

bpy.ops.object.select_all(action='DESELECT'); rig.select_set(True); full.select_set(True)
bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'ARMATURE','MESH'},
    apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=True, axis_forward='-Z', axis_up='Y',
    add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=True,
    bake_anim=False, mesh_smooth_type='FACE', use_mesh_modifiers=False, path_mode='STRIP', embed_textures=False)
print(f"EXPORTED {name}: {len(full.data.polygons)} caras, altura {top:.3f}")
