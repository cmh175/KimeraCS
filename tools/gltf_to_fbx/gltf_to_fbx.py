"""
gltf_to_fbx.py - turns a KimeraCS glTF export (an FF7 model with its animations) into FBX files for retargeting
in iClone, Maya, MotionBuilder or UE5: one <ANIM>.fbx per animation, each with the model, its skeleton and
that animation (textures embedded), so every file opens on its own (iClone opens each FBX as its own scene).

Runs inside Blender 4.5 in the background (no Blender window):

    blender -b --factory-startup -P gltf_to_fbx.py -- <model.gltf> [--out <folder>] [--height 1.7]
            [--anims ACFE,AAFF | --anims all] [--skeleton-only] [--model-file]

  --out        output folder (default: <gltf folder>\\fbx)
  --height     model height in meters (default 1.7; FF7 units are not metric)
  --anims      which animations to write (default: all)
  --skeleton-only  animation FBXs without the model (skeleton and animation only)
  --model-file     also write <name>.fbx: the model in its rest pose, without animation
  --fps        frame rate of the FBX animations (default: from the glTF timestamps; KimeraCS writes 30 fps for
               field animations, 15 fps for battle animations and 60 fps when converting to 60 fps)

What changes on the way:
  - The FF7 root placement (standing height, walking and jumping travel, turns), which KimeraCS keeps on a
    non-bone "root" node, becomes a "pelvis" bone at the top of the skeleton. The FF7 root joints (field: hip,
    l_hip, r_hip; battle: bone_00, weapon) hang under it, so the skeleton is one tree with the body motion on its
    top bone, the way HumanIK and other retargeting systems expect.
  - Blender space (Z up, facing -Y; FBX units are centimeters after export).
  - The rest pose is KimeraCS's export rest pose (the bind pose of the glTF).
"""
import atexit
import bpy
import json
import os
import shutil
import struct
import sys
import tempfile
import zlib
from mathutils import Matrix, Quaternion, Vector

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
if not argv: raise SystemExit('Usage: blender -b -P gltf_to_fbx.py -- <model.gltf> [--out folder] [--height 1.7] [--anims all]')
opt = {'gltf': argv[0], 'out': None, 'height': 1.7, 'anims': 'all', 'with_mesh': True, 'model_file': False, 'fps': None}
i = 1
while i < len(argv):
    a = argv[i]
    if a == '--out': opt['out'] = argv[i + 1]; i += 2
    elif a == '--height': opt['height'] = float(argv[i + 1]); i += 2
    elif a == '--anims': opt['anims'] = argv[i + 1]; i += 2
    elif a == '--skeleton-only': opt['with_mesh'] = False; i += 1
    elif a == '--model-file': opt['model_file'] = True; i += 1
    elif a == '--fps': opt['fps'] = int(argv[i + 1]); i += 2
    else: raise SystemExit('Unknown argument: ' + a)
gdir = os.path.dirname(os.path.abspath(opt['gltf']))
name = os.path.splitext(os.path.basename(opt['gltf']))[0]
out = opt['out'] or os.path.join(gdir, 'fbx')
os.makedirs(out, exist_ok=True)

report = []
def log(s):
    report.append(s); print(s, flush=True)

# ---------------------------------------------------------------------------------------------- read glTF
with open(opt['gltf'], 'r', encoding='utf-8-sig') as f: G = json.load(f)
with open(os.path.join(gdir, G['buffers'][0]['uri']), 'rb') as f: BIN = f.read()

def read(ai):
    acc = G['accessors'][ai]; view = G['bufferViews'][acc['bufferView']]
    n = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4, 'MAT4': 16}[acc['type']]
    fmt = {5126: 'f', 5121: 'B', 5123: 'H', 5125: 'I', 5122: 'h', 5120: 'b'}[acc['componentType']]
    size = struct.calcsize('<' + fmt)
    stride = view.get('byteStride', n * size)
    base = view.get('byteOffset', 0) + acc.get('byteOffset', 0)
    out_ = [struct.unpack_from('<%d%s' % (n, fmt), BIN, base + k * stride) for k in range(acc['count'])]
    return out_

nodes = G['nodes']
parent = {c: p for p, n in enumerate(nodes) for c in n.get('children', [])}
skin = G['skins'][0]
joints = skin['joints']
jset = set(joints)
top = parent.get(joints[0])                       # KimeraCS "root" node (ff7_root in converted models)

def trs(t, q):
    return Matrix.Translation(Vector(t)) @ Quaternion((q[3], q[0], q[1], q[2])).to_matrix().to_4x4()

def node_trs(ni):
    n = nodes[ni]
    return n.get('translation', (0, 0, 0)), n.get('rotation', (0, 0, 0, 1))

# glTF (Y up, FF7 models face -Z) -> Blender (Z up, facing -Y): (x, y, z) -> (-x, z, y)
C = Matrix(((-1, 0, 0), (0, 0, 1), (0, 1, 0)))
C4 = C.to_4x4()
C4i = C4.inverted()

# size: model height in glTF units from the mesh bounds (body only: battle weapons hang on a "weapon" joint)
ys = []
for n in nodes:
    if 'mesh' in n:
        p0 = G['meshes'][n['mesh']]['primitives'][0]
        if 'JOINTS_0' in p0.get('attributes', {}):
            j0 = read(p0['attributes']['JOINTS_0'])[0][0]
            if nodes[joints[j0]].get('name') == 'weapon': continue
        for p in G['meshes'][n['mesh']]['primitives']:
            a = G['accessors'][p['attributes']['POSITION']]
            if 'min' in a: ys += [a['min'][1], a['max'][1]]
            else: ys += [v[1] for v in read(p['attributes']['POSITION'])]
src_height = (max(ys) - min(ys)) if ys else 1.0
S = opt['height'] / src_height
log('Model: %s (%d joints, height %.2f glTF units -> %.2f m, x%.4f)' % (opt['gltf'], len(joints), src_height, opt['height'], S))

def to_blender(m):
    """glTF world matrix -> rigid Blender matrix, translation scaled to meters."""
    b = C4 @ m @ C4i
    t, q, _ = b.decompose()
    r = q.normalized().to_matrix().to_4x4()
    r.translation = t * S
    return r

# ---------------------------------------------------------------------------------------------- skeleton
# bone list: "pelvis" (the root node's placement) + every joint, parents before children
bones = ['pelvis'] + [nodes[j].get('name', 'joint%d' % j) for j in joints]
bone_of_node = {j: k + 1 for k, j in enumerate(joints)}
bparent = [None]
for j in joints:
    p = parent.get(j)
    bparent.append(bone_of_node[p] if p in jset else 0)
if len(set(bones)) != len(bones): raise SystemExit('Joint names are not unique.')

def world_matrices(local_of):
    """World matrices (glTF space) of [pelvis] + joints, given a function node -> local matrix."""
    root_m = local_of(top) if top is not None else Matrix.Identity(4)
    if top is not None:
        a = parent.get(top)
        while a is not None:
            root_m = local_of(a) @ root_m; a = parent.get(a)
    W = {}
    def w(ni):
        if ni in W: return W[ni]
        p = parent.get(ni)
        m = (w(p) if p in jset else root_m) @ local_of(ni)
        W[ni] = m; return m
    return [root_m] + [w(j) for j in joints]

rest_world = [to_blender(m) for m in world_matrices(lambda ni: trs(*node_trs(ni)))]

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
arm_data = bpy.data.armatures.new(name)
arm = bpy.data.objects.new(name, arm_data)
scene.collection.objects.link(arm)
bpy.context.view_layer.objects.active = arm
bpy.ops.object.mode_set(mode='EDIT')
blen = 0.05 * opt['height']
ebs = []
for k, bn in enumerate(bones):
    eb = arm_data.edit_bones.new(bn)
    eb.head = (0, 0, 0); eb.tail = (0, blen, 0)
    eb.matrix = rest_world[k]
    ebs.append(eb)
for k, p in enumerate(bparent):
    if p is not None: ebs[k].parent = ebs[p]
bpy.ops.object.mode_set(mode='OBJECT')
rest_arm = [arm_data.bones[bn].matrix_local.copy() for bn in bones]   # what Blender actually stored
log('Skeleton: pelvis + %d FF7 joints' % len(joints))

# ---------------------------------------------------------------------------------------------- mesh
def load_png_material(mi):
    m = G['materials'][mi] if mi is not None and mi < len(G.get('materials', [])) else None
    mat = bpy.data.materials.new(m.get('name', 'material%d' % mi) if m else 'material')
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    pbr = (m or {}).get('pbrMetallicRoughness', {})
    if 'baseColorFactor' in pbr and bsdf: bsdf.inputs['Base Color'].default_value = pbr['baseColorFactor']
    tex = pbr.get('baseColorTexture')
    if tex is not None and bsdf:
        img_def = G['images'][G['textures'][tex['index']]['source']]
        path = os.path.join(gdir, img_def.get('uri', ''))
        if os.path.exists(path):
            img = bpy.data.images.load(path, check_existing=True)
            # Transparency only for real decals (eyes, mouth: textures with see-through pixels). Other textures
            # go in without an alpha channel: iClone treats any alpha channel as opacity and then sorts every
            # transparent part against the others (see-through body, eyes that come and go).
            see_through = (m or {}).get('alphaMode') in ('MASK', 'BLEND') and clear_share(img) > 0.01
            if not see_through and img.channels == 4:
                img = rgb_copy(img)
            node = mat.node_tree.nodes.new('ShaderNodeTexImage'); node.image = img
            mat.node_tree.links.new(node.outputs['Color'], bsdf.inputs['Base Color'])
            if see_through:
                mat.node_tree.links.new(node.outputs['Alpha'], bsdf.inputs['Alpha'])
                if hasattr(mat, 'blend_method'): mat.blend_method = 'CLIP'
    return mat

def clear_share(img):
    px = img.pixels[:]
    a = px[3::4]
    return sum(1 for x in a if x < 0.5) / float(max(1, len(a)))

rgb_dir = tempfile.mkdtemp(prefix='gltf_to_fbx_')
atexit.register(shutil.rmtree, rgb_dir, True)          # the copies are embedded in the FBX files; not needed afterwards
def rgb_copy(img):
    """The same picture as an RGB PNG (no alpha channel), loaded for embedding in the FBX."""
    w, h = img.size
    px = img.pixels[:]                          # RGBA floats, rows bottom to top
    rows = []
    for y in range(h - 1, -1, -1):
        row = px[y * w * 4:(y + 1) * w * 4]
        rows.append(b'\x00' + bytes(int(round(max(0.0, min(1.0, v)) * 255)) for k, v in enumerate(row) if k % 4 != 3))
    def chunk(t, d): return struct.pack('>I', len(d)) + t + d + struct.pack('>I', zlib.crc32(t + d) & 0xffffffff)
    signature = bytes((0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A))
    data = signature + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 2, 0, 0, 0)) + \
        chunk(b'IDAT', zlib.compress(b''.join(rows), 6)) + chunk(b'IEND', b'')
    path = os.path.join(rgb_dir, os.path.basename(img.filepath) or (img.name + '.png'))
    with open(path, 'wb') as f: f.write(data)
    return bpy.data.images.load(path, check_existing=True)

materials = {}
mesh_objects = []
for ni, n in enumerate(nodes):
    if 'mesh' not in n: continue
    gm = G['meshes'][n['mesh']]
    verts, faces, uvs, weights, mat_idx = [], [], [], [], []
    slots = []
    for p in gm['primitives']:
        base = len(verts)
        P = read(p['attributes']['POSITION'])
        UV = read(p['attributes']['TEXCOORD_0']) if 'TEXCOORD_0' in p['attributes'] else [(0, 0)] * len(P)
        J = read(p['attributes']['JOINTS_0']); W = read(p['attributes']['WEIGHTS_0'])
        I = [x[0] for x in read(p['indices'])] if 'indices' in p else list(range(len(P)))
        mi = p.get('material')
        if mi not in materials: materials[mi] = load_png_material(mi)
        if materials[mi] not in slots: slots.append(materials[mi])
        si = slots.index(materials[mi])
        for v in P:
            b = C @ Vector(v) * S
            verts.append(tuple(b))
        for k in range(len(P)):
            weights.append([(bones[1 + J[k][q]], W[k][q]) for q in range(4) if W[k][q] > 0])
        for t in range(0, len(I), 3):
            faces.append((base + I[t], base + I[t + 1], base + I[t + 2]))
            uvs.append([(UV[I[t + q]][0], 1.0 - UV[I[t + q]][1]) for q in range(3)])
            mat_idx.append(si)
    me = bpy.data.meshes.new(n.get('name', 'mesh%d' % ni))
    me.from_pydata(verts, [], faces)
    uvl = me.uv_layers.new(name='UVMap')
    for fi, poly in enumerate(me.polygons):
        poly.material_index = mat_idx[fi]
        for q, li in enumerate(poly.loop_indices): uvl.data[li].uv = uvs[fi][q]
    for s in slots: me.materials.append(s)
    me.validate()
    ob = bpy.data.objects.new(me.name, me)
    scene.collection.objects.link(ob)
    ob.parent = arm
    groups = {}
    for vi, ws in enumerate(weights):
        for bn, w in ws:
            g = groups.get(bn) or ob.vertex_groups.new(name=bn)
            groups[bn] = g
            g.add([vi], w, 'REPLACE')
    mod = ob.modifiers.new('Armature', 'ARMATURE'); mod.object = arm
    mesh_objects.append(ob)
log('Meshes: %d parts, %d vertices' % (len(mesh_objects), sum(len(o.data.vertices) for o in mesh_objects)))

# ---------------------------------------------------------------------------------------------- animations
anims = G.get('animations', [])
want = None if opt['anims'].lower() == 'all' else {x.strip().upper() for x in opt['anims'].split(',') if x.strip()}
if want is not None: anims = [a for a in anims if a['name'].upper() in want or a['name'][:4].upper() in want]

def write_fbx(path, objects, with_anim):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects: o.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'ARMATURE', 'MESH'},
                             add_leaf_bones=False, bake_anim=with_anim, bake_anim_use_all_actions=False,
                             bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True,
                             bake_anim_simplify_factor=0.0, bake_anim_step=1.0, path_mode='COPY', embed_textures=True,
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_NONE', mesh_smooth_type='FACE')

# optional model-only FBX in the rest pose
if opt['model_file']:
    arm.animation_data_clear()
    write_fbx(os.path.join(out, name + '.fbx'), [arm] + mesh_objects, False)
    log('Written: %s.fbx (model in its rest pose)' % name)

rest_local = []
for k in range(len(bones)):
    p = bparent[k]
    rest_local.append(rest_arm[k] if p is None else rest_arm[p].inverted() @ rest_arm[k])

for an in anims:
    ch = {}
    times = None
    for c in an['channels']:
        smp = an['samplers'][c['sampler']]
        ch[(c['target'].get('node'), c['target']['path'])] = read(smp['output'])
        if times is None: times = [t[0] for t in read(smp['input'])]
    nf = max(len(v) for v in ch.values())
    fps = opt['fps'] or (round(1.0 / (times[1] - times[0])) if times and len(times) > 1 and times[1] > times[0] else 30)
    def local_at(ni, k):
        t = ch.get((ni, 'translation')); r = ch.get((ni, 'rotation'))
        bt, br = node_trs(ni)
        return trs(t[min(k, len(t) - 1)] if t else bt, r[min(k, len(r) - 1)] if r else br)
    act = bpy.data.actions.new(an['name'])
    arm.animation_data_create(); arm.animation_data.action = act
    curves = {}
    for bn in bones:
        dp = 'pose.bones["%s"]' % bn
        curves[bn] = ([act.fcurves.new(dp + '.location', index=i, action_group=bn) for i in range(3)],
                      [act.fcurves.new(dp + '.rotation_quaternion', index=i, action_group=bn) for i in range(4)])
    vals = {bn: ([[] for _ in range(3)], [[] for _ in range(4)]) for bn in bones}
    prevq = {}
    for k in range(nf):
        Wg = world_matrices(lambda ni: local_at(ni, k))
        Wb = [to_blender(m) for m in Wg]
        for b in range(len(bones)):
            p = bparent[b]
            pose_local = Wb[b] if p is None else Wb[p].inverted() @ Wb[b]
            basis = rest_local[b].inverted() @ pose_local
            t, q, _ = basis.decompose()
            q = q.normalized()
            if bones[b] in prevq and prevq[bones[b]].dot(q) < 0: q = -q
            prevq[bones[b]] = q
            for i in range(3): vals[bones[b]][0][i] += [k, t[i]]
            for i in range(4): vals[bones[b]][1][i] += [k, q[i]]
    for bn in bones:
        for i, fc in enumerate(curves[bn][0]):
            fc.keyframe_points.add(nf); fc.keyframe_points.foreach_set('co', vals[bn][0][i])
        for i, fc in enumerate(curves[bn][1]):
            fc.keyframe_points.add(nf); fc.keyframe_points.foreach_set('co', vals[bn][1][i])
        for fc in curves[bn][0] + curves[bn][1]:
            for kp in fc.keyframe_points: kp.interpolation = 'LINEAR'
            fc.update()
    for pb in arm.pose.bones: pb.rotation_mode = 'QUATERNION'
    # Blender 4.4+ plays an action only through one of its slots
    if hasattr(act, 'slots') and len(act.slots) and getattr(arm.animation_data, 'action_slot', None) is None:
        arm.animation_data.action_slot = act.slots[0]
    scene.render.fps = fps; scene.render.fps_base = 1.0
    scene.frame_start = 0; scene.frame_end = nf - 1          # FBX time starts at 0
    # make sure the action really drives the skeleton before exporting it
    if nf > 1:
        scene.frame_set(0); a0 = [pb.matrix.copy() for pb in arm.pose.bones]
        moved = False
        for k in range(1, nf):
            scene.frame_set(k)
            if any((pb.matrix.translation - m.translation).length > 1e-6 or
                   pb.matrix.to_quaternion().rotation_difference(m.to_quaternion()).angle > 1e-5
                   for pb, m in zip(arm.pose.bones, a0)):
                moved = True; break
        still = all(len(set(round(v, 6) for v in vals[bn][1][i][1::2])) == 1 for bn in bones for i in range(4)) and \
                all(len(set(round(v, 6) for v in vals[bn][0][i][1::2])) == 1 for bn in bones for i in range(3))
        if not moved and not still:
            raise SystemExit('Internal error: the animation %s does not drive the skeleton.' % an['name'])
        scene.frame_set(0)
    write_fbx(os.path.join(out, an['name'] + '.fbx'), [arm] + (mesh_objects if opt['with_mesh'] else []), True)
    log('  %s.fbx: %d frame(s) at %d fps%s' % (an['name'], nf, fps, '' if opt['with_mesh'] else ', skeleton only'))
    arm.animation_data.action = None

log('Done: %d animation(s) -> %s' % (len(anims), out))
with open(os.path.join(out, name + '_fbx_report.txt'), 'w', encoding='utf-8') as f: f.write('\n'.join(report) + '\n')
print('GLTF_TO_FBX_DONE', flush=True)
