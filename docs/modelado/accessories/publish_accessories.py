"""Create explicit URP materials/remaps for OBJ imports, using the stage pipeline."""
import json
import math
import re
import uuid
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
SOURCE=Path(__file__).resolve().parent
OUT=ROOT/'Assets/_Project/Modules/Shop/Models/Accessories'
BASE=ROOT/'Assets/_Project/Modules/Money/Models/Bundle'


def guid_for(path, importer='DefaultImporter'):
    meta=Path(str(path)+'.meta')
    if meta.exists():
        return re.search(r'^guid: ([0-9a-f]{32})',meta.read_text(),re.M).group(1)
    guid=uuid.uuid4().hex
    meta.write_text(f'fileFormatVersion: 2\nguid: {guid}\n{importer}:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    return guid


def gamma(v):
    return 12.92*v if v<=.0031308 else 1.055*v**(1/2.4)-.055


def texture(path,color,fabric=False,normal=False):
    im=Image.new('RGB',(256,256))
    pix=im.load()
    for y in range(256):
        for x in range(256):
            weave=math.sin(x*math.tau/8)*math.sin(y*math.tau/8)
            if normal:
                dx=.09*math.cos(x*math.tau/8)*math.sin(y*math.tau/8)
                dy=.09*math.sin(x*math.tau/8)*math.cos(y*math.tau/8)
                length=math.sqrt(1+dx*dx+dy*dy)
                pix[x,y]=tuple(round((v/length*.5+.5)*255) for v in [-dx,-dy,1])
            else:
                factor=1+.025*weave if fabric else 1
                pix[x,y]=tuple(round(max(0,min(1,gamma(v)*factor))*255) for v in color[:3])
    im.save(path)
    guid=guid_for(path)
    meta=(BASE/'Bundle_BaseColor.png.meta').read_text()
    meta=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,meta,count=1)
    if normal:
        meta=meta.replace('sRGBTexture: 1','sRGBTexture: 0')
        meta=re.sub(r'  textureType: \d+','  textureType: 1',meta)
    Path(str(path)+'.meta').write_text(meta)
    return guid


def publish():
    reports=json.loads((SOURCE/'manifest.json').read_text())
    assert len(reports)==10
    for item in reports:
        name=item['name'];folder=OUT/name
        obj=folder/(name+'.obj')
        objtext=obj.read_text()
        imported=set(re.findall(r'^usemtl (.+)$',objtext,re.M))
        assert imported==set(item['materials']), (name,imported,item['materials'])
        remap='  externalObjects:\n'
        for matname,data in item['materials'].items():
            fabric=any(key in matname for key in ['Fabric','Leather'])
            texguid=texture(folder/(matname+'_BaseColor.png'),data['linear_color'],fabric)
            normalguid=texture(folder/(matname+'_Normal.png'),data['linear_color'],normal=True) if fabric else None
            mat=(BASE/'Bundle_Surface.mat').read_text().replace('Bundle_Surface',matname)
            mat=re.sub(r'(m_Texture: \{fileID: 2800000, guid: )[0-9a-f]{32}',r'\g<1>'+texguid,mat)
            mat=re.sub(r'_Smoothness: [0-9.]+','_Smoothness: '+str(round(1-data['roughness'],3)),mat)
            mat=re.sub(r'_Metallic: [0-9.]+','_Metallic: '+str(round(data['metallic'],3)),mat)
            if normalguid:
                mat=mat.replace('m_ValidKeywords: []','m_ValidKeywords:\n  - _NORMALMAP')
                mat=mat.replace('- _BumpMap:\n        m_Texture: {fileID: 0}',f'- _BumpMap:\n        m_Texture: {{fileID: 2800000, guid: {normalguid}, type: 3}}')
                mat=mat.replace('_BumpScale: 1','_BumpScale: 0.18')
            path=folder/(matname+'.mat');path.write_text(mat)
            guid=guid_for(path,'NativeFormatImporter')
            Path(str(path)+'.meta').write_text(f'fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 2100000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
            remap+=f'  - first:\n      type: UnityEngine:Material\n      assembly: UnityEngine.CoreModule\n      name: {matname}\n    second: {{fileID: 2100000, guid: {guid}, type: 2}}\n'
        guid=guid_for(obj)
        meta=(BASE/'Bundle.obj.meta').read_text()
        meta=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,meta,count=1)
        start=meta.index('  externalObjects:');end=meta.index('  materials:',start)
        meta=meta[:start]+remap+meta[end:]
        Path(str(obj)+'.meta').write_text(meta)
        guid_for(folder/(name+'.mtl'))
        guid_for(folder,'DefaultImporter')
    # Unity folder metadata must identify directories explicitly.
    for folder in [OUT,*OUT.iterdir()]:
        if folder.is_dir():
            guid=guid_for(folder)
            Path(str(folder)+'.meta').write_text(f'fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    print('Published ten OBJ models with explicit URP material remaps, albedo textures and woven normal maps.')


if __name__=='__main__':publish()
