using HarmonyLib;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecWrathFork
{
	[HarmonyPatch(typeof(CharGenVM), "NeedNamePhase")]
	internal static class NeedName_Patch
	{
		private static void Postfix(ref bool __result)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsMC() && !Main.settings.PreserveMCBiographicalInformation)
					{
						__result = false;
						return;
					}
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
/*
	[HarmonyPatch(typeof(CharGenVM), "NeedRacePhase")]
	internal static class NeedRace_Patch
	{
		private static void Postfix(ref bool __result)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsMC() && !Main.settings.FullRespecStoryCompanion)
					{
						//__result = false;
						return;
					}
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
*/
	[HarmonyPatch(typeof(CharGenVM), "NeedAlignmentPhase")]
	internal static class NeedAlignment_Patch
	{
		private static void Postfix(ref bool __result)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsMC() && Main.settings.PreserveMCBiographicalInformation)
					{
						__result = false;
						return;
					}
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}

	[HarmonyPatch(typeof(CharGenVM), "NeedPortraitPhase")]
	internal static class NeedPortrait_Patch
	{
		private static void Postfix(ref bool __result)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsMC() && Main.settings.PreserveMCBiographicalInformation)
					{
						__result = false;
						return;
					}
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
}
