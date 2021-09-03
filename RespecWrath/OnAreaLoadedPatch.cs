using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
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
			try
			{
				///Main.logger.Log("onload");
				Main.GetUnitsForMemory();
				foreach (UnitEntityData data in Game.Instance.Player.AllCharacters)
				{
					if (!data.IsPet && data.IsStoryCompanion())
					{
						///Main.logger.Log("Unit:" + data.CharacterName + data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit);
						data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
						data.Blueprint.GetComponent<MythicLevelLimit>().LevelLimit = 0;
					}
				}
			}
			catch(Exception e)
            {
				Main.logger.Error(e.ToString());
            }
		}
	}
}