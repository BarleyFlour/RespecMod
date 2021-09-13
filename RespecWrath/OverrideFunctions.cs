using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecWrathFork
{
	[HarmonyPatch(typeof(LevelUpController), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(bool), typeof(LevelUpState.CharBuildMode) })]
	[HarmonyPriority(9999)]
	internal static class LevelUpController_ctor_Patch
	{
		private static void Postfix(LevelUpController __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
		{
			try
			{
				if (Main.IsRespec == true)
				{
					if (Main.EntityUnit.IsMC() && !Main.settings.FullRespecStoryCompanion)
					{
						__instance.SelectRace(Main.EntityUnit.Progression.Race);
						__instance.SelectGender(Main.EntityUnit.Gender);
						__instance.SetBirthDay(Main.EntityUnit.Descriptor.BirthDay, Main.EntityUnit.Descriptor.BirthMonth);
						__instance.SelectVoice(Main.EntityUnit.Descriptor.Asks);
						
						FeatureSelectionState s = __instance.State.Selections.Find(x => x.Source.Blueprint.name.Equals(Stuf.DeitySelect.name));
                        if (s == null)
                        {
							Main.logger.Log("Couldn't get diety selection this way");
                        }
						else
                        {

							BlueprintFeatureSelection deity = BlueprintRoot.Instance.Progression.DeitySelection;
							

							//__instance.SelectFeature(BlueprintRoot.Instance.Progression.DeitySelection, s.SelectedItem = s.);
                        }
						return;
					}
				}
			}
			catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
}
