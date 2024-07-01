using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using System;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;

namespace RespecWrath
{
    [HarmonyPatch(typeof(Recruit), nameof(Recruit.SwitchToCompanion), typeof(RecruitData))]
    internal static class HandleRecruit_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(Recruit __instance, RecruitData data)
        {
            try
            {
#if DEBUG
                Main.logger.Log($"Recruited: {data.CompanionBlueprint.CharacterName}");
#endif
                Main.isrecruit = true;
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Recruit __instance, RecruitData data)
        {
            try
            {
                Main.isrecruit = false;
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Recruit),  nameof(Recruit.RunAction))]
    [HarmonyPatch(new Type[] { })]
    internal static class HandleRecruitRun_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(Recruit __instance)
        {
            try
            {
                Main.isrecruit = true;
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }
}