using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KimeraCS
{
    // Readable names for the bones of the party battle models, added after the bone number
    // ("bone_05_l_uparm"). Battle skeletons store no names, so each layout below was named by hand from
    // its rest pose, mesh and the way the knees bend, following the field skeletons (hip, chest, head,
    // l_chest, l_collar, l_uparm, l_foarm, l_hand, l_hip, l_femur, l_tibia, l_foot, the r_ ones, plus
    // l_toe / r_toe and tail_N; Red XIII's front legs and Cait Sith's moogle ("d" prefix) as in their
    // field models). Bones with no counterpart there (hair, capes, coat flaps, claws, connectors) keep the
    // plain number, as do models not listed here (enemies, summons).
    //
    // The number stays first because FFNx finds the game's bones by it (target cursor, damage numbers and
    // effects follow the glTF body). A layout is only used when the model's bone hierarchy matches the one
    // it was made for, so a mod that changes a skeleton falls back to plain numbers.
    public static class BattleBoneNames
    {
        private class Layout
        {
            public readonly string Character;
            public readonly string[] Models;
            public readonly int[] Parents;
            public readonly Dictionary<int, string> Names = new Dictionary<int, string>();

            public Layout(string character, string[] models, string parents, string names)
            {
                Character = character;
                Models = models;
                Parents = parents.Split(' ').Select(p => int.Parse(p, CultureInfo.InvariantCulture)).ToArray();
                foreach (string entry in names.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kv = entry.Split(':');
                    Names[int.Parse(kv[0], CultureInfo.InvariantCulture)] = kv[1];
                }
            }
        }

        private static readonly Layout[] LAYOUTS =
        {
            new Layout("Cloud", new[] { "RTAA", "SIAA" },
                       "-1 0 1 0 3 4 5 6 0 8 9 10 11 -1 13 14 15 16 -1 18 19 20 21",
                       "0:hip 1:chest 2:head 3:l_chest 4:l_collar 5:l_uparm 6:l_foarm 7:l_hand 8:r_chest 9:r_collar " +
                       "10:r_uparm 11:r_foarm 12:r_hand 13:l_hip 14:l_femur 15:l_tibia 16:l_foot 17:l_toe 18:r_hip " +
                       "19:r_femur 20:r_tibia 21:r_foot 22:r_toe"),
            new Layout("Barret", new[] { "SBAA", "SCAA", "SDAA", "SEAA" },
                       "-1 0 1 0 3 4 5 6 0 8 9 10 -1 12 13 14 15 -1 17 18 19 20",
                       "0:hip 1:chest 2:head 3:l_chest 4:l_collar 5:l_uparm 6:l_foarm 7:l_hand 8:r_chest 9:r_collar " +
                       "10:r_uparm 11:r_foarm 12:l_hip 13:l_femur 14:l_tibia 15:l_foot 16:l_toe 17:r_hip 18:r_femur " +
                       "19:r_tibia 20:r_foot 21:r_toe"),
            new Layout("Tifa", new[] { "RUAA" },
                       "-1 0 1 2 3 4 5 6 -1 8 9 10 11 12 13 14 8 16 17 18 19 20 21 -1 23 24 25 26 -1 28 29 30",
                       "0:chest 1:head 8:hip 9:l_hip 11:l_femur 12:l_tibia 13:l_foot 15:l_toe 16:r_hip 18:r_femur " +
                       "19:r_tibia 20:r_foot 22:r_toe 23:l_chest 24:l_collar 25:l_uparm 26:l_foarm 27:l_hand 28:r_chest " +
                       "29:r_collar 30:r_uparm 31:r_foarm"),
            new Layout("Aerith", new[] { "RVAA" },
                       "-1 0 1 2 3 4 5 6 0 8 9 10 11 0 13 14 15 16 -1 18 19 20 21 -1 23 24 -1 26 27 28 29",
                       "0:hip 1:chest 2:head 8:l_chest 9:l_collar 10:l_uparm 11:l_foarm 12:l_hand 13:r_chest 14:r_collar " +
                       "15:r_uparm 16:r_foarm 17:r_hand 18:l_hip 19:l_femur 20:l_tibia 21:l_foot 22:l_toe 26:r_hip " +
                       "27:r_femur 28:r_tibia 29:r_foot 30:r_toe"),
            new Layout("Red XIII", new[] { "RWAA" },
                       "-1 0 1 2 3 3 5 6 7 3 9 10 11 3 13 1 15 16 17 18 19 1 21 22 23 24 25 -1 27 28 29 30 31 -1 33 34 35 36 37 38 -1 40 41 42 43 44",
                       "0:hip 2:chest 3:neck 4:head 15:l_chest 16:l_collar 17:l_uparm 18:l_foarm 19:l_hand1 20:l_hand2 " +
                       "21:r_chest 22:r_collar 23:r_uparm 24:r_foarm 25:r_hand1 26:r_hand2 27:l_hip 28:l_femur " +
                       "29:l_tibia 30:l_foot 32:l_toe 33:tail_1 34:tail_2 35:tail_3 36:tail_4 37:tail_5 38:tail_6 " +
                       "39:tail_7 40:r_hip 41:r_femur 42:r_tibia 43:r_foot 45:r_toe"),
            new Layout("Yuffie", new[] { "RXAA", "NPAA", "NQAA", "NRAA", "NSAA", "NTAA", "NUAA" },
                       "-1 0 1 2 3 3 0 6 7 8 9 9 6 12 0 14 15 16 17 -1 19 20 21 22 23 -1 25 26 27 28 29",
                       "0:hip 1:chest 2:head 6:l_chest 7:l_collar 8:l_uparm 9:l_foarm 10:l_hand 14:r_chest 15:r_collar " +
                       "16:r_uparm 17:r_foarm 18:r_hand 19:l_hip 20:l_femur 21:l_tibia 22:l_foot 24:l_toe 25:r_hip " +
                       "26:r_femur 27:r_tibia 28:r_foot 30:r_toe"),
            new Layout("Cait Sith", new[] { "RYAA" },
                       "-1 0 1 -1 3 4 -1 6 7 8 9 6 11 12 13 13 15 16 17 18 13 20 21 22 23 13 25 12 27 28 29 12 31 32 33 12 35 36 37 6 39 40 6 42 43 44",
                       "0:dr_hip 1:dr_femur 2:dr_foot 3:dl_hip 4:dl_femur 5:dl_foot 6:dchest 7:dr_chest 8:dr_colla " +
                       "9:dr_uparm 10:dr_foarm 13:chest 14:head 15:l_chest 16:l_collar 17:l_uparm 18:l_foarm 19:l_hand " +
                       "20:r_chest 21:r_collar 22:r_uparm 23:r_foarm 24:r_hand 27:l_hip 28:l_femur 29:l_tibia 30:l_foot " +
                       "31:tail_1 32:tail_2 33:tail_3 34:tail_4 35:r_hip 36:r_femur 37:r_tibia 38:r_foot 42:dl_chest " +
                       "43:dl_colla 44:dl_uparm 45:dl_foarm"),
            new Layout("Cid", new[] { "RZAA" },
                       "-1 0 1 2 0 4 5 6 7 0 9 10 0 12 13 14 15 -1 17 18 19 20 21 -1 23 24 25 26 27",
                       "0:hip 1:chest 2:head 4:l_chest 5:l_collar 6:l_uparm 7:l_foarm 8:l_hand 12:r_chest 13:r_collar " +
                       "14:r_uparm 15:r_foarm 16:r_hand 17:l_hip 18:l_femur 19:l_tibia 20:l_foot 22:l_toe 23:r_hip " +
                       "24:r_femur 25:r_tibia 26:r_foot 28:r_toe"),
            new Layout("Sephiroth", new[] { "SAAA" },
                       "-1 0 1 2 3 4 5 6 2 8 9 10 11 12 2 14 15 16 17 18 -1 20 21 22 23 24 20 26 20 28 20 30 20 32 33 34 35 -1 37 38 39 40 -1 42 43 44 45",
                       "0:chest 1:head 20:hip 21:l_hip 22:l_femur 23:l_tibia 24:l_foot 25:l_toe 32:r_hip 33:r_femur " +
                       "34:r_tibia 35:r_foot 36:r_toe 37:l_chest 38:l_collar 39:l_uparm 40:l_foarm 41:l_hand 42:r_chest " +
                       "43:r_collar 44:r_uparm 45:r_foarm 46:r_hand"),
            new Layout("Vincent", new[] { "SFAA", "SGAA", "SHAA" },
                       "-1 0 1 2 3 4 1 6 7 0 9 10 11 12 9 14 15 16 9 18 19 20 9 22 23 24 -1 26 27 28 29 30 26 32 33 34 35 -1 37 38 39 40 -1 42 43 44 45",
                       "0:chest 1:head 26:hip 27:l_hip 28:l_femur 29:l_tibia 30:l_foot 31:l_toe 32:r_hip 33:r_femur " +
                       "34:r_tibia 35:r_foot 36:r_toe 37:l_chest 38:l_collar 39:l_uparm 40:l_foarm 41:l_hand 42:r_chest " +
                       "43:r_collar 44:r_uparm 45:r_foarm 46:r_hand"),
            new Layout("Galian Beast", new[] { "SJAA" },
                       "-1 0 1 2 3 4 5 6 5 8 5 10 5 12 1 14 14 16 1 18 19 20 21 22 21 24 21 26 21 28 -1 30 31 32 -1 34 35 36 37 38 39 40 41 42 -1 44 45 46",
                       "0:hip 1:chest 2:l_collar 3:l_uparm 4:l_foarm 5:l_hand 14:neck 15:head 18:r_collar 19:r_uparm " +
                       "20:r_foarm 21:r_hand 30:l_hip 31:l_femur 32:l_tibia 33:l_foot 34:tail_1 35:tail_2 36:tail_3 " +
                       "37:tail_4 38:tail_5 39:tail_6 40:tail_7 41:tail_8 42:tail_9 43:tail_10 44:r_hip 45:r_femur " +
                       "46:r_tibia 47:r_foot"),
            new Layout("Death Gigas", new[] { "SKAA" },
                       "-1 0 1 0 3 4 5 6 7 7 9 0 11 12 13 14 15 16 15 -1 19 20 21 -1 23 24 25",
                       "0:hip 1:chest 2:head 3:l_chest 4:l_collar 5:l_uparm 6:l_foarm 7:l_hand 11:r_chest 12:r_collar " +
                       "13:r_uparm 14:r_foarm 15:r_hand 19:l_hip 20:l_femur 21:l_tibia 22:l_foot 23:r_hip 24:r_femur " +
                       "25:r_tibia 26:r_foot"),
            new Layout("Hellmasker", new[] { "SLAA" },
                       "-1 0 1 2 3 4 5 6 0 8 9 10 11 12 13 12 15 0 17 18 19 20 21 22 -1 24 25 26 -1 28 29 30",
                       "0:hip 1:chest 2:neck 3:head 8:l_chest 9:l_collar 10:l_uparm 11:l_foarm 12:l_hand 17:r_chest " +
                       "18:r_collar 19:r_uparm 20:r_foarm 21:r_hand 24:l_hip 25:l_femur 26:l_tibia 27:l_foot 28:r_hip " +
                       "29:r_femur 30:r_tibia 31:r_foot"),
            new Layout("Chaos", new[] { "SMAA" },
                       "-1 0 1 2 0 4 5 6 7 4 9 10 11 0 13 14 15 16 13 18 19 20 -1 22 23 24 25 22 27 28 29",
                       "1:chest 2:neck 3:head 4:l_chest 5:l_collar 6:l_uparm 7:l_foarm 8:l_hand 13:r_chest 18:r_collar " +
                       "19:r_uparm 20:r_foarm 21:r_hand 22:hip 23:l_hip 24:l_femur 25:l_tibia 26:l_foot 27:r_hip " +
                       "28:r_femur 29:r_tibia 30:r_foot"),
            new Layout("Frog", new[] { "RSAA" },
                       "-1 0 1 0 3 4 5 6 0 8 9 10 11 -1 13 14 15 -1 17 18 19",
                       "0:hip 1:chest 2:head 3:l_chest 4:l_collar 5:l_uparm 6:l_foarm 7:l_hand 8:r_chest 9:r_collar " +
                       "10:r_uparm 11:r_foarm 12:r_hand 13:l_hip 14:l_femur 15:l_tibia 16:l_foot 17:r_hip 18:r_femur " +
                       "19:r_tibia 20:r_foot"),
        };

        // Joint names for a battle model: "bone_NN" or "bone_NN_<name>". parents[i] is bone i's parent
        // (-1 for the root), as the exporter builds the joints. layout = the character whose names were used,
        // or null.
        public static string[] JointNames(string skeletonFileName, IList<int> parents, out string layout)
        {
            string model = (skeletonFileName ?? "").ToUpperInvariant();
            Layout match = LAYOUTS.FirstOrDefault(l => l.Models.Contains(model) && l.Parents.SequenceEqual(parents));
            layout = match?.Character;

            string[] names = new string[parents.Count];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = "bone_" + i.ToString("00", CultureInfo.InvariantCulture);
                if (match != null && match.Names.TryGetValue(i, out string name)) names[i] += "_" + name;
            }
            return names;
        }

        // A model in the table whose hierarchy differs (a mod that changed the skeleton), for the report.
        public static bool IsListed(string skeletonFileName) =>
            LAYOUTS.Any(l => l.Models.Contains((skeletonFileName ?? "").ToUpperInvariant()));
    }
}
