using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace RespecWrath
{
    [HarmonyPatch]
    internal static class SaveHooker
    {
        [HarmonyPatch(typeof(ZipSaver))]
        [HarmonyPatch("SaveJson"), HarmonyPostfix]
        private static void Zip_Saver(string name, ZipSaver __instance)
        {
            DoSave(name, __instance);
        }

        [HarmonyPatch(typeof(FolderSaver))]
        [HarmonyPatch("SaveJson"), HarmonyPostfix]
        private static void Folder_Saver(string name, FolderSaver __instance)
        {
            DoSave(name, __instance);
        }

        private static void DoSave(string name, ISaver saver)
        {
            if (name != "header")
                return;

            try
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                var writer = new StringWriter();
                serializer.Serialize(writer, GlobalLevelInfo.Instance);
                writer.Flush();
                saver.SaveJson(LoadHooker.FileName, writer.ToString());
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Game))]
    internal static class LoadHooker
    {
        public const string FileName = "header.json.barley_levelrecords";

        [HarmonyPatch("LoadGame"), HarmonyPostfix]
        private static void LoadGame(SaveInfo saveInfo)
        {
            using (saveInfo)
            {
                using (saveInfo.GetReadScope())
                {
                    ThreadedGameLoader.RunSafelyInEditor((Action)(() =>
                    {
                        string raw;
                        using (ISaver saver = saveInfo.Saver.Clone())
                        {
                            raw = saver.ReadJson(FileName);
                        }
                        if (raw != null)
                        {
                            var serializer = new JsonSerializer();
                            var rawReader = new StringReader(raw);
                            var jsonReader = new JsonTextReader(rawReader);
                            GlobalLevelInfo.Instance = serializer.Deserialize<GlobalLevelInfo>(jsonReader);
                        }
                        else
                        {
                            GlobalLevelInfo.Instance = new GlobalLevelInfo();
                        }
                    })).Wait();
                }
            }
        }
    }

    public class GlobalLevelInfo
    {
        public LevelInfo ForCharacter(UnitEntityData unit)
        {
            var key = unit.UniqueId;
            if (!PerCharacter.TryGetValue(key, out var record))
            {
                record = new LevelInfo();
                PerCharacter.Add(key, record);
                //PerCharacter[key] = record;
            }
            return record;
        }

        public Dictionary<string, LevelInfo> PerCharacter = new Dictionary<string, LevelInfo>();
        public static GlobalLevelInfo Instance = new GlobalLevelInfo();

        public class LevelInfo
        {
            public Dictionary<int, StatType> AbilityScoresByLevel = new Dictionary<int, StatType>();
            public Dictionary<int, Dictionary<StatType, int>> SkillsByLevel = new Dictionary<int, Dictionary<StatType, int>>();
        }
    }

    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelUpActions")]
    internal static class ApplyLevelUpActions_Patch
    {
        public static void Prefix(LevelUpController __instance, UnitEntityData unit, List<ILevelUpAction> __result)
        {
            if (!unit.IsPlayerFaction || !Game.Instance.Player.AllCharacters.Contains(unit)) return;
            try
            {
                // __result = __instance.LevelUpActions;
                foreach (var ilevelupaction in __instance.LevelUpActions)
                {
                    if (ilevelupaction.GetType() == typeof(SpendAttributePoint))
                    {
                        var leveleupaction = (SpendAttributePoint)ilevelupaction;
                        if (leveleupaction != null)
                        {
                            var entry = GlobalLevelInfo.Instance.ForCharacter(unit);
                            //Main.logger.Log(leveleupaction.Attribute.ToString());
                            if (entry.AbilityScoresByLevel.TryGetValue(unit.Progression.CharacterLevel + 1, out StatType stat))
                            {
                                entry.AbilityScoresByLevel[unit.Progression.CharacterLevel + 1] = leveleupaction.Attribute;
                            }
                            else
                            {
                                entry.AbilityScoresByLevel.Add(unit.Progression.CharacterLevel + 1, leveleupaction.Attribute);
                            }
                        }
                    }
                    else if (ilevelupaction.GetType() == typeof(SpendSkillPoint))
                    {
                        var leveleupaction = (SpendSkillPoint)ilevelupaction;
                        if (leveleupaction != null)
                        {
                            //  Main.logger.Log(leveleupaction.Skill.ToString());
                            var entry = GlobalLevelInfo.Instance.ForCharacter(unit);
                            //  Main.logger.Log("a");
                            if (entry.SkillsByLevel.TryGetValue(unit.Progression.CharacterLevel + 1, out Dictionary<StatType, int> statentry))
                            {
                                // Main.logger.Log("b");
                                if (statentry.ContainsKey(leveleupaction.Skill))
                                {
                                    statentry[leveleupaction.Skill] = statentry[leveleupaction.Skill] + 1;
                                    //  Main.logger.Log("c");
                                }
                                else
                                {
                                    statentry.Add(leveleupaction.Skill, 1);
                                    //  Main.logger.Log("d");
                                }
                            }
                            else
                            {
                                //Main.logger.Log("e");
                                var statentrynew = new Dictionary<StatType, int>();
                                statentrynew.Add(leveleupaction.Skill, 1);
                                entry.SkillsByLevel.Add(unit.Progression.CharacterLevel + 1, statentrynew);
                                //Main.logger.Log("f");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }
}