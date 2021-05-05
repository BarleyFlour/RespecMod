using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(Player), "OnAreaLoaded")]
	internal static class OnAreaLoadedPatch
	{
		private static void Postfix()
		{
			Main.GetUnitsForMemory();
			foreach (UnitEntityData data in Game.Instance.Player.AllCharacters)
			{
				data.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			}
		}
	}
}