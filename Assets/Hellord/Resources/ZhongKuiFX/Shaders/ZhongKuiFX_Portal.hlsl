// Zhong Kui Portal (t1709): Hell Gate standing ossuary arch, soul-fire mouth, 8 s. Billboard (upright). Not a flat disc.

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = zk_t();
    float hot = zsat(_Hot);
    float fade = zsat(_Fade);
    float form = zsat(_Form);
    float inten = zsat(_Intensity);
    float row = -1.0, idx = 0.0, gl = 0.0;
    float hw = _Width;                  // half width of the opening
    float H = _Height * form * (1.0 - fade * 0.85);
    float thick = max(_Size, 3.0);
    float live = inten * form * (1.0 - fade);
    if (H < 8.0 || live < 0.02) return zk_none();
    // 3/4: far pillar (negative y offset slightly, a touch thinner) — keep both upright for readability
    // LEFT / RIGHT bone pillars
    for (int s = 0; s < 2; s++)
    {
        float side = s == 0 ? -1.0 : 1.0;
        float2 q = p - float2(side * hw, 0.0);
        float ph = H * 0.86;
        if (q.y >= -1.0 && q.y <= ph && abs(q.x) <= thick + 1.2)
        {
            if (abs(q.x) > thick || q.y < 0.0) { row = 0.0; idx = 0.0; }
            else
            {
                row = 0.0;
                idx = (q.x * side < 0.0) ? 5.0 : 2.0;
                if (abs(q.x) < 0.8) idx = 4.0;
                float e = zN(_NoiseTex, float2(p.x, p.y) / 7.0 + float2(_Seed, 0.0)).a;
                if (e < 0.07) idx = 0.0;
            }
            gl = max(gl, 0.22 * live);
        }
    }
    // ribbed lintel (slight arch)
    float xn = p.x / max(hw + thick, 1.0);
    float lint = H * 0.86 + 2.0 - xn * xn * 3.0;
    if (abs(p.x) <= hw + thick + 2.0 && p.y >= lint - 1.0 && p.y <= lint + thick + 1.5)
    {
        float along = p.y - lint;
        if (along >= -0.5 && along <= thick + 1.0)
        {
            row = 0.0;
            idx = (along > thick * 0.75 || abs(xn) > 1.0) ? 0.0 : ((p.x < 0.0) ? 5.0 : 3.0);
            if (zmod(abs(p.x) + 2.0, 5.0) < 1.1 && along < thick) idx = 0.0;
            gl = max(gl, 0.25 * live);
        }
    }
    // three bone spikes on the lintel
    for (int k = 0; k < 3; k++)
    {
        float fk = float(k);
        float sx = (fk - 1.0) * (hw * 0.7);
        float sh = 5.0 + ((fk > 0.5 && fk < 1.5) ? 3.0 : 0.0);
        float f = gg_crystal(p - float2(sx, lint + thick - 1.0), sh * form, 1.8, 0.0);
        if (f > -0.5) { row = 0.0; idx = zk_bidx(f, hot); }
    }
    // mouth: darkness + flickering soul fire, not a filled oval
    if (abs(p.x) < hw - 1.0 && p.y > 3.0 && p.y < lint - 0.5)
    {
        float vy = p.y / max(lint - 2.0, 1.0);
        float fl = zflow(_NoiseTex, float2(p.x * 1.3, p.y), 9.0, t, 8.0, _Seed);
        float dens = (1.0 - abs(p.x) / hw) * (0.55 + 0.45 * vy);
        if (fl > 0.44 && dens > 0.20)
        {
            row = 1.0;
            idx = (fl > 0.78 * (0.45 + 0.55 * hot)) ? 5.0 : ((fl > 0.60) ? 4.0 : 2.0);
            gl = max(gl, dens * 0.48 * live);
        }
        else if (dens > 0.15 && row < -0.5)
        { row = 0.0; idx = 1.0; }
        // hanging shreds
        float shv = zN(_NoiseTex, float2(p.x * 0.28, t * 0.15) + float2(_Seed, 0.4)).g;
        if (abs(p.x - (shv - 0.5) * hw * 1.3) < 1.2 && p.y > lint - 2.0 - shv * 9.0 && p.y < lint - 0.5)
        { row = 1.0; idx = 3.0; }
    }
    if (row < -0.5) return zk_none();
    if (_Layer > 0.5)
    {
        gl = min(gl, _GlowCap);
        if (gl < 0.02) return zk_none();
        return zk_glow(_RampTex, gl, p + w * 0.01, _GlowGain, 1.0);
    }
    return float4(zramp(_RampTex, row, idx), 1.0);
}
