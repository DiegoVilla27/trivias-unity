"""Animaciones del presentador (Alegría, Decepción y Hablar) sobre su propio esqueleto.
Construye el rig igual que rig_character.py, define controles (cadera, columna, cabeza, muñecas con IK de dos huesos,
tobillos clavados) con curvas suaves (cúbica monótona: sin pasarse ni frenazos) y hornea cada fotograma a los huesos.
Uso: blender -b -P presenter_anims.py -- <rig_character.py> <base_basic_shaded.fbx> <carpeta salida> [render]"""
import bpy, sys, math
from mathutils import Vector, Matrix, Quaternion

argv = sys.argv[sys.argv.index("--") + 1:]
RIG_SCRIPT, SRC, OUT = argv[0], argv[1], argv[2]
RENDER = len(argv) > 3 and argv[3] == "render"
sys.argv = [sys.argv[0], "--", SRC, OUT + "/rig_check.fbx", "Presenter", OUT + "/rig"]
exec(open(RIG_SCRIPT).read())

FPS = 30
bpy.context.scene.render.fps = FPS  # Las curvas se diseñan a 30 fotogramas por segundo (Blender viene a 24).
bpy.context.scene.render.fps_base = 1.0
rig = bpy.data.objects["ContestantRig"]
body = bpy.data.objects["Body"]
bones = rig.data.bones
REST = {b.name: b.matrix_local.copy() for b in bones}
PARENT = {b.name: (b.parent.name if b.parent else None) for b in bones}
LENGTH = {b.name: b.length for b in bones}
ORDER = []
def visit(b):
    ORDER.append(b.name)
    for c in b.children: visit(c)
visit(bones["Hips"])

def rot3(m): return m.to_3x3().normalized()
def deg(x): return math.radians(x)
def delta(pitch=0.0, yaw=0.0, roll=0.0):
    """Giro en ejes del plató en reposo: pitch hacia delante (+X), yaw hacia su izquierda (+Z), roll (Y)."""
    return Matrix.Rotation(deg(yaw), 3, 'Z') @ Matrix.Rotation(deg(pitch), 3, 'X') @ Matrix.Rotation(deg(roll), 3, 'Y')

# ---- Curvas: cúbica monótona (Fritsch–Carlson) por canal; frena sola en los extremos, nunca se pasa ----
def monotone(keys, f, periodic=None):
    ks = sorted(keys)
    if periodic:
        f = f % periodic
    if f <= ks[0][0]: return ks[0][1]
    if f >= ks[-1][0]: return ks[-1][1]
    xs = [k[0] for k in ks]; ys = [k[1] for k in ks]; n = len(xs)
    d = [(ys[i+1]-ys[i])/(xs[i+1]-xs[i]) for i in range(n-1)]
    m = [0.0]*n
    m[0] = d[0]; m[-1] = d[-1]
    if periodic:
        m[0] = m[-1] = (d[0]+d[-1])*0.5 if d[0]*d[-1] > 0 else 0.0
    for i in range(1, n-1):
        m[i] = 0.0 if d[i-1]*d[i] <= 0 else (d[i-1]+d[i])*0.5
    for i in range(n-1):
        if d[i] == 0: m[i] = m[i+1] = 0.0; continue
        a, b = m[i]/d[i], m[i+1]/d[i]
        s = a*a+b*b
        if s > 9:
            t = 3/math.sqrt(s); m[i] = t*a*d[i]; m[i+1] = t*b*d[i]
    i = max(j for j in range(n-1) if xs[j] <= f)
    h = xs[i+1]-xs[i]; t = (f-xs[i])/h
    h00 = 2*t**3-3*t**2+1; h10 = t**3-2*t**2+t; h01 = -2*t**3+3*t**2; h11 = t**3-t**2
    return h00*ys[i]+h10*h*m[i]+h01*ys[i+1]+h11*h*m[i+1]

def curve(keys, f, periodic=None):
    if isinstance(keys[0][1], (tuple, list, Vector)):
        return Vector([monotone([(k[0], k[1][c]) for k in keys], f, periodic) for c in range(3)])
    return monotone(keys, f, periodic)

# ---- Cinemática ----
def fk(D, name, R=None):
    parent = PARENT[name]
    local = REST[parent].inverted() @ REST[name]
    base = D[parent] @ local
    if R is None: R = Matrix.Identity(3)
    rr = rot3(REST[name])
    D[name] = base @ (rr.inverted() @ R @ rr).to_4x4()

def unposed(D, name):
    parent = PARENT[name]
    return D[parent] @ (REST[parent].inverted() @ REST[name])

def aim(D, name, direction):
    base = unposed(D, name)
    cur = base.to_3x3().normalized().col[1]
    q = cur.rotation_difference(direction.normalized())
    D[name] = Matrix.Translation(base.to_translation()) @ (q.to_matrix() @ rot3(base)).to_4x4()

def absolute(D, name, R3):
    D[name] = Matrix.Translation(unposed(D, name).to_translation()) @ R3.to_4x4()

def two_bone(s, target, l1, l2, pole):
    v = target - s; d = v.length; reach = (l1+l2)*0.999
    if d > reach:
        print(f"  aviso: objetivo a {d:.3f} m, alcance {reach:.3f}")
        d = reach
    dirT = v.normalized()
    a = (l1*l1 - l2*l2 + d*d)/(2*d); h = math.sqrt(max(0.0, l1*l1 - a*a))
    p = pole - dirT*pole.dot(dirT); p = p.normalized() if p.length > 1e-6 else Vector((0, 0, -1))
    return s + dirT*a + p*h, s + dirT*d

def frame_rot(direction, up):
    """Rotación que lleva el eje del hueso (Y) a direction y su Z hacia up."""
    y = direction.normalized(); z = (up - y*up.dot(y)).normalized(); x = y.cross(z)
    return Matrix((x, y, z)).transposed()

REST_HAND_FRAME = {}
for side in ("Left", "Right"):
    rr = rot3(REST[f"{side}Hand"])
    REST_HAND_FRAME[side] = rr

def pose_arm(D, side, wrist, pole, hand=None):
    """IK de brazo; hand=(dirección de los dedos, dirección del micro/pulgar) o None (sigue al antebrazo)."""
    up_name, low_name, hand_name = f"{side}UpperArm", f"{side}LowerArm", f"{side}Hand"
    s = unposed(D, up_name).to_translation()
    elbow, w = two_bone(s, wrist, LENGTH[up_name], LENGTH[low_name], pole)
    aim(D, up_name, elbow - s)
    aim(D, low_name, w - elbow)
    if hand is None:
        fk(D, hand_name)
    else:
        fingers, thumb = hand
        # En reposo los dedos van por el eje del hueso y el pulgar (micro) hacia +Z del plató.
        rest = REST_HAND_FRAME[side]
        rest_frame = frame_rot(rest.col[1], MIC_REST_AXIS)
        new_frame = frame_rot(fingers, thumb)
        absolute(D, hand_name, new_frame @ rest_frame.inverted() @ rest)

ANKLE = {s: REST[f"{s}Foot"].to_translation() for s in ("Left", "Right")}
def pose_leg(D, side, ankle, pole=Vector((0, -1, 0.15))):
    up_name, low_name, foot = f"{side}UpperLeg", f"{side}LowerLeg", f"{side}Foot"
    s = unposed(D, up_name).to_translation()
    knee, a = two_bone(s, ankle, LENGTH[up_name], LENGTH[low_name], pole)
    aim(D, up_name, knee - s); aim(D, low_name, a - knee)
    absolute(D, foot, rot3(REST[foot]))  # Pie plano en el suelo.
    fk(D, f"{side}Toes")

# ---- Pose a partir de controles ----
def build_pose(c):
    D = {}
    rr = rot3(REST["Hips"])
    D["Hips"] = Matrix.Translation(REST["Hips"].to_translation() + Vector((0, 0, c.get("hips_z", 0.0)))) @ \
        (delta(c.get("hips_pitch", 0), c.get("hips_yaw", 0), c.get("hips_roll", 0)) @ rr).to_4x4()
    fk(D, "Spine", delta(c.get("spine_pitch", 0), c.get("spine_yaw", 0), c.get("spine_roll", 0)))
    fk(D, "Chest", delta(c.get("chest_pitch", 0), c.get("chest_yaw", 0), c.get("chest_roll", 0)))
    fk(D, "Neck", delta(c.get("head_pitch", 0)*0.35, c.get("head_yaw", 0)*0.35, c.get("head_roll", 0)*0.35))
    fk(D, "Head", delta(c.get("head_pitch", 0)*0.65, c.get("head_yaw", 0)*0.65, c.get("head_roll", 0)*0.65))
    # Hombros: elevar (+) y adelantar (+).
    fk(D, "LeftShoulder", delta(0, -c.get("l_shoulder_fwd", 0), -c.get("l_shoulder_up", 0)))
    fk(D, "RightShoulder", delta(0, c.get("r_shoulder_fwd", 0), c.get("r_shoulder_up", 0)))
    pose_arm(D, "Left", c["l_wrist"], c.get("l_pole", Vector((0.6, 0.4, -0.7))), c.get("l_hand"))
    pose_arm(D, "Right", c["r_wrist"], c.get("r_pole", Vector((-0.6, 0.4, -0.7))), c.get("r_hand"))
    for side in ("Left", "Right"):
        lift = c.get("feet_z", 0.0)
        pose_leg(D, side, ANKLE[side] + Vector((0, c.get("feet_y", 0.0), lift)))
    return D

def key_pose(D, frame, prev):
    for name in ORDER:
        parent = PARENT[name]
        base = REST[name] if parent is None else D[parent] @ (REST[parent].inverted() @ REST[name])
        B = base.inverted() @ D[name]
        pb = rig.pose.bones[name]
        pb.rotation_mode = 'QUATERNION'
        q = B.to_quaternion()
        if name in prev and prev[name].dot(q) < 0: q.negate()
        prev[name] = q
        pb.rotation_quaternion = q
        pb.keyframe_insert("rotation_quaternion", frame=frame)
        if name == "Hips":
            pb.location = B.to_translation(); pb.keyframe_insert("location", frame=frame)

def bake(name, frames, controls_at):
    action = bpy.data.actions.new(name)
    rig.animation_data_create(); rig.animation_data.action = action
    prev = {}
    for f in range(frames + 1):
        key_pose(build_pose(controls_at(f)), f, prev)
    for fc in action.fcurves:
        for k in fc.keyframe_points: k.interpolation = 'LINEAR'  # Ya es un fotograma por fotograma.
    print(f"ANIM {name}: {frames + 1} fotogramas")
    return action

# ---- Referencias del cuerpo ----
MOUTH = Vector((0.0, -0.445, 1.17))
NEUTRAL_L = Vector((0.40, 0.03, 0.53)); NEUTRAL_R = Vector((-0.40, 0.03, 0.53))
FIST = 0.09  # Del centro del puño a la muñeca, por la mano.
MIC_REST_AXIS = Vector((0, -1, 0))  # En reposo (pose T) el micro apunta al frente: con el brazo caído, sigue al frente.
MIC_REACH = 0.27  # Del centro del puño al centro de la cabeza del micro.

def mic_hand(fist, top, fingers_hint):
    """Muñeca y orientación de la mano derecha para que el micro (eje del pulgar) vaya del puño a la boca."""
    axis = (top - fist).normalized()
    fingers = (fingers_hint - axis*fingers_hint.dot(axis)).normalized()
    return fist - fingers*FIST, (fingers, axis)

# ======================= HABLAR (bucle de 3,2 s) =======================
TALK_FRAMES = 96
def talk(f):
    t = f / TALK_FRAMES * 2*math.pi
    top = MOUTH + Vector((0.0, -0.03, -0.02)) + Vector((0, 0, 0.005*math.sin(2*t)))
    fist = top - Vector((0.10, -0.40, 0.91)).normalized()*MIC_REACH
    r_wrist, r_hand = mic_hand(fist, top, Vector((0.8, -0.4, 0.2)))
    # La otra mano acompaña lo que dice: dos gestos abiertos por ciclo, con pausas.
    l_keys = [(0, (0.40, 0.00, 0.56)), (10, (0.40, -0.02, 0.57)), (24, (0.47, -0.22, 0.73)), (36, (0.43, -0.15, 0.68)),
              (52, (0.48, -0.27, 0.78)), (66, (0.44, -0.12, 0.66)), (82, (0.40, -0.01, 0.57)), (96, (0.40, 0.00, 0.56))]
    l_wrist = curve(l_keys, f, periodic=TALK_FRAMES)
    open_amount = max(0.0, min(1.0, (l_wrist.y*-1 - 0.0)/0.25))
    l_hand = (Vector((0.35, -0.9, 0.25 + 0.3*open_amount)), Vector((0.2*open_amount, -0.3, 1.0)))
    return dict(
        r_wrist=r_wrist, r_hand=r_hand, r_pole=Vector((-0.8, 0.2, -0.6)), r_shoulder_fwd=10, r_shoulder_up=3,
        l_wrist=l_wrist, l_hand=l_hand, l_pole=Vector((0.7, 0.3, -0.65)), l_shoulder_fwd=4*open_amount,
        head_pitch=3.5*math.sin(2*t) + 1.5*math.sin(3*t + 0.6) - 1.5*math.sin(0.6),
        head_yaw=4.0*math.sin(t), head_roll=2.5*math.sin(t + 0.9) - 2.5*math.sin(0.9),
    )

# ======================= ALEGRÍA (2,2 s) =======================
HAPPY_FRAMES = 66
def happy(f):
    hips = [(0, 0), (6, -0.055), (11, 0.05), (15, 0.115), (18, 0.10), (21, 0.0), (24, -0.05), (29, 0.0), (66, 0.0)]
    feet = [(0, 0), (10, 0.0), (14, 0.085), (17, 0.09), (21, 0.0), (66, 0)]
    chest = [(0, 0), (6, 9), (12, -11), (18, -9), (23, 7), (29, -3), (34, 5), (39, -3), (44, 3), (54, 0), (66, 0)]
    head = [(0, 0), (8, 7), (14, -15), (20, -12), (25, 8), (31, -5), (36, 5), (41, -4), (47, 2), (57, 0), (66, 0)]
    roll = [(0, 0), (16, -4), (30, 6), (44, -3), (58, 0), (66, 0)]
    lw = [(0, NEUTRAL_L), (6, (0.44, 0.12, 0.50)), (12, (0.56, -0.13, 1.19)), (18, (0.58, -0.11, 1.24)), (23, (0.55, -0.15, 1.15)),
          (28, (0.43, -0.25, 0.84)), (33, (0.56, -0.16, 1.17)), (38, (0.44, -0.26, 0.84)), (44, (0.47, -0.15, 0.94)),
          (56, (0.41, 0.01, 0.57)), (66, NEUTRAL_L)]
    rw = [(0, NEUTRAL_R), (6, (-0.44, 0.10, 0.50)), (12, (-0.50, -0.14, 0.99)), (20, (-0.52, -0.12, 1.02)), (28, (-0.45, -0.15, 0.84)),
          (44, (-0.44, -0.12, 0.81)), (56, (-0.41, 0.01, 0.57)), (66, NEUTRAL_R)]
    lsh = [(0, 0), (12, 14), (20, 12), (28, 4), (33, 10), (38, 3), (50, 0), (66, 0)]
    return dict(
        hips_z=curve(hips, f), feet_z=curve(feet, f), chest_pitch=curve(chest, f), spine_pitch=curve(chest, f - 1)*0.5,
        head_pitch=curve(head, f), head_roll=curve(roll, f), hips_roll=curve(roll, f - 2)*0.4,
        l_wrist=curve(lw, f), r_wrist=curve(rw, f), l_shoulder_up=curve(lsh, f), r_shoulder_up=curve(lsh, f - 2)*0.5,
        l_pole=Vector((1.0, 0.2, -0.4)), r_pole=Vector((-1.0, 0.2, -0.4)),
    )

# ======================= DECEPCIÓN (2,8 s) =======================
SAD_FRAMES = 84
def sad(f):
    chest = [(0, 0), (9, -6), (21, 15), (68, 13), (84, 0)]
    head = [(0, 0), (10, -7), (25, 21), (66, 19), (84, 0)]
    shake = [(0, 0), (26, 0), (33, 11), (41, -11), (49, 9), (57, -7), (64, 0), (84, 0)]
    shoulders = [(0, 0), (9, 7), (21, -9), (68, -8), (84, 0)]
    hips = [(0, 0), (9, 0.012), (21, -0.03), (68, -0.028), (84, 0)]
    lw = [(0, NEUTRAL_L), (21, (0.36, -0.07, 0.49)), (31, (0.47, -0.19, 0.66)), (41, (0.47, -0.20, 0.67)),
          (52, (0.37, -0.07, 0.49)), (70, (0.37, -0.06, 0.49)), (84, NEUTRAL_L)]
    rw = [(0, NEUTRAL_R), (21, (-0.36, -0.07, 0.49)), (70, (-0.37, -0.06, 0.50)), (84, NEUTRAL_R)]
    return dict(
        chest_pitch=curve(chest, f), spine_pitch=curve(chest, f - 2)*0.45, head_pitch=curve(head, f),
        head_yaw=curve(shake, f), chest_yaw=curve(shake, f - 3)*0.15,
        l_shoulder_up=curve(shoulders, f), r_shoulder_up=curve(shoulders, f - 1), hips_z=curve(hips, f),
        l_wrist=curve(lw, f), r_wrist=curve(rw, f),
        l_pole=Vector((0.8, 0.45, -0.5)), r_pole=Vector((-0.8, 0.45, -0.5)),
    )

CLIPS = [("Presenter_Talk", TALK_FRAMES, talk), ("Presenter_Happy", HAPPY_FRAMES, happy), ("Presenter_Sad", SAD_FRAMES, sad)]
actions = {name: bake(name, frames, fn) for name, frames, fn in CLIPS}

# ---- Micrófono de prueba (en Unity va uno igual, pegado a la mano derecha) ----
def make_mic():
    bpy.context.view_layer.objects.active = rig
    rig.animation_data.action = None
    for pb in rig.pose.bones: pb.rotation_quaternion = (1, 0, 0, 0); pb.location = (0, 0, 0)
    bpy.context.view_layer.update()
    fist = REST["RightHand"].to_translation() + rot3(REST["RightHand"]).col[1]*FIST
    parts = []
    for nm, size, z, color in (("MicHandle", (0.042, 0.28, 0.042), 0.09, (0.05, 0.05, 0.06, 1)), ("MicHead", (0.075, 0.075, 0.075), 0.265, (0.45, 0.46, 0.5, 1))):
        bpy.ops.mesh.primitive_cube_add(size=1)
        o = bpy.context.active_object; o.name = nm; o.scale = size
        mat = bpy.data.materials.new(nm); mat.diffuse_color = color; o.data.materials.append(mat)
        o.matrix_world = Matrix.Translation(fist + MIC_REST_AXIS*z) @ Matrix.Diagonal((*size, 1))
        mw = o.matrix_world.copy()
        o.parent = rig; o.parent_type = 'BONE'; o.parent_bone = "RightHand"
        o.matrix_world = mw
        parts.append(o)
    return parts
mic = make_mic()

# ---- Láminas de comprobación ----
if RENDER:
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'; scene.display.shading.light = 'STUDIO'; scene.display.shading.color_type = 'TEXTURE'
    scene.render.resolution_x, scene.render.resolution_y = 520, 560
    cam = scene.camera
    cam.data.type = 'ORTHO'; cam.data.ortho_scale = 2.3
    views = {"front": ((0.0, -6.0, 1.0), (90, 0, 0)), "side": ((-6.0, -2.2, 1.0), (90, 0, -70))}
    picks = {"Presenter_Talk": [0, 24, 52], "Presenter_Happy": [0, 6, 15, 23, 28, 33, 44], "Presenter_Sad": [0, 9, 25, 36, 49, 70]}
    for name, frames in picks.items():
        rig.animation_data.action = actions[name]
        for view, (loc, rotd) in views.items():
            if name != "Presenter_Talk" and view == "side": continue
            cam.location = loc; cam.rotation_euler = tuple(math.radians(a) for a in rotd)
            for f in frames:
                scene.frame_set(f)
                scene.render.filepath = f"{OUT}/{name}_{view}_{f:03d}.png"
                bpy.ops.render.render(write_still=True)

# ---- Exportar una animación por archivo, con los mismos ajustes que el modelo ----
for o in mic: o.select_set(False)
for name, frames, _ in CLIPS:
    rig.animation_data.action = actions[name]
    bpy.context.scene.frame_start = 0; bpy.context.scene.frame_end = frames
    bpy.ops.object.select_all(action='DESELECT'); rig.select_set(True); bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(filepath=f"{OUT}/{name}.fbx", use_selection=True, object_types={'ARMATURE'},
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=True, axis_forward='-Z', axis_up='Y',
        add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=True,
        bake_anim=True, bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True,
        bake_anim_simplify_factor=0.0, bake_anim_step=1.0, path_mode='STRIP')
    print(f"EXPORTED {name}")
