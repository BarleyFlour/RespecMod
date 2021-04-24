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
	[HarmonyPatch(typeof(StatsDistribution), MethodType.Constructor)]

	internal static class StatsDistributionPatch
	{
		private static void Postfix(StatsDistribution __instance)
        {
			__instance.Points = 0;
			__instance.TotalPoints = 0;

		}

	}
}

