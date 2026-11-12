using System.Collections.Generic;
using UnityEngine;

// step74 (VH t1104): Sins panel on the step73 stone-guardian art (Art/previews/step73_equal_tabs_talismans, runtime_art in step74).
// Art is 480x270 native drawn over the full 700x392 panel space (x1.4583 units; 2.667 screen px at 1280x720, point filtered).
// Reversible: SinArt74On = false -> step64 trial (also off) -> original DrawSinPanel737. Mechanics reuse the 737 fields/helpers.
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool SinArt74On = true;
        Texture2D _s74bg, _s74lock, _s74on; bool _s74tried;
        const float K74 = 700f / 480f;
        static float U74(float v) => v * K74;
        static readonly Color S74_SAND = new Color32(0xde, 0xcc, 0xa8, 255), S74_DIM = new Color32(0x86, 0x7a, 0x6c, 255), S74_EMB = new Color32(0xec, 0x60, 0x2c, 255),
            S74_GOLD = new Color32(0xf0, 0xc8, 0x5a, 255), S74_RED = new Color32(0xd6, 0x46, 0x38, 255);
        public Vector2? _s74Fake; Vector2? _s74Click; public int _s74Pages, _s74Page, _s74Lines, _s74Cap; public bool _s74Overflow;

        bool SinArt74Ready()
        {
            if (!_s74tried) { _s74tried = true; _s74bg = UiTex("s77_bg"); if (!_s74bg) _s74bg = UiTex("s74_bg"); _s74lock = UiTex("s74_lock"); _s74on = UiTex("s74_btn_on"); }
            return _s74bg && _s74lock && _s74on;
        }
        void Img74(Texture2D t, float x, float y) => GUI.DrawTexture(new Rect(U74(x), U74(y), U74(t.width), U74(t.height)), t);
        void Box74(float x, float y, float w, float h, Color c) => R53(U74(x), U74(y), U74(w), U74(h), c);
        void Ol74(float x, float y, float w, float h, Color c, float t = 1) { Box74(x, y, w, t, c); Box74(x, y + h - t, w, t, c); Box74(x, y, t, h, c); Box74(x + w - t, y, t, h, c); }
        bool Over74(float x, float y, float w, float h) => Over51(U74(x), U74(y), U74(w), U74(h));
        bool Hit74(float x, float y, float w, float h)
        {   // autotest hook: _s74Fake = click in native art px, consumed on next repaint through the same rects
            if (!Gate49) return false; // t1149 modal confirm: underlying controls are inert while it is open
            if (_s74Click.HasValue && new Rect(x, y, w, h).Contains(_s74Click.Value)) { _s74Click = null; return true; }
            return Click53(U74(x), U74(y), U74(w), U74(h));
        }
        void Tx74(string s, float x, float y, Color c) => Tx69(s, U74(x), U74(y), c);
        float TW74(string s) => TW69(s) / K74 / _txS56; // native px (t1159: text is not zoomed by the Sins view crop)
        /// <summary>centered label; splits into two lines at the space nearest the middle when wider than w-pad*2.</summary>
        void Lbl74(string s, float x, float y, float w, float h, Color c, float pad = 6)
        {
            if (TW74(s) <= w - pad * 2 || s.IndexOf(' ') < 0 && s.IndexOf('-') < 0) { Tx74(s, x + (w - TW74(s)) / 2, y + (h - 6) / 2, c); return; }
            int mid = s.Length / 2, best = -1; for (int i = 0; i < s.Length; i++) if ((s[i] == ' ' || s[i] == '-') && (best < 0 || Mathf.Abs(i - mid) < Mathf.Abs(best - mid))) best = i;
            string a = s[best] == '-' ? s.Substring(0, best + 1) : s.Substring(0, best), b = s.Substring(best + 1);
            Tx74(a, x + (w - TW74(a)) / 2, y + h / 2 - 7, c); Tx74(b, x + (w - TW74(b)) / 2, y + h / 2 + 1, c);
        }
        // Step87: atlas reuse, no new art. Small recessed controls belong to the panel header, not the floor.
        void Atlas87(Rect src,Rect dst,float border=3)=>Patch88(_s74bg,src,dst,border);
        void Patch88(Texture2D tex,Rect src,Rect dst,float border=3)
        {
            float[] a={src.x,src.x+border,src.xMax-border,src.xMax},b={src.y,src.y+border,src.yMax-border,src.yMax};
            float[] dx={dst.x,dst.x+border,dst.xMax-border,dst.xMax},dy={dst.y,dst.y+border,dst.yMax-border,dst.yMax};GUI.color=Color.white;
            for(int j=0;j<3;j++)for(int k=0;k<3;k++)GUI.DrawTextureWithTexCoords(new Rect(U74(dx[k]),U74(dy[j]),U74(dx[k+1]-dx[k]),U74(dy[j+1]-dy[j])),tex,new Rect(a[k]/tex.width,1-b[j+1]/tex.height,(a[k+1]-a[k])/tex.width,(b[j+1]-b[j])/tex.height));
        }
        // t1127: native socket stone; corners and cropped edge tiles stay 1:1, never stretched.
        void StoneTile89(Rect src,Rect dst)
        {
            GUI.color=Color.white;
            for(float y=dst.y;y<dst.yMax;y+=src.height)for(float x=dst.x;x<dst.xMax;x+=src.width){float w=Mathf.Min(src.width,dst.xMax-x),h=Mathf.Min(src.height,dst.yMax-y);GUI.DrawTextureWithTexCoords(new Rect(U74(x),U74(y),U74(w),U74(h)),_s74bg,new Rect(src.x/480,1-(src.y+h)/270,w/480,h/270));}
        }
        void StonePlaque89(float x,float y,float w,float h,float b)
        {
            StoneTile89(new Rect(8,7,18,7),new Rect(x+b,y+b,w-2*b,h-2*b));
            float[] sx={126,126+b,166-b,166},sy={46,46+b,86-b,86},dx={x,x+b,x+w-b,x+w},dy={y,y+b,y+h-b,y+h};
            for(int j=0;j<3;j++)for(int k=0;k<3;k++)if(j!=1||k!=1)StoneTile89(new Rect(sx[k],sy[j],sx[k+1]-sx[k],sy[j+1]-sy[j]),new Rect(dx[k],dy[j],dx[k+1]-dx[k],dy[j+1]-dy[j]));
            // Engraved inner lip/contact shadow; texture remains visible throughout the face.
            Box74(x+b,y+b,w-2*b,1,new Color32(13,12,11,255));Box74(x+b,y+b,1,h-2*b,new Color32(15,14,12,255));
        }
        void Channel89(float x,float y,float w)
        {Box74(x,y,w,1,new Color32(14,13,11,255));Box74(x+1,y+1,w-2,1,new Color32(57,54,47,255));}
        // t1126: an allocated header budget, not a floating stack. Currency labels are words, never guessed icons.
        bool _s88TabsFit,_s88StatsFit;string _s88CostState="";
        string Num88(int n)=>n.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string CostLine88(int souls,int sigils)=>Num88(souls)+" SOULS + "+Num88(sigils)+" SIGILS";
        static readonly Rect S90Action=new Rect(325,198,69,21),S90Back=new Rect(325,221,69,17),S90Text=new Rect(326,184,70,13);
        // One original bitmap font throughout. Normal amounts are grouped, with exact ungrouped
        // digits only when grouping would overfill their column; never shrink labels or lose digits.
        string Balance90(string label,int n)
        {string grouped=n.ToString("N0",System.Globalization.CultureInfo.InvariantCulture);return TW74(label)+7+TW74(grouped)<=80?grouped:Num88(n);}
        bool Currency88(string label,int value,float x)
        {
            string n=Balance90(label,value);Tx74(label,x,65,S74_SAND);Tx74(n,x+TW74(label)+7,65,S74_SAND);
            return TW74(label)+7+TW74(n)<=80;
        }
        string Requirement90(int slot,int sc,int gc,out bool headerContext)
        {
            string grouped=sc.ToString("N0",System.Globalization.CultureInfo.InvariantCulture)+" SOULS + "+gc.ToString("N0",System.Globalization.CultureInfo.InvariantCulture)+" SIGILS";
            string prefix="SLOT "+slot+" COST  ";string line=prefix+grouped;
            if(TW74(line)>166)line=prefix+CostLine88(sc,gc);
            headerContext=TW74(line)>166;return headerContext?CostLine88(sc,gc):line;
        }
        void Budget88()
        {
            _currencyFit91=true;CurrencyCounter91(222,46,false);CurrencyCounter91(314,46,true);StonePlaque89(222,77,178,15,3);
            string owned="OWNED "+SinOwnedCount737+"/"+HSins.Count,slots="SLOTS "+_sinSlots737+"/"+SlotsMax737,requirement;bool headerContext=false;Color reqColor=S74_GOLD;
            if(SlotsMax737<=0){requirement="NO SLOTS AVAILABLE";_s88CostState="UNAVAILABLE";reqColor=S74_SAND;}
            else if(_sinSlots737>=SlotsMax737){requirement="ALL SLOTS OPEN - READY TO EQUIP";_s88CostState="ALL_OPEN";}
            else{int sc=Arr737(ST737.sinSlotSoulCost737,_sinSlots737,100),gc=Arr737(ST737.sinSlotSigilCost737,_sinSlots737,1);requirement=Requirement90(_sinSlots737+1,sc,gc,out headerContext);_s88CostState="COST_SLOT_"+(_sinSlots737+1);if(headerContext)slots+=" COST "+(_sinSlots737+1);}
            // Three deliberately spaced text rows, not four rows pushed against their bevels.
            Tx74(owned,78,87,S74_SAND);Tx74(slots,214-TW74(slots),87,SinFeel1148On&&_f48UnlockSlot>=0&&Now48-_f48UnlockT<3.2f?Reward48:S74_SAND);
            _s88StatsFit=TW74(owned)+TW74(slots)+12<=136;
            _s88StatsFit &= _currencyFit91;
            if(HdrOverride48(out var hs,out var hc)){requirement=hs;reqColor=hc;}
            Tx74(requirement,228,82,reqColor);_s88StatsFit &= TW74(requirement)<=166;
        }
        void Layout88()
        {
            // Header begins below all authored paper alpha (last native row45). Guardians/FX remain outside these regions.
            Box74(70,46,332,68,new Color32(12,12,11,255));Box74(322,43,80,3,new Color32(12,12,11,255));
            for(int i=0;i<3;i++){float x=78+i*48;GUI.DrawTextureWithTexCoords(new Rect(U74(x),U74(46),U74(40),U74(40)),_s74bg,new Rect(x/480,1-86f/270,40f/480,40f/270));}
            // Reclaim the old large selected-icon bay for the planned info block. Preview moves beside its action button.
            Atlas87(new Rect(70,116,240,125),new Rect(318,114,84,126));Box74(321,117,78,120,new Color32(15,15,14,255));
            Patch88(_s74bg,new Rect(325,204,69,29),S90Action,3);
        }
        bool PageButton88(string label,float x,bool enabled)
        {Recess87(x,115,50,12);BtnFx48("page"+label,x,115,50,12,enabled);var po=Off48("page"+label,x,115,50,12);Tx74(label,x+po.x+(50-TW74(label))/2,118+po.y,enabled?S74_SAND:S74_DIM);bool c=Btn48("page"+label,x,115,50,12,enabled);if(c&&SinFeel1148On)HellordAudio.Play("altar_carousel");return c;}
        void Recess87(float x,float y,float w,float h)=>Atlas87(new Rect(131,92,55,20),new Rect(x,y,w,h),2);
        void Arrow87(float x,float y,int direction,Color c){for(int j=0;j<3;j++)Box74(x-j*direction,y-j,1,j*2+1,c);}
        bool ArrowButton87(float x,float y,float w,float h,bool enabled,int direction,string id="arrow")
        {Recess87(x,y,w,h);BtnFx48(id,x,y,w,h,enabled);var ao=Off48(id,x,y,w,h);Arrow87(x+ao.x+w/2,y+ao.y+h/2,direction,enabled?S74_SAND:S74_DIM);bool c=Btn48(id,x,y,w,h,enabled);if(c&&SinFeel1148On)HellordAudio.Play("altar_carousel");return c;}
        void CollectionPanel87()
        {
            Atlas87(new Rect(70,116,240,125),new Rect(70,114,240,128));
            Box74(73,117,234,122,new Color32(15,15,14,255));Box74(74,115,232,12,new Color32(28,27,24,255));Box74(75,127,230,1,new Color32(54,51,44,255));
            for(int r=0;r<4;r++)for(int c=0;c<8;c++)GUI.DrawTextureWithTexCoords(new Rect(U74(74+c*29),U74(128+r*28),U74(26),U74(26)),_s74bg,new Rect(74f/480,1-147f/270,26f/480,26f/270));
        }
        void Icon74(SinDef d, float x, float y, int sc)
        {   // approved v53 icon, integer screen scale; native-px position
            int ic = SinIconCol902(d.name); if (ic < 0 || _tSinIcons902 == null) return;
            // t1153: the Sins screen uses a copy of the v53 atlas with the baked navy (#262b44) plate stripped, so icons sit on the warm stone; HUD/drop cards keep the plated atlas
            if (!_sinIconsNPTried1153) { _sinIconsNPTried1153 = true; _tSinIconsNP1153 = UiTex("v53_sin_icons_20_noplate"); }
            var atlas = SinIconsNoPlate1153 && _tSinIconsNP1153 != null && _tSinIconsNP1153.width == _tSinIcons902.width ? _tSinIconsNP1153 : _tSinIcons902;
            float cell = Mathf.Max(1, HellordTuning.I.sinIconCell902), tw = atlas.width; GUI.color = Color.white;
            float px = 20f * sc * 700f / Screen.width * (Screen.width / 1280f) ; // icon size in panel units for sc screen-px-per-texel at 1280 wide
            px = 20f * sc * (700f / 1280f);
            GUI.DrawTextureWithTexCoords(new Rect(Mathf.Round(U74(x) * 1280f / 700f) * 700f / 1280f, Mathf.Round(U74(y) * 1280f / 700f) * 700f / 1280f, px, px), atlas, new Rect(ic * cell / tw, 0, cell / tw, 1));
        }
        Texture2D _tSinIconsNP1153; bool _sinIconsNPTried1153;
        /// <summary>t1153: off = old navy-plated icons on the Sins screen.</summary>
        public static bool SinIconsNoPlate1153 = true;
        float IconN74(int sc) => 20f * sc * (700f / 1280f) / K74; // icon size in native px

        void DrawSinPanel74()
        {
            if (DrawSinView56()) return; // t1159 view crop + pixel snap (-sinlayout1156) / t1175 kit view (default)
            SinLoad737(); Feel48Begin();
            _s74Click = null; if (_s74Fake.HasValue && Event.current.type == EventType.Repaint) { _s74Click = _s74Fake; _s74Fake = null; }
            R53(0, 0, 700, 392, new Color(0, 0, 0, 0.9f)); GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, 0, 700, 392), _s74bg);
            Fx84Bg(); SinEyes1154(); // t1154 guardian ember eyes
            if (SinLayout1156) { DrawSinPanel1156(); return; } // t1156 mockup only (-sinlayout1156)
            if (Kit98On && _v56) { DrawSinPanelK98(); return; } // t1175 v0.0.98 AI kit (default; -sinkit98off = v0.0.97 layout)
            Layout88();CollectionPanel87();
            var selC = S74_EMB; // step79: static selection (no pulse)
            // ---- equipped sockets (art 40x40 at x 78/126/174, y 46)
            int[] sx = { 78, 126, 174 }; const int sy = 46, ss = 40; string nextCost = null; bool nextOk = false;
            for (int i = 0; i < 3; i++)
            {
                float x = sx[i]; bool unl = i < _sinSlots737 && i < SlotsMax737;
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null;
                    if (d != null) { float n = IconN74(3); Icon74(d, x + (ss - n) / 2, sy + (ss - n) / 2, 3); if (_sinSel737 == d.id) Ol74(x + 6, sy + 6, ss - 12, ss - 12, selC); if (Hit74(x, sy, ss, ss)) _sinSel737 = d.id; }
                    else if (!UnlockReveal48(i, x, sy, ss)) Lbl74("EMPTY", x, sy, ss, ss, S74_DIM, 2);
                }
                else if (i < SlotsMax737)
                {
                    GUI.color = Color.white; Img74(_s74lock, x + DenyShake48(i), sy);
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    if (next) { nextCost = sc + " SOULS"; nextOk = afford; _s74NextSig = gc + " SIGIL" + (gc == 1 ? "" : "S"); if (!SinFeel1148On && afford && Over74(x, sy, ss, ss)) Ol74(x + 3, sy + 3, ss - 6, ss - 6, S74_GOLD); }
                    if (LockSocket48(i, x, sy, ss, next, afford, sc, gc)) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; Unlocked48(i); }
                }
            }
            // ---- planned ownership/balance/requirement block; amounts are exact, unabridged game values.
            Budget88();
            // ---- four EQUAL78x18 tabs, same original font, longest label on ONE line.
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };_s88TabsFit=true;
            for(int i=0;i<4;i++)
            {
                float x=74+i*82;bool sel=_sinFilter737==i;
                if(sel)Patch88(_s74on,new Rect(0,0,55,20),new Rect(x,94,78,18));else Atlas87(new Rect(131,92,55,20),new Rect(x,94,78,18));
                BtnFx48("tab"+i,x,94,78,18,true);var to=Off48("tab"+i,x,94,78,18);Lbl74(fl[i],x+to.x,94+to.y,78,18,sel?S74_EMB:S74_SAND,4);_s88TabsFit &= TW74(fl[i])<=70;
                if(Btn48("tab"+i,x,94,78,18,true)){if(!sel)HellordAudio.Play("ui_confirm");_sinFilter737=i;_sinPage737=0;_sinSel737=-1;}
            }
            // ---- collection (step77 = step76 A): 8x4 unchanged26px stone tiles, x74+c*29/y128+r*28, with an internal header rail
            var list = SinFiltered737(); const int cols = 8, rows = 4, per = cols * rows; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1);
            _s77Pages = pages; _s77Count = list.Count;
            float icn = IconN74(2);
            for (int k = 0; k < per; k++)
            {
                int li = _sinPage737 * per + k; if (li >= list.Count) break; var d = list[li];
                float x = 74 + (k % cols) * 29, y = 128 + (k / cols) * 28, w = 26, h = 26; bool own = _sinOwned737[d.id];
                if (own) Icon74(d, x + (w - icn) / 2, y + (h - icn) / 2, 2); else Lbl74("?", x, y, w, h, S74_DIM, 0);
                if (SinEquipped737(d.id)) Box74(x + w - 7, y + 4, 3, 3, S74_GOLD);
                if (_sinSel737 == d.id) Fx77Rim(x + 1, y + 1, w - 2, h - 2);
                if (Hit74(x, y, w, h)) { _sinSel737 = d.id; _s74Page = 0; }
            }
            // ---- right panel: big icon frame (interior ~ 340..380 x 65..105) + details (x 325..397, y 123..199)
            const float dX = 326, dW = 70, dY = 123, dH = 76; string act = null; bool actOn = false; int actKind = 0, free = -1;
            var lines = new List<KeyValuePair<string, Color>>();
            if (_sinSel737 >= 0)
            {
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id];
                // Selected icon remains intact as an action-plate badge, freeing the entire top bay for real data.
                Wrap74(lines, HSins.TierName[(int)d.tier] + "  NO." + (d.id + 1), dW, S74_RED);
                if (own)
                {
                    Wrap74(lines, d.name, dW, S74_GOLD);
                    // step77: Vietnamese alternate name (d.vn) kept in data, not shown in this English menu
                    var q = _sinRoll737[d.id]; Wrap74(lines, HSins.QualityTxt(q, d), dW, HSins.Quality(q) >= ST737.sinPerfectAt737 ? S74_GOLD : S74_SAND);
                    foreach (var l in HSins.Lines(d, q)) Wrap74(lines, l, dW, l.StartsWith("-") ? S74_RED : l.StartsWith("EFFECT NOT") ? S74_DIM : S74_SAND);
                    if (SinEquipped737(d.id)) { act = "UNEQUIP"; actOn = true; actKind = 1; }
                    else { for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; } act = _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; }
                }
                else { Wrap74(lines, "UNKNOWN SIN", dW, S74_SAND); Wrap74(lines, "Not found yet. Sins drop from demons and bosses.", dW, S74_DIM); act="NOT FOUND"; }
            }
            else {Wrap74(lines, "Select a sin.", dW, S74_DIM);act="SELECT A SIN";}
            // paging when the narrow column overflows (8.6 native px per line)
            const float lh = 8.6f; int cap = Mathf.FloorToInt(dH / lh); int capP = lines.Count > cap ? cap - 1 : cap;
            _s74Pages = Mathf.Max(1, (lines.Count + capP - 1) / capP); _s74Page = Mathf.Clamp(_s74Page, 0, _s74Pages - 1); _s74Lines = lines.Count; _s74Cap = capP; _s74Overflow = false;
            for (int i = 0; i < capP; i++) { int li = _s74Page * capP + i; if (li >= lines.Count) break; Tx74(lines[li].Key, dX, dY + i * lh, lines[li].Value); }
            if (_s74Pages > 1)
            {
                // Compact in-column text rail. No separate oversized count box.
                Box74(326,184,70,13,new Color32(22,22,20,255));Box74(326,184,70,1,new Color32(55,52,46,255));Box74(326,196,70,1,new Color32(8,8,7,255));
                string textPage="TEXT "+(_s74Page+1)+"/"+_s74Pages;Tx74(textPage,361-TW74(textPage)/2,188,S74_SAND);
                if(ArrowButton87(326,185,14,11,_s74Page>0,-1,"txtL"))_s74Page--;
                if(ArrowButton87(382,185,14,11,_s74Page<_s74Pages-1,1,"txtR"))_s74Page++;
            }
            // action button (baked red plate 325..394 x 204..233)
            if (act != null)
            {
                if(_sinSel737>=0){var preview=HSins.All[_sinSel737];if(_sinOwned737[preview.id])Icon74(preview,328,201,2);else Lbl74("?",328,201,15,15,S74_DIM,0);}
                BtnFx48("action",S90Action.x,S90Action.y,S90Action.width,S90Action.height,actOn);
                var acto=Off48("action",S90Action.x,S90Action.y,S90Action.width,S90Action.height);Lbl74(act,(_sinSel737>=0?346:325)+acto.x,S90Action.y+acto.y,_sinSel737>=0?46:69,S90Action.height,actOn?S74_SAND:S74_DIM,2);
                if (Btn48("action",S90Action.x,S90Action.y,S90Action.width,S90Action.height,actOn))
                {
                    var d = HSins.All[_sinSel737];
                    if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); if(SinFeel1148On)HellordAudio.Play("ui_back"); }
                    else if (free >= 0) { _sinEq737[free] = d.id; SinSave737(); if(SinFeel1148On)HellordAudio.Play("ui_confirm"); }
                }
            }
            // The heading is deliberately gone: its entire width now belongs to explicit page controls/count.
            string pageCount="PAGE "+(_sinPage737+1)+" OF "+pages;Tx74(pageCount,190-TW74(pageCount)/2,118,S74_SAND);
            if(PageButton88("PREV",78,_sinPage737>0))_sinPage737--;
            if(PageButton88("NEXT",252,_sinPage737<pages-1))_sinPage737++;
            // t1128: secondary exit is the aligned FOOTER of the right stone panel, not the floor.
            // Dedicated full-width exit plaque, with a readable destination and a 45px-high hit area at 1280.
            StonePlaque89(S90Back.x,S90Back.y,S90Back.width,S90Back.height,3);
            BtnFx48("back",S90Back.x,S90Back.y,S90Back.width,S90Back.height,true);
            Box74(328,224,63,1,new Color32(112,102,84,255));Box74(328,234,63,1,new Color32(10,9,8,255));
            var bo=Off48("back",S90Back.x,S90Back.y,S90Back.width,S90Back.height);Arrow87(330+bo.x,229+bo.y,-1,new Color32(238,225,194,255));Tx69("BACK TO MENU",U74(337+bo.x),U74(227+bo.y),new Color32(238,225,194,255),false); // t1151: exactly one glyph pass (no offset shadow copy that read as a 2nd label)
            if(BackBtn48(S90Back.x,S90Back.y,S90Back.width,S90Back.height)){SinClose87();return;}
            Confirm49Draw();
            if (_sinMsgT737 > 0 && !SinFeel1148On) Lbl74(_sinMsg737, 322, 242, 76, 18, S74_GOLD, 2); // t1148: header + socket carry it now (no duplicate under BACK)
        }
        string _s74NextSig = "";
        // ---- step77 shader FX (Resources/HellordShaders/HellordSinFX77.shader), drawn with Graphics.DrawTexture on Repaint only
        public static bool SinFx77On = true; public bool _s80Flip; readonly Dictionary<int, Material> _s80mat = new Dictionary<int, Material>(); public bool _s77FxOk; public int _s77Pages, _s77Count; Material[] _s77m; bool _s77tried;
        bool Fx77Ready()
        {
            if (!_s77tried)
            {
                _s77tried = true; var sh = Resources.Load<Shader>("HellordShaders/HellordSinFX77"); if (sh == null) sh = Shader.Find("Hellord/SinFX77");
                if (sh != null && sh.isSupported)
                {
                    _s77m = new Material[5];
                    for (int i = 0; i < 5; i++)
                    {
                        var m = new Material(sh) { hideFlags = HideFlags.DontSave }; m.SetFloat("_Mode", i);
                        bool add = i == 1 || i == 2; m.SetFloat("_SrcBlend", add ? 1 : 5); m.SetFloat("_DstBlend", add ? 1 : 10); _s77m[i] = m;
                    }
                    // step78: HFX procedural fire; flame-free backing (ui_s78_fire_backing = ui_s77_bg with painted flames replaced from same-row plate/bowl pixels)
                    var fb = UiTex("s78_fire_backing"); if (fb) { _s77m[0].SetTexture("_FireBacking", fb); _s77m[0].SetFloat("_UseCleanBacking", 1f); }
                    // step79 (HFX): talisman paper layer/backing for mode 4 + explicit fire/ember params (= shader defaults)
                    var pl = UiTex("s79_paper_layer"); var pb = UiTex("s79_paper_backing");
                    if (pl && pb) { _s77m[4].SetTexture("_PaperLayer", pl); _s77m[4].SetTexture("_PaperBacking", pb); _s77m[4].SetFloat("_PaperReady", 1f); _s77m[4].SetFloat("_PaperSway", 1.6f); }
                    Debug.Log("[step79] paper layer " + (pl && pb ? "ON" : "missing -> static talismans"));
                    _s77m[0].SetFloat("_FireHeight", 30f); _s77m[0].SetFloat("_FireWidth", 8f); _s77m[0].SetFloat("_FireBase", 5f); _s77m[0].SetFloat("_FireSpeed", 1f); _s77m[0].SetFloat("_FireFPS", 18f);
                    _s77m[1].SetFloat("_FireSpeed", 1f); _s77m[1].SetFloat("_FireFPS", 18f);
                    // step80 (HFX): source-driven 72-frame fire flipbook, opt-in only with clean backing
                    var ff = UiTex("s80_fire_flow72"); _s80Flip = ff && fb;
                    if (_s80Flip) { _s77m[0].SetTexture("_FireFlipbook", ff); _s77m[0].SetFloat("_FireFlipReady", 1f); }
                    Debug.Log("[step80] fire flipbook " + (_s80Flip ? "ON " + ff.width + "x" + ff.height + " filter " + ff.filterMode + " fmt " + ff.format + " paper " + (pl ? pl.format + "/" + pl.filterMode : "-") + " " + (pb ? pb.format + "/" + pb.filterMode : "-") : "OFF (missing atlas/backing)"));
                    Debug.Log("[step78] fire backing " + (fb ? "ON" : "missing -> painted atlas"));
                    _s77FxOk = true;
                }
                Debug.Log("[step77] SinFX77 shader " + (sh == null ? "MISSING" : sh.isSupported ? "supported" : "NOT supported"));
            }
            return _s77FxOk && SinFx77On;
        }
        void Fx77(int mode, float x, float y, float w, float h, float seed)
        {
            if (Event.current.type != EventType.Repaint || !Fx77Ready()) return;
            // step80: one material per (mode, seed) so each draw keeps its own phase even if IMGUI batches/defers DrawTexture
            var key = mode * 1000 + Mathf.RoundToInt(seed * 10); if (!_s80mat.TryGetValue(key, out var m)) { m = new Material(_s77m[mode]) { hideFlags = HideFlags.DontSave }; _s80mat[key] = m; }
            var src = new Rect(x / 480f, 1f - (y + h) / 270f, w / 480f, h / 270f);
            m.SetFloat("_T", Time.unscaledTime); m.SetFloat("_Seed", seed); m.SetVector("_Src", new Vector4(src.x, src.y, src.width, src.height)); m.SetVector("_Px", new Vector4(w, h, 0, 0));
            Graphics.DrawTexture(new Rect(U74(x), U74(y), U74(w), U74(h)), _s74bg, src, 0, 0, 0, 0, Color.white, m);
        }
        SinFx84Renderer _s84;bool _s84failed;public bool Sin84Ready => _s84!=null && _s84.Ready;
        void Fx84Bg(){if(Event.current.type!=EventType.Repaint||_s84failed)return;if(_s84!=null&&_s84L1246)DisposeSin84();if(_s84==null){SinFx84Renderer.Layout1246=false;_s84=new SinFx84Renderer(transform);}if(!_s84.Ready){_s84failed=true;return;}_s84.Frame(Time.unscaledTime);Graphics.DrawTexture(new Rect(0,0,700,392),_s74bg,new Rect(0,0,1,1),0,0,0,0,Color.white,_s84.Composite);}
        void DisposeSin84(){if(_s84!=null){_s84.Dispose();_s84=null;}_s84L1246=false;}
        void Fx77Bg()
        {
            Fx77(0, 16, 184, 28, 40, 0); Fx77(0, 434, 184, 28, 40, 1.7f);          // brazier flames + heat shimmer
            Fx77(1, 10, 112, 40, 108, 1f); Fx77(1, 428, 112, 40, 108, 2f);          // step79: 26 embers per side (rect ends y220)
            Fx77(4, 54, 6, 28, 42, 0f); Fx77(4, 162, 6, 28, 42, 1.7f); Fx77(4, 292, 6, 28, 42, 3.4f); Fx77(4, 404, 6, 28, 42, 5.1f); // talisman paper sway, pins fixed
        }
        void Fx77Rim(float x, float y, float w, float h)
        {
            Ol74(x, y, w, h, S74_EMB); // static outline; no hover or selection FX
        }
        void Wrap74(List<KeyValuePair<string, Color>> outL, string s, float w, Color c)
        {
            int m = Mathf.Max(6, Mathf.FloorToInt((U74(w) * K69 + 1) / 6)); string cur = "";
            foreach (var word in Sanitize(s).Split(' '))
            {
                if (cur.Length > 0 && cur.Length + 1 + word.Length > m) { outL.Add(new KeyValuePair<string, Color>(cur, c)); cur = word; }
                else cur = cur.Length > 0 ? cur + " " + word : word;
            }
            if (cur.Length > 0) outL.Add(new KeyValuePair<string, Color>(cur, c));
        }
    }
}
