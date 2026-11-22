// Falling sun: white core, yellow body with dark granulation / sunspots, writhing orange corona (polar noise tongues),
// streaming flame trail opposite _Vel that grows with _Fall, dark smoke at the trail end. Pivot = sun centre.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float R = _Radius;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float vl = length(_Vel.xy);
    float2 vd = vl > 1.0 ? _Vel.xy / vl : float2(0.0, -1.0);
    float2 b = -vd;
    float along = dot(p, b), across = dot(p, float2(-b.y, b.x));
    float r = length(p) / R;
    float a = atan2(p.y, p.x);
    float glowF = zsat(1.0 - (r - 0.6) / 1.4) * 0.9;
    // trail (behind, in the travel frame)
    float tl = R * (1.2 + 4.5 * _Fall);
    if (along > 0.0 && along < tl + 10.0)
    {
        float an = along / tl;
        float hw = R * (0.95 - 0.7 * an) * (1.0 + 0.2 * (zN(_NoiseTex, float2(along / 14.0 + t * 1.4, _Seed)).r - 0.5));
        float fl = zN(_NoiseTex, float2(across / 7.0 + _Seed, along / 9.0 - t * 4.0)).r;
        float th = (1.0 - abs(across) / max(hw, 0.5)) * (1.0 - an) * 1.3 + (fl - 0.5) * 0.9;
        float4 fc = zfire(_RampTex, th, w, 0.2);
        float sm = zsat(1.0 - abs(across) / (hw + 4.0)) * smoothstep(0.55, 1.0, an) * (1.0 - smoothstep(1.0, 1.4, an)) * (fl * 2.0 - 0.6);
        if (zdithS(sm, w, 0.2)) col = zrow(_RampTex, 1.0, 3.0, sm * 0.4, w);
        if (fc.a > 0.5) col = fc;
        glowF = max(glowF, zsat(th) * 0.6);
    }
    // corona
    if (r >= 0.9 && r < 1.9)
    {
        float cn = zN(_NoiseTex, float2(a / ZTAU * 3.0 + _Seed, r * 0.35 - t * 0.9)).r;
        float cn2 = zN(_NoiseTex, float2(a / ZTAU * 7.0 - t * 0.1, _Seed + 0.5)).g;
        float len = 0.25 + 0.75 * cn * cn2 * 1.6;
        float ch = (1.0 - (r - 0.9) / len) * 0.95;
        float4 cc = zfire(_RampTex, ch, w, 0.2);
        if (cc.a > 0.5) col = cc;
        else if (ch > 0.05) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
    }
    if (r < 1.0)
    {
        float z = sqrt(max(0.0, 1.0 - r * r));
        float2 suv = p / (R * (0.6 + 0.4 * z)) * 0.18 + float2(t * 0.05 + _Seed, 0.0);
        float gran = zN(_NoiseTex, suv * 2.0).r;
        float spot = zN(_NoiseTex, suv * 1.2 + float2(0.3, t * 0.03)).a;
        float bh = 0.55 + 0.45 * z + (gran - 0.5) * 0.45;
        if (spot < 0.12 && r > 0.35) bh -= 0.45;
        if (r < 0.38) bh = 1.0;
        col = zfire(_RampTex, bh, w, 0.0);
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
