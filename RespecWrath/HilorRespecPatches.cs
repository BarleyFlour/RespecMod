using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecWrath
{
	[HarmonyPatch(typeof(RespecCompanion), "RunAction")]
	internal static class GetPetLevel
	{
		private static void Postfix()
		{
			RespecModBarley.Main.IsHilorRespec = true;
		}
	}
	[HarmonyPatch(typeof(RespecCompanion), "FinishRespecialization")]
	public static class FinishRespecHilor
    {
		public static void Postfix()
        {
			RespecModBarley.Main.IsHilorRespec = false;
        }
    }
}
