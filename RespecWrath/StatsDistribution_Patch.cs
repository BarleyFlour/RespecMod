using HarmonyLib;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace RespecWrath
{
    ///[HarmonyPatch(typeof(StatsDistribution), MethodType.Constructor)]
    [HarmonyPatch(typeof(StatsDistribution), "Start")]
    internal static class StatsDistributionPatch
    {
        private static void Prefix(ref int pointCount)
        {
            if (Main.IsRespec == true)
            {
                pointCount = Main.settings.PointsCount;
            }
        }

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

    /*[HarmonyPatch(typeof(StatsDistribution), "IsComplete")]

	internal static class StatsDistributionPatch_IsComplete
	{
		private static void Postfix(StatsDistribution __instance, ref bool __result)
		{
			if (Main.IsRespec == true)
			{
				//__result = true;
			}
		}
	}*/
}