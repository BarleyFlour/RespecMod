using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RespecModBarley
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(LevelUpState.CharBuildMode) })]
	internal static class LevelUpState_ctor_Patch
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000041B4 File Offset: 0x000023B4
		private static void Postfix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
		{
			var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
			try
			{
				int[] initStatsByUnit = Main.GetInitStatsByUnit(unit);
				unit.Descriptor.Stats.Strength.BaseValue = initStatsByUnit[0];
				unit.Descriptor.Stats.Dexterity.BaseValue = initStatsByUnit[1];
				unit.Descriptor.Stats.Constitution.BaseValue = initStatsByUnit[2];
				unit.Descriptor.Stats.Intelligence.BaseValue = initStatsByUnit[3];
				unit.Descriptor.Stats.Wisdom.BaseValue = initStatsByUnit[4];
				unit.Descriptor.Stats.Charisma.BaseValue = initStatsByUnit[5];
				if (__instance.NextCharacterLevel == 1 && unit.Progression.Experience > 0)
				{
					foreach (BlueprintFeature blueprintFeature in unit.Descriptor.OriginalBlueprint.Race.m_Features)
					{
						unit.Descriptor.AddFact(blueprintFeature);
					}
					foreach (BlueprintFeature featuretoadd in Main.featurestoadd)
					{
							///Main.logger.Log(featuretoadd.ToString());
							unit.Descriptor.AddFact(featuretoadd);
					}
					if (unit.Progression.Race == Stuf.HumanRace || unit.Progression.Race == Stuf.HalfElfRace || unit.Progression.Race == Stuf.HalfOrcRace)
					{
						__instance.CanSelectRaceStat = true;
					}
					var blueprintUnit = Game.Instance.BlueprintRoot.SelectablePlayerCharacters.Where(u => u == unit.Blueprint).FirstOrDefault();
					bool flag = unit.Blueprint == Game.Instance.BlueprintRoot.DefaultPlayerCharacter;
					bool flag2 = flag;
					if (flag || Main.extraPoints == Main.ExtraPointsType.P25)
					{
						__instance.StatsDistribution.Start(25);
					}
					else
					{
						__instance.AttributePoints = 0;
						Traverse.Create(__instance.StatsDistribution).Property("Available", null).SetValue(false);
						Traverse.Create(__instance.StatsDistribution).Property("Points", null).SetValue(0);
						Traverse.Create(__instance.StatsDistribution).Property("TotalPoints", null).SetValue(0);
					}
					if (!unit.IsCustomCompanion())
					{
						Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.LevelUp);
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
						__instance.CanSelectAlignment = false;
						__instance.CanSelectRace = false;
						__instance.CanSelectPortrait = false;
						__instance.CanSelectGender = false;
						__instance.CanSelectName = false;
						__instance.CanSelectVoice = false;
						return;
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


