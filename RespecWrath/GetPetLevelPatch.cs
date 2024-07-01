using HarmonyLib;
using Kingmaker.UnitLogic.FactLogic;

namespace RespecWrath
{
    [HarmonyPatch(typeof(AddPet), nameof(AddPet.GetPetLevel))]
    internal static class GetPetLevel
    {
        [HarmonyPostfix]
        private static void Postfix(AddPet __instance, ref int __result)
        {
            if (Main.IsRespec == true)
            {
                __result = 0;
            }
        }
    }
}