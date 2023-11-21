using HarmonyLib;
using Kingmaker.UI.MVVM._VM.CharGen;
using System;

namespace RespecWrath
{
    [HarmonyPatch(typeof(CharGenVM), "NeedNamePhase")]
    internal static class NeedName_Patch
    {
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec == true)
                {
                    if (__instance.m_LevelUpController.Unit.IsMC() && Main.settings.PreserveMCName)
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
    }

    /*
        [HarmonyPatch(typeof(CharGenVM), "NeedRacePhase")]
        internal static class NeedRace_Patch
        {
            private static void Postfix(ref bool __result)
            {
                try
                {
                    if (Main.IsRespec == true)
                    {
                        if (Main.EntityUnit.IsMC() && !Main.settings.FullRespecStoryCompanion)
                        {
                            //__result = false;
                            return;
                        }
                    }
                }
                catch (Exception e) { Main.logger.Log(e.ToString()); }
            }
        }
    */

    [HarmonyPatch(typeof(CharGenVM), "NeedAlignmentPhase")]
    internal static class NeedAlignment_Patch
    {
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec == true)
                {
                    if (__instance.m_LevelUpController.Unit.IsMC() && Main.settings.PreserveMCAlignment)
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
    }

    [HarmonyPatch(typeof(CharGenVM), "NeedPortraitPhase")]
    internal static class NeedPortrait_Patch
    {
        private static void Postfix(ref bool __result)
        {
            try
            {
                if (Main.IsRespec == true)
                {
                    if (Main.settings.PreservePortrait)
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
    }
}