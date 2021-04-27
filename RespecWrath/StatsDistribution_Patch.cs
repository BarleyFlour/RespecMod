using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
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
		private static void Postfix(StatsDistribution __instance, int pointCount)
        {
			if (Main.IsRespec == true)
            {
			  pointCount = Main.PointsCount;
			  __instance.Points = Main.PointsCount;
			  __instance.TotalPoints = Main.PointsCount;
			  __instance.TotalPoints = Main.PointsCount;
		    }
     	}

	}
}

