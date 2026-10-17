"""Validate exported OBJ indices, material remaps, textures and portable GLBs."""
import json
import math
import re
import struct
from pathlib import Path
from PIL import Image

SOURCE=Path(__file__).resolve().parent
ROOT=SOURCE.parents[2]
OUT=ROOT/'Assets/_Project/Modules/Shop/Models/Accessories'
result=[]
for item in json.loads((SOURCE/'manifest.json').read_text()):
    name=item['name'];folder=OUT/name
    text=(folder/(name+'.obj')).read_text()
    lines=text.splitlines()
    positions=[list(map(float,line.split()[1:])) for line in lines if line.startswith('v ')]
    normals=[line for line in lines if line.startswith('vn ')]
    uv=[line for line in lines if line.startswith('vt ')]
    faces=[line.split()[1:] for line in lines if line.startswith('f ')]
    assert all(math.isfinite(v) for p in positions for v in p)
    assert positions and uv and normals and faces
    for face in faces:
        assert len(face)==3
        for ref in face:
            a,b,c=map(int,ref.split('/'))
            assert 1<=a<=len(positions) and 1<=b<=len(uv) and 1<=c<=len(normals)
    used=set(re.findall(r'^usemtl (.+)$',text,re.M))
    defined=set(re.findall(r'^newmtl (.+)$',(folder/(name+'.mtl')).read_text(),re.M))
    assert used==defined==set(item['materials'])
    importer=Path(str(folder/(name+'.obj'))+'.meta').read_text()
    for mat in used:
        path=folder/(mat+'.mat')
        guid=re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M).group(1)
        assert f'name: {mat}\n' in importer and f'guid: {guid}, type: 2' in importer
        assert '933532a4fcc9baf4fa0491de14d08ed7' in path.read_text()
        Image.open(folder/(mat+'_BaseColor.png')).verify()
    blob=(SOURCE/name/(name+'.glb')).read_bytes()
    magic,version,length=struct.unpack_from('<III',blob)
    assert magic==0x46546c67 and version==2 and length==len(blob)
    size=struct.unpack_from('<I',blob,12)[0];data=json.loads(blob[20:20+size])
    assert data['meshes'] and data['materials']
    assert all('uri' not in buffer for buffer in data['buffers'])
    result.append({'name':name,'triangles':len(faces),'materials':len(used),'obj_indices_uv_normals':'pass','urp_remaps':'pass','glb':'pass'})
(SOURCE/'export-validation.json').write_text(json.dumps(result,indent=2))
assert all(not item['problems'] for item in json.loads((SOURCE/'geometry-validation.json').read_text()))
print(json.dumps(result,indent=2))
