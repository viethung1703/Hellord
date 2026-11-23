// Shared vertex / fragment for the quad-based Leizu effects. The quad (SpriteRenderer with a white sprite, or a MeshRenderer
// quad) only defines coverage; all shape comes from world-pixel maths around _Origin.
struct ZAttr { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
struct ZV2F { float4 pos : SV_POSITION; float4 color : COLOR; float2 wpx : TEXCOORD0; };
ZV2F vert(ZAttr v)
{
    ZV2F o;
    o.pos = ZFX_OBJ2CLIP(v.vertex.xyz);
    o.wpx = ZFX_OBJ2WORLD(v.vertex.xyz).xy * _PPU;
    o.color = v.color;
    return o;
}
float4 frag(ZV2F i) : SV_Target
{
    float2 w = floor(i.wpx) + float2(0.5, 0.5);                 // world pixel snap (16 PPU)
    float2 p = w - floor(_Origin.xy * _PPU + float2(0.5, 0.5));  // pivot snapped to a whole pixel
    float4 c = ZFX_Frag(p, w);
    if (_Layer > 0.5) { clip(c.a - 0.5); return float4(c.rgb * i.color.a, 1.0); }   // glow: vertex alpha fades it
    clip(c.a * i.color.a - 0.5);                                                       // base: vertex alpha is a cutout gate
    return float4(c.rgb, 1.0);
}
