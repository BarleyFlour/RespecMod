using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace RespecModBarley
{
	public class UnitInfo : ScriptableObject
    {
		public int OrgLvl
        {
			get;
			set;
        }
		public UnitEntityData Data
        {
			get;
			set;
		}
    }
	public class Main
	{
		public static bool NenioEtudeBool;
		public static BlueprintFeatureSelection featureSelection;
		public static UnityModManager.ModEntry ModEntry;
		public static UnitEntityView entityView;
		static bool Load(UnityModManager.ModEntry modEntry)
		{
			try
			{
				ModEntry = modEntry;
				logger = modEntry.Logger;
				var harmony = new Harmony(modEntry.Info.Id);
				harmony.PatchAll();
				modEntry.OnGUI = OnGUI;
				IsEnabled = ModEntry.Enabled;
			}
			catch (Exception e)
			{
				throw e;
			}
			return true;
		}
		public static UnitEntityData EntityUnit;
		public static int MythicXP;
		static bool FreeRespec = false;
		public static bool IsRespec = false;
		public static List<BlueprintFeature> featurestoadd = new List<BlueprintFeature> { };
		public static List<EntityPart> partstoadd = new List<EntityPart> { };
		public static List<UnitInfo> UnitMemory = new List<UnitInfo> { };
		public static UnitGroup unitgroupparty;
		public static string[] partslist = new String[] { "Kingmaker.UnitLogic.Parts.UnitPartPartyWeatherBuff", "Kingmaker.UnitLogic.Parts.UnitPartCompanion", "Kingmaker.UnitLogic.Parts.UnitPartNonStackBonuses", "Kingmaker.Corruption.UnitPartCorruption", "Kingmaker.UnitLogic.Parts.UnitPartWeariness", "Kingmaker.UnitLogic.Parts.UnitPartInteractions", "Kingmaker.UnitLogic.Parts.UnitPartVendor","Kingmaker.UnitLogic.Parts.UnitPartAbilityModifiers","Kingmaker.UnitLogic.Parts.UnitPartDamageGrace","Kingmaker.UnitLogic.Parts.UnitPartInspectedBuffs","Kingmaker.AreaLogic.SummonPool.SummonPool+PooledPart", "Kingmaker.UnitLogic.Parts.UnitPartHiddenFacts" };
		public static int PointsCount;
		public static bool OriginalStats;

		public static int GetUnitInfo(UnitEntityData unit)
        {
		   foreach(UnitInfo info in UnitMemory)
		   {
			    if(unit.CharacterName == info.Data.CharacterName)
				{
					///Main.logger.Log("MatchNameGetUnitInfo");
                    return info.OrgLvl;
				}
		   }
		   return 5;
        }
		public static int GetUnitInfoBP(BlueprintUnit unit)
		{
			foreach (UnitInfo info in UnitMemory)
			{
				if (unit.CharacterName == info.Data.CharacterName)
				{
					///Main.logger.Log("MatchNameGetUnitInfo");
					return info.OrgLvl;
				}
			}
			return 5;
		}
		public static void GetUnitForMemory(UnitEntityData data)
		{
			try
			{
					var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
					unitinfoinstance.Data = data;
					unitinfoinstance.OrgLvl = unitinfoinstance.Data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit;
					foreach (UnitInfo unitInfo in UnitMemory)
					{
						if (unitInfo.Data.CharacterName == data.CharacterName)
						{
							return;
						}
					}
					{
						UnitMemory.Add(unitinfoinstance);
						///Main.logger.Log(unitinfoinstance.Data.CharacterName.ToString() +" "+ unitinfoinstance.OrgLvl.ToString());
					}
			}
			catch (Exception e)
			{
				Main.logger.Log(e.ToString());
			}
		}
        public static void GetUnitsForMemory()
        {
			try
			{
				var entityPool = Game.Instance.State.Units;
				var entityPool2 = Game.Instance.Player.AllCharacters;
				foreach(UnitEntityData unit in entityPool)
                {
					if (unit.Blueprint.name.Contains("Companion") && unit.IsPet == false)
					{
						var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
						unitinfoinstance.Data = unit;
						unitinfoinstance.OrgLvl = unitinfoinstance.Data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit;
						foreach (UnitInfo unitInfo in UnitMemory)
						{
							if (unitInfo.Data.CharacterName == unit.CharacterName)
							{
								///Main.logger.Log("alreadyexist");
								return;
							}
						}
						{
							UnitMemory.Add(unitinfoinstance);
							///Main.logger.Log(unitinfoinstance.Data.CharacterName.ToString() + " " + unitinfoinstance.OrgLvl.ToString());
						}
					}
				}
				foreach (UnitEntityData unit in entityPool2)
				{
					if (unit.Blueprint.name.Contains("Companion"))
					{
						var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
						unitinfoinstance.Data = unit;
						unitinfoinstance.OrgLvl = unitinfoinstance.Data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit;
						foreach (UnitInfo unitInfo in UnitMemory)
						{
							if (unitInfo.Data.CharacterName == unit.CharacterName)
							{
								///Main.logger.Log("alreadyexist");
								return;
							}
						}
						{
							UnitMemory.Add(unitinfoinstance);
							Main.logger.Log(unitinfoinstance.Data.CharacterName.ToString() + " " + unitinfoinstance.OrgLvl.ToString());
						}
					}
				}
				/*foreach (UnitInfo a in UnitMemory)
				{
					Main.logger.Log(a.Data.CharacterName.ToString());
				}*/

			}
			catch (Exception e)
			{
				Main.logger.Log(e.ToString());
			}
		}
		private static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			Main.GetUnitsForMemory();
			if(IsEnabled == false){return;}
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			List<UnitEntityData> list = (from x in UIUtility.GetGroup(true, false)
										 where !x.IsInCombat && !x.Descriptor.State.IsFinallyDead
										 select x).ToList<UnitEntityData>();

			bool flag2 = list.Any((UnitEntityData x) => x.Descriptor.Progression.CharacterLevel == 0);
			if (flag2)
			{
				list = (from x in list
						where x.Descriptor.Progression.CharacterLevel == 0
						select x).ToList<UnitEntityData>();
				Main.selectedCharacter = 0;
			}
			else if (Main.selectedCharacter >= list.Count)
			{
				Main.selectedCharacter = 0;
			}
			int num = 0;
			UnitEntityData selected = null;
			foreach (UnitEntityData unitEntityData in list)
			{
				if(unitEntityData.IsMainCharacter == true)
                {
				///	unitgroupparty = unitEntityData.m_Group;
				}
				///if (!unitEntityData.IsPet && unitEntityData.IsPlayerFaction && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))
				if (!unitEntityData.IsPet && unitEntityData.IsPlayerFaction && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))
				{
					if (GUILayout.Toggle(Main.selectedCharacter == num, " " + unitEntityData.CharacterName, new GUILayoutOption[]
					{
								GUILayout.ExpandWidth(false)
					}))
					{
						Main.selectedCharacter = num;
						selected = unitEntityData;
					}
					num++;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			int i = Main.tabId;
			if (i == 0)
			{
				/*GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("Stats: ", new GUILayoutOption[]
				{
							GUILayout.ExpandWidth(false)
				});
				for (int j = 0; j < Main.extraPointLabels.Length; j++)
				{
					bool flag3 = Main.extraPoints == (Main.ExtraPointsType)j;
					bool flag4 = GUILayout.Toggle(flag3, Main.extraPointLabels[j], new GUILayoutOption[]
					{
								GUILayout.ExpandWidth(false)
					});
					if (flag4 != flag3 && flag4)
					{
						Main.extraPoints = (Main.ExtraPointsType)j;
					}
				}
				GUILayout.EndHorizontal();*/
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				OriginalStats = GUILayout.Toggle(OriginalStats, "Original Stats", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			float value = PointsCount;
			value = Math.Max(0, Math.Min(50, value));
			float abilityscoreslider = (float)Math.Round(GUILayout.HorizontalSlider(value, 0f, 50f, GUILayout.Width(100)));
			string stringstuff = GUILayout.TextField(abilityscoreslider.ToString(),GUILayout.ExpandWidth(false));
			int pointsstring = Int32.Parse(stringstuff);
			value = pointsstring;
			PointsCount = (int)value;
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());

			///Main.logger.Log(abilityscorepoints.ToString());
			int[] initStatsByUnit = Main.GetInitStatsByUnit(selected);
			int num2 = 0;
				foreach (StatType stat in StatTypeHelper.Attributes)
				{
					GUILayout.Label(string.Format("  {0} {1}", LocalizedTexts.Instance.Stats.GetText(stat), initStatsByUnit[num2++]), new GUILayoutOption[]
					{
								GUILayout.ExpandWidth(false)
					});
				}
			/*if (Main.extraPoints != Main.ExtraPointsType.Original)
			{
				GUILayout.Label("  Extra points " + ((Main.extraPoints == Main.ExtraPointsType.P25) ? "+25" : "Original"), new GUILayoutOption[]
				{
								GUILayout.ExpandWidth(false)
				});
			}*/
			GUILayout.Label(("  Extra points +" + (PointsCount.ToString())), new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (selected.Descriptor.Progression.CharacterLevel != 0 && GUILayout.Button(string.Format("Submit ({0}g)", Main.respecCost), UnityModManager.UI.button, new GUILayoutOption[]
			{
							GUILayout.ExpandWidth(false)
			}))
			{
				bool flag5 = false;
				if (!selected.IsCustomCompanion())
				{
					if (Game.Instance.Player.SpendMoney(Main.respecCost))
					{
						flag5 = true;
						modEntry.Logger.Log(string.Format("Money changed -{0}", Main.respecCost));
					}
					else
					{
						modEntry.Logger.Log(string.Format("Not enough money {0}", Main.respecCost));
					}
				}
				else if (Game.Instance.Player.Money >= Main.respecCost)
				{
					flag5 = true;
				}
				else
				{
					modEntry.Logger.Log(string.Format("Not enough money {0}", Main.respecCost));
				}
				if (flag5)
				{
					try
					{
						///Main.featurestoadd.Clear();
						Main.PreRespec(selected);
					}
					catch (Exception ex)
					{
						modEntry.Logger.Error(ex.ToString());
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			FreeRespec = GUILayout.Toggle(FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			if (FreeRespec == true)
			{
				respecCost = 0L;
			}
			if (FreeRespec == false)
			{
				respecCost = 1000L;
			}
		}
		/*internal sealed class PrivateImplementationDetails
		{
			internal static uint ComputeStringHash(string s)
			{
				uint num = new uint();
				if (s != null)
				{
					num = 0x811c9dc5;
					for (int i = 0; i < s.Length; i++)
					{
						num = (s[i] ^ num) * 0x1000193;
					}
				}
				return num;
			}
		}*/
		public static int[] GetInitStatsByUnit(UnitEntityData unit)
		{
			int[] numArray = new int[6] { 10, 10, 10, 10, 10, 10 };
			if (OriginalStats == true)
			{
				if (unit.IsStoryCompanion())
                {
					numArray = new int[6]
                    {
					 unit.OriginalBlueprint.Strength,
					 unit.OriginalBlueprint.Dexterity,
					 unit.OriginalBlueprint.Constitution,
					 unit.OriginalBlueprint.Intelligence,
					 unit.OriginalBlueprint.Wisdom,
					 unit.OriginalBlueprint.Charisma
                    };
				}
			}
			return numArray;
		}
		public static void PreRespec(UnitEntityData entityData)
		{
			if (IsEnabled == false) { return; }
			IsRespec = true;
			EntityUnit = entityData;
			entityView = entityData.View;
			///var NenioEtude = ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf");
			try
			{
				if (entityData.CharacterName.Contains("Nenio") && !Game.Instance.Player.EtudesSystem.EtudeIsCompleted(ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf")))
				{
					Main.NenioEtudeBool = true;
					///var KitsuneHeritageSelect = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ec40cc350b18c8c47a59b782feb91d1f");
					var KitsuneHeritageClassic = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
					var KitsuneHeritageKeen = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
					var facts = new List<BlueprintUnitFact> {KitsuneHeritageClassic,KitsuneHeritageKeen};
					foreach (IHiddenUnitFacts i in entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
					{
						foreach (BlueprintUnitFact fact in facts)
						{
							i.Facts.Remove(fact);
						}
					}
					///foreach (IHiddenUnitFacts refer in entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
					{
						/// if(refer.Guid.Contains("cd6cd774fb7cc844b8417193ee3a5ebe") || refer.Guid.Contains("d6bc49651fbaa2944bba6e2e5a1720ff") || refer.Guid.Contains("KitsuneHeritageSelection"))
						{
							/*foreach (IHiddenUnitFacts i in entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
							{
								foreach (BlueprintUnitFact a in i.Facts)
								{
									if (a.AssetGuid.Contains("cd6cd774fb7cc844b8417193ee3a5ebe") || a.AssetGuid.Contains("d6bc49651fbaa2944bba6e2e5a1720ff") || a.AssetGuid.Contains("KitsuneHeritageSelection"))
									{
										i.Facts.Remove(a);
										Main.logger.Log(a.ToString()+" Bro");
									}
								}

								///entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts.Get<IHiddenUnitFacts>(0). .Remove(refer);
							}*/
							///entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts.Get<IHiddenUnitFacts>(0). .Remove(refer);

						}
					}
					///entityData.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts.re

				}
			}
			catch(Exception e)
            {
				Main.logger.Log(e.ToString());
            }
			///var NenioEtude = ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf");
		/*	if (!Game.Instance.Player.EtudesSystem.EtudeIsCompleted(NenioEtude))
			{
				Game.Instance.Player.EtudesSystem.MarkEtudeCompleted(ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf"));
				Game.Instance.Player.EtudesSystem.UpdateEtudes();
				NenioEtudeBool = true;
			}*/
			var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
			BlueprintUnit defaultPlayerCharacter = Game.Instance.BlueprintRoot.DefaultPlayerCharacter;
			UnitDescriptor descriptor = entityData.Descriptor;
			UnitProgressionData unitProgressionData = entityData.Progression;
			UnitEntityData unit = entityData;
			MythicXP = unitProgressionData.MythicExperience;
			LevelUpHelper.GetTotalIntelligenceSkillPoints(descriptor, 0);
			LevelUpHelper.GetTotalSkillPoints(descriptor, 0);
			Traverse.Create(descriptor.Progression).Field("m_Selections").GetValue<Dictionary<BlueprintFeatureSelection, FeatureSelectionData>>().Clear();
			entityData.PrepareRespec();
			entityData.Descriptor.Buffs.RawFacts.Clear();
			Traverse.Create(descriptor).Field("Stats").SetValue(new CharacterStats(descriptor));
			descriptor.Stats.HitPoints.BaseValue = defaultPlayerCharacter.MaxHP+1;
			descriptor.Stats.Speed.BaseValue = defaultPlayerCharacter.Speed.Value;
			descriptor.UpdateSizeModifiers();
			var BPBackgroundList = new List<BlueprintFeature>{ };
			foreach (EntityPart entityPart in unit.Parts.Parts)
			{
				if(partslist.Contains(entityPart.ToString()))
                {
				    partstoadd.Add(entityPart);
					///Main.logger.Log(entityPart.ToString());
				}
			}
			Traverse.Create(unit.Parts).Field("Parts").SetValue(partstoadd);
			if (unit.Parts.Get<UnitPartCompanion>().State == CompanionState.InParty)
            {
             unit.Ensure<UnitPartCompanion>().SetState(CompanionState.InParty);
			}
			foreach(Buff buff in unit.Buffs)
            {
				buff.Detach();
            }
			if (unit.IsStoryCompanion())
			{
				foreach (Feature blueprintf in unit.Descriptor.Progression.Features.Enumerable)
				{
					if (backgroundsarray.Contains(blueprintf.Blueprint))
					{
						///Main.logger.Log(blueprintf.ToString());
						Main.featurestoadd.Add(blueprintf.Blueprint);
					}
				}
			}
			/*var pets = new List<UnitEntityData> { };
			if (unit.Pets != null)
			{
				foreach(UnitEntityData petdata in unit.Pets)
                {
					pets.Add(petdata);
				}
			}
			foreach (UnitEntityData petdata in pets)
			{
				unit.Pets.Remove(petdata);
				petdata.RemoveMaster();
			}*/
			/*LevelUpState_ctor_Patch.hasfeat = false;
				var feature = ScriptableObject.CreateInstance<BlueprintComponent>();
				feature.name = Main.OriginalBPLvl.ToString();
				ExtensionMethods.AddComponent(unit.Blueprint, feature);*/
			///var asd = new List<UnitPartHiddenFacts> { };
			/*foreach(UnitPart facts in unit.Parts.Parts)
			{
				var iss = facts;
				if (iss.ToString() == "Kingmaker.UnitLogic.Parts.UnitPartHiddenFacts")
				{
					unit.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
					unit.Parts.Get<UnitPartHiddenFacts>().RemoveSelf();
					var asdainnit = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("fd188bb7bb0002e49863aec93bfb9d99");
					var Trav = Traverse.Create<ClassLevelLimit>().Method("ReplaceRace").SetValue(asdainnit);
				}
			}*/
			/*foreach(UnitPart unit1 in asd)
            {
				unit1.RemoveSelf();
            }*/
			///Traverse.Create<>().
			///unit.Parts.Get<UnitPartHiddenFacts>().ReplaceRace.
			///entityData.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			RespecClass.Respecialize(entityData);
			/*bool asda = true;
			try
			{
				if (unit.CharacterName.Contains("Nenio"))
				{
					var kitsuneFeatureSelect = new BlueprintFeatureSelection();
					var classickitsune = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cd6cd774fb7cc844b8417193ee3a5ebe");
					var keenkitsune = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d6bc49651fbaa2944bba6e2e5a1720ff");
					var kitsuneFeatures = new List<BlueprintFeature> { keenkitsune, classickitsune };
					var kitsuneFeaturesr = new List<BlueprintFeatureReference> { keenkitsune.ToReference<BlueprintFeatureReference>(), classickitsune.ToReference<BlueprintFeatureReference>() };
					/*if (asda == true)
					{
						kitsuneFeatureSelect.SetFeatures(kitsuneFeatures);
						asda = false;
					}*//*
					kitsuneFeatureSelect.Features.AddItem(classickitsune);
					kitsuneFeatureSelect.Features.AddItem(keenkitsune);
					kitsuneFeatureSelect.AllFeatures.AddItem(keenkitsune);
					kitsuneFeatureSelect.AllFeatures.AddItem(classickitsune);
					kitsuneFeatureSelect.m_AllFeatures.AddItem(classickitsune.ToReference<BlueprintFeatureReference>());
					kitsuneFeatureSelect.m_AllFeatures.AddItem(keenkitsune.ToReference<BlueprintFeatureReference>());
					kitsuneFeatureSelect.m_Features.AddItem(classickitsune.ToReference<BlueprintFeatureReference>());
					kitsuneFeatureSelect.m_Features.AddItem(keenkitsune.ToReference<BlueprintFeatureReference>());
					kitsuneFeatureSelect.m_Features = kitsuneFeaturesr.ToArray();
					kitsuneFeatureSelect.m_AllFeatures = kitsuneFeaturesr.ToArray();
					kitsuneFeatureSelect.Obligatory = true;
					kitsuneFeatureSelect.name = "bruv";
					Traverse.Create(kitsuneFeatureSelect).Field("Name").SetValue("Bro");
					kitsuneFeatureSelect.Group = FeatureGroup.Racial;
					kitsuneFeatureSelect.HideInCharacterSheetAndLevelUp = false;
					kitsuneFeatureSelect.HideInUI = false;
					kitsuneFeatureSelect.HideNotAvailibleInUI = false;
					featureSelection = kitsuneFeatureSelect;
					///__instance.AddSelection(null, unit.Blueprint.Race, kitsuneFeatureSelect, 0);
				}
			}
			catch(Exception e) { Main.logger.Log(e.ToString()); }*/
			/*unit.Progression.AdvanceMythicExperience(MythicXP);
			if (unit.Progression.MythicExperience != MythicXP)
			{
				unit.Progression.GainMythicExperience(1);
			}*/
			///unitProgressionData.GainMythicExperience(MythicXP);
			///logger.Log(MythicXP.ToString());
			/*foreach (BlueprintFeature featuretoadd in BPBackgroundList)
            {
					///Main.logger.Log(featuretoadd.ToString());
				unit.Descriptor.AddFact(featuretoadd);

			}*/
		}
		public static UnityModManager.ModEntry.ModLogger logger;
		public static bool IsEnabled;
		public static int tabId = 0;
		public static long respecCost = 1000L;
		/*private static readonly string[] extraPointLabels = new string[]
		{
			" Original",
			" +25"
		};*/
		private static int selectedCharacter = 0;
		public enum ExtraPointsType
		{
			Original,
			P25
		}

	}
}
