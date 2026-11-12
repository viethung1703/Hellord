using System;using UnityEngine;using UnityEngine.Rendering;using Object=UnityEngine.Object;
namespace Hellord {
// t1121: approved step83 original fire renderer, draw-only Sins decoration. No profile/gameplay access.
public sealed class SinFx84Renderer:IDisposable {
 public static bool Tal1265 = !System.Environment.CommandLine.Contains("-talisman1265off") && !System.Environment.CommandLine.Contains("-talisman1263old"); // t1265: the step84 smooth anchored flex (4x-expanded paper, bilinear) at the pass3e pillar spots; -talisman1265off = the t1263 hand-pixel frames
public static bool Emb1271 = !System.Environment.CommandLine.Contains("-embers1271off"); // t1271: more, longer-rising, smoothly cooling embers at screen resolution; -embers1271off = the t1262 embers
public static bool Emb1271Solo = System.Environment.CommandLine.Contains("-embers1271solo"); // debug only
public static bool Emb1271Strip = System.Environment.CommandLine.Contains("-embers1271strip"); // debug only
public static bool FrameLight1273 = !System.Environment.CommandLine.Contains("-framelight1273off"); // t1273: brazier firelight also on the main frame stone; -framelight1273off = pillars only
public static bool Emb1272 = !System.Environment.CommandLine.Contains("-embers1272off"); // t1272: small embers that split into tiny sparks; -embers1272off = the t1271 embers
public static bool Tal1270 = !System.Environment.CommandLine.Contains("-talisman1270off"); // t1270: talisman flex rendered at screen resolution (same motion, smooth, sharp-bilinear); -talisman1270off = the t1265 native-pixel flex
public static int Tal1267 = Arg1267(); // t1267 preview: -talisman1267 a|b|c (1|2|3); 0 = default step84 flex
static int Arg1267(){var a=System.Environment.GetCommandLineArgs();for(int i=0;i<a.Length-1;i++)if(a[i]=="-talisman1267"){var v=a[i+1].ToLowerInvariant();return v=="a"||v=="1"?1:v=="b"||v=="2"?2:v=="c"||v=="3"?3:0;}return 0;}
public static bool Tal1263 = !System.Environment.CommandLine.Contains("-talisman1263old") && !Tal1265; // t1263: HA's hand-pixelled sway frames (HellordSin1246 layout only); -talisman1263old = the previous shader row-shift animation
public static bool NewFire1260 = !System.Environment.CommandLine.Contains("-sinfire1246old"); // t1260: palette fire + banded fire light computed in HellordSinFX1246 (no fire RT/camera); -sinfire1246old = the v0.0.99 renderer fire
 public static bool Layout1246; // v0.0.99: t1246b pass3e layout (HellordSinFX1246 + ui_t1246_* textures); false = the older v0.0.97 layout (HellordSinFX84)
 GameObject root;Sprite quad;Texture2D noise,ramp,paper,ink;Material emberMat,energyMat;CommandBuffer clock;Wall left,right;RenderTexture energy;public Material Composite{get;private set;}public bool Ready{get;private set;}float last=-999;
 Texture2D Clone(string path,FilterMode f,TextureWrapMode w){var s=Resources.Load<Texture2D>(path);if(!s)throw new Exception(path);bool srgb=UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsSRGBFormat(s.graphicsFormat);var previous=RenderTexture.active;bool write=GL.sRGBWrite;GL.PushMatrix();var rt=RenderTexture.GetTemporary(s.width,s.height,0,RenderTextureFormat.ARGB32,srgb?RenderTextureReadWrite.sRGB:RenderTextureReadWrite.Linear);try{GL.sRGBWrite=srgb&&QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(s,rt);RenderTexture.active=rt;var t=new Texture2D(s.width,s.height,TextureFormat.RGBA32,false,!srgb){filterMode=f,wrapMode=w};t.ReadPixels(new Rect(0,0,s.width,s.height),0,0);t.Apply();return t;}finally{RenderTexture.active=previous;GL.sRGBWrite=write;GL.PopMatrix();RenderTexture.ReleaseTemporary(rt);}}
 // Step85: RGB already contains printed ink. Glow must use exactly its same texel grid and reconstruction filter.
 Texture2D Expand4(Texture2D source,bool linear){var pc=source.GetPixels32();for(int j=0;j<pc.Length;j++)if(pc[j].a==0)pc[j]=new Color32(0,0,0,0);int pw=source.width,ph=source.height;var fine=new Color32[pw*ph*16];for(int yy=0;yy<ph*4;yy++)for(int xx=0;xx<pw*4;xx++)fine[yy*pw*4+xx]=pc[(yy/4)*pw+xx/4];var t=new Texture2D(pw*4,ph*4,TextureFormat.RGBA32,false,linear){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name=linear?"Sin85 attached 4x ink mask":"Sin85 pixel-preserving 4x paper"};t.SetPixels32(fine);t.Apply(false,true);Object.Destroy(source);return t;}
 GameObject Go(string n){var g=new GameObject(n){hideFlags=HideFlags.HideAndDontSave,layer=31};g.transform.SetParent(root.transform,false);return g;}
 // t1147 diagnostic parity switch only (default false); production chooses the path from the active pipeline.
 public static bool ForceImmediate;
 public SinFx84Renderer(Transform owner){
 try{
 root=new GameObject("Sins84 isolated draw-only fire cameras"){hideFlags=HideFlags.HideAndDontSave};root.transform.SetParent(owner,false);
 clock=new CommandBuffer{name="Sins84 approved original fire unscaled clock"};var tx=new Texture2D(4,4,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var px=new Color32[16];for(int i=0;i<16;i++)px[i]=new Color32(255,255,255,255);tx.SetPixels32(px);tx.Apply();quad=Sprite.Create(tx,new Rect(0,0,4,4),new Vector2(.5f,.5f),4);
 noise=Clone("HellordFX928/fx928_noise128",FilterMode.Bilinear,TextureWrapMode.Repeat);ramp=Clone("HellordFX928/fx948_firewall_ramp",FilterMode.Point,TextureWrapMode.Clamp);emberMat=new Material(Shader.Find("Sprites/Default"));
 left=new Wall(this,32,1.2f,.4f,false);left.sr.enabled=false;right=new Wall(this,32,4.8f,2.7f,false);right.sr.enabled=false;
 var sh=Resources.Load<Shader>(Layout1246?"HellordShaders/HellordSinFX1246":"HellordShaders/HellordSinFX84");var es=Resources.Load<Shader>("HellordShaders/HellordSinEnergy84");if(!sh||!es||!sh.isSupported||!es.isSupported)throw new Exception("Sins84 compositor/energy unsupported");Composite=new Material(sh){hideFlags=HideFlags.DontSave};energyMat=new Material(es);
 Composite.SetFloat("_FireMode",Layout1246&&NewFire1260?1f:0f);Composite.SetFloat("_Tal1263",Layout1246&&Tal1263?1f:0f);Composite.SetFloat("_Tal1265",Layout1246&&Tal1265?1f:0f);Composite.SetFloat("_Tal1267",Layout1246&&Tal1265?(float)Tal1267:0f);if(Layout1246)Composite.SetTexture("_FrameMask",Resources.Load<Texture2D>("HellordArt/ui_t1273_frame_mask"));if(Layout1246){Composite.SetTexture("_TalFrames",Resources.Load<Texture2D>("HellordArt/ui_t1263_talisman_frames"));Composite.SetTexture("_TalMasks",Resources.Load<Texture2D>("HellordArt/ui_t1263_talisman_masks"));}Composite.SetTexture("_Backing",Resources.Load<Texture2D>(Layout1246?"HellordArt/ui_t1246_base":"HellordArt/ui_s78_fire_backing"));Composite.SetTexture("_PaperBacking",Resources.Load<Texture2D>("HellordArt/ui_s79_paper_backing"));paper=Expand4(Clone(Layout1246?"HellordArt/ui_t1246_paper_layer":"HellordArt/ui_s79_paper_layer",FilterMode.Bilinear,TextureWrapMode.Clamp),false);ink=Expand4(Clone(Layout1246?"HellordArt/ui_t1246_ink_mask":"HellordArt/ui_s84_ink_mask",FilterMode.Point,TextureWrapMode.Clamp),true);if(paper.width!=ink.width||paper.height!=ink.height)throw new Exception("Paper/ink attachment grids differ");Composite.SetTexture("_Paper",paper);Composite.SetTexture("_InkMask",ink);Composite.SetTexture("_Stone",Resources.Load<Texture2D>(Layout1246?"HellordArt/ui_t1246_stone_mask":"HellordArt/ui_s84_stone_mask"));Composite.SetTexture("_Cracks",Resources.Load<Texture2D>(Layout1246?"HellordArt/ui_t1246_cracks":"HellordArt/ui_s84_cracks"));
 Composite.SetTexture("_FireL",left.rt);Composite.SetTexture("_FireR",right.rt);Composite.SetTexture("_PostL",left.postRT);Composite.SetTexture("_PostR",right.postRT);Composite.SetFloat("_FireScale",28f/48);energyMat.SetTexture("_Right",right.rt);energy=new RenderTexture(2,1,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear){filterMode=FilterMode.Point};energy.Create();Composite.SetTexture("_Energy",energy);Ready=true;
 Debug.Log("[step84] approved original SpriteRenderer fire ON,32px/28:48,640x360->320x180 glow0.3->1280x720 CRT; anchored continuous paper and masked stone FX ON");
 }catch(Exception e){Debug.LogError("[step84] "+e);Dispose();}
 }
 public void Frame(float t){if(!Ready)return;var previous=RenderTexture.active;bool write=GL.sRGBWrite;GL.PushMatrix();try{if(t-last>=1f/30||t<last){if(!(Layout1246&&NewFire1260)){left.Render(t);right.Render(t);Graphics.Blit(left.rt,energy,energyMat);}last=t;}}finally{RenderTexture.active=previous;GL.sRGBWrite=write;GL.PopMatrix();}Composite.SetFloat("_T",t);if(Layout1246)Composite.SetFloat("_Tal1267",Tal1265?(float)Tal1267:0f);if(Layout1246)Composite.SetFloat("_Tal1270",Tal1270?1f:0f);if(Layout1246)Composite.SetFloat("_Emb1272",Emb1272?1f:0f);if(Layout1246)Composite.SetFloat("_Frame1273",FrameLight1273?1f:0f);if(Layout1246)Composite.SetFloat("_Emb1271",Emb1271?(Emb1271Strip?3f:Emb1271Solo?2f:1f):0f);}
 public void Dispose(){Ready=false;if(clock!=null){clock.Dispose();clock=null;}foreach(var w in new[]{left,right}){if(w==null)continue;foreach(var r in new[]{w.rt,w.worldRT,w.postRT,w.postWorld,w.ga,w.gb})if(r){r.Release();Object.Destroy(r);}if(w.mat)Object.Destroy(w.mat);if(w.crt)Object.Destroy(w.crt);}if(energy){energy.Release();Object.Destroy(energy);}if(Composite)Object.Destroy(Composite);if(energyMat)Object.Destroy(energyMat);if(emberMat)Object.Destroy(emberMat);if(paper)Object.Destroy(paper);if(ink)Object.Destroy(ink);if(noise)Object.Destroy(noise);if(ramp)Object.Destroy(ramp);if(quad){Object.Destroy(quad.texture);Object.Destroy(quad);}if(root)Object.Destroy(root);}
 class Wall {
  public Camera cam;public RenderTexture rt,worldRT,postRT,postWorld,ga,gb;public Material crt;public SpriteRenderer sr;public Material mat;public SpriteRenderer[] sparks;public float len;public Vector2 dir;public float seed,phase;public bool originalSparks;
  public Wall(SinFx84Renderer o,float length,float seed,float phase,bool originalSparks,int width=48,int height=64,Vector2? direction=null){
   this.o=o;len=length;this.seed=seed;this.phase=phase;this.originalSparks=originalSparks;dir=direction??Vector2.right;
   var cg=o.Go("step83_camera");cam=cg.AddComponent<Camera>();cam.enabled=false;cam.orthographic=true;cam.orthographicSize=360/32f;cam.aspect=640f/360;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.cullingMask=1<<31;cam.allowHDR=false;cam.allowMSAA=false;
   rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="step83 original SpriteRenderer RT"};rt.Create();
   worldRT=new RenderTexture(640,360,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Point};worldRT.Create();cam.targetTexture=worldRT;
   ga=new RenderTexture(320,180,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear){filterMode=FilterMode.Bilinear};ga.Create();gb=new RenderTexture(320,180,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear){filterMode=FilterMode.Bilinear};gb.Create();postWorld=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){filterMode=FilterMode.Point};postWorld.Create();postRT=new RenderTexture(width*2,height*2,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){filterMode=FilterMode.Point};postRT.Create();
   crt=new Material(Resources.Load<Shader>("HellordShaders/HellordCRT"));crt.SetVector("_SrcSize",new Vector4(640,360,0,0));crt.SetFloat("_GlowAmt",.3f);crt.SetFloat("_FogAmt",0);crt.SetFloat("_PulseAmt",0);crt.SetFloat("_Haze",0);
   var sh=Resources.Load<Shader>("HellordShaders/HellordPixelFireWall");if(!sh||!sh.isSupported)throw new Exception("Original firewall unsupported");mat=new Material(sh){name="step83 clone of runtime fx957_firewall construction"};mat.SetTexture("_NoiseTex",o.noise);mat.SetTexture("_RampTex",o.ramp);mat.SetFloat("_RampCount",7);mat.SetFloat("_PPU",16);mat.SetFloat("_FlameH",22);mat.SetFloat("_Gain",.889f);mat.SetFloat("_Scorch",1);mat.SetFloat("_Grow",1);mat.SetFloat("_TimeStep",12);mat.SetFloat("_Seed",seed);mat.SetFloat("_T0",phase);mat.SetVector("_Center",Vector4.zero);mat.SetVector("_Dir",dir);mat.SetFloat("_RadiusPx",length/2);mat.SetFloat("_BandW",4.8f);
   sr=o.Go("fx948_firewall ORIGINAL SpriteRenderer").AddComponent<SpriteRenderer>();sr.sprite=o.quad;sr.sharedMaterial=mat;sr.sortingOrder=-19989;
   // Exact FireWallFx948 mesh support for world-length len/PPU, thickness .6u, pad .3u.
   float half=length/32,up=(22+18*.889f)/16,pad=.3f;
   float hx=Mathf.Abs(dir.x)*half+.3f+.3f,hy=Mathf.Abs(dir.y)*half+.3f+pad;
   sr.transform.position=new Vector3(0,up*.5f-pad*.5f,0);sr.transform.localScale=new Vector3(hx*2+.2f,hy*2+up+.2f-pad,1);
   int n=originalSparks?Mathf.Clamp(Mathf.RoundToInt(length/8),1,48):0;sparks=new SpriteRenderer[n];
   for(int j=0;j<n;j++){var e=o.Go("fx957_ember ORIGINAL SpriteRenderer").AddComponent<SpriteRenderer>();e.sprite=o.quad;e.sharedMaterial=o.emberMat;e.sortingOrder=-19988;e.transform.localScale=Vector3.one/16;sparks[j]=e;}
   Debug.Log($"[s84] lenPx={length} dir={dir} RT={width}x{height} sr pos={sr.transform.position} scale={sr.transform.localScale} vertices={o.quad.vertices.Length} UV={string.Join(",",Array.ConvertAll(o.quad.uv,x=>x.ToString()))} seed={seed} T0={phase} sparks={n}");
  }
  SinFx84Renderer o;
  public void Render(float t){
   sr.enabled=true;foreach(var e in sparks)e.enabled=true;
   float f30=Mathf.Floor(t*12)/12*30;
   for(int j=0;j<sparks.Length;j++){float sp=.6f+.6f*Mathf.Repeat(j*.618034f,.99999f),ph=Mathf.Repeat(j*.414214f,.99999f),angle=j*2.39996f,across=Mathf.Repeat(j*.754877f,.99999f);float age=Mathf.Repeat(f30*sp/40+ph,1);var e=sparks[j];e.enabled=age<.9f;
    Vector2 b=dir*((across-.5f)*len)+new Vector2(Mathf.Sin(f30*.15f+angle)*2*age,8+42*age);e.transform.position=new Vector3((Mathf.Floor(b.x)+.5f)/16,(Mathf.Floor(b.y)+.5f)/16,0);e.color=age<.35f?new Color32(254,231,97,255):age<.65f?new Color32(247,118,34,255):new Color32(162,38,51,255);
   }
   var priorTime=Shader.GetGlobalVector("_Time");if(GraphicsSettings.currentRenderPipeline==null&&!ForceImmediate){o.clock.Clear();o.clock.SetGlobalVector("_Time",new Vector4(t/20,t,t*2,t*3));cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha,o.clock);cam.Render();cam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha,o.clock);}else RenderImmediate(t,priorTime);Shader.SetGlobalVector("_Time",priorTime);
   Graphics.CopyTexture(worldRT,0,0,320-rt.width/2,180-12,rt.width,rt.height,rt,0,0,0,0);
   crt.SetVector("_BlurDir",new Vector4(1,0,QualitySettings.activeColorSpace==ColorSpace.Linear?1:0,0));Graphics.Blit(worldRT,ga,crt,1);crt.SetVector("_BlurDir",new Vector4(0,1,0,0));Graphics.Blit(ga,gb,crt,1);crt.SetTexture("_GlowTex",gb);Graphics.Blit(worldRT,postWorld,crt,0);
   Graphics.CopyTexture(postWorld,0,0,640-rt.width,360-24,rt.width*2,rt.height*2,postRT,0,0,0,0);
   sr.enabled=false;foreach(var e in sparks)e.enabled=false;
  }
  // t1147: Camera.Render + camera CommandBuffers are built-in-renderer only; under URP/SRP (VH's Windows Editor showed the
  // AddCommandBuffer/RemoveCommandBuffer warning) the fire RT stayed empty. Same quad, same matrices, same material and clock,
  // drawn by an immediately executed CommandBuffer, which every pipeline supports. Built-in keeps the original camera path.
  static Mesh quadMesh;static MaterialPropertyBlock mpb;
  static Mesh QuadMesh(Sprite s){if(quadMesh)return quadMesh;quadMesh=new Mesh{name="Sins84 sprite quad",hideFlags=HideFlags.HideAndDontSave};quadMesh.vertices=Array.ConvertAll(s.vertices,v=>(Vector3)v);quadMesh.uv=s.uv;quadMesh.triangles=Array.ConvertAll(s.triangles,i=>(int)i);quadMesh.colors32=Array.ConvertAll(s.vertices,_=>new Color32(255,255,255,255));return quadMesh;}
  void RenderImmediate(float t,Vector4 priorTime){
   var cb=new CommandBuffer{name="Sins84 fire (pipeline-independent)"};var mesh=QuadMesh(o.quad);if(mpb==null)mpb=new MaterialPropertyBlock();
   var view=Matrix4x4.Scale(new Vector3(1,1,-1))*cam.transform.worldToLocalMatrix;var proj=Matrix4x4.Ortho(-cam.orthographicSize*cam.aspect,cam.orthographicSize*cam.aspect,-cam.orthographicSize,cam.orthographicSize,cam.nearClipPlane,cam.farClipPlane);
   cb.SetRenderTarget(worldRT);cb.ClearRenderTarget(true,true,Color.clear);cb.SetViewProjectionMatrices(view,proj);cb.SetGlobalVector("_Time",new Vector4(t/20,t,t*2,t*3));
   cb.DrawMesh(mesh,sr.transform.localToWorldMatrix,mat,0,0);
   foreach(var e in sparks)if(e.enabled){mpb.Clear();mpb.SetColor("_RendererColor",e.color);cb.DrawMesh(mesh,e.transform.localToWorldMatrix,o.emberMat,0,0,mpb);}
   cb.SetGlobalVector("_Time",priorTime);Graphics.ExecuteCommandBuffer(cb);cb.Release();
  }
 }
}
}
