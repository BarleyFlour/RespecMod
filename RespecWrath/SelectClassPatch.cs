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
	/*[HarmonyPatch(typeof(SelectClass), "Apply")]
	internal static class SelectClass_Apply_Patch
	{
		private static void Prefix(SelectClass __instance, LevelUpState state, UnitDescriptor unit)
		{
			 /*   if(Main.IsEnabled == false){return;}
				if (unit.Progression.Experience > 0 && unit.Progression.CharacterLevel == 0 && Main.IsRespec == true)
				{
				state.StatsDistribution.Start(Main.PointsCount);
				foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Progression.Race.Features.OfType<BlueprintFeatureSelection>())
					{
						state.AddSelection(null, unit.Progression.Race, blueprintFeatureSelection, 0);
					}
				}*//*
		}
	}*/
	[HarmonyPatch(typeof(SelectClass), "Apply")]
	internal static class SelectClass_Apply_Patch
	{
		/*private static void Prefix(SelectClass __instance, LevelUpState state, UnitDescriptor unit)
		{
			if (Main.IsRespec == true)
			{
				if (unit.IsCustomCompanion() || unit.IsMainCharacter)
				{
					Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
					state.CanSelectAlignment = true;
					state.CanSelectPortrait = true;
					state.CanSelectRace = true;
					state.CanSelectGender = true;
					state.CanSelectName = true;
					state.CanSelectVoice = true;
				}
				if (unit.IsCustomCompanion() == false && unit.IsMainCharacter == false)
				{
					Traverse.Create(state).Field("Mode").SetValue(LevelUpState.CharBuildMode.Respec);
					Traverse.Create(state.StatsDistribution).Property("Available", null).SetValue(true);
					Traverse.Create(state).Property("CanSelectName", null).SetValue(false);
					Traverse.Create(state).Property("CanSelectVoice", null).SetValue(false);
					state.CanSelectAlignment = false;
					state.CanSelectRace = false;
					state.CanSelectPortrait = false;
					state.CanSelectGender = false;
					state.CanSelectName = false;
					state.CanSelectVoice = false;
					return;
				}
			}
		}*/
	}
}
