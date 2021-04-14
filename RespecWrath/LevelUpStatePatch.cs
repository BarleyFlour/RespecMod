using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RespecModBarley
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(LevelUpState.CharBuildMode) })]
	internal static class LevelUpState_ctor_Patch
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000041B4 File Offset: 0x000023B4
		private static void Postfix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
		{
			try
			{
				if (__instance.NextCharacterLevel == 1 && unit.Progression.Experience > 0)
				{
					var blueprintUnit = Game.Instance.BlueprintRoot.SelectablePlayerCharacters.Where(u => u == unit.Blueprint).FirstOrDefault();
					bool flag = unit.Blueprint == Game.Instance.BlueprintRoot.DefaultPlayerCharacter;
					bool flag2 = flag;
					if (flag || Main.extraPoints == Main.ExtraPointsType.P25)
					{
						__instance.StatsDistribution.Start(25);
					}
					else
					{
						__instance.AttributePoints = 0;
						Traverse.Create(__instance.StatsDistribution).Property("Available", null).SetValue(false);
						Traverse.Create(__instance.StatsDistribution).Property("Points", null).SetValue(0);
						Traverse.Create(__instance.StatsDistribution).Property("TotalPoints", null).SetValue(0);
					}
					if (!unit.IsCustomCompanion())
					{
						Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
						__instance.CanSelectAlignment = true;
						__instance.CanSelectPortrait = true;
						__instance.CanSelectRace = true;
						__instance.CanSelectGender = true;
						__instance.CanSelectName = true;
						__instance.CanSelectVoice = true;
					}
					if (unit.IsStoryCompanion())
					{
						Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.LevelUp);
						Traverse.Create(__instance.StatsDistribution).Property("Available", null).SetValue(true);
						if (unit.Progression.Race == Stuf.HumanRace || unit.Progression.Race == Stuf.HalfElfRace || unit.Progression.Race == Stuf.HalfOrcRace)
						{
							__instance.CanSelectRaceStat = true;
						}
						__instance.CanSelectAlignment = false;
						__instance.CanSelectRace = false;
						__instance.CanSelectPortrait = false;
						__instance.CanSelectGender = false;
						__instance.CanSelectName = false;
						__instance.CanSelectVoice = false;
						return;
					}
				}
			}
			catch (Exception ex)
			{
				Main.logger.Log(ex.ToString());
			}
		}
	}
}


