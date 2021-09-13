using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.Voice;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using RespecWrathFork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecWrathFork
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
					if (Main.EntityUnit.IsCustomCompanion() || Main.EntityUnit.IsMC() && Main.settings.FullRespecStoryCompanion || Main.EntityUnit.IsStoryCompanion() && Main.settings.FullRespecStoryCompanion)
					{
						__result = true;
						return;
					}
					if (Main.IsRespec && Main.EntityUnit.IsMC() && !Main.settings.FullRespecStoryCompanion)
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
				if(Main.IsRespec && !Main.settings.BackgroundDeity && (Main.EntityUnit.IsStoryCompanion() || Main.EntityUnit.IsMC()) )
                {
					__result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.DeitySelect);
					__result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.BackgroundsBaseSelection);

					
				}
				
				/*else if(Main.IsRespec && Main.settings.BackgroundDeity && Main.EntityUnit.IsStoryCompanion())
                {
					__result.Add(Stuf.DeitySelect);
					new FeatureSelectionState(null,null,Stuf.DeitySelect,)
					__result.Add(Stuf.BackgroundsBaseSelection.);
				}*/
				/*if (Main.IsRespec && !Main.FullRespecStoryCompanion && !Main.EntityUnit.Descriptor.IsCustomCompanion() && !Main.EntityUnit.Descriptor.IsMainCharacter && Main.EntityUnit.IsStoryCompanion() || Main.IsRespec && !Main.BackgroundDeity && !Main.EntityUnit.Descriptor.IsCustomCompanion() && !Main.EntityUnit.Descriptor.IsMainCharacter && Main.EntityUnit.IsStoryCompanion())
				{
					__result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.DeitySelect);
					__result.RemoveAll(a => a.Selection as BlueprintFeatureBase == Stuf.BackgroundsBaseSelection);
				}*/
			}
			catch(Exception e)
            {
				Main.logger.Log(e.ToString());
            }
		}
	}
	/*[HarmonyPatch(typeof(CharGenVM), "NeedAlignmentPhase")]
	internal static class NeedAlignmentPhase_Patch
	{
		private static void Postfix(CharGenVM __instance, ref bool __result)
		{
			if (Main.FullRespecStoryCompanion && __instance.PreviewUnit.Value.Blueprint.NameForAcronym == "Camelia_Companion")
			{
				__result = false;
				return;
			}
		}
	}*/
}
