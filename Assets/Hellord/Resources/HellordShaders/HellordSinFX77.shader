// step80: existing approved FireWall source -> native colour flipbook (explicit opt-in); flexible pinned paper.
// Existing step79 analytic fire retained ONLY as safe migration fallback, not selected art direction.
// Modes: 0 fire, 1 additive embers, 2 DISABLED lintel FX, 3 STATIC selection, 4 paper+ink.
// The original atlas is never spatially warped. All motion is isolated to fire or alpha-masked paper.
Shader "Hellord/SinFX77"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mode ("Mode", Float) = 0
        _T ("Unscaled seconds", Float) = 0
        _Src ("Source UV rect", Vector) = (0,0,1,1)
        _Px ("Rect native pixels", Vector) = (16,16,0,0)
        _Seed ("Seed", Float) = 0
        _FireHeight ("Flame height envelope (native px)", Range(12,30)) = 30
        _FireWidth ("Body half-width (native px)", Range(4,10)) = 8
        _FireBase ("Protected bowl rows", Float) = 5
        _FireSpeed ("Evolution speed", Range(0.2,2)) = 1
        _FireFPS ("Animation steps per second", Range(8,30)) = 18
        _FireBacking ("Verified flame-free full atlas", 2D) = "black" {}
        _UseCleanBacking ("Use assigned backing", Float) = 0
        _FireFlipbook ("Source-flow72: 8x9 tiles of 28x40", 2D) = "black" {}
        _FireFlipReady ("Explicitly enable assigned flow flipbook", Float) = 0
        _PaperLayer ("Isolated moving paper RGBA atlas", 2D) = "black" {}
        _PaperBacking ("Verified pre-paper full atlas", 2D) = "black" {}
        _PaperReady ("Both paper atlases assigned", Float) = 0
        _PaperSway ("Paper tip sway (native px)", Range(0,2)) = 1.6
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex, _FireBacking, _PaperLayer, _PaperBacking, _FireFlipbook;
            float _Mode, _T, _Seed, _FireHeight, _FireWidth, _FireBase, _FireSpeed, _FireFPS;
            float _UseCleanBacking, _PaperReady, _PaperSway, _FireFlipReady;
            float4 _Src, _Px;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 GuiColor(fixed4 c)
            {
                #if !defined(UNITY_COLORSPACE_GAMMA)
                c.rgb=LinearToGammaSpace(c.rgb);
                #endif
                return c;
            }
            // Authored coherent contour, not four disconnected noise candles.
            // One breathing body + two unequal side licks + one rising off-centre air pocket.
            float3 FirePixel(float2 q, float t, out float cover)
            {
                float x=q.x,y=q.y,s=_Seed,w=_FireWidth;
                float h=_FireHeight*(0.89+0.07*sin(t*1.8+s)+0.04*sin(t*3.1+s*1.7));
                float v=y/h;
                float lean=2.8*sin(t*1.35+s)+1.2*sin(t*2.2+s*0.7);
                float axis=lean*v*v+2.4*sin(v*5.4-t*2.7+s)*v;
                float width=w*(0.68+0.56*sin(saturate(v)*3.141592653589793))*pow(saturate(1-v),0.83);
                float d=min(width-abs(x-axis),min(y+0.5,h-y));
                for(int j=0;j<2;j++)
                {
                    float z=(float)j,side=j==0?-1.0:1.0;
                    float phase=t*(2.1+z*0.31)+s+z*2.6;
                    float bh=_FireHeight*(0.63+z*0.04+0.14*sin(phase));
                    float bv=(y-2.0)/bh;
                    float ba=side*w*0.63+side*(1.6+1.8*sin(phase+1.2))*bv*bv+lean*0.35*bv;
                    float bw=w*(0.45+0.04*sin(phase))*pow(saturate(1-bv),0.74);
                    float bd=min(bw-abs(x-ba),min(y-1.5,bh-(y-2.0)));
                    d=max(d,bd);
                }
                float phase=frac(t*0.6+s*0.17);
                float cy=h*(0.46+phase*0.48),cx=lean*0.6+2.6*sin(t*1.8+s);
                float2 holeQ=float2((x-cx)/1.45,(y-cy)/2.5);
                float hole=(length(holeQ)-1.0)*1.45;
                d=min(d,hole+3.0*(1.0-smoothstep(0.12,0.28,phase)));
                // Offset tapered gold plume + smaller ivory root; no horizontal hot bar.
                float cv=y/(h*0.73);
                float ca=-1.3+lean*0.48*cv*cv+0.8*sin(cv*4.0-t*2.0+s)*cv;
                float cw=w*0.61*pow(saturate(1-cv),0.8);
                float core=min(cw-abs(x-ca),min(y-0.4,h*0.73-y));
                float iv=y/(h*0.43),iw=w*0.29*pow(saturate(1-iv),0.7);
                float ivory=min(iw-abs(x-(-1.5+lean*0.25*iv)),min(y-1.4,h*0.43-y));
                cover=(y>=0 && d>=0)?1.0:0.0;
                float3 col=float3(174,38,16)/255.0;
                if(d>0.6) col=float3(224,67,18)/255.0;
                if(d>1.5 && y<h*0.8) col=float3(247,103,20)/255.0;
                if(core>=0) col=float3(255,170,38)/255.0;
                if(core>1.1 && y<h*0.55) col=float3(255,205,75)/255.0;
                if(ivory>=0) col=float3(255,233,150)/255.0;
                for(int j=0;j<3;j++)
                {
                    float z=(float)j,p=frac(t*(0.68+z*0.08)+z*0.37+s*0.13);
                    float px=lean*0.6+sin(t*1.8+z*2.0+s)*3.2,py=h*0.65+p*10.0;
                    if(abs(x-px)/0.85+abs(y-py)/1.6<1.0 && p>0.18 && p<0.78 && y>=0)
                    { cover=1.0;col=float3(244,121,26)/255.0; }
                }
                return col;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 loc=saturate((i.uv-_Src.xy)/_Src.zw),rs=_Px.xy;
                float2 np=min(floor(loc*rs),rs-1.0);
                float2 uv=_Src.xy+(np+0.5)/rs*_Src.zw;
                float t=floor(_T*max(1.0,_FireFPS))/max(1.0,_FireFPS)*_FireSpeed;
                if(_Mode<0.5)
                {
                    fixed4 bg=GuiColor(tex2D(_MainTex,uv));
                    if(_UseCleanBacking>0.5) bg=GuiColor(tex2D(_FireBacking,uv));
                    if(_FireFlipReady>0.5)
                    {
                        // Native, point-sampled RGBA; no interpolation / crossfade / glow.
                        float fr=fmod(floor(_T*12.0*_FireSpeed+_Seed*11.0),72.0);
                        float col=fmod(fr,8.0),row=floor(fr/8.0);
                        float2 fp=min(floor((np+0.5)/rs*float2(28,40)),float2(27,39));
                        float2 fuv=float2((col*28.0+fp.x+0.5)/224.0,
                                            1.0-(row*40.0+(39.0-fp.y)+0.5)/360.0);
                        fixed4 flow=GuiColor(tex2D(_FireFlipbook,fuv));
                        return fixed4(lerp(bg.rgb,flow.rgb,step(0.5,flow.a)),1);
                    }
                    float2 q=float2(np.x+0.5-rs.x*0.5,np.y+0.5-_FireBase);
                    float cover;float3 col=FirePixel(q,t,cover);
                    return fixed4(lerp(bg.rgb,col,cover),1);
                }
                if(_Mode<1.5)
                {
                    float3 acc=0;
                    // BurnFX-inspired hierarchy: bright low, mid orange, few distant red cinders.
                    // Even emission phases and lanes, not random positions across the panel.
                    float et=floor(_T*12.0)/12.0*_FireSpeed;
                    for(int e=0;e<26;e++)
                    {
                        float z=(float)e,group=e<12?0.0:(e<22?1.0:2.0);
                        float per=group<0.5?1.6:(group<1.5?2.65:3.8);
                        per+=fmod(z,3.0)*0.11;
                        float life=frac(et/per+(z+0.5)/26.0+_Seed*0.17);
                        float rise=group<0.5?48.0:(group<1.5?72.0:92.0);
                        float px=floor(rs.x*0.5+(fmod(z,7.0)-3.0)*1.05
                            +(0.8+0.4*_Seed)*life*life*2.0
                            +sin(life*4.0+group*1.6+_Seed)*life*2.6);
                        float py=floor(16.0+life*rise),tall=(e%6==0 && life<0.35)?2.0:1.0;
                        float hit=(np.x==px && np.y>=py && np.y<py+tall && life<0.86)?1.0:0.0;
                        float fade=smoothstep(0.0,0.10,life)*pow(1.0-life,1.2);
                        float power=group<0.5?0.85:(group<1.5?0.60:0.35);
                        float3 spark=life<0.28?float3(254,174,52)/255.0:
                            (life<0.62?float3(247,118,34)/255.0:float3(162,38,51)/255.0);
                        acc+=hit*spark*fade*power;
                    }
                    return fixed4(acc,1);
                }
                // Explicitly neutralize legacy animated lintel seals; additive mode returns ZERO.
                if(_Mode<2.5) return fixed4(0,0,0,0);
                if(_Mode<3.5)
                {
                    // One-pixel STATIC copper selection; no pulse, travelling glint or inner wash.
                    float d=min(min(np.x,np.y),min(rs.x-1.0-np.x,rs.y-1.0-np.y));
                    return fixed4(float3(192,99,47)/255.0,d<0.5?1.0:0.0);
                }
                fixed4 original=GuiColor(tex2D(_MainTex,uv));
                // Safe failure: unassigned atlas never blanks or displaces a rectangular art region.
                if(_Mode>4.5 || _PaperReady<0.5) return original;
                fixed4 backing=GuiColor(tex2D(_PaperBacking,uv));
                float down=rs.y-1.0-np.y;
                if(down<8.0) return original; // brass clasp / upper paper are hard pinned
                float bend=saturate((down-8.0)/(rs.y-10.0));
                float phase=_T*1.7-bend*4.8+_Seed;
                float crease=0.5+0.5*sin(phase+bend*6.0);
                float offset=_PaperSway*(bend*(0.72*sin(phase)+0.30*sin(phase*1.7+bend*4.0))
                    +0.25*sin(bend*3.14159265)*sin(bend*9.0-_T*1.1+_Seed));
                float cx=(rs.x-1.0)*0.5;
                float sx=floor(cx+(np.x-cx)/(1.0-0.085*crease*bend)-offset+0.5);
                float bow=1.15*sin(phase+1.2)*bend*sin(bend*3.14159265);
                float sd=down-floor(bow+0.5);
                float2 sp=float2(sx,rs.y-1.0-sd);
                fixed4 paper=GuiColor(tex2D(_PaperLayer,_Src.xy+(sp+0.5)/rs*_Src.zw));
                float ink=(paper.r>0.20 && paper.r>paper.g*1.25 && paper.r>paper.b*1.6)?1.0:0.0;
                // One restrained stroke-local wake, not the entire symbol flashing orange.
                float band=frac(_T*0.25+_Seed*0.10)*40.0;
                float wake=pow(saturate(1.0-abs(sd-band)/6.0),2.0);
                float shade=1.0-0.07*crease*bend;
                float3 col=saturate(paper.rgb*shade+ink*(0.02+0.30*wake)*float3(0.70,0.11,0.02));
                return fixed4(lerp(backing.rgb,col,paper.a),1);
            }
            ENDCG
        }
    }
}
