// Rebirth pillar (one-shot, _Progress over ~1.6 s). Phases: 0-.25 ground cracks glow + low licks (anticipation),
// .25-.42 rush (tapered column shoots up), .42-.55 roar (narrow white-hot shaft, full height), .55-1 settle into
// separate tongues, dark smoke rises, erodes away. Tapered D2 Fire Wall / Inferno language, never a box.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float pr = zsat(_Progress);
    float H = _Height, W = _Width;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    // ground cracks (ground plane, squash 0.5) radiating from the feet
    float2 gp = float2(p.x, p.y / 0.5);
    float gr = length(gp);
    float crR = W * (0.4 + 1.1 * zeaseOut(pr / 0.25));
    float ang = atan2(gp.y, gp.x);
    float spoke = abs(sin(ang * 4.5 + (zN(_NoiseTex, gp / 30.0 + float2(_Seed, 0.0)).r - 0.5) * 3.0));
    float e = zN(_NoiseTex, gp / 50.0 + float2(_Seed, 0.4)).a;
    float crackLive = (1.0 - smoothstep(0.7, 1.0, pr));
    if (gr < crR && abs(p.y) < W * 0.6 && (spoke < 0.09 + 0.05 * (1.0 - gr / crR) || e < 0.07))
    {
        float ch = (0.9 - 0.5 * gr / crR) * crackLive + 0.2 * step(pr, 0.25);
        float4 cc = zfire(_RampTex, ch, w, 0.15);
        col = cc.a > 0.5 ? cc : float4(zramp(_RampTex, 5.0, 0.0), 1.0);
        glowF = ch * 0.5;
    }
    // column envelope
    float rush = zeaseOut((pr - 0.22) / 0.2);
    float settle = smoothstep(0.55, 1.0, pr);
    float hNow = H * (0.12 + 0.88 * rush) * (1.0 - 0.6 * settle);
    if (pr < 0.22) hNow = 4.0 + 6.0 * pr / 0.22;
    float roar = (pr > 0.4 && pr < 0.56) ? 1.0 : 0.0;
    float y = p.y;
    float heat = 0.0;
    if (y > -2.0 && y < hNow + 6.0)
    {
        float yn = zsat(y / max(hNow, 1.0));
        float n1 = zN(_NoiseTex, float2(y / 40.0 - t * 0.6, _Seed + 0.3)).r - 0.5;
        float hw = W * 0.5 * (1.0 - pow(yn, 1.4) * 0.78) * (1.0 + 0.25 * (n1)) * (1.0 + 0.5 * zsat(1.0 - y / 6.0));
        float cx = p.x - n1 * 4.0 * yn;
        float xn = abs(cx) / max(hw, 0.5);
        float fl = zflow(_NoiseTex, p + float2(_Seed * 13.0, 0.0), 6.0, t, 70.0, _Seed);
        float fl2 = zflow(_NoiseTex, p * float2(1.0, 0.5) + float2(_Seed * 7.0, 0.0), 4.0, t, 50.0, _Seed + 0.4);
        heat = (1.0 - pow(xn, 1.6)) * 1.25 * (1.0 - pow(yn, 2.2)) + (fl - 0.5) * 0.7;
        // tongues break off near the top and when settling
        heat -= smoothstep(0.55 - settle * 0.4, 1.0, yn) * (0.9 - fl2 * 0.6) + settle * (0.55 - fl2 * 0.5);
        if (roar > 0.5)
        {
            float wob = (zN(_NoiseTex, float2(y / 18.0 - t * 2.0, _Seed + 0.6)).r - 0.5) * 3.0;
            float sx = abs(p.x - wob * yn) / (5.5 + 3.0 * (1.0 - yn));
            heat = max(heat, (1.0 - sx * sx) * (1.05 + 0.45 * fl) * (1.0 - yn * 0.45));
        }
        if (y > hNow) heat -= (y - hNow) / 4.0;
    }
    glowF = max(glowF, zsat(heat) * 0.8 + roar * 0.4 * zsat(1.0 - abs(p.x) / 10.0) * step(p.y, hNow));
    // smoke while settling
    float2 qs = p - float2(0.0, H * 0.25 + settle * H * 0.35);
    float sd = length(float2(qs.x / (W * 0.9), qs.y / (H * 0.45)));
    float sf = zflow(_NoiseTex, qs + float2(_Seed * 11.0, 0.0), 8.0, t, 18.0, _Seed + 0.7);
    float sm = smoothstep(0.5, 0.75, pr) * (1.0 - smoothstep(0.85, 1.0, pr)) * zsat(1.2 - sd) * (sf * 2.2 - 0.8);
    if (heat < 0.18 && zdithS(sm, w, 0.2)) col = zrow(_RampTex, 1.0, 3.0, sm * 0.5, w);
    float4 fc = zfire(_RampTex, heat, w, 0.18);
    if (fc.a > 0.5) col = fc;
    else if (heat > 0.1) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
