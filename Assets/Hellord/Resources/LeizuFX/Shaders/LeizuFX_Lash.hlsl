// Leizu Thunder Lash (t1666): 150 deg close-range arc. Pivot = Leizu's chest; aim = _Angle (deg, as the game hook passes), arc = _Arc (deg).
// The lash front sweeps across the arc in ~0.13 s, a jagged rim bolt follows it (white at the front, cyan then blue behind),
// radial bolts fire from the hand as the front passes them, then everything breaks up into blue residual by _Progress 1.
float4 ZFX_Frag(float2 p, float2 w)
{
    float aim = _Angle * ZPI / 180.0;
    float2 dir = float2(cos(aim), sin(aim));
    float2 l = float2(dot(p, dir), dot(p, float2(-dir.y, dir.x)));
    float2 g = float2(l.x, l.y / _Squash);
    float rr = length(g); float th = atan2(g.y, g.x);
    float HA = _Arc * 0.5 * ZPI / 180.0; float R = _Radius;
    float pr = zsat(_Progress);
    float st = floor(_FxTime * _StepFps);
    float sw = zeaseOut(zsat(pr / 0.45));
    float thf = -HA + 2.0 * HA * sw;                          // sweep front
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    float brk = zsat((pr - 0.5) / 0.5);
    // rim bolt (polar jag), only behind the front
    if (th >= -HA - 0.02 && th <= thf + 0.02 && rr > R * 0.5)
    {
        float K = 18.0;
        float sa = (th + HA) / (2.0 * HA) * K; float i0 = floor(sa); float fa = sa - i0;
        float sd = floor(st / 1.0) + _Seed * 7.0;
        float j0 = (zhash(float2(i0, sd)) - 0.5) * 5.0; float j1 = (zhash(float2(i0 + 1.0, sd)) - 0.5) * 5.0;
        float rj = R * 0.88 + lerp(j0, j1, fa);
        float slope = abs(j1 - j0) * K / (2.0 * HA) / max(rr, 1.0);
        float d = abs(rr - rj) / sqrt(1.0 + slope * slope);
        float age = (thf - th) / (2.0 * HA) + pr * 0.6;           // 0 at the front
        float hot = age < 0.18 ? 1.0 : (age < 0.45 ? 0.5 : 0.0);
        float chunk = zhash(float2(floor(sa * 2.0), _Seed * 11.0));
        float wd = hot > 0.75 ? 1.0 : 0.0;
        if (chunk >= brk)
        {
            float4 bc = lz_boltcol(_RampTex, d, wd, hot, true);
            if (bc.a > 0.5) c = bc;
            gl = max(gl, zsat(1.0 - d / 6.0) * (0.3 + 0.7 * hot) * (1.0 - brk));
        }
    }
    // radial bolts fired as the front passes
    for (int i = 0; i < 7; i++)
    {
        float fi = float(i);
        if (fi >= _Bolts) break;
        float ta = -HA + (fi + 0.5) / _Bolts * 2.0 * HA + (zhash(float2(fi, _Seed * 5.0)) - 0.5) * 0.15;
        float tpass = (ta + HA) / (2.0 * HA) * 0.45;               // progress when the front reaches it
        float a = (pr - tpass) / 0.35;
        if (a < 0.0 || a > 1.0) continue;
        float hot = a < 0.25 ? 1.0 : (a < 0.55 ? 0.5 : 0.0);
        float2 dd = float2(cos(ta), sin(ta));
        float u;
        float d = lz_jag(g, dd * 5.0, dd * R * 0.86, 4.0, 2.5, frac(_Seed + fi * 0.21), u);
        float chunk = zhash(float2(floor(u * 6.0), fi + _Seed * 3.0));
        if (chunk < zsat((a - 0.55) / 0.45)) continue;
        float4 bc = lz_boltcol(_RampTex, d, 0.0, hot, hot > 0.35);
        if (bc.a > 0.5 && c.a < 0.5) c = bc;
        gl = max(gl, zsat(1.0 - d / 5.0) * (0.3 + 0.6 * hot));
    }
    // tip star riding the front + hand spark at the pivot
    if (pr < 0.5)
    {
        float2 tp = float2(cos(thf), sin(thf)) * R * 0.88;
        float2 q = g - tp; q.y = q.y * _Squash;
        float f = lz_star(q, 4.0, 0.5);
        if (f > 0.0) c = float4(zramp(_RampTex, 0.0, f > 0.5 ? 4.0 : 3.0), 1.0);
        gl = max(gl, zsat(1.0 - length(q) / 7.0));
        float fh = lz_star(p, 3.0, 0.5);
        if (fh > 0.0 && pr < 0.3) c = float4(zramp(_RampTex, 0.0, fh > 0.5 ? 4.0 : 3.0), 1.0);
    }
    if (_Layer > 0.5) return zglow(_RampTex, gl, w, _GlowGain);
    return c;
}
