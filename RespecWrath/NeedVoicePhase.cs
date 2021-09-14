using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.Voice;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.FeatureSelector;

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
					if (Main.EntityUnit.IsCustomCompanion() || Main.EntityUnit.IsStoryCompanion() && Main.settings.FullRespecStoryCompanion)
					{
						__result = true;
						return;
					}
					if (Main.IsRespec && Main.settings.PreserveVoice)
					{
						__result = false;
						return;
					}


				}
			}
			catch(Exception e) { Main.logger.Log(e.ToString()); }
		 }
	}
	[HarmonyPatch(typeof(CharGenVM), "DefineVisibleFeatureSelections")]
	internal static class DefineVisibleFeatureSelections_Patch
	{
		private static void Postfix(CharGenVM __instance, ref List<FeatureSelectionState> __result)
		{
			try
			{
				if(Main.IsRespec && !Main.settings.BackgroundDeity && (Main.EntityUnit.IsStoryCompanion()) )
                {
                    __result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.DeitySelect);
					__result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.BackgroundsBaseSelection);

					
				}
            }
			catch(Exception e)
            {
				Main.logger.Log(e.ToString());
            }
		}
	}
}
