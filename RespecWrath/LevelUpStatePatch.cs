using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM._VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RespecWrathFork
{
    // Token: 0x02000003 RID: 3
    [HarmonyPatch(typeof(LevelUpState), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(LevelUpState.CharBuildMode), typeof(bool) })]
    [HarmonyPriority(9999)]
    internal static class LevelUpState_ctor_Patch
    {
        /*	private static void Prefix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode, bool isPregen)
			{
			}*/
        // Token: 0x0600000C RID: 12 RVA: 0x000041B4 File Offset: 0x000023B4
        private static void Postfix(LevelUpState __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
        {

       


            /*if(unit.IsPet == true)
            {
		        ///__instance.HasSelection
				///Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.LevelUp);
			}*/
            /*if(Main.PetBool == true && unit.IsPet == true)
            {
				unit.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
				mode = LevelUpState.CharBuildMode.CharGen;
			}*/
            ///unit.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
            if (Main.IsRespec == true)
            {
           
                ///var backgroundsarray = new BlueprintFeature[] { Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
                try
                {
                    

                    if (Main.IsEnabled == true)
                    {
                        /*if(__instance.AttributePoints != Main.settings.PointsCount)
                        {
                            __instance.AttributePoints = Main.settings.PointsCount;
                        }*/
                        /*if (unit.IsStoryCompanion() && !Main.FullRespecStoryCompanion)
                        {
                            if (unit.CharacterName.Contains("Nenio"))
                            {
                                foreach (BlueprintFeatureBase f in unit.Blueprint.Race.Features)
                                {
                                    if(f.ToString() != "ChangeShapeKitsune")
                                    {
                                        unit.AddFact(f);
                                    }
                                }
                                unit.AddFact(ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("52bed4c5617625e4faf029b5c750667f"));
                            }
                            if (!unit.CharacterName.Contains("Nenio"))
                            {
                                foreach (BlueprintFeatureBase f in unit.Blueprint.Race.Features)
                                {
                                    unit.AddFact(f);
                                }
                            }
                        }*/
                        ///unit.Progression.SetRace(unit.Blueprint.Race);
                        if (Main.IsRespec == true && unit.IsPet == false)
                        {
                            var bu = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("047b715d404d5f245ad37019b5b6f1de");
                            if (unit.CharacterName.Contains("Nenio") && !unit.HasFact(bu))
                            {
                                unit.AddFact(bu);
                                ///Main.logger.Log("neniostuff");
                            }
                            /*if (unit.IsCustomCompanion() || unit.IsMainCharacter)
							{
								foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Blueprint.Race.Features.OfType<BlueprintFeatureSelection>())
								{
									Main.logger.Log(blueprintFeatureSelection.ToString());
									__instance.AddSelection(null, unit.Progression.Race, blueprintFeatureSelection, 0);
								}
							}*/

                            if (unit.IsStoryCompanion() && !unit.IsMC() && !Main.settings.FullRespecStoryCompanion || unit.Blueprint.ToString().Contains("_Companion"))
                            {
                               
                                    
                                foreach (BlueprintFeature feat in unit.Blueprint.Race.Features)
                                {
                                    //Main.logger.Log(feat.ToString());
                                    if (!unit.HasFact(feat))
                                    {
                                        unit.AddFact(feat);
                                    }
                                }
                                foreach (BlueprintFeatureSelection blueprintFeatureSelection in unit.Blueprint.Race.Features.OfType<BlueprintFeatureSelection>())
                                {
                                    blueprintFeatureSelection.Obligatory = false;
                                    ///Main.logger.Log(blueprintFeatureSelection.ToString());
                                    //Optimize
                                    __instance.AddSelection(null, unit.Blueprint.Race, blueprintFeatureSelection, 0);
                                }
                            }
                            /*
                            else if (unit.IsMC() && !Main.settings.FullRespecStoryCompanion)
                            {
                                
                                BlueprintRace unitBluePrintRace = unit.Blueprint.Race;
                                if (unitBluePrintRace == null)
                                {
                                    unitBluePrintRace = unit.Descriptor.Progression.Race;
                                    
                                }
                                if (unitBluePrintRace == null)
                                {
                                    unitBluePrintRace = unit.Progression.Race;
                                }
                                Main.logger.Log("Cloning MC race features");

                                foreach (BlueprintFeature feat in unitBluePrintRace.Features)
                                {
                                    //Main.logger.Log(feat.ToString());
                                    if (!unit.HasFact(feat))
                                    {
                                        unit.AddFact(feat);
                                    }
                                }

                                Main.logger.Log("Cloning MC race features foreach number 2");
                                foreach (BlueprintFeatureSelection blueprintFeatureSelection in unitBluePrintRace.Features.OfType<BlueprintFeatureSelection>())
                                {
                                    blueprintFeatureSelection.Obligatory = false;
                                    ///Main.logger.Log(blueprintFeatureSelection.ToString());
                                    //Optimize
                                    __instance.AddSelection(null, unitBluePrintRace, blueprintFeatureSelection, 0);
                                }
                                Main.logger.Log("Done Cloning MC race features");
                                
                            }*/
                            /*if (unit.CharacterName.Contains("Nenio"))
                            {
								var Kitsune = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("fd188bb7bb0002e49863aec93bfb9d99");
								var kitsuneFeatureSelect = new BlueprintFeatureSelection();
								var classickitsune = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cd6cd774fb7cc844b8417193ee3a5ebe");
								var keenkitsune = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d6bc49651fbaa2944bba6e2e5a1720ff");
								var kitsuneFeatures = new List<BlueprintFeature> {keenkitsune,classickitsune };
								kitsuneFeatureSelect.SetFeatures(kitsuneFeatures);
								kitsuneFeatureSelect.Features.AddItem(classickitsune);
								kitsuneFeatureSelect.Features.AddItem(keenkitsune);
								kitsuneFeatureSelect.AllFeatures.AddItem(keenkitsune);
								kitsuneFeatureSelect.AllFeatures.AddItem(classickitsune);
								kitsuneFeatureSelect.m_AllFeatures.AddItem(classickitsune.ToReference<BlueprintFeatureReference>());
								kitsuneFeatureSelect.m_AllFeatures.AddItem(keenkitsune.ToReference<BlueprintFeatureReference>());
								kitsuneFeatureSelect.m_Features.AddItem(classickitsune.ToReference<BlueprintFeatureReference>());
								kitsuneFeatureSelect.m_Features.AddItem(keenkitsune.ToReference<BlueprintFeatureReference>());
								kitsuneFeatureSelect.Group = FeatureGroup.BloodLine;
								kitsuneFeatureSelect.HideInCharacterSheetAndLevelUp = false;
								kitsuneFeatureSelect.HideInUI = false;
								kitsuneFeatureSelect.HideNotAvailibleInUI = false;
								kitsuneFeatureSelect.Obligatory = false;
								unit.AddFact(kitsuneFeatureSelect);
								__instance.AddSelection(null, Kitsune, kitsuneFeatureSelect, 0);
                            }*/
                        }

                        /*var KitsuneHeritage1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cd6cd774fb7cc844b8417193ee3a5ebe");
						var KitsuneHeritage2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d6bc49651fbaa2944bba6e2e5a1720ff");
						var Kitsune = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("fd188bb7bb0002e49863aec93bfb9d99");
						var KitsuneSelect = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ec40cc350b18c8c47a59b782feb91d1f");
						if (unit.Blueprint.NameForAcronym == "Nenio_Companion")
                        {
							KitsuneSelect.Features.AddItem(KitsuneHeritage1);
							KitsuneSelect.Features.AddItem(KitsuneHeritage2);
							KitsuneSelect.AllFeatures.AddItem(KitsuneHeritage1);
							KitsuneSelect.AllFeatures.AddItem(KitsuneHeritage2);
							KitsuneSelect.m_Features.AddItem(KitsuneHeritage1.ToReference<BlueprintFeatureReference>());
							KitsuneSelect.m_Features.AddItem(KitsuneHeritage2.ToReference<BlueprintFeatureReference>());
							KitsuneSelect.m_AllFeatures.AddItem(KitsuneHeritage1.ToReference<BlueprintFeatureReference>());
							KitsuneSelect.m_AllFeatures.AddItem(KitsuneHeritage2.ToReference<BlueprintFeatureReference>());
							KitsuneSelect.Obligatory = false;
							Traverse.Create(unit.Progression.VisibleRace).SetValue(unit.Progression.Race);
							///unit.Progression.VisibleRace = unit.Progression.Race
							__instance.AddSelection(null, unit.OriginalBlueprint.Race, KitsuneSelect, 0);
							Main.logger.Log("nenio");
						    /*foreach(BlueprintFeatureSelection blueprintFeatureSelection in Kitsune.Features)
                            {
								__instance.AddSelection(null, unit.OriginalBlueprint.Race, blueprintFeatureSelection, 2);
                            }*/
                        /*}*/

                        if (Main.IsRespec == true)
                        {
                            unit.Parts.m_Parts.Clear();
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
                            /*if (unit.Pets != null)
							{
								unit.Pets.Clear();
							}*/

                            foreach (BlueprintFeature featuretoadd in Main.featurestoadd)
                            {
                                ///Main.logger.Log(featuretoadd.ToString());
                                if (!unit.HasFact(featuretoadd))
                                {
                                    unit.Descriptor.AddFact(featuretoadd);
                                }
                            }
                   

                            if (unit.Progression.Race == Stuf.HumanRace || unit.Progression.Race == Stuf.HalfElfRace || unit.Progression.Race == Stuf.HalfOrcRace)
                            {
                                if (!Main.settings.FullRespecStoryCompanion && !unit.IsMC())
                                {
                                    __instance.CanSelectRaceStat = true;
                                }

                            }
                       
                            var blueprintUnit = Game.Instance.BlueprintRoot.SelectablePlayerCharacters.Where(u => u == unit.Blueprint).FirstOrDefault();

                            if (unit.IsCustomCompanion() && unit.IsPet == false || unit.IsMainCharacter && unit.IsPet == false || unit.IsStoryCompanion() && Main.settings.FullRespecStoryCompanion)
                            {
                                
                                Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
                                Traverse.Create(__instance).Property("CanSelectVoice", null).SetValue(true);
                                /*if(Main.FullRespecStoryCompanion && unit.Descriptor.Alignment.Undetectable == true && unit.Blueprint.NameForAcronym == "Camelia_Companion")
                                {
								 __instance.CanSelectAlignment = false;
									///Main.logger.Log("noselect");
								}*/
                                if (unit.Blueprint.CharacterName != "Camellia")
                                {
                                    __instance.CanSelectAlignment = true;
                                }
                                else
                                {
                                    __instance.CanSelectAlignment = false;
                                }
                                __instance.CanSelectPortrait = true;
                                __instance.CanSelectRace = true;
                                __instance.CanSelectGender = true;
                                __instance.CanSelectName = true;
                                __instance.CanSelectVoice = true;
                                

                            }
                            else if (unit.IsMC() && unit.IsPet == false && Main.settings.PreserveMCBiographicalInformation)
                            {
                                
                                Traverse.Create(__instance).Field("Mode").SetValue(LevelUpState.CharBuildMode.CharGen);
                                Traverse.Create(__instance.StatsDistribution).Property("Available", null).SetValue(true);

                                //Does not prevent name from appearing, does cause corrent value to load into input field and disregarding changes with current input timing
                                Traverse.Create(__instance).Property("CanSelectName", null).SetValue(false);
                                __instance.CanSelectName = false;


                                Traverse.Create(__instance).Property("CanSelectVoice", null).SetValue(false);
                                __instance.CanSelectVoice = false;


                                Traverse.Create(__instance).Property("CanSelectAlignment", null).SetValue(false);
                                __instance.CanSelectAlignment = false;


                                __instance.CanSelectRace = true;

                                
                                Traverse.Create(__instance).Property("CanSelectPortrait", null).SetValue(false);
                                __instance.CanSelectPortrait = false;



                                __instance.CanSelectGender = true;
                               

                               





                            }
                            else if (unit.IsStoryCompanion() && !Main.settings.FullRespecStoryCompanion || unit.Blueprint.ToString().Contains("_Companion") && unit.IsPet == false && !Main.settings.FullRespecStoryCompanion)
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
                            else
                            {
                             
                            }
                        }
                    }
                }
                catch (NullReferenceException nullref)
                {
                    Main.logger.Log(nullref.ToString());

                    Main.logger.Log($"Null ref object :{nullref.Source}");

                }

                catch (Exception ex)
                {
                    Main.logger.Log(ex.ToString());
                }
            }
            
        }
    }
}


