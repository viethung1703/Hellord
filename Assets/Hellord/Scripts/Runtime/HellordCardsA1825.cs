using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1825 Option A (VH pick): PoE2 tooltip card on every card screen (level-up, start pick, chest, kit, WHICH SKILL?).
    // Order: name plate / comma tag line / icon (+NEW) / gold stat lines with dim labels / 1 px rule (40 px, 6 px above and
    // below) / short white fact lines / rule + TO: in cyan (supports) / level dots. Copy = CardCopyA1825 (data).
    // Every section starts at the same Y across a row; text keeps CardTextPad (16 px) from the card edge.
    // =====================================================================================================================
    public partial class HellordGame
    {
        // ---- game constants the cards print (were bare literals in the game code; same values, one source of truth)
        public const float StChillS1825 = 2f, StFreezeS1825 = 1.2f, StShockS1825 = 3f, StBleedS1825 = 3f, StBleedDps1825 = 0.25f, StCurseS1825 = 4f;
        public const float EnemyChillMul1825 = 0.65f, StatPickupAdd1825 = 0.15f;
        public const int StFreezeHits1825 = 3, ZhHydraHeads1825 = 3, LzClapBeat1825 = 3, NkNovaHarvest1825 = 4, GgAoPillars1825 = 4, NkLegionMul1825 = 2;

        const int CardRuleW = 40, CardRuleGap = 6, CardTagMax = 2, CardFactMax = 5;
        static readonly Color CGreen = new Color32(0x63, 0xc7, 0x4d, 255), CRule = new Color32(0x5a, 0x69, 0x88, 255);

        public struct CSeg { public string t; public Color c; }
        sealed class CardFaceA
        {
            public List<string> title = new List<string>();
            public readonly List<List<CSeg>> tags = new List<List<CSeg>>(), stats = new List<List<CSeg>>(), facts = new List<List<CSeg>>(), to = new List<List<CSeg>>();
            public bool showNew, hasDots = true, tagOverflow; public int dots, rar; public string name = "";
            public readonly List<string> errs = new List<string>();
            public int Words() { int n = 0; foreach (var b in new[] { stats, facts }) foreach (var l in b) { var s = LineStr(l).Trim(); if (s.Length > 0) n += s.Split(' ').Length; } return n; }
        }
        struct RowA { public int plateH, tagY, iconY, statY, statH, ruleY, factY, factH, toRuleY, toY, toH, dotsY, cardH, nTags; public bool rule, toRule; }

        static string LineStr(List<CSeg> l) { var sb = new StringBuilder(); foreach (var s in l) sb.Append(s.t); return sb.ToString(); }
        static int LineW(List<CSeg> l) { int n = 0; foreach (var s in l) n += s.t.Length; return n * 6 - 1; }

        // ---------------------------------------------------------------- copy -> coloured segments
        static bool IsStatusAt(string[] w, int i, out int span, out string kw)
        {
            foreach (var k in CardCopyA1825.StatusWords)
            {
                var parts = k.Split(' ');
                if (i + parts.Length > w.Length) continue;
                bool ok = true;
                for (int q = 0; q < parts.Length && ok; q++) ok = Core(w[i + q]) == parts[q];
                if (ok) { span = parts.Length; kw = k; return true; }
            }
            span = 1; kw = null; return false;
        }
        static string Core(string w) { int e = w.Length; while (e > 0 && (w[e - 1] == ',' || w[e - 1] == '.' || w[e - 1] == ';' || w[e - 1] == ':')) e--; return w.Substring(0, e); }
        /// <summary>Split a number-led word into gold number / dim unit S / rest. "1.4S" -> 1.4 + S, "X2.0," -> X2.0 + ",".</summary>
        static int NumLen(string w, out int unitLen)
        {
            unitLen = 0; int i = 0;
            if (i < w.Length && (w[i] == '+' || w[i] == '-')) i++;
            if (i < w.Length && w[i] == 'X' && i + 1 < w.Length && char.IsDigit(w[i + 1])) i++;
            if (i >= w.Length || !char.IsDigit(w[i])) return 0;
            while (i < w.Length && (char.IsDigit(w[i]) || w[i] == '%' || ((w[i] == '.' || w[i] == '-' || w[i] == '/') && i + 1 < w.Length && char.IsDigit(w[i + 1])))) i++;
            if (i < w.Length && (w[i] == 'X' || w[i] == '+')) i++;
            if (i < w.Length && w[i] == 'S' && (i + 1 == w.Length || !char.IsLetter(w[i + 1]))) unitLen = 1;
            else if (i + 1 < w.Length && w[i] == '/' && w[i + 1] == 'S' && (i + 2 == w.Length || !char.IsLetter(w[i + 2]))) unitLen = 2;
            return i;
        }
        static void Add(List<CSeg> l, string t, Color c)
        {
            if (t.Length == 0) return;
            if (l.Count > 0 && l[l.Count - 1].c == c) { var s = l[l.Count - 1]; s.t += t; l[l.Count - 1] = s; } else l.Add(new CSeg { t = t, c = c });
        }
        /// <summary>kind: S stat, F/U fact, D/X drawback (all red), H header (white), T target (cyan), N dim.</summary>
        static List<CSeg> Colorize(string text, char kind)
        {
            var l = new List<CSeg>(); text = Sanitize(text ?? "");
            bool red = kind == 'D' || kind == 'X';
            Color baseC = kind == 'S' ? CDim : kind == 'T' ? HCyan : kind == 'N' ? CDim : Color.white;
            var w = text.Split(' ');
            int label = -1; if (kind == 'F' || kind == 'U') for (int i = 0; i < w.Length; i++) if (w[i].EndsWith(":")) label = i;
            for (int i = 0; i < w.Length; )
            {
                if (i > 0) Add(l, " ", red ? CRed : baseC);
                if (red) { Add(l, w[i], CRed); i++; continue; }
                if (kind != 'T' && IsStatusAt(w, i, out int span, out string kw))
                {
                    var kc = CardCopyA1825.StatusCol(kw); string joined = string.Join(" ", w, i, span), core = Core(joined);
                    Add(l, core, kc); Add(l, joined.Substring(core.Length), i + span - 1 <= label ? CDim : baseC); i += span; continue;
                }
                string word = w[i]; Color wc = i <= label ? CDim : baseC;
                int ul = 0, nl = kind == 'T' ? 0 : NumLen(word, out ul);
                if (nl > 0) { Add(l, word.Substring(0, nl), CGold); if (ul > 0) Add(l, word.Substring(nl, ul), CDim); Add(l, word.Substring(nl + ul), wc); }
                else Add(l, word, wc);
                i++;
            }
            return l;
        }

        // ---------------------------------------------------------------- tag line
        static string TagName1825(STag t) => t == STag.Area ? "AOE" : t.ToString().ToUpperInvariant();
        /// <summary>Comma tag line, element tags in their colour, mechanics dim, packed in at most 2 lines of CardTextW.
        /// Canonical order when it fits greedily, else the first order-keeping 2-line split.</summary>
        static List<List<CSeg>> TagLines1825(STag tags, out bool overflow)
        {
            var names = new List<STag>();
            foreach (var t in HData.TagOrder1825) if ((tags & t) != 0 && (t & CardCopyA1825.HiddenTags) == 0) names.Add(t);
            if (names.Count == 0) foreach (var t in HData.TagOrder1825) if ((tags & t) != 0) names.Add(t); // only hidden tags (a plain movement): show them
            int maxC = (CardTextW + 1) / 6;
            int L(List<STag> ts) { int n = 0; for (int i = 0; i < ts.Count; i++) n += TagName1825(ts[i]).Length + (i > 0 ? 2 : 0); return n; }
            var lines = new List<List<STag>>(); var cur = new List<STag>();
            for (int k = 0; k < names.Count; k++)
            {
                var tr = new List<STag>(cur) { names[k] }; int trW = L(tr) + (k < names.Count - 1 ? 1 : 0); // a non-final tag carries its comma
                if (cur.Count > 0 && trW > maxC) { lines.Add(cur); cur = new List<STag> { names[k] }; } else cur = tr;
            }
            if (cur.Count > 0) lines.Add(cur);
            if (lines.Count > CardTagMax)
            {
                int n = names.Count;
                for (int mask = 1; mask < (1 << n) - 1; mask++)
                {
                    var a = new List<STag>(); var b = new List<STag>();
                    for (int i = 0; i < n; i++) (((mask >> i) & 1) != 0 ? a : b).Add(names[i]);
                    if (L(a) + 1 <= maxC && L(b) <= maxC) { lines = new List<List<STag>> { a, b }; break; }
                }
            }
            overflow = lines.Count > CardTagMax;
            var o = new List<List<CSeg>>();
            for (int li = 0; li < lines.Count; li++)
            {
                var seg = new List<CSeg>();
                for (int i = 0; i < lines[li].Count; i++)
                {
                    var t = lines[li][i]; Color c = CardCopyA1825.TagCol(t, out var ec) ? ec : CDim;
                    if (i > 0) Add(seg, " ", CDim);
                    Add(seg, TagName1825(t), c);
                    bool last = li == lines.Count - 1 && i == lines[li].Count - 1; if (!last) Add(seg, ",", CDim);
                }
                o.Add(seg);
            }
            return o;
        }

        // ---------------------------------------------------------------- building faces
        CardCtx1825 CtxFor1825(SkillInst inst, SkillDef def, int L, int A)
        {
            var c = new CardCtx1825 { L = L, A = A, maxlv = HData.MaxLv(def), summon = HData.Summon561(def) };
            var si = inst ?? new SkillInst { def = def, level = Mathf.Max(1, L) };
            c.cdm = si.CdMul; c.dm = si.DurMul; c.cd = def.cooldown * c.cdm;
            c.lvm = HData.LevelMul(Mathf.Max(1, L));
            c.lvdmg = A > 0 ? c.lvm / Mathf.Max(1e-4f, HData.LevelMul(A)) - 1f : 0f;
            return c;
        }
        static void AddLines(CardFaceA f, List<List<CSeg>> into, List<CardEval1825.Line> ls) { foreach (var l in ls) into.Add(Colorize(l.text, l.kind)); }
        void SkillBody1825(CardFaceA f, SkillDef def, SkillInst inst, int L, int A, bool levelCard, string header)
        {
            var ctx = CtxFor1825(inst, def, L, A);
            CardCopyA1825.Skill.TryGetValue(def.id, out var src);
            if (src == null) f.errs.Add("no copy for skill " + def.id);
            if (header != null) f.stats.Add(Colorize(header, 'H'));
            if (levelCard) AddLines(f, f.stats, CardEval1825.Lines(CardCopyA1825.LevelHead, "S", ctx, f.errs));
            AddLines(f, f.stats, CardEval1825.Lines(src, "SD", ctx, f.errs));
            if (levelCard)
            {
                AddLines(f, f.stats, CardEval1825.Lines(CardCopyA1825.LevelTail, "S", ctx, f.errs));
                if (inst != null && inst.sup.Count > 0) f.stats.Add(Colorize($"SUPPORTS {inst.sup.Count}/{Mathf.Min(inst.level, HData.MaxSupports)}", 'S'));
                AddLines(f, f.facts, CardEval1825.Lines(src, "UX", ctx, f.errs));
                AddLines(f, f.facts, CardEval1825.Lines(CardCopyA1825.LevelTail, "F", ctx, f.errs));
            }
            else AddLines(f, f.facts, CardEval1825.Lines(src, "FUX", ctx, f.errs));
            f.tags.AddRange(TagLines1825(def.tags, out f.tagOverflow));
            f.dots = L;
        }
        /// <summary>Support face. tgtIx = SupInst1651 index of the TO skill (-1 none); pickN > 1 = several targets.</summary>
        void SupportBody1825(CardFaceA f, SupportDef sd, int L, int cur, SkillInst tgt, int pickN, bool anyFit, string header, bool withTo)
        {
            var ctx = new CardCtx1825 { L = L, A = cur, tgt = tgt != null ? (int)tgt.def.id : -1, multi = tgt == null || HData.MultiHonoured1651(tgt.def.id) };
            CardCopyA1825.Support.TryGetValue(sd.id, out var src); if (src == null) f.errs.Add("no copy for support " + sd.id);
            if (header != null) f.stats.Add(Colorize(header, 'H'));
            AddLines(f, f.stats, CardEval1825.Lines(src, "SD", ctx, f.errs));
            if (sd.excludes != 0) f.facts.Add(Colorize(ExcludeLine1825(sd.excludes), 'F'));
            AddLines(f, f.facts, CardEval1825.Lines(src, "FUX", ctx, f.errs));
            // tag block: SUPPORT / NEEDS: <tag> (green when the TO skill has it, red when not)
            bool fit = tgt != null ? HData.Fits(sd, tgt.def) : anyFit;
            var l1 = new List<CSeg>(); Add(l1, "SUPPORT", CDim); f.tags.Add(l1);
            var l2 = new List<CSeg>(); Add(l2, "NEEDS: ", CDim); Add(l2, sd.needs == STag.Any ? "ANY" : HData.TagText(sd.needs, " / ", true), fit ? CGreen : CRed); f.tags.Add(l2);
            if (withTo)
            {
                f.to.Add(Colorize("TO:", 'N'));
                if (pickN > 1) f.to.Add(Colorize($"PICK 1 OF {pickN}", 'T'));
                else if (tgt != null) foreach (var w in WrapCard(tgt.def.name)) f.to.Add(Colorize(w, 'T'));
                else f.to.Add(Colorize(anyFit ? "NO OPEN SLOT" : "NO MATCHING SKILL", 'X'));
            }
            f.dots = L;
        }
        static string ExcludeLine1825(STag ex)
        {
            const STag kit = STag.Movement | STag.Special | STag.Ultimate; var parts = new List<string>();
            foreach (var t in HData.TagOrder1825) if ((ex & t) != 0 && (t & kit) == 0) parts.Add(TagName1825(t));
            if ((ex & kit) == kit) parts.Add("KIT"); else foreach (var t in HData.TagOrder1825) if ((ex & t & kit) != 0) parts.Add(TagName1825(t));
            return "NOT " + string.Join(" OR ", parts);
        }
        CardFaceA BuildFace1825(Card c)
        {
            var f = new CardFaceA(); f.name = CardNameOf(c); f.title = WrapCard(f.name);
            f.rar = c.kind == CardKind.Stat && c.stat.id == StatId.Slot ? 3 : c.kind == CardKind.NewSkill ? 2 : c.kind == CardKind.Support ? 1 : (c.kind == CardKind.SkillLevel && skills[c.skillIndex].level + 1 >= HData.MaxSkillLevel) ? 3 : 0;
            if (c.kit > 0)
            {
                var k = KitK1666(c.kit - 1, c.kitPick); var ki = c.kitLv1651 ? ZkSlotInst1651(c.kit - 1) : ZkI1651(k); var def = ki != null ? ki.def : ZkI1651(k).def;
                if (c.kitLv1651 && ki != null) SkillBody1825(f, def, ki, ki.level + 1, ki.level, true, null);
                else { SkillBody1825(f, def, null, 1, 0, false, ZhKitType1639[c.kit]); f.showNew = true; }
                return f;
            }
            switch (c.kind)
            {
                case CardKind.NewSkill: SkillBody1825(f, c.skill, null, 1, 0, false, null); f.showNew = true; break;
                case CardKind.SkillLevel: { var s = skills[c.skillIndex]; SkillBody1825(f, s.def, s, s.level + 1, s.level, true, null); break; }
                case CardKind.Stat:
                {
                    CardCopyA1825.Stat.TryGetValue(c.stat.id, out var src); if (src == null) f.errs.Add("no copy for stat " + c.stat.id);
                    var ctx = new CardCtx1825(); AddLines(f, f.stats, CardEval1825.Lines(src, "SD", ctx, f.errs)); AddLines(f, f.facts, CardEval1825.Lines(src, "FUX", ctx, f.errs));
                    var l1 = new List<CSeg>(); Add(l1, "STAT", CDim); f.tags.Add(l1); f.hasDots = false; break;
                }
                case CardKind.Support:
                {
                    var tl = SupportTargets(c.support); int ti = tl.Count > 0 ? tl[0] : -1; var tgt = tl.Count == 1 ? SupInst1651(ti) : null; int cur = tgt != null ? tgt.Lvl(c.support.id) : 0;
                    bool anyUp = false; foreach (int q in tl) { var sq = SupInst1651(q); if (sq != null && sq.Has(c.support.id)) anyUp = true; }
                    bool anyFit = false; foreach (var sk in SupPool1651()) if (HData.Fits(c.support, sk.def)) anyFit = true;
                    string header = tl.Count > 1 ? (anyUp ? "UPGRADE OR NEW" : null) : cur > 0 ? $"UPGRADE LV {cur} - {cur + 1}" : null;
                    SupportBody1825(f, c.support, cur + 1, cur, tgt, tl.Count, anyFit, header, true);
                    f.showNew = cur == 0 && !anyUp; break;
                }
            }
            return f;
        }

        // ---------------------------------------------------------------- row layout (one source for drawing and the audit)
        static int THA(int n) => n <= 0 ? 0 : (n - 1) * CardLine + CardGlyph;
        RowA LayoutRow1825(IList<CardFaceA> faces)
        {
            int mT = 1, mTag = 0, mS = 0, mF = 0, mTo = 0;
            foreach (var f in faces) { mT = Mathf.Max(mT, f.title.Count); mTag = Mathf.Max(mTag, f.tags.Count); mS = Mathf.Max(mS, f.stats.Count); mF = Mathf.Max(mF, f.facts.Count); mTo = Mathf.Max(mTo, f.to.Count); }
            var r = new RowA(); r.nTags = mTag; r.plateH = CardPlateH(mT); int y = r.plateH + CardPad;
            r.tagY = y; if (mTag > 0) y += THA(mTag) + CardPad;
            r.iconY = y; y += CardIcon + CardPad;
            r.statY = y; r.statH = THA(mS); y += r.statH;
            if (mF > 0) { if (mS > 0) { r.rule = true; r.ruleY = y + CardRuleGap; y = r.ruleY + 1 + CardRuleGap; } r.factY = y; r.factH = THA(mF); y += r.factH; }
            else r.factY = y;
            if (mTo > 0) { r.toRule = true; r.toRuleY = y + CardRuleGap; y = r.toRuleY + 1 + CardRuleGap; r.toY = y; r.toH = THA(mTo); y += r.toH; }
            y += CardPad; r.dotsY = y; y += CardDot + CardPad; r.cardH = y;
            return r;
        }

        // ---------------------------------------------------------------- drawing
        sealed class AuditRec { public int x, y, cardH; public readonly List<string> rows = new List<string>(); public readonly Dictionary<string, int> secY = new Dictionary<string, int>(); public readonly List<int[]> lines = new List<int[]>(); public int[] icon, dots, rule, toRule; }
        List<AuditRec> _auditA1825; // non-null while the audit records one repaint
        void DrawSegLine(List<CSeg> l, int cardX, int y, AuditRec ar, string sec)
        {
            int w = LineW(l), x = BoxCenterX(cardX + CardTextPad, CardTextW, w), cx = x;
            foreach (var s in l) { Text(s.t, cx, y, s.c); cx += s.t.Length * 6; }
            if (ar != null) { ar.lines.Add(new[] { x, y, w, cardX }); ar.rows.Add(sec + " y=" + y + " x=" + x + " w=" + w + " L=" + (x - cardX) + " R=" + (cardX + CardW - x - w) + " '" + LineStr(l) + "'"); if (!ar.secY.ContainsKey(sec)) ar.secY[sec] = y; }
        }
        void DrawRule1825(int cardX, int y, AuditRec ar, bool to)
        {
            int rx = BoxCenterX(cardX, CardW, CardRuleW); GUI.color = new Color(CRule.r, CRule.g, CRule.b, _guiA); GUI.DrawTexture(new Rect(rx, y, CardRuleW, 1), Texture2D.whiteTexture); GUI.color = new Color(1, 1, 1, _guiA);
            if (ar != null) { if (to) ar.toRule = new[] { rx, y }; else ar.rule = new[] { rx, y }; }
        }
        /// <summary>Draw one A card at (x, y) with the shared row layout. drawIcon draws the 48 px icon at (ix, iy).</summary>
        void DrawCardA1825(CardFaceA f, RowA r, int x, int y, int artX, int index, float ra, System.Action<int, int> drawIcon)
        {
            AuditRec ar = _auditA1825 != null && Event.current != null && Event.current.type == EventType.Repaint ? new AuditRec { x = x, y = y, cardH = r.cardH } : null; if (ar != null) _auditA1825.Add(ar);
            DrawCardChrome(artX, x, y, CardW, r.cardH, r.plateH);
            DrawCardTitle(x, y, r.plateH, f.title);
            for (int i = 0; i < f.tags.Count; i++) DrawSegLine(f.tags[i], x, y + r.tagY + i * CardLine, ar, "tags");
            int ix = BoxCenterX(x, CardW, CardIcon), iy = y + r.iconY;
            StampCardWell(0, ix, iy); GUI.color = new Color(1, 1, 1, ra); drawIcon?.Invoke(ix, iy); GUI.color = new Color(1, 1, 1, ra);
            if (f.showNew && _tNew) Blit(_tNew, 0, 0, CardBadgeW, CardBadgeH, x + CardW - CardPad - CardBadgeW, iy);
            if (ar != null) ar.icon = new[] { ix, iy };
            for (int i = 0; i < f.stats.Count; i++) DrawSegLine(f.stats[i], x, y + r.statY + i * CardLine, ar, "stats");
            if (r.rule && f.facts.Count > 0 && f.stats.Count > 0) DrawRule1825(x, y + r.ruleY, ar, false);
            for (int i = 0; i < f.facts.Count; i++) DrawSegLine(f.facts[i], x, y + r.factY + i * CardLine, ar, "facts");
            if (r.toRule && f.to.Count > 0) DrawRule1825(x, y + r.toRuleY, ar, true);
            for (int i = 0; i < f.to.Count; i++) DrawSegLine(f.to[i], x, y + r.toY + i * CardLine, ar, "to");
            if (f.hasDots) { DrawCardDots(x, y, r.cardH, f.dots, f.rar); if (ar != null) ar.dots = new[] { BoxCenterX(x, CardW, CardDotsW), y + r.cardH - CardPad - CardDot }; }
            DrawCardIndex(x, y, r.cardH, index);
        }
        void DrawSkillIcon1825(SkillId id, int ix, int iy)
        {
            SkillBarTex();
            if (IceIconBlit(id, ix, iy, CardIcon)) { }
            else if (ZhCardIcon1625(id) != null) GUI.DrawTexture(new Rect(ix, iy, CardIcon, CardIcon), ZhCardIcon1625(id));
            else if (_icons44 && id >= SkillId.ChainLightning && id <= SkillId.PhantomAxes && _tSkill24 != null) BlitInset(_tSkill24, (int)id * 28, 0, 24, 24, ix, iy, CardIcon, CardIcon);
            else { var pt = PhTint(id); var a = GUI.color.a; GUI.color = new Color(pt.r, pt.g, pt.b, a); BlitInset(_tSkillIcons, SkillIcon(id) * 16, 0, 16, 16, ix, iy, CardIcon, CardIcon); GUI.color = new Color(1, 1, 1, a); }
        }

        /// <summary>t1825: DrawCardsB body (level-up / start pick / chest) in the A layout.</summary>
        void DrawCardsA1825(string title, string sub, bool levelUp)
        {
            FloorTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.69f * (levelUp ? FX26Dim : 1f)));
            _guiA = levelUp ? FX26Reveal(-1) : 1f; TextC(title, CardScreenTitleY, HCyan, 2); TextC(sub, CardScreenSubY, Color.white); _guiA = 1f;
            int n = cards.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; Card picked = null;
            var faces = new List<CardFaceA>(n); for (int i = 0; i < n; i++) faces.Add(BuildFace1825(cards[i]));
            var r = LayoutRow1825(faces);
            for (int i = 0; i < n; i++)
            {
                var c = cards[i]; int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, r.cardH);
                float ra = levelUp ? FX26Reveal(i) : 1f; if (ra <= 0.001f) continue; _guiA = ra; GUI.color = new Color(1, 1, 1, ra);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0) + (levelUp ? FX26Slide(i) : 0);
                int artX = (faces[i].rar * 2 + (hover ? 1 : 0)) * CardArtW;
                DrawCardA1825(faces[i], r, x, y, artX, i, ra, (ix, iy) => DrawCardFace(c, x, y, 0, ra, false, iy));
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
        int _lastFootY1825, _lastCardH1825;

        /// <summary>t1825: WHICH SKILL? picker in the A layout (one card per target skill).</summary>
        List<CardFaceA> PickFaces1825()
        {
            var tp = _tp; var faces = new List<CardFaceA>();
            if (tp == null) return faces;
            foreach (int ti in tp.targets)
            {
                var sk = SupInst1651(ti); var f = new CardFaceA(); faces.Add(f);
                if (sk == null) continue;
                f.name = sk.def.name; f.title = WrapCard(sk.def.name); f.rar = 1;
                int cur = sk.Lvl(tp.sd.id), L = tp.swap ? 1 : cur + 1;
                var ctx = new CardCtx1825 { L = L, A = cur, tgt = (int)sk.def.id, multi = HData.MultiHonoured1651(sk.def.id) };
                CardCopyA1825.Support.TryGetValue(tp.sd.id, out var src); if (src == null) f.errs.Add("no copy for support " + tp.sd.id);
                if (tp.swap && sk.sup.Count > 0) { f.stats.Add(Colorize("SWAP OUT:", 'H')); f.stats.Add(Colorize(HData.Support(sk.sup[sk.sup.Count - 1]).name, 'D')); }
                else f.stats.Add(Colorize(cur > 0 ? $"UPGRADE LV {cur} - {cur + 1}" : "NEW LV 1", 'H'));
                AddLines(f, f.stats, CardEval1825.Lines(src, "SD", ctx, f.errs));
                var nd = new List<CSeg>(); Add(nd, "NEEDS: ", CDim); Add(nd, tp.sd.needs == STag.Any ? "ANY" : HData.TagText(tp.sd.needs, " / ", true), HData.Fits(tp.sd, sk.def) ? CGreen : CRed); f.facts.Add(nd);
                f.facts.Add(Colorize($"SKILL LV {sk.level}", 'F'));
                f.facts.Add(Colorize($"SLOTS {sk.sup.Count}/{Mathf.Min(sk.level, HData.MaxSupports)}", 'F'));
                f.tags.AddRange(TagLines1825(sk.def.tags, out f.tagOverflow));
                f.dots = L;
            }
            return faces;
        }
        void DrawTargetPickA1825()
        {
            var tp = _tp; if (tp == null) return; FloorTex(); SkillBarTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.8f));
            TextC("WHICH SKILL?", CardScreenTitleY, HCyan, 2); TextC(Sanitize(tp.sd.name) + (tp.swap ? " - SWAP" : ""), CardScreenSubY, Color.white);
            int n = tp.targets.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; int picked = -1;
            var faces = PickFaces1825(); var r = LayoutRow1825(faces);
            for (int i = 0; i < n; i++)
            {
                var sk = SupInst1651(tp.targets[i]); if (sk == null) continue;
                int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, r.cardH);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0);
                var id = sk.def.id;
                DrawCardA1825(faces[i], r, x, y, (1 * 2 + (hover ? 1 : 0)) * CardArtW, i, 1f, (ix, iy) => DrawSkillIcon1825(id, ix, iy));
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover) { picked = i; ev.Use(); }
            }
            int footY = rowY + r.cardH + CardPad; _lastFootY1825 = footY; _lastCardH1825 = r.cardH;
            TextC($"{CardKeyRange1831(n)} PICK THE SKILL - ESC BACK", footY, CDim);
            if (picked >= 0) ConfirmTarget(picked);
        }
    }
}
