using System.Globalization;
using System.Text;
using UnityEngine;

// v0.0.19: read-only game-state export for the external play bot (tools/playbot.py). Enabled ONLY by the '-botstate <file>'
// command-line arg; writes a small JSON snapshot ~20x/s (atomic rename). The bot still plays through real key presses -
// nothing here changes gameplay. World units (16 px), y up.
namespace Hellord
{
    public partial class HellordGame
    {
        string _botPath; bool _botInit; float _botT; readonly StringBuilder _bsb = new StringBuilder(32768); int _botSeq;
        static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
        void BotStateTick()
        {
            if (!_botInit)
            {
                _botInit = true; var a = System.Environment.GetCommandLineArgs(); int i = System.Array.IndexOf(a, "-botstate");
                if (i >= 0 && i + 1 < a.Length) { _botPath = a[i + 1]; Debug.Log("[Hellord] botstate -> " + _botPath); }
            }
            if (_botPath == null) return;
            _botT += Time.unscaledDeltaTime; if (_botT < 0.045f) return; _botT = 0;
            try { WriteBotState(); } catch (System.Exception ex) { Debug.LogWarning("[Hellord] botstate: " + ex.Message); }
        }
        void V(string k, Vector2 v) { _bsb.Append('"').Append(k).Append("\":[").Append(F(v.x)).Append(',').Append(F(v.y)).Append("],"); }
        void WriteBotState()
        {
            var b = _bsb; b.Clear(); b.Append('{');
            b.Append("\"seq\":").Append(++_botSeq).Append(",\"t\":").Append(F(Time.unscaledTime)).Append(",\"run\":").Append(F(runTime));
            b.Append(",\"mode\":\"").Append(mode).Append("\",\"hp\":").Append(F(hp)).Append(",\"maxHp\":").Append(F(maxHp));
            b.Append(",\"lvl\":").Append(level).Append(",\"xp\":").Append(xp).Append(",\"aim\":").Append(autoAim ? 1 : 0).Append(",\"kills\":").Append(kills).Append(",\"bones\":").Append(bones);
            b.Append(",\"iframe\":").Append(F(_iframe)).Append(",\"speed\":").Append(F(baseSpeed * speedMul)).Append(",\"pickR\":").Append(F(basePickup * pickupMul)).Append(',');
            if (_hero) V("hero", _heroPos);
            if (_room != null) { _room.Walkable(HeroMarginPx, out var lo, out var hi); V("lo", lo); V("hi", hi); }
            var r = Cur;
            if (r != null)
            {
                b.Append("\"room\":{\"i\":").Append(curRoom).Append(",\"type\":\"").Append(r.type).Append("\",\"active\":").Append(r.active ? 1 : 0)
                 .Append(",\"cleared\":").Append(r.cleared ? 1 : 0).Append(",\"locked\":").Append(r.locked ? 1 : 0).Append(",\"goalT\":").Append(F(r.goalT))
                 .Append(",\"goalN\":").Append(r.goalN).Append(",\"goalHave\":").Append(r.goalHave).Append(",\"gx\":").Append(r.gx).Append(",\"gy\":").Append(r.gy).Append("},");
                b.Append("\"doors\":[");
                for (int i = 0; i < _doors.Count; i++)
                {
                    var d = _doors[i]; var to = floor[d.to]; var p = DoorPivotPx(d.dir);
                    if (i > 0) b.Append(','); b.Append("{\"dir\":").Append(d.dir).Append(",\"to\":").Append(d.to).Append(",\"type\":\"").Append(to.type)
                     .Append("\",\"visited\":").Append(to.visited ? 1 : 0).Append(",\"cleared\":").Append(to.cleared ? 1 : 0).Append(",\"secret\":").Append(d.secret ? 1 : 0)
                     .Append(",\"x\":").Append(F(p.x / HArt.PPU)).Append(",\"y\":").Append(F(-p.y / HArt.PPU)).Append('}');
                }
                b.Append("],\"floor\":[");
                for (int i = 0; i < floor.Count; i++) { var f = floor[i]; if (i > 0) b.Append(','); b.Append("[").Append(f.gx).Append(',').Append(f.gy).Append(",\"").Append(f.type).Append("\",").Append(f.visited ? 1 : 0).Append(',').Append(f.cleared ? 1 : 0).Append(",[").Append(f.nb[0]).Append(',').Append(f.nb[1]).Append(',').Append(f.nb[2]).Append(',').Append(f.nb[3]).Append("]]"); }
                b.Append("],");
                if (r.type == RoomType.Shrine && r.shop != null && _room != null)
                {
                    var D = _room.D; float sx = D.shrineX > 0 ? D.shrineX : D.w / 2, sy = D.shrineY > 0 ? D.shrineY : D.h / 2;
                    b.Append("\"shop\":{\"x\":").Append(F(sx / HArt.PPU)).Append(",\"y\":").Append(F(-(sy + 40) / HArt.PPU)).Append(",\"near\":").Append(NearShrine(HeroPx) ? 1 : 0).Append(",\"items\":[");
                    for (int i = 0; i < 3; i++) { if (i > 0) b.Append(','); b.Append("[\"").Append(HData.Support(r.shop[i]).name).Append("\",").Append(r.price[i]).Append(',').Append(r.sold[i] ? 1 : 0).Append(",\"").Append(ShopTag(r, i)).Append("\"]"); }
                    b.Append("]},");
                }
                AppendSolids(b, r);
            }
            b.Append("\"skills\":[");
            for (int i = 0; i < skills.Count; i++) { if (i > 0) b.Append(','); b.Append("[\"").Append(skills[i].def.name).Append("\",").Append(skills[i].level).Append(',').Append(skills[i].sup.Count).Append(']'); }
            b.Append("],\"cards\":[");
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i]; if (i > 0) b.Append(',');
                b.Append("[\"").Append(c.kind).Append("\",\"").Append(c.skill != null ? c.skill.name : c.support != null ? c.support.name : c.stat != null ? c.stat.name : "").Append("\"]");
            }
            b.Append("],\"en\":[");
            bool first = true;
            if (_en != null) foreach (var e in _en)
                {
                    if (!e.alive || e.boss) continue; if (!first) b.Append(','); first = false;
                    b.Append('[').Append(F(e.pos.x)).Append(',').Append(F(e.pos.y)).Append(',').Append(F(e.radius)).Append(",\"").Append(e.type).Append("\",")
                     .Append(e.elite ? 1 : 0).Append(',').Append(e.nest ? 1 : 0).Append(',').Append(F(e.hp)).Append(',').Append(F(e.speed)).Append(']');
                }
            b.Append("],\"pk\":[");
            first = true;
            if (_pk != null) foreach (var p in _pk) { if (!p.alive) continue; if (!first) b.Append(','); first = false; b.Append('[').Append(F(p.pos.x)).Append(',').Append(F(p.pos.y)).Append(',').Append(p.bone ? 1 : 0).Append(']'); }
            b.Append("],\"chest\":").Append(_chestAlive ? "[" + F(_chestPos.x) + "," + F(_chestPos.y) + "]" : "null");
            b.Append(",\"bb\":[");
            first = true;
            if (_bb != null) foreach (var x in _bb) { if (!x.alive) continue; if (!first) b.Append(','); first = false; b.Append('[').Append(F(x.pos.x)).Append(',').Append(F(x.pos.y)).Append(',').Append(F(x.vel.x)).Append(',').Append(F(x.vel.y)).Append(']'); }
            b.Append(']'); AppendHooks(b); AppendGossip(b); // v0.0.24 hook lanes, v0.0.25 shout rings + scramble
            if (BossAlive)
            {
                var e = _boss; b.Append(",\"boss\":{\"x\":").Append(F(e.pos.x)).Append(",\"y\":").Append(F(e.pos.y)).Append(",\"hp\":").Append(F(e.hp)).Append(",\"max\":").Append(F(e.maxHp))
                 .Append(",\"st\":\"").Append(_bs).Append("\",\"stT\":").Append(F(_bsT)).Append(",\"rage\":").Append(_raging ? 1 : 0).Append(",\"p2\":").Append(_phase2 ? 1 : 0)
                 .Append(",\"cdx\":").Append(F(_chargeDir.x)).Append(",\"cdy\":").Append(F(_chargeDir.y)).Append(",\"lane\":").Append(F(LanePx / 32f))
                 .Append(",\"sweep\":").Append(F(_sweepDeg)).Append(",\"hornR\":").Append(F(HornReach)).Append(",\"bcx\":").Append(F(BossC.x)).Append(",\"bcy\":").Append(F(BossC.y))
                 .Append(",\"sx\":").Append(F(_stompC.x)).Append(",\"sy\":").Append(F(_stompC.y)).Append(",\"herdX\":").Append(F(HerdLaneX / HArt.PPU)).Append(",\"bbTell\":").Append(F(_bbTellT)).Append('}');
            }
            b.Append('}');
            var tmp = _botPath + ".tmp"; System.IO.File.WriteAllText(tmp, b.ToString()); if (System.IO.File.Exists(_botPath)) System.IO.File.Replace(tmp, _botPath, null); else System.IO.File.Move(tmp, _botPath);
        }
        // v0.0.21: solid geometry for the bot, world units (1u = 16px, y up / negative into the room). Hero movement is blocked ONLY by
        // the walkable rect (room walls) and the shrine statue footprint; the rock outcrops / pillars in the room art are floor
        // decoration (not solid in game code), so they are not listed. Tongue traps are listed as hazards (circle = strike radius,
        // trigger = trigger radius, state 0 idle 1 tell 2 strike 3 retract 4 cooldown).
        void AppendSolids(System.Text.StringBuilder b, FloorRoom r)
        {
            if (_room == null) return; _room.Walkable(HeroMarginPx, out var lo, out var hi);
            b.Append("\"walk\":{\"x0\":").Append(F(lo.x)).Append(",\"y0\":").Append(F(lo.y)).Append(",\"x1\":").Append(F(hi.x)).Append(",\"y1\":").Append(F(hi.y)).Append("},\"solids\":[");
            if (r.type == RoomType.Shrine)
            {
                var D = _room.D; float sx = D.shrineX > 0 ? D.shrineX : D.w / 2, sy = D.shrineY > 0 ? D.shrineY : D.h / 2;
                var p0 = HellordRoom.Px(sx - 28, sy + 3); var p1 = HellordRoom.Px(sx + 28, sy - 96);
                b.Append("{\"kind\":\"shrine_statue\",\"shape\":\"rect\",\"x0\":").Append(F(p0.x)).Append(",\"y0\":").Append(F(p0.y)).Append(",\"x1\":").Append(F(p1.x)).Append(",\"y1\":").Append(F(p1.y)).Append("}");
            }
            b.Append("],\"hazards\":[");
            for (int i = 0; i < _traps.Count; i++)
            {
                var t = _traps[i]; if (i > 0) b.Append(',');
                b.Append("{\"kind\":\"tongue_trap\",\"shape\":\"circle\",\"x\":").Append(F(t.pos.x)).Append(",\"y\":").Append(F(t.pos.y)).Append(",\"r\":").Append(F(HellordTuning.I.tongueStrikeRadiusU))
                 .Append(",\"lash\":").Append(F(HellordTuning.I.tongueLashLengthU)).Append(",\"trigger\":").Append(F(HellordTuning.I.tongueTriggerRadiusPx / HArt.PPU)).Append(",\"state\":").Append((int)t.st).Append('}');
            }
            b.Append("],");
        }
}
}
