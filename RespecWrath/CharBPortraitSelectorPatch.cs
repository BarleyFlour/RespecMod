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
using Kingmaker.UI.LevelUp;
using Kingmaker;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.Portrait;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Enums;

/*namespace RespecModBarley
{
	internal static class BluePrintThing
	{
		// Token: 0x06000053 RID: 83 RVA: 0x000039A0 File Offset: 0x00001BA0
		public static TBlueprint[] GetBlueprints<TBlueprint>() where TBlueprint : BlueprintScriptableObject
		{
			return Utilities.GetScriptableObjects<BlueprintScriptableObject>().OfType<TBlueprint>().ToArray();
		}
	}
	[HarmonyPatch(typeof(CharGenPortraitPhaseVM), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(LevelUpController)})]
	internal static class Charbportrait_patch
	{
		private static void Postfix(CharGenPortraitPhaseVM __instance)
		{
			try
			{
				foreach (var bp in BluePrintThing.GetBlueprints<BlueprintPortrait>())
				{
					var o = new CharGenPortraitSelectorItemVM(bp);

					__instance.m_AllPortraitsCollection.Add(o);
					CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = new CharGenPortraitSelectorItemVM(bp, false);
					__instance.m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
					PortraitCategory portraitCategory = bp.Data.PortraitCategory;
					if (!__instance.PortraitGroupVms.ContainsKey(portraitCategory))
					{
						__instance.PortraitGroupVms.Add(portraitCategory, new CharGenPortraitGroupVM(portraitCategory));
						__instance.PortraitGroupVms[portraitCategory].Expanded.Value = (portraitCategory != PortraitCategory.KingmakerNPC);
					}
					__instance.PortraitGroupVms[portraitCategory].Add(charGenPortraitSelectorItemVM);
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
}
*/