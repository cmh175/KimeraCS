using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KimeraCS
{
    // Readable names for the bones of the party battle models, the summons and Cait Sith's limit models,
    // added after the bone number ("bone_05_l_uparm"). Battle and magic skeletons store no names, so each
    // layout below was named by hand from its rest pose, mesh and the way the knees bend, following the
    // field skeletons (hip, chest, neck, head, l_chest, l_collar, l_uparm, l_foarm, l_hand, l_hip, l_femur,
    // l_tibia, l_foot, the r_ ones, plus l_toe / r_toe). Creatures: a mount uses the field "d" prefix (Cait
    // Sith's moogle, Odin's horse, the chocobo under Choco/Mog); four legs: front legs with arm names, back
    // legs with leg names (as Red XIII's field model); an extra leg pair gets "_2"; necks and tails are
    // numbered from the body (neck_1, tail_1); wings are lettered per chain and numbered from the body
    // (l_wing_a_1); jaw. Bones with no counterpart (hair, capes, coat flaps, claws, fingers, weapons,
    // connectors) keep the plain number, as do models not listed here (enemies).
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
            // party (battle.lgp)
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

            // summons and Cait Sith's limit models (magic.lgp)
            new Layout("Shiva", new[] { "CYVADAT.D" },
                       "-1 0 1 2 3 4 5 6 7 8 3 10 11 12 13 14 3 16 17 18 19 20 -1 22 23 24 25 22 27 28 29 -1 31 32 33 34 -1 36 36 -1 39 -1 41 -1 43 44 45 46",
                       "0:chest 1:head 22:hip 23:l_hip 24:l_femur 25:l_tibia 26:l_foot 27:r_hip 28:r_femur 29:r_tibia " +
                       "30:r_foot 31:l_chest 32:l_collar 33:l_uparm 34:l_foarm 35:l_hand 43:r_chest 44:r_collar " +
                       "45:r_uparm 46:r_foarm 47:r_hand"),
            new Layout("Ifrit", new[] { "IFREETDA.D" },
                       "-1 0 1 2 3 3 0 6 7 8 9 10 9 12 0 14 15 16 17 18 17 20 0 22 0 24 0 26 0 28 -1 30 31 32 32 34 35 32 37 -1 39 40 41 42 43 41 45 41",
                       "0:hip 1:chest 3:neck 4:head 6:l_chest 7:l_collar 8:l_uparm 9:l_foarm 10:l_hand 14:r_chest " +
                       "15:r_collar 16:r_uparm 17:r_foarm 18:r_hand 30:l_hip 31:l_femur 32:l_tibia 33:l_foot 39:r_hip " +
                       "40:r_femur 41:r_tibia 47:r_foot"),
            new Layout("Titan", new[] { "TITANDAT.D" },
                       "-1 0 1 2 3 4 5 6 0 8 9 10 11 12 12 14 8 16 0 18 19 0 21 22 23 24 25 26 25 21 29 -1 31 32 33 -1 35 36 37",
                       "0:hip 1:chest 2:neck 3:head 8:l_chest 9:l_collar 10:l_uparm 11:l_foarm 12:l_hand 21:r_chest " +
                       "22:r_collar 23:r_uparm 24:r_foarm 25:r_hand 31:l_hip 32:l_femur 33:l_tibia 34:l_foot 35:r_hip " +
                       "36:r_femur 37:r_tibia 38:r_foot"),
            new Layout("Ramuh", new[] { "LAMDAT.D" },
                       "-1 0 1 2 3 4 4 6 6 8 8 10 3 1 13 13 15 13 17 13 19 20 21 22 23 13 25 26 27 28 29 13 31 32 1 34 35 36 36 38 38 40 41 42 43 40 35",
                       "0:hip 1:chest 2:l_collar 3:l_uparm 4:l_foarm 6:l_hand 13:neck 14:head 34:r_collar 35:r_uparm " +
                       "36:r_foarm 38:r_hand"),
            new Layout("Odin (Zantetsuken)", new[] { "ODIN_ZDA.D" },
                       "-1 0 1 2 2 4 2 6 7 8 2 10 11 12 2 14 15 16 0 18 19 20 0 22 23 24 0 26 27 28 0 30 31 32 0 34 -1 36 37 38 39 40 36 42 43 44 45 36",
                       "0:dchest 2:chest 3:head 14:r_collar 15:r_uparm 16:r_foarm 17:r_hand 18:dl_chest_2 19:dl_uparm_2 " +
                       "20:dl_foarm_2 21:dl_hand_2 22:dl_chest 23:dl_uparm 24:dl_foarm 25:dl_hand 26:dr_chest_2 " +
                       "27:dr_uparm_2 28:dr_foarm_2 29:dr_hand_2 30:dr_chest 31:dr_uparm 32:dr_foarm 33:dr_hand 34:dneck " +
                       "35:dhead 36:dhip 37:dl_hip 38:dl_femur 39:dl_tibia 40:dl_foot 41:dl_toe 42:dr_hip 43:dr_femur " +
                       "44:dr_tibia 45:dr_foot 46:dr_toe 47:dtail_1"),
            new Layout("Odin (Gunge Lance)", new[] { "ODIN_GDA.D" },
                       "-1 0 1 2 2 4 5 6 2 8 9 10 2 12 13 14 2 16 0 18 19 20 0 22 23 24 0 26 27 28 0 30 31 32 0 34 -1 36 37 38 39 40 36 42 43 44 45 36",
                       "0:dchest 2:chest 3:head 4:l_collar 5:l_uparm 6:l_foarm 7:l_hand 18:dl_chest_2 19:dl_uparm_2 " +
                       "20:dl_foarm_2 21:dl_hand_2 22:dl_chest 23:dl_uparm 24:dl_foarm 25:dl_hand 26:dr_chest_2 " +
                       "27:dr_uparm_2 28:dr_foarm_2 29:dr_hand_2 30:dr_chest 31:dr_uparm 32:dr_foarm 33:dr_hand 34:dneck " +
                       "35:dhead 36:dhip 37:dl_hip 38:dl_femur 39:dl_tibia 40:dl_foot 41:dl_toe 42:dr_hip 43:dr_femur " +
                       "44:dr_tibia 45:dr_foot 46:dr_toe 47:dtail_1"),
            new Layout("Alexander", new[] { "ALEXDAT.D" },
                       "-1 0 1 2 3 3 5 6 7 6 9 6 11 6 13 6 15 6 17 6 19 6 21 6 23 6 25 6 27 6 29 6 31 5 33 34 5 36 37 3",
                       "0:hip 6:chest 7:head 33:l_collar 34:l_uparm 35:l_foarm 36:r_collar 37:r_uparm 38:r_foarm"),
            new Layout("Hades", new[] { "HADESDAT.D" },
                       "-1 0 1 2 3 3 5 6 3 2 9 10 11 12 13 14 13 16 13 18 13 20 13 22 12 24 25 2 27 28 29 30 31 32 30 34 35 34 37 34 39 34 41 34 43 -1",
                       "2:hip 3:chest 4:head 9:l_chest 10:l_collar 11:l_uparm 12:l_foarm 13:l_hand 27:r_chest " +
                       "28:r_collar 29:r_uparm 30:r_foarm 34:r_hand"),
            new Layout("Knights of the Round (1, 5, 12)", new[] { "KNIGHT01.D", "KNIGHT05.D", "KNIGHT12.D" },
                       "-1 0 1 2 3 4 5 6 7 8 1 10 11 12 1 14 15 16 1 18 19 20 0 22 23 24 25 0 27 28 29 30 31 -1 33 34 35 -1 37 38 -1 40 41 -1 43 44 45",
                       "0:hip 1:chest 2:head 22:l_chest 23:l_collar 24:l_uparm 25:l_foarm 26:l_hand 27:r_chest " +
                       "28:r_collar 29:r_uparm 30:r_foarm 31:r_hand 33:l_hip 34:l_femur 35:l_tibia 36:l_foot 43:r_hip " +
                       "44:r_femur 45:r_tibia 46:r_foot"),
            new Layout("Knights of the Round (2, 8, 10)", new[] { "KNIGHT02.D", "KNIGHT08.D", "KNIGHT10.D" },
                       "-1 0 1 2 3 4 5 6 7 3 9 10 11 12 0 14 15 16 17 -1 19 20 21 19 23 24 25 26 19 28 29 19 31 32 33 34 -1 36 37 38 39 -1 41 42 43 44 45",
                       "0:chest 1:neck 2:head 19:hip 23:l_hip 24:l_femur 25:l_tibia 26:l_foot 27:l_toe 31:r_hip " +
                       "32:r_femur 33:r_tibia 34:r_foot 35:r_toe 36:r_chest 37:r_collar 38:r_uparm 39:r_foarm 40:r_hand " +
                       "41:l_chest 42:l_collar 43:l_uparm 44:l_foarm 45:l_hand"),
            new Layout("Knights of the Round (3, 7, 9)", new[] { "KNIGHT03.D", "KNIGHT07.D", "KNIGHT09.D" },
                       "-1 0 1 1 3 4 5 1 7 8 9 1 11 12 13 1 15 16 17 0 19 20 21 22 0 24 25 26 27 28 -1 30 31 32 -1 34 35 36 -1 38 39",
                       "0:hip 1:chest 2:head 19:l_chest 20:l_collar 21:l_uparm 22:l_foarm 23:l_hand 24:r_chest " +
                       "25:r_collar 26:r_uparm 27:r_foarm 28:r_hand 30:l_hip 31:l_femur 32:l_tibia 33:l_foot 34:r_hip " +
                       "35:r_femur 36:r_tibia 37:r_foot"),
            new Layout("Knights of the Round (4, 6, 11)", new[] { "KNIGHT04.D", "KNIGHT06.D", "KNIGHT11.D" },
                       "-1 0 1 0 3 4 5 6 -1 8 9 10 11 8 13 14 15 16 17 18 8 20 21 22 23 24 25 8 27 28 -1 30 31 32 33 34 -1 36 37 38 39 40 41",
                       "0:chest 1:neck 2:head 8:hip 13:r_hip 14:r_femur 16:r_tibia 17:r_foot 19:r_toe 20:l_hip " +
                       "21:l_femur 23:l_tibia 24:l_foot 26:l_toe 30:r_chest 31:r_collar 33:r_uparm 34:r_foarm 35:r_hand " +
                       "36:l_chest 37:l_collar 39:l_uparm 40:l_foarm 41:l_hand"),
            new Layout("Knights of the Round (13)", new[] { "KNIGHT13.D" },
                       "-1 0 1 2 3 4 -1 6 7 8 9 6 11 6 13 6 15 6 17 6 19 20 21 -1 23 24 25 26 -1 28 29 30 30 32 32 34 29 36 36 38 38 40 -1 42 43 44 45 46",
                       "1:head 6:hip 7:l_hip 8:l_femur 9:l_tibia 10:l_foot 19:r_hip 20:r_femur 21:r_tibia 22:r_foot " +
                       "23:l_chest 24:l_collar 25:l_uparm 26:l_foarm 27:l_hand 28:chest 42:r_chest 43:r_collar " +
                       "44:r_uparm 45:r_foarm 46:r_hand"),
            new Layout("Bahamut", new[] { "BAHAMDAT.D" },
                       "-1 0 1 2 3 4 0 6 7 8 9 8 11 0 13 14 15 16 0 18 19 20 21 0 23 24 25 26 25 28 -1 30 31 32 33 -1 35 36 37 38 -1 40 41 42 43 44 45",
                       "0:chest 1:l_chest 2:l_collar 3:l_uparm 4:l_foarm 5:l_hand 6:l_wing_a_1 7:l_wing_a_2 8:l_wing_a_3 " +
                       "9:l_wing_a_4 10:l_wing_a_5 13:neck_1 14:neck_2 15:neck_3 16:head 17:jaw 18:r_chest 19:r_collar " +
                       "20:r_uparm 21:r_foarm 22:r_hand 23:r_wing_a_1 24:r_wing_a_2 25:r_wing_a_3 26:r_wing_a_4 " +
                       "27:r_wing_a_5 30:l_hip 31:l_femur 32:l_tibia 33:l_foot 34:l_toe 35:r_hip 36:r_femur 37:r_tibia " +
                       "38:r_foot 39:r_toe 40:tail_1 41:tail_2 42:tail_3 43:tail_4 44:tail_5 45:tail_6 46:tail_7"),
            new Layout("Neo Bahamut", new[] { "BAHAMRDA.D" },
                       "-1 0 1 0 3 4 0 6 7 0 9 0 11 0 13 0 15 0 17 0 19 20 21 22 23 24 25 26 27 27 27 30 31 32 30 -1 35 36 37 38 -1 40 41 42 -1 44 45 46",
                       "0:chest 2:l_wing_a_1 3:l_collar 4:l_uparm 5:l_foarm 6:r_collar 7:r_uparm 8:r_foarm 10:l_wing_b_1 " +
                       "12:r_wing_b_1 14:r_wing_a_1 16:r_wing_c_1 18:l_wing_c_1 20:neck_1 21:neck_2 22:neck_3 23:neck_4 " +
                       "24:neck_5 25:neck_6 26:neck_7 27:head 28:jaw 36:tail_1 37:tail_2 38:tail_3 39:tail_4 40:r_hip " +
                       "41:r_femur 42:r_tibia 43:r_foot 44:l_hip 45:l_femur 46:l_tibia 47:l_foot"),
            new Layout("Bahamut ZERO", new[] { "BAHAMZDA.D" },
                       "-1 0 1 2 3 4 5 4 7 8 9 4 11 12 13 0 0 16 17 18 0 20 21 22 0 24 25 26 0 0 29 30 31 0 33 34 35 0 37 38 39 -1 41 42 43 44 -1 -1",
                       "0:chest 1:neck_1 2:neck_2 3:neck_3 4:neck_4 5:head 6:jaw 15:l_uparm 17:l_wing_a_1 18:l_wing_a_2 " +
                       "19:l_wing_a_3 21:l_wing_b_1 22:l_wing_b_2 23:l_wing_b_3 25:l_wing_c_1 26:l_wing_c_2 " +
                       "27:l_wing_c_3 28:r_uparm 30:r_wing_a_1 31:r_wing_a_2 32:r_wing_a_3 34:r_wing_b_1 35:r_wing_b_2 " +
                       "36:r_wing_b_3 38:r_wing_c_1 39:r_wing_c_2 40:r_wing_c_3 42:tail_1 43:tail_2 44:tail_3 45:tail_4 " +
                       "46:l_femur 47:r_femur"),
            new Layout("Leviathan", new[] { "RIVADAT.D" },
                       "-1 0 1 2 3 4 5 6 7 7 9 10 11 12 13 14 7 16 17 18 19 20 21 4 23 4 25 2 27 2 29 -1 31 32 33 34 35 36 37 38 39 40 41 42 43 44",
                       "0:chest 1:neck_1 2:neck_2 3:neck_3 4:neck_4 5:neck_5 6:neck_6 7:head 8:jaw 31:tail_1 32:tail_2 " +
                       "33:tail_3 34:tail_4 35:tail_5 36:tail_6 37:tail_7 38:tail_8 39:tail_9 40:tail_10 41:tail_11 " +
                       "42:tail_12 43:tail_13 44:tail_14 45:tail_15"),
            new Layout("Phoenix", new[] { "PHEONIXD.D" },
                       "-1 0 1 2 3 1 5 6 0 8 0 10 11 12 10 14 15 -1 17 18 19 -1 21 22 23 21 25 26 21 28 29 28 31 28 33 28 35 28 37 38 39 40 41 -1 43 44 45",
                       "0:chest 2:l_wing_b_1 3:l_wing_b_2 4:l_wing_b_3 5:l_wing_a_1 6:l_wing_a_2 7:l_wing_a_3 8:neck " +
                       "9:head 11:r_wing_b_1 12:r_wing_b_2 13:r_wing_b_3 14:r_wing_a_1 15:r_wing_a_2 16:r_wing_a_3 " +
                       "17:l_hip 18:l_femur 19:l_tibia 20:l_foot 28:tail_1 37:tail_2 38:tail_3 39:tail_4 40:tail_5 " +
                       "41:tail_6 42:tail_7 43:r_hip 44:r_femur 45:r_tibia 46:r_foot"),
            new Layout("Kujata", new[] { "KUJATADA.D" },
                       "-1 0 1 2 3 4 5 4 7 4 1 10 11 12 1 14 15 16 -1 18 19 20 -1 22 23 24 25 26 27 -1 29 30 31",
                       "0:hip 2:chest 3:head 10:l_chest 11:l_uparm 12:l_foarm 13:l_hand 14:r_chest 15:r_uparm 16:r_foarm " +
                       "17:r_hand 18:l_hip 19:l_femur 20:l_tibia 21:l_foot 23:tail_1 24:tail_2 25:tail_3 26:tail_4 " +
                       "27:tail_5 28:tail_6 29:r_hip 30:r_femur 31:r_tibia 32:r_foot"),
            new Layout("Typhoon", new[] { "TUPONDAT.D" },
                       "-1 0 0 2 0 4 -1 6 7 8 9 -1 11 11 13 11 15 16 17 -1 19 20 21 22 23 22 25 22 27 22 29 21 31 -1 33 34 35 36 37 36 39 36 41 36 43 35 45",
                       "0:chest 1:head 19:l_chest 20:l_uparm 21:l_foarm 22:l_hand 33:r_chest 34:r_uparm 35:r_foarm " +
                       "36:r_hand"),
            new Layout("Choco/Mog", new[] { "MOGURIDA.D" },
                       "-1 0 1 2 3 0 5 0 7 -1 9 10 11 12 13 12 15 12 17 12 19 10 21 22 10 24 10 26 10 28 29 10 31 10 33 -1 35 36 37 38 38 -1 41 42 43 44 44",
                       "0:dhip 1:dchest 2:dneck 3:dhead 6:dr_wing_a_1 8:dl_wing_a_1 11:chest 12:head 21:l_chest " +
                       "22:l_uparm 23:l_foarm 24:l_hip 25:l_femur 26:r_hip 27:r_femur 28:r_chest 29:r_uparm 30:r_foarm " +
                       "32:l_wing_a_1 34:r_wing_a_1 35:dl_hip 36:dl_femur 37:dl_tibia 38:dl_foot 40:dl_toe 41:dr_hip " +
                       "42:dr_femur 43:dr_tibia 44:dr_foot 46:dr_toe"),
            new Layout("Fat Chocobo", new[] { "DEBCYODA.D" },
                       "-1 0 1 1 3 0 5 0 7 -1 9 -1 11 12 13 14 13 16 13 18 -1 20 21 22 23 22 25 22 27",
                       "0:chest 1:head 6:l_wing_a_1 8:r_wing_a_1 10:tail_1 11:l_hip 12:l_femur 13:l_foot 20:r_hip " +
                       "21:r_femur 22:r_foot"),
            new Layout("Mog Dance", new[] { "MOGDAT.D" },
                       "-1 0 1 2 1 4 1 6 1 8 0 10 0 12 -1 14 -1 16 17 -1 19 20 -1 22",
                       "0:chest 1:head 11:r_wing_a_1 13:l_wing_a_1 14:l_hip 15:l_femur 16:l_chest 17:l_uparm 18:l_foarm " +
                       "19:r_chest 20:r_uparm 21:r_foarm 22:r_hip 23:r_femur"),
            new Layout("Game Over", new[] { "DEATHDAT.D" },
                       "-1 0 1 2 3 4 1 1 7 8 9 1 11 12 11 14 11 16 11 18",
                       "0:hip 1:chest 2:r_collar 3:r_uparm 4:r_foarm 5:r_hand 6:head 7:l_collar 8:l_uparm 9:l_foarm " +
                       "10:l_hand"),
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
