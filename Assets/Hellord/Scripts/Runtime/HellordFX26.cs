using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.26: Pixel FX Pack 2.5.1 modules (LevelUpFX, HurtFX, DoorFX, RoomFX), each behind its own HellordTuning toggle
// (fxLevelUp / fxHeroHurt / fxDoors / fxRoom). Toggle off = the exact v0.0.25 behaviour. Visual only: no gameplay numbers change.
namespace Hellord
{
    public partial class HellordGame
    {
        int _luH = -2, _stairsBeacon = -1; bool _fx26Setup, _luPicked; readonly List<int> _nestSeals = new List<int>(4), _doorFxH = new List<int>(4);
        public int fx26LevelOpens, fx26HeroHits, fx26DoorsReg, fx26Locks, fx26Unlocks, fx26Nests, fx26Stairs; // autotest counters

        static HellordTuning FT => HellordTuning.I;
        void EnsureFX26()
        {
            if (_fx26Setup) return; _fx26Setup = true;
            var tmp = NewSR("matprobe26", null, null, 0); var mat = tmp.sharedMaterial; Destroy(tmp.gameObject);
            // each runner writes its own atlas into mainTexture, so every module needs its own material copy
            LevelUpFX.MaterialOverride = new Material(mat); LevelUpFX.SortingOrder = FT.fxLevelUpSortingOrder;
            HurtFX.MaterialOverride = new Material(mat); HurtFX.SortingOrder = FT.fxHurtSortingOrder;
            DoorFX.MaterialOverride = new Material(mat);
            RoomFX.MaterialOverride = new Material(mat);
            if (FT.fxLevelUp) { LevelUpFX.Prewarm(); LevelUpFX.SetGroundSorting("Default", -20000); }
            if (FT.fxHeroHurt) HurtFX.Prewarm();
            if (FT.fxDoors) DoorFX.Prewarm();
            if (FT.fxRoom) RoomFX.Prewarm();
        }

        /// <summary>Per frame (any mode): hero anchors for LevelUpFX / HurtFX.</summary>
        void UpdateFX26()
        {
            if (!_fx26Setup) return;
            if (FT.fxLevelUp) LevelUpFX.SetHero(_heroPos);
            if (FT.fxHeroHurt && mode == Mode.Play)
            {
                if (_iframe > 0 && HellordTuning.I.iframeRing969) HurtFX.IFrames(_heroPos, _iframe, Mathf.Max(0.01f, HeroIFrame));
                if (maxHp > 0) HurtFX.LowHpPips(HeroCenter + Vector2.up * 1.1f, hp / maxHp);
            }
        }

        // ---- level-up ----
        void FX26LevelOpen() { if (!FT.fxLevelUp) return; EnsureFX26(); _luH = LevelUpFX.Open(_heroPos, ShownLevel); _luPicked = false; fx26LevelOpens++; }
        void FX26LevelPicked(LevelPick k) { if (!FT.fxLevelUp || _luH < 0) return; LevelUpFX.Picked(_luH, k, _heroPos); _luPicked = true; }
        bool FX26CardsLocked => FT.fxLevelUp && mode == Mode.LevelUp && _luH >= 0 && !LevelUpFX.CardsReady(_luH);
        float FX26Reveal(int card) => FT.fxLevelUp && mode == Mode.LevelUp && _luH >= 0 ? LevelUpFX.Reveal(_luH, card) : 1f;
        int FX26Slide(int card) => FT.fxLevelUp && mode == Mode.LevelUp && _luH >= 0 ? LevelUpFX.SlidePx(_luH, card) : 0;
        float FX26Dim => FT.fxLevelUp && _luH >= 0 ? LevelUpFX.DimK(_luH) : 1f;
        static LevelPick PickKind(CardKind k) => k == CardKind.NewSkill ? LevelPick.NewSkill : k == CardKind.SkillLevel ? LevelPick.SkillLevel : k == CardKind.Support ? LevelPick.Support : LevelPick.Stat;
        /// <summary>Play-mode OnGUI: the dim fades back out after a pick instead of popping off.</summary>
        void DrawFX26ResumeDim()
        {
            if (!FT.fxLevelUp || _luH < 0) return;
            // left the level-up screen without a pick (debug / test / any other path): drop the dim at once, never leave the game darkened
            if (!_luPicked) { LevelUpFX.Kill(_luH); _luH = -2; return; }
            float k = LevelUpFX.DimK(_luH);
            if (k <= 0.001f) { _luH = -2; return; }
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.69f * k));
        }

        // ---- hero hurt ----
        void FX26HeroHit(float dmg)
        {
            if (!FT.fxHeroHurt) return; EnsureFX26(); fx26HeroHits++;
            Vector2 src = _heroPos + Vector2.down; float best = 9e9f; // nearest live enemy = the source direction (good enough for a visual)
            foreach (var e in _en) { if (!e.alive) continue; float d = (e.pos - _heroPos).sqrMagnitude; if (d < best) { best = d; src = e.pos; } }
            HurtFX.Hit(HeroCenter, src, dmg, null);
            if (hp <= 0) HurtFX.Death(HeroCenter, src);
        }

        // ---- doors / room ----
        void FX26RegisterDoors()
        {
            if (!FT.fxDoors) return; EnsureFX26(); DoorFX.Clear(); _doorFxH.Clear();
            foreach (var d in _doors)
            {
                if (d.secret || d.sr == null) continue;
                Vector2 n = d.dir == 0 ? Vector2.down : d.dir == 1 ? Vector2.left : d.dir == 2 ? Vector2.up : Vector2.right;
                var p = DoorPivotPx(d.dir); int h = DoorFX.RegisterDoor(HellordRoom.Px(p.x, p.y), n, 32f); if (h >= 0) _doorFxH.Add(h); fx26DoorsReg++;
            }
        }
        void FX26Lock() { if (!FT.fxDoors) return; DoorFX.LockAll(); fx26Locks++; }
        void FX26Unlock() { if (!FT.fxDoors) return; DoorFX.UnlockAll(); fx26Unlocks++; }
        void FX26NestDead(Vector2 pos) { if (!FT.fxRoom) return; EnsureFX26(); int h = RoomFX.NestDestroyed(pos + Vector2.up * 0.3f); if (h >= 0) _nestSeals.Add(h); fx26Nests++; }
        void FX26Stairs(Vector2 pos) { if (!FT.fxRoom) return; EnsureFX26(); if (_stairsBeacon >= 0) RoomFX.Kill(_stairsBeacon); _stairsBeacon = RoomFX.StairsOpen(pos); fx26Stairs++; }
        void FX26RoomUnload()
        {
            if (!_fx26Setup) return;
            if (FT.fxRoom) { foreach (var h in _nestSeals) RoomFX.Kill(h); _nestSeals.Clear(); if (_stairsBeacon >= 0) RoomFX.Kill(_stairsBeacon); _stairsBeacon = -1; }
            if (FT.fxDoors) { DoorFX.Clear(); _doorFxH.Clear(); }
        }
        void FX26Reset()
        {
            if (!_fx26Setup) return;
            if (FT.fxLevelUp) LevelUpFX.Clear(); _luH = -2; _luPicked = false;
            if (FT.fxHeroHurt) HurtFX.Clear();
            FX26RoomUnload(); if (FT.fxRoom) RoomFX.Clear();
        }
    }
}
