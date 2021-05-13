using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;
using Object = UnityEngine.Object;

namespace RespecModBarley
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(Player), "OnAreaLoaded")]
	internal static class KingmakerRunnerStartPatch
	{
		private static void Postfix()
		{
			try
			{
				Main.GetUnitsForMemory();
			}
			catch(Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
}