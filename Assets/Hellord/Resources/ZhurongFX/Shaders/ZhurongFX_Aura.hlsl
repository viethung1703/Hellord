// Hero-silhouette aura. Reads the live hero frame (_MainTex + _SpriteRect) so it always matches loadout / facing.
// _Mode 0 Ember Effigy (drawn INSTEAD of a hero copy: char rim, flowing banded fire body, tongues off the top, burns away
//         upward with a bright lip on _Fade), 1 Molten Shell (crust plates framing the sprite, seams pulse, widen on _Crack),
//         2 Molten Heart (gold-white chest heart + sparse shoulder licks), 3 Hearth Blessing (gold feet tongues + flickering
//         warm edge, _Cleanse white-gold flash). _Front 0 = layer behind the hero, 1 = small details over the hero.
float za_a(float2 sp)
{
    if (sp.x < 0.0 || sp.y < 0.0 || sp.x >= _SpritePx.x || sp.y >= _SpritePx.y) return 0.0;
    float2 uv = _SpriteRect.xy + (floor(sp) + float2(0.5, 0.5)) / _SpritePx.xy * _SpriteRect.zw;
    return (ZTEX(_MainTex, uv).a > 0.5) ? 1.0 : 0.0;
}
float3 za_c(float2 sp)
{
    float2 uv = _SpriteRect.xy + (floor(sp) + float2(0.5, 0.5)) / _SpritePx.xy * _SpriteRect.zw;
    return ZTEX(_MainTex, uv).rgb;
}
// 0 inside, 1..5 px outside, 6 farther
float za_dist(float2 sp)
{
    if (za_a(sp) > 0.5) return 0.0;
    for (int r = 1; r < 6; r++)
    {
        float fr = float(r);
        float m = 0.0;
        m = max(m, za_a(sp + float2(fr, 0.0))); m = max(m, za_a(sp - float2(fr, 0.0)));
        m = max(m, za_a(sp + float2(0.0, fr))); m = max(m, za_a(sp - float2(0.0, fr)));
        m = max(m, za_a(sp + float2(fr, fr) * 0.7)); m = max(m, za_a(sp - float2(fr, fr) * 0.7));
        m = max(m, za_a(sp + float2(fr, -fr) * 0.7)); m = max(m, za_a(sp + float2(-fr, fr) * 0.7));
        if (m > 0.5) return fr;
    }
    return 6.0;
}
bool za_rim(float2 sp)
{
    return za_a(sp) > 0.5 && (za_a(sp + float2(1.0, 0.0)) < 0.5 || za_a(sp - float2(1.0, 0.0)) < 0.5 || za_a(sp + float2(0.0, 1.0)) < 0.5 || za_a(sp - float2(0.0, 1.0)) < 0.5);
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float2 sp = p + _SpritePx.zw;
    float hh = _SpritePx.y - _SpritePx.w;              // body height above the feet
    float hv = zsat(p.y / max(hh, 1.0));
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    float inside = za_a(sp);
    float dn = zN(_NoiseTex, p / 6.0 + float2(_Seed, 0.0)).r;
    if (_Mode < 0.5)
    {
        // ---- Ember Effigy
        float front = (_Fade * 1.3 - 0.15) * hh + (dn - 0.5) * 7.0;
        float formOK = (dn < _Form * 1.25) ? 1.0 : 0.0;
        float fl = zflow(_NoiseTex, p + float2(_Seed * 20.0, 0.0), 4.0, t, 18.0, _Seed);
        float heat = 0.0;
        for (int k = 1; k < 10; k++)
        {
            float fk = float(k);
            if (za_a(sp - float2(0.0, fk)) > 0.5 && inside < 0.5)
            {
                float Hk = 2.5 + 7.0 * zN(_NoiseTex, float2(p.x / 5.0 + _Seed, t * 0.5)).g * (1.0 - _Fade);
                heat = max(heat, (1.0 - fk / Hk) * (0.55 + 0.7 * fl));
                break;
            }
        }
        float4 tc = zfire(_RampTex, heat * formOK, w, 0.25);
        if (tc.a > 0.5 && p.y > front) col = tc;
        if (inside > 0.5 && formOK > 0.5 && p.y > front)
        {
            // heat-map of the live hero frame: its own outline / eyes / shading survive, so the copy reads as the hero
            float3 hc = za_c(sp);
            float lum = max(hc.r, max(hc.g, hc.b));
            float band = zN(_NoiseTex, float2(p.x / 14.0, p.y / 9.0 - t * 0.9) + float2(_Seed, 0.0)).r;
            float bh = (lum - 0.2) * 0.95 + (fl - 0.5) * 0.45 + (band - 0.5) * 0.35 - hv * 0.12;
            float ch = zN(_NoiseTex, p / 7.0 + float2(t * 0.15, -t * 0.5)).g;
            if (za_rim(sp) || lum < 0.25) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
            else if (ch < 0.2) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            else col = zfire(_RampTex, bh, w, 0.0);
            if (p.y < front + 1.6) col = zfire(_RampTex, 1.0, w, 0.0);      // bright burn lip
        }
        glowF = (inside > 0.5 && p.y > front) ? 0.35 + 0.3 * fl : zsat(heat) * 0.5;
        glowF *= formOK;
    }
    else if (_Mode < 1.5)
    {
        // ---- Molten Shell
        float dd = za_dist(sp);
        float reveal = (sp.y / _SpritePx.y) < (_Form * 1.35 - dn * 0.35) ? 1.0 : 0.0;
        float4 vz = zN(_NoiseTex, sp / 48.0 + float2(_Seed, 0.0));
        float seamW = 0.1 + 0.28 * _Crack + 0.06 * _Pulse + 0.03 * sin(t * 3.0 + vz.b * 9.0);
        float thick = 2.0 + 2.5 * zN(_NoiseTex, sp / 30.0 + float2(_Seed, 0.3)).r + 1.2 * (1.0 - hv);
        bool band = (_Front < 0.5) ? (dd >= 1.0 && dd <= thick) : (za_rim(sp) && (hv < 0.35 || hv > 0.75) && vz.b > 0.3);
        if (band && reveal > 0.5)
        {
            if (vz.a < seamW)
            {
                float sh = min(0.5 + 0.2 * sin(t * 2.5 + vz.b * 6.0) + _Crack * 0.3 + _Pulse * 0.25, 0.8);
                col = zfire(_RampTex, sh, w, 0.0);
                glowF = sh * 0.6;
            }
            else
            {
                float lit = zsat(0.55 + (-(p.x) * 0.05) + (p.y - hh * 0.5) * 0.03 + (vz.b - 0.5) * 0.5 - (dd - 1.0) * 0.1);
                col = float4(zramp(_RampTex, 3.0, 1.0 + zband(lit, 4.0, w, 0.5)), 1.0);
                if (vz.a < seamW * 1.8) col = float4(zramp(_RampTex, 0.0, 2.0), 1.0);   // lava bleed at plate edges
                if (_Crack > 0.4 && zdithS((_Crack - 0.4) * 1.6 * (1.0 - vz.a), w, 0.6)) col = float4(zramp(_RampTex, 0.0, 2.0), 1.0);
            }
        }
    }
    else if (_Mode < 2.5)
    {
        // ---- Molten Heart
        float2 c = float2(0.0, _ChestY);
        float pulse = 0.5 + 0.5 * sin(t * 5.0);
        if (_Front > 0.5)
        {
            float2 dq = p - c;
            float r = length(dq);
            float cr = 1.8 + 1.0 * pulse;
            float vein = zN(_NoiseTex, (p + float2(_Seed * 30.0, 0.0)) / 26.0 + float2(0.0, t * 0.02)).a;
            if (r < cr) col = zrow(_RampTex, 2.0, 8.0, 0.72 + 0.28 * (1.0 - r / cr), w);
            else if (r < cr + 1.0) col = zfire(_RampTex, 0.5 + 0.2 * pulse, w, 0.0);
            else if (inside > 0.5 && vein < 0.07 && r < 6.0 + 2.0 * pulse) col = zfire(_RampTex, 0.75 - r * 0.06, w, 0.0);   // molten veins
            glowF = zsat(1.0 - r / 9.0) * (0.6 + 0.4 * pulse);
        }
        else
        {
            float fl = zflow(_NoiseTex, p + float2(_Seed * 20.0, 0.0), 3.5, t, 16.0, _Seed);
            float heat = 0.0;
            if (inside < 0.5)
            {
                for (int k = 1; k < 6; k++)
                {
                    float fk = float(k);
                    if (za_a(sp - float2(0.0, fk)) > 0.5)
                    {
                        float g = smoothstep(0.55, 0.75, zN(_NoiseTex, float2(p.x / 4.0 + _Seed, t * 0.6)).g);
                        heat = (1.0 - fk / (2.0 + 4.0 * g)) * g * (0.6 + 0.6 * fl);
                        break;
                    }
                }
            }
            float4 tc = zfire(_RampTex, heat, w, 0.4);
            if (tc.a > 0.5) col = tc;
            float dd = za_dist(sp);
            if (dd > 0.5 && dd < 1.5 && hv < 0.85 && zdithS(0.35 + 0.3 * fl - 0.2, w, 0.8)) col = float4(zramp(_RampTex, 0.0, 3.0 + step(0.6, fl)), 1.0);
            glowF = zsat(1.0 - length(p - c) / 14.0) * 0.5;
        }
    }
    else
    {
        // ---- Hearth Blessing (behind the hero)
        float fl = zflow(_NoiseTex, p + float2(_Seed * 20.0, 0.0), 4.0, t, 12.0, _Seed);
        float dd = za_dist(sp);
        float heat = 0.0;
        if (inside < 0.5 && p.y < 16.0)
        {
            for (int k = 0; k < 11; k++)
            {
                float fk = float(k);
                float2 s2 = sp - float2(0.0, fk);
                if (s2.y - _SpritePx.w < 5.0 && za_dist(s2) < 2.5)
                {
                    float g = 0.4 + 0.6 * zN(_NoiseTex, float2(p.x / 4.0 + _Seed, t * 0.5)).g;
                    heat = max(heat, (1.0 - fk / (4.0 + 7.0 * g)) * (0.6 + 0.6 * fl));
                }
            }
        }
        if (dd > 0.5 && dd < 2.5 && zclump(_NoiseTex, (0.75 - dd * 0.2) * (0.3 + 1.0 * fl) * _Form, p + float2(0.0, -t * 14.0), _Seed)) col = zrow(_RampTex, 2.0, 8.0, 0.35 + 0.25 * fl, w);
        float4 hc = (heat > 0.25) ? zrow(_RampTex, 2.0, 8.0, heat, w) : float4(0.0, 0.0, 0.0, 0.0);
        if (hc.a > 0.5) col = hc;
        if (_Cleanse > 0.0 && dd > 0.5 && dd < 1.0 + 2.0 * _Cleanse && zclump(_NoiseTex, 0.4 + 0.6 * _Cleanse, p * 2.0, _Seed + t)) col = zrow(_RampTex, 2.0, 8.0, 0.7 + 0.3 * _Cleanse, w);
        glowF = (zsat(1.0 - abs(dd - 1.5) / 2.5) * 0.45 * _Form + _Cleanse * 0.8 * zsat(1.0 - (dd - 1.0) / 4.0)) * step(0.5, dd) + heat * 0.4;
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
