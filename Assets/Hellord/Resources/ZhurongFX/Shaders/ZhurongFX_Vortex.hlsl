// Backdraft: 0-.15 outward puff (anticipation), .15-.85 inward log-spiral of violet-grey smoke arms carrying embers
// toward the focal point + blotchy floor darkening, .85-1 a hot ember knot that winks out. No ring, no disc.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float pr = zsat(_Progress);
    float2 gp = float2(p.x, p.y / _Squash);
    float R = _Radius;
    float r = length(gp) / R;
    float a = atan2(gp.y, gp.x);
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    float pull = smoothstep(0.12, 0.85, pr);
    float env = smoothstep(0.0, 0.12, pr) * (1.0 - smoothstep(0.82, 0.95, pr));
    // floor darkening (blotchy, dithered, strongest mid-pull)
    float dk = zN(_NoiseTex, gp / 14.0 + float2(_Seed, 0.0)).r;
    if (r < 1.0 && zclump(_NoiseTex, (1.0 - r) * env * (dk * 1.6 - 0.3), gp, _Seed)) col = float4(zramp(_RampTex, 1.0, 1.0), 1.0);
    // puff: outward ring of smoke
    if (pr < 0.2)
    {
        float pr0 = pr / 0.2;
        float pd = abs(r - (0.15 + 0.55 * zeaseOut(pr0))) / (0.12 + 0.1 * pr0);
        float pn = zN(_NoiseTex, gp / 8.0 + float2(_Seed, t)).g;
        if (zclump(_NoiseTex, (1.0 - pd) * (1.0 - pr0) * (pn * 2.0 - 0.2), gp, _Seed + 0.4)) col = float4(zramp(_RampTex, (pn > 0.5) ? 4.0 : 1.0, (pn > 0.5) ? 3.0 : 2.0), 1.0);
    }
    // spiral arms (log spiral, scrolling inward)
    float lr = log(max(r, 0.02));
    float u = a * 2.0 / ZTAU + lr * 1.1 + pull * 2.6 + t * 0.4;
    float arm = zN(_NoiseTex, float2(u * 0.5, lr * 0.4 + _Seed)).g;
    float s = sin(u * ZTAU);
    float width = 0.75 + 0.75 * arm;                                         // arm thickness breathes with noise
    float fadeR = zsat(1.15 - r) * zsat(r * 5.0 - 0.3);
    bool armOn = s > 1.0 - width * fadeR * env * (0.6 + 0.6 * pull);
    if (pr >= 0.12 && armOn)
    {
        bool lip = s < 1.0 - width * fadeR * env * (0.6 + 0.6 * pull) * 0.55;
        col = float4(zramp(_RampTex, lip ? 4.0 : 1.0, lip ? 3.0 : ((arm > 0.55) ? 2.0 : 1.0)), 1.0);
        if (!lip && r < 0.45 && s > 0.97) { col = zfire(_RampTex, 0.35 + (0.45 - r), w, 0.0); glowF = 0.4; }   // arms heat up as they reach the eye
    }
    // collapse knot
    float kn = (pr > 0.7) ? (1.0 - length(p) / (1.5 + 3.5 * (1.0 - abs(pr - 0.86) / 0.14))) * (1.0 - smoothstep(0.9, 1.0, pr)) : 0.0;
    if (kn > 0.0) { col = zfire(_RampTex, 0.5 + kn, w, 0.0); glowF = max(glowF, kn + 0.3); }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
