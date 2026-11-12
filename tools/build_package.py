import os,sys,hashlib,re
sys.path.insert(0,'/workspace/PixelFXPack/tools')
from unitypackage_writer import write_unitypackage
R='/workspace/Hellord'; A=R+'/Assets/Hellord'
g=lambda p: hashlib.md5(('Hellord/'+p).encode()).hexdigest()
GAME_GUID=g('Assets/Hellord/Scripts/Runtime/HellordGame.cs')
# ready scene from pack showcase template
s=open('/workspace/PixelFXPack/Assets/PixelFXPack/Demo/PixelFXShowcase.unity').read()
s=s.replace('71f6cedc397431a1359fa829c926a6f2',GAME_GUID).replace('m_Name: PixelFXShowcase','m_Name: Hellord')
s=re.sub(r'  pixelsPerUnit: 16\n  uberMaterial: .*\n','',s)
os.makedirs(A+'/Scenes',exist_ok=True); open(A+'/Scenes/Hellord_Prototype.unity','w').write(s)
def meta(p,isdir):
    G=g(p); h=f'fileFormatVersion: 2\nguid: {G}\n'
    if isdir: return h+'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.cs'): return h+'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.asmdef'): return h+'AssemblyDefinitionImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.png'):
        w='0' if 'floor_' in p else '1'
        return h+'TextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    wrapU: '+w+'\n    wrapV: '+w+'\n  nPOTScale: 0\n  spriteMode: 1\n  spritePixelsToUnits: 16\n  alphaIsTransparency: 1\n  textureType: 8\n  textureShape: 1\n  platformSettings:\n  - serializedVersion: 3\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 2048\n    textureCompression: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.wav'):  # v0.0.29: SFX, Decompress On Load (loadType 0), mono, PCM; heartbeat loops via AudioSource.loop
        return h+'AudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 0\n    quality: 1\n    conversionMode: 0\n    preloadAudioData: 1\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 0\n  loadInBackground: 0\n  ambisonic: 0\n  3D: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.ogg'):  # v0.0.33: music, Vorbis, Streaming (loadType 2), stereo
        return h+'AudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 2\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n    quality: 0.7\n    conversionMode: 0\n    preloadAudioData: 0\n  platformSettingOverrides: {}\n  forceToMono: 0\n  normalize: 0\n  loadInBackground: 1\n  ambisonic: 0\n  3D: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.shader'): return h+'ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    if p.endswith('.unity'): return h+'DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    return h+'TextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
E=[]
for root,dirs,files in [w for base in (R+'/Assets/WebGLTemplates', A) for w in os.walk(base)]:  # v0.0.9: + WebGL template
    dirs.sort(); rel=os.path.relpath(root,R)
    E.append((rel,g(rel),meta(rel,True).encode(),None))
    for f in sorted(files):
        if f.endswith('.meta'): continue
        p=rel+'/'+f; E.append((p,g(p),meta(p,False).encode(),open(R+'/'+p,'rb').read()))
os.makedirs(R+'/Builds',exist_ok=True)
# v0.0.23: bundle the game's own Assets/PixelFXPack (synced verbatim from PixelFXPack-v1.9.2.unitypackage, same GUIDs, so importing
# Hellord upgrades an older pack in place). The old NaN-guard/_MainTex patches are upstream since 1.8.x, no overlay needed.
FXR=R+'/Assets/PixelFXPack'; FXE=[]
def guid_of(mp): return re.search(r'guid: ([0-9a-f]{32})', open(mp).read()).group(1)
for root,dirs,files in os.walk(FXR):
    dirs.sort(); rel=os.path.relpath(root,R)
    FXE.append((rel,guid_of(root+'.meta'),open(root+'.meta','rb').read(),None))
    for f in sorted(files):
        if f.endswith('.meta'): continue
        fp=root+'/'+f; FXE.append((rel+'/'+f,guid_of(fp+'.meta'),open(fp+'.meta','rb').read(),open(fp,'rb').read()))
FXE.sort(key=lambda e:e[0]); E=FXE+E
VER=sys.argv[1] if len(sys.argv)>1 else 'v0.0.23'
out=R+f'/Builds/Hellord_Prototype_{VER}.unitypackage'; write_unitypackage(out,E); print(out,len(E),'entries')
