using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Hellord
{
    /// <summary>t1825: context of one card's copy (what {tokens} read).</summary>
    public sealed class CardCtx1825
    {
        public int L = 1, A = 0, tgt = -1, maxlv = HData.MaxSkillLevel;
        public float cd, cdm = 1f, dm = 1f, lvdmg, dmg, kit, lvm = 1f; // t1831 dmg = live hit (SkillDamage at L); kit = live kit hit (ZkHit1651 at L); t1831u lvm = HData.LevelMul(L)
        public bool multi = true, summon;
    }

    /// <summary>t1825: resolves {expr|fmt} tokens and ?cond? prefixes in CardCopyA1825 lines against live data
    /// (HellordTuning.I fields, HellordGame / HData constants, the card context). Unknown names are reported, never guessed.</summary>
    public static class CardEval1825
    {
        static readonly Dictionary<string, MemberInfo> _mem = new Dictionary<string, MemberInfo>();
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public struct Line { public char kind; public string text; }

        /// <summary>Resolve every line of an entry. kinds = which K letters to keep (e.g. "SFDX"). errs collects unknown names.</summary>
        public static List<Line> Lines(string[] src, string kinds, CardCtx1825 c, List<string> errs)
        {
            var o = new List<Line>();
            if (src == null) return o;
            foreach (var raw in src)
            {
                string s = raw; bool show = true;
                if (s.Length > 0 && s[0] == '?')
                {
                    int q = s.IndexOf('?', 1); if (q < 0) { errs?.Add("bad cond: " + raw); continue; }
                    show = Math.Abs(Eval(s.Substring(1, q - 1), c, errs, raw)) > 1e-6; s = s.Substring(q + 1);
                }
                if (s.Length < 2 || s[1] != ':') { errs?.Add("bad line: " + raw); continue; }
                char k = s[0]; if (kinds.IndexOf(k) < 0 || !show) continue;
                o.Add(new Line { kind = k, text = Resolve(s.Substring(2), c, errs, raw) });
            }
            return o;
        }
        public static string Resolve(string s, CardCtx1825 c, List<string> errs, string src = null)
        {
            if (s.IndexOf('{') < 0) return s;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '{') { sb.Append(s[i]); continue; }
                int j = s.IndexOf('}', i); if (j < 0) { errs?.Add("unclosed {: " + (src ?? s)); sb.Append(s.Substring(i)); break; }
                string body = s.Substring(i + 1, j - i - 1), fmt = "";
                int bar = body.LastIndexOf('|'); if (bar >= 0) { fmt = body.Substring(bar + 1); body = body.Substring(0, bar); }
                sb.Append(Fmt(Eval(body, c, errs, src ?? s), fmt));
                i = j;
            }
            return sb.ToString();
        }
        public static string Fmt(double v, string f)
        {
            switch (f)
            {
                case "i": return Mathf.RoundToInt((float)v).ToString(Inv);
                case "p": return Mathf.RoundToInt((float)(v * 100.0)).ToString(Inv);
                case "x": return Math.Round(v, 2).ToString("0.0#", Inv);
                default: return Math.Round(v, 2).ToString("0.##", Inv);
            }
        }

        // ---- tiny expression parser
        sealed class P { public string s; public int i; public CardCtx1825 c; public List<string> errs; public string src; }
        public static double Eval(string expr, CardCtx1825 c, List<string> errs, string src = null)
        {
            var p = new P { s = expr, i = 0, c = c, errs = errs, src = src ?? expr };
            double v = Cmp(p); Ws(p);
            if (p.i < p.s.Length) Err(p, "trailing '" + p.s.Substring(p.i) + "'");
            return v;
        }
        static void Err(P p, string m) { p.errs?.Add(m + " in " + p.src); }
        static void Ws(P p) { while (p.i < p.s.Length && p.s[p.i] == ' ') p.i++; }
        static bool Eat(P p, string t) { Ws(p); if (string.CompareOrdinal(p.s, p.i, t, 0, t.Length) == 0) { p.i += t.Length; return true; } return false; }
        static double Cmp(P p)
        {
            double a = Sum(p);
            if (Eat(p, "<=")) return a <= Sum(p) + 1e-9 ? 1 : 0;
            if (Eat(p, ">=")) return a >= Sum(p) - 1e-9 ? 1 : 0;
            if (Eat(p, "==")) return Math.Abs(a - Sum(p)) < 1e-6 ? 1 : 0;
            if (Eat(p, "!=")) return Math.Abs(a - Sum(p)) >= 1e-6 ? 1 : 0;
            if (Eat(p, "<")) return a < Sum(p) - 1e-9 ? 1 : 0;
            if (Eat(p, ">")) return a > Sum(p) + 1e-9 ? 1 : 0;
            return a;
        }
        static double Sum(P p)
        {
            double a = Prod(p);
            while (true) { if (Eat(p, "+")) a += Prod(p); else if (Eat(p, "-")) a -= Prod(p); else return a; }
        }
        static double Prod(P p)
        {
            double a = Un(p);
            while (true) { if (Eat(p, "*")) a *= Un(p); else if (Eat(p, "/")) { double b = Un(p); a = Math.Abs(b) < 1e-12 ? 0 : a / b; } else return a; }
        }
        static double Un(P p)
        {
            if (Eat(p, "!")) return Math.Abs(Un(p)) < 1e-6 ? 1 : 0;
            if (Eat(p, "-")) return -Un(p);
            return Atom(p);
        }
        static double Atom(P p)
        {
            Ws(p);
            if (Eat(p, "(")) { double v = Cmp(p); if (!Eat(p, ")")) Err(p, "missing )"); return v; }
            if (p.i >= p.s.Length) { Err(p, "unexpected end"); return 0; }
            char ch = p.s[p.i];
            if (char.IsDigit(ch) || ch == '.')
            {
                int st = p.i; while (p.i < p.s.Length && (char.IsDigit(p.s[p.i]) || p.s[p.i] == '.')) p.i++;
                return double.Parse(p.s.Substring(st, p.i - st), Inv);
            }
            int a0 = p.i; while (p.i < p.s.Length && (char.IsLetterOrDigit(p.s[p.i]) || p.s[p.i] == '_' || p.s[p.i] == '.')) p.i++;
            string id = p.s.Substring(a0, p.i - a0);
            if (id.Length == 0) { Err(p, "bad char '" + ch + "'"); p.i++; return 0; }
            if (Eat(p, "("))
            {
                if (id == "len") { int b0 = p.i; while (p.i < p.s.Length && p.s[p.i] != ')') p.i++; string nm = p.s.Substring(b0, p.i - b0).Trim(); Eat(p, ")"); var arr = Get(p, nm) as Array; if (arr == null) { Err(p, "len of non-array " + nm); return 0; } return arr.Length; }
                double x = Cmp(p); if (!Eat(p, ",")) { Err(p, id + " needs 2 args"); return x; } double y = Cmp(p); Eat(p, ")");
                if (id == "min") return Math.Min(x, y); if (id == "max") return Math.Max(x, y);
                Err(p, "unknown fn " + id); return 0;
            }
            object val = Get(p, id);
            if (p.i < p.s.Length && p.s[p.i] == '@')
            {
                p.i++; int b0 = p.i; while (p.i < p.s.Length && char.IsLetterOrDigit(p.s[p.i])) p.i++;
                string ix = p.s.Substring(b0, p.i - b0); var arr = val as Array;
                if (arr == null || arr.Length == 0) { Err(p, id + " is not an array"); return 0; }
                int lv = ix == "MAX" ? arr.Length : ix == "L" ? p.c.L : ix == "A" ? p.c.A : int.TryParse(ix, out int n) ? n : -999;
                if (lv == -999) { Err(p, "bad index @" + ix); return 0; }
                return Num(arr.GetValue(Mathf.Clamp(lv - 1, 0, arr.Length - 1)));
            }
            if (val is Array) { Err(p, id + " is an array (needs @L / @MAX)"); return 0; }
            return Num(val);
        }
        static double Num(object o)
        {
            switch (o) { case null: return 0; case bool b: return b ? 1 : 0; case int i: return i; case float f: return f; case double d: return d; case Enum e: return Convert.ToInt32(e); }
            try { return Convert.ToDouble(o, Inv); } catch { return 0; }
        }
        static object Get(P p, string id)
        {
            var c = p.c;
            switch (id)
            {
                case "L": return c.L; case "A": return c.A; case "cd": return c.cd; case "cdm": return c.cdm; case "dm": return c.dm;
                case "lvdmg": return c.lvdmg; case "tgt": return c.tgt; case "multi": return c.multi; case "summon": return c.summon; case "maxlv": return c.maxlv; case "dmg": return c.dmg; case "kit": return c.kit; case "lvm": return c.lvm;
            }
            if (id.StartsWith("S.")) { if (Enum.TryParse(id.Substring(2), out SkillId sk)) return (int)sk; Err(p, "unknown skill " + id); return -2; }
            Type t; object inst = null; string nm;
            if (id.StartsWith("T.")) { t = typeof(HellordTuning); inst = HellordTuning.I; nm = id.Substring(2); }
            else if (id.StartsWith("G.")) { t = typeof(HellordGame); nm = id.Substring(2); }
            else if (id.StartsWith("D.")) { t = typeof(HData); nm = id.Substring(2); }
            else { Err(p, "unknown name " + id); return 0; }
            if (!_mem.TryGetValue(id, out var m))
            {
                const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
                m = (MemberInfo)t.GetField(nm, bf) ?? t.GetProperty(nm, bf); _mem[id] = m;
            }
            if (m == null) { Err(p, "unknown field " + id); return 0; }
            try
            {
                if (m is FieldInfo fi) return fi.IsStatic ? fi.GetValue(null) : fi.GetValue(inst);
                var pi = (PropertyInfo)m; return pi.GetValue(pi.GetMethod.IsStatic ? null : inst);
            }
            catch (Exception e) { Err(p, "read " + id + ": " + e.Message); return 0; }
        }
    }
}
