using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
    [HarmonyPatch]
    static class SaveHooker
    {

        [HarmonyPatch(typeof(ZipSaver))]
        [HarmonyPatch("SaveJson"), HarmonyPostfix]
        static void Zip_Saver(string name, ZipSaver __instance)
        {
            DoSave(name, __instance);
        }

        [HarmonyPatch(typeof(FolderSaver))]
        [HarmonyPatch("SaveJson"), HarmonyPostfix]
        static void Folder_Saver(string name, FolderSaver __instance)
        {
            DoSave(name, __instance);
        }

        static void DoSave(string name, ISaver saver)
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
            catch(Exception e)
            {
                RespecModBarley.Main.logger.Log(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Game))]
    static class LoadHooker
    {
        public const string FileName = "header.json.barley_levelrecords";

        [HarmonyPatch("LoadGame"), HarmonyPostfix]
        static void LoadGame(SaveInfo saveInfo)
        {
            using (saveInfo)
            {
                using (saveInfo.GetReadScope())
                {
                    ThreadedGameLoader.RunSafelyInEditor((Action)(() => {
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
            public Dictionary<int,Dictionary<StatType,int>> SkillsByLevel = new Dictionary<int, Dictionary<StatType, int>>();
        }
    }
    //[HarmonyPatch(typeof(ApplySkillPoints), "Apply")]
    static class SpendSkillPoint_Patch
    {
        public static void Postfix(ApplySkillPoints __instance, LevelUpState state, UnitDescriptor unit)
        {
            try
            {
                if (!unit.IsPlayerFaction) return;
                Main.logger.Log(unit.ToString());
                Main.logger.Log(unit.Unit.ToString());
                Main.logger.Log(unit.Unit.Progression.ToString());
                var entry = GlobalLevelInfo.Instance.ForCharacter(unit.Unit);
                if (entry.SkillsByLevel.TryGetValue(unit.Unit.Progression.CharacterLevel + 1, out Dictionary<StatType, int> statentry))
                {
                   // if(statentry.ContainsKey(__instance.Skill))
                    {
                       // statentry[__instance.Skill] = statentry[__instance.Skill] + 1;
                    }
                   // else
                    {
                      //  statentry.Add(__instance.Skill, 1);
                    }
                }
               // else
                {
                    var statentrynew = new Dictionary<StatType, int>();
                  //  statentry.Add(__instance.Skill, 1);
                }
                //entry.SkillsByLevel[unit.Unit.Progression.CharacterLevel + 1] = new Dictionary<StatType, int>();
            }
            catch(Exception e)
            {
                RespecModBarley.Main.logger.Log(e.ToString());
            }
        }
    }
   // [HarmonyPatch(typeof(SpendAttributePoint), "Apply")]
    static class AddStatPoint_Patch
    {
        public static void Postfix(AddStatPoint __instance, LevelUpState state,UnitDescriptor unit)
        {
            Main.logger.Log("AWDFadsfsdaf");
            try
            {
               // if (!unit.IsPlayerFaction) return;
                var entry = GlobalLevelInfo.Instance.ForCharacter(unit.Unit);
                if(entry.AbilityScoresByLevel.TryGetValue(unit.Unit.Progression.CharacterLevel+1,out StatType stat))
                {
                    Main.logger.Log(__instance.Attribute.ToString());
                    entry.AbilityScoresByLevel[unit.Unit.Progression.CharacterLevel] = __instance.Attribute;
                }
                else
                {
                    Main.logger.Log(__instance.Attribute.ToString());
                    entry.AbilityScoresByLevel.Add(unit.Unit.Progression.CharacterLevel,__instance.Attribute);
                }
                //entry.SkillsByLevel[unit.Unit.Progression.CharacterLevel + 1] = new Dictionary<StatType, int>();
            }
            catch (Exception e)
            {
                RespecModBarley.Main.logger.Log(e.ToString());
            }
        }
    }
    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelUpActions")]
    static class ApplyLevelUpActions_Patch
    {
        public static void Prefix(LevelUpController __instance, UnitEntityData unit,List<ILevelUpAction> __result)
        {
            if (!unit.IsPlayerFaction) return;
            try
            {
               // __result = __instance.LevelUpActions;
                foreach(var ilevelupaction in __instance.LevelUpActions)
                {
                    if (ilevelupaction.GetType() == typeof(SpendAttributePoint))
                    {
                        var leveleupaction = (SpendAttributePoint)ilevelupaction;
                        if (leveleupaction != null)
                        {
                            var entry = GlobalLevelInfo.Instance.ForCharacter(unit);
                            //Main.logger.Log(leveleupaction.Attribute.ToString());
                            if (entry.AbilityScoresByLevel.TryGetValue(unit.Progression.CharacterLevel, out StatType stat))
                            {
                                entry.AbilityScoresByLevel[unit.Progression.CharacterLevel] = leveleupaction.Attribute;
                            }
                            else
                            {
                                entry.AbilityScoresByLevel.Add(unit.Progression.CharacterLevel, leveleupaction.Attribute);
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
                            if (entry.SkillsByLevel.TryGetValue(unit.Progression.CharacterLevel, out Dictionary<StatType, int> statentry))
                            {

                               // Main.logger.Log("b");
                                if (statentry.ContainsKey(leveleupaction.Skill))
                                {
                                     statentry[leveleupaction.Skill] = statentry[leveleupaction.Skill]+1;
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
                                entry.SkillsByLevel.Add(unit.Progression.CharacterLevel, statentrynew);
                                //Main.logger.Log("f");
                            }
                        }
                    }


                }
              /*  // if (!unit.IsPlayerFaction) return;
                var entry = GlobalLevelInfo.Instance.ForCharacter(unit);
                if (entry.AbilityScoresByLevel.TryGetValue(unit.Progression.CharacterLevel + 1, out StatType stat))
                {
                    Main.logger.Log(__instance.Attribute.ToString());
                    entry.AbilityScoresByLevel[unit.Progression.CharacterLevel + 1] = __instance.Attribute;
                }
                else
                {
                    Main.logger.Log(__instance.Attribute.ToString());
                    entry.AbilityScoresByLevel.Add(unit.Progression.CharacterLevel, __instance.Attribute);
                }*/
                //entry.SkillsByLevel[unit.Unit.Progression.CharacterLevel + 1] = new Dictionary<StatType, int>();
            }
            catch (Exception e)
            {
                RespecModBarley.Main.logger.Log(e.ToString());
            }
        }
    }
}
