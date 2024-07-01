using HarmonyLib;
using Kingmaker.UI.MVVM._VM.CharGen;
using System;

namespace RespecWrath
{
    [HarmonyPatch(typeof(CharGenVM), nameof(CharGenVM.NeedNamePhase))]
    internal static class CharGenVM_NeedName_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec != true) return;
                if (__instance.m_LevelUpController.Unit.IsMC() && Main.settings.PreserveMCName)
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

    [HarmonyPatch(typeof(CharGenVM), nameof(CharGenVM.NeedAlignmentPhase))]
    internal static class CharGenVM_NeedAlignment_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CharGenVM __instance, ref bool __result)
        {
            try
            {
                if (Main.IsRespec != true) return;
                if (__instance.m_LevelUpController.Unit.IsMC() && Main.settings.PreserveMCAlignment)
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

    [HarmonyPatch(typeof(CharGenVM), nameof(CharGenVM.NeedPortraitPhase))]
    internal static class CharGenVM_NeedPortrait_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(ref bool __result)
        {
            try
            {
                if (Main.IsRespec != true) return;
                if (Main.settings.PreservePortrait)
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
}