using System.Collections.Generic;
using UnityEngine;

// v0.8: level-up / start / chest card screens, death and floor-cleared screens on the 640x360 canvas (exported levelup_* + pause_box art, 5x7 font).
namespace Hellord
{
    public partial class HellordGame
    {
        int _sndHover = -1; // v0.0.34 card hover sound
        static readonly Color CDim = new Color32(0x8b, 0x9b, 0xb4, 255), CBody = new Color32(0xc0, 0xcb, 0xdc, 255), CRed = new Color32(0xe4, 0x3b, 0x44, 255), CGold = new Color32(0xfe, 0xe7, 0x61, 255);
        enum CardTone : byte { Body, Header, Stat, Need, Target }
        struct CardRun { public string text; public CardTone tone; public bool sectionStart; }
        sealed class CardCopy
        {
            public readonly List<CardRun> runs = new List<CardRun>();
            public int LineN => runs.Count;
            public void AddSection(CardTone tone, List<string> lines)
            {
                if (lines == null) return;
                bool first = true;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (string.IsNullOrEmpty(lines[i])) continue;
                    runs.Add(new CardRun { text = lines[i], tone = tone, sectionStart = first && runs.Count > 0 });
                    first = false;
                }
            }
            public void AddSection(CardTone tone, string line)
            {
                if (string.IsNullOrEmpty(line)) return;
                runs.Add(new CardRun { text = line, tone = tone, sectionStart = runs.Count > 0 });
            }
            public void AddMixed(bool newSection, CardTone tone, List<string> lines)
            {
                if (lines == null) return;
                bool first = true;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (string.IsNullOrEmpty(lines[i])) continue;
                    runs.Add(new CardRun { text = lines[i], tone = tone, sectionStart = newSection && first && runs.Count > 0 });
                    first = false; newSection = false;
                }
            }
            public int BodyH()
            {
                if (runs.Count == 0) return 0;
                int h = CardGlyph;
                for (int i = 1; i < runs.Count; i++) h += CardLine + (runs[i].sectionStart ? CardSection : 0);
                return h;
            }
        }
        // t1823: one layout source for every DrawCardsB / kit / support / picker card.
        // CardW is even so 1-4 card rows on the 640 canvas have equal left/right margins (margin = gap = 8).
        // CardTextPad is ink-to-outer-frame clearance (4 px chrome + 12 px air inside the well).
        // 5x7 lines have odd ink width (6n-1); BoxCenterX floors the extra pixel to the right so a line is never 1 px off another.
        const int CardW = 150, CardPad = 8, CardGap = 8, CardTextPad = 16, CardSection = 6, CardLine = 10, CardGlyph = 7, CardIcon = 48;
        const int CardPlate = 18, CardHover = 4, CardDot = 7, CardDotGap = 2, CardDotsN = 5;
        const int CardArtW = 116, CardArtH = 164, CardSliceL = 4, CardSliceR = 4, CardSliceT = 18, CardSliceB = 4;
        const int CardWell = 42, CardWellX = 37, CardWellY = 22;
        const int CardBadgeW = 25, CardBadgeH = 9;
        const int CardScreenTitleY = 40, CardScreenSubY = 62;
        static int CardInnerW => CardW - 2 * CardPad;
        static int CardTextW => CardW - 2 * CardTextPad;
        static int CardDotsW => CardDotsN * CardDot + (CardDotsN - 1) * CardDotGap;
        static int CardTextH(int lines) => lines <= 0 ? 0 : (lines - 1) * CardLine + CardGlyph;
        static int CardPlateH(int titleLines) => CardPlate + (Mathf.Max(1, titleLines) - 1) * CardLine;
        static int CardHeight(int titleLines, int bodyH) => CardPlateH(titleLines) + CardPad + CardIcon + CardPad + bodyH + CardPad + CardDot + CardPad;
        static int CardRowWidth(int n) => n <= 0 ? 0 : n * CardW + (n - 1) * CardGap;
        static int CardRowX0(int n) => (HW - CardRowWidth(n)) / 2;
        static int CardRowY() => CardScreenSubY + CardGlyph + CardPad;
        static int BoxCenterX(int boxX, int boxW, int contentW) => boxX + (boxW - contentW) / 2;
        static int CardLineX(int cardX, string s) => BoxCenterX(cardX + CardTextPad, CardTextW, TextW(s));
        static Color CardToneCol(CardTone t)
        {
            switch (t)
            {
                case CardTone.Header: return Color.white;
                case CardTone.Stat: return CGold;
                case CardTone.Need: return CDim;
                case CardTone.Target: return HCyan;
                default: return CBody;
            }
        }

        static int SkillIcon(SkillId id) { switch (id) { case SkillId.Fireball: return 0; case SkillId.FrostCleave: return 11; case SkillId.StormRing: return 10; case SkillId.BoneOrbit: return 4; case SkillId.ChainLightning: return 10; case SkillId.HellfirePillar: return 0; case SkillId.BloodWhip: return 4; case SkillId.SoulSpears: return 11; case SkillId.MagmaThrow: case SkillId.RollingMagmaRock: case SkillId.FirePillar: return 0; default: return 8; } } // v0.0.36: new skills reuse a base icon (tinted + tagged, see PhTint/PhTag)
        /// <summary>v0.0.9: distinct existing skill-strip icons for some supports/stats (Burn no longer shares the Shock glyph,
        /// Swiftness no longer shares Hide's). -1 = no art yet (room-icon stand-in; missing art).</summary>
        static int SupportIcon(SupportId id) { switch (id) { case SupportId.Burn: return 5; case SupportId.Bleed: return 2; case SupportId.MultipleProjectiles: return 1; default: return -1; } }
        static int StatIcon(StatId id) { switch (id) { case StatId.Armor: return 7; case StatId.Speed: return 9; default: return -1; } }
        /// <summary>Blit with UVs pulled in 1/8 texel so point-sampled upscales never pick up the neighbouring frame (stray marks on cards).</summary>
        static void BlitInset(Texture2D t, float sx, float sy, float sw, float sh, float x, float y, float dw, float dh)
        {
            if (!t) return; const float e = 0.125f;
            GUI.DrawTextureWithTexCoords(new Rect(Mathf.Round(x), Mathf.Round(y), dw, dh), t, new Rect((sx + e) / t.width, 1 - (sy + sh - e) / t.height, (sw - 2 * e) / t.width, (sh - 2 * e) / t.height));
        }
        void DimScreen(Color c)
        {
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; GUI.color = c; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; GUI.matrix = m;
        }
        void TextC(string s, float y, Color c, int sc = 1) { s = Sanitize(s); Text(s, BoxCenterX(0, HW, TextW(s) * sc), y, c, true, sc); }
        static List<string> Wrap(string s, int max)
        {
            var lines = new List<string>(); string cur = "";
            foreach (var w0 in Sanitize(s).Split(' '))
            {
                if (w0.Length == 0) continue; string w = w0.Length > max ? w0.Substring(0, max) : w0;
                if (cur.Length == 0) cur = w; else if (cur.Length + 1 + w.Length <= max) cur += " " + w; else { lines.Add(cur); cur = w; }
            }
            if (cur.Length > 0) lines.Add(cur); return lines;
        }
        static bool CardIsValue(string t)
        {
            if (string.IsNullOrEmpty(t)) return false;
            if (t.StartsWith("LV") && t.Length > 2 && char.IsDigit(t[2])) return true;
            int i = 0; if (t[0] == '+' || t[0] == '-') i = 1; if (i >= t.Length) return false;
            if (t[i] == 'X' || t[i] == 'R') { i++; if (i >= t.Length) return false; }
            if (char.IsDigit(t[i])) return true;
            return t.IndexOf('%') >= 0;
        }
        static bool CardIsUnit(string t)
        {
            if (string.IsNullOrEmpty(t)) return false;
            string u = t; if (u[u.Length - 1] == '.' || u[u.Length - 1] == ',') u = u.Substring(0, u.Length - 1);
            return u == "HP" || u == "DMG" || u == "HIT" || u == "S" || u == "SEC";
        }
        static bool CardClauseEnd(string t)
        {
            if (string.IsNullOrEmpty(t)) return false;
            char c = t[t.Length - 1];
            if (c != '.' && c != ':' && c != ';') return false;
            // "X1.3." / "R1.4." — trailing separator after a dotted value is still a clause end
            return true;
        }
        static List<string> CardTokens(string s)
        {
            var raw = new List<string>();
            foreach (var w0 in Sanitize(s).Split(' ')) if (w0.Length > 0) raw.Add(w0);
            var toks = new List<string>();
            for (int i = 0; i < raw.Count; )
            {
                string t = raw[i];
                if (CardIsValue(t))
                {
                    int j = i + 1;
                    while (j < raw.Count && CardIsUnit(raw[j])) { t = t + " " + raw[j]; j++; }
                    toks.Add(t); i = j;
                }
                else { toks.Add(t); i++; }
            }
            return toks;
        }
        static int CardTokW(List<string> toks, int a, int b)
        {
            if (b <= a) return 0;
            int n = 0; for (int i = a; i < b; i++) n += toks[i].Length + (i > a ? 1 : 0);
            return n * 6 - 1;
        }
        static List<string> WrapBalanced(List<string> toks, int maxW)
        {
            var lines = new List<string>();
            if (toks == null || toks.Count == 0) return lines;
            int n = toks.Count;
            // hard-split tokens that themselves overflow
            var flat = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string w = toks[i];
                while (TextW(w) > maxW)
                {
                    int k = 1; while (k < w.Length && TextW(w.Substring(0, k + 1)) <= maxW) k++;
                    flat.Add(w.Substring(0, k)); w = w.Substring(k);
                }
                if (w.Length > 0) flat.Add(w);
            }
            toks = flat; n = toks.Count;
            const int Inf = 1000000000;
            var cost = new int[n + 1]; var back = new int[n + 1];
            for (int i = 1; i <= n; i++) { cost[i] = Inf; back[i] = -1; }
            cost[0] = 0;
            for (int i = 0; i < n; i++)
            {
                if (cost[i] >= Inf) continue;
                for (int j = i + 1; j <= n; j++)
                {
                    int w = CardTokW(toks, i, j); if (w > maxW) break;
                    int leftover = maxW - w;
                    int c = leftover * leftover;
                    if (CardClauseEnd(toks[j - 1])) c = c * 3 / 4; // prefer wrapping after a clause
                    if (leftover > maxW / 2) c += 400;
                    if (cost[i] + c < cost[j]) { cost[j] = cost[i] + c; back[j] = i; }
                }
            }
            if (back[n] < 0)
            {
                string cur = "";
                for (int i = 0; i < n; i++)
                {
                    string trial = cur.Length == 0 ? toks[i] : cur + " " + toks[i];
                    if (TextW(trial) <= maxW) cur = trial; else { if (cur.Length > 0) lines.Add(cur); cur = toks[i]; }
                }
                if (cur.Length > 0) lines.Add(cur);
                return lines;
            }
            var acc = new List<string>();
            for (int j = n; j > 0; j = back[j])
            {
                int i = back[j]; var sb = new System.Text.StringBuilder();
                for (int k = i; k < j; k++) { if (k > i) sb.Append(' '); sb.Append(toks[k]); }
                acc.Add(sb.ToString());
            }
            acc.Reverse();
            return acc;
        }
        static List<string> WrapCard(string s)
        {
            s = Sanitize(s ?? "");
            var lines = new List<string>();
            if (s.Length == 0) return lines;
            int maxW = CardTextW;
            // Keep a "TO: NAME" (or "MAX: …") label with its value when the name fits on one line by itself.
            int colon = s.IndexOf(':');
            if (colon >= 0 && colon <= 4)
            {
                string label = s.Substring(0, colon + 1), rest = s.Substring(colon + 1).Trim();
                if (rest.Length > 0 && TextW(s) > maxW && TextW(rest) <= maxW)
                {
                    lines.Add(label); lines.Add(rest); return lines;
                }
            }
            return WrapBalanced(CardTokens(s), maxW);
        }

        void DrawCardChrome(int artX, int x, int y, int w, int h, int plateH)
        {
            if (!_tCard) return;
            int sl = CardSliceL, sr = CardSliceR, sb = CardSliceB;
            int midS = CardArtW - sl - sr;
            int midD = w - sl - sr;
            int fillSrcH = CardSliceT - 5;
            int fillDstH = Mathf.Max(1, plateH - 5);
            Blit(_tCard, artX, 0, sl, 4, x, y);
            Blit(_tCard, artX + sl, 0, midS, 4, x + sl, y, midD, 4);
            Blit(_tCard, artX + CardArtW - sr, 0, sr, 4, x + w - sr, y);
            Blit(_tCard, artX, 4, sl, fillSrcH, x, y + 4, sl, fillDstH);
            Blit(_tCard, artX + sl, 4, midS, fillSrcH, x + sl, y + 4, midD, fillDstH);
            Blit(_tCard, artX + CardArtW - sr, 4, sr, fillSrcH, x + w - sr, y + 4, sr, fillDstH);
            Blit(_tCard, artX, 17, sl, 1, x, y + plateH - 1);
            Blit(_tCard, artX + sl, 17, midS, 1, x + sl, y + plateH - 1, midD, 1);
            Blit(_tCard, artX + CardArtW - sr, 17, sr, 1, x + w - sr, y + plateH - 1);
            int bodySrcY = CardSliceT, bodySrcH = CardArtH - CardSliceT - sb;
            int bodyDstY = y + plateH, bodyDstH = h - plateH - sb;
            if (bodyDstH > 0)
            {
                Blit(_tCard, artX, bodySrcY, sl, bodySrcH, x, bodyDstY, sl, bodyDstH);
                Blit(_tCard, artX + CardArtW - sr, bodySrcY, sr, bodySrcH, x + w - sr, bodyDstY, sr, bodyDstH);
                Blit(_tCard, artX + sl, 100, midS, 1, x + sl, bodyDstY, midD, bodyDstH);
            }
            int by = y + h - sb;
            Blit(_tCard, artX, CardArtH - sb, sl, sb, x, by);
            Blit(_tCard, artX + sl, CardArtH - sb, midS, sb, x + sl, by, midD, sb);
            Blit(_tCard, artX + CardArtW - sr, CardArtH - sb, sr, sb, x + w - sr, by);
        }
        void StampCardWell(int artX, int iconX, int iconY)
        {
            if (!_tCard) return;
            int wx = BoxCenterX(iconX, CardIcon, CardWell), wy = BoxCenterX(iconY, CardIcon, CardWell);
            Blit(_tCard, artX + CardWellX, CardWellY, CardWell, CardWell, wx, wy);
        }
        void DrawCardTitle(int cardX, int cardY, int plateH, List<string> title)
        {
            int fillTop = 4, fillH = plateH - 5, n = Mathf.Max(1, title.Count);
            int block = CardTextH(n); int ty = cardY + fillTop + (fillH - block) / 2;
            for (int i = 0; i < n; i++) Text(title[i], CardLineX(cardX, title[i]), ty + i * CardLine, Color.white);
        }
        void DrawCardDots(int cardX, int cardY, int cardH, int filled, int rar)
        {
            int dx = BoxCenterX(cardX, CardW, CardDotsW), dy = cardY + cardH - CardPad - CardDot;
            for (int k = 0; k < CardDotsN; k++) Blit(_tDots, (k < filled ? rar + 1 : 0) * CardDot, 0, CardDot, CardDot, dx + k * (CardDot + CardDotGap), dy);
        }
        void DrawCardIndex(int cardX, int cardY, int cardH, int i)
        {
            string s = (i + 1).ToString(); Text(s, cardX + CardPad, cardY + cardH - CardPad - CardGlyph, CDim);
        }

        void FillCardDesc(Card c, CardCopy copy)
        {
            if (c.kit > 0) { ZhKitCardBody1639(c, copy); return; }
            switch (c.kind)
            {
                case CardKind.NewSkill:
                    copy.AddSection(CardTone.Body, WrapCard(c.skill.desc));
                    copy.AddSection(CardTone.Need, WrapCard(HData.TagText(c.skill.tags, " ", true, true)));
                    break;
                case CardKind.SkillLevel:
                {
                    var s = skills[c.skillIndex]; int lv = s.level + 1;
                    copy.AddSection(CardTone.Header, $"LV {s.level} - {lv}");
                    string body = HData.Summon561(s.def) ? SummonLvText561(s.def, lv) : lv >= HData.MaxSkillLevel ? "MAX: widest reach, most projectiles" : $"More reach and coverage. support slot {Mathf.Min(lv, 4)} opens";
                    copy.AddSection(CardTone.Body, WrapCard(body));
                    break;
                }
                case CardKind.Stat:
                    copy.AddSection(CardTone.Stat, WrapCard(HData.StatDesc(c.stat)));
                    break;
                case CardKind.Support:
                {
                    var tl = SupportTargets(c.support); int ti = tl.Count > 0 ? tl[0] : -1; int cur = ti >= 0 && SupInst1651(ti) != null ? SupInst1651(ti).Lvl(c.support.id) : 0;
                    if (tl.Count > 1) { bool anyUp = false; foreach (int q in tl) { var sq = SupInst1651(q); if (sq != null && sq.Has(c.support.id)) anyUp = true; } cur = 0; copy.AddSection(CardTone.Header, anyUp ? "UPGRADE OR NEW" : "NEW LV 1"); }
                    else copy.AddSection(CardTone.Header, cur > 0 ? $"UPGRADE LV {cur} - {cur + 1}" : "NEW LV 1");
                    copy.AddSection(CardTone.Stat, WrapCard(HData.SupportEffect(c.support.id, cur + 1, tl.Count == 1 && SupInst1651(ti) != null ? (int)SupInst1651(ti).def.id : -1)));
                    copy.AddSection(CardTone.Body, WrapCard(HData.SupportCardDesc(c.support)));
                    copy.AddMixed(true, CardTone.Need, WrapCard("NEEDS " + HData.TagText(c.support.needs) + (c.support.excludes != 0 ? " NOT " + HData.TagText(c.support.excludes) : "")));
                    var tgt = tl.Count > 1 ? $"TO: PICK 1 OF {tl.Count}" : SupInst1651(ti)?.def != null ? "TO: " + Sanitize(SupInst1651(ti).def.name) : "NO OPEN SLOT";
                    copy.AddMixed(false, tgt.StartsWith("TO:") ? CardTone.Target : CardTone.Need, WrapCard(tgt));
                    break;
                }
            }
        }
        void DrawCardCopy(CardCopy copy, int cardX, int textY)
        {
            int y = textY;
            for (int i = 0; i < copy.runs.Count; i++)
            {
                var r = copy.runs[i];
                if (r.sectionStart) y += CardSection;
                Text(r.text, CardLineX(cardX, r.text), y, CardToneCol(r.tone));
                y += CardLine;
            }
        }
        string CardNameOf(Card c) => c.kit > 0 ? ZhKitName1639(c) : c.kind == CardKind.NewSkill || c.kind == CardKind.SkillLevel ? (c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.name : c.skill.name) : c.kind == CardKind.Support ? c.support.name : c.stat.name;
        void DrawCardFace(Card c, int x, int y, int plateH, float ra, bool showNew, int iyAt = int.MinValue)
        {
            int ix = BoxCenterX(x, CardW, CardIcon), iy = iyAt != int.MinValue ? iyAt : y + plateH + CardPad; // t1825: A layout passes the icon row Y
            StampCardWell(0, ix, iy);
            if (c.kit > 0)
            {
                var ic = KitIcon1666(ZhKitKey1639(c));
                if (ic != null) { GUI.color = new Color(1, 1, 1, ra); GUI.DrawTexture(new Rect(ix, iy, CardIcon, CardIcon), ic); GUI.color = new Color(1, 1, 1, ra); }
                else BlitInset(_tRoomIcons, 0, 0, 8, 8, ix, iy, CardIcon, CardIcon);
            }
            else
            {
                int icon = c.kind == CardKind.NewSkill ? SkillIcon(c.skill.id) : c.kind == CardKind.SkillLevel ? SkillIcon(skills[c.skillIndex].def.id) : -1;
                if (icon < 0) icon = c.kind == CardKind.Support ? SupportIcon(c.support.id) : StatIcon(c.stat.id);
                bool newSup = c.kind == CardKind.Support && icon < 0; SkillBarTex();
                SkillId? nsk = c.kind == CardKind.NewSkill ? c.skill.id : c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.id : (SkillId?)null;
                if (nsk.HasValue && (IceArt.IsIce(nsk.Value) || IsFb1481(nsk.Value) || IsPb1567(nsk.Value))) { GUI.color = new Color(1, 1, 1, ra); IceIconBlit(nsk.Value, ix, iy, CardIcon); }
                else if (nsk.HasValue && ZhCardIcon1625(nsk.Value) != null) { GUI.color = new Color(1, 1, 1, ra); GUI.DrawTexture(new Rect(ix, iy, CardIcon, CardIcon), ZhCardIcon1625(nsk.Value)); GUI.color = Color.white; }
                else if (_icons44 && nsk.HasValue && nsk.Value >= SkillId.ChainLightning && nsk.Value <= SkillId.PhantomAxes && _tSkill24 != null) { GUI.color = new Color(1, 1, 1, ra); BlitInset(_tSkill24, (int)nsk.Value * 28, 0, 24, 24, ix, iy, CardIcon, CardIcon); }
                else if (icon >= 0 && !newSup)
                {
                    SkillId? psk = c.kind == CardKind.NewSkill ? c.skill.id : c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.id : (SkillId?)null; var pt = psk.HasValue ? PhTint(psk.Value) : Color.white;
                    GUI.color = new Color(pt.r, pt.g, pt.b, ra); BlitInset(_tSkillIcons, icon * 16, 0, 16, 16, ix, iy, CardIcon, CardIcon); GUI.color = new Color(1, 1, 1, ra);
                }
                else if (newSup && _tSup20 != null) { int si = SupIx(c.support.id); BlitInset(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, ix, iy, CardIcon, CardIcon); }
                else BlitInset(_tRoomIcons, (c.kind == CardKind.Support ? 5 : 0) * 8, 0, 8, 8, ix, iy, CardIcon, CardIcon);
            }
            if (showNew && _tNew) Blit(_tNew, 0, 0, CardBadgeW, CardBadgeH, x + CardW - CardPad - CardBadgeW, iy);
        }

        void DrawCardsB(string title, string sub, bool levelUp)
        {
            if (_tp != null) { DrawTargetPickB(); return; }
            if (HellordTuning.I.cardsD1831) { DrawCardsD1831(title, sub, levelUp); return; } // t1831 frame D (Bronze Leiwen)
            if (HellordTuning.I.cardsA1825) { DrawCardsA1825(title, sub, levelUp); return; } // t1825 Option A (PoE2 tooltip card)
            FloorTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.69f * (levelUp ? FX26Dim : 1f)));
            _guiA = levelUp ? FX26Reveal(-1) : 1f; TextC(title, CardScreenTitleY, HCyan, 2); TextC(sub, CardScreenSubY, Color.white); _guiA = 1f;
            int n = cards.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; Card picked = null;
            var copies = new CardCopy[n]; var titles = new List<string>[n];
            int maxBody = 0, maxTitle = 1;
            for (int ci = 0; ci < n; ci++)
            {
                copies[ci] = new CardCopy(); FillCardDesc(cards[ci], copies[ci]);
                int bh = copies[ci].BodyH(); if (bh > maxBody) maxBody = bh;
                titles[ci] = WrapCard(CardNameOf(cards[ci])); if (titles[ci].Count > maxTitle) maxTitle = titles[ci].Count;
            }
            int plateH = CardPlateH(maxTitle), cardH = CardHeight(maxTitle, maxBody);
            for (int i = 0; i < n; i++)
            {
                var c = cards[i]; int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, cardH);
                float ra = levelUp ? FX26Reveal(i) : 1f; if (ra <= 0.001f) continue; _guiA = ra; GUI.color = new Color(1, 1, 1, ra);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0) + (levelUp ? FX26Slide(i) : 0);
                int rar = c.kind == CardKind.Stat && c.stat.id == StatId.Slot ? 3 : c.kind == CardKind.NewSkill ? 2 : c.kind == CardKind.Support ? 1 : (c.kind == CardKind.SkillLevel && skills[c.skillIndex].level + 1 >= HData.MaxSkillLevel) ? 3 : 0;
                int artX = (rar * 2 + (hover ? 1 : 0)) * CardArtW;
                DrawCardChrome(artX, x, y, CardW, cardH, plateH);
                DrawCardTitle(x, y, plateH, titles[i]);
                bool showNew = (c.kit > 0 && !c.kitLv1651) || c.kind == CardKind.NewSkill;
                DrawCardFace(c, x, y, plateH, ra, showNew);
                int textY = y + plateH + CardPad + CardIcon + CardPad;
                DrawCardCopy(copies[i], x, textY);
                if ((c.kind == CardKind.NewSkill && c.kit == 0) || c.kind == CardKind.SkillLevel || c.kind == CardKind.Support)
                {
                    int sti = c.kind == CardKind.Support ? (SupportTargets(c.support).Count == 1 ? SupportTarget(c.support) : -1) : -1;
                    int lvl = c.kind == CardKind.NewSkill ? 1 : c.kind == CardKind.Support ? (sti >= 0 && SupInst1651(sti) != null ? SupInst1651(sti).Lvl(c.support.id) + 1 : 1) : skills[c.skillIndex].level + 1;
                    DrawCardDots(x, y, cardH, lvl, rar);
                }
                DrawCardIndex(x, y, cardH, i);
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover && !FX26CardsLocked) { picked = c; ev.Use(); }
                _guiA = 1f; GUI.color = Color.white;
            }
            int footY = rowY + cardH + CardPad;
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

        void DrawEndBox(string title, Color tc, string line, string line2)
        {
            DimScreen(title == "YOU DIED" ? new Color(0.25f, 0.02f, 0.04f, 0.7f) : new Color(HInk.r, HInk.g, HInk.b, 0.75f));
            TextC(title, 110, tc, 3); TextC(line, 150, Color.white); if (line2 != null) TextC(line2, 162, CDim);
            float bw = 120, bh = 40, bx = 320 - bw / 2, by = 184;
            Nine(_tPause, 0, 90, 60, 5, 5, 4, 4, bx, by, bw, bh);
            Blit(_tPauseCur, 0, 0, _tPauseCur ? _tPauseCur.width : 0, _tPauseCur ? _tPauseCur.height : 0, bx + 13, by + 16);
            Text("R  RESTART", bx + 22, by + 16, Color.white);
        }
        string RunLine() { int m = (int)(runTime / 60), s = (int)(runTime % 60); int vis = 0; foreach (var r in floor) if (r.visited) vis++; return $"{m:00}:{s:00}  ROOMS {vis}/{floor.Count}  LV {level}  KILLS {kills}  BONES {bones}"; }
        void DrawDeathB() { FloorTex(); DrawEndBox("YOU DIED", CRed, RunLine(), PactEndLine51()); } // v0.0.51 souls line
        void DrawWonB() { FloorTex(); DrawEndBox("RUN COMPLETE", HCyan, RunLine(), FloorCount + " FLOORS CLEARED. " + PactEndLine51()); } // v0.0.51
    }
}
