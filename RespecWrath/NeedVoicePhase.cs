using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;

namespace RespecWrath
{
    [HarmonyPatch(typeof(CharGenVM), "NeedVoicePhase")]
    internal static class NeedVoice_Patch
    {
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec == true)
                {
                    if (__instance.m_LevelUpController.Unit.IsCustomCompanion() || __instance.m_LevelUpController.Unit.IsStoryCompanionLocal() && Main.settings.FullRespecStoryCompanion && !Main.settings.PreserveVoice)
                    {
                        __result = true;
                        return;
                    }
                    if (Main.IsRespec && Main.settings.PreserveVoice)
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
    }

    [HarmonyPatch(typeof(CharGenVM), "DefineVisibleFeatureSelections")]
    internal static class DefineVisibleFeatureSelections_Patch
    {
        private static void Postfix(CharGenVM __instance, ref List<FeatureSelectionState> __result)
        {
            try
            {
                if (Main.IsRespec && !Main.settings.BackgroundDeity && (__instance.m_LevelUpController.Unit.IsStoryCompanionLocal()))
                {
                    __result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.DeitySelect);
                    __result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.BackgroundSelect as BlueprintFeatureBase);
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }
}