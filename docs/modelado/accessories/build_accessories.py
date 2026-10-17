"""Build the ten approved rigid accessories in Blender 4.3, in metres.

Run with Blender --background --python <this file>. Blender coordinates: Z up,
-Y wearer forward. OBJ uses Unity Y up, +Z forward. No runtime code is changed.
"""
import bpy
import bmesh
import math
import json
import sys
from pathlib import Path
from mathutils import Vector
from mathutils.geometry import tessellate_polygon

ROOT = Path(__file__).resolve().parents[3]
SOURCE = Path(__file__).resolve().parent
OUT = ROOT / 'Assets/_Project/Modules/Shop/Models/Accessories'
TAU = math.tau
MATS = {}
REPORT = []


def material(name, color, metallic=0, roughness=.35, fabric=False):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if fabric:
        noise = mat.node_tree.nodes.new('ShaderNodeTexNoise')
        noise.inputs['Scale'].default_value = 180
        bump = mat.node_tree.nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .12
        bump.inputs['Distance'].default_value = .00035
        mat.node_tree.links.new(noise.outputs['Fac'], bump.inputs['Height'])
        mat.node_tree.links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
    MATS[name] = mat
    return mat


def finish(obj, name, mat, bevel=0):
    obj.name = name
    obj.data.materials.append(MATS[mat])
    if bevel:
        mod = obj.modifiers.new('Soft manufactured edges', 'BEVEL')
        mod.width = bevel
        mod.segments = 3
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
    if obj.type == 'MESH':
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
    return obj


def mesh(name, verts, faces, mat, bevel=0):
    data = bpy.data.meshes.new(name)
    data.from_pydata(verts, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return finish(obj, name, mat, bevel)


def ellipsoid(name, loc, scale, mat, rotation=None):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=20, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    if rotation:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, name, mat)


def box(name, loc, scale, mat, bevel=.006):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, name, mat, bevel)


def tube(name, points, radius, mat, cyclic=False):
    curve = bpy.data.curves.new(name, 'CURVE')
    curve.dimensions = '3D'
    curve.resolution_u = 8
    curve.bevel_depth = radius
    curve.bevel_resolution = 3
    curve.use_fill_caps = True
    spline = curve.splines.new('BEZIER')
    spline.bezier_points.add(len(points)-1)
    for point, co in zip(spline.bezier_points, points):
        point.co = co
        point.handle_left_type = 'AUTO'
        point.handle_right_type = 'AUTO'
    spline.use_cyclic_u = cyclic
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(MATS[mat])
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target='MESH')
    obj.select_set(False)
    return obj


def lathe(name, profile, mat, sx=1, sy=1, n=96):
    verts = [(r*math.cos(i*TAU/n)*sx, r*math.sin(i*TAU/n)*sy, z)
             for r,z in profile for i in range(n)]
    faces = []
    for j in range(len(profile)):
        k=(j+1)%len(profile)
        for i in range(n):
            q=(i+1)%n
            faces.append((j*n+i,j*n+q,k*n+q,k*n+i))
    return mesh(name,verts,faces,mat,.002)


def polygon_solid(name, points, depth, mat, curve=None, bevel=.004):
    # X/Z outline, front is -Y. Optional curvature wraps the face naturally.
    verts=[]
    for side in [-1,1]:
        for x,z in points:
            y=(curve(x,z) if curve else 0)+side*depth/2
            verts.append((x,y,z))
    n=len(points)
    poly=[Vector((x,z,0)) for x,z in points]
    lookup={tuple(v):i for i,v in enumerate(poly)}
    tris=[[v if isinstance(v,int) else lookup[tuple(v)] for v in tri] for tri in tessellate_polygon([poly])]
    faces=[tuple(reversed(t)) for t in tris]+[tuple(i+n for i in t) for t in tris]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,verts,faces,mat,bevel)


def ring_outline(name, outer, inner, depth, mat, curve=None):
    n=len(outer)
    assert len(inner)==n
    verts=[]
    for yoff in [-depth/2,depth/2]:
        for points in [outer,inner]:
            verts += [(x,(curve(x,z) if curve else 0)+yoff,z) for x,z in points]
    faces=[]
    for i in range(n):
        j=(i+1)%n
        faces += [(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),
                  (i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)]
    return mesh(name,verts,faces,mat,.003)


def crown():
    n=240; verts=[]
    for r,top in [(.306,False),(.306,True),(.283,True),(.283,False)]:
        for i in range(n):
            a=i*TAU/n
            h=.12+.16*((1+math.cos(6*(a+math.pi/2)))/2)**1.4 if top else .025
            verts.append((r*math.cos(a),r*math.sin(a),h))
    faces=[]
    for j in range(4):
        for i in range(n):
            q=(i+1)%n;k=(j+1)%4
            faces.append((j*n+i,j*n+q,k*n+q,k*n+i))
    mesh('Crown open circlet',verts,faces,'Gold',.005)
    for z in [.025,.057]:
        tube('Rolled lower rim',[(.303*math.cos(i*TAU/48),.303*math.sin(i*TAU/48),z) for i in range(48)],.011,'Gold',True)
    for i in range(6):
        a=i*TAU/6-math.pi/2
        ellipsoid('Rounded crown tip',(.301*math.cos(a),.301*math.sin(a),.285),(.027,.027,.027),'Gold')
        loc=Vector((.309*math.cos(a),.309*math.sin(a),.091))
        socket=ellipsoid('Gem gold setting',loc,(.034,.014,.043),'Gold')
        socket.rotation_euler.z=a+math.pi/2
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=loc+Vector((.011*math.cos(a),.011*math.sin(a),0)))
        gem=bpy.context.object;gem.scale=(.027,.012,.035);gem.rotation_euler.z=a+math.pi/2
        finish(gem,'Faceted sapphire','Sapphire')
        for p in gem.data.polygons:p.use_smooth=False


def bow_tie():
    for sign in [-1,1]:
        verts=[];nu=28;nv=40
        for i in range(nu+1):
            u=i/nu
            x=sign*(.017+.185*u)
            h=.033+.064*math.sin(math.pi*u*.65)
            for j in range(nv):
                a=j*TAU/nv
                fold=1+.08*math.cos(a*4)*(1-u)
                verts.append((x, .025*math.sin(a)*fold*(.75+.25*math.sin(math.pi*u)),h*math.cos(a)*fold))
        faces=[]
        for i in range(nu):
            for j in range(nv):faces.append((i*nv+j,i*nv+(j+1)%nv,(i+1)*nv+(j+1)%nv,(i+1)*nv+j))
        faces += [tuple(reversed(range(nv))),tuple(nu*nv+j for j in range(nv))]
        mesh('Satin bow wing',verts,faces,'BurgundyFabric',.004)
    box('Wrapped center knot',(0,-.005,0),(.065,.065,.09),'BurgundyFabric',.018)
    box('Rear clip',(0,.037,0),(.09,.018,.025),'Charcoal',.006)


def top_hat():
    lathe('Crown shell',[(.255,.027),(.274,.36),(.27,.375),(.002,.375),(.002,.359),(.251,.355),(.234,.027)],'BlackFabric',sy=.88)
    lathe('Oval rounded brim',[(.237,.027),(.354,.027),(.365,.018),(.36,.004),(.235,.004)],'Charcoal',sy=.88)
    lathe('Burgundy satin band',[(.253,.035),(.26,.12),(.268,.12),(.261,.035)],'BurgundyFabric',sy=.88)
    box('Rear ribbon seam',(0,.232,.077),(.015,.006,.08),'BurgundyFabric',.002)


def star_points(cx,scale):
    return [(cx+scale*(1 if i%2==0 else .48)*math.sin(i*TAU/10),scale*(1 if i%2==0 else .48)*math.cos(i*TAU/10)) for i in range(10)]


def star_glasses():
    for sign in [-1,1]:
        ring_outline('Star gold frame',star_points(sign*.154,.151),star_points(sign*.154,.124),.025,'Gold')
        polygon_solid('Navy star lens',star_points(sign*.154,.125),.013,'NavyLens',bevel=.003)
        box('Temple hinge',(sign*.285,.014,.025),(.018,.03,.035),'Gold',.004)
        tube('Open navy temple',[(sign*.285,.015,.025),(sign*.31,.10,.02),(sign*.325,.25,.005),(sign*.32,.32,-.04)],.012,'Navy')
    tube('Gold nose bridge',[(-.04,0,.008),(0,-.008,.025),(.04,0,.008)],.012,'Gold')


def mustache():
    for sign in [-1,1]:
        # Swept elliptical solid locks, tapered toward the curl tips.
        controls=[(0,-.015,.01),(.06,-.045,.035),(.14,-.04,-.002),(.22,-.02,-.02),(.28,0,.005),(.305,.004,.068),(.28,.003,.098),(.263,0,.082)]
        radii=[.035,.053,.051,.038,.025,.018,.012,.004]
        # Catmull-Rom samples avoid segmented curve silhouettes.
        points=[];rs=[]
        for i in range(len(controls)-1):
            p0=Vector(controls[max(i-1,0)]);p1=Vector(controls[i]);p2=Vector(controls[i+1]);p3=Vector(controls[min(i+2,len(controls)-1)])
            for j in range(10):
                t=j/10
                points.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t))
                rs.append(radii[i]*(1-t)+radii[i+1]*t)
        points.append(Vector(controls[-1]));rs.append(radii[-1])
        verts=[];faces=[];n=20
        for i,p in enumerate(points):
            tangent=(points[min(i+1,len(points)-1)]-points[max(i-1,0)]).normalized()
            side=Vector((0,1,0));up=tangent.cross(side).normalized()
            for j in range(n):
                a=j*TAU/n;v=p+rs[i]*(up*math.cos(a)+side*.64*math.sin(a));verts.append((sign*v.x,v.y,v.z))
        for i in range(len(points)-1):
            for j in range(n):faces.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
        faces += [tuple(reversed(range(n))),tuple((len(points)-1)*n+j for j in range(n))]
        mesh('Curled mustache lobe',verts,faces,'Hair')
        for k in [-1,0,1]:
            pts=[(sign*p.x,p.y-r*.64*.93,p.z+k*r*.38) for p,r in zip(points[::5],rs[::5])]
            tube('Sculpted hair ridge',pts,.0012,'HairHighlight')


def pirate_hat():
    # A genuine hollow wearable cap under a swept three-corner brim.
    lathe('Hollow navy cap',[(.245,.01),(.25,.08),(.225,.18),(.14,.245),(.002,.26),(.002,.24),(.12,.226),(.205,.165),(.23,.075),(.225,.01)],'NavyFabric',sy=.88)
    n=144;verts=[]
    def edge(a):
        r=.343+.06*math.cos(3*(a+math.pi/2))
        z=.075+.11*(.5+.5*math.cos(3*(a+math.pi/2)))
        return (r*math.cos(a),r*math.sin(a)*.88,z)
    for side in [-1,1]:
        for ring in range(8):
            u=ring/7
            for i in range(n):
                a=i*TAU/n;x,y,z=edge(a)
                verts.append(((.245*(1-u)+math.hypot(x,y/.88)*u)*math.cos(a),(.245*(1-u)+math.hypot(x,y/.88)*u)*math.sin(a)*.88,.027*(1-u)+z*u+side*.008))
    faces=[];s=8*n
    for side in range(2):
        off=side*s
        for ring in range(7):
            for i in range(n):
                j=(i+1)%n;faces.append((off+ring*n+i,off+ring*n+j,off+(ring+1)*n+j,off+(ring+1)*n+i))
    for ring in [0,7]:
        for i in range(n):
            j=(i+1)%n;faces.append((ring*n+i,ring*n+j,s+ring*n+j,s+ring*n+i))
    mesh('Tricorn swept brim',verts,faces,'NavyFabric')
    tube('Gold rolled tricorn edge',[edge(i*TAU/72) for i in range(72)],.013,'Gold',True)
    # Proud high front fold carries the reference skull.
    panel=[(-.30,.04),(-.26,.13),(-.15,.22),(0,.30),(.15,.22),(.26,.13),(.30,.04)]
    polygon_solid('High front fold',panel,.027,'NavyFabric',lambda x,z:-.39+.12*(x/.3)**2,.009)
    tube('Front fold gold edge',[(x,-.405+.12*(x/.3)**2,z) for x,z in panel],.012,'Gold')
    ellipsoid('Ivory skull',(0,-.419,.175),(.052,.016,.059),'Ivory')
    box('Skull jaw',(0,-.42,.12),(.066,.028,.035),'Ivory',.008)
    for x in [-.021,.021]:ellipsoid('Skull eye socket',(x,-.435,.181),(.014,.006,.017),'Charcoal')
    polygon_solid('Skull nose',[(-.008,.148),(.008,.148),(0,.16)],.006,'Charcoal',lambda x,z:-.438,.001)
    for x in [-.014,.014]:box('Skull tooth groove',(x,-.436,.112),(.004,.003,.018),'Charcoal',.001)


def superhero_mask():
    curve=lambda x,z:.9*x*x
    n=64
    for sign in [-1,1]:
        outer=[];inner=[]
        for i in range(n):
            a=i*TAU/n
            outer.append((sign*(.137+.142*math.cos(a)),.005+.105*math.sin(a)+.025*math.cos(a)))
            inner.append((sign*(.137+.088*math.cos(a)),.005+.052*math.sin(a)+.014*math.cos(a)))
        ring_outline('Curved satin mask half',outer,inner,.02,'BurgundyFabric',curve)
        for name,points in [('Outer gold piping',outer),('Eye opening gold piping',inner)]:
            tube(name,[(x,curve(x,z)-.013,z) for x,z in points[::2]],.004,'Gold',True)
    polygon_solid('Curved nose bridge',[(-.04,.068),(.04,.068),(.038,-.017),(0,-.044),(-.038,-.017)],.022,'BurgundyFabric',lambda x,z:-.012-.035*max(0,-z/.044),.008)
    tube('Nose bridge piping',[(-.038,-.025,-.017),(0,-.053,-.044),(.038,-.025,-.017)],.004,'Gold')


def halo():
    tube('Floating gold halo',[(.31*math.cos(i*TAU/64),.245*math.sin(i*TAU/64),0) for i in range(64)],.024,'Gold',True)


def angel_wings():
    box('Central back mount',(0,0,0),(.055,.052,.105),'Gold',.014)
    for sign in [-1,1]:
        ellipsoid('Wing root',(sign*.062,0,.015),(.07,.038,.11),'Ivory')
        feathers=[(.15,.10,.14,.75),(.205,.04,.125,1.1),(.21,-.02,.11,1.4),(.17,-.07,.10,1.8),(.105,-.10,.08,2.1)]
        for i,(x,z,length,angle) in enumerate(feathers):
            obj=ellipsoid('Rounded primary feather',(sign*x,.006,z),(.055,.029,length),'Ivory')
            obj.rotation_euler.y=sign*angle
        for x,z,angle in [(.075,.07,-.3),(.10,.005,-.6),(.095,-.055,-.8)]:
            obj=ellipsoid('Layered soft covert',(sign*x,-.025,z),(.04,.025,.072),'Cream')
            obj.rotation_euler.y=sign*angle


def dragon_wings():
    box('Central back mount',(0,0,0),(.06,.048,.105),'Charcoal',.013)
    for sign in [-1,1]:
        # Curved scalloped membrane outline, with structural ribs on both sides.
        outline=[(.027,.025),(.10,.09),(.18,.23),(.22,.16),(.29,.115),(.35,.07),(.29,.015),(.32,-.13),(.24,-.08),(.18,-.055),(.155,-.18),(.10,-.115),(.035,-.08)]
        polygon_solid('Burgundy wing membrane',[(sign*x,z) for x,z in outline],.016,'BurgundyLeather',lambda x,z:.045*abs(x),.008)
        border=[(sign*x,.045*x,z) for x,z in outline]
        tube('Rounded dark wing border',border,.012,'Charcoal',True)
        for end in [( .35,.07),(.32,-.13),(.155,-.18)]:
            for yoffset in [-.012,.012]:
                tube('Wing structural rib',[(sign*.18,yoffset+.008,.23),(sign*(.18+end[0])*.5,yoffset+.012,(.23+end[1])*.5),(sign*end[0],yoffset+.045*end[0],end[1])],.009,'Charcoal')
        for x,z in [(.18,.23),(.35,.07),(.32,-.13),(.155,-.18)]:
            ellipsoid('Rounded gold tip',(sign*x,.045*x,z),(.015,.015,.015),'Gold')


BUILDERS=[('Crown',crown,'Head','Origin at circlet seating plane'),('BowTie',bow_tie,'Neck','Origin at knot center'),('TopHat',top_hat,'Head','Origin at brim seating plane'),('StarGlasses',star_glasses,'Face','Origin at nose bridge'),('Mustache',mustache,'Face','Origin at upper lip center'),('PirateHat',pirate_hat,'Head','Origin at cap seating plane'),('SuperheroMask',superhero_mask,'Face','Origin at nose bridge'),('Halo',halo,'Head','Origin at ring center; place above hair'),('AngelWings',angel_wings,'Back','Origin at central back mount'),('DragonWings',dragon_wings,'Back','Origin at central back mount')]


def recalculate_and_uv(objects):
    for obj in objects:
        bm=bmesh.new();bm.from_mesh(obj.data)
        bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-7)
        bm.to_mesh(obj.data);bm.free()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.02)
    bpy.ops.object.mode_set(mode='OBJECT')


def camera_at(center, distance, direction):
    cam=bpy.context.scene.camera
    cam.location=Vector(center)+Vector(direction).normalized()*distance
    cam.rotation_euler=(Vector(center)-cam.location).to_track_quat('-Z','Y').to_euler()


def studio(objects):
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=24
    scene.cycles.use_denoising=True
    scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.film_transparent=True
    scene.world.color=(.32,.32,.32)
    scene.view_settings.view_transform='AgX'
    scene.view_settings.look='AgX - Medium High Contrast'
    verts=[obj.matrix_world@v.co for obj in objects for v in obj.data.vertices]
    lo=Vector(tuple(min(v[i] for v in verts) for i in range(3)))
    hi=Vector(tuple(max(v[i] for v in verts) for i in range(3)))
    center=(lo+hi)/2;span=max(hi-lo)
    bpy.ops.object.camera_add();scene.camera=bpy.context.object
    scene.camera.data.type='ORTHO';scene.camera.data.ortho_scale=span*1.4
    for loc,power,size in [((-2,-3,4),400,3),((3,-1,2),220,2.5),((0,3,3),450,2)]:
        bpy.ops.object.light_add(type='AREA',location=loc)
        lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size
        lamp.rotation_euler=(center-lamp.location).to_track_quat('-Z','Y').to_euler()
    return center,span,lo,hi


def main():
    selected = sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    previous = json.loads((SOURCE/'manifest.json').read_text()) if (SOURCE/'manifest.json').exists() else []
    REPORT.extend(item for item in previous if selected and item['name'] not in selected)
    for name,builder,slot,pivot in BUILDERS:
        if selected and name not in selected:
            continue
        bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
        MATS.clear()
        for key,rgb,metal,rough,fabric in [
            ('Gold',(.72,.37,.055),.8,.23,False),('Sapphire',(.012,.10,.34),.35,.18,False),
            ('BurgundyFabric',(.20,.012,.031),0,.43,True),('BlackFabric',(.016,.019,.026),0,.48,True),
            ('Charcoal',(.022,.026,.035),.12,.3,False),('Navy',(.018,.043,.105),.05,.28,False),
            ('NavyFabric',(.018,.043,.105),0,.48,True),('NavyLens',(.009,.025,.065),.3,.15,False),
            ('Hair',(.033,.028,.029),0,.38,False),('HairHighlight',(.05,.043,.043),0,.4,False),
            ('Ivory',(.86,.76,.58),0,.3,False),('Cream',(.75,.64,.46),0,.36,False),
            ('BurgundyLeather',(.19,.012,.031),0,.4,True)]:
            material(name+'_'+key,rgb,metal,rough,fabric)
            MATS[key]=MATS.pop(name+'_'+key)
        builder()
        objects=[obj for obj in bpy.context.scene.objects if obj.type=='MESH']
        recalculate_and_uv(objects)
        folder=OUT/name;folder.mkdir(parents=True,exist_ok=True)
        sources=SOURCE/name;sources.mkdir(parents=True,exist_ok=True)
        bpy.ops.wm.obj_export(filepath=str(folder/(name+'.obj')),export_selected_objects=True,forward_axis='Z',up_axis='Y',export_materials=True,export_triangulated_mesh=True,apply_modifiers=True)
        bpy.ops.export_scene.gltf(filepath=str(sources/(name+'.glb')),export_format='GLB',use_selection=True)
        center,span,lo,hi=studio(objects)
        # Preview and four actual rotations; unlike concept images these show the geometry.
        for view,direction in [('Preview',(1,-3,1.5)),('Front',(0,-1,.12)),('Back',(0,1,.12)),('Left',(-1,0,.12)),('Right',(1,0,.12))]:
            camera_at(center,3,direction)
            bpy.context.scene.render.filepath=str(sources/(name+'_'+view+'.png'))
            bpy.ops.render.render(write_still=True)
        # Source stores the useful preview camera; modeling objects retain their pivots.
        camera_at(center,3,(1,-3,1.5))
        bpy.ops.wm.save_as_mainfile(filepath=str(sources/(name+'.blend')))
        triangles=sum(len(poly.vertices)-2 for obj in objects for poly in obj.data.polygons)
        used={mat.name: {'linear_color':list(mat.diffuse_color),'metallic':mat.node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value,'roughness':mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value} for obj in objects for mat in obj.data.materials}
        REPORT.append({'name':name,'slot':slot,'pivot':pivot,'triangles':triangles,'parts':len(objects),'bounds_blender':{'min':list(lo),'max':list(hi)},'materials':used})
        (SOURCE/'manifest.json').write_text(json.dumps(REPORT,indent=2))
        print('ACCESSORY_COMPLETE',name,triangles,flush=True)


if __name__=='__main__':
    main()
