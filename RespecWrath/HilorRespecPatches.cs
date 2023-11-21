using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;

namespace RespecWrath
{
    [HarmonyPatch(typeof(RespecCompanion), "RunAction")]
    internal static class RespecCompanion_RunAction_Patch
    {
        private static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = true;
        }
    }

    [HarmonyPatch(typeof(RespecCompanion), "FinishRespecialization")]
    public static class FinishRespecHilor
    {
        public static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = false;
        }
    }
}