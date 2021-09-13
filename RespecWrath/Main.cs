using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.Metamagic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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

namespace RespecWrathFork
{
	/*public class UnitInfo : ScriptableObject
    {
		public int OrgLvl
        {
			get;
			set;
        }
		public BlueprintUnit Data
        {
			get;
			set;
		}
		public int orgMythLvl
        {
			get;
			set;
        }
    }*/
	public class Main
	{
		public static Settings settings;
		public static bool OldCost;
		public static bool haspatched = false;
		public static bool NenioEtudeBool = false;
		public static BlueprintFeatureSelection featureSelection;
		public static UnityModManager.ModEntry ModEntry;
		
		//public static UnitEntityView entityView;
		static bool Load(UnityModManager.ModEntry modEntry)
		{
			try
			{
				ModEntry = modEntry;
				logger = modEntry.Logger;
				settings = Settings.Load(modEntry);
				var harmony = new Harmony(modEntry.Info.Id);
				harmony.PatchAll();
				modEntry.OnGUI = OnGUI;
				modEntry.OnSaveGUI = OnSaveGUI;
				IsEnabled = ModEntry.Enabled;
				if (!Main.haspatched)
				{
					Main.PatchLibrary();
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			return true;
		}
		static void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			settings.Save(modEntry);
		}

        public static bool isrecruit = false;
		//public static SimpleBlueprint[] blueprints;
		public static UnitEntityData EntityUnit;
		public static int MythicXP;
		public static bool IsRespec = false;
		public static List<BlueprintFeature> featurestoadd = new List<BlueprintFeature> { };
		public static List<EntityPart> partstoadd = new List<EntityPart> { };
		//public static List<UnitInfo> UnitMemory = new List<UnitInfo> { };
		//public static UnitGroup unitgroupparty;
		public static string[] partslist = new String[] { "Kingmaker.UnitLogic.Parts.UnitPartPartyWeatherBuff", "Kingmaker.UnitLogic.Parts.UnitPartWeariness", "Kingmaker.UnitLogic.Parts.UnitPartInteractions", "Kingmaker.UnitLogic.Parts.UnitPartVendor","Kingmaker.UnitLogic.Parts.UnitPartAbilityModifiers","Kingmaker.UnitLogic.Parts.UnitPartDamageGrace","Kingmaker.UnitLogic.Parts.UnitPartInspectedBuffs","Kingmaker.AreaLogic.SummonPool.SummonPool+PooledPart", "Kingmaker.UnitLogic.Parts.UnitPartHiddenFacts" };

		/*public static int[] GetUnitInfo(UnitEntityData unit)
        {
			var list = new int[] { unit.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit, unit.Blueprint.GetComponent<MythicLevelLimit>().LevelLimit };
			return list;
		}
		public static int[] GetUnitInfoBP(BlueprintUnit unit)
		{
			///var list = new int[] { unit.GetComponent<ClassLevelLimit>().LevelLimit, unit.GetComponent<MythicLevelLimit>().LevelLimit };
			var memdata = UnitMemory.First(a => a.Data.CharacterName == unit.CharacterName);
			var list = new int[] {memdata.OrgLvl,memdata.orgMythLvl };
			return list;
			/*foreach (UnitInfo info in UnitMemory)
			{
				if (unit.CharacterName == info.Data.CharacterName)
				{
					///Main.logger.Log("MatchNameGetUnitInfo");
					int info = new int[];
					return info.OrgLvl;
				}
			}
			return 5;*//*
		}*/
		/*public static void GetUnitForMemory(BlueprintUnit data)
		{
			try
			{
				if(data.CharacterName == null) return;
                if (!UnitMemory.Any(a => a.Data.CharacterName == data.CharacterName))
				{
					var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
					unitinfoinstance.Data = data;
					unitinfoinstance.OrgLvl = unitinfoinstance.Data.GetComponent<ClassLevelLimit>().LevelLimit;
					var myth = unitinfoinstance.Data.GetComponent<MythicLevelLimit>();
					if (myth != null)
					{
						unitinfoinstance.orgMythLvl = myth.LevelLimit;
					}
					UnitMemory.Add(unitinfoinstance);
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
				var entityPool = Game.Instance.State.Units.All.ToList();
				var entityPool2 = Game.Instance.Player.AllCharacters.ToList();
				var poolconc = entityPool.Concat(entityPool2).ToList();
                var poolconcc = poolconc.Concat(Game.Instance.State.Units).ToList();
				if (poolconcc.Count <= 0) return;
				foreach (UnitEntityData unit in poolconcc)
                {
					///Main.logger.Log(unit.Blueprint.CharacterName);
					if (unit.Blueprint.ToString().Contains("_Companion") && !unit.Blueprint.ToString().Contains("!")  && !UnitMemory.Any(a => a.Data.CharacterName == unit.Blueprint.CharacterName))
					{
						var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
						unitinfoinstance.Data = unit.Blueprint;
						unitinfoinstance.OrgLvl = unitinfoinstance.Data.GetComponent<ClassLevelLimit>().LevelLimit;
						var myth = unitinfoinstance.Data.GetComponent<MythicLevelLimit>();
						if (myth != null)
						{
							unitinfoinstance.orgMythLvl = myth.LevelLimit;
					    }
						/*if (UnitMemory.Any(a => a.Data.CharacterName == unit.CharacterName)) return;
						{*/
					//UnitMemory.Add(unitinfoinstance);
							///Main.logger.Log(unitinfoinstance.Data.CharacterName.ToString() + " " + unitinfoinstance.OrgLvl.ToString());
						///}
					//}
				//}
				/*foreach (UnitEntityData unit in entityPool2)
				{
					if (unit.Blueprint.name.Contains("Companion"))
					{
						var unitinfoinstance = ScriptableObject.CreateInstance<UnitInfo>();
						unitinfoinstance.Data = unit.Blueprint;
						unitinfoinstance.OrgLvl = unitinfoinstance.Data.GetComponent<ClassLevelLimit>().LevelLimit;
						var myth = unitinfoinstance.Data.GetComponent<MythicLevelLimit>();
						if (myth != null)
						{
						unitinfoinstance.orgMythLvl = myth.LevelLimit;
						}
						if (UnitMemory.Any(a => a.Data.CharacterName == unit.CharacterName)) return;
						{
							UnitMemory.Add(unitinfoinstance);
							///Main.logger.Log(unitinfoinstance.Data.CharacterName.ToString() + " " + unitinfoinstance.OrgLvl.ToString());
						}
					}
				}*/
				/*foreach (UnitInfo a in UnitMemory)
				{
					Main.logger.Log(a.Data.CharacterName.ToString());
				}*//*

			}
			catch (Exception e)
			{
				Main.logger.Log(e.ToString());
			}
		}*/
		private static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			try
			{
               // Main.GetUnitsForMemory();
				if (IsEnabled == false) { return; }
				GUILayout.Space(5f);
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				List<UnitEntityData> list = (from x in UIUtility.GetGroup(true, false)
											 where !x.IsInCombat && !x.Descriptor.State.IsFinallyDead
											 select x).ToList<UnitEntityData>();

				bool flag2 = list.Any((UnitEntityData x) => x.Descriptor.Progression.CharacterLevel == 0 && !x.IsPet);
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
					///Main.logger.Log(unitEntityData.CharacterName);
					/*if (unitEntityData.IsMainCharacter == true)
					{
						///	unitgroupparty = unitEntityData.m_Group;
					}*/
					///if (!unitEntityData.IsPet && unitEntityData.IsPlayerFaction && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))
					if (unitEntityData.IsPlayerFaction && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))
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
					if (selected.IsStoryCompanion() && !selected.IsMainCharacter)
					{
						settings.OriginalStats = GUILayout.Toggle(settings.OriginalStats, "Original Stats", GUILayout.ExpandWidth(false));
					}
					else
				    {
						settings.OriginalStats = false;
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				float value = settings.PointsCount;
				value = Math.Max(0, Math.Min(102, value));
				float abilityscoreslider = (float)Math.Round(GUILayout.HorizontalSlider(value, 0f, 102f, GUILayout.Width(120)));
				string stringstuff = GUILayout.TextField(abilityscoreslider.ToString(), GUILayout.ExpandWidth(false));
				int pointsstring = Int32.Parse(stringstuff);
				value = pointsstring;
				settings.PointsCount = (int)value;
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
				GUILayout.Label(("  Extra points +" + (settings.PointsCount.ToString())), new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				if (selected.Descriptor.Progression.CharacterLevel != 0 && GUILayout.Button(string.Format("Submit ({0}g)", Main.respecCost), UnityModManager.UI.button, new GUILayoutOption[]
				{
							GUILayout.ExpandWidth(false)
				}))
				{
					string nameandtype = "";
					if (selected.IsPet)
                    {
						nameandtype = $"pet {selected.CharacterName}";
                    }
					else if (selected.IsMainCharacter)
                    {
						nameandtype = $"main character {selected.CharacterName}";
                    }
					else if (selected.IsStoryCompanion())
                    {
						nameandtype = $"story character {selected.CharacterName}";
					}
					else
                    {
						nameandtype = $"merc {selected.CharacterName}";
					}
					modEntry.Logger.Log($"Initiating respec of {nameandtype}");
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
                            Main.IsRespec = true;
							Main.PreRespec(selected);
						}
						catch (Exception ex)
						{
							modEntry.Logger.Error(ex.ToString());
						}
					}
				}
				//GUILayout.EndHorizontal();

                GUILayout.Space(5f);
                //GUILayout.BeginHorizontal();
                if (selected.Descriptor.Progression.CharacterLevel != 0 && GUILayout.Button(string.Format("Mythic Only ({0}g)", Main.respecCost * 0.25), UnityModManager.UI.button, new GUILayoutOption[]
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
                            MythicRespec.MyhticRespec(selected);
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
				settings.FreeRespec = GUILayout.Toggle(settings.FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
				if(selected.IsStoryCompanion() && !selected.IsMC() )
				{
					settings.FullRespecStoryCompanion = GUILayout.Toggle(settings.FullRespecStoryCompanion, "Full Story Companion Respec", GUILayout.ExpandWidth(false));
					if(settings.FullRespecStoryCompanion)
                    {
						settings.BackgroundDeity = true;
                    }
				}
				else
                {
					settings.FullRespecStoryCompanion = false;
                }
				if (selected.IsMC())
                {
					settings.PreserveMCBiographicalInformation = GUILayout.Toggle(settings.PreserveMCBiographicalInformation, "Retain Name/Alignment/Birthday/Portrait", GUILayout.ExpandWidth(false));
				}
				else
                {
					settings.PreserveMCBiographicalInformation = false;
				}


				if(selected.IsStoryCompanion() && !settings.FullRespecStoryCompanion)
                {
					settings.BackgroundDeity = GUILayout.Toggle(settings.BackgroundDeity, "Choose Background/Deity", GUILayout.ExpandWidth(false));
				}
				else if (( selected.IsStoryCompanion()) && settings.FullRespecStoryCompanion)
                {
					settings.BackgroundDeity = true;
				}
                else if (!settings.FullRespecStoryCompanion)
                {
					settings.BackgroundDeity = false;
                }
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				OldCost = GUILayout.Toggle(OldCost, "Fixed/Scaling Cost", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				if (settings.FreeRespec == true)
				{
					respecCost = 0L;
				}
				if (settings.FreeRespec == false)
				{
					if (Main.OldCost)
					{
						respecCost = 1000L;
					}
					else
					{
						var lvl = selected.Progression.CharacterLevel;
						float cost = lvl * lvl / 4 * 1000;
						respecCost = (long)Math.Max(250, Math.Round(cost));
						///Main.logger.Log(respecCost.ToString());
						///respecCost = 1000L;
					}
				}
			}
			catch(Exception e) { Main.logger.Log(e.Message + "   " + e.StackTrace); }
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
			if (settings.OriginalStats == true)
			{
				if (unit.IsStoryCompanion() && settings.OriginalStats || unit.Blueprint.ToString().Contains("_Companion") && settings.OriginalStats)
                {
					numArray = new int[6]
                    {
					 unit.Blueprint.Strength,
					 unit.Blueprint.Dexterity,
					 unit.Blueprint.Constitution,
					 unit.Blueprint.Intelligence,
					 unit.Blueprint.Wisdom,
					 unit.Blueprint.Charisma
                    };
				}
			}
			return numArray;
		}
		public static void PatchLibrary()
        {
			if (Main.IsEnabled == false) { return; }
			if(Game.HasInstance)
			{
				try
				{
					Main.logger.Log("Library patching initiated");
					var tempbp = ExtensionMethods.GetBlueprints();
					var unitbp = tempbp.OfType<BlueprintUnit>();
					var abilitybps = tempbp.OfType<BlueprintFeature>().ToList().FindAll(list => list.name.Contains("_FeatureList"));
					var unitbps = tempbp.OfType<BlueprintUnit>().ToList().FindAll(BPUnits => BPUnits.ToString().Contains("_Companion") && BPUnits.LocalizedName != null && BPUnits.GetComponent<ClassLevelLimit>());
					///var noSelectionIfAlreadyHasFeatureBackgroundSelect = new NoSelectionIfAlreadyHasFeature();
					///noSelectionIfAlreadyHasFeatureBackgroundSelect.AnyFeatureFromSelection = false;
					///var FeatureListsT = abilitybps.ToArray().Select(lis => lis.ToReference<BlueprintFeatureReference>()).ToArray();
					var religionsbp = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").Items.Select(a => a.Feature).ToArray();
					/*foreach(var asd in religionsbp)
                    {
						Main.logger.Log(asd.ToString());
                    }*/
					/*
					foreach (var asd in religionsbp)
                    {
						Main.logger.Log(asd.ToString());
                    }
					var DeityNoSelection = new NoSelectionIfAlreadyHasFeature();
					DeityNoSelection.AnyFeatureFromSelection = false;
					DeityNoSelection.m_Features = FeatureListsT;
					Main.blueprints = abilitybps.Concat<SimpleBlueprint>(unitbps).Concat(religionsbp).ToArray();*/
					Stuf.deityfeatures = religionsbp.ToArray();
					/*foreach (BlueprintFeatureReference f in FeatureListsT)
					{
						Main.logger.Log(f.ToString());
					}*/
					/*foreach(BlueprintUnit unit in unitbps)
                    {
						Main.logger.Log(unit.NameForAcronym);
                    }*//*.
					var asdus = new NoSelectionIfAlreadyHasFeature
					{
						m_Features = FeatureListsT, AnyFeatureFromSelection = false,
						name = "noselecc"
					};*/

					///noSelectionIfAlreadyHasFeatureBackgroundSelect.m_Features = FeatureListsT;
					///ExtensionMethods.AddComponent(Stuf.BackgroundSelect, noSelectionIfAlreadyHasFeatureBackgroundSelect);
					///var UnitBPs = Utilities.GetScriptableObjects<BlueprintScriptableObject>().OfType<BlueprintUnit>().ToList();
					/*foreach (BlueprintUnit data in unitbps.Where(a => a.CharacterName != null && a.ToString().Contains("_Companion")))
					{
						Main.GetUnitForMemory(data);
					}*/
					Main.haspatched = true;
				}
				catch (Exception ex)
				{
					Main.logger.Log("Error while patching library");
					Main.logger.Log(ex.ToString());
				}
			}
		}
		public static void PreRespec(UnitEntityData entityData)
		{
            Main.IsRespec = true;
			try
			{
                Main.IsRespec = true;
				var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
				backgroundsarray = backgroundsarray.Concat<BlueprintFeature>(Stuf.deityfeatures).ToArray();
				Main.IsRespec = true;
				
				/*if (entityData.IsStoryCompanion())
				{
					Main.GetUnitForMemory(entityData.Blueprint);
				}*/
				Main.EntityUnit = entityData;
				foreach (EntityPart entityPart in entityData.Parts.m_Parts)
				{
					if (Main.partslist.Contains(entityPart.ToString()))
					{
						

						Main.partstoadd.Add(entityPart);
						///Main.logger.Log(entityPart.ToString());
					}
				}
				/*if (entityData.Descriptor.Alignment.Undetectable.Value == true && Main.FullRespecStoryCompanion && entityData.Blueprint.NameForAcronym == "Camelia_Companion")
				{
					var AmuletFact = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("14e09d21335cd81478252653ba1a8464"); ;
					entityData.GetFact(AmuletFact).OnDeactivate();
					entityData.GetFact(AmuletFact).Dispose();
					entityData.GetFact(AmuletFact).Detach();
					Main.logger.Log("asd");
				}*/
				if (entityData.IsStoryCompanion() && !Main.settings.FullRespecStoryCompanion || entityData.Blueprint.ToString().Contains("_Companion"))
				{
					foreach (Feature blueprintf in entityData.Descriptor.Progression.Features.Enumerable)
					{
						var nosource = blueprintf.SourceClass == null && blueprintf.SourceProgression == null && blueprintf.SourceRace == null;
                        if (backgroundsarray.Contains(blueprintf.Blueprint) && !Main.settings.BackgroundDeity || blueprintf.Hidden && nosource && !blueprintf.NameForAcronym.Contains("Cantrip") || entityData.Progression.Race.m_Features.Any(A => A.Cached == blueprintf.Blueprint))
						{
							Main.logger.Log(blueprintf.ToString());
							Main.featurestoadd.Add(blueprintf.Blueprint);
						}
						/*else if(backgroundsarray.Contains(blueprintf.Blueprint) && !Main.settings.BackgroundDeity || blueprintf.Hidden && blueprintf.m_Source == null)
                        {
							Main.logger.Log("== null " + blueprintf.ToString());
						}*/

					}
				}
				//if (entityData.Blueprint.GetComponent<ClassLevelLimit>())
				{
					//testy
					//entityData.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
				}
				//if (entityData.Blueprint.GetComponent<MythicLevelLimit>())
				{
					//testy
					//entityData.Blueprint.GetComponent<MythicLevelLimit>().LevelLimit = 0;
				}
				
				RespecClass.Respecialize(entityData);
			}

			catch(Exception e) { Main.logger.Log(e.ToString());
                Main.IsRespec = false;
            }
			/*if (IsEnabled == false) { return; }
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
				}
			}
			catch(Exception e)
            {
				Main.logger.Log(e.ToString());
            }
			///var NenioEtude = ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf");
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
