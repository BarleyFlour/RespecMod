using HarmonyLib;
using Kingmaker.UnitLogic.FactLogic;

namespace RespecWrath
{
    [HarmonyPatch(typeof(AddPet), "GetPetLevel")]
    internal static class GetPetLevel
    {
        private static void Postfix(AddPet __instance, ref int __result)
        {
            if (Main.IsRespec == true)
            {
                __result = 0;
            }
        }
    }
}