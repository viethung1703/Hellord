using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1641 (VH approved plan): the bottom skill bar = the 4-slot kit: LMB basic | SPACE movement | RMB special | R ultimate.
    //  Same belt art (black outline, #3a4466 frame, #181425 fill, 26 px icon frame, 2x2 support gems + level on the basic, sweep masks).
    //  Each cell: key label on top (centred), icon below (icons_v2 for kit skills, the existing icon for the basic), cooldown sweep.
    //  Kit slots are empty + locked (padlock, dim label) until picked on level-up. Replaces the t1618 MOV / SPC / ULT debug text.
    //  Geometry (HUD px, 640x360), measured for VH: cell 57 x 48 (inner 55 x 46, odd width so every 5x7 label centres exactly), gap 2.
    //   rows: border | 3 pad | 7 label | 3 gap | 30 content | 3 pad | border (kit icon frame: 5 | 26 | 5 between label and border).   basic content: 2 pad | 26 frame | 4 | 21 gems | 2 pad.
    // =====================================================================================================================
    public partial class HellordGame
    {
        bool KitBar1641 => HellordTuning.I.kitBar1641 && HellordTuning.I.skillBelt720;
        const int KbN1641 = 4, KbLabelY1641 = 4, KbContentY1641 = 14;
        readonly float[] _kbFull1641 = new float[4], _kbPrev1641 = new float[4];

        string KbLabel1641(int i) => i == 0 ? "LMB" : i == 1 ? (Manual716 ? Label716(Bind716(Act716.Dash)) : "SPACE") : i == 2 ? "RMB" : "R";

        void DrawKitBelt1641()
        {
            var g = GUI.color; float gm = HellordTuning.I.beltGem716; Vector2 mp = Event.current.mousePosition; int hov = -1;
            int[] pick = { 0, ZhMove1618, ZhSpec1618, ZhUlt1618 }; float[] cd = { 0f, ZhMoveCd, ZhSpecCd, ZhUltCd };
            string[] key = { null, MoveKey1618(), SpecKey1618(), UltKey1618() };
            for (int i = 0; i < KbN1641; i++)
            {
                var r = Cell716(i); float x = r.x, y = r.y; if (r.Contains(mp)) hov = i;
                Fill716(x - 1, y - 1, r.width + 2, r.height + 2, Color.black); Fill716(x, y, r.width, r.height, C716(0x3a4466)); Fill716(x + 1, y + 1, r.width - 2, r.height - 2, C716(0x181425));
                if (hov == i && i == 0) Frame716(x, y, r.width, r.height, C716(0xead4aa));
                var k = i == 0 ? (_slot716[0] ?? (skills.Count > 0 ? skills[0] : null)) : null;
                bool filled = i == 0 ? k != null : pick[i] > 0;
                string lb = KbLabel1641(i); GUI.color = Color.white;
                Text(lb, x + Mathf.Floor((r.width - TextW(lb)) / 2f), y + KbLabelY1641, filled ? C716(0xc0cbdc) : C716(0x5a6988));
                float cy = y + KbContentY1641;
                if (i == 0)
                {   // basic: icon frame + 2x2 support gems + level, the group centred in the cell
                    float grp = 26 + 4 + (2 * gm + 1), fx = x + Mathf.Floor((r.width - grp) / 2f), ix = fx + 1, iy = cy + 3;
                    Frame716(ix - 1, iy - 1, 26, 26, C716(0x3a4466, 0.9f));
                    if (k == null) { Fill716(ix + 11, iy + 7, 2, 10, C716(0x3a4466)); Fill716(ix + 7, iy + 11, 10, 2, C716(0x3a4466)); continue; }
                    int ic = _icons44 ? (int)k.def.id : BarTile(k.def.id);
                    if (IceArt.IsIce(k.def.id) || IsFb1481(k.def.id) || IsPb1567(k.def.id)) { GUI.color = Color.white; if (!IceIconBlit(k.def.id, ix, iy, 24) && _tSkill24 != null) Blit(_tSkill24, 0, 0, 24, 24, ix, iy); }
                    else if (ZhCardIcon1625(k.def.id) != null) { GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), ZhCardIcon1625(k.def.id)); }
                    else if (_tSkill24 != null) { var pt = _icons44 ? Color.white : PhTint(k.def.id); GUI.color = pt; Blit(_tSkill24, ic * 28, 0, 24, 24, ix, iy); }
                    float full = Mathf.Max(0.05f, k.def.cooldown * k.CdMul); bool cont = k.def.id == SkillId.BoneOrbit || HData.Summon561(k.def);
                    float cdf = cont ? 0 : Mathf.Clamp01(k.cd / full);
                    if ((cdf > 0.02f && k.cd > 0.15f || BeltSweep1517(k)) && _tCdMask != null) { int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - cdf) * nf), 0, nf); GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]); if (full >= 1.5f) CdNumber(Mathf.CeilToInt(k.cd), ix, iy, true); }
                    float rdy = BeltReady1517(k); if (rdy > 0f) { Frame716(ix - 1, iy - 1, 26, 26, C716(0xfee761, rdy)); Frame716(ix - 2, iy - 2, 28, 28, C716(0xfeae34, rdy * 0.6f)); }
                    float gx0 = fx + 26 + 4, gy0 = cy;
                    for (int p = 0; p < 4; p++)
                    {
                        float sx = gx0 + (p % 2) * (gm + 1), sy = gy0 + (p / 2) * (gm + 1); bool open = p < Mathf.Min(k.level, 4), fill = p < k.sup.Count;
                        if (fill && _tSup20 != null) { int si = SupIx(k.sup[p]); GUI.color = Color.white; Blit(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, sx, sy, gm, gm); }
                        else { Frame716(sx, sy, gm, gm, open ? C716(0x5a6988) : C716(0x262b44)); if (!open) Fill716(sx + 1, sy + 1, gm - 2, gm - 2, C716(0x262b44)); }
                    }
                    GUI.color = Color.white; Text("L" + k.level, gx0, gy0 + 2 * (gm + 1) + 1, k.level >= 5 ? C716(0xfee761) : C716(0x8b9bb4));
                    continue;
                }
                {   // kit slot: icon frame centred (26 px frame in the 55 px inner width: 14 | 26 | 15)
                    float ix = x + Mathf.Floor((r.width - 24) / 2f), iy = cy + 3;
                    Frame716(ix - 1, iy - 1, 26, 26, C716(0x3a4466, 0.9f));
                    if (!filled)
                    {   // padlock (10 x 12, centred in the 24 px icon box: 7 | 10 | 7, 6 | 12 | 6), bar ink like the empty '+'
                        var lc = C716(0x3a4466); Fill716(ix + 9, iy + 6, 6, 1, lc); Fill716(ix + 9, iy + 7, 1, 3, lc); Fill716(ix + 14, iy + 7, 1, 3, lc);
                        Fill716(ix + 7, iy + 10, 10, 8, lc); Fill716(ix + 11, iy + 12, 2, 3, C716(0x181425));
                        _kbFull1641[i] = _kbPrev1641[i] = 0f; continue;
                    }
                    var tex = ZhIcon1618(key[i]); GUI.color = Color.white;
                    if (tex != null) GUI.DrawTexture(new Rect(ix, iy, 24, 24), tex); else BlitInset(_tRoomIcons, 0, 0, 8, 8, ix + 4, iy + 4, 16, 16);
                    float c = Mathf.Max(0f, cd[i]); if (c > _kbPrev1641[i] + 0.01f) _kbFull1641[i] = c; _kbPrev1641[i] = c; float fl = Mathf.Max(0.05f, _kbFull1641[i]); // full = the cooldown just set
                    bool unarmed = i == 3 && c <= 0f && !ZhUltArmed1618 && ZhUlt1618 != 2; // the ultimate waits for its condition (Rebirth Flame is automatic)
                    if (_tCdMask != null && (c > 0.05f || unarmed))
                    {
                        int nf = _tCdMask.Length - 1; int fr = unarmed ? 0 : Mathf.Clamp(Mathf.FloorToInt((1 - Mathf.Clamp01(c / fl)) * nf), 0, nf);
                        GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]); if (c > 0.05f && fl >= 1.5f) CdNumber(Mathf.CeilToInt(c), ix, iy, true);
                    }
                }
            }
            DrawSinHud818(Cell716(0).x, Cell716(0).y - 1); // t818u sins frame above the belt, left-aligned (unchanged)
            if (hov == 0 && _slot716[0] != null && HellordTuning.I.beltTooltip716 && !_ctrlOpen716) DrawTip716(_slot716[0], Cell716(0));
            GUI.color = g;
        }
    }
}
