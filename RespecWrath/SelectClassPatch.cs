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

namespace RespecWrath
{
	[HarmonyPatch(typeof(SelectClass), "Apply")]
	internal static class SelectClass_Apply_Patch
	{
		private static void Prefix(SelectClass __instance, LevelUpState state, UnitDescriptor unit)
		{
				if (state.Mode != LevelUpState.CharBuildMode.CharGen && unit.Progression.Experience > 0 && unit.Progression.CharacterLevel == 0 && unit.Progression.Race)
				{
					foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Progression.Race.Features.OfType<BlueprintFeatureSelection>())
					{
						state.AddSelection(null, unit.Progression.Race, blueprintFeatureSelection, 0);
					}
				}
		}
	}
}
