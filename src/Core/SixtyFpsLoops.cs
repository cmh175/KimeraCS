using System;
using System.Collections.Generic;
using System.Linq;

namespace KimeraCS
{
    // Which animations loop, for the 60 fps conversion's "Auto" setting.
    //
    // A loop also gets in-between keys from its last frame back to its first; a one-shot must not, or its
    // end blends back towards its first pose (FFNx stretches the keys over the game's animation, so those
    // extra keys are shown). An animation's movement can't tell the two apart (most battle one-shots end in
    // the pose they started from), so this follows the official 60FPS mod (v1.11), which was made with
    // Kimera's interpolation and its loop question answered per animation: 4n frames = loop, 4n - 3 =
    // one-shot. The lists below are read from the mod's own files (devtools/loop_check.py compares).
    //   field:  the mod converts every animation as a one-shot (2n - 1 frames)
    //   battle: the idle (ANIM_00) loops, except for a few models; some other animations loop too
    //   limit breaks: per pack
    //   summons: everything loops, except Mog's second animation and the Knights of the Round (KNIGHT01..13,
    //   one animation each: one-shots in the mod's KOTRAnimation60FPS files, except knight11)
    // Animations the mod doesn't have (new models, new packs) follow the same rules.
    public static class SixtyFpsLoops
    {
        // battle models whose idle (ANIM_00) does not loop
        private static readonly HashSet<string> battleIdleOneShot = new HashSet<string>(
            "aa ab ac ad ae af ag ah ai aj bx bz gk hc ij".Split(' '));

        // other looping battle animations, by model code (the skeleton's first two letters)
        private static readonly Dictionary<string, int[]> battleLoops = Parse(
            "ak:20 al:20 an:20 ao:20 ap:20 aw:20 bh:24,30,54 bw:12 co:20,40 cp:20 " +
            "cq:3,8,9,10,11,12,13,14,15,16 cs:2,3,4 ct:10 cz:11 da:1 dd:10 di:20 dn:10 ex:12 fa:20 fb:26 fn:10 " +
            "fr:10 ft:10 fu:10 go:10 gw:10 ha:10,20 hn:20 hq:11 hr:1 hu:10 hx:26 iu:28,38 iv:38 ko:12 kq:20 " +
            "ks:20 lk:21 ll:12 ln:24 lo:24 lu:32 lv:32 lw:32 ly:7 me:26 mm:22,36,50 mt:16 " +
            "nb:3,10,13 nc:3,10,13 nd:3,10,13 ne:3,10,13 nf:3,10,13 ng:3,10,13 nh:3,10,13 ni:3,10,13 " +
            "nj:3,10,13 nk:3,10,13 nl:3,10,13 nm:3,10,13 nn:3,10,13 no:3,10,13 rs:5,10,15 " +
            "rt:1,7,16 ru:1,7,16,27 rv:1,7,16 rw:1,2,7,16 rx:1,7 ry:1,2,7,16,27 rz:1,7 sa:7 sb:1,2,7 sc:1,2,7 " +
            "sd:1,2,7 se:1,2,7 sf:1,7 sg:1,7,16 sh:1,7,16 si:1,7,16 sj:2 sk:2 sm:2");

        // limit break packs: the looping parts (packs the mod has with none listed: all one-shots)
        private static readonly Dictionary<string, int[]> limitLoops = Parse(
            "BLAVER:0,1,2 KYOU:0,1,2 LIMCL2:0 LIMCL4:0,1 LIMCL6:0,1 LIMFAST:0,1,2,4,5,6 LIMEA2:0,1 LIMRD5:0 " +
            "LIMRD6:0,1 LIMRD7:0 LIMSLED:0,1,2,3 LIMYF1:0,1,2 LIMYF6:0,1,3,4,5,6 LIMYF7:0,1,2,3,4,5,6 " +
            "LIMCD3:0,1,3 LIMCD5:0,1 LIMBR6:0,1,2,3 LIMBR7:0,1");
        private static readonly HashSet<string> limitPacks = new HashSet<string>((
            "BLAVER DICE HVSHOT IYASH KODO KYOU LIMBR2 LIMBR3 LIMBR4 LIMBR5 LIMBR6 LIMBR7 LIMCD1 LIMCD2 LIMCD3 " +
            "LIMCD4 LIMCD5 LIMCD6 LIMCD7 LIMCL2 LIMCL3 LIMCL4 LIMCL6 LIMCL7 LIMEA2 LIMEA3 LIMEA4 LIMEA5 LIMEA6 " +
            "LIMFAST LIMRD3 LIMRD4 LIMRD5 LIMRD6 LIMRD7 LIMSLED LIMYF1 LIMYF2 LIMYF3 LIMYF4 LIMYF5 LIMYF6 LIMYF7").Split(' '));

        // summon / magic models: every animation loops except these
        private static readonly Dictionary<string, int[]> magicOneShots = Parse("MOGURIDA:1");
        private static readonly HashSet<string> magicModels = new HashSet<string>((
            "5DETHDAT ALEXDAT ANGELDAT BAHAMDAT BAHAMRDA BAHAMZDA CONFDAT CYVADAT DEATHDAT DEBCYODA HADESDAT " +
            "IFREETDA KUJATADA LAMDAT MOGDAT MOGURIDA ODIN_GDA ODIN_ZDA PHEONIXD RIVADAT TITANDAT TOYDAT TUPONDAT").Split(' '));

        private static Dictionary<string, int[]> Parse(string s) =>
            s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
             .Select(e => e.Split(':'))
             .ToDictionary(e => e[0], e => e[1].Split(',').Select(int.Parse).ToArray());

        // modelCode: battle skeleton's first two letters (Cloud: "rt")
        public static bool Battle(string modelCode, int index, out string why)
        {
            string c = (modelCode ?? "").ToLowerInvariant();
            bool known = string.CompareOrdinal(c, "aa") >= 0 && string.CompareOrdinal(c, "of") <= 0 ||
                         string.CompareOrdinal(c, "rs") >= 0 && string.CompareOrdinal(c, "sm") <= 0;
            bool loop = index == 0 ? !battleIdleOneShot.Contains(c)
                                   : battleLoops.TryGetValue(c, out int[] l) && l.Contains(index);
            why = known ? "as the 60FPS mod" : index == 0 ? "battle idle" : "battle";
            return loop;
        }

        // pack: limit break pack name without extension (e.g. "BLAVER")
        public static bool Limit(string pack, int index, out string why)
        {
            string p = (pack ?? "").ToUpperInvariant();
            why = limitPacks.Contains(p) ? "as the 60FPS mod" : "limit break";
            return limitLoops.TryGetValue(p, out int[] l) && l.Contains(index);
        }

        // model: summon / magic model name without extension (e.g. "BAHAMDAT")
        public static bool Magic(string model, int index, out string why)
        {
            string m = (model ?? "").ToUpperInvariant();
            bool knight = System.Text.RegularExpressions.Regex.IsMatch(m, "^KNIGHT[0-9][0-9]$");
            why = magicModels.Contains(m) || knight ? "as the 60FPS mod" : "summon";
            if (knight) return m == "KNIGHT11";
            return !(magicOneShots.TryGetValue(m, out int[] l) && l.Contains(index));
        }

        public static bool Field(out string why)
        {
            why = "field, as the 60FPS mod";
            return false;
        }
    }
}
