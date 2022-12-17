using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SoulsIds;
using SoulsFormats;
using static SoulsFormats.EMEVD.Instruction;
using static SoulsIds.Events;

namespace EldenConvenienceMod
{
    internal class Mods
    {
        public enum Mod
        {
            Unknown,
            Maps,
            Icons,
            Varre,
            Torrent,
            Dungeon,
            Tutorials,
            Achievements,
            Upgrade,
            Purchase,
            Sell,
            Siofra,
        }
        public static readonly SortedSet<Mod> NoMods = new SortedSet<Mod>();
        public static readonly SortedSet<Mod> AllMods = new SortedSet<Mod>(((Mod[])Enum.GetValues(typeof(Mod))).Except(new[] { Mod.Unknown }));
        public static readonly Dictionary<string, Mod> ModNames =
            AllMods.ToDictionary(e => e.ToString().ToLowerInvariant(), e => e);

        public class ModInfo
        {
            public Mod Type { get; set; }
            public bool All { get; set; }
            public string DisplayName { get; set; }
            public string Desc { get; set; }
            public string InternalName => Type.ToString().ToLowerInvariant();
        }
        public static readonly List<ModInfo> AllInfos = new List<ModInfo>
        {
            new ModInfo
            {
                DisplayName = "All Mods",
                Desc = "Install or uninstall all mods at once",
                All = true,
            },
            new ModInfo
            {
                Type = Mod.Maps,
                DisplayName = "Unlock all maps",
                Desc = "All map fragments are unlocked after arriving in Limgrave",
            },
            new ModInfo
            {
                Type = Mod.Icons,
                DisplayName = "Unlock all icons",
                Desc = "All named landmark icons appear after arriving in Limgrave.\nIf the mod is uninstalled mid-playthrough, they will need to be revisited to appear again",
            },
            new ModInfo
            {
                Type = Mod.Varre,
                DisplayName = "Remove invasion requirement for Varré quest",
                Desc = "Talk to Varré after getting the fingers to progress the quest as if invasions had occurred",
            },
            new ModInfo
            {
                Type = Mod.Torrent,
                DisplayName = "Unlock Sites of Grace while riding Torrent",
                Desc = "Add a new prompt to \"Touch grace\" while on horseback.\nThis does not light the Site of Grace, but allows warping to it",
            },
            new ModInfo
            {
                Type = Mod.Dungeon,
                DisplayName = "Warp from dungeons without defeating the boss",
                Desc = "Allow warping away from catacombs, caves, and tunnels without defeating the boss.\nThis excludes Sealed Tunnel, which uses a separate mechanism",
            },
            new ModInfo
            {
                Type = Mod.Tutorials,
                DisplayName = "Don't show tutorials",
                Desc = "Prevent tutorial popups from appearing in menus and in the world, regardless of game options",
            },
            new ModInfo
            {
                Type = Mod.Achievements,
                DisplayName = "Don't award achievements",
                Desc = "Prevent the game from awarding achievements, if not desired during a modded playthrough",
            },
            new ModInfo
            {
                Type = Mod.Upgrade,
                DisplayName = "Additional weapon upgrade menus",
                Desc = "Upgrade your weapon at Sites of Grace and Twin Maiden Husks",
            },
            new ModInfo
            {
                Type = Mod.Purchase,
                DisplayName = "Additional purchase menus",
                Desc = "Add the main Twin Maiden Husks shop to Sites of Grace (does not include Bell Bearing shops)",
            },
            new ModInfo
            {
                Type = Mod.Sell,
                DisplayName = "Additional sell menus",
                Desc = "Sell items at Sites of Grace, Twin Maiden Husks, and Finger Reader Enia",
            },
            new ModInfo
            {
                Type = Mod.Siofra,
                DisplayName = "Skip Siofra and Nokron flame pillar puzzles",
                Desc = "Show prompts to access Ancestor Spirit fights without lighting all flames",
            },
        };

        private static readonly SortedSet<string> tutorialFiles = new SortedSet<string>(
            "common common_func m10_00_00_00 m10_01_00_00 m11_10_00_00 m18_00_00_00 m30_02_00_00 m30_04_00_00 m30_11_00_00 m31_02_00_00 m31_03_00_00 m31_15_00_00 m31_17_00_00 m32_01_00_00 m60_35_44_00 m60_41_36_00 m60_41_38_00 m60_42_36_00 m60_42_37_00 m60_42_38_00 m60_42_40_00 m60_43_35_00 m60_43_36_00 m60_43_37_00 m60_43_38_00 m60_43_39_00 m60_44_34_00 m60_44_35_00 m60_44_36_00 m60_44_36_10 m60_44_37_00 m60_44_37_10 m60_45_37_00 m60_45_37_10 m60_46_36_00 m60_46_36_10 m60_46_38_00 m60_46_38_10".Split(' '));
        private static readonly SortedSet<string> achievementFiles = new SortedSet<string>(
            "common m11_71_00_00 m19_00_00_00".Split(' '));
        public static readonly Dictionary<Mod, List<string>> modFiles = new Dictionary<Mod, List<string>>
        {
            [Mod.Maps] = new List<string> { "/event/common.emevd.dcx" },
            [Mod.Icons] = new List<string> { "WorldMapPointParam.param" },
            [Mod.Varre] = new List<string> { "/event/common.emevd.dcx" },
            [Mod.Torrent] = new List<string> { "/event/common.emevd.dcx", "ActionButtonParam.param", "BonfireWarpParam.param" },
            [Mod.Dungeon] = new List<string> { "MapDefaultInfoParam.param" },
            [Mod.Tutorials] = tutorialFiles.Select(f => $"/event/{f}.emevd.dcx").Concat(new[] { "TutorialParam.param" }).ToList(),
            [Mod.Achievements] = achievementFiles.Select(f => $"/event/{f}.emevd.dcx").ToList(),
            [Mod.Sell] = new List<string> { "/script/talk/m00_00_00_00.talkesdbnd.dcx", "/script/talk/m11_10_00_00.talkesdbnd.dcx" },
            [Mod.Upgrade] = new List<string> { "/script/talk/m00_00_00_00.talkesdbnd.dcx" },
            [Mod.Purchase] = new List<string> { "/script/talk/m00_00_00_00.talkesdbnd.dcx" },
            [Mod.Siofra] = new List<string> { "/event/m12_02_00_00.emevd.dcx" },
        };

        private static readonly Dictionary<Mod, (int, int)> skipCommands = new Dictionary<Mod, (int, int)>
        {
            [Mod.Tutorials] = (2007, 15),
            [Mod.Achievements] = (2003, 28),
        };
        private static readonly Dictionary<Mod, Dictionary<string, List<int>>> editTalkIds = new Dictionary<Mod, Dictionary<string, List<int>>>
        {
            // The actual selling entries are in t102001110_x50 but this would leave entry/select split up
            [Mod.Sell] = new Dictionary<string, List<int>>
            {
                ["t000001000"] = new List<int> { 15000390 }, // Bonfire: Memorize spell
                ["t102001110"] = new List<int> { 20000009 }, // Enia: anything calling x50, which has Leave in it
                ["t600001110"] = new List<int> { 26000010 }, // Twin Husks: Purchase
            },
            [Mod.Upgrade] = new Dictionary<string, List<int>>
            {
                ["t000001000"] = new List<int> { 15000390 }, // Bonfire: Memorize spell
                ["t600001110"] = new List<int> { 26000010 }, // Twin Husks: Purchase
            },
            [Mod.Purchase] = new Dictionary<string, List<int>>
            {
                ["t000001000"] = new List<int> { 15000390 }, // Bonfire: Memorize spell
            },
        };
        private static readonly List<uint> mapInstalledFlags = new List<uint> { 18000021, 6001 };
        // From item randomizer
        private static readonly SortedDictionary<int, int> mapFlags = new SortedDictionary<int, int>
        {
            [8600] = 62010,  // Map: Limgrave, West
            [8601] = 62011,  // Map: Weeping Peninsula
            [8602] = 62012,  // Map: Limgrave, East
            [8603] = 62020,  // Map: Liurnia, East
            [8604] = 62021,  // Map: Liurnia, North
            [8605] = 62022,  // Map: Liurnia, West
            [8606] = 62030,  // Map: Altus Plateau
            [8607] = 62031,  // Map: Leyndell, Royal Capital
            [8608] = 62032,  // Map: Mt. Gelmir
            [8609] = 62040,  // Map: Caelid
            [8610] = 62041,  // Map: Dragonbarrow
            [8611] = 62050,  // Map: Mountaintops of the Giants, West
            [8612] = 62051,  // Map: Mountaintops of the Giants, East
            [8613] = 62060,  // Map: Ainsel River
            [8614] = 62061,  // Map: Lake of Rot
            [8615] = 62063,  // Map: Siofra River
            [8616] = 62062,  // Map: Mohgwyn Palace
            [8617] = 62064,  // Map: Deeproot Depths
            [8618] = 62052,  // Map: Consecrated Snowfield
        };

        internal void Run(
            SortedSet<Mod> install,
            SortedSet<Mod> uninstall,
            Dictionary<string, EMEVD> emevds,
            Dictionary<string, BND4> esds,
            Dictionary<string, PARAM> allParams,
            SortedSet<Mod> check)
        {
            SortedSet<Mod> mods = new SortedSet<Mod>(install.Concat(uninstall));
            if (check == null)
            {
                check = new SortedSet<Mod>();
            }
            else
            {
                install = new SortedSet<Mod>();
                uninstall = new SortedSet<Mod>();
                check.Clear();
            }

            // EMEVD
            Dictionary<(int, int), Mod> skipMods = skipCommands
                .Where(e => mods.Contains(e.Key))
                .ToDictionary(e => e.Value, e => e.Key);
            Dictionary<Mod, SortedSet<string>> checkedCommands = skipCommands
                .Where(e => mods.Contains(e.Key))
                .ToDictionary(e => e.Key, e => new SortedSet<string>());
            // Use same ids from randomizer... just keep them fixed in the future
            int varreEvent = 19003110;
            int mapEvent = 19003111;
            int torrentEvent = 1060606000;
            int torrentAction = 6109;
            bool torrentEventInstalled = false;
            foreach (KeyValuePair<string, EMEVD> entry in emevds)
            {
                string map = entry.Key;
                EMEVD emevd = entry.Value;
                void addNewEvent(
                    int id,
                    ICollection<EMEVD.Instruction> instrs,
                    EMEVD.Event.RestBehaviorType rest = EMEVD.Event.RestBehaviorType.Default)
                {
                    EMEVD.Event ev = new EMEVD.Event(id, rest);
                    ev.Instructions.AddRange(instrs);
                    emevd.Events.Add(ev);
                    emevd.Events[0].Instructions.Add(new EMEVD.Instruction(2000, 0, new List<object> { 0, (uint)id, (uint)0 }));
                }
                void removeEvent(int id)
                {
                    emevd.Events.RemoveAll(x => x.ID == id);
                    emevd.Events[0].Instructions.RemoveAll(ins =>
                    {
                        if (ins.Bank == 2000 && ins.ID == 0)
                        {
                            List<object> args = ins.UnpackArgs(Enumerable.Repeat(ArgType.Int32, ins.ArgData.Length / 4));
                            return args.Count >= 3 && (int)args[1] == id;
                        }
                        return false;
                    });
                }
                if (map == "common" && mods.Contains(Mod.Maps))
                {
                    bool isInstalled = emevd.Events.Any(x => x.ID == mapEvent);
                    if (isInstalled) check.Add(Mod.Maps);
                    if (install.Contains(Mod.Maps))
                    {
                        if (!isInstalled)
                        {
                            List<EMEVD.Instruction> instrs = new List<EMEVD.Instruction>();
                            instrs.Add(new EMEVD.Instruction(3, 0, new List<object> { (sbyte)0, (byte)1, (byte)0, mapInstalledFlags[0] }));
                            foreach (KeyValuePair<int, int> mapFlag in mapFlags)
                            {
                                int flag = mapFlag.Value;
                                instrs.Add(new EMEVD.Instruction(2003, 66, new List<object> { (byte)0, flag, (byte)1 }));
                            }
                            addNewEvent(mapEvent, instrs);
                        }
                    }
                    else if (uninstall.Contains(Mod.Maps))
                    {
                        removeEvent(mapEvent);
                    }
                }
                if (map == "common" && mods.Contains(Mod.Varre))
                {
                    bool isInstalled = emevd.Events.Any(x => x.ID == varreEvent);
                    if (isInstalled) check.Add(Mod.Varre);
                    if (install.Contains(Mod.Varre))
                    {
                        if (!isInstalled)
                        {
                            addNewEvent(varreEvent, new List<EMEVD.Instruction>
                            {
                                new EMEVD.Instruction(1003, 2, new List<object> { (byte)0, (byte)1, (byte)0, 1035449235 }),
                                new EMEVD.Instruction(3, 0, new List<object> { (sbyte)0, (byte)1, (byte)0, 1035449207 }),
                                new EMEVD.Instruction(2003, 66, new List<object> { (byte)0, 1035449235, (byte)1 }),
                                new EMEVD.Instruction(2003, 66, new List<object> { (byte)0, 3198, (byte)1 }),
                            });
                        }
                    }
                    else if (uninstall.Contains(Mod.Varre))
                    {
                        removeEvent(varreEvent);
                    }
                }
                if (map == "common" && mods.Contains(Mod.Torrent))
                {
                    torrentEventInstalled = emevd.Events.Any(x => x.ID == torrentEvent);
                    if (install.Contains(Mod.Torrent) || uninstall.Contains(Mod.Torrent))
                    {
                        // Redo this every time, just to refresh the list
                        removeEvent(torrentEvent);
                    }
                    if (install.Contains(Mod.Torrent) && allParams.TryGetValue("BonfireWarpParam", out PARAM warpParam))
                    {
                        EMEVD.Event ev = new EMEVD.Event(torrentEvent);
                        // Conditions: Flag is off, bonfire is backread, riding horse, action button on bonfire
                        // This shouldn't allow sequence breaking: Fire Giant grace is after the fog wall. Can add a condition for that if needed.
                        ev.Instructions.AddRange(new List<EMEVD.Instruction>
                        {
                            new EMEVD.Instruction(1003, 2, new List<object> { (byte)0, (byte)1, (byte)0, (uint)0 }),
                            new EMEVD.Instruction(5, 10, new List<object> { (sbyte)0, (uint)0, (byte)1, (byte)0, (float)1 }),
                            new EMEVD.Instruction(5, 10, new List<object> { (sbyte)-1, (uint)0, (byte)0, (byte)0, (float)1 }),
                            new EMEVD.Instruction(3, 0, new List<object> { (sbyte)-1, (byte)1, (byte)0, (uint)0 }),
                            new EMEVD.Instruction(4, 32, new List<object> { (sbyte)1, (uint)10000, (byte)1 }),
                            new EMEVD.Instruction(3, 24, new List<object> { (sbyte)1, torrentAction, (uint)0 }),
                            new EMEVD.Instruction(0, 0, new List<object> { (sbyte)-1, (byte)1, (sbyte)1 }),
                            new EMEVD.Instruction(0, 0, new List<object> { (sbyte)0, (byte)1, (sbyte)-1 }),
                            new EMEVD.Instruction(1000, 8, new List<object> { (byte)1, (byte)0, (sbyte)1 }),
                            new EMEVD.Instruction(2003, 66, new List<object> { (byte)0, (uint)0, (byte)1 }),
                            new EMEVD.Instruction(2007, 2, new List<object> { (byte)13 }),
                        });
                        ev.Parameters.AddRange(new List<EMEVD.Parameter>
                        {
                            // Flag
                            new EMEVD.Parameter(0, 4, 0, 4),
                            new EMEVD.Parameter(3, 4, 0, 4),
                            new EMEVD.Parameter(9, 4, 0, 4),
                            // Asset
                            new EMEVD.Parameter(1, 4, 4, 4),
                            new EMEVD.Parameter(2, 4, 4, 4),
                            new EMEVD.Parameter(5, 8, 4, 4),
                        });
                        emevd.Events.Add(ev);
                        // All initializations
                        int slot = 0;
                        foreach (PARAM.Row row in warpParam.Rows)
                        {
                            // Only bother if horse is possible here.
                            // Entity id is more reliable than map coord position, but this tends to overinclude maps, so it should be fine.
                            byte area = (byte)row["areaNo"].Value;
                            byte block = (byte)row["gridXNo"].Value;
                            byte region = (byte)row["gridZNo"].Value;
                            if (area == 60 || area == 12 || (area == 34 && block == 11))
                            {
                                uint flag = (uint)row["eventflagId"].Value;
                                // We could use chr instead (which is consistently asset-1000), but asset works fine with IfAssetBackread
                                // The Player is asset-970, but it can't be used with SetPlayerRespawnPoint
                                uint asset = (uint)row["bonfireEntityId"].Value;
                                List<object> args = new List<object> { slot++, torrentEvent, flag, asset };
                                emevd.Events[0].Instructions.Add(new EMEVD.Instruction(2000, 0, args));
                            }
                        }
                    }
                }
                if (map == "m12_02_00_00" && mods.Contains(Mod.Siofra))
                {
                    bool allInstalled = true;
                    foreach (int warpId in new[] { 12022609, 12022629 })
                    {
                        EMEVD.Event ev = emevd.Events.Find(x => x.ID == warpId);
                        bool isInstalled = false;
                        if (ev != null)
                        {
                            int gotoIndex = ev.Instructions.FindIndex(ins => ins.Bank == 1003 && ins.ID == 101);
                            if (gotoIndex >= 0)
                            {
                                isInstalled = RunEvent(
                                    ev, gotoIndex, install.Contains(Mod.Siofra), uninstall.Contains(Mod.Siofra));
                            }
                        }
                        allInstalled &= isInstalled;
                    }
                    if (allInstalled)
                    {
                        check.Add(Mod.Siofra);
                    }
                }
                if (skipMods.Count == 0) continue;
                foreach (EMEVD.Event ev in emevd.Events)
                {
                    for (int j = ev.Instructions.Count - 1; j >= 0; j--)
                    {
                        EMEVD.Instruction ins = ev.Instructions[j];
                        (int, int) key = (ins.Bank, ins.ID);
                        if (skipMods.TryGetValue(key, out Mod mod))
                        {
                            bool isInstalled = RunEvent(ev, j, install.Contains(mod), uninstall.Contains(mod));
                            if (isInstalled)
                            {
                                checkedCommands[mod].Add(map);
                            }
                        }
                    }
                }
            }
            bool eventTutorials = false;
            foreach (KeyValuePair<Mod, SortedSet<string>> entry in checkedCommands)
            {
                if (entry.Key == Mod.Tutorials && tutorialFiles.All(f => entry.Value.Contains(f)))
                {
                    // This is somewhat incompatible with randomizer, which just removes the commands, so they never show up as skipped.
                    // No real issues result from it. The mod still works.
                    eventTutorials = true;
                }
                if (entry.Key == Mod.Achievements && achievementFiles.All(f => entry.Value.Contains(f)))
                {
                    check.Add(Mod.Achievements);
                }
            }

            // ESD
            (string, int) parseMachine(string name)
            {
                string[] parts = name.Split('_');
                if (!AST.ParseMachine(parts[1], out int machine)) throw new Exception($"Internal error: badly format {name}");
                return (parts[0], machine);
            }
            Dictionary<(string, int), List<Mod>> esdModes = editTalkIds
                .Where(e => mods.Contains(e.Key))
                .SelectMany(e => e.Value.SelectMany(esdEntry => esdEntry.Value.Select(msgId => (e.Key, (esdEntry.Key, msgId)))))
                .GroupBy(e => e.Item2)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Item1).ToList());
            List<string> esdNames = esdModes.Select(e => e.Key.Item1).ToList();
            Dictionary<Mod, SortedSet<(string, int)>> checkedMachines = editTalkIds
                .Where(e => mods.Contains(e.Key))
                .ToDictionary(e => e.Key, e => new SortedSet<(string, int)>());
            foreach (KeyValuePair<string, BND4> entry in esds)
            {
                foreach (BinderFile bndFile in entry.Value.Files)
                {
                    string name = GameEditor.BaseName(bndFile.Name);
                    if (!esdNames.Contains(name)) continue;
                    ESD esd = ESD.Read(bndFile.Bytes);
                    foreach (KeyValuePair<(string, int), List<Mod>> edit in esdModes)
                    {
                        (string esdId, int msgId) = edit.Key;
                        if (esdId == name)
                        {
                            foreach (Mod mod in edit.Value.OrderBy(x => x))
                            {
                                List<long> machineIds = ESDEdits.FindMachinesWithTalkData(esd, msgId);
                                if (machineIds.Count == 0) continue;
                                bool isInstalled = true;
                                foreach (long machineId in machineIds)
                                {
                                    isInstalled &= RunMachine(
                                        esd.StateGroups[machineId],
                                        mod,
                                        install.Contains(mod),
                                        uninstall.Contains(mod));
                                }
                                if (isInstalled)
                                {
                                    checkedMachines[mod].Add(edit.Key);
                                }
                            }
                        }
                    }
                    if (install.Count > 0 || uninstall.Count > 0)
                    {
                        bndFile.Bytes = esd.Write();
                    }
                }
            }
            foreach (KeyValuePair<Mod, SortedSet<(string, int)>> entry in checkedMachines)
            {
                if (entry.Value.Count == editTalkIds[entry.Key].Count)
                {
                    check.Add(entry.Key);
                }
            }

            // Params
            PARAM param;
            // MapDefaultInfoParam.txt:30070000[EnableFastTravelEventFlagId]
            if (mods.Contains(Mod.Torrent) && allParams.TryGetValue("ActionButtonParam", out param))
            {
                if (param[torrentAction] != null)
                {
                    if (torrentEventInstalled)
                    {
                        check.Add(Mod.Torrent);
                    }
                    if (uninstall.Contains(Mod.Torrent))
                    {
                        param.Rows.RemoveAll(r => r.ID == torrentAction);
                    }
                }
                if (install.Contains(Mod.Torrent))
                {
                    PARAM.Row touchRow = param[6100];
                    PARAM.Row row = new PARAM.Row(torrentAction, null, param.AppliedParamdef);
                    GameEditor.CopyRow(touchRow, row);
                    // Higher priority?
                    row["category"].Value = (byte)2;
                    row["isGrayoutForRide"].Value = (byte)0;
                    // Larger range. 1.5 by default
                    row["radius"].Value = 3f;
                    param.Rows.Add(row);
                }
            }
            if (mods.Contains(Mod.Dungeon) && allParams.TryGetValue("MapDefaultInfoParam", out param))
            {
                bool allInstalled = true;
                foreach (PARAM.Row row in param.Rows)
                {
                    if (row.ID < 30000000 || row.ID >= 33000000) continue;
                    // Overwrite this one way or another, don't check previous value
                    uint flag = (uint)row["EnableFastTravelEventFlagId"].Value;
                    uint rowFlag = (uint)(row.ID + 800);
                    allInstalled &= flag == 0;
#if DEBUG
                    if (flag > 10000000 && flag != rowFlag) throw new Exception($"Mismatched map {row.ID} flag {flag}, expected {rowFlag}");
#endif
                    if (install.Contains(Mod.Dungeon))
                    {
                        row["EnableFastTravelEventFlagId"].Value = (uint)0;
                    }
                    else if (uninstall.Contains(Mod.Dungeon))
                    {
                        row["EnableFastTravelEventFlagId"].Value = (uint)rowFlag;
                    }
                }
                if (allInstalled)
                {
                    check.Add(Mod.Dungeon);
                }
            }
            if (mods.Contains(Mod.Tutorials) && allParams.TryGetValue("TutorialParam", out param))
            {
                bool allInstalled = eventTutorials;
                foreach (PARAM.Row row in param.Rows)
                {
                    byte menuType = (byte)row["menuType"].Value;
                    bool isInstalled = menuType % 100 >= 50;
                    allInstalled &= isInstalled;
                    if (install.Contains(Mod.Tutorials))
                    {
                        if (!isInstalled)
                        {
                            row["menuType"].Value = menuType + 50;
                        }
                    }
                    else if (uninstall.Contains(Mod.Tutorials) && isInstalled)
                    {
                        row["menuType"].Value = menuType - 50;
                    }
                }
                if (allInstalled)
                {
                    check.Add(Mod.Tutorials);
                }
            }
            if (mods.Contains(Mod.Icons) && allParams.TryGetValue("WorldMapPointParam", out param))
            {
                bool allInstalled = true;
                foreach (PARAM.Row row in param.Rows)
                {
                    if ((int)row["textId1"].Value <= 0) continue;
                    uint flagId = (uint)row["eventFlagId"].Value;
                    uint backupId = (uint)row["textDisableFlagId8"].Value;
                    bool isInstalled = mapInstalledFlags.Contains(flagId);
                    allInstalled &= isInstalled;
                    if (install.Contains(Mod.Icons))
                    {
                        if (!isInstalled)
                        {
                            row["eventFlagId"].Value = mapInstalledFlags[0];
                            if (backupId == 0)
                            {
                                row["textDisableFlagId8"].Value = flagId;
                            }
                        }
                    }
                    else if (uninstall.Contains(Mod.Icons) && isInstalled)
                    {
                        if (backupId > 0)
                        {
                            row["eventFlagId"].Value = backupId;
                            row["textDisableFlagId8"].Value = 0;
                        }
                    }
                }
                if (allInstalled)
                {
                    check.Add(Mod.Icons);
                }
            }
        }

        private bool RunMachine(
            Dictionary<long, ESD.State> machine,
            Mod mod,
            bool install,
            bool uninstall)
        {
            // TODO: Migrate to ESDEdits
            ESDEdits.CustomTalkData data;
            if (mod == Mod.Sell)
            {
                data = new ESDEdits.CustomTalkData
                {
                    LeaveMsg = 20000009,
                    Msg = 20000011,
                    ConsistentID = 65,
                };
            }
            else if (mod == Mod.Upgrade)
            {
                data = new ESDEdits.CustomTalkData
                {
                    LeaveMsg = 20000009,
                    Msg = 22130001,
                    ConsistentID = 66,
                };
            }
            else if (mod == Mod.Purchase)
            {
                data = new ESDEdits.CustomTalkData
                {
                    LeaveMsg = 20000009,
                    Msg = 26000010,
                    ConsistentID = 67,
                };
            }
            else throw new Exception($"Internal error: unknown ESD mod {mod}");

            bool isInstalled = ESDEdits.ModifyCustomTalkEntry(machine, data, install, uninstall, out long resultStateId);
            // (long resultStateId, ESD.State resultState) = AST.AllocateState(machine, ref baseId);
            if (machine.TryGetValue(resultStateId, out ESD.State resultState))
            {
                int waitType;
                if (mod == Mod.Sell)
                {
                    // c1_46 OpenSellShop(-1, -1)
                    // c1_141(6)
                    resultState.EntryCommands.AddRange(new List<ESD.CommandCall>
                    {
                        AST.MakeCommand(1, 46, -1, -1),
                        AST.MakeCommand(1, 141, 6),
                    });
                    waitType = 6;
                }
                else if (mod == Mod.Upgrade)
                {
                    // c1_49 CombineMenuFlagAndEventFlag(6001, 232) - also 233 234 235
                    // c1_141(9)
                    // c1_24 OpenEnhanceShop(0)
                    resultState.EntryCommands.AddRange(new List<ESD.CommandCall>
                    {
                        AST.MakeCommand(1, 49, 6001, 232),
                        AST.MakeCommand(1, 49, 6001, 233),
                        AST.MakeCommand(1, 49, 6001, 234),
                        AST.MakeCommand(1, 49, 6001, 235),
                        AST.MakeCommand(1, 141, 9),
                        AST.MakeCommand(1, 24, 0),
                    });
                    waitType = 9;
                }
                else if (mod == Mod.Purchase)
                {
                    // c1_22 OpenRegularShop(101800, 101899)
                    resultState.EntryCommands.AddRange(new List<ESD.CommandCall>
                    {
                        AST.MakeCommand(1, 22, 101800, 101899),
                    });
                    waitType = 5;
                }
                else throw new Exception();

                // f59 CheckSpecificPersonMenuIsOpen
                // f58 CheckSpecificPersonGenericDialogIsOpen
                // Sell shop:
                // f59 f58 assert not (CheckSpecificPersonMenuIsOpen(6, 0) == 1 and not CheckSpecificPersonGenericDialogIsOpen(0))
                // Strengthen shop: 9 instead. Purchase: 5 instead.
                AST.Expr waitExpr = new AST.BinaryExpr
                {
                    Op = "||",
                    Lhs = new AST.BinaryExpr { Op = "==", Lhs = AST.MakeFunction("f59", waitType, 0), Rhs = AST.MakeVal(0) },
                    Rhs = AST.MakeFunction("f58", 0),
                };
                resultState.Conditions[0].Evaluator = AST.AssembleExpression(waitExpr);
            }

            return isInstalled;
        }

        private bool RunEvent(
            EMEVD.Event ev,
            int index,
            bool install,
            bool uninstall)
        {
            bool isInstalled = false;
            if (index > 0 && index < ev.Instructions.Count)
            {
                EMEVD.Instruction pre = ev.Instructions[index - 1];
                if (pre.Bank == 1000 && pre.ID == 3)
                {
                    List<object> args = pre.UnpackArgs(new List<ArgType> { ArgType.Byte });
                    isInstalled = (byte)args[0] == 1;
                }
            }
            if ((install && !isInstalled) || (uninstall && isInstalled))
            {
                OldParams oldParams = OldParams.Preprocess(ev);
                if (install)
                {
                    if (!isInstalled)
                    {
                        ev.Instructions.Insert(index, new EMEVD.Instruction(1000, 3, new List<object> { (byte)1 }));
                    }
                }
                else if (uninstall && isInstalled)
                {
                    ev.Instructions.RemoveAt(index - 1);
                }
                oldParams.Postprocess();
            }
            return isInstalled;
        }
    }
}
