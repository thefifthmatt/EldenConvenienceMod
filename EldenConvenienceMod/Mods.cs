using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoulsIds;
using SoulsFormats;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using static SoulsIds.GameSpec;
using static EldenConvenienceMod.Installer;
using System.Reflection;
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
            Tutorials,
            Achievements,
            Sell,
            Upgrade,
            Siofra,
        }
        public static readonly SortedSet<Mod> NoMods = new SortedSet<Mod>();
        public static readonly SortedSet<Mod> AllMods = new SortedSet<Mod>(((Mod[])Enum.GetValues(typeof(Mod))).Except(new[] { Mod.Unknown }));
        public static readonly Dictionary<string, Mod> ModNames =
            AllMods.ToDictionary(e => e.ToString().ToLowerInvariant(), e => e);

        public class ModInfo
        {
            public Mod Type { get; set; }
            public string DisplayName { get; set; }
            public string Desc { get; set; }
            public string InternalName => Type.ToString().ToLowerInvariant();
        }
        public static readonly List<ModInfo> AllInfos = new List<ModInfo>
        {
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
                Type = Mod.Tutorials,
                DisplayName = "Don't show tutorials",
                Desc = "Prevent tutorial popups from appearing in menus and in the world",
            },
            new ModInfo
            {
                Type = Mod.Achievements,
                DisplayName = "Don't award achievements",
                Desc = "Prevent the game from awarding achievements, if not desired during a modded playthrough",
            },
            new ModInfo
            {
                Type = Mod.Sell,
                DisplayName = "Additional sell menus",
                Desc = "Sell items at Sites of Grace, Twin Maiden Husks, and Finger Reader Enia",
            },
            new ModInfo
            {
                Type = Mod.Upgrade,
                DisplayName = "Additional weapon upgrade menus",
                Desc = "Upgrade your weapon at Sites of Grace and Twin Maiden Husks",
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
            [Mod.Tutorials] = tutorialFiles.Select(f => $"/event/{f}.emevd.dcx").Concat(new[] { "TutorialParam.param" }).ToList(),
            [Mod.Achievements] = achievementFiles.Select(f => $"/event/{f}.emevd.dcx").ToList(),
            [Mod.Sell] = new List<string> { "/script/talk/m00_00_00_00.talkesdbnd.dcx", "/script/talk/m11_10_00_00.talkesdbnd.dcx" },
            [Mod.Upgrade] = new List<string> { "/script/talk/m00_00_00_00.talkesdbnd.dcx" },
            [Mod.Siofra] = new List<string> { "/event/m12_02_00_00.emevd.dcx" },
        };

        private static readonly Dictionary<Mod, (int, int)> skipCommands = new Dictionary<Mod, (int, int)>
        {
            [Mod.Tutorials] = (2007, 15),
            [Mod.Achievements] = (2003, 28),
        };
        private static readonly Dictionary<Mod, List<string>> editMachines = new Dictionary<Mod, List<string>>
        {
            // The actual selling entries are in t102001110_x50 but this would leave entry/select split up
            [Mod.Sell] = new List<string> { "t000001000_x32", "t102001110_x41", "t102001110_x43", "t102001110_x44", "t102001110_x47", "t102001110_x48", "t600001110_x3" },
            [Mod.Upgrade] = new List<string> { "t000001000_x32", "t600001110_x3" },
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
            foreach (KeyValuePair<string, EMEVD> entry in emevds)
            {
                string map = entry.Key;
                EMEVD emevd = entry.Value;
                void addNewEvent(int id, ICollection<EMEVD.Instruction> instrs, EMEVD.Event.RestBehaviorType rest = EMEVD.Event.RestBehaviorType.Default)
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
            Dictionary<(string, int), List<Mod>> machineMods = editMachines
                .Where(e => mods.Contains(e.Key))
                .SelectMany(e => e.Value.Select(m => (e.Key, m)))
                .GroupBy(e => e.Item2)
                .ToDictionary(g => parseMachine(g.Key), g => g.Select(e => e.Item1).ToList());
            List<string> esdNames = machineMods.Select(e => e.Key.Item1).ToList();
            Dictionary<Mod, SortedSet<(string, int)>> checkedMachines = editMachines
                .Where(e => mods.Contains(e.Key))
                .ToDictionary(e => e.Key, e => new SortedSet<(string, int)>());
            foreach (KeyValuePair<string, BND4> entry in esds)
            {
                foreach (BinderFile bndFile in entry.Value.Files)
                {
                    string name = GameEditor.BaseName(bndFile.Name);
                    if (!esdNames.Contains(name)) continue;
                    ESD esd = ESD.Read(bndFile.Bytes);
                    foreach (KeyValuePair<(string, int), List<Mod>> edit in machineMods)
                    {
                        if (edit.Key.Item1 == name
                            && esd.StateGroups.TryGetValue(edit.Key.Item2, out Dictionary<long, ESD.State> machine))
                        {
                            foreach (Mod mod in edit.Value.OrderByDescending(x => x))
                            {
                                bool isInstalled = RunMachine(
                                    machine,
                                    mod,
                                    install.Contains(mod),
                                    uninstall.Contains(mod));
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
                if (entry.Value.Count == editMachines[entry.Key].Count)
                {
                    check.Add(entry.Key);
                }
            }

            // Params
            PARAM param;
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
            // action:20000011:"Sell"
            // action:22130001:"Strengthen armament"
            int msg = mod == Mod.Sell ? 20000011 : 22130001;
            int leaveMsg = 20000009;
            int talkListId = mod == Mod.Sell ? 65 : 66;
            int talkState = mod == Mod.Sell ? 65 : 66;
            bool isInstalled = false;

            // Required
            long loopId = -1;
            long entryId = -1;
            long checkId = -1;
            foreach (KeyValuePair<long, ESD.State> stateEntry in machine)
            {
                ESD.State state = stateEntry.Value;
                // ClearTalkListData c1_20
                if (state.EntryCommands.Any(c => c.CommandBank == 1 && c.CommandID == 20)) loopId = stateEntry.Key;
                // AddTalkListData c1_19
                // There may be multiple states like this, so finding the last one is fine
                if (state.EntryCommands.Any(c => c.CommandBank == 1 && c.CommandID == 19)) entryId = stateEntry.Key;
                // GetTalkListEntryResult f23 == 3 condition
                foreach (ESD.Condition cond in state.Conditions)
                {
                    bool found = false;
                    AST.AstVisitor talkListEntryVisitor = AST.AstVisitor.PostAct(expr =>
                    {
                        found |= expr is AST.FunctionCall call && call.Name == "f23";
                    });
                    AST.DisassembleExpression(cond.Evaluator).Visit(talkListEntryVisitor);
                    if (found)
                    {
                        checkId = stateEntry.Key;
                        break;
                    }
                }
            }
            if (loopId == -1 || entryId == -1 || checkId == -1)
            {
                if (install)
                {
                    throw new Exception($"Can't install {mod} mod: ESD missing states {loopId} {entryId} {checkId}");
                }
                // If it can't be installed, don't count it as such
                return false;
            }
            bool isTalkEntry(ESD.CommandCall c, int findMsg)
            {
                if (findMsg == -1) return (c.CommandBank == 1 || c.CommandBank == 5) && c.CommandID == 19;
                return c.CommandBank == 1 && c.CommandID == 19 && c.Arguments.Count == 3
                    && AST.DisassembleExpression(c.Arguments[1]) is AST.ConstExpr con
                    && con.AsInt() == findMsg;
            }

            // Search for existing talk list entry
            ESD.CommandCall existingEntry = machine[entryId].EntryCommands.Find(c => isTalkEntry(c, msg));
            isInstalled = existingEntry != null;

            if (!install && !uninstall) return isInstalled;

            // If it exists, try to uninstall it
            List<int> usedTalkIds = new List<int>();
            if (isInstalled)
            {
                machine[entryId].EntryCommands.Remove(existingEntry);
                int findCheck = -1;
                if (AST.DisassembleExpression(existingEntry.Arguments[0]) is AST.ConstExpr talkCon)
                {
                    findCheck = talkCon.AsInt();
                }
                // Find condition
                ESD.Condition existingCond = null;
                foreach (ESD.Condition cond in machine[checkId].Conditions)
                {
                    int talkCheck = -1;
                    AST.AstVisitor talkListEntryVisitor = AST.AstVisitor.PostAct(expr =>
                    {
                        // For the moment, check for things of the form GetTalkListEntryResult() == 7
                        if (expr is AST.BinaryExpr bin
                            && bin.Lhs is AST.FunctionCall call && call.Name == "f23"
                            && bin.Rhs is AST.ConstExpr con) {
                            talkCheck = con.AsInt();
                        }
                    });
                    AST.DisassembleExpression(cond.Evaluator).Visit(talkListEntryVisitor);
                    if (talkCheck != -1)
                    {
                        usedTalkIds.Add(talkCheck);
                        if (existingCond == null && talkCheck == findCheck)
                        {
                            existingCond = cond;
                        }
                    }
                }
                if (existingCond != null)
                {
                    machine[checkId].Conditions.Remove(existingCond);
                    if (existingCond.TargetState is long destState)
                    {
                        machine.Remove(destState);
                    }
                }
            }
            if (!install) return isInstalled;

            // If we're installing, find an unused state and talk list id
            while (machine.ContainsKey(talkState)) talkState++;
            while (usedTalkIds.Contains(talkListId)) talkListId++;

            // Add new talk list entry
            ESD.State entryState = machine[entryId];
            int leaveEntry = entryState.EntryCommands.FindLastIndex(c => isTalkEntry(c, leaveMsg));
            if (leaveEntry == -1)
            {
                // Prefer to put it before the "Leave" command
                // Otherwise, avoid interfering with non-talk commands, if possible
                leaveEntry = entryState.EntryCommands.FindLastIndex(c => isTalkEntry(c, -1));
                leaveEntry = leaveEntry == -1 ? entryState.EntryCommands.Count : leaveEntry + 1;
            }
            ESD.CommandCall newEntry = AST.MakeCommand(1, 19, talkListId, msg, -1);
            entryState.EntryCommands.Insert(leaveEntry, newEntry);

            long baseId = talkState;
            (long resultStateId, ESD.State resultState) = AST.AllocateState(machine, ref baseId);

            if (mod == Mod.Sell)
            {
                // c1_46 OpenSellShop(-1, -1)
                // c1_141(6)
                resultState.EntryCommands.AddRange(new List<ESD.CommandCall>
                {
                    AST.MakeCommand(1, 46, -1, -1),
                    AST.MakeCommand(1, 141, 6),
                });
            }
            else
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
            }

            // f59 CheckSpecificPersonMenuIsOpen
            // f58 CheckSpecificPersonGenericDialogIsOpen
            // Sell shop:
            // f59 f58 assert not (CheckSpecificPersonMenuIsOpen(6, 0) == 1 and not CheckSpecificPersonGenericDialogIsOpen(0))
            // Strengthen shop:
            // f59 f58 assert not (CheckSpecificPersonMenuIsOpen(9, 0) == 1 and not CheckSpecificPersonGenericDialogIsOpen(0))
            int waitType = mod == Mod.Sell ? 6 : 9;
            AST.Expr waitExpr = new AST.BinaryExpr
            {
                Op = "||",
                Lhs = new AST.BinaryExpr { Op = "==", Lhs = AST.MakeFunction("f59", waitType, 0), Rhs = AST.MakeVal(0) },
                Rhs = AST.MakeFunction("f58", 0),
            };
            resultState.Conditions.Add(new ESD.Condition(loopId, AST.AssembleExpression(waitExpr)));

            // Add talk condition for state
            AST.Expr buyCond = new AST.BinaryExpr { Op = "==", Lhs = AST.MakeFunction("f23"), Rhs = AST.MakeVal(talkListId) };
            machine[checkId].Conditions.Insert(0, new ESD.Condition(resultStateId, AST.AssembleExpression(buyCond)));

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
