#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;
namespace Hellord
{
    /// <summary>t1736 Zhong Kui kit clips (run with -atRec 20). Captions via Toast.
    /// nkclip1736_basics: Soul Spears, Teeth, Skeleton Warriors, Skeleton Archers.
    /// nkclip1736_special: Decrepify, Soul Plague, Skeletal Mage, Jiangshi (Clay Golem slot), Ghost Step, Bone Armour, Bone Wall.
    /// nkclip1736_ult: Ossuary Nova, Bone Colossus, Grave Legion.
    /// Tight beats so each file stays under 1 MB. Not a functional PASS/FAIL suite — visual record only.</summary>
    public partial class HellordGame
    {
        IEnumerator AtNkClip1736(int which)
        {
            string[] names = { "nkclip1736_basics", "nkclip1736_special", "nkclip1736_ult" };
            string scen = names[which], clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.SoulSpears);
            _pactPath51 = 1; T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true;
            T.nkMove1713 = T.nkSpecial1713 = T.nkUlt1713 = 0; ClearNk1713(); NkRunReset1713();
            var L = new List<Enemy>(); Vector2 H0 = _heroPos; ZhFxLogOn1625 = true; ZhFxLogReset1625();
            Enemy Pack(EType t, Vector2 at, float speed, float hpv = 800f) => ZMk1618(L, t, at, speed, hpv);
            SkillInst Basic(SkillId id, string name) { var s = new SkillInst { def = HData.Skill(id) }; if (skills.Count > 0) skills[0] = s; else skills.Add(s); Toast(name); return s; }
            IEnumerator LF(int n) { for (int i = 0; i < n; i++) { yield return ZF1618(1); pendingLevels = 0; if (mode != Mode.Play) mode = Mode.Play; foreach (var p in _pk) if (p.alive) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); } } }
            IEnumerator Hold(int n) { HInput.AtLmb = true; yield return LF(n); HInput.AtLmb = false; }
            void Res(string k, bool ok, string d) => ARes(scen + ":" + k, ok, d);
            void Reset() { ZKill1618(L); _nkColCdPend1772 = 0f; NkEndColossus1713(false); _nkLegionT1772 = 0f; ClearNk1713(); _heroPos = H0; SnapCamera(); ZhMoveCd = ZhSpecCd = ZhUltCd = 0f; HInput.AtLmb = false; }
            void PackFront(int n = 6) { for (int i = 0; i < n; i++) Pack(EType.Imp, H0 + new Vector2(2.2f + (i % 3) * 0.7f, -0.8f + (i / 3) * 0.8f), 0f); }
            void FillMinions(int n)
            {
                while (_nmin536.Count < n && _nmin536.Count < NCap536())
                {
                    int kind = _nmin536.Count % 3 == 2 ? 1 : 0;
                    NSpawn536(kind, false, _heroPos + new Vector2(0.8f * ((_nmin536.Count % 4) - 1.5f), 0.6f * (_nmin536.Count / 4)));
                }
            }
            bool on = NkOn1713; Res("active", on, $"NkOn1713 {on} (kit1618 {T.kit1618}, nkKit1713 {T.nkKit1713}, path {_pactPath51})");
            yield return LF(10);

            if (which == 0)
            {
                Basic(SkillId.SoulSpears, "SOUL SPEARS"); PackFront(); atMouse986 = H0 + new Vector2(4f, 0f); yield return LF(4); skills[0].cd = 0f; yield return Hold(4); yield return LF(22);
                Reset(); Basic(SkillId.Teeth, "TEETH"); PackFront(); atMouse986 = H0 + new Vector2(4f, 0f); yield return LF(4); skills[0].cd = 0f; yield return Hold(4); yield return LF(22);
                Reset(); Basic(SkillId.SkeletonWarriors, "SKELETON WARRIORS"); PackFront(); FillMinions(4);
                atMouse986 = H0 + new Vector2(3.5f, 0f); yield return LF(8); skills[0].cd = 0f; yield return Hold(3); yield return LF(28);
                Reset(); Basic(SkillId.SkeletonArchers, "SKELETON ARCHERS"); PackFront();
                for (int i = 0; i < 3; i++) NSpawn536(1, false, _heroPos + new Vector2(-0.6f + i * 0.6f, 0.8f));
                atMouse986 = H0 + new Vector2(3.5f, 0f); yield return LF(8); skills[0].cd = 0f; yield return Hold(3); yield return LF(32);
                Res("basics", true, "soul spears / teeth / warriors / archers shown");
            }
            else if (which == 1)
            {
                Basic(SkillId.SoulSpears, "ZHONG KUI SPECIAL + MOVE"); PackFront();
                T.nkSpecial1713 = 1; Toast("DECREPIFY"); atMouse986 = H0 + new Vector2(3f, 0f); ZhSpecCd = 0f; yield return LF(4); NkSpecial1713(); yield return LF(28);
                Reset(); PackFront(); T.nkSpecial1713 = 2; Toast("SOUL PLAGUE"); atMouse986 = H0 + new Vector2(2.4f, 0f); ZhSpecCd = 0f; yield return LF(4); NkSpecial1713(); yield return LF(18);
                // kill the marked one so the spread can read
                foreach (var e in L) if (e.alive && e.nkPlague1713 > runTime) { e.hp = 1f; Hit(e, 50f, ZkI1651(NkK1713(1, 2)), Vector2.zero); break; }
                yield return LF(20);
                Reset(); PackFront(); T.nkSpecial1713 = 3; Toast("SKELETAL MAGE"); atMouse986 = H0 + new Vector2(2f, 0.4f); ZhSpecCd = 0f; yield return LF(4); NkSpecial1713(); yield return LF(36);
                Reset(); PackFront(); T.nkSpecial1713 = 4; Toast("JIANGSHI"); atMouse986 = H0 + new Vector2(2.2f, 0f); ZhSpecCd = 0f; yield return LF(4); NkSpecial1713(); yield return LF(16);
                ZhSpecCd = 0f; NkSpecial1713(); yield return LF(24); // cap press = slam
                // movement
                Reset(); PackFront(4); T.nkMove1713 = 1; Toast("GHOST STEP"); _heroPos = H0 + new Vector2(-2f, 0f); SnapCamera(); atMouse986 = _heroPos + new Vector2(8f, 0f); yield return LF(4); yield return ZDash1618(Vector2.right); yield return LF(18);
                Reset(); PackFront(4); T.nkMove1713 = 2; Toast("BONE ARMOUR"); _heroPos = H0; SnapCamera(); atMouse986 = H0 + new Vector2(4f, 0f); yield return LF(4); yield return ZDash1618(Vector2.right); yield return LF(24);
                Reset(); PackFront(4); T.nkMove1713 = 3; Toast("BONE WALL"); _heroPos = H0 + new Vector2(-2f, 0f); SnapCamera();
                for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(-5f, -1f + i * 0.7f), 2.2f, 4000f);
                atMouse986 = H0 + new Vector2(6f, 0f); yield return LF(4); yield return ZDash1618(Vector2.right); yield return LF(32);
                Res("special", true, "curses, mage, golem, three movement skills shown");
            }
            else
            {
                Basic(SkillId.SoulSpears, "ZHONG KUI ULTIMATES");
                T.nkUlt1713 = 1; NkFillUlt1713(); Toast("OSSUARY NOVA"); PackFront(8); atMouse986 = H0 + new Vector2(3f, 0f); ZhUltCd = 0f; yield return LF(4); NkUltimate1713(); yield return LF(28);
                Reset(); T.nkUlt1713 = 2; FillMinions(6); PackFront(8); NkFillUlt1713(); Toast("BONE COLOSSUS"); atMouse986 = H0 + new Vector2(4f, 0f); ZhUltCd = 0f; yield return LF(4); NkUltimate1713(); yield return LF(50);
                Reset(); T.nkUlt1713 = 3; FillMinions(12); PackFront(8); NkFillUlt1713(); Toast("GRAVE LEGION"); atMouse986 = H0 + new Vector2(3f, 0f); ZhUltCd = 0f; yield return LF(4); NkUltimate1713(); yield return LF(44);
                Res("ult", true, "five ultimates shown");
            }
            T.kitSlotOverride1639 = false; T.nkMove1713 = T.nkSpecial1713 = T.nkUlt1713 = 0;
            yield return LF(6);
            _zClip = null; ZhEnd1618(scen, clip);
        }
    }
}
#endif
