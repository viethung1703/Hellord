#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1511b: sfx log for the fire1481 clip (frame clip volume pitch), mixed offline into the preview audio.</summary>
    public static class AtAudioLog1481
    {
        public static System.Text.StringBuilder Sb = new System.Text.StringBuilder(); public static int Frame = -1;
        public static void Log(string clip, float vol, float pitch) { if (Frame >= 0) Sb.AppendLine(Frame + " " + clip + " " + vol.ToString("0.000") + " " + pitch.ToString("0.000")); }
    }
    /// <summary>t1481 / t1511 fire basics clip scenario ("fire1481"). Env: AT_FB=0 Molten Spit | 1 Infernal Boulder | 2 Fire Pillar; AT_CLEAN=1 no enemies; AT_ONCE=1 a single cast east; AT_NOGLOW=1 no additive glow; AT_HEROX=<m> moves the hero from the room centre (negative = west: the camera then stops at the wall, so a full 184 px Boulder roll fits the 320 px view); AT_SPARSE=1 few imps (3 east, 2 west); AT_EAST=1 aims east only (Boulder: no west group); AT_FBDARK=0 / AT_DECLUTTER=0 turn the t1511b floor darkening / declutter off for A-B clips.
    /// Imps start 3-5 hero heights away, deal no damage and walk at 8% speed; held LMB from frame 20 to 215, aiming east then west from frame 120. Frames go to AtRecFrame, sfx to audio_events.txt.</summary>
    public partial class HellordGame
    {
        int _r1481; string _clip1481;
        IEnumerator AtFire1481()
        {
            int k = int.Parse(System.Environment.GetEnvironmentVariable("AT_FB") ?? "0"); SkillId id = SkillId.MagmaThrow + k; _atGod = true; maxHp = hp = 100000; var T = HellordTuning.I; T.camResHud636 = false;
            if (System.Environment.GetEnvironmentVariable("AT_NOGLOW") == "1") FireBasicsFX1481.ForceNoGlow = true; bool clean = System.Environment.GetEnvironmentVariable("AT_CLEAN") == "1"; if (System.Environment.GetEnvironmentVariable("AT_FBDARK") == "0") T.fbDark1481 = false; if (System.Environment.GetEnvironmentVariable("AT_DECLUTTER") == "0") { T.fbDeclutter1481 = false; T.fbNoHitBlob1481 = false; } string clip = "fb" + k; _clip1481 = clip; yield return AtBegin(clip);
            AtKeepPlay(); AtClearWorld(); ClearIce(); AtSetSkills((id, 3)); float hx = float.Parse(System.Environment.GetEnvironmentVariable("AT_HEROX") ?? "0", System.Globalization.CultureInfo.InvariantCulture); bool eastOnly = System.Environment.GetEnvironmentVariable("AT_EAST") == "1"; Vector2 c0 = RoomCenterFeet() + new Vector2(hx, 0f); ClampToRoom(ref c0, 8); AtHeroTo(c0); SnapCamera();
            yield return AtFrames(60); AtKeepPlay(); AtClearWorld(); AtSetSkills((id, 3));
            AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; _r1481 = 0; HellordAudio.LoopLog = (kind, id, a, b) => AtAudioLog1481.Log(kind == "play" ? id : kind + "|" + id, a, b); HInput.AtLmb = false; atMouse986 = _heroPos + new Vector2(5f, 0.5f);
            // targets: row to the east (rock), clusters (lob / pillar)
            var list = new List<Enemy>();
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= 0.08f; e.hp = e.maxHp = 60; list.Add(e); } }
            float H = 1.46f; // t1511: enemies start 3-5 hero heights away (hero = 1.46 m)
            bool sparse = System.Environment.GetEnvironmentVariable("AT_SPARSE") == "1"; // t1514: few imps (3 east, 2 west), like the PoE2 tutorial
            if (clean) { } else if (sparse) { Mk(3.2f * H, 0.4f); Mk(4.2f * H, -0.8f); Mk(5f * H, 1.1f); if (!eastOnly) { Mk(-3.4f * H, 0.3f); Mk(-4.4f * H, -0.9f); } } else if (k == 1) { for (int i = 0; i < 6; i++) Mk(3f * H + i * 0.75f, 0.5f + (i % 2) * 0.3f); if (!eastOnly) for (int i = 0; i < 5; i++) Mk(-3f * H - i * 0.75f, 0.2f + (i % 2) * 0.3f); }
            else { for (int i = 0; i < 7; i++) Mk(3.2f * H + (i % 4) * 0.6f + Random.Range(-0.2f, 0.2f), 0.5f - 1.5f + i * 0.5f); for (int i = 0; i < 6; i++) Mk(-3.2f * H - (i % 3) * 0.7f + Random.Range(-0.2f, 0.2f), 1f - 1.25f + i * 0.5f); }
            int casts = 0; float lastCd = 0; bool once = System.Environment.GetEnvironmentVariable("AT_ONCE") == "1";
            for (int f = 0; f < 240; f++)
            {
                yield return null; AtFrameChecks(); AtAudioLog1481.Frame = _r1481; if (mode != Mode.Play) mode = Mode.Play; hp = Mathf.Max(hp, maxHp);
                bool west = !once && !eastOnly && f >= 120; if (k == 1) atMouse986 = _heroPos + new Vector2(west ? -5f : 5f, west ? 0.2f : 0.6f); else atMouse986 = _heroPos + (west ? new Vector2(-5.6f, 1f) : new Vector2(5.8f, 0.5f));
                HInput.AtLmb = once ? (f >= 20 && f < 22) : (f >= 20 && f < 215);
                var sk = skills.Find(x => x.def.id == id); if (sk != null && sk.cd > lastCd + 0.05f) casts++; if (sk != null) lastCd = sk.cd;
                AtRecFrame(_clip1481, ref _r1481);
            }
            HInput.AtLmb = false; int alive = 0; foreach (var e in list) if (e.alive) alive++;
            System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            ARes("fire1481", casts >= 3, $"skill {id} casts {casts} aliveOfTargets {alive}/{list.Count} frames {_r1481} pend {_fbPend.Count} strands {_fbStrands.Count} rocks {_fbRocks.Count} balls {_fbBalls.Count} patches {_fbPatches.Count} cd {FbCd1481(id)}");
            HellordAudio.LoopLog = null; AtClearWorld(); yield return AtFrames(5);
        }
    }
}
#endif
