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
using Object = UnityEngine.Object;

namespace RespecModBarley
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(LevelUpState.CharBuildMode), typeof(bool) })]
	[HarmonyPriority(9999)]
	internal static class LevelUpState_ctor_Patch
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000041B4 File Offset: 0x000023B4
		private static void Postfix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode, bool isPregen)
		{
			///unit.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			if (Main.IsRespec == true)
			{
				///var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
				
				try
				{
					if (Main.IsEnabled == true)
					{
						if (unit.Progression.Experience > 0 && unit.Progression.CharacterLevel == 0 && Main.IsRespec == true)
						{
							foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Progression.Race.Features.OfType<BlueprintFeatureSelection>())
							{
								__instance.AddSelection(null, unit.Progression.Race, blueprintFeatureSelection, 0);
							}
						}
						if (Main.IsRespec == true && unit.Progression.Experience > 0 && unit.Progression.Experience > 0)
						{
							foreach (Spellbook spellbook in unit.Spellbooks)
							{
								spellbook.UpdateAllSlotsSize(true);
								spellbook.UpdateMythicLevel();
							}
							int[] initStatsByUnit = Main.GetInitStatsByUnit(unit);
							unit.Descriptor.Stats.Strength.BaseValue = initStatsByUnit[0];
							unit.Descriptor.Stats.Dexterity.BaseValue = initStatsByUnit[1];
							unit.Descriptor.Stats.Constitution.BaseValue = initStatsByUnit[2];
							unit.Descriptor.Stats.Intelligence.BaseValue = initStatsByUnit[3];
							unit.Descriptor.Stats.Wisdom.BaseValue = initStatsByUnit[4];
							unit.Descriptor.Stats.Charisma.BaseValue = initStatsByUnit[5];
							if (unit.Pets != null)
							{
								unit.Pets.Clear();
							}
					        unit.Parts.m_Parts.Clear();
							foreach (BlueprintFeature featuretoadd in Main.featurestoadd)
							{
								///Main.logger.Log(featuretoadd.ToString());
								unit.Descriptor.AddFact(featuretoadd);
							}
						///	if (unit.Progression.Race == Stuf.HumanRace || unit.Progression.Race == Stuf.HalfElfRace || unit.Progression.Race == Stuf.HalfOrcRace)
							{
								__instance.CanSelectRaceStat = true;
							}
							var blueprintUnit = Game.Instance.BlueprintRoot.SelectablePlayerCharacters.Where(u => u == unit.Blueprint).FirstOrDefault();
							if (unit.IsCustomCompanion() || unit.IsMainCharacter)
							{
								Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
								__instance.CanSelectAlignment = true;
								__instance.CanSelectPortrait = true;
								__instance.CanSelectRace = true;
								__instance.CanSelectGender = true;
								__instance.CanSelectName = true;
								__instance.CanSelectVoice = true;
							}
							if (unit.IsStoryCompanion())
							{
								Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.LevelUp);
								Traverse.Create(__instance.StatsDistribution).Property("Available", null).SetValue(true);
								Traverse.Create(__instance).Property("CanSelectName", null).SetValue(false);
								Traverse.Create(__instance).Property("CanSelectVoice", null).SetValue(false);
								__instance.CanSelectAlignment = false;
								__instance.CanSelectRace = false;
								__instance.CanSelectPortrait = false;
								__instance.CanSelectGender = false;
								__instance.CanSelectName = false;
								__instance.CanSelectVoice = false;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Main.logger.Log(ex.ToString());
				}
			}
		}
	}
}


