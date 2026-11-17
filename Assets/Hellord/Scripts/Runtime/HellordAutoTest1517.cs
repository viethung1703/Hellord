#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1517 LMB basic clip scenario ("lmb1517"). Env AT_SKILL=SkillId name (the basic, slot 1). 300 frames at 30 fps: the scripted cursor
    /// circles the hero; LMB held f20-110, released f110-150 (must not fire), taps at f150 and f160/f172 (the 2nd/3rd are during the cooldown =
    /// not-ready clicks), held again f200-270, released to the end. Sparse imps. Frames to AtRecFrame, sfx to audio_events.txt.</summary>
    public partial class HellordGame
    {
        int _r1517, _poseShown1517, _walkCast1528, _pops1517; System.Text.StringBuilder _cur1517 = new System.Text.StringBuilder();
        IEnumerator AtLmb1517()
        {
            var id = (SkillId)System.Enum.Parse(typeof(SkillId), System.Environment.GetEnvironmentVariable("AT_SKILL") ?? "Fireball"); string clip = "lmb1517_" + id;
            _atGod = true; maxHp = hp = 100000; Manual716 = false; HellordTuning.I.camResHud636 = false;
            AtKeepPlay(); AtClearWorld(); ClearIce(); AtSetSkills((id, 3)); yield return AtFrames(60); AtKeepPlay(); AtClearWorld(); AtSetSkills((id, 3));
            AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; _r1517 = 0; _cur1517.Length = 0; _pops1517 = 0; _walkCast1528 = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            HInput.AtLmb = false; Vector2 c0 = _heroPos; atMouse986 = c0 + new Vector2(4f, 0f); SnapCamera();
            var list = new List<Enemy>(); float H = 1.46f;
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= 0.08f; e.hp = e.maxHp = 60; list.Add(e); } }
            Mk(3.2f * H, 0.6f); Mk(4.3f * H, -1.2f); Mk(-3.4f * H, 0.8f); Mk(0.6f, 3.4f); Mk(-1.2f, -3.6f);
            string walkEnv = System.Environment.GetEnvironmentVariable("AT_WALK"); bool walk = walkEnv == "1" || walkEnv == "2", walk2 = walkEnv == "2"; int casts = 0, castsReleased = 0, notReady = 0; float lastCd = 0; int firstCastF = -1; bool heldPrev = false; // the cast lands in the Update after the frame LMB was set, so judge it against the previous frame's LMB
            for (int f = 0; f < 300; f++)
            {
                yield return null; AtFrameChecks(); AtAudioLog1481.Frame = _r1517; if (mode != Mode.Play) mode = Mode.Play; hp = Mathf.Max(hp, maxHp);
                float a = f * 0.035f; float rr = 3.6f + 0.8f * Mathf.Sin(f * 0.05f); atMouse986 = _heroPos + new Vector2(Mathf.Cos(a) * rr * 1.25f, Mathf.Sin(a) * rr * 0.75f + 0.5f);
                bool held = f >= 20 && f < 110 || f >= 150 && f < 152 || f >= 160 && f < 162 || f >= 172 && f < 174 || f >= 200 && f < 270; HInput.AtLmb = held; if (walk) AtMove1396 = f >= 200 && f < 270 ? new Vector2(f < 235 ? 1f : -1f, 0.3f).normalized : walk2 && f >= 20 && f < 110 ? new Vector2(Mathf.Cos(f * 0.05f), Mathf.Sin(f * 0.05f) * 0.6f).normalized : Vector2.zero; // AT_WALK=1: walk while casting f200-270; AT_WALK=2: also f20-110 (a slow loop)
                var sk = skills.Find(x => x.def.id == id); if (sk != null && sk.cd > lastCd + 0.05f) { casts++; if (!heldPrev) castsReleased++; if (firstCastF < 0) firstCastF = f; } if (sk != null) lastCd = sk.cd;
                if (HeroPose1517(out _) != null) { _poseShown1517++; if (_walkF1528 >= 0 && _castWalk1528 != null) _walkCast1528++; } { var hp1517 = WorldToHud(MouseWorld()); _cur1517.AppendLine($"{_r1517} {hp1517.x:0} {hp1517.y:0} {(sk != null ? sk.cd : -1):0.000} {_readyFlash1517:0.000} {(held ? 1 : 0)}"); if (_readyFlash1517 > 0f) _pops1517++; } heldPrev = held; AtRecFrame(clip, ref _r1517);
            }
            HInput.AtLmb = false; AtMove1396 = Vector2.zero; var log = AtAudioLog1481.Sb.ToString(); foreach (var l in log.Split('\n')) if (l.Contains("basic_not_ready")) notReady++;
            System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "audio_events.txt"), log); System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "cursor_hud.txt"), _cur1517.ToString()); // frame hudX hudY(640x360, top-left) cd readyFlash lmb
            ARes("lmb1517", casts >= 4 && castsReleased == 0 && firstCastF >= 20 && firstCastF <= 22, $"skill {id} basic {(_basic1517 != null ? _basic1517.def.id.ToString() : "none")} casts {casts} castsWhileReleased {castsReleased} firstCastFrame {firstCastF} (LMB from 20) notReadyClicks {notReady} frames {_r1517} castBody {(_castBody1528 != null ? _castBody1528.Length : -1)} aura {(_auraFire1528 != null)} spark {(_sparkFrost1528 != null)} poseFramesShown {_poseShown1517} walkCastFrames {_walkCast1528} readyPopFrames {_pops1517} poseCalls {_poseCalls1517} hero47 {Hero47On} poseOn {HellordTuning.I.heroPose1517} poseT {_poseT1517:0.00}");
            HellordAudio.LoopLog = null; AtClearWorld(); yield return AtFrames(5);
        }
    }
}
#endif
