using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // t981u v0.0.87: hero size must stay the same for the whole run (VH: hero grew after the 2nd skill, Archers then Spears).
    public partial class HellordGame
    {
#if HELLORD_AUTOTEST
        string HeroSizeInfo987(out float baseW, out float allW, out float scrPx)
        {
            baseW = _heroBase.bounds.size.x; var b = _heroBase.bounds;
            foreach (var l in _heroLayers) if (l.sr && l.sr.enabled && l.sr.sprite) b.Encapsulate(l.sr.bounds);
            allW = b.size.x; scrPx = baseW * (ViewPxH / (2f * _cam.orthographicSize));
            return $"baseW {baseW:0.000} allW {allW:0.000} scl {_hero.lossyScale.x:0.000}/{_heroBase.transform.lossyScale.x:0.000} ortho {_cam.orthographicSize:0.000} rt {(_rt ? _rt.width + "x" + _rt.height : "-")} scrPx {scrPx:0.0} spr {(_heroBase.sprite ? _heroBase.sprite.name + "@" + _heroBase.sprite.pixelsPerUnit + " " + _heroBase.sprite.rect.size : "-")} mat {(_heroBase.sharedMaterial ? _heroBase.sharedMaterial.name : "-")} layers {_heroLayers.Count}";
        }
        IEnumerable<SpriteRenderer> AllHeroSr987() { yield return _heroBase; foreach (var l in _heroLayers) if (l.sr && l.sr.sprite) yield return l.sr; }
        System.Collections.IEnumerator AtSize987()
        {
            string clip = "size987_archer_then_spear"; yield return AtBegin(clip); LoadCtrl716(); bool oMan = Manual716; Manual716 = true;
            AtClearWorld(); AtSetSkills(); Vector2 c0 = RoomCenterFeet(); AtHeroTo(c0); SnapCamera(); maxHp = hp = 100000; _atGod = false; _iframe = 0;
            yield return AtWait(0.4f); int recI = 0;
            string i0 = HeroSizeInfo987(out float w0, out _, out float s0); ALog("size987 start " + i0);
            float maxDevW = 0, maxDevS = 0; string worst = i0; int ifFrames = 0, texBad = 0, shared = 0;
            System.Action<string> sample = tag => { var inf = HeroSizeInfo987(out float w, out _, out float s); float dw = Mathf.Abs(w - w0), ds = Mathf.Abs(s - s0); if (dw > maxDevW || ds > maxDevS) { worst = tag + ": " + inf; } maxDevW = Mathf.Max(maxDevW, dw); maxDevS = Mathf.Max(maxDevS, ds); };
            foreach (var id in new[] { SkillId.SkeletonArchers, SkillId.SoulSpears })
            {
                mode = Mode.LevelUp; Choose(new Card { kind = CardKind.NewSkill, skill = HData.Skill(id) });
                if (_slotPick716 != null) { for (int i = 0; i < BeltN716; i++) if (_slot716[i] == null) { PutSlot716(i); break; } }
                float t = 0; int hitN = 0;
                while (t < 3f)
                {
                    yield return null; AtFrameChecks(); t += Time.deltaTime; hp = maxHp;
                    if (hitN < 2 && t > 0.6f + hitN * 1.2f && _iframe <= 0) { HurtHero(5f, "at_size987"); hitN++; }
                    sample(id + " t" + t.ToString("0.00"));
                    if (IfMatOn987(_heroBase))
                    {
                        ifFrames++; var seen = new HashSet<Material>();
                        foreach (var r in AllHeroSr987()) { if (!IfMatOn987(r)) continue; var tx = r.sprite.texture; var v = r.sharedMaterial.GetVector("_TexelPx"); if (Mathf.Abs(v.x * tx.width - 1f) > 1e-3f || Mathf.Abs(v.y * tx.height - 1f) > 1e-3f) texBad++; if (!seen.Add(r.sharedMaterial)) shared++; }
                    } AtRecFrame(clip, ref recI);
                }
                ALog("size987 after " + id + " " + HeroSizeInfo987(out _, out _, out _));
            }
            ARes("size987:iframe_texel_per_layer", ifFrames > 0 && texBad == 0 && shared == 0, $"iframe frames {ifFrames} texel mismatches {texBad} shared mats {shared} layers {_heroLayers.Count}");
            ARes("size987:hero_same_size", maxDevW < 0.01f && maxDevS < 0.5f, $"start [{i0}] maxDev world {maxDevW:0.000} screenPx {maxDevS:0.00} worst [{worst}]");
            _atGod = true; _iframe = 0; AtSetSkills(); Manual716 = oMan; AtEnd(clip);
        }
#endif
    }
}
