# t1310 (VH t1310: skip checks, he'll test): v0.1.1 package = v0.1.0 package entries
# (same GUIDs + same importer .meta) with current asset bytes, + new assets
# (generated metas, md5 GUIDs as build_package.py), same exclusions as v0.1.0.
# Export only: no import check, no test suite, no capture, no measurement.
import os,sys,json,tarfile,hashlib,importlib.util
sys.path.insert(0,'/workspace/PixelFXPack/tools'); from unitypackage_writer import write_unitypackage
R='/workspace/Hellord'; B=R+'/Builds'
src=open(R+'/tools/build_package.py').read(); ns={}
exec(src.split('E=[]')[0].split("# ready scene")[0]+src[src.index('def meta'):src.index('E=[]')],ns)  # import meta() + g() only
meta=ns['meta']; g=ns['g']
OLD=B+'/Hellord_Prototype_v0.1.0.unitypackage'; man=json.load(open(B+'/Hellord_Prototype_v0.1.0.manifest.json'))
EXC=set(man['excluded'])
t=tarfile.open(OLD); old={}
for m in t.getmembers():
    if m.name.endswith('/pathname'):
        gd=m.name.split('/')[0]; p=t.extractfile(m).read().decode().split('\n')[0]
        old[p]=(gd,t.extractfile(gd+'/asset.meta').read())
E=[]; added=[]; changed=[]
for base in ('Assets/PixelFXPack','Assets/WebGLTemplates','Assets/Hellord'):
    for root,dirs,files in os.walk(R+'/'+base):
        dirs.sort(); rel=os.path.relpath(root,R)
        items=[(rel,True)]+[(rel+'/'+f,False) for f in sorted(files) if not f.endswith('.meta')]
        for p,isdir in items:
            if p in EXC or '.bak_' in p: continue
            data=None if isdir else open(R+'/'+p,'rb').read()
            if p in old: gd,mt=old[p]
            else:
                gd=g(p); mt=meta(p,isdir).encode(); added.append(p)
                assert gd not in [v[0] for v in old.values()]
            if p in old and not isdir:
                try: od=t.extractfile(gd+'/asset').read()
                except KeyError: od=None
                if od!=data: changed.append(p)
            E.append((p,gd,mt,data))
E.sort(key=lambda e:e[0])
missing=[p for p in old if p not in {e[0] for e in E}]
out=B+'/Hellord_Prototype_v0.1.1.unitypackage'; write_unitypackage(out,E)
raw=open(out,'rb').read(); md5=hashlib.md5(raw).hexdigest(); sha=hashlib.sha256(raw).hexdigest()
open(out+'.md5','w').write(f'{md5}  {os.path.basename(out)}\n'); open(out+'.sha256','w').write(f'{sha}  {os.path.basename(out)}\n')
ents=[{'path':p,'guid':gd,'directory':d is None,'sha256':None if d is None else hashlib.sha256(d).hexdigest(),'meta_sha256':hashlib.sha256(mt).hexdigest()} for p,gd,mt,d in E]
json.dump({'release_version':'0.1.1','package_name':os.path.basename(out),'bytes':len(raw),'sha256':sha,'md5':md5,'entries':ents,
  'excluded':sorted(EXC),'preserved_previous_release_guids':len(E)-len(added),'added_since_v0.1.0':added,'changed_since_v0.1.0':changed,
  'removed_since_v0.1.0':missing,'authored_guid_repairs':[],
  'packaging_method':'v0.1.0 entries reused (same GUIDs + importer metas), current asset bytes; new assets get build_package.py metas (point filter); same GNU-tar/gzip writer; export only, no import/test'},
  open(B+'/Hellord_Prototype_v0.1.1.manifest.json','w'),indent=2)
print(out,len(E),'entries; added',len(added),'changed',len(changed),'removed',missing)
print('ADDED:'); print('\n'.join(added) if added else '(none)')
print('CHANGED:'); print('\n'.join(changed) if changed else '(none)')
print('MD5',md5); print('SHA256',sha)
