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
using Kingmaker.Settings;
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
                        settings.OriginalStats =
 GUILayout.Toggle(settings.OriginalStats, "Original Stats", GUILayout.ExpandWidth(false));
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
                float abilityscoreslider =
 (float)Math.Round(GUILayout.HorizontalSlider(value, 0f, 102f, GUILayout.Width(120)));
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
                settings.FreeRespec =
 GUILayout.Toggle(settings.FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
                settings.KeepSkillPoints =
 GUILayout.Toggle(settings.KeepSkillPoints, "Retain Skillpoint Distribution when leveling down", GUILayout.ExpandWidth(false));
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
                    settings.PreserveVoice =
 GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreserveMCAlignment =
 GUILayout.Toggle(settings.PreserveMCAlignment, "Retain Alignment", GUILayout.ExpandWidth(false));
                    settings.PreserveMCBirthday =
 GUILayout.Toggle(settings.PreserveMCBirthday, "Retain Birthday", GUILayout.ExpandWidth(false));
                    settings.PreserveMCName =
 GUILayout.Toggle(settings.PreserveMCName, "Retain Name", GUILayout.ExpandWidth(false));
                    //settings.PreserveMCRace = settings.PreserveMCAlignment;
                    settings.PreservePortrait =
 GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
                    //GUILayout.EndVertical();
                }
                if (selected.IsStoryCompanionLocal() && !settings.FullRespecStoryCompanion)
                {
                    //GUILayout.BeginVertical();
                    {
                        settings.BackgroundDeity =
 GUILayout.Toggle(settings.BackgroundDeity, "Choose Background/Deity", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                    //GUILayout.BeginVertical();
                    {
                        settings.OriginalLevel =
 GUILayout.Toggle(settings.OriginalLevel, "Respec From Recruit Level", GUILayout.ExpandWidth(false));
                    }
                    //GUILayout.EndVertical();
                }
                else if ((selected.IsStoryCompanionLocal()) && settings.FullRespecStoryCompanion)
                {
                    settings.BackgroundDeity = true;
                    settings.PreserveVoice =
 GUILayout.Toggle(settings.PreserveVoice, "Retain Voice", GUILayout.ExpandWidth(false));
                    settings.PreservePortrait =
 GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait", GUILayout.ExpandWidth(false));
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
        public static int MythicXP;
        public static bool IsRespec = false;
        public static bool IsHilorRespec = false;
        public static List<BlueprintFeature> featurestoadd = new List<BlueprintFeature> { };
        public static List<EntityPart> partstoadd = new List<EntityPart> { };

        public static string[] partslist = new String[]
        {
            "Kingmaker.UnitLogic.Parts.UnitPartPartyWeatherBuff", "Kingmaker.UnitLogic.Parts.UnitPartWeariness",
            "Kingmaker.UnitLogic.Parts.UnitPartInteractions", "Kingmaker.UnitLogic.Parts.UnitPartVendor",
            "Kingmaker.UnitLogic.Parts.UnitPartAbilityModifiers", "Kingmaker.UnitLogic.Parts.UnitPartDamageGrace",
            "Kingmaker.UnitLogic.Parts.UnitPartInspectedBuffs", "Kingmaker.AreaLogic.SummonPool.SummonPool+PooledPart",
            "Kingmaker.UnitLogic.Parts.UnitPartHiddenFacts", "Kingmaker.UnitLogic.Parts.UnitPartLocustSwarm",
            "VisualAdjustments.UnitPartVAFX", "VisualAdjustments.UnitPartVAEELs"
        };
#if UMM
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                // Main.GetUnitsForMemory();
                if (IsEnabled == false)
                {
                    return;
                }

                if (!SettingsController.IsInMainMenu())
                {
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    List<UnitEntityData> list = (from x in UIUtility.GetGroup(true, false)
                        where !x.IsInCombat && !x.Descriptor.State.IsFinallyDead
                        select x).ToList<UnitEntityData>();

                    bool flag2 =
                        list.Any((UnitEntityData x) => x.Descriptor.Progression.CharacterLevel == 0 && !x.IsPet);
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
                        if (unitEntityData.IsPlayerFaction)
                        {
                            if (GUILayout.Toggle(Main.selectedCharacter == num, " " + unitEntityData.CharacterName,
                                    new GUILayoutOption[]
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
                        GUILayout.Space(10f);
                        GUILayout.BeginHorizontal();
                        if (selected?.IsStoryCompanionLocal() == true && selected?.IsMainCharacter != true)
                        {
                            settings.OriginalStats = GUILayout.Toggle(settings.OriginalStats, "Original Stats",
                                GUILayout.ExpandWidth(false));
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
                    float abilityscoreslider =
                        (float)Math.Round(GUILayout.HorizontalSlider(value, 0f, 102f, GUILayout.Width(120)));
                    string stringstuff =
                        GUILayout.TextField(abilityscoreslider.ToString(), GUILayout.ExpandWidth(false));
                    int pointsstring = Int32.Parse(stringstuff);
                    value = pointsstring;
                    settings.PointsCount = (int)value;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    int[] initStatsByUnit = Main.GetInitStatsByUnit(selected);
                    int num2 = 0;
                    foreach (StatType stat in StatTypeHelper.Attributes)
                    {
                        GUILayout.Label(
                            string.Format("  {0} {1}", LocalizedTexts.Instance.Stats.GetText(stat),
                                initStatsByUnit[num2++]), new GUILayoutOption[]
                            {
                                GUILayout.ExpandWidth(false)
                            });
                    }

                    GUILayout.Label(("  Extra Ability Score points +" + (settings.PointsCount.ToString())),
                        new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("Submit ({0}g)", Main.respecCost), UnityModManager.UI.button,
                            new GUILayoutOption[]
                            {
                                GUILayout.Width(250f)
                            }))
                    {
#if DEBUG
                    string nameandtype = "";
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

                    modEntry.Logger.Log($"Initiating respec of {nameandtype}");
#endif
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

                    GUILayout.Space(5f);
                    if (selected.Descriptor.Progression.CharacterLevel != 0 && GUILayout.Button(
                            string.Format("Mythic Only ({0}g)", Main.respecCost * 0.25), UnityModManager.UI.button,
                            new GUILayoutOption[]
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
                                MythicRespecClass.MythicRespec(selected);
                            }
                            catch (Exception ex)
                            {
                                modEntry.Logger.Error(ex.ToString());
                            }
                        }
                    }

                    /*GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(
                            string.Format("-1 Level"),
                            UnityModManager.UI.button, new GUILayoutOption[]
                            {
                                GUILayout.Width(250f)
                            }))
                    {
                        ArbitraryLevelRemoval.RemoveClassLevel(selected, selected.Progression);
                    }

                    GUILayout.Space(5f);
                    if (selected.Descriptor.Progression.CharacterLevel > 1 && GUILayout.Button(
                            string.Format("-1 Mythic Level"), UnityModManager.UI.button, new GUILayoutOption[]
                            {
                                GUILayout.Width(250f)
                            }))
                    {
                        Main.MythicXP = selected.Progression.MythicExperience;
                        selected.Progression.RemoveMythicLevel();
                        selected.Progression.AdvanceMythicExperience(Main.MythicXP);
                    }*/

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    settings.FreeRespec =
                        GUILayout.Toggle(settings.FreeRespec, "Free Respec", GUILayout.ExpandWidth(false));
                    /*settings.KeepSkillPoints = GUILayout.Toggle(settings.KeepSkillPoints,
                        "Retain Skillpoint Distribution when leveling down", GUILayout.ExpandWidth(false));*/
                    if (selected.IsStoryCompanionLocal() && !selected.IsMC())
                    {
                        settings.FullRespecStoryCompanion = GUILayout.Toggle(settings.FullRespecStoryCompanion,
                            "Respec as Mercenary", GUILayout.ExpandWidth(false));
                        if (settings.FullRespecStoryCompanion)
                        {
                            settings.BackgroundDeity = true;
                        }

                        settings.PreserveMCAlignment = false;
                        settings.PreserveMCBirthday = false;
                        settings.PreserveMCName = false;
                    }
                    else
                    {
                        settings.FullRespecStoryCompanion = false;
                    }

                    if (selected.IsMC())
                    {
                        settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice",
                            GUILayout.ExpandWidth(false));
                        settings.PreserveMCAlignment = GUILayout.Toggle(settings.PreserveMCAlignment,
                            "Retain Alignment",
                            GUILayout.ExpandWidth(false));
                        settings.PreserveMCBirthday = GUILayout.Toggle(settings.PreserveMCBirthday, "Retain Birthday",
                            GUILayout.ExpandWidth(false));
                        settings.PreserveMCName = GUILayout.Toggle(settings.PreserveMCName, "Retain Name",
                            GUILayout.ExpandWidth(false));
                        settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait",
                            GUILayout.ExpandWidth(false));
                    }

                    if (selected.IsStoryCompanionLocal() && !settings.FullRespecStoryCompanion)
                    {
                        settings.BackgroundDeity = GUILayout.Toggle(settings.BackgroundDeity, "Choose Background/Deity",
                            GUILayout.ExpandWidth(false));
                        settings.OriginalLevel = GUILayout.Toggle(settings.OriginalLevel, "Respec From Recruit Level",
                            GUILayout.ExpandWidth(false));
                    }
                    else if ((selected.IsStoryCompanionLocal()) && settings.FullRespecStoryCompanion)
                    {
                        settings.BackgroundDeity = true;
                        settings.PreserveVoice = GUILayout.Toggle(settings.PreserveVoice, "Retain Voice",
                            GUILayout.ExpandWidth(false));
                        settings.PreservePortrait = GUILayout.Toggle(settings.PreservePortrait, "Retain Portrait",
                            GUILayout.ExpandWidth(false));
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

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    OldCost = GUILayout.Toggle(OldCost, "Fixed/Scaling Cost", GUILayout.ExpandWidth(false));
                    settings.AttributeInClassPage = GUILayout.Toggle(settings.AttributeInClassPage,
                        "Attribute Selection icons on class page", GUILayout.ExpandWidth(false));
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
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Respec Mod has no real functionality in the Main Menu, please load into the game to use.");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    float value = settings.PointsCount;
                    value = Math.Max(0, Math.Min(102, value));
                    float abilityscoreslider =
                        (float)Math.Round(GUILayout.HorizontalSlider(value, 0f, 102f, GUILayout.Width(120)));
                    string stringstuff =
                        GUILayout.TextField(abilityscoreslider.ToString(), GUILayout.ExpandWidth(false));
                    int pointsstring = Int32.Parse(stringstuff);
                    value = pointsstring;
                    settings.PointsCount = (int)value;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    GUILayout.Label(("  Extra Ability Score points +" + (settings.PointsCount.ToString())),
                        new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    settings.AttributeInClassPage = GUILayout.Toggle(settings.AttributeInClassPage,
                        "Attribute Selection icons on class page", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.Message + "   " + e.StackTrace);
            }
        }
#endif
        public static int[] GetInitStatsByUnit(UnitEntityData unit)
        {
            int[] numArray = new int[6] { 10, 10, 10, 10, 10, 10 };
            if (settings.OriginalStats == true)
            {
                if (unit.IsStoryCompanionLocal() && settings.OriginalStats ||
                    unit.Blueprint.ToString().Contains("_Companion") && settings.OriginalStats)
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
            if (Main.IsEnabled == false)
            {
                return;
            }

            if (!Game.HasInstance) return;
            try
            {
                Main.logger.Log("Library data gathering initiated");
                DeityBackground.DeityFeatures = DeityBackground.DeitySelect?.AllFeatures.Select(a => a).ToArray();
                var tempbackgroundlist = new List<BlueprintFeature>();
                tempbackgroundlist.Add(DeityBackground.BackgroundSelect);
                foreach (var background in DeityBackground.BackgroundSelect?.AllFeatures)
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

                DeityBackground.backgroundfeatures = tempbackgroundlist.ToArray();
                Main.haspatched = true;
            }
            catch (Exception ex)
            {
                Main.logger.Log("Error while gathering library data");
                Main.logger.Log(ex.ToString());
            }
        }

        public static void PreRespec(UnitEntityData entityData)
        {
            Main.IsRespec = true;
            try
            {
                Main.IsRespec = true;
                var backgroundsarray = DeityBackground.backgroundfeatures
                    .Concat<BlueprintFeature>(DeityBackground.DeityFeatures).ToArray();
                Main.IsRespec = true;
                foreach (EntityPart entityPart in entityData.Parts.m_Parts)
                {
                    if (Main.partslist.Contains(entityPart.ToString()))
                    {
                        Main.partstoadd.Add(entityPart);
#if DEBUG
                        Main.logger.Log($"PartToAdd: {entityPart.ToString()}");
#endif
                    }
                }

                if (entityData.IsStoryCompanionLocal() && !Main.settings.FullRespecStoryCompanion ||
                    entityData.Blueprint.ToString().Contains("_Companion") && !Main.settings.FullRespecStoryCompanion)
                {
                    foreach (var blueprintf in entityData.Facts.m_Facts)
                    {
                        if (blueprintf.GetType() == typeof(Feature))
                        {
                            var blueprintfeature = (Feature)blueprintf;
                            var nosource = (blueprintfeature.SourceClass == null &&
                                            blueprintfeature.SourceProgression == null &&
                                            blueprintfeature.SourceRace == null &&
                                            blueprintfeature.SourceItem == null &&
                                            blueprintfeature.SourceRace == null &&
                                            blueprintfeature.SourceAbility == null &&
                                            blueprintfeature.SourceFact == null &&
                                            blueprintfeature.SourceProgression == null &&
                                            blueprintfeature.SourceAbility == null &&
                                            blueprintfeature.MythicSource == null &&
                                            !blueprintfeature.Blueprint.IsClassFeature &&
                                            blueprintfeature.SourceItem == null);
                            if (backgroundsarray.Contains(blueprintfeature.Blueprint) &&
                                !Main.settings.BackgroundDeity || blueprintfeature.Hidden && nosource &&
                                !blueprintfeature.NameForAcronym.Contains("Cantrip"))
                            {
                                Main.featurestoadd.Add(blueprintfeature.Blueprint);
                            }
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
                            var nosource = (blueprintfeature.SourceClass == null &&
                                            blueprintfeature.SourceProgression == null &&
                                            blueprintfeature.SourceRace == null &&
                                            blueprintfeature.SourceItem == null &&
                                            blueprintfeature.SourceRace == null &&
                                            blueprintfeature.SourceAbility == null &&
                                            blueprintfeature.SourceFact == null &&
                                            blueprintfeature.SourceProgression == null &&
                                            blueprintfeature.SourceAbility == null &&
                                            blueprintfeature.MythicSource == null &&
                                            !blueprintfeature.Blueprint.IsClassFeature &&
                                            blueprintfeature.SourceItem == null);
                            if (nosource && !statbooks.Keys.Contains(blueprintfeature.NameForAcronym))
                            {
                                Main.featurestoadd.Add(blueprintfeature.Blueprint);
                            }
                        }
                    }
                }

                RespecClass.Respecialize(entityData);
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
                Main.IsRespec = false;
            }
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
        private static int selectedCharacter = 0;

        public enum ExtraPointsType
        {
            Original,
            P25
        }
    }
}