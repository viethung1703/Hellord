// t561u + t534u (v0.0.53): Summon Skeleton Warriors / Summon Skeleton Archers.
// Skill levels drive HellordNecro536 minions (counts, damage, respawn, life). Art = VH-approved step37n sheets in Resources/HellordArt/Summon561
// (pivots/sizes from skel_*_pivots_sizes.json, baked below). Warrior: idle S/E/N, walk S/E/N, attack S/E, death S/E.
// Archer: idle S/N (no East by design), walk S/E/N, shoot S/E (brown wood bow). West = East flipped. No archer death sheet -> short fade of last frame.
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        // ---- skill -> numbers
        SkillInst NSkill561(int kind) { var id = kind == 0 ? SkillId.SkeletonWarriors : SkillId.SkeletonArchers; foreach (var s in skills) if (s.def.id == id) return s; return null; }
        int NSkillLv561(int kind) { var s = NSkill561(kind); return s == null ? 0 : s.level; }
        bool HasSummon561 { get { foreach (var s in skills) if (HData.Summon561(s.def)) return true; return false; } }
        float NBaseHp561(int kind) { int lv = Mathf.Max(1, NSkillLv561(kind)); return (kind == 0 ? PT.skelWarHp561 : PT.skelArcHp561) * (1f + PT.skelHpPerLv561 * (lv - 1)) * _sinMinionMul737 * SinMinHp818(kind); }
        float NBaseDmg561(int kind) { int lv = NSkillLv561(kind); if (lv <= 0) return PT.minionDmg536 * _sinMinionMul737 * SinMinDmg818(kind); return HData.At(kind == 0 ? PT.skelWarDmg561 : PT.skelArcDmg561, lv, 6f) * _sinMinionMul737 * SinMinDmg818(kind); }
        float NResp561(int kind) { int lv = NSkillLv561(kind); if (lv <= 0) return PT.minionRespawn536 / SinResp818(); return HData.At(kind == 0 ? PT.skelWarResp561 : PT.skelArcResp561, lv, 6f) / SinResp818(); } // t818u #28
        string SummonLvText561(SkillDef d, int lv)
        {
            bool w = d.id == SkillId.SkeletonWarriors; int n = HData.At(w ? PT.skelWarCount561 : PT.skelArcCount561, lv, 1);
            float dm = HData.At(w ? PT.skelWarDmg561 : PT.skelArcDmg561, lv, 5f), r = HData.At(w ? PT.skelWarResp561 : PT.skelArcResp561, lv, 6f);
            return $"Lv{lv}: {n} {(w ? "warriors" : "archers")} · {dm:0} dmg · respawn {r:0.#}s";
        }
        string SummonStatus561() { string a = ""; foreach (var s in skills) if (HData.Summon561(s.def)) a += s.def.name + " Lv" + s.level + "  "; return a + "| warriors " + NCount536(0, false) + (NCount536(0, true) > 0 ? "+" + NCount536(0, true) + " temp" : "") + " archers " + NCount536(1, false) + " | minions " + _nmin536.Count + "/" + NCap536(); }

        // ---- baseline bullet block
        void Block561(NMin536 m)
        {
            float r = PT.minionBlockR561; Vector2 c = m.p + Vector2.up * 0.5f;
            for (int i = _shots44.Count - 1; i >= 0; i--)
            {
                var s = _shots44[i]; if (s.kind == 2) continue; // rolling fire wheel is not a bullet
                if ((s.p - c).magnitude < s.r + r) { FxSpark561(s.p); if (!SinFreeBlock818(m)) m.hp -= s.dmg * PT.minionBulletHurt561; EndShot44(s, true); _shots44.RemoveAt(i); }
            }
            if (_bb != null) foreach (var b in _bb) if (b.alive && (b.pos - c).magnitude < BBulletR + r) { FxSpark561(b.pos); m.hp -= BBulletDmg * PT.minionBulletHurt561; KillBB(b, true); }
        }

        // ---- art
        class Anim561 { public Sprite[] f; public float fps; }
        Dictionary<string, Anim561> _an561; bool _anTried561;
        Anim561 A561(string key) { if (_an561 == null) Load561(); Anim561 a; return _an561.TryGetValue(key, out a) ? a : null; }
        void Load561()
        {
            _an561 = new Dictionary<string, Anim561>();
            // values from skel_warrior_pivots_sizes.json / skel_archer_pivots_sizes.json (step37n export)
            Sheet561("skel_warrior_all", 42, 38, 20, 35, new[] { "idle_south:8", "idle_east:8", "idle_north:8", "walk_south:12", "walk_east:12", "walk_north:12", "attack_south:12", "attack_east:12", "death_south:10", "death_east:10" });
            Sheet561("skel_archer_all", 38, 39, 19, 35, new[] { "idle_south:8", "idle_north:8", "walk_south:12", "walk_east:12", "walk_north:12", "shoot_south:12", "shoot_east:12" });
        }
        void Sheet561(string pre, int fw, int fh, int px, int py, string[] rows)
        {
            foreach (var r in rows)
            {
                var kv = r.Split(':'); var tx = Resources.Load<Texture2D>("HellordArt/Summon561/" + pre + "_" + kv[0]); if (tx == null) continue;
                tx.filterMode = FilterMode.Point; int n = Mathf.Max(1, tx.width / fw); var a = new Anim561 { f = new Sprite[n], fps = float.Parse(kv[1]) };
                for (int i = 0; i < n; i++) a.f[i] = Sprite.Create(tx, new Rect(i * fw, 0, fw, fh), new Vector2(px / (float)fw, 1f - py / (float)fh), 16f);
                _an561[pre + "_" + kv[0]] = a;
            }
        }
        readonly List<SpriteRenderer> _skSr561 = new List<SpriteRenderer>(); int _skN561;
        SpriteRenderer SR561()
        {
            if (_skN561 >= _skSr561.Count) _skSr561.Add(NewSR("skel561", null, null, 0));
            var sr = _skSr561[_skN561++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true); return sr;
        }
        void HideSk561() { for (int i = _skN561; i < _skSr561.Count; i++) if (_skSr561[i] != null && _skSr561[i].gameObject.activeSelf) _skSr561[i].gameObject.SetActive(false); _skN561 = 0; }
        static string Dir561(Vector2 d, bool hasEast, out bool flip) { flip = d.x < 0; if (hasEast && Mathf.Abs(d.x) > Mathf.Abs(d.y) * 1.1f) return "east"; return d.y > 0 ? "north" : "south"; }
        bool Put561(Anim561 a, float t, bool loop, Vector2 p, bool flip, Color c)
        {
            if (a == null || a.f.Length == 0) return false; int fr = (int)(t * a.fps); fr = loop ? fr % a.f.Length : Mathf.Min(fr, a.f.Length - 1);
            var sr = SR561(); sr.sprite = a.f[fr]; sr.flipX = flip; sr.color = c; sr.transform.position = p; sr.transform.rotation = Quaternion.identity;
            sr.transform.localScale = Vector3.one * PT.skelArtScale561; sr.sortingOrder = YSort(p.y); return true;
        }
        // returns false if art is missing -> caller draws the old Spr53 placeholder
        // t602u: direction with hysteresis -> 0 south, 1 east, 2 north; flip only on a clear left/right
        string DirH602(NMin536 m, Vector2 d, bool hasEast, out bool flip)
        {
            if (d.sqrMagnitude > 0.0001f)
            {
                float ax = Mathf.Abs(d.x), ay = Mathf.Abs(d.y), l = d.magnitude; int k = m.dirK;
                if (k < 0) k = ax > ay ? 1 : d.y > 0 ? 2 : 0;
                else if (k == 1) { if (ay > ax * PT.minionDirHyst602) k = d.y > 0 ? 2 : 0; }
                else { if (ax > ay * PT.minionDirHyst602) k = 1; else if (k == 0 && d.y > 0.35f * l) k = 2; else if (k == 2 && d.y < -0.35f * l) k = 0; }
                m.dirK = k; if (ax > 0.3f * l) m.dirFlip = d.x < 0;
            }
            flip = m.dirFlip; int kk = m.dirK < 0 ? 0 : m.dirK; if (!hasEast && kk == 1) kk = m.face.y > 0 ? 2 : 0;
            return kk == 1 ? "east" : kk == 2 ? "north" : "south";
        }
        bool Skel561(NMin536 m, int idx, float dt)
        {   // t602u: anim picks from INTENT (brain walk / attack timers), not from frame-to-frame displacement (separation & enemy pushes used to flicker walk/idle and S/E/N)
            string pre = m.kind == 0 ? "skel_warrior_all_" : "skel_archer_all_";
            m.walkHold -= dt; bool moving = m.walkHold > 0f;
            if (m.wantFace.sqrMagnitude > 0.0001f) m.face = m.wantFace;
            if (moving) { int f0 = (int)(m.moveT * 12f) % 6; m.moveT += dt; int f1 = (int)(m.moveT * 12f) % 6; if (f1 != f0 && (f1 == 0 || f1 == 3)) FxDust561(m.p, m.face); } m.prev = m.p;
            Color c = m.temp ? new Color(0.75f, 0.95f, 1f) : Color.white; if (m.hurtT > 0) c = Color.Lerp(c, new Color(1f, 0.45f, 0.4f), 0.7f); // hurt = short tint flash, anim keeps playing
            bool flip; float ph = idx * 0.37f;
            float ao = m.kind == 0 ? 4f / 12f : 0f; // warrior: anim joins at swing frame 4 so the downslash reads on damage frame 0 (FX SPEC v2)
            float at = Time.time - m.atkT + ao;
            Anim561 atk = A561(pre + (m.kind == 0 ? "attack_" : "shoot_") + "south");
            if (atk != null && at < atk.f.Length / atk.fps)
            {   // locked: facing frozen at the moment the attack started
                bool af = m.atkFace.x < 0; string ad = Mathf.Abs(m.atkFace.x) > Mathf.Abs(m.atkFace.y) ? "east" : "south";
                var a2 = A561(pre + (m.kind == 0 ? "attack_" : "shoot_") + ad) ?? atk; return Put561(a2, at, false, m.p, af, c);
            }
            string dk = DirH602(m, m.face, true, out flip);
            if (moving) return Put561(A561(pre + "walk_" + dk), m.moveT, true, m.p, flip, c);
            string ik = m.kind == 0 ? dk : (dk == "east" ? (m.face.y > 0 ? "north" : "south") : dk);
            return Put561(A561(pre + "idle_" + ik), Time.time + ph, true, m.p, m.kind == 0 && flip, c);
        }
        // ---- death playback (art only; the minion is already removed and its respawn timer started)
        struct Dead561 { public Vector2 p, face; public int kind; public float t; }
        readonly List<Dead561> _dead561 = new List<Dead561>();
        void Dead561Tick(float dt)
        {
            for (int i = _dead561.Count - 1; i >= 0; i--)
            {
                var d = _dead561[i]; d.t += dt; bool flip; float life;
                if (d.kind == 0) { var a = A561("skel_warrior_all_death_" + Dir561(d.face, true, out flip).Replace("north", "south")); life = a != null ? a.f.Length / a.fps + 0.4f : 0f; if (d.t < life) Put561(a, d.t, false, d.p, flip, new Color(1, 1, 1, Mathf.Clamp01((life - d.t) / 0.4f))); }
                else { var a = A561("skel_archer_all_idle_" + Dir561(d.face, false, out flip)); life = 0.35f; if (a != null && d.t < life) Put561(a, 0f, false, d.p + Vector2.down * (Mathf.Floor(d.t / life * 3f) + 1f) * 6f / 16f, false, Color.white); } // FX v7: 3 stepped 6px sink, no alpha
                if (d.t >= life) _dead561.RemoveAt(i); else _dead561[i] = d;
            }
        }
    }
}
