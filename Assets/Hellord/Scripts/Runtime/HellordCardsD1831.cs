using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1831 frame D "Bronze Leiwen" (VH pick, frame2_style_D): warm bronze for skills, green bronze for supports.
    // Headline + stat table layout (write3 / frame2 sheets): name plate / leiwen fret / icon in a 2 px ring (+NEW) /
    // white headline / 40 px fret-coloured rule (6 px above and below) / table (grey label at the 16 px left inset, value at
    // the 16 px right inset) / silver detail (red for costs) / grey tags pinned to the bottom / diamond level dots + hotkey.
    // The frame is generated here from the same rules as fs2.py D (one border profile on all 4 edges, flat plate, 1 px
    // square-wave fret, period 6, centred), cached per size. Copy = CardCopyD1831 (data, live numbers).
    // =====================================================================================================================
    public partial class HellordGame
    {
        const int CardDTagGap = 12, CardDTitleTop = 4, CardDHeadMax = 3, CardDDetailMax = 5, CardDTagMax = 3, CardDTitleMax = 2, CardDRowGap = 6;

        // ---- frame D spec (Endesga 32), the fs2.py D values
        static readonly Color32 E32Ink = new Color32(0x18, 0x14, 0x25, 255), E32Tan = new Color32(0xe4, 0xa6, 0x72, 255), E32Brown = new Color32(0xb8, 0x6f, 0x50, 255),
            E32DBrown = new Color32(0x73, 0x3e, 0x39, 255), E32Copper = new Color32(0xd7, 0x76, 0x43, 255), E32Amber = new Color32(0xfe, 0xae, 0x34, 255),
            E32Green = new Color32(0x63, 0xc7, 0x4d, 255), E32DGreen = new Color32(0x3e, 0x89, 0x48, 255), E32Forest = new Color32(0x26, 0x5c, 0x42, 255),
            E32Cyan = new Color32(0x2c, 0xe8, 0xf5, 255);
        static readonly Color CDSilver = new Color32(0xc0, 0xcb, 0xdc, 255);
        sealed class FrameDSpec { public Color32 hi, mid, lo, fret, ringOut, ringIn, dotOn, dotOff; }
        static readonly FrameDSpec FrameDSkill = new FrameDSpec { hi = E32Tan, mid = E32Brown, lo = E32DBrown, fret = E32Copper, ringOut = E32Copper, ringIn = E32DBrown, dotOn = E32Amber, dotOff = E32DBrown };
        static readonly FrameDSpec FrameDSupport = new FrameDSpec { hi = E32Green, mid = E32DGreen, lo = E32Forest, fret = E32Green, ringOut = E32DGreen, ringIn = E32Forest, dotOn = E32Cyan, dotOff = E32Forest };
        /// <summary>1 px square-wave fret, period 6, symmetric about its bump centre (fs2 WAVE).</summary>
        static readonly string[] FrameDWave = { "####..", "#..#..", "#..###" };
        static readonly string[] FrameDDot = { "...#...", "..###..", ".#####.", "#######", ".#####.", "..###..", "...#..." };
        const int FrameDFretX0 = 7, FrameDRingW = 2;

        sealed class CardFaceD
        {
            public string name = ""; public List<string> title = new List<string>();
            public readonly List<List<CSeg>> head = new List<List<CSeg>>(), detail = new List<List<CSeg>>(), tags = new List<List<CSeg>>();
            public readonly List<List<CSeg>[]> rows = new List<List<CSeg>[]>();
            public bool showNew, hasDots = true, support; public int dots;
            public readonly List<string> errs = new List<string>();
            public readonly List<string> widows = new List<string>();
            public readonly List<string> wrapNotes = new List<string>(); // ragged / single-word wraps that the 50% rule cannot meet
            public List<string> supTags; // t1831 task 9: the support's two tag phrases ("SUPPORT", "NEEDS HIT") for the row-even break // t1831 task 9: wrapped paragraphs whose last line is one word
        }
        struct RowD { public int plateH, iconY, headY, headN, ruleY, bodyY, bodyH, tagsY, tagN, dotsY, cardH; }

        // ---------------------------------------------------------------- glyph ink (ink-centred text, like the mockups)
        static int[] _gInkL1831, _gInkR1831;
        void GlyphInk1831()
        {
            if (_gInkL1831 != null || !_tFont) return;
            int n = FontMap.Length; var l = new int[n]; var r = new int[n];
            for (int k = 0; k < n; k++) { l[k] = 0; r[k] = 4; }
            try
            {
                int w = _tFont.width, h = _tFont.height;
                var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32); var prev = RenderTexture.active;
                Graphics.Blit(_tFont, rt); RenderTexture.active = rt;
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false); t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply(false);
                RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
                var px = t.GetPixels32(); Destroy(t);
                for (int k = 0; k < n && (k + 1) * 5 <= w; k++)
                {
                    int a = 5, b = -1;
                    for (int c = 0; c < 5; c++) for (int y = 0; y < h; y++) if (px[y * w + k * 5 + c].a > 127) { a = Mathf.Min(a, c); b = Mathf.Max(b, c); }
                    if (b >= 0) { l[k] = a; r[k] = b; }
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[t1831] glyph ink readback failed, advance-based centring: " + e.Message); }
            _gInkL1831 = l; _gInkR1831 = r;
        }
        /// <summary>Ink span [a, b] of a string drawn at x = 0 (glyph bearings included). b &lt; a when there is no ink.</summary>
        static void InkD(string s, out int a, out int b)
        {
            a = int.MaxValue; b = int.MinValue;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = FontGlyph(s[i]); if (ch == ' ') continue; int k = FontMap.IndexOf(ch); if (k < 0) k = FontMap.IndexOf('?');
                int gl = _gInkL1831 != null ? _gInkL1831[k] : 0, gr = _gInkR1831 != null ? _gInkR1831[k] : 4;
                a = Mathf.Min(a, i * 6 + gl); b = Mathf.Max(b, i * 6 + gr);
            }
            if (b < a) { a = 0; b = -1; }
        }
        static int InkW(string s) { InkD(s, out int a, out int b); return b - a + 1; }
        /// <summary>x that centres the ink of s on the card (odd leftover pixel goes right).</summary>
        static int CentreInkX(int cardX, string s) { InkD(s, out int a, out int b); return cardX + (CardW - (b - a + 1)) / 2 - a; }

        // ---------------------------------------------------------------- copy -> coloured segments (wr3 legend)
        static bool IsStatusD(string[] w, int i, out int span, out string kw)
        {
            foreach (var k in CardCopyD1831.StatusWords)
            {
                var parts = k.Split(' '); if (i + parts.Length > w.Length) continue;
                bool ok = true; for (int q = 0; q < parts.Length && ok; q++) ok = Core(w[i + q]) == parts[q];
                if (ok) { span = parts.Length; kw = k; return true; }
            }
            span = 1; kw = null; return false;
        }
        /// <summary>Numbers (with their % / S / /S unit) gold, status words in their hue, a "LABEL:" lead-in grey, the rest baseC.</summary>
        static List<CSeg> ColorizeD(string text, Color baseC, bool red = false)
        {
            var l = new List<CSeg>(); text = Sanitize(text ?? "").Trim(); if (text.Length == 0) return l;
            var w = text.Split(' ');
            int label = -1; if (!red) for (int i = 0; i < w.Length - 1; i++) if (w[i].EndsWith(":")) label = i;
            for (int i = 0; i < w.Length;)
            {
                if (i > 0) Add(l, " ", red ? CRed : baseC);
                if (red) { Add(l, w[i], CRed); i++; continue; }
                Color wc = i <= label ? CDim : baseC;
                if (IsStatusD(w, i, out int span, out string kw)) { string joined = string.Join(" ", w, i, span), core = Core(joined); Add(l, core, CardCopyD1831.StatusCol(kw)); Add(l, joined.Substring(core.Length), wc); i += span; continue; }
                string word = w[i]; int nl = NumLen(word, out int ul);
                if (nl > 0) { Add(l, word.Substring(0, nl + ul), CGold); Add(l, word.Substring(nl + ul), wc); } else Add(l, word, wc);
                i++;
            }
            return l;
        }
        /// <summary>Key range on a card-row footer: "1" when there is one card, otherwise "1-n".</summary>
        static string CardKeyRange1831(int n) => n <= 1 ? "1" : "1-" + n;
        static bool SentEndD1831(string w) { if (string.IsNullOrEmpty(w)) return false; char c = w[w.Length - 1]; return c == '.' || c == '!' || c == '?'; }
        /// <summary>Balanced wrap of a word list to maxW ink px (Knuth-Plass / DP). Uses the fewest lines that fit,
        /// then breaks that minimise the sum of squared slack on every line. No one-word line unless the block cannot
        /// avoid it. A sentence prefers to start on a new line, but never at the cost of a short line.</summary>
        static List<string> WrapWordsD1831(List<string> words, int maxW)
        {
            var o = new List<string>(); int n = words.Count; if (n == 0) return o;
            if (n == 1) { o.Add(words[0]); return o; }
            string Join(int a, int b) { var sb = new StringBuilder(); for (int i = a; i < b; i++) { if (i > a) sb.Append(' '); sb.Append(words[i]); } return sb.ToString(); }
            const int Inf = 1 << 28, Bury = 1800; // buried sentence-end costs less than a very short line (slack ~70^2)
            var fit = new bool[n + 1, n + 1];
            var lc = new int[n + 1, n + 1];
            for (int i = 0; i < n; i++) for (int j = i + 1; j <= n; j++)
            {
                int ink = InkW(Join(i, j)); bool ok = j - i == 1 || ink <= maxW; fit[i, j] = ok;
                if (!ok) { lc[i, j] = Inf; continue; }
                int slack = Mathf.Max(0, maxW - ink), cost = slack * slack;
                for (int k = i; k < j - 1; k++) if (SentEndD1831(words[k])) cost += Bury; // sentence should start on the next line when it does not create a short line
                lc[i, j] = cost;
            }
            var minL = new int[n + 1]; minL[0] = 0; for (int j = 1; j <= n; j++) { minL[j] = Inf; for (int i = 0; i < j; i++) if (fit[i, j] && minL[i] + 1 < minL[j]) minL[j] = minL[i] + 1; }
            int K = minL[n]; if (K >= Inf) { o.Add(Join(0, n)); return o; }
            int[,] dp = new int[K + 1, n + 1], pr = new int[K + 1, n + 1];
            void Run(bool noOne)
            {
                for (int k = 0; k <= K; k++) for (int j = 0; j <= n; j++) { dp[k, j] = Inf; pr[k, j] = -1; }
                dp[0, 0] = 0;
                for (int k = 1; k <= K; k++)
                    for (int j = 1; j <= n; j++)
                        for (int i = 0; i < j; i++)
                        {
                            if (!fit[i, j] || dp[k - 1, i] >= Inf) continue;
                            if (noOne && j - i == 1) continue;
                            int c = dp[k - 1, i] + lc[i, j]; if (c < dp[k, j]) { dp[k, j] = c; pr[k, j] = i; }
                        }
            }
            Run(true); if (dp[K, n] >= Inf) Run(false);
            if (dp[K, n] >= Inf) { o.Add(Join(0, n)); return o; }
            var cuts = new List<int>(); int p = n, k0 = K; cuts.Add(n);
            while (k0 > 0 && p > 0) { p = pr[k0, p]; cuts.Add(p); k0--; }
            cuts.Reverse();
            for (int q = 0; q + 1 < cuts.Count; q++) o.Add(Join(cuts[q], cuts[q + 1]));
            return o;
        }
        /// <summary>Fewest lines the wrapper uses for this text at maxW (same rule as WrapD).</summary>
        static int MinLinesD1831(string text, int maxW)
        {
            text = Sanitize(text ?? "").Trim(); if (text.Length == 0) return 0;
            return WrapWordsD1831(new List<string>(text.Split(' ')), maxW).Count;
        }
        /// <summary>Word wrap of a coloured line to maxW ink px (balanced DP, same rule for headlines and details).</summary>
        static List<List<CSeg>> WrapD(List<CSeg> line, int maxW)
        {
            var chars = new List<KeyValuePair<char, Color>>(); foreach (var s in line) foreach (char ch in s.t) chars.Add(new KeyValuePair<char, Color>(ch, s.c));
            var words = new List<List<KeyValuePair<char, Color>>>(); var cur = new List<KeyValuePair<char, Color>>();
            foreach (var kv in chars) { if (kv.Key == ' ') { if (cur.Count > 0) { words.Add(cur); cur = new List<KeyValuePair<char, Color>>(); } } else cur.Add(kv); }
            if (cur.Count > 0) words.Add(cur);
            var o = new List<List<CSeg>>(); if (words.Count == 0) return o;
            var plain = new List<string>(); foreach (var w in words) { var sb = new StringBuilder(); foreach (var kv in w) sb.Append(kv.Key); plain.Add(sb.ToString()); }
            var lines = WrapWordsD1831(plain, maxW);
            int wi = 0;
            foreach (var t in lines)
            {
                int need = t.Split(' ').Length; var seg = new List<CSeg>();
                for (int k = 0; k < need && wi < words.Count; k++, wi++)
                { if (k > 0) Add(seg, " ", words[wi][0].Value); foreach (var kv in words[wi]) Add(seg, kv.Key.ToString(), kv.Value); }
                o.Add(seg);
            }
            return o;
        }
        static List<CSeg> JoinBlocksD1831(List<List<CSeg>> blocks)
        {
            var a = new List<CSeg>();
            foreach (var b in blocks)
            {
                if (b == null || b.Count == 0) continue;
                if (a.Count > 0) Add(a, " ", a[a.Count - 1].c);
                a.AddRange(b);
            }
            return a;
        }
        static void NoteWrapD1831(List<List<CSeg>> lines, int maxW, List<string> notes, string label)
        {
            if (notes == null || lines == null || lines.Count < 2) return;
            int mn = int.MaxValue, mx = 0; var one = new List<string>(); var all = new List<string>();
            foreach (var l in lines)
            {
                string t = LineStr(l).Trim(); all.Add(t); int w = InkW(t); if (w < mn) mn = w; if (w > mx) mx = w;
                int wc = 0; for (int i = 0; i < t.Length; i++) if (t[i] != ' ' && (i == 0 || t[i - 1] == ' ')) wc++;
                if (wc == 1) one.Add(t);
            }
            string shown = string.Join(" / ", all);
            if (one.Count > 0) notes.Add(label + " single-word line '" + string.Join("', '", one.ToArray()) + "' in '" + shown + "'");
            if (mx > 0 && mn * 2 < mx) notes.Add(label + " shortest " + mn + " px is under 50% of longest " + mx + " px in '" + shown + "'");
        }
        static List<List<CSeg>> WrapCheckedD1831(List<CSeg> line, int maxW, List<string> errs, List<string> notes, string label)
        {
            var w = WrapD(line, maxW); int min = MinLinesD1831(LineStr(line), maxW);
            if (w.Count != min) errs?.Add("wrap used " + w.Count + " lines, min is " + min + " at " + maxW + " px: '" + LineStr(line) + "'");
            NoteWrapD1831(w, maxW, notes, label);
            return w;
        }
        static string KitHeaderD1831(SkillDef d)
        {
            if (d == null || !d.kit1651) return null;
            if ((d.tags & STag.Movement) != 0) return ZhKitType1639[1];
            if ((d.tags & STag.Special) != 0) return ZhKitType1639[2];
            if ((d.tags & STag.Ultimate) != 0) return ZhKitType1639[3];
            return ZhKitType1639[2];
        }

        /// <summary>"COOLDOWN 1.4S" -> COOLDOWN | 1.4S; "+15% MAX HP" -> MAX HP | +15%. False when the line has no number.</summary>
        static bool SplitRowD(string s, out string label, out string value)
        {
            var w = Sanitize(s).Trim().Split(' '); label = value = null;
            for (int i = 0; i < w.Length; i++)
            {
                if (NumLen(w[i], out _) == 0) continue;
                if (i == 0) { if (w.Length == 1) return false; value = w[0]; label = string.Join(" ", w, 1, w.Length - 1); }
                else { label = string.Join(" ", w, 0, i); value = string.Join(" ", w, i, w.Length - i); }
                return true;
            }
            return false;
        }

        // ---------------------------------------------------------------- building faces
        float LiveDmg1831(SkillDef def, SkillInst inst, int L, List<string> errs)
        {
            try
            {
                var si = inst ?? new SkillInst { def = def, level = Mathf.Max(1, L) };
                float d = SkillDamage(si);
                if (inst != null && inst.level != L) d *= HData.LevelMul(L) / Mathf.Max(1e-4f, HData.LevelMul(inst.level));
                return d;
            }
            catch (System.Exception e) { errs.Add("dmg: " + e.Message); return 0f; }
        }
        /// <summary>Live kit hit at the shown level, the same formula as ZkHit1651 (kit base x the skill's damage mods x level).</summary>
        float LiveKit1831(SkillInst inst, int L, List<string> errs)
        {
            try { return ZkHit1618 * (inst != null ? inst.More() : 1f) * HData.LevelMul(Mathf.Max(1, L)) / Mathf.Max(1e-4f, HData.LevelMul(1)); }
            catch (System.Exception e) { errs.Add("kit: " + e.Message); return 0f; }
        }
        void FillD1831(CardFaceD f, string[] src, string[] srcA, CardCtx1825 ctx)
        {
            var headSb = new StringBuilder(); int skip = -1; var fl = new List<List<CSeg>>(); var xl = new List<List<CSeg>>();
            void Row(string lab, string val) { f.rows.Add(new[] { ColorizeD(lab, CDim), ColorizeD(val, Color.white) }); }
            foreach (var l in CardEval1825.Lines(src, "HRFXA", ctx, f.errs))
            {
                switch (l.kind)
                {
                    case 'H': if (headSb.Length > 0) headSb.Append(' '); headSb.Append(l.text); break;
                    case 'R': { int t = l.text.IndexOf(" ~ "); if (t < 0) { f.errs.Add("row without ' ~ ': " + l.text); break; } Row(l.text.Substring(0, t), l.text.Substring(t + 3)); break; }
                    case 'F': fl.Add(ColorizeD(l.text, CDSilver)); break;
                    case 'X': xl.Add(ColorizeD(l.text, CRed, true)); break;
                    case 'A': if (!int.TryParse(l.text.Trim(), out skip)) { f.errs.Add("bad A:" + l.text); skip = 0; } break;
                }
            }
            if (skip >= 0)
            {
                if (srcA == null) f.errs.Add("A: but no Option A copy");
                else
                {
                    foreach (var l in CardEval1825.Lines(srcA, "SD", ctx, f.errs))
                    {
                        if (l.kind == 'D') { xl.Add(ColorizeD(l.text, CRed, true)); continue; }
                        if (SplitRowD(l.text, out var lab, out var val)) Row(lab, val); else fl.Add(ColorizeD(l.text, CDSilver));
                    }
                    var now = new List<string>(); foreach (var s in srcA) if (s.IndexOf("@MAX") < 0 && s.IndexOf("MAX LV") < 0) now.Add(s);
                    int fi = 0;
                    foreach (var l in CardEval1825.Lines(now.ToArray(), "FX", ctx, f.errs))
                    {
                        if (l.kind == 'F' && fi++ < skip) continue;
                        if (l.kind == 'X') xl.Add(ColorizeD(l.text, CRed, true)); else fl.Add(ColorizeD(l.text, CDSilver));
                    }
                }
            }
            if (headSb.Length == 0) f.errs.Add("no headline");
            void Par(List<List<CSeg>> into, List<CSeg> l, string label) { var w = WrapCheckedD1831(l, CardTextW, f.errs, f.wrapNotes, label); if (w.Count > 1 && LineStr(w[w.Count - 1]).Trim().IndexOf(' ') < 0) f.widows.Add(string.Join(" / ", w.ConvertAll(LineStr))); into.AddRange(w); }
            Par(f.head, ColorizeD(headSb.ToString(), Color.white), "headline");
            if (fl.Count > 0) Par(f.detail, JoinBlocksD1831(fl), "detail");
            if (xl.Count > 0) Par(f.detail, JoinBlocksD1831(xl), "limit");
        }
        /// <summary>t1831 task 9: the skill's printed tag names (canonical order, hidden tags dropped), as TagLines1825 picks them.</summary>
        /// <summary>t1831 task 9: the support's red limit line as a plain phrase ("NOT FOR MINION OR KIT SKILLS.") instead of A's "NOT MINION OR KIT".</summary>
        static string ExcludeLineD1831(STag ex) { string a = ExcludeLine1825(ex); return "NOT FOR " + (a.StartsWith("NOT ") ? a.Substring(4) : a) + " SKILLS."; }
        static List<string> TagNamesD1831(STag tags)
        {
            var names = new List<string>();
            foreach (var t in HData.TagOrder1825) if ((tags & t) != 0 && (t & CardCopyA1825.HiddenTags) == 0) names.Add(TagName1825(t));
            if (names.Count == 0) foreach (var t in HData.TagOrder1825) if ((tags & t) != 0) names.Add(TagName1825(t));
            return names;
        }
        /// <summary>t1831 task 9: pack whole tags into grey lines of CardTextW. Breaks only BETWEEN tags (a tag or phrase is never
        /// split), fewest lines first, then the most even line lengths, then the longer top line. commaAtBreak: a line that
        /// continues keeps its trailing comma (skill tag lists; when their canonical order needs 3+ lines, whole tags may move between the
        /// two lines to fit 2); false = the break replaces the comma ("SUPPORT" / "NEEDS CHAIN").</summary>
        static List<List<CSeg>> PackTagsD1831(List<string> items, bool commaAtBreak, out bool overflow)
        {
            var o = new List<List<CSeg>>(); overflow = false; int n = items.Count; if (n == 0) return o;
            int maxC = (CardTextW + 1) / 6;
            string LineOf(int a, int b, bool last) { var sb = new StringBuilder(); for (int i = a; i < b; i++) { if (i > a) sb.Append(", "); sb.Append(items[i]); } if (!last && commaAtBreak) sb.Append(','); return sb.ToString(); }
            List<int> best = null; int bestSpread = int.MaxValue, bestTop = -1;
            for (int k = 1; k <= n && best == null; k++)
            {
                var cuts = new int[k + 1]; cuts[0] = 0; cuts[k] = n;
                void Rec(int idx)
                {
                    if (idx == k)
                    {
                        int mx = 0, mn = int.MaxValue;
                        for (int q = 0; q < k; q++) { int L = LineOf(cuts[q], cuts[q + 1], q == k - 1).Length; if (L > maxC) return; mx = Mathf.Max(mx, L); mn = Mathf.Min(mn, L); }
                        int spread = mx - mn, top = LineOf(cuts[0], cuts[1], k == 1).Length;
                        if (best == null || spread < bestSpread || (spread == bestSpread && top > bestTop)) { best = new List<int>(cuts); bestSpread = spread; bestTop = top; }
                        return;
                    }
                    for (int c = cuts[idx - 1] + 1; c <= n - (k - idx); c++) { cuts[idx] = c; Rec(idx + 1); }
                }
                Rec(1);
            }
            if (best == null) { overflow = true; best = new List<int>(); for (int i = 0; i <= n; i++) best.Add(i); } // a single tag wider than the card
            if (commaAtBreak && best.Count - 1 > 2 && n <= 12)
            {   // canonical order needs 3+ lines: move whole tags between the two lines (each line keeps the canonical order),
                // as TagLines1825 did, and take the most even 2-line split (longer top line on a tie)
                int bm = -1, bs = int.MaxValue, bt = -1;
                string Join(int mask, bool want, bool last) { var sb = new StringBuilder(); for (int i = 0; i < n; i++) if ((((mask >> i) & 1) != 0) == want) { if (sb.Length > 0) sb.Append(", "); sb.Append(items[i]); } if (!last) sb.Append(','); return sb.ToString(); }
                for (int mask = 1; mask < (1 << n) - 1; mask++)
                {
                    int a = Join(mask, true, false).Length, b = Join(mask, false, true).Length; if (a > maxC || b > maxC) continue;
                    int sp = Mathf.Abs(a - b); if (sp < bs || (sp == bs && a > bt)) { bm = mask; bs = sp; bt = a; }
                }
                if (bm >= 0)
                {
                    foreach (var t in new[] { Join(bm, true, false), Join(bm, false, true) }) { var l = new List<CSeg>(); Add(l, t, CDim); o.Add(l); }
                    return o;
                }
            }
            for (int q = 0; q + 1 < best.Count; q++) { var l = new List<CSeg>(); Add(l, LineOf(best[q], best[q + 1], q + 2 == best.Count), CDim); o.Add(l); }
            if (o.Count > CardDTagMax) overflow = true;
            return o;
        }
        static void GreyTags1831(List<List<CSeg>> into, List<List<CSeg>> src) { foreach (var l in src) { var g = new List<CSeg>(); Add(g, LineStr(l), CDim); into.Add(g); } }
        void SkillFaceD1831(CardFaceD f, SkillDef def, SkillInst inst, int L, string header)
        {
            var ctx = CtxFor1825(inst, def, L, 0); ctx.dmg = LiveDmg1831(def, inst, L, f.errs); ctx.kit = LiveKit1831(inst, L, f.errs);
            CardCopyD1831.Skill.TryGetValue(def.id, out var src); if (src == null) f.errs.Add("no D copy for skill " + def.id);
            CardCopyA1825.Skill.TryGetValue(def.id, out var srcA);
            FillD1831(f, src, srcA, ctx);
            if (!string.IsNullOrEmpty(header)) { var h = new List<CSeg>(); Add(h, Sanitize(header), CDim); f.tags.Add(h); }
            var tn = TagNamesD1831(def.tags);
            if (!string.IsNullOrEmpty(header)) { string type = header.Trim().Split(' ')[0]; tn.RemoveAll(t => t == type); } // kit-type line already names MOVEMENT / SPECIAL / ULTIMATE
            f.tags.AddRange(PackTagsD1831(tn, true, out bool ov)); if (ov) f.errs.Add("tag overflow");
            f.dots = L;
        }
        void SupportFaceD1831(CardFaceD f, SupportDef sd, int L, SkillInst tgt, bool multiTgt)
        {
            f.support = true;
            var ctx = new CardCtx1825 { L = L, A = 0, tgt = tgt != null ? (int)tgt.def.id : -1, multi = tgt == null || HData.MultiHonoured1651(tgt.def.id) };
            CardCopyD1831.Support.TryGetValue(sd.id, out var src); if (src == null) f.errs.Add("no D copy for support " + sd.id);
            CardCopyA1825.Support.TryGetValue(sd.id, out var srcA);
            FillD1831(f, src, srcA, ctx);
            if (sd.excludes != 0) f.detail.AddRange(WrapCheckedD1831(ColorizeD(ExcludeLineD1831(sd.excludes), CRed, true), CardTextW, f.errs, f.wrapNotes, "limit"));
            // t1831 task 9: two whole phrases, never split inside one: "SUPPORT, NEEDS HIT" when it fits, else "SUPPORT" / "NEEDS CHAIN"
            if (HellordTuning.I.supTagsOwn1832)
            {   // t1832 PREVIEW (VH): the green frame already says SUPPORT; the tag row shows only the tag(s) a skill needs to take it, in the
                // same colours as the skill cards' tag rows so the two match by eye. Fits any skill = no tag.
                var need = new List<string>(); if (sd.needs != STag.Any) need.AddRange(TagNamesD1831(sd.needs)); foreach (var n in TagNamesD1831(sd.requiresAll)) if (sd.requiresAll != 0 && !need.Contains(n)) need.Add(n);
                f.tags.AddRange(PackTagsD1831(need, true, out bool ov2)); if (ov2) f.errs.Add("support tag overflow");
            }
            else
            {
            var tp = new List<string> { "SUPPORT", sd.needs == STag.Any ? "FITS ANY SKILL" : "NEEDS " + HData.TagText(sd.needs, " OR ", true) };
            f.supTags = tp; f.tags.AddRange(PackTagsD1831(tp, false, out bool ov)); if (ov) f.errs.Add("support tag overflow");
            }
            f.dots = L;
        }
        CardFaceD BuildFaceD1831(Card c)
        {
            var f = new CardFaceD(); f.name = CardNameOf(c); f.title = WrapCard(f.name);
            if (c.kit > 0)
            {
                var k = KitK1666(c.kit - 1, c.kitPick); var ki = c.kitLv1651 ? ZkSlotInst1651(c.kit - 1) : ZkI1651(k); var def = ki != null ? ki.def : ZkI1651(k).def;
                if (c.kitLv1651 && ki != null) SkillFaceD1831(f, def, ki, ki.level + 1, ZhKitType1639[c.kit]);
                else { SkillFaceD1831(f, def, null, 1, ZhKitType1639[c.kit]); f.showNew = true; }
                return f;
            }
            switch (c.kind)
            {
                case CardKind.NewSkill: SkillFaceD1831(f, c.skill, null, 1, null); f.showNew = true; break;
                case CardKind.SkillLevel: { var s = skills[c.skillIndex]; SkillFaceD1831(f, s.def, s, s.level + 1, null); break; }
                case CardKind.Stat:
                {
                    CardCopyD1831.Stat.TryGetValue(c.stat.id, out var src); if (src == null) f.errs.Add("no D copy for stat " + c.stat.id);
                    CardCopyA1825.Stat.TryGetValue(c.stat.id, out var srcA);
                    FillD1831(f, src, srcA, new CardCtx1825());
                    var l1 = new List<CSeg>(); Add(l1, "STAT", CDim); f.tags.Add(l1); f.hasDots = false; break;
                }
                case CardKind.Support:
                {
                    var tl = SupportTargets(c.support); var tgt = tl.Count == 1 ? SupInst1651(tl[0]) : null; int cur = tgt != null ? tgt.Lvl(c.support.id) : 0;
                    bool anyUp = false; foreach (int q in tl) { var sq = SupInst1651(q); if (sq != null && sq.Has(c.support.id)) anyUp = true; }
                    SupportFaceD1831(f, c.support, cur + 1, tgt, tl.Count > 1);
                    f.showNew = cur == 0 && !anyUp; break;
                }
            }
            return f;
        }
        /// <summary>WHICH SKILL? picker: one D card per target skill (its own face, dots = its level).</summary>
        List<CardFaceD> PickFacesD1831()
        {
            var tp = _tp; var faces = new List<CardFaceD>(); if (tp == null) return faces;
            for (int ix = 0; ix < tp.targets.Count; ix++)
            {
                var sk = SupInst1651(tp.targets[ix]); var f = new CardFaceD(); faces.Add(f); if (sk == null) continue;
                f.name = sk.def.name; f.title = WrapCard(sk.def.name);
                SkillFaceD1831(f, sk.def, sk, sk.level, null);
                if (tp.swap && sk.sup.Count > 0) f.detail.AddRange(WrapCheckedD1831(ColorizeD("REPLACES " + HData.Support(sk.sup[sk.sup.Count - 1]).name, CRed, true), CardTextW, f.errs, f.wrapNotes, "limit"));
            }
            return faces;
        }

        // ---------------------------------------------------------------- row layout (one source for drawing and the audit)
        static int BodyHD(CardFaceD f) { int h = 0; if (f.rows.Count > 0) h += THA(f.rows.Count); if (f.detail.Count > 0) h += (f.rows.Count > 0 ? CardPad : 0) + THA(f.detail.Count); return h; }
        /// <summary>t1831 task 9: when one support in the offer row needs the 2-line tag break ("SUPPORT" / "NEEDS PHYSICAL"), every
        /// support in that row takes the same break, so the support tag blocks read the same across the row.</summary>
        static void EvenSupTagsD1831(IList<CardFaceD> faces)
        {
            bool two = false; foreach (var f in faces) if (f.support && f.supTags != null && f.tags.Count > 1) two = true;
            if (!two) return;
            foreach (var f in faces)
            {
                if (!f.support || f.supTags == null || f.tags.Count > 1) continue;
                f.tags.Clear(); foreach (var t in f.supTags) { var l = new List<CSeg>(); Add(l, t, CDim); f.tags.Add(l); }
            }
        }
        RowD LayoutRowD1831(IList<CardFaceD> faces)
        {
            int mT = 1, mH = 1, mTag = 1, mB = 0;
            foreach (var f in faces) { mT = Mathf.Max(mT, f.title.Count); mH = Mathf.Max(mH, f.head.Count); mTag = Mathf.Max(mTag, f.tags.Count); mB = Mathf.Max(mB, BodyHD(f)); }
            var r = new RowD(); r.plateH = CardPlateH(mT); r.iconY = r.plateH + CardPad;
            r.headY = r.iconY + CardIcon + CardPad; r.headN = mH;
            r.ruleY = r.headY + THA(mH) + CardRuleGap; r.bodyY = r.ruleY + 1 + CardRuleGap; r.bodyH = mB;
            r.tagsY = r.bodyY + mB + CardDTagGap; r.tagN = mTag; r.dotsY = r.tagsY + THA(mTag) + CardPad; r.cardH = r.dotsY + CardDot + CardPad;
            return r;
        }

        // ---------------------------------------------------------------- the frame texture (fs2.py D, pixel for pixel)
        readonly Dictionary<long, Texture2D> _frameD1831 = new Dictionary<long, Texture2D>();
        Texture2D FrameTexD1831(bool support, RowD r, int dots)
        {
            long key = ((((long)(support ? 1 : 0) * 1024 + r.cardH) * 1024 + r.plateH) * 1024 + r.ruleY) * 1024 + r.iconY; key = key * 16 + (dots + 1); key = key * 1024 + r.dotsY;
            if (_frameD1831.TryGetValue(key, out var tex) && tex) return tex;
            if (_frameD1831.Count > 64) { foreach (var t in _frameD1831.Values) if (t) Destroy(t); _frameD1831.Clear(); }
            var K = support ? FrameDSupport : FrameDSkill; int W = CardW, H = r.cardH;
            var px = new Color32[W * H];
            void P(int x, int y, Color32 c) { if (x >= 0 && x < W && y >= 0 && y < H) px[(H - 1 - y) * W + x] = c; }
            void HL(int y, int x0, Color32 c) { for (int x = x0; x <= W - 1 - x0; x++) P(x, y, c); }   // always mirror-symmetric
            var prof = new[] { E32Ink, K.hi, K.mid, K.lo }; int bw = prof.Length;
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { int d = Mathf.Min(Mathf.Min(x, W - 1 - x), Mathf.Min(y, H - 1 - y)); P(x, y, d < bw ? prof[d] : E32Ink); }
            for (int y = CardDTitleTop; y <= r.plateH - 5; y++) HL(y, bw, K.lo);                                   // flat title plate
            int fy = r.plateH - FrameDWave.Length, period = FrameDWave[0].Length;                                     // fret under the plate
            for (int x = FrameDFretX0; x <= W - 1 - FrameDFretX0; x++) { int col = (x - FrameDFretX0) % period; for (int q = 0; q < FrameDWave.Length; q++) if (FrameDWave[q][col] == '#') P(x, fy + q, K.fret); }
            int ix = (W - CardIcon) / 2, iy = r.iconY;                                                                   // 2 px icon ring
            HL(iy - FrameDRingW, ix - FrameDRingW, K.ringOut); HL(iy + CardIcon + 1, ix - FrameDRingW, K.ringOut);
            HL(iy - 1, ix - 1, K.ringIn); HL(iy + CardIcon, ix - 1, K.ringIn);
            for (int y = iy - 1; y <= iy + CardIcon; y++) { P(ix - 2, y, K.ringOut); P(W - 1 - (ix - 2), y, K.ringOut); P(ix - 1, y, K.ringIn); P(W - 1 - (ix - 1), y, K.ringIn); }
            HL(r.ruleY, (W - CardRuleW) / 2, K.fret);                                                                      // 40 px rule
            if (dots >= 0)
            {
                int dx = (W - CardDotsW) / 2;
                for (int d = 0; d < CardDotsN; d++) for (int q = 0; q < FrameDDot.Length; q++) for (int i = 0; i < FrameDDot[q].Length; i++)
                    if (FrameDDot[q][i] == '#') P(dx + d * (CardDot + CardDotGap) + i, r.dotsY + q, d < dots ? K.dotOn : K.dotOff);
            }
            tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "cardD1831" };
            tex.SetPixels32(px); tex.Apply(false, false); _frameD1831[key] = tex;
            return tex;
        }

        // ---------------------------------------------------------------- drawing
        // ---------------------------------------------------------------- t1831 task 9: icons on the pixel grid
        /// <summary>One icon sprite: texture + source cell in texels (sy from the top, like Blit) + tint. Resolved with the same
        /// precedence as DrawCardFace / DrawSkillIcon1825 so every card keeps its current art.</summary>
        struct IconSrcD { public Texture2D t; public int sx, sy, sw, sh; public Color tint; public string via; }
        static IconSrcD IconCellD(Texture2D t, int sx, int sy, int sw, int sh, string via, Color tint)
        {
            if (t == null) return default; sw = Mathf.Min(sw, t.width - sx); sh = Mathf.Min(sh, t.height - sy);
            return new IconSrcD { t = t, sx = sx, sy = sy, sw = sw, sh = sh, tint = tint, via = via };
        }
        static IconSrcD IconWholeD(Texture2D t, string via) => t == null ? default : IconCellD(t, 0, 0, t.width, t.height, via, Color.white);
        IconSrcD RoomIconD1831(int cell) => IconCellD(_tRoomIcons, cell * 8, 0, 8, 8, "room", Color.white);
        IconSrcD SkillIconSrcD1831(SkillId id)
        {
            SkillBarTex();
            var zt = ZhCardIcon1625(id); if (zt != null) return IconWholeD(zt, "kit/zh");
            if (IsPb1567(id)) return IconCellD(PbIcon1567(id), 0, 0, 24, 24, "pb", Color.white);
            if (IsFb1481(id)) return IconCellD(FbIcon1481(id), 0, 0, 24, 24, "fb", Color.white);
            if (IceArt.IsIce(id)) return IconCellD(IceArt.Icon24(id), 0, 0, 24, 24, "ice", Color.white);
            if (_icons44 && id >= SkillId.ChainLightning && id <= SkillId.PhantomAxes && _tSkill24 != null) return IconCellD(_tSkill24, (int)id * 28, 0, 24, 24, "sheet24", Color.white);
            int ic = SkillIcon(id); if (ic < 0) return RoomIconD1831(0);
            return IconCellD(_tSkillIcons, ic * 16, 0, 16, 16, "sheet16", PhTint(id));
        }
        IconSrcD CardIconSrcD1831(Card c)
        {
            SkillBarTex();
            if (c.kit > 0) { var ic = KitIcon1666(ZhKitKey1639(c)); return ic != null ? IconWholeD(ic, "kit") : RoomIconD1831(0); }
            if (c.kind == CardKind.NewSkill) return SkillIconSrcD1831(c.skill.id);
            if (c.kind == CardKind.SkillLevel) return SkillIconSrcD1831(skills[c.skillIndex].def.id);
            if (c.kind == CardKind.Support)
            {
                int si = SupportIcon(c.support.id); if (si >= 0) return IconCellD(_tSkillIcons, si * 16, 0, 16, 16, "sheet16", Color.white);
                if (_tSup20 != null) { int k = SupIx(c.support.id); return IconCellD(_tSup20, (k % 10) * 20, (k / 10) * 20, 20, 20, "sup20", Color.white); }
                return RoomIconD1831(5);
            }
            int st = c.stat != null ? StatIcon(c.stat.id) : -1;
            return st >= 0 ? IconCellD(_tSkillIcons, st * 16, 0, 16, 16, "sheet16", Color.white) : RoomIconD1831(0);
        }
        /// <summary>Draw an icon at the largest WHOLE-number scale that fits the 48 px ring interior, centred (odd leftover px right /
        /// bottom), point-sampled with exact texel UVs, so every texel is an exact N x N block of native pixels (3N x 3N on
        /// screen). A sprite bigger than 48 px is shown 1:1, centre-cropped (logged).</summary>
        void DrawIconD1831(IconSrcD s, int ix, int iy, float ra, AuditRecD ar)
        {
            if (s.t == null) { ar?.log.Add("icon none"); return; }
            int sx = s.sx, sy = s.sy, sw = s.sw, sh = s.sh, sc = Mathf.Min(CardIcon / Mathf.Max(1, sw), CardIcon / Mathf.Max(1, sh)); bool crop = false;
            if (sc < 1) { sc = 1; int cw = Mathf.Min(sw, CardIcon), ch = Mathf.Min(sh, CardIcon); sx += (sw - cw) / 2; sy += (sh - ch) / 2; sw = cw; sh = ch; crop = true; }
            int dw = sw * sc, dh = sh * sc, mL = (CardIcon - dw) / 2, mT = (CardIcon - dh) / 2, dx = ix + mL, dy = iy + mT;
            if (s.t.filterMode != FilterMode.Point) s.t.filterMode = FilterMode.Point;
            float W = s.t.width, H = s.t.height;
            GUI.color = new Color(s.tint.r, s.tint.g, s.tint.b, ra);
            GUI.DrawTextureWithTexCoords(new Rect(dx, dy, dw, dh), s.t, new Rect(sx / W, 1f - (sy + sh) / H, sw / W, sh / H));
            GUI.color = new Color(1, 1, 1, ra);
            if (ar != null)
            {
                ar.iconD = new[] { dx - ar.x, dy - ar.y, dw, dh, sc, mL, CardIcon - dw - mL, mT, CardIcon - dh - mT };
                ar.log.Add("icon dx=" + (dx - ar.x) + " dy=" + (dy - ar.y) + " dw=" + dw + " dh=" + dh + " sc=" + sc + " m=" + mL + "," + (CardIcon - dw - mL) + "," + mT + "," + (CardIcon - dh - mT)
                    + " tex='" + s.t.name + "' tw=" + s.t.width + " th=" + s.t.height + " src=" + sx + "," + sy + "," + sw + "," + sh + " tint=" + (s.tint == Color.white ? "white" : ColorUtility.ToHtmlStringRGB(s.tint)) + " via=" + s.via + (crop ? " CROPPED" : ""));
            }
        }
        sealed class AuditRecD
        {
            public int x, y, cardH; public bool support; public string name;
            public readonly List<string> log = new List<string>();
            public readonly List<int[]> centred = new List<int[]>();   // {inkL, inkR, y, sec}
            public readonly List<int[]> rows = new List<int[]>();      // {labelInkL, labelInkR, valueInkL, valueInkR, y}
            public readonly Dictionary<string, int> secY = new Dictionary<string, int>();
            public int[] icon, title, iconD; public int tagsBottom = -1, lastBodyBottom = -1, hotkeyInkL;
        }
        List<AuditRecD> _auditD1831;
        static readonly string[] SecD = { "title", "head", "detail", "tags" };
        void SegsD(List<CSeg> l, int x, int y) { int cx = x; foreach (var s in l) { Text(s.t, cx, y, s.c); cx += s.t.Length * 6; } }
        void CentredD(List<CSeg> l, int cardX, int y, AuditRecD ar, int sec)
        {
            string s = LineStr(l); InkD(s, out int a, out int b); int x = CentreInkX(cardX, s); SegsD(l, x, y);
            if (ar != null) { ar.centred.Add(new[] { x + a, x + b, y, sec }); string k = SecD[sec]; if (!ar.secY.ContainsKey(k)) ar.secY[k] = y - ar.y; ar.log.Add(k + " y+" + (y - ar.y) + " L" + (x + a - cardX) + " R" + (cardX + CardW - 1 - (x + b)) + " '" + s + "'"); }
        }
        void DrawCardD1831(CardFaceD f, RowD r, int x, int y, int index, float ra, IconSrcD icon)
        {
            GlyphInk1831();
            AuditRecD ar = _auditD1831 != null && Event.current != null && Event.current.type == EventType.Repaint ? new AuditRecD { x = x, y = y, cardH = r.cardH, support = f.support, name = f.name } : null; if (ar != null) _auditD1831.Add(ar);
            var tex = FrameTexD1831(f.support, r, f.hasDots ? f.dots : -1);
            GUI.color = new Color(1, 1, 1, ra); GUI.DrawTexture(new Rect(x, y, CardW, r.cardH), tex); GUI.color = new Color(1, 1, 1, ra);
            int nT = Mathf.Max(1, f.title.Count), plateFill = r.plateH - 5 - CardDTitleTop + 1, ty = y + CardDTitleTop + (plateFill - (THA(nT) + 1)) / 2;
            for (int i = 0; i < f.title.Count; i++) { var tl = new List<CSeg>(); Add(tl, f.title[i], Color.white); CentredD(tl, x, ty + i * CardLine, ar, 0); }
            int ix = BoxCenterX(x, CardW, CardIcon), iy = y + r.iconY;
            GUI.color = new Color(1, 1, 1, ra); StampCardWell(0, ix, iy); DrawIconD1831(icon, ix, iy, ra, ar);
            if (f.showNew && _tNew) Blit(_tNew, 0, 0, CardBadgeW, CardBadgeH, x + CardW - CardPad - CardBadgeW, iy);
            if (ar != null) ar.icon = new[] { ix, iy };
            for (int i = 0; i < f.head.Count; i++) CentredD(f.head[i], x, y + r.headY + i * CardLine, ar, 1);
            int yy = y + r.bodyY;
            for (int i = 0; i < f.rows.Count; i++)
            {
                string ls = LineStr(f.rows[i][0]), vs = LineStr(f.rows[i][1]); InkD(ls, out int la, out int lb); InkD(vs, out int va, out int vb);
                int lx = x + CardTextPad - la, vx = x + CardW - CardTextPad - 1 - vb, ry = yy + i * CardLine;
                SegsD(f.rows[i][0], lx, ry); SegsD(f.rows[i][1], vx, ry);
                if (ar != null) { ar.rows.Add(new[] { lx + la, lx + lb, vx + va, vx + vb, ry }); if (!ar.secY.ContainsKey("rows")) ar.secY["rows"] = ry - y; ar.log.Add("row y+" + (ry - y) + " '" + ls + "' | '" + vs + "'"); ar.lastBodyBottom = ry + CardGlyph - y; }
            }
            if (f.rows.Count > 0) { yy += THA(f.rows.Count); if (f.detail.Count > 0) yy += CardPad; }
            for (int i = 0; i < f.detail.Count; i++) { CentredD(f.detail[i], x, yy + i * CardLine, ar, 2); if (ar != null) ar.lastBodyBottom = yy + i * CardLine + CardGlyph - y; }
            int n = f.tags.Count, tgy = y + r.tagsY + (r.tagN - n) * CardLine;
            for (int i = 0; i < n; i++) CentredD(f.tags[i], x, tgy + i * CardLine, ar, 3);
            if (ar != null) { ar.tagsBottom = n > 0 ? tgy + (n - 1) * CardLine + CardGlyph - y : -1; }
            string hk = (index + 1).ToString(); InkD(hk, out int ha, out _); Text(hk, x + CardPad - ha, y + r.dotsY, CDim);
            if (ar != null) ar.hotkeyInkL = CardPad;
        }

        /// <summary>t1831: DrawCardsB body (level-up / start pick / chest / kit) with frame D cards.</summary>
        void DrawCardsD1831(string title, string sub, bool levelUp)
        {
            FloorTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.69f * (levelUp ? FX26Dim : 1f)));
            _guiA = levelUp ? FX26Reveal(-1) : 1f; TextC(title, CardScreenTitleY, HCyan, 2); TextC(sub, CardScreenSubY, Color.white); _guiA = 1f;
            int n = cards.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; Card picked = null;
            var faces = new List<CardFaceD>(n); for (int i = 0; i < n; i++) faces.Add(BuildFaceD1831(cards[i]));
            EvenSupTagsD1831(faces); var r = LayoutRowD1831(faces);
            for (int i = 0; i < n; i++)
            {
                var c = cards[i]; int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, r.cardH);
                float ra = levelUp ? FX26Reveal(i) : 1f; if (ra <= 0.001f) continue; _guiA = ra; GUI.color = new Color(1, 1, 1, ra);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0) + (levelUp ? FX26Slide(i) : 0);
                DrawCardD1831(faces[i], r, x, y, i, ra, CardIconSrcD1831(c));
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover && !FX26CardsLocked) { picked = c; ev.Use(); }
                _guiA = 1f; GUI.color = Color.white;
            }
            int footY = rowY + r.cardH + CardPad; _lastFootY1825 = footY; _lastCardH1825 = r.cardH;
            string foot;
            if (levelUp)
            {
                string rr;
                if (freeRerolls > 0) rr = $"SPACE REROLL (FREE x{freeRerolls})";
                else if (CanReroll1058()) rr = $"SPACE REROLL ({RerollBoneCost1058()} BONES)";
                else rr = $"SPACE REROLL ({RerollBoneCost1058()} BONES) - NEED MORE";
                foot = $"{rr} - ESC SKIP (+10 BONES) - {CardKeyRange1831(n)} PICK";
            }
            else foot = mode == Mode.Chest ? (_bossReward37 ? $"{CardKeyRange1831(n)} TAKE" : $"{CardKeyRange1831(n)} TAKE - ESC SKIP (+10 BONES)") : $"{CardKeyRange1831(n)} PICK";
            TextC(foot, footY, CDim);
            if (!string.IsNullOrEmpty(warning)) TextC(warning, footY + CardLine, CRed);
            if (mode == Mode.Chest) TextC(_bossReward37 ? "BOSS REWARD: THIS PICK CANNOT BE SKIPPED" : "PICK A SUPPORT. IF SEVERAL SKILLS CAN TAKE IT YOU CHOOSE WHICH", footY + (string.IsNullOrEmpty(warning) ? CardLine : CardLine * 2), CDim);
            if (picked != null)
            {
                if (picked.kind == CardKind.Support) { PickSupTarget1831u(picked.support, SupportTargets(picked.support), false, tIx => Choose(picked, tIx)); } /* t1831u */
                else Choose(picked);
            }
        }
        void DrawTargetPickD1831()
        {
            var tp = _tp; if (tp == null) return; FloorTex(); SkillBarTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.8f));
            TextC("WHICH SKILL?", CardScreenTitleY, HCyan, 2); TextC(Sanitize(tp.sd.name) + (tp.swap ? " - SWAP" : ""), CardScreenSubY, Color.white);
            int n = tp.targets.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; int picked = -1;
            var faces = PickFacesD1831(); var r = LayoutRowD1831(faces);
            for (int i = 0; i < n; i++)
            {
                var sk = SupInst1651(tp.targets[i]); if (sk == null) continue;
                int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, r.cardH);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0);
                var id = sk.def.id;
                DrawCardD1831(faces[i], r, x, y, i, 1f, SkillIconSrcD1831(id));
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover) { picked = i; ev.Use(); }
            }
            int footY = rowY + r.cardH + CardPad; _lastFootY1825 = footY; _lastCardH1825 = r.cardH;
            TextC($"{CardKeyRange1831(n)} PICK THE SKILL - ESC BACK", footY, CDim);
            if (picked >= 0) ConfirmTarget(picked);
        }
    }
}
