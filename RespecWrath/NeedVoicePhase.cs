using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.Voice;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using RespecModBarley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(CharGenVM), "NeedVoicePhase")]
	internal static class NeedVoice_Patch
	{
		private static void Postfix(ref bool __result)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsCustomCompanion() || Main.EntityUnit.IsMainCharacter)
					{
						__result = true;
						return;
					}
				}
			}
			catch(Exception e) { Main.logger.Log(e.ToString()); }
		 }
	}
}
