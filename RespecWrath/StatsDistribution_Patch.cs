using HarmonyLib;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace RespecWrath
{
    [HarmonyPatch(typeof(StatsDistribution), nameof(StatsDistribution.Start))]
    internal static class StatsDistribution_Start_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(ref int pointCount)
        {
            if (Main.IsRespec == true)
            {
                pointCount = Main.settings.PointsCount;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(StatsDistribution __instance, int pointCount)
        {
            if (Main.IsRespec == true)
            {
                if (pointCount != Main.settings.PointsCount)
                {
                    pointCount = Main.settings.PointsCount;
                    __instance.Points = pointCount;
                    __instance.TotalPoints = pointCount;
                }
            }
        }
    }
}