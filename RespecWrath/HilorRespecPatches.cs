using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;

namespace RespecWrath
{
    [HarmonyPatch(typeof(RespecCompanion), nameof(RespecCompanion.RunAction))]
    internal static class RespecCompanion_RunAction_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = true;
        }
    }

    [HarmonyPatch(typeof(RespecCompanion), nameof(RespecCompanion.FinishRespecialization))]
    public static class FinishRespecHilor
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = false;
        }
    }
}