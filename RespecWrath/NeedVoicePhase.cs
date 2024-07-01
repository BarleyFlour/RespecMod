using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;

namespace RespecWrath
{
    [HarmonyPatch(typeof(CharGenVM), nameof(CharGenVM.NeedVoicePhase))]
    internal static class CharGenVM_NeedVoice_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec != true) return;
                if (__instance.m_LevelUpController.Unit.IsCustomCompanion() ||
                    __instance.m_LevelUpController.Unit.IsStoryCompanionLocal() &&
                    Main.settings.FullRespecStoryCompanion && !Main.settings.PreserveVoice)
                {
                    __result = true;
                    return;
                }

                if (Main.settings.PreserveVoice)
                {
                    __result = false;
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(CharGenVM), nameof(CharGenVM.DefineVisibleFeatureSelections))]
    internal static class CharGenVM_DefineVisibleFeatureSelections_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharGenVM __instance, ref List<FeatureSelectionState> __result)
        {
            try
            {
                if (Main.IsRespec != true) return;
                if (!Main.settings.BackgroundDeity && __instance.m_LevelUpController.Unit.IsStoryCompanionLocal())
                {
                    __result.RemoveAll(a => a.Selection as BlueprintFeatureBase == DeityBackground.DeitySelect);
                    __result.RemoveAll(a =>
                        a.Selection as BlueprintFeatureBase ==
                        DeityBackground.BackgroundSelect as BlueprintFeatureBase);
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }
}