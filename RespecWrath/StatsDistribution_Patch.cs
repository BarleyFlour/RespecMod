using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{

	///[HarmonyPatch(typeof(StatsDistribution), MethodType.Constructor)]
	[HarmonyPatch(typeof(StatsDistribution),"Start")]

	internal static class StatsDistributionPatch
	{
		private static void Prefix(ref int pointCount)
        {
			if(Main.IsRespec == true)
			{
				pointCount = Main.PointsCount;
			}
        }
		private static void Postfix(StatsDistribution __instance, int pointCount)
        {
			if (Main.IsRespec == true)
            {
				if (pointCount != Main.PointsCount)
				{
					pointCount = Main.PointsCount;
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

