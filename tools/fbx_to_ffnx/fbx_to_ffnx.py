"""
fbx_to_ffnx.py - converts a skinned character (FBX from Character Creator / iClone, Maya, UE5 ...) into a glTF that
FFNx's smooth-skinned model loader accepts, as a stand-in for an FF7 model.

Runs inside Blender 4.5 in the background (no Blender window):

    blender -b --factory-startup -P fbx_to_ffnx.py -- --reference AAAC.gltf --name AAAC --out <folder>
            --model Character.fbx --anim Idle.fbx=ACFE --anim Walk.fbx=AAFF

  --reference  a KimeraCS glTF export of the FF7 model being replaced (same animations). It supplies the
               target height (the new model is scaled to the same height), the frame count of each animation
               (compared) and, for --root-motion game, the game's root motion per frame.
  --root-motion  gltf (default): the new model's animation is kept as made, hip height and travel included.
               The "ff7_root" node gets still keys, which tells FFNx (with its animation-independence work,
               2026-10) to use the glTF's own root motion instead of the game's. FFNx 1.24.0 ignores root
               motion entirely, so this is right there too.
               game: for FFNx builds that apply the game's .a root motion (standing height, jumps) on top of
               the glTF joints: that root motion is taken back out of the new animation frame by frame (taken
               from the reference), so the model stands and moves where the original did.
  --name       glTF file name = one of the FF7 model's .p names (FFNx looks the model up by it), e.g. AAAC
  --model      FBX with the mesh and skeleton (default: the --anim file of the reference's first animation,
               normally the idle; else the first --anim file)
  --anim       FBX=NAME: an FBX animation and the FF7 animation it replaces (4-letter name, e.g. ACFE).
               Without "=NAME" the file name is used (ACFE.fbx -> ACFE).
  --out        output folder (the .gltf, .bin and a textures folder with PNG + DDS go there)
  --height     target height in FF7 units instead of the reference's
  --max-texture  largest texture side in pixels (default 1024)

Conventions (same as KimeraCS's exporter, see GltfRigExporter.cs):
  - joints are in FF7's own space (Y down) under a scene node "ff7_root" that holds the 180 degree turn (and,
    for --root-motion game, the game's root motion for viewers)
  - every joint has translation and rotation keys in every animation, one key per frame
  - each accessor in its own bufferView, JOINTS_0 unsigned byte, skin joints in depth-first order
  - image names match their file names; textures written as PNG and uncompressed DDS
"""
import bpy
import json
import math
import os
import struct
import sys
import zlib
from mathutils import Matrix, Quaternion, Vector

# ---------------------------------------------------------------------------------------------- arguments
argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
opt = {'anims': [], 'model': None, 'reference': None, 'name': None, 'out': None, 'height': None, 'max_texture': 1024,
       'root_motion': 'gltf'}
i = 0
while i < len(argv):
    a = argv[i]
    if a == '--anim': opt['anims'].append(argv[i + 1]); i += 2
    elif a in ('--model', '--reference', '--name', '--out'): opt[a[2:]] = argv[i + 1]; i += 2
    elif a == '--height': opt['height'] = float(argv[i + 1]); i += 2
    elif a == '--max-texture': opt['max_texture'] = int(argv[i + 1]); i += 2
    elif a == '--root-motion':
        opt['root_motion'] = argv[i + 1].lower(); i += 2
        if opt['root_motion'] not in ('gltf', 'game'): raise SystemExit('--root-motion is gltf or game')
    elif a == '--folder':
        # Folder mode (fbx_to_ffnx.bat): <folder>\reference\<NAME>.gltf is the KimeraCS export of the FF7 model,
        # <folder>\character.fbx (or model.fbx) the mesh, every other .fbx an animation named after its file
        # (ACFE.fbx -> ACFE). Output: <folder>\ffnx\<NAME>.gltf.
        folder = argv[i + 1]; i += 2
        refs = [f for f in os.listdir(os.path.join(folder, 'reference'))] if os.path.isdir(os.path.join(folder, 'reference')) else []
        refs = [f for f in refs if f.lower().endswith('.gltf')]
        if len(refs) != 1:
            raise SystemExit('Put exactly one KimeraCS export (.gltf + .bin) of the FF7 model in %s\\reference' % folder)
        opt['reference'] = os.path.join(folder, 'reference', refs[0])
        opt['name'] = opt['name'] or os.path.splitext(refs[0])[0]
        opt['out'] = opt['out'] or os.path.join(folder, 'ffnx')
        fbx = sorted(f for f in os.listdir(folder) if f.lower().endswith('.fbx'))
        model = [f for f in fbx if os.path.splitext(f)[0].lower() in ('character', 'model')]
        if model: opt['model'] = os.path.join(folder, model[0])
        opt['anims'] += [os.path.join(folder, f) for f in fbx if f not in model]
    else: raise SystemExit('Unknown argument: ' + a)
if not opt['anims'] or not opt['name'] or not opt['out']:
    raise SystemExit('Needs --name, --out and at least one --anim (see the top of fbx_to_ffnx.py).')

anims = []
for a in opt['anims']:
    path, _, name = a.partition('=')
    if not name: name = os.path.splitext(os.path.basename(path))[0]
    anims.append((path, name.upper()))
model_path = opt['model']
os.makedirs(os.path.join(opt['out'], 'textures'), exist_ok=True)

report = []
warnings = []
def log(s):
    report.append(s)
    print(s, flush=True)

# ---------------------------------------------------------------------------------------------- reference
ref = None
if opt['reference']:
    with open(opt['reference'], 'r', encoding='utf-8-sig') as f:
        ref = json.load(f)
    with open(os.path.join(os.path.dirname(opt['reference']), ref['buffers'][0]['uri']), 'rb') as f:
        ref_bin = f.read()

def ref_read(ai):
    acc = ref['accessors'][ai]; view = ref['bufferViews'][acc['bufferView']]
    n = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4, 'MAT4': 16}[acc['type']]
    fmt = {5126: 'f', 5121: 'B', 5123: 'H', 5125: 'I'}[acc['componentType']]
    vals = struct.unpack_from('<%d%s' % (acc['count'] * n, fmt), ref_bin, view.get('byteOffset', 0) + acc.get('byteOffset', 0))
    return [vals[k * n:(k + 1) * n] for k in range(acc['count'])]

F4 = Matrix.Diagonal((-1, -1, 1, 1))     # FF7 (Y down) <-> glTF viewer space: 180 degrees about Z
RG = Matrix(((1, 0, 0), (0, 0, -1), (0, 1, 0)))   # Blender world (Z up, faces -Y) -> FF7 space (Y down, faces +Z)
RG_Q = RG.to_quaternion()

def trs(t, q):
    return Matrix.Translation(Vector(t)) @ Quaternion((q[3], q[0], q[1], q[2])).to_matrix().to_4x4()

ref_root_rest = Matrix.Identity(4)
ref_height = None
ref_anims = {}
if ref:
    nodes = ref['nodes']
    joint_set = set(ref['skins'][0]['joints'])
    parent = {c: p for p, n in enumerate(nodes) for c in n.get('children', [])}
    root_idx = parent.get(ref['skins'][0]['joints'][0])
    if root_idx is not None:
        rn = nodes[root_idx]
        ref_root_rest = trs(rn.get('translation', (0, 0, 0)), rn.get('rotation', (0, 0, 0, 1)))
    ys = []
    for n in nodes:
        if 'mesh' in n:
            for p in ref['meshes'][n['mesh']]['primitives']:
                acc = ref['accessors'][p['attributes']['POSITION']]
                ys += [acc['min'][1], acc['max'][1]]
    ref_height = max(ys) - min(ys)
    for an in ref.get('animations', []):
        rt = rr = None; frames = 0
        for c in an['channels']:
            out = ref_read(an['samplers'][c['sampler']]['output'])
            if c['target'].get('node') == root_idx:
                if c['target']['path'] == 'translation': rt = out
                elif c['target']['path'] == 'rotation': rr = out
            elif c['target']['path'] == 'rotation':
                frames = max(frames, len(out))
        ref_anims[an['name'][:4].upper()] = (rt, rr, frames)
    log('Reference: %s (height %.2f FF7 units, animations %s)' % (opt['reference'], ref_height, ', '.join(sorted(ref_anims))))
else:
    warnings.append('No --reference: no height match' +
                    (' and no root-motion compensation (the model may float in FFNx).' if opt['root_motion'] == 'game' else '.'))

def game_root(name, k):
    """The game's root placement (FF7 space) at frame k of animation `name`, from the reference. With
    --root-motion gltf the new animation carries its own placement: none."""
    if opt['root_motion'] == 'gltf':
        return Matrix.Identity(4)
    a = ref_anims.get(name[:4])
    if not a or not a[0] or not a[1]:
        return F4 @ ref_root_rest
    rt, rr, _ = a
    k = min(k, len(rt) - 1, len(rr) - 1)
    return F4 @ trs(rt[k], rr[k])     # the reference's root node = F4 @ game root

def rigid_inverse(m):
    r = m.to_3x3().transposed()
    t = -(r @ m.to_translation())
    out = r.to_4x4(); out.translation = t
    return out

# ---------------------------------------------------------------------------------------------- FBX import
def import_fbx(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.abspath(path), automatic_bone_orientation=False)
    arms = [o for o in bpy.data.objects if o.type == 'ARMATURE']
    if not arms: raise SystemExit('No skeleton (armature) in ' + path)
    arm = max(arms, key=lambda o: len(o.data.bones))
    return arm

def decompose(m):
    t, q, s = m.decompose()
    return t, q.normalized(), s

def to_game(m_world, scale):
    """Blender world matrix -> rigid FF7-space matrix (uniform scale folded into translations)."""
    t, q, _ = decompose(m_world)
    out = (RG_Q @ q).to_matrix().to_4x4()
    out.translation = (RG @ t) * scale
    return out

# ---------------------------------------------------------------------------------------------- model
if not model_path:
    # every FBX holds the model too: take it from the reference's first animation (KimeraCS puts the default idle
    # first), else from the first animation given
    first = [a['name'][:4].upper() for a in ref.get('animations', [])] if ref else []
    pick = [p for p, n in anims if first and n[:4] == first[0]]
    model_path = pick[0] if pick else anims[0][0]
log('Model: ' + model_path)
arm = import_fbx(model_path)

def keep(bone):
    return bone.use_deform or any(keep(c) for c in bone.children)
joints = []
def walk(b):
    if not keep(b): return
    joints.append(b.name)
    for c in b.children: walk(c)
for b in arm.data.bones:
    if b.parent is None: walk(b)
jindex = {n: i for i, n in enumerate(joints)}
jparent = [jindex.get(arm.data.bones[n].parent.name) if arm.data.bones[n].parent else None for n in joints]
log('Skeleton: %d joints (%s ...)' % (len(joints), ', '.join(joints[:4])))
if len(joints) > 255:
    warnings.append('%d joints: no FFNx build supports more than 255 (bgfx limit); reduce the skeleton.' % len(joints))
elif len(joints) > 128:
    warnings.append('%d joints: FFNx 1.24.0 uses at most 128; newer builds up to 255.' % len(joints))

meshes = [o for o in bpy.data.objects if o.type == 'MESH' and any(m.type == 'ARMATURE' for m in o.modifiers)]
zs = [(o.matrix_world @ v.co).z for o in meshes for v in o.data.vertices]
src_height = max(zs) - min(zs)
target_height = opt['height'] or ref_height
scale = target_height / src_height if target_height else 1.0
log('Size: %.3f in the FBX -> %.2f FF7 units (x%.3f)' % (src_height, src_height * scale, scale))

rest_game = [to_game(arm.matrix_world @ arm.data.bones[n].matrix_local, scale) for n in joints]
root_rest = F4 @ ref_root_rest if opt['root_motion'] == 'game' else Matrix.Identity(4)
log('Root motion: ' + ('from the game (taken back out of the new animation)' if opt['root_motion'] == 'game'
                       else 'the new animation\'s own (glTF root motion)'))

def locals_from_world(world_game, groot):
    inv_root = rigid_inverse(groot)
    jw = [inv_root @ w for w in world_game]
    out = []
    for i, w in enumerate(jw):
        p = jparent[i]
        l = rigid_inverse(jw[p]) @ w if p is not None else w
        t, q, _ = decompose(l)
        out.append((t, q))
    return out

rest_local = locals_from_world(rest_game, root_rest)
ibm = [rigid_inverse(F4 @ w) for w in rest_game]

# ---------------------------------------------------------------------------------------------- textures
def png_bytes(w, h, rgba_rows):
    raw = b''.join(b'\x00' + r for r in rgba_rows)
    def chunk(t, d): return struct.pack('>I', len(d)) + t + d + struct.pack('>I', zlib.crc32(t + d) & 0xffffffff)
    return b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6, 0, 0, 0)) + \
        chunk(b'IDAT', zlib.compress(raw, 6)) + chunk(b'IEND', b'')

def dds_bytes(w, h, bgra):
    """Uncompressed 32-bit DDS, legacy header, one level (same layout as KimeraCS's DDSWriter)."""
    hdr = struct.pack('<4sIIIIIII44xIIIIIIIII16x', b'DDS ', 124, 0x1 | 0x2 | 0x4 | 0x8 | 0x1000, h, w, w * 4, 0, 1,
                      32, 0x41, 0, 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000, 0x1000)
    return hdr + bgra

def base_color_image(mat):
    if not mat or not mat.use_nodes: return None, None
    for n in mat.node_tree.nodes:
        if n.type == 'BSDF_PRINCIPLED':
            inp = n.inputs.get('Base Color')
            col = tuple(inp.default_value) if inp else (1, 1, 1, 1)
            if inp and inp.is_linked:
                src = inp.links[0].from_node
                if src.type == 'TEX_IMAGE' and src.image: return src.image, col
            return None, col
    return None, None

def uses_alpha(mat):
    if not mat or not mat.use_nodes: return False
    for n in mat.node_tree.nodes:
        if n.type == 'BSDF_PRINCIPLED':
            a = n.inputs.get('Alpha')
            return bool(a and (a.is_linked or a.default_value < 0.999))
    return False

texture_cache = {}
def write_texture(img, image_name):
    """Writes PNG + DDS; returns (image name, share of see-through pixels), or (None, 0)."""
    if img.name in texture_cache: return texture_cache[img.name]
    w, h = img.size
    if w == 0 or h == 0: return None, 0.0
    src = img
    m = max(w, h)
    if m > opt['max_texture']:
        src = img.copy()
        f = opt['max_texture'] / m
        src.scale(max(1, int(w * f)), max(1, int(h * f)))
        w, h = src.size
    px = list(src.pixels[:])            # float RGBA, rows bottom to top
    rows_rgba = []
    rows_bgra = []
    for y in range(h - 1, -1, -1):
        base = y * w * 4
        rgba = bytes(max(0, min(255, int(round(v * 255)))) for v in px[base:base + w * 4])
        rows_rgba.append(rgba)
        b = bytearray(rgba)
        b[0::4], b[2::4] = rgba[2::4], rgba[0::4]
        rows_bgra.append(bytes(b))
    tdir = os.path.join(opt['out'], 'textures')
    with open(os.path.join(tdir, image_name + '.png'), 'wb') as f: f.write(png_bytes(w, h, rows_rgba))
    with open(os.path.join(tdir, image_name + '.dds'), 'wb') as f: f.write(dds_bytes(w, h, b''.join(rows_bgra)))
    clear = sum(1 for r in rows_rgba for a in r[3::4] if a < 128) / float(w * h)
    texture_cache[img.name] = (image_name, clear)
    log('  texture %s -> textures\\%s.png + .dds (%dx%d%s)' % (img.name, image_name, w, h,
                                                              ', %.0f%% see-through' % (clear * 100) if clear > 0.01 else ''))
    return image_name, clear

# ---------------------------------------------------------------------------------------------- glTF builder
class Builder:
    def __init__(self):
        self.bin = bytearray(); self.views = []; self.accessors = []
    def view(self, data, target=None):
        while len(self.bin) % 4: self.bin += b'\x00'
        v = {'buffer': 0, 'byteOffset': len(self.bin), 'byteLength': len(data)}
        if target: v['target'] = target
        self.bin += data; self.views.append(v)
        return len(self.views) - 1
    def floats(self, vals, typ, minmax=False, target=None):
        n = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4, 'MAT4': 16}[typ]
        flat = [x for v in vals for x in (v if n > 1 else (v,))]
        acc = {'bufferView': self.view(struct.pack('<%df' % len(flat), *flat), target), 'componentType': 5126,
               'count': len(vals), 'type': typ}
        if minmax:
            acc['min'] = [min(v[c] for v in vals) for c in range(n)] if n > 1 else [min(vals)]
            acc['max'] = [max(v[c] for v in vals) for c in range(n)] if n > 1 else [max(vals)]
        self.accessors.append(acc); return len(self.accessors) - 1
    def bytes4(self, vals):
        flat = [x for v in vals for x in v]
        self.accessors.append({'bufferView': self.view(struct.pack('<%dB' % len(flat), *flat), 34962),
                               'componentType': 5121, 'count': len(vals), 'type': 'VEC4'})
        return len(self.accessors) - 1
    def indices(self, idx, nverts):
        if nverts < 65536: data, ct = struct.pack('<%dH' % len(idx), *idx), 5123
        else: data, ct = struct.pack('<%dI' % len(idx), *idx), 5125
        self.accessors.append({'bufferView': self.view(data, 34963), 'componentType': ct, 'count': len(idx), 'type': 'SCALAR'})
        return len(self.accessors) - 1

gb = Builder()
gl = {'asset': {'version': '2.0', 'generator': 'KimeraCS fbx_to_ffnx (Blender %s)' % bpy.app.version_string},
      'scene': 0, 'scenes': [{'nodes': []}], 'nodes': [], 'meshes': [], 'materials': [], 'textures': [], 'images': [],
      'samplers': [{'magFilter': 9729, 'minFilter': 9987}], 'skins': [], 'animations': []}

def quat_xyzw(q): return [q.x, q.y, q.z, q.w]

# nodes: ff7_root, joints, meshes
root_node = 0
rt, rq, _ = decompose(F4 @ root_rest)          # the 180 degree turn (+ the game's rest placement)
gl['nodes'].append({'name': 'ff7_root', 'translation': list(rt), 'rotation': quat_xyzw(rq), 'children': []})
joint_node = []
for i, n in enumerate(joints):
    t, q = rest_local[i]
    gl['nodes'].append({'name': n, 'translation': list(t), 'rotation': quat_xyzw(q)})
    joint_node.append(len(gl['nodes']) - 1)
for i, p in enumerate(jparent):
    if p is None: gl['nodes'][root_node]['children'].append(joint_node[i])
    else: gl['nodes'][joint_node[p]].setdefault('children', []).append(joint_node[i])
gl['skins'].append({'name': 'skin', 'joints': joint_node,
                    'inverseBindMatrices': gb.floats([[x for col in zip(*m) for x in col] for m in ibm], 'MAT4')})

# ---------------------------------------------------------------------------------------------- meshes
material_index = {}
def get_material(mat):
    key = mat.name if mat else '(none)'
    if key in material_index: return material_index[key]
    img, col = base_color_image(mat)
    alpha = uses_alpha(mat)
    clear = 0.0
    m = {'name': key, 'pbrMetallicRoughness': {'metallicFactor': 0, 'roughnessFactor': 0.8}}
    if col: m['pbrMetallicRoughness']['baseColorFactor'] = [col[0], col[1], col[2], 1.0]
    if img:
        new = img.name not in texture_cache
        written, clear = write_texture(img, '%s_%d' % (opt['name'], len(gl['images'])))
        if written:
            if new:
                gl['images'].append({'name': written, 'uri': 'textures/%s.png' % written, 'mimeType': 'image/png'})
                gl['textures'].append({'sampler': 0, 'source': len(gl['images']) - 1})
            tex = next(i for i, t in enumerate(gl['textures']) if gl['images'][t['source']]['name'] == written)
            m['pbrMetallicRoughness']['baseColorTexture'] = {'index': tex}
            m['pbrMetallicRoughness']['baseColorFactor'] = [1, 1, 1, 1]
    else:
        warnings.append('Material %s has no base colour texture (FFNx 1.24.0 draws it invisible).' % key)
    # Transparency only where the texture really has see-through pixels: hair cards and lashes get MASK with
    # both sides drawn; a cornea is a clear shell (BLEND). Character Creator eyeballs keep other data in their
    # alpha channel, so they stay opaque.
    lname = key.lower()
    if alpha and clear > 0.01:
        if 'cornea' in lname:
            m['alphaMode'] = 'BLEND'
        elif 'eye' in lname and 'lash' not in lname:
            pass
        else:
            m['alphaMode'] = 'MASK'; m['alphaCutoff'] = 0.5; m['doubleSided'] = True
    gl['materials'].append(m)
    material_index[key] = len(gl['materials']) - 1
    return material_index[key]

total_v = total_t = 0
unweighted = 0
for obj in meshes:
    me = obj.data
    me.calc_loop_triangles()
    uv_layer = me.uv_layers.active
    normals = me.corner_normals
    mw = obj.matrix_world
    nm = mw.to_3x3().inverted_safe().transposed()
    groups = {g.index: g.name for g in obj.vertex_groups}
    vweights = []
    for v in me.vertices:
        ws = [(jindex[groups[g.group]], g.weight) for g in v.groups if g.group in groups and groups[g.group] in jindex and g.weight > 0]
        ws.sort(key=lambda x: -x[1]); ws = ws[:4]
        s = sum(w for _, w in ws)
        if s <= 0:
            unweighted += 1
            ws, s = [(0, 1.0)], 1.0
        ws = [(j, w / s) for j, w in ws] + [(0, 0.0)] * (4 - len(ws))
        vweights.append(ws)
    prims = {}
    for tri in me.loop_triangles:
        p = prims.setdefault(tri.material_index, {'map': {}, 'pos': [], 'nrm': [], 'uv': [], 'j': [], 'w': [], 'idx': []})
        corner = []
        for li in tri.loops:
            vi = me.loops[li].vertex_index
            uv = tuple(uv_layer.data[li].uv) if uv_layer else (0.0, 0.0)
            nrm = tuple(normals[li].vector)
            key = (vi, round(uv[0], 6), round(uv[1], 6), round(nrm[0], 4), round(nrm[1], 4), round(nrm[2], 4))
            idx = p['map'].get(key)
            if idx is None:
                idx = len(p['pos'])
                p['map'][key] = idx
                g = RG @ (mw @ me.vertices[vi].co) * scale            # FF7 space
                p['pos'].append((-g.x, -g.y, g.z))                     # viewer space (F4)
                n = RG @ (nm @ Vector(nrm)); n = Vector((-n.x, -n.y, n.z)).normalized()
                p['nrm'].append(tuple(n))
                p['uv'].append((uv[0], 1.0 - uv[1]))
                p['j'].append(tuple(j for j, _ in vweights[vi]))
                p['w'].append(tuple(w for _, w in vweights[vi]))
            corner.append(idx)
        # Blender and glTF both use counter-clockwise front faces; the mirror in F4 x RG is a rotation, so no flip
        p['idx'] += corner
    out_prims = []
    for mi, p in sorted(prims.items()):
        mat = obj.material_slots[mi].material if mi < len(obj.material_slots) else None
        n = len(p['pos'])
        attrs = {'POSITION': gb.floats(p['pos'], 'VEC3', True, 34962), 'NORMAL': gb.floats(p['nrm'], 'VEC3', False, 34962),
                 'TEXCOORD_0': gb.floats(p['uv'], 'VEC2', False, 34962),
                 'COLOR_0': gb.floats([(1.0, 1.0, 1.0, 1.0)] * n, 'VEC4', False, 34962),
                 'JOINTS_0': gb.bytes4(p['j']), 'WEIGHTS_0': gb.floats(p['w'], 'VEC4', False, 34962)}
        out_prims.append({'attributes': attrs, 'indices': gb.indices(p['idx'], n), 'material': get_material(mat)})
        total_v += n; total_t += len(p['idx']) // 3
    gl['meshes'].append({'name': obj.name, 'primitives': out_prims})
    gl['nodes'].append({'name': obj.name, 'mesh': len(gl['meshes']) - 1, 'skin': 0})
    gl['nodes'][root_node]['children'].append(len(gl['nodes']) - 1)
    log('  mesh %s: %d primitive(s), %d vertices' % (obj.name, len(out_prims), sum(len(p['pos']) for p in prims.values())))
if unweighted: warnings.append('%d vertices had no bone weights; bound to %s.' % (unweighted, joints[0]))
if any(o.data.shape_keys for o in meshes): log('  shape keys (facial expressions) dropped: FFNx has no morph support')
log('Meshes: %d, %d vertices, %d triangles' % (len(meshes), total_v, total_t))
gl['scenes'][0]['nodes'] = [root_node]

# ---------------------------------------------------------------------------------------------- animations
log('Animations (one key per frame, 30 fps timestamps):')
for path, name in anims:
    a = import_fbx(path)
    act = a.animation_data.action if a.animation_data else None
    if not act:
        warnings.append('%s has no animation; skipped.' % path); continue
    f0, f1 = int(round(act.frame_range[0])), int(round(act.frame_range[1]))
    sc = bpy.context.scene
    missing = [n for n in joints if n not in a.pose.bones]
    if missing: warnings.append('%s lacks bones %s; they keep their rest pose.' % (path, ', '.join(missing[:5])))
    keys_t = [[] for _ in joints]; keys_r = [[] for _ in joints]; root_t = []; root_r = []
    nframes = f1 - f0 + 1
    for k in range(nframes):
        sc.frame_set(f0 + k)
        world = []
        for i, n in enumerate(joints):
            pb = a.pose.bones.get(n)
            world.append(to_game(a.matrix_world @ pb.matrix, scale) if pb else rest_game[i])
        groot = game_root(name, k)
        loc = locals_from_world(world, groot)
        for i, (t, q) in enumerate(loc):
            if keys_r[i]:                                        # stay in the previous key's hemisphere
                px, py, pz, pw = keys_r[i][-1]
                if q.x * px + q.y * py + q.z * pz + q.w * pw < 0: q = -q
            keys_t[i].append(tuple(t)); keys_r[i].append(tuple(quat_xyzw(q)))
        vt, vq, _ = decompose(F4 @ groot)
        root_t.append(tuple(vt)); root_r.append(tuple(quat_xyzw(vq)))
    times = [k / 30.0 for k in range(nframes)]
    inp = gb.floats(times, 'SCALAR', True)
    samplers, channels = [], []
    def add(node, path_, vals, typ):
        samplers.append({'input': inp, 'interpolation': 'LINEAR', 'output': gb.floats(vals, typ)})
        channels.append({'sampler': len(samplers) - 1, 'target': {'node': node, 'path': path_}})
    add(root_node, 'translation', root_t, 'VEC3'); add(root_node, 'rotation', root_r, 'VEC4')
    for i in range(len(joints)):
        add(joint_node[i], 'translation', keys_t[i], 'VEC3'); add(joint_node[i], 'rotation', keys_r[i], 'VEC4')
    gl['animations'].append({'name': name, 'channels': channels, 'samplers': samplers})
    ref_frames = ref_anims.get(name[:4], (None, None, 0))[2] if ref else 0
    note = ''
    if ref and name[:4] not in ref_anims:
        note = '  (not in the reference%s)' % (': no root-motion compensation' if opt['root_motion'] == 'game' else '')
    elif ref_frames and ref_frames != nframes:
        if opt['root_motion'] == 'gltf':
            # FFNx with animation independence stretches the animation over the game's length
            note = '  (the game animation has %d frames: FFNx stretches it to fit; FFNx 1.24.0 shows one key per frame)' % ref_frames
        else:
            note = '  WARNING: the game animation has %d frames' % ref_frames
            warnings.append('%s: %d frames, but the game animation has %d (FFNx plays the game\'s frame count; '
                            'extra frames are never shown, missing ones hold the last pose).' % (name, nframes, ref_frames))
    log('  %s <- %s: %d frame(s)%s' % (name, os.path.basename(path), nframes, note))

# ---------------------------------------------------------------------------------------------- write
gl['buffers'] = [{'byteLength': len(gb.bin), 'uri': opt['name'] + '.bin'}]
gl['bufferViews'] = gb.views
gl['accessors'] = gb.accessors
for k in ('textures', 'images'):
    if not gl[k]: del gl[k]
gltf_path = os.path.join(opt['out'], opt['name'] + '.gltf')
with open(os.path.join(opt['out'], opt['name'] + '.bin'), 'wb') as f: f.write(gb.bin)
with open(gltf_path, 'w', encoding='utf-8') as f: json.dump(gl, f, indent=1)
if warnings:
    log('')
    log('Warnings:')
    for w in warnings: log('  ' + w)
log('')
log('Written: ' + gltf_path)
with open(os.path.join(opt['out'], opt['name'] + '_export_report.txt'), 'w', encoding='utf-8') as f:
    f.write('\n'.join(report) + '\n')
print('FBX_TO_FFNX_DONE', flush=True)
