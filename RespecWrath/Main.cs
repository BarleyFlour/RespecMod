using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace RespecModBarley
{
	public class Main
	{
		public static UnityModManager.ModEntry ModEntry;
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
		public static int MythicXP;
		static bool FreeRespec = false;
		public static List<BlueprintFeature> featurestoadd = new List<BlueprintFeature> { };
		private static void OnGUI(UnityModManager.ModEntry modEntry)
		{
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
			int i = Main.tabId;
			if (i == 0)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
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
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			int[] initStatsByUnit = Main.GetInitStatsByUnit(selected);
			int num2 = 0;
				foreach (StatType stat in StatTypeHelper.Attributes)
				{
					GUILayout.Label(string.Format("  {0} {1}", LocalizedTexts.Instance.Stats.GetText(stat), initStatsByUnit[num2++]), new GUILayoutOption[]
					{
								GUILayout.ExpandWidth(false)
					});
				}
			if (Main.extraPoints != Main.ExtraPointsType.Original)
			{
				GUILayout.Label("  Extra points " + ((Main.extraPoints == Main.ExtraPointsType.P25) ? "+25" : "Original"), new GUILayoutOption[]
				{
								GUILayout.ExpandWidth(false)
				});
			}
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
						Main.featurestoadd.Clear();
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
		internal sealed class PrivateImplementationDetails
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
		}
		public static int[] GetInitStatsByUnit(UnitEntityData unit)
		{
			int[] numArray = new int[6] { 10, 10, 10, 10, 10, 10 };
			if (Main.extraPoints == Main.ExtraPointsType.Original)
			{
				if (unit.Descriptor.IsMainCharacter)
				{
					foreach (BlueprintUnit selectablePlayerCharacter in Game.Instance.BlueprintRoot.SelectablePlayerCharacters)
					{
						if ((UnityEngine.Object)unit.Descriptor.Blueprint == (UnityEngine.Object)selectablePlayerCharacter)
							numArray = new int[6]
							{
							  selectablePlayerCharacter.Strength,
							  selectablePlayerCharacter.Dexterity,
							  selectablePlayerCharacter.Constitution,
							  selectablePlayerCharacter.Intelligence,
							  selectablePlayerCharacter.Wisdom,
							  selectablePlayerCharacter.Charisma
							};
					}
				}
				else
				{
					switch (unit.Descriptor.Blueprint.name)
					{
						case "Camelia_Companion":
							numArray = new int[6] { 11, 17, 14, 10, 16, 13 };
							break;
						case "Arueshalae_Companion":
							numArray = new int[6] { 12, 20, 16, 18, 14, 20 };
							break;
						case "Daeran_Companion":
							numArray = new int[6] { 7, 14, 13, 11, 14, 18 };
							break;
						case "Staunton_Companion":
							numArray = new int[6] { 15, 18, 10, 7, 10, 14 };
							break;
						case "Regill_Companion":
							numArray = new int[6] { 15, 17, 14, 10, 13, 12 };
							break;
						case "Nenio_Companion":
							numArray = new int[6] { 7, 17, 12, 18, 9, 13 };
							break;
						case "SosielVaenic_Companion":
							numArray = new int[6] { 16, 10, 14, 10, 16, 14 };
							break;
						case "Ember_Companion":
							numArray = new int[6] { 9, 14, 14, 10, 13, 17 };
							break;
						case "Seelah_Companion":
							numArray = new int[6] { 16, 13, 14, 10, 13, 15 };
							break;
						case "Delamere_Companion":
							numArray = new int[6] { 16, 15, 10, 9, 10, 16 };
							break;
						case "Woljif_Companion":
							numArray = new int[6] { 10, 18, 13, 16, 10, 13 };
							break;
						case "Wenduag_Companion":
							numArray = new int[6] { 14, 18, 14, 10, 12, 7 };
							break;
						case "Lann_Companion":
							numArray = new int[6] { 15, 15, 12, 11, 17, 11 };
							break;
						case "EvilArueshalae_Companion":
							numArray = new int[6] { 12, 20, 16, 18, 14, 20 };
							break;
						case "Greybor_Companion":
							numArray = new int[6] { 16, 16, 12, 13, 10, 12 };
							break;
						case "Trever_Companion":
							numArray = new int[6] { 16, 10, 14, 10, 16, 14 };
							break;


					}
				}
			}
			return numArray;
		}
		public static void PreRespec(UnitEntityData entityData)
		{
			if (IsEnabled == false) { return; }
			var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
			BlueprintUnit defaultPlayerCharacter = Game.Instance.BlueprintRoot.DefaultPlayerCharacter;
			UnitDescriptor descriptor = entityData.Descriptor;
			UnitProgressionData unitProgressionData = entityData.Progression;
			UnitEntityData unit = entityData;
			MythicXP = unitProgressionData.MythicLevel;
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
			List<BlueprintFeature> list = new List<BlueprintFeature> { };
			foreach (Feature blueprintf in unit.Descriptor.Progression.Features.Enumerable)
			{
				if (backgroundsarray.Contains(blueprintf.Blueprint))
				{
					///Main.logger.Log(blueprintf.ToString());
					Main.featurestoadd.Add(blueprintf.Blueprint);
				}
			}
			if (unit.Pets != null)
			{
				foreach(UnitEntityData petdata in unit.Pets)
                {
					unit.Pets.Remove(petdata);
					petdata.RemoveMaster();
				}
			}
			UnitHelper.Respec(entityData);
			unit.Progression.AdvanceMythicExperience(MythicXP);
			if (unit.Progression.MythicExperience != MythicXP)
			{
				unit.Progression.GainMythicExperience(1);
			}
			///unitProgressionData.GainMythicExperience(MythicXP);
			logger.Log(MythicXP.ToString());
			/*foreach (BlueprintFeature featuretoadd in BPBackgroundList)
            {
					///Main.logger.Log(featuretoadd.ToString());
				unit.Descriptor.AddFact(featuretoadd);

			}*/
		}
		public static Main.ExtraPointsType extraPoints;
		public static UnityModManager.ModEntry.ModLogger logger;
		public static bool IsEnabled;
		public static int tabId = 0;
		public static long respecCost = 1000L;
		private static readonly string[] extraPointLabels = new string[]
		{
			" Original",
			" +25"
		};
		private static int selectedCharacter = 0;
		public enum ExtraPointsType
		{
			Original,
			P25
		}

	}
}
