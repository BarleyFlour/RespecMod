using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Modding;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.UI.MVVM._VM.ChangeVisual;
using UniRx;
using UnityEngine;
#if UMM
using Kingmaker.UI.MVVM._PCView.ChangeVisual;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
#endif

namespace RespecWrath
{
    
    
    
#if UMM && DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        public static Dictionary<string, string> statbooks
        {
            get
            {
                if (m_statbooks == null)
                {
                    m_statbooks = new Dictionary<string, string>();
                    m_statbooks["TomeOfClearThoughtPlus2_Feature"] = "3584c2a2f8b5b1b43ae11128f0ff1583";
                    m_statbooks["TomeOfLeadershipAndInfluencePlus2_Feature"] = "37e2f09923a96234ca486bc9db0b6ad6";
                    m_statbooks["TomeOfUnderstandingPlus2_Feature"] = "419a486154514594c99193da785d4302";
                    m_statbooks["ManualOfBodilyHealthPlus2_Feature"] = "2d4d510a69da09c48893f945ec197210";
                    m_statbooks["ManualOfGainfulExercisePlus2_Feature"] = "7a7b1dfa67aa4544aa468d0558b1f667";
                    m_statbooks["ManualOfQuicknessOfActionPlus2_Feature"] = "b8ea6d2f787c7004c9cab9f519f687f8";
                    m_statbooks["ElixirMasterpieceFeature"] = "5219d5846529ae949b88c87858c1bb9e";
                }
                return m_statbooks;
            }
        }
        
        public static Dictionary<string, string> m_statbooks;
        public static Settings settings;
        public static bool OldCost;
        public static bool forcecost = false;
        public static bool haspatched = false;
        public static bool NenioEtudeBool = false;
        public static BlueprintFeatureSelection featureSelection;
#if WrathMod
        public static OwlcatModification modEntry;
        [OwlcatModificationEnterPoint]
        public static void EnterPoint(OwlcatModification ModEntry)
        {
            try
            {
                modEntry = ModEntry;
                var harmony = new Harmony(modEntry.Manifest.UniqueName);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                settings = ModEntry.LoadData<Settings>();

                ///ModEntry.OnGUI += OnGUI;
                //modEntry.OnDrawGUI += OnGUI;
                modEntry.OnGUI += OnGUI;
                IsEnabled = true;
               // if (!Main.haspatched)
                {
                 //   Main.PatchLibrary();
                }
            }
            catch (Exception e)
            {
                modEntry.Logger.Log(e.ToString()); ;
                throw e;
            }
        }
        private static void OnGUI()
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
                    if (unitEntityData.IsPlayerFaction)/* && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))*/
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
                    if (selected.IsStoryCompanionLocal() && !selected.IsMainCharacter)
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
                if (/*selected.Descriptor.Progression.CharacterLevel != 0 && */GUILayout.Button(string.Format("Submit ({0}g)", Main.respecCost), new GUILayoutOption[]
                {
                            GUILayout.Width(250f)
                }))
                {
                    /*string nameandtype = "";
					if (selected.IsPet)
                    {
						nameandtype = $"pet {selected.CharacterName}";
                    }
					else if (selected.IsMainCharacter)
                    {
						nameandtype = $"main character {selected.CharacterName}";
                    }
					else if (selected.IsStoryCompanionLocal())
                    {
						nameandtype = $"story character {selected.CharacterName}";
					}
					else
                    {
						nameandtype = $"merc {selected.CharacterName}";
					}
					modEntry.Logger.Log($"Initiating respec of {nameandtype}");*/
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
                if (selected.Descriptor.Progression.CharacterLevel != 0 && GUILayout.Button(string.Format("Mythic Only ({0}g)", Main.respecCost * 0.25), new GUILayoutOption[]
                {
                    GUILayout.Width(250f)
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
                if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(string.Format("-1 Level"), new GUILayoutOption[]
                {
                    GUILayout.Width(250f)
                }))
                {
                    ArbitraryLevelRemoval.RemoveMythicLevel(selected, selected.Progression);
                }
                GUILayout.Space(5f);
                if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(string.Format("-1 Mythic Level"), new GUILayoutOption[]
                {
                    GUILayout.Width(250f)
                }))
                {
                    Main.MythicXP = selected.Progression.MythicExperience;
                    selected.Progression.RemoveMythicLevel();
                    selected.Progression.AdvanceMythicExperience(Main.MythicXP);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                settings.FreeRespec = GUILayout.Toggle(settings.FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
                settings.KeepSkillPoints = GUILayout.Toggle(settings.KeepSkillPoints, "Retain Skillpoint Distribution when leveling down", GUILayout.ExpandWidth(false));
                if (selected.IsStoryCompanionLocal() && !selected.IsMC())
                {
                    {
                        //GUILayout.BeginVertical();
                        settings.FullRespecStoryCompanion = GUILayout.Toggle(settings.FullRespecStoryCompanion,
                            "Respec as Mercenary", GUILayout.ExpandWidth(false));
                        if (settings.FullRespecStoryCompanion)
                        {
                            settings.BackgroundDeity = true;
                        }
                        //settings.PreserveVoice = false;
                        settings.PreserveMCAlignment = false;
                        settings.PreserveMCBirthday = false;
                        settings.PreserveMCName = false;
                        //settings.PreserveMCRace = false;
                        // settings.PreservePortrait = false;
                        //GUILayout.EndVertical();
                    }
                }
                else
                {
                    settings.FullRespecStoryCompanion = false;
                }
                if (selected.IsMC())
                {
                    //GUILayout.BeginVertical();
                    settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreserveMCAlignment = GUILayout.Toggle(settings.PreserveMCAlignment, "Retain Alignment", GUILayout.ExpandWidth(false));
                    settings.PreserveMCBirthday = GUILayout.Toggle(settings.PreserveMCBirthday, "Retain Birthday", GUILayout.ExpandWidth(false));
                    settings.PreserveMCName = GUILayout.Toggle(settings.PreserveMCName, "Retain Name", GUILayout.ExpandWidth(false));
                    //settings.PreserveMCRace = settings.PreserveMCAlignment;
                    settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
                    //GUILayout.EndVertical();
                }
                if (selected.IsStoryCompanionLocal() && !settings.FullRespecStoryCompanion)
                {
                    //GUILayout.BeginVertical();
                    {
                        settings.BackgroundDeity = GUILayout.Toggle(settings.BackgroundDeity, "Choose Background/Deity", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                    //GUILayout.BeginVertical();
                    {
                        settings.OriginalLevel = GUILayout.Toggle(settings.OriginalLevel, "Respec From Recruit Level", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                }
                else if ((selected.IsStoryCompanionLocal()) && settings.FullRespecStoryCompanion)
                {
                    settings.BackgroundDeity = true;
                    settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
                }
                else if (!settings.FullRespecStoryCompanion)
                {
                    settings.BackgroundDeity = false;
                }
                if (selected.IsCustomCompanion())
                {
                    settings.PreserveVoice = false;
                    settings.PreservePortrait = false;
                }
                /*	foreach (var classbp in selected?.Progression.Classes.Select(a => a.CharacterClass))
					{
						if (GUILayout.Button(classbp.name))
						{
							selectedclass = classbp;
						}
					}*/
                /*if(GUILayout.Button("-1 level in"+ selected.Progression.m_ClassesOrder.Last(b => !b.IsMythic)?.name))
                {
					ArbitraryLevelRemoval.RemoveMythicLevel(selected,selected.Progression);
                }*/
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                OldCost = GUILayout.Toggle(OldCost, "Fixed/Scaling Cost", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                if (settings.FreeRespec == true && !forcecost)
                {
                    respecCost = 0L;
                }
                if (settings.FreeRespec == false || forcecost)
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
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.Message + "   " + e.StackTrace); }
        }

        //public static BlueprintCharacterClass selectedclass;
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
#endif
#if UMM
        public static UnityModManager.ModEntry ModEntry;

        //public static UnitEntityView entityView;
        private static bool Load(UnityModManager.ModEntry modEntry)
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
#if DEBUG
                modEntry.OnUnload = Unload;
#endif
                modEntry.OnToggle = OnToggle;
                IsEnabled = ModEntry.Enabled;
                if (!Main.haspatched)
                {
                    Main.PatchLibrary();
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
                throw e;
            }
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            IsEnabled = value;
            return true; // Permit or not.
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static bool Unload(UnityModManager.ModEntry modEntry)
        {
            new Harmony(modEntry.Info.Id).UnpatchAll(modEntry.Info.Id);
            return true;
        }
#endif
        public static bool isrecruit = false;

        //public static SimpleBlueprint[] blueprints;
        //public static UnitEntityData EntityUnit;
        public static int MythicXP;

        public static bool IsRespec = false;
        public static bool IsHilorRespec = false;
        public static List<BlueprintFeature> featurestoadd = new List<BlueprintFeature> { };
        public static List<EntityPart> partstoadd = new List<EntityPart> { };

        //public static List<UnitInfo> UnitMemory = new List<UnitInfo> { };
        //public static UnitGroup unitgroupparty;
        public static string[] partslist = new String[] { "Kingmaker.UnitLogic.Parts.UnitPartPartyWeatherBuff", "Kingmaker.UnitLogic.Parts.UnitPartWeariness", "Kingmaker.UnitLogic.Parts.UnitPartInteractions", "Kingmaker.UnitLogic.Parts.UnitPartVendor", "Kingmaker.UnitLogic.Parts.UnitPartAbilityModifiers", "Kingmaker.UnitLogic.Parts.UnitPartDamageGrace", "Kingmaker.UnitLogic.Parts.UnitPartInspectedBuffs", "Kingmaker.AreaLogic.SummonPool.SummonPool+PooledPart", "Kingmaker.UnitLogic.Parts.UnitPartHiddenFacts", "Kingmaker.UnitLogic.Parts.UnitPartLocustSwarm", "VisualAdjustments.UnitPartVAFX", "VisualAdjustments.UnitPartVAEELs" };

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
#if UMM
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
                    if (unitEntityData.IsPlayerFaction)/* && (!flag2 || unitEntityData.Descriptor.Progression.CharacterLevel <= 0))*/
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
                    if (selected?.IsStoryCompanionLocal() == true && selected?.IsMainCharacter != true)
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
                if (/*selected.Descriptor.Progression.CharacterLevel != 0 && */GUILayout.Button(string.Format("Submit ({0}g)", Main.respecCost), UnityModManager.UI.button, new GUILayoutOption[]
                {
                            GUILayout.Width(250f)
                }))
                {
                    /*string nameandtype = "";
					if (selected.IsPet)
                    {
						nameandtype = $"pet {selected.CharacterName}";
                    }
					else if (selected.IsMainCharacter)
                    {
						nameandtype = $"main character {selected.CharacterName}";
                    }
					else if (selected.IsStoryCompanionLocal())
                    {
						nameandtype = $"story character {selected.CharacterName}";
					}
					else
                    {
						nameandtype = $"merc {selected.CharacterName}";
					}
					modEntry.Logger.Log($"Initiating respec of {nameandtype}");*/
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
                    GUILayout.Width(250f)
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
                if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(string.Format("-1 Level"), UnityModManager.UI.button, new GUILayoutOption[]
                {
                    GUILayout.Width(250f)
                }))
                {
                    ArbitraryLevelRemoval.RemoveMythicLevel(selected, selected.Progression);
                }
                GUILayout.Space(5f);
                if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(string.Format("-1 Mythic Level"), UnityModManager.UI.button, new GUILayoutOption[]
                {
                    GUILayout.Width(250f)
                }))
                {
                    Main.MythicXP = selected.Progression.MythicExperience;
                    selected.Progression.RemoveMythicLevel();
                    selected.Progression.AdvanceMythicExperience(Main.MythicXP);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                settings.FreeRespec = GUILayout.Toggle(settings.FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
                settings.KeepSkillPoints = GUILayout.Toggle(settings.KeepSkillPoints, "Retain Skillpoint Distribution when leveling down", GUILayout.ExpandWidth(false));
                if (selected.IsStoryCompanionLocal() && !selected.IsMC())
                {
                    {
                        //GUILayout.BeginVertical();
                        settings.FullRespecStoryCompanion = GUILayout.Toggle(settings.FullRespecStoryCompanion,
                            "Respec as Mercenary", GUILayout.ExpandWidth(false));
                        if (settings.FullRespecStoryCompanion)
                        {
                            settings.BackgroundDeity = true;
                        }
                        //settings.PreserveVoice = false;
                        settings.PreserveMCAlignment = false;
                        settings.PreserveMCBirthday = false;
                        settings.PreserveMCName = false;
                        //settings.PreserveMCRace = false;
                        // settings.PreservePortrait = false;
                        //GUILayout.EndVertical();
                    }
                }
                else
                {
                    settings.FullRespecStoryCompanion = false;
                }
                if (selected.IsMC())
                {
                    //GUILayout.BeginVertical();
                    settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreserveMCAlignment = GUILayout.Toggle(settings.PreserveMCAlignment, "Retain Alignment", GUILayout.ExpandWidth(false));
                    settings.PreserveMCBirthday = GUILayout.Toggle(settings.PreserveMCBirthday, "Retain Birthday", GUILayout.ExpandWidth(false));
                    settings.PreserveMCName = GUILayout.Toggle(settings.PreserveMCName, "Retain Name", GUILayout.ExpandWidth(false));
                    //settings.PreserveMCRace = settings.PreserveMCAlignment;
                    settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
                    //GUILayout.EndVertical();
                }
                if (selected.IsStoryCompanionLocal() && !settings.FullRespecStoryCompanion)
                {
                    //GUILayout.BeginVertical();
                    {
                        settings.BackgroundDeity = GUILayout.Toggle(settings.BackgroundDeity, "Choose Background/Deity", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                    //GUILayout.BeginVertical();
                    {
                        settings.OriginalLevel = GUILayout.Toggle(settings.OriginalLevel, "Respec From Recruit Level", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                }
                else if ((selected.IsStoryCompanionLocal()) && settings.FullRespecStoryCompanion)
                {
                    settings.BackgroundDeity = true;
                    settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
                }
                else if (!settings.FullRespecStoryCompanion)
                {
                    settings.BackgroundDeity = false;
                }
                if (selected.IsCustomCompanion())
                {
                    settings.PreserveVoice = false;
                    settings.PreservePortrait = false;
                }
                /*	foreach (var classbp in selected?.Progression.Classes.Select(a => a.CharacterClass))
					{
						if (GUILayout.Button(classbp.name))
						{
							selectedclass = classbp;
						}
					}*/
                /*if(GUILayout.Button("-1 level in"+ selected.Progression.m_ClassesOrder.Last(b => !b.IsMythic)?.name))
                {
					ArbitraryLevelRemoval.RemoveMythicLevel(selected,selected.Progression);
                }*/
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                OldCost = GUILayout.Toggle(OldCost, "Fixed/Scaling Cost", GUILayout.ExpandWidth(false));
                settings.AttributeInClassPage = GUILayout.Toggle(settings.AttributeInClassPage, "Attribute Selection icons on class page", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                if (settings.FreeRespec == true && !forcecost)
                {
                    respecCost = 0L;
                }
                if (settings.FreeRespec == false || forcecost)
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
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.Message + "   " + e.StackTrace); }
        }

        //public static BlueprintCharacterClass selectedclass;
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
#endif
        public static int[] GetInitStatsByUnit(UnitEntityData unit)
        {
            int[] numArray = new int[6] { 10, 10, 10, 10, 10, 10 };
            if (settings.OriginalStats == true)
            {
                if (unit.IsStoryCompanionLocal() && settings.OriginalStats || unit.Blueprint.ToString().Contains("_Companion") && settings.OriginalStats)
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
            if (Game.HasInstance)
            {
                try
                {
                    Main.logger.Log("Library patching initiated");
                    //var tempbp = ExtensionMethods.GetBlueprints();
                    //var unitbp = tempbp.OfType<BlueprintUnit>();
                    //var abilitybps = tempbp.OfType<BlueprintFeature>().ToList().FindAll(list => list.name.Contains("_FeatureList"));
                    //var unitbps = tempbp.OfType<BlueprintUnit>().ToList().FindAll(BPUnits => BPUnits.ToString().Contains("_Companion") && BPUnits.LocalizedName != null && BPUnits.GetComponent<ClassLevelLimit>());
                    ///var noSelectionIfAlreadyHasFeatureBackgroundSelect = new NoSelectionIfAlreadyHasFeature();
                    ///noSelectionIfAlreadyHasFeatureBackgroundSelect.AnyFeatureFromSelection = false;
                    ///var FeatureListsT = abilitybps.ToArray().Select(lis => lis.ToReference<BlueprintFeatureReference>()).ToArray();
                    var religionsbp = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4")?.AllFeatures.Select(a => a).ToArray();
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
                    var tempbackgroundlist = new List<BlueprintFeature>();
                    tempbackgroundlist.Add(Stuf.BackgroundSelect);
                    foreach (var background in Stuf.BackgroundSelect?.AllFeatures)
                    {
                        if (background != null)
                        {
                            tempbackgroundlist.Add(background);
                            if (background.GetType() == typeof(BlueprintFeatureSelection))
                            {
                                foreach (var selection in ((BlueprintFeatureSelection)background)?.AllFeatures)
                                {
                                    tempbackgroundlist.Add(selection);
                                    if (selection.GetType() == typeof(BlueprintFeatureSelection))
                                    {
                                        foreach (var selection2 in ((BlueprintFeatureSelection)selection)?.AllFeatures)
                                        {
                                            tempbackgroundlist.Add(selection2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Stuf.backgroundfeatures = tempbackgroundlist.ToArray();
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
                //var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
                var backgroundsarray = Stuf.backgroundfeatures.Concat<BlueprintFeature>(Stuf.deityfeatures).ToArray();
                Main.IsRespec = true;
                //Main.IsRespec = false;

                /*if (entityData.IsStoryCompanionLocal())
				{
					Main.GetUnitForMemory(entityData.Blueprint);
				}*/
                //Main.EntityUnit = entityData;
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
                if (entityData.IsStoryCompanionLocal() && !Main.settings.FullRespecStoryCompanion || entityData.Blueprint.ToString().Contains("_Companion") && !Main.settings.FullRespecStoryCompanion)
                {
                    foreach (var blueprintf in entityData.Facts.m_Facts)
                    {
                        if (blueprintf.GetType() == typeof(Feature))
                        {
                            var blueprintfeature = (Feature)blueprintf;
                            var nosource = (blueprintfeature.SourceClass == null && blueprintfeature.SourceProgression == null && blueprintfeature.SourceRace == null && blueprintfeature.SourceItem == null && blueprintfeature.SourceRace == null && blueprintfeature.SourceAbility == null && blueprintfeature.SourceFact == null && blueprintfeature.SourceProgression == null && blueprintfeature.SourceAbility == null && blueprintfeature.MythicSource == null && !blueprintfeature.Blueprint.IsClassFeature && blueprintfeature.SourceItem == null);
                            if (backgroundsarray.Contains(blueprintfeature.Blueprint) && !Main.settings.BackgroundDeity || blueprintfeature.Hidden && nosource && !blueprintfeature.NameForAcronym.Contains("Cantrip")) //|| entityData.Progression.Race.m_Features.Any(A => A.Cached == blueprintf.Blueprint))
                            {
                                //Main.logger.Log(blueprintf.ToString());
                                Main.featurestoadd.Add(blueprintfeature.Blueprint);
                            }
                            /*else if(backgroundsarray.Contains(blueprintf.Blueprint) && !Main.settings.BackgroundDeity || blueprintf.Hidden && blueprintf.m_Source == null)
							{
								Main.logger.Log("== null " + blueprintf.ToString());
							}*/
                        }
                    }
                }
                else if (entityData.IsMC())
                {
                    foreach (var blueprintf in entityData.Facts.m_Facts)
                    {
                        if (blueprintf.GetType() == typeof(Feature))
                        {
                            var blueprintfeature = (Feature)blueprintf;
                            var nosource = (blueprintfeature.SourceClass == null && blueprintfeature.SourceProgression == null && blueprintfeature.SourceRace == null && blueprintfeature.SourceItem == null && blueprintfeature.SourceRace == null && blueprintfeature.SourceAbility == null && blueprintfeature.SourceFact == null && blueprintfeature.SourceProgression == null && blueprintfeature.SourceAbility == null && blueprintfeature.MythicSource == null && !blueprintfeature.Blueprint.IsClassFeature && blueprintfeature.SourceItem == null);
                            if (nosource && !statbooks.Keys.Contains(blueprintfeature.NameForAcronym))
                            {
                                Main.featurestoadd.Add(blueprintfeature.Blueprint);
                                //Main.logger.Log(blueprintf.NameForAcronym);
                            }
                        }
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
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
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
			if (unit.IsStoryCompanionLocal())
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
#if UMM
        public static UnityModManager.ModEntry.ModLogger logger;
#endif
#if WrathMod
        public static readonly LogChannel logger = LogChannelFactory.GetOrCreate("Respec");
#endif
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