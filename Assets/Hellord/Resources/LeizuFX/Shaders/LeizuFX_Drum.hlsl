// Leizu Thunder Ministry Drum (t1666): a placed war drum, drawn in shader (no baked sprite). Pivot = drum foot centre.
// Red lacquer barrel with gold rims and studs on two dark stands, pale hide head with a blue thunder sigil.
// _Hit 1->0 per boom: head flashes, sigil burns white, arcs crawl the rims, barrel jumps 1 px. _Booms 0..10 cracks it,
// _Break 0..1 splits it apart (noise dissolve + slump) when it breaks or expires. _Spawn 0..1 drops it in.
float4 ZFX_Frag(float2 p, float2 w)
{
    float S = max(_Size, 0.5);
    float spawn = min(_Spawn, zsat(_FxTime / 0.25));             // drops in over the first 0.25 s by itself
    float drop = floor((1.0 - zeaseIn(spawn)) * 10.0);
    float jump = _Hit > 0.6 ? 1.0 : 0.0;
    float slump = floor(zeaseIn(_Break) * 3.0);
    float2 q = (p - float2(0.0, drop + jump - slump)) / S;
    float rx = 9.0; float base = 3.0; float top = 13.0; float ry = 3.0;
    float st = floor(_FxTime * _StepFps);
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    // break dissolve (chunky 3 px cells)
    float bn = zhash(floor(q / 3.0) + float2(_Seed * 7.0, 1.0));
    bool gone = bn < _Break * 1.15 - 0.05;
    // shadow (dithered, small) on the floor, only when landed
    float2 sq = float2(p.x / (rx + 1.0), (p.y + 0.5) / 2.5);
    if (spawn > 0.95 && _Break < 0.6 && length(sq) < 1.0 && zbayer(w) < 0.5) c = float4(zramp(_RampTex, 7.0, 0.0), 1.0);
    // stands (two dark legs)
    if (!gone && q.y >= 0.0 && q.y < base && (abs(abs(q.x) - 6.0) <= 1.0)) c = float4(zramp(_RampTex, 4.0, 1.0), 1.0);
    // barrel: x within the bulging profile between base and top
    float yb = (q.y - base) / (top - base);
    float bulge = rx + 1.0 * sin(zsat(yb) * ZPI);
    bool inBody = q.y >= base - ry * sqrt(zsat(1.0 - (q.x * q.x) / (bulge * bulge))) && q.y <= top && abs(q.x) <= bulge;
    float tq = (q.y - top) / ry;                                   // top ellipse
    bool inHead = (q.x * q.x) / (rx * rx) + tq * tq <= 1.0;
    if (!gone && (inBody || inHead))
    {
        float xn = q.x / bulge;
        float shade = xn > 0.45 ? 2.0 : (xn < -0.55 ? 4.0 : 3.0);   // lit from the left
        float3 col = zramp(_RampTex, 4.0, shade);
        // gold rims near the top and bottom of the barrel
        if (abs(q.y - (top - 1.5)) < 0.9 || abs(q.y - (base + 1.0)) < 0.9) col = zramp(_RampTex, 4.0, xn > 0.4 ? 5.0 : 6.0);
        // studs in a row under the top rim
        if (abs(q.y - (top - 3.5)) < 0.6 && abs(zmod(q.x + 20.0, 4.0) - 2.0) < 0.6) col = zramp(_RampTex, 4.0, 7.0);
        // drum head
        if (inHead && q.y > top - 0.5)
        {
            col = zramp(_RampTex, 1.0, tq > 0.2 ? 5.0 : 4.0);
            // sigil: a small zig-zag bolt on the head
            float sig = abs(tq * ry - (abs(zmod(q.x + 6.0, 4.0) - 2.0) - 1.0) * 0.8);
            if (abs(q.x) < 4.5 && sig < 0.6) col = _Hit > 0.3 ? zramp(_RampTex, 0.0, 4.0) : zramp(_RampTex, 0.0, 2.0);
            if (_Hit > 0.7 && zbayer(w) < 0.6) col = zramp(_RampTex, 0.0, 4.0);
            if (abs(q.x) < 5.0 && sig < 1.6) gl = max(gl, 0.35 + 0.65 * _Hit);
        }
        // outline
        bool edge = abs(q.x) > bulge - 1.0 || (inHead && (q.x * q.x) / (rx * rx) + tq * tq > 0.72 && q.y > top);
        if (edge && !(inHead && q.y > top)) col = zramp(_RampTex, 7.0, 0.0);
        // cracks with booms (crack-edge noise, thickened), and glowing for a moment on a hit
        float ncr = floor(_Booms * 0.45 + 0.01);                // 0..4 crack lines, one more every ~2 booms
        for (int k = 0; k < 4; k++)
        {
            float fk = float(k);
            if (fk >= ncr) break;
            float h = zhash(float2(fk * 2.7, _Seed * 3.0 + 1.0));
            float2 c0 = float2((h - 0.5) * 14.0, top - 1.0); float2 c1 = c0 + float2((frac(h * 7.0) - 0.5) * 6.0, -5.0 - 4.0 * frac(h * 3.0));
            float u;
            float dc = lz_jag(q, c0, c1, 3.0, 1.5, h, u);
            if (dc <= 0.5 && !(inHead && q.y > top)) { col = _Hit > 0.4 ? zramp(_RampTex, 0.0, 3.0) : zramp(_RampTex, 7.0, 0.0); if (_Hit > 0.4) gl = max(gl, 0.6 * _Hit); }
        }
        c = float4(col, 1.0);
        gl = max(gl, (edge ? 0.0 : 0.15) * _Hit);
    }
    // arcs crawling the rims on a hit
    if (_Hit > 0.15 && !gone)
    {
        for (int i = 0; i < 2; i++)
        {
            float fi = float(i);
            float h = zhash(float2(st + fi * 3.0, _Seed * 9.0));
            float y = fi < 0.5 ? top : base + 1.0;
            float2 a0 = float2(-rx + h * 6.0, y); float2 a1 = float2(rx - (1.0 - h) * 6.0, y + (fi < 0.5 ? 2.0 : -1.0));
            float u;
            float d = lz_jag(q, a0, a1, 4.0, 2.0, frac(h + st * 0.1), u);
            if (d <= 0.9 / S + 0.05) { c = float4(zramp(_RampTex, 0.0, _Hit > 0.6 ? 4.0 : 3.0), 1.0); gl = max(gl, 0.9 * _Hit); }   // v2: 2 px rim arcs
            else if (d <= 1.9 / S + 0.05 && _Hit > 0.6) { c = float4(zramp(_RampTex, 0.0, 2.0), 1.0); gl = max(gl, 0.6 * _Hit); }
        }
    }
    // break: a few lacquer chunks fall away
    if (_Break > 0.0 && _Break < 0.9)
    {
        for (int k = 0; k < 4; k++)
        {
            float fk = float(k); float h = zhash(float2(fk * 3.3, _Seed * 4.0));
            float2 cp = float2((fk - 1.5) * 6.0 + (h - 0.5) * 4.0, 8.0) + float2((fk - 1.5) * 6.0 * _Break, 6.0 * _Break - 26.0 * _Break * _Break);
            float2 dq = abs(q - cp);
            if (dq.x <= 1.5 && dq.y <= 1.0) c = float4(zramp(_RampTex, 4.0, h > 0.5 ? 3.0 : 6.0), 1.0);
        }
    }
    if (_Layer > 0.5) return zglow(_RampTex, gl, w, _GlowGain);
    return c;
}
