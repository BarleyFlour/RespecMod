using System;
using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace RespecWrath
{
    [HarmonyPatch(typeof(CharGenContextVM), nameof(CharGenContextVM.HandleRespecInitiate))]
    internal static class CharGenContextVM_HandleRespecInitiate_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            RespecWrath.Main.IsHilorRespec = true;
        }
    }
    [HarmonyPatch(typeof(CharGenContextVM), nameof(CharGenContextVM.CloseWithoutComplete))]
    public static class CharGenContextVM_CloseWithoutComplete_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = false;
        }
    }
    [HarmonyPatch(typeof(CharGenContextVM), nameof(CharGenContextVM.CloseRespec))]
    public static class CharGenContextVM_CloseRespec_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            RespecWrath.Main.IsHilorRespec = false;
        }
    }
}