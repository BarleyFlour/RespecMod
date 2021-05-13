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
	[HarmonyPatch(typeof(AddPet), "GetPetLevel")]
	internal static class GetPetLevel
	{
		private static void Postfix(AddPet __instance, ref int __result)
        {
			if (Main.IsRespec == true)
		    {
				__result = 0;
			}
        }
	}
}