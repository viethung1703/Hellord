using UnityEngine;
using System.Collections.Generic;
namespace Hellord {
 // t1156 MOCKUP layout (opt-in via -sinlayout1156; default OFF, shipped layout unchanged).
 // One grid: content x69..410 (341), y49..244, gutter G=3 everywhere. One panel frame (StonePlaque89 b=3),
 // one button family (Recess87 face / _s74on primary, sizes M=14 and S=12, same font, same ember states).
 public partial class HellordGame {
  public static bool SinLayout1156 = System.Environment.CommandLine.Contains("-sinlayout1156");
  const float L56G = 3, L56X0 = 69, L56X1 = 410, L56Y0 = 49, L56Y1 = 244;
  static readonly Rect L56Grid = new Rect(69, 109, 239, 135), L56Det = new Rect(311, 109, 99, 135);
  static readonly Rect L56Act = new Rect(317, 207, 87, 14), L56Back = new Rect(317, 224, 87, 14);
  // t1159: the Sins view is a symmetric crop of ui_s77_bg (no stretched/duplicated fill columns). The crop window is centred
  // on the measured pillar axis (inner lit edges x61 | x418 -> axis 239.5) and sits wholly inside the clean art (x6..469, y6..262),
  // so the baked black margins are simply off-screen. All frames/buttons are snapped to whole SCREEN pixels with one border width.
  public static float View56Axis = 239.5f, View56W = 455.1f, View56CY = 134.5f;
  public static float UI56Dx = Arg56("-sinuidx=", 0.5f);
  static float Arg56(string key, float def) { foreach (var a in System.Environment.GetCommandLineArgs()) if (a.StartsWith(key) && float.TryParse(a.Substring(key.Length), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v; return def; }
  Matrix4x4 _bgFull56; float _bgVox56;
  bool _v56; float _vs56 = 1, _vox56, _voy56, _k56 = 1; Matrix4x4 _full56;
  float _txS56 => _v56 ? _vs56 : 1f;
  void Tx56Map(ref float x, ref float y) { if (_v56) { x = (x - _vox56) * _vs56; y = (y - _voy56) * _vs56; } }
  bool DrawSinView56()
  {
   if (!(SinLayout1156 || Kit98On && Kit98Ready) || _v56) return false;
   var G = GUI.matrix; float vx = View56Axis - View56W / 2, vh = View56W * 392f / 700f, vy = View56CY - vh / 2;
   K98Integer = false; K98Scale = 0;
   if (SinLayout1156 || !K98ViewScale(G)) { _vs56 = 480f / View56W; _vox56 = U74(vx); _voy56 = U74(vy); } // t1175: integer native-px scale when possible
   var M = Matrix4x4.Scale(new Vector3(_vs56, _vs56, 1)) * Matrix4x4.Translate(new Vector3(-_vox56, -_voy56, 0));
   _full56 = G * M; _k56 = _full56.m00 * K74; _v56 = true; GUI.matrix = _full56;
   _bgFull56 = _full56; _bgVox56 = _vox56;
   try { DrawSinPanel74(); } finally { _v56 = false; GUI.matrix = G; }
   return true;
  }
  Vector2 Scr56(float nx, float ny) { var p = _full56.MultiplyPoint3x4(new Vector3(U74(nx), U74(ny), 0)); return new Vector2(Mathf.Round(p.x), Mathf.Round(p.y)); }
  Rect ScrR56(float x, float y, float w, float h) { var a = Scr56(x, y); var b = Scr56(x + w, y + h); return Rect.MinMaxRect(a.x, a.y, b.x, b.y); }
  float Px56(float native) => Mathf.Max(1, Mathf.Round(native * _k56));
  void Blit56(Texture2D t, Rect src, Rect d) { if (d.width <= 0 || d.height <= 0) return; GUI.color = Color.white; GUI.DrawTextureWithTexCoords(d, t, new Rect(src.x / t.width, 1 - src.yMax / t.height, src.width / t.width, src.height / t.height)); }
  // Everything below draws with GUI.matrix = identity in whole screen pixels.
  void Scr56Begin(out Matrix4x4 m) { m = GUI.matrix; GUI.matrix = Matrix4x4.identity; }
  void Fill56(float x, float y, float w, float h, Color c) { Scr56Begin(out var m); var d = ScrR56(x, y, w, h); GUI.color = c; GUI.DrawTexture(d, Texture2D.whiteTexture); GUI.color = Color.white; GUI.matrix = m; }
  void FillS56(Rect d, Color c) { GUI.color = c; GUI.DrawTexture(d, Texture2D.whiteTexture); GUI.color = Color.white; }
  void BgSlice56(float sx, float sy, float w, float h, float dx, float dy, float dw, float dh) { Scr56Begin(out var m); Blit56(_s74bg, new Rect(sx, sy, w, h), ScrR56(dx, dy, dw, dh)); GUI.matrix = m; }
  void BlitF56(Texture2D t, Rect src, Rect d, bool fx, bool fy)
  {
   if (d.width <= 0 || d.height <= 0) return; GUI.color = Color.white;
   float u = src.x / t.width, v = 1 - src.yMax / t.height, uw = src.width / t.width, vh = src.height / t.height;
   GUI.DrawTextureWithTexCoords(d, t, new Rect(fx ? u + uw : u, fy ? v + vh : v, fx ? -uw : uw, fy ? -vh : vh));
  }
  // t1159: the source frames are lit top-left (socket art right rim = 6px with a dark outer column, bottom likewise), which made
  // every right/bottom outline read thinner. Right/bottom slices are now the left/top slices mirrored: identical thickness and rim.
  void Patch56S(Texture2D t, Rect src, float sb, Rect d, float db, bool skipCentre = false)
  {
   float[] a = { src.x, src.x + sb, src.xMax - sb, src.xMax }, b = { src.y, src.y + sb, src.yMax - sb, src.yMax };
   float[] dx = { d.x, d.x + db, d.xMax - db, d.xMax }, dy = { d.y, d.y + db, d.yMax - db, d.yMax };
   for (int j = 0; j < 3; j++) for (int k = 0; k < 3; k++)
   {
    if (skipCentre && j == 1 && k == 1) continue;
    int sk = k == 2 ? 0 : k, sj = j == 2 ? 0 : j;
    BlitF56(t, new Rect(a[sk], b[sj], a[sk + 1] - a[sk], b[sj + 1] - b[sj]), new Rect(dx[k], dy[j], dx[k + 1] - dx[k], dy[j + 1] - dy[j]), k == 2, j == 2);
   }
  }
  // Socket and tile wells: left/top half mirrored onto right/bottom half, so a socket or tile never has a heavier right rim.
  void BgSym56(float sx, float sy, float w, float h, float dx, float dy, float dw, float dh) { Scr56Begin(out var m); var d = ScrR56(dx, dy, dw, dh); Patch56S(_s74bg, new Rect(sx, sy, w, h), w / 2, d, Mathf.Floor(d.width / 2)); GUI.matrix = m; }
  void Patch56(Texture2D t, Rect src, float border, float x, float y, float w, float h) { Scr56Begin(out var m); Patch56S(t, src, border, ScrR56(x, y, w, h), Px56(border)); GUI.matrix = m; }
  // ONE panel frame: socket-stone 9-slice, border = round(3 native px) screen px on all four sides, face tiled at whole-pixel steps.
  Rect Frame56(float x, float y, float w, float h, bool well)
  {
   Scr56Begin(out var m); var d = ScrR56(x, y, w, h); float bd = Px56(3), lp = Px56(1);
   var inner = Rect.MinMaxRect(d.x + bd, d.y + bd, d.xMax - bd, d.yMax - bd);
   if (well) FillS56(inner, new Color32(15, 15, 14, 255));
   else { float tw = Px56(18), th = Px56(7); for (float yy = inner.y; yy < inner.yMax; yy += th) for (float xx = inner.x; xx < inner.xMax; xx += tw) { float cw = Mathf.Min(tw, inner.xMax - xx), ch = Mathf.Min(th, inner.yMax - yy); Blit56(_s74bg, new Rect(8, 7, 18 * cw / tw, 7 * ch / th), new Rect(xx, yy, cw, ch)); } }
   // border: 9-slice of the 40px socket frame with the centre skipped
   Patch56S(_s74bg, new Rect(126, 46, 40, 40), 3, d, bd, true);
   // engraved lip: identical shadow lip on left AND right (mirror-symmetric), top shadow, bottom light; same thickness everywhere
   FillS56(new Rect(inner.x, inner.y, inner.width, lp), new Color32(8, 8, 7, 255)); FillS56(new Rect(inner.x, inner.y, lp, inner.height), new Color32(10, 9, 8, 255));
   if (!well) FillS56(new Rect(inner.xMax - lp, inner.y, lp, inner.height), new Color32(10, 9, 8, 255));
   if (well) { FillS56(new Rect(inner.x, inner.yMax - lp, inner.width, lp), new Color32(40, 38, 33, 255)); FillS56(new Rect(inner.xMax - lp, inner.y, lp, inner.height), new Color32(10, 9, 8, 255)); }
   GUI.matrix = m; return inner;
  }
  void Well56(float x, float y, float w, float h) => Frame56(x, y, w, h, true);
  void Ol56(float x, float y, float w, float h, Color c) { Scr56Begin(out var m); var d = ScrR56(x, y, w, h); float t = Px56(1); FillS56(new Rect(d.x, d.y, d.width, t), c); FillS56(new Rect(d.x, d.yMax - t, d.width, t), c); FillS56(new Rect(d.x, d.y, t, d.height), c); FillS56(new Rect(d.xMax - t, d.y, t, d.height), c); GUI.matrix = m; }
  void Recess56(float x, float y, float w, float h) => Patch56(_s74bg, new Rect(131, 92, 55, 20), 2, x, y, w, h);
  bool Btn56(string id, float x, float y, float w, float h, string label, bool enabled, bool primary, Color? col = null)
  {
   if (primary) Patch56(_s74on, new Rect(0, 0, 55, 20), 3, x, y, w, h); else Recess56(x, y, w, h);
   BtnFx48(id, x, y, w, h, enabled); var o = Off48(id, x, y, w, h);
   if (label != null) Lbl74(label, x + o.x, y + o.y, w, h, col ?? (enabled ? S74_SAND : S74_DIM), 2);
   return Btn48(id, x, y, w, h, enabled);
  }
  void Currency56(float x, float y, float w, float h, bool sigil)
  {
   Frame56(x, y, w, h, false); float iy = y + (h - 12) / 2; Fill56(x + 5, iy, 16, 12, new Color32(13, 12, 11, 255));
   var t = T53(sigil ? "sigil" : "soul"); if (t) { Scr56Begin(out var m); var d = ScrR56(x + 7, iy, 12, 12); GUI.color = Color.white; GUI.DrawTexture(d, t); GUI.matrix = m; }
   string label = sigil ? "SIGILS" : "SOULS"; int n = Shown48(sigil, sigil ? _sigils51 : _souls51); string value = n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
   float tx = x + 27, vy = y + (h - 6) / 2; Tx74(label, tx, vy, S74_SAND); Tx74(value, tx + TW74(label) + 5, vy, Rolling48 ? Heat48 : S74_SAND);
  }
  void DrawSinPanel1156()
  {
   // t1159: UI-only sub-pixel shift (native px) so the measured gap to the ox pillar equals the gap to the horse pillar.
   _full56 = _bgFull56 * Matrix4x4.Translate(new Vector3(U74(UI56Dx), 0, 0)); _vox56 = _bgVox56 - U74(UI56Dx); GUI.matrix = _full56;
   // Uniform mortar behind the whole content block: every gap is an intentional 3px joint, never a void.
   Fill56(L56X0 - 3, L56Y0 - 2, (L56X1 - L56X0) + 6, (L56Y1 - L56Y0) + 4, new Color32(22, 20, 17, 255));
   var selC = S74_EMB;
   // ---- Row A (y49..89): three socket frames (own carved art) + currency pair + cost plaque
   const int sy = 49, ss = 40; float[] sx = { 69, 112, 155 };
   for (int i = 0; i < 3; i++) BgSym56(126, 46, 40, 40, sx[i], sy, 40, 40);
   for (int i = 0; i < 3; i++)
   {
    float x = sx[i]; bool unl = i < _sinSlots737 && i < SlotsMax737;
    if (unl)
    {
     int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null;
     if (d != null) { float n = IconN74(3); Icon74(d, x + (ss - n) / 2, sy + (ss - n) / 2, 3); if (_sinSel737 == d.id) Ol56(x + 6, sy + 6, ss - 12, ss - 12, selC); if (Hit74(x, sy, ss, ss)) _sinSel737 = d.id; }
     else if (!UnlockReveal48(i, x, sy, ss)) Lbl74("EMPTY", x, sy, ss, ss, S74_DIM, 2);
    }
    else if (i < SlotsMax737)
    {
     { Scr56Begin(out var lm); Blit56(_s74lock, new Rect(0, 0, _s74lock.width, _s74lock.height), ScrR56(x + DenyShake48(i), sy, _s74lock.width, _s74lock.height)); GUI.matrix = lm; }
     int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
     if (LockSocket48(i, x, sy, ss, next, afford, sc, gc)) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; Unlocked48(i); }
    }
   }
   _currencyFit91 = true; Currency56(198, 49, 104, 19, false); Currency56(305, 49, 105, 19, true);
   Frame56(198, 71, 212, 18, false);
   string slots = "SLOTS " + _sinSlots737 + "/" + SlotsMax737, req; Color rc = S74_GOLD;
   if (SlotsMax737 <= 0) { req = "NO SLOTS AVAILABLE"; rc = S74_SAND; }
   else if (_sinSlots737 >= SlotsMax737) req = "ALL OPEN - READY TO EQUIP";
   else { int sc = Arr737(ST737.sinSlotSoulCost737, _sinSlots737, 100), gc = Arr737(ST737.sinSlotSigilCost737, _sinSlots737, 1); req = "NEXT " + sc.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " SOULS + " + gc.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " SIGILS"; if (TW74(slots) + 8 + TW74(req) > 200) req = CostLine88(sc, gc); }
   if (HdrOverride48(out var hs, out var hc)) { req = hs; rc = hc; }
   Tx74(slots, 204, 77, SinFeel1148On && _f48UnlockSlot >= 0 && Now48 - _f48UnlockT < 3.2f ? Reward48 : S74_SAND); Tx74(req, 404 - TW74(req), 77, rc);
   // ---- Row B (y92..106): four EQUAL 83px tabs spanning the full content width, G=3
   string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };
   for (int i = 0; i < 4; i++) { float x = 69 + i * 86; bool sel = _sinFilter737 == i; if (Btn56("tab" + i, x, 92, 83, 14, fl[i], true, sel, sel ? S74_EMB : S74_SAND)) { if (!sel) HellordAudio.Play("ui_confirm"); _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; } }
   // ---- Row C left: collection well with its own page rail; tiles centred (2px each side)
   Well56(L56Grid.x, L56Grid.y, L56Grid.width, L56Grid.height);
   Fill56(72, 113, 233, 14, new Color32(26, 25, 22, 255)); Fill56(72, 127, 233, 1, new Color32(14, 13, 11, 255)); Fill56(73, 128, 231, 1, new Color32(57, 54, 47, 255));
   var list = SinFiltered737(); const int cols = 8, rows = 4, per = cols * rows; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1); _s77Pages = pages; _s77Count = list.Count;
   for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++) BgSym56(74, 121, 26, 26, 74 + c * 29, 129 + r * 28, 26, 26);
   float icn = IconN74(2);
   for (int k = 0; k < per; k++)
   {
    int li = _sinPage737 * per + k; if (li >= list.Count) break; var d = list[li];
    float x = 74 + (k % cols) * 29, y = 129 + (k / cols) * 28, w = 26, h = 26; bool own = _sinOwned737[d.id];
    if (own) Icon74(d, x + (w - icn) / 2, y + (h - icn) / 2, 2); else Lbl74("?", x, y, w, h, S74_DIM, 0);
    if (SinEquipped737(d.id)) Fill56(x + w - 7, y + 4, 3, 3, S74_GOLD);
    if (_sinSel737 == d.id) Ol56(x + 1, y + 1, w - 2, h - 2, S74_EMB);
    if (Hit74(x, y, w, h)) { _sinSel737 = d.id; _s74Page = 0; }
   }
   string pc = "PAGE " + (_sinPage737 + 1) + " OF " + pages + "   OWNED " + SinOwnedCount737 + "/" + HSins.Count; Tx74(pc, 188.5f - TW74(pc) / 2, 117, S74_SAND);
   if (Btn56("pagePREV", 74, 114, 40, 12, "PREV", _sinPage737 > 0, false)) { _sinPage737--; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
   if (Btn56("pageNEXT", 263, 114, 40, 12, "NEXT", _sinPage737 < pages - 1, false)) { _sinPage737++; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
   // ---- Row C right: detail well, same frame; header (icon + tier), body, then M buttons stacked with G
   Well56(L56Det.x, L56Det.y, L56Det.width, L56Det.height);
   const float dX = 317, dW = 87, dY = 133, dH = 72; string act = null; bool actOn = false; int actKind = 0, free = -1;
   var lines = new List<KeyValuePair<string, Color>>();
   Fill56(314, 113, 93, 17, new Color32(26, 25, 22, 255)); Fill56(314, 130, 93, 1, new Color32(14, 13, 11, 255)); Fill56(315, 131, 91, 1, new Color32(57, 54, 47, 255));
   if (_sinSel737 >= 0)
   {
    var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id];
    if (own) Icon74(d, 316, 114, 2); else Lbl74("?", 316, 114, 15, 15, S74_DIM, 0);
    string tier = HSins.TierName[(int)d.tier] + " NO." + (d.id + 1); Tx74(tier, 334, 119, S74_RED);
    if (own)
    {
     Wrap74(lines, d.name, dW, S74_GOLD);
     var q = _sinRoll737[d.id]; Wrap74(lines, HSins.QualityTxt(q, d), dW, HSins.Quality(q) >= ST737.sinPerfectAt737 ? S74_GOLD : S74_SAND);
     foreach (var l in HSins.Lines(d, q)) Wrap74(lines, l, dW, l.StartsWith("-") ? S74_RED : l.StartsWith("EFFECT NOT") ? S74_DIM : S74_SAND);
     if (SinEquipped737(d.id)) { act = "UNEQUIP"; actOn = true; actKind = 1; }
     else { for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; } act = _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; }
    }
    else { Wrap74(lines, "UNKNOWN SIN", dW, S74_SAND); Wrap74(lines, "Not found yet. Sins drop from demons and bosses.", dW, S74_DIM); act = "NOT FOUND"; }
   }
   else { Tx74("DETAILS", 317, 119, S74_DIM); Wrap74(lines, "Select a sin from the collection to read it here.", dW, S74_DIM); act = "SELECT A SIN"; }
   const float lh = 8.6f; int cap = Mathf.FloorToInt(dH / lh); int capP = lines.Count > cap ? cap - 1 : cap;
   _s74Pages = Mathf.Max(1, (lines.Count + capP - 1) / capP); _s74Page = Mathf.Clamp(_s74Page, 0, _s74Pages - 1); _s74Lines = lines.Count; _s74Cap = capP; _s74Overflow = false;
   for (int i = 0; i < capP; i++) { int li = _s74Page * capP + i; if (li >= lines.Count) break; Tx74(lines[li].Key, dX, dY + i * lh, lines[li].Value); }
   // Body never left as a void: when the text is short, the selected sin is shown large, centred in the free space, with its state.
   int shown = Mathf.Min(capP, lines.Count - _s74Page * capP); float freeY = dY + shown * lh + 2, freeH = 205 - freeY;
   if (_s74Pages <= 1 && _sinSel737 >= 0 && freeH >= 40)
   {
    var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id]; float n = IconN74(4), cy = freeY + (freeH - n - 10) / 2;
    Fill56(321, freeY + 2, 79, 1, new Color32(40, 38, 33, 255));
    if (own) Icon74(d, 360.5f - n / 2, cy, 4); else Lbl74("?", 360.5f - n / 2, cy, n, n, S74_DIM, 0);
    int eqSlot = -1; for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) eqSlot = i;
    string st = !own ? "NOT FOUND" : eqSlot >= 0 ? "EQUIPPED - SLOT " + (eqSlot + 1) : "OWNED"; Tx74(st, 360.5f - TW74(st) / 2, cy + n + 4, eqSlot >= 0 ? S74_GOLD : S74_DIM);
   }
   if (_s74Pages > 1)
   {
    string tp = "TEXT " + (_s74Page + 1) + "/" + _s74Pages; Tx74(tp, 360.5f - TW74(tp) / 2, 196, S74_SAND);
    if (Btn56("txtL", 317, 192, 14, 12, null, _s74Page > 0, false)) _s74Page--; Arrow87(324, 198, -1, _s74Page > 0 ? S74_SAND : S74_DIM);
    if (Btn56("txtR", 390, 192, 14, 12, null, _s74Page < _s74Pages - 1, false)) _s74Page++; Arrow87(397, 198, 1, _s74Page < _s74Pages - 1 ? S74_SAND : S74_DIM);
   }
   if (act != null && Btn56("action", L56Act.x, L56Act.y, L56Act.width, L56Act.height, act, actOn, actOn))
   {
    var d = HSins.All[_sinSel737];
    if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_back"); }
    else if (free >= 0) { _sinEq737[free] = d.id; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_confirm"); }
   }
   // BACK: same family (Recess face, M size), arrow + label, inside the panel with 3px padding.
   Recess56(L56Back.x, L56Back.y, L56Back.width, L56Back.height); BtnFx48("back", L56Back.x, L56Back.y, L56Back.width, L56Back.height, true);
   var bo = Off48("back", L56Back.x, L56Back.y, L56Back.width, L56Back.height); string bl = "BACK TO MENU"; float bw = TW74(bl) + 9, bx = L56Back.x + (L56Back.width - bw) / 2;
   Arrow87(bx + 2 + bo.x, 231 + bo.y, -1, new Color32(238, 225, 194, 255)); Tx74(bl, bx + 9 + bo.x, 228 + bo.y, new Color32(238, 225, 194, 255));
   if (BackBtn48(L56Back.x, L56Back.y, L56Back.width, L56Back.height)) { SinClose87(); return; }
   Confirm49Draw();
   string mk = "MOCKUP NOT RUNTIME"; Fill56(239.5f - TW74(mk) / 2 - 4, 251, TW74(mk) + 8, 11, new Color32(10, 9, 8, 230)); Tx74(mk, 239.5f - TW74(mk) / 2, 253, S74_GOLD);
  }
 }
}
