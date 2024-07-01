using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Linq;

namespace RespecWrath
{
    [HarmonyPatch(typeof(SpendSkillPoint), nameof(SpendSkillPoint.Check))]
    internal static class SpendSkillPoint_check_patch
    {
        [HarmonyPostfix]
        public static void Postfix(SpendSkillPoint __instance, LevelUpState state, UnitDescriptor unit,
            ref bool __result)
        {
            __result = StatTypeHelper.Skills.Contains(__instance.Skill) &&
                       unit.Stats.GetStat(__instance.Skill).BaseValue < state.NextCharacterLevel &&
                       state.SkillPointsRemaining >= 0;
        }
    }

    [HarmonyPatch(typeof(CharGenContextVM), nameof(CharGenContextVM.CompleteLevelUp))]
    internal static class CompleteLevelUp_patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharGenContextVM __instance)
        {
            if (__instance?.CharGenVM?.Value?.CharacterLevel < 2 ||
                __instance?.CharGenVM?.Value?.CharacterLevel == null)
            {
                try
                {
                    __instance?.CloseCharGen();
                }
                catch (Exception e)
                {
                    Main.logger.Log(e.ToString());
                    throw;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(LevelUpState.CharBuildMode), typeof(bool) })]
    [HarmonyPriority(9999)]
    internal static class LevelUpState_ctor_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
        {
            if (!Main.IsEnabled || Main.IsRespec != true || unit == null) return;
            try
            {
                if (unit.IsPet == false)
                {
                    var bu = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("047b715d404d5f245ad37019b5b6f1de");
                    if (unit.CharacterName.Contains("Nenio") && !unit.HasFact(bu))
                    {
                        unit.AddFact(bu);
                    }

                    if (!unit.IsMC() && unit.IsStoryCompanionLocal() &&
                        !Main.settings?.FullRespecStoryCompanion == true ||
                        unit.Blueprint.ToString().Contains("_Companion") && !unit.IsMC() &&
                        !Main.settings?.FullRespecStoryCompanion == true)
                    {
                        foreach (BlueprintFeature feat in unit?.Blueprint?.Race?.Features)
                        {
                            if (!unit.HasFact(feat))
                            {
                                unit.AddFact(feat);
                            }
                        }

                        foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Blueprint.Race.Features
                                     .OfType<BlueprintFeatureSelection>())
                        {
                            blueprintFeatureSelection.Obligatory = false;
                            __instance.AddSelection(null, unit.Blueprint.Race, blueprintFeatureSelection, 0);
                        }
                    }
                }

                unit.Parts.m_Parts.Clear();
                foreach (Spellbook spellbook in unit.Spellbooks)
                {
                    spellbook.UpdateAllSlotsSize(true);
                    spellbook.UpdateMythicLevel();
                }

                int[] initStatsByUnit = Main.GetInitStatsByUnit(unit);
                unit.Descriptor.Stats.Strength.BaseValue = initStatsByUnit[0];
                unit.Descriptor.Stats.Dexterity.BaseValue = initStatsByUnit[1];
                unit.Descriptor.Stats.Constitution.BaseValue = initStatsByUnit[2];
                unit.Descriptor.Stats.Intelligence.BaseValue = initStatsByUnit[3];
                unit.Descriptor.Stats.Wisdom.BaseValue = initStatsByUnit[4];
                unit.Descriptor.Stats.Charisma.BaseValue = initStatsByUnit[5];
                if (Main.featurestoadd != null)
                    foreach (BlueprintFeature featuretoadd in Main.featurestoadd)
                    {
                        if (!unit.HasFact(featuretoadd))
                        {
                            unit.Descriptor.AddFact(featuretoadd);
                        }
                    }

                if (unit?.Progression?.Race?.SelectableRaceStat == true &&
                    unit?.OriginalBlueprint?.AssetGuidThreadSafe != "ae766624c03058440a036de90a7f2009")
                {
                    if (!Main.settings.FullRespecStoryCompanion && !unit.IsMC())
                    {
                        __instance.CanSelectRaceStat = true;
                    }
                }

                if (unit?.IsCustomCompanion() == true && unit.IsPet == false ||
                    unit.IsMainCharacter && unit.IsPet == false || unit?.IsStoryCompanionLocal() == true &&
                    Main.settings.FullRespecStoryCompanion)
                {
                    Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
                    __instance.CanSelectVoice = true;
                    if (unit?.Blueprint?.CharacterName != "Camellia")
                    {
                        __instance.CanSelectAlignment = true;
                    }
                    else
                    {
                        __instance.CanSelectAlignment = false;
                    }

                    if (Main.settings.PreservePortrait)
                    {
                        __instance.CanSelectPortrait = false;
                    }
                    else
                    {
                        __instance.CanSelectPortrait = true;
                    }

                    __instance.CanSelectRace = true;
                    __instance.CanSelectGender = true;
                    __instance.CanSelectName = true;
                    if (Main.settings.PreserveVoice)
                    {
                        __instance.CanSelectVoice = false;
                    }
                    else
                    {
                        __instance.CanSelectVoice = true;
                    }
                }
                else if (unit.IsMC() && !unit.IsPet)
                {
                    Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
                    __instance.StatsDistribution.Available = true;
                    //Does not prevent name from appearing, does cause corrent value to load into input field and disregarding changes with current input timing
                    if (Main.settings.PreserveMCName)
                    {
                        __instance.CanSelectName = false;
                    }
                    else
                    {
                        __instance.CanSelectName = true;
                    }

                    if (Main.settings.PreserveVoice)
                    {
                        __instance.CanSelectVoice = false;
                    }
                    else
                    {
                        __instance.CanSelectVoice = true;
                    }

                    if (Main.settings.PreserveMCAlignment)
                    {
                        __instance.CanSelectAlignment = false;
                    }
                    else
                    {
                        __instance.CanSelectAlignment = true;
                    }

                    if (Main.settings.PreservePortrait)
                    {
                        __instance.CanSelectPortrait = false;
                    }

                    __instance.CanSelectGender = true;
                }
                else if (unit.IsStoryCompanionLocal() && !Main.settings.FullRespecStoryCompanion ||
                         unit.Blueprint.ToString().Contains("_Companion") && unit.IsPet == false &&
                         !Main.settings.FullRespecStoryCompanion)
                {
                    Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.LevelUp);
                    __instance.StatsDistribution.Available = true;
                    __instance.CanSelectAlignment = false;
                    __instance.CanSelectRace = false;
                    __instance.CanSelectPortrait = false;
                    __instance.CanSelectGender = false;
                    __instance.CanSelectName = false;
                    __instance.CanSelectVoice = false;
                }
            }
            catch (NullReferenceException nullref)
            {
                Main.logger.Error(nullref.ToString());
            }
        }
    }
}