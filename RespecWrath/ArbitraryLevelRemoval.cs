using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Notes
/// Unitprogressiondata.AddClassLevel()
/// Unitprogressiondata.RemoveClassLevels()
/// Selection Stuff
///
///
///
/// </summary>

namespace RespecWrath
{
    internal static class ArbitraryLevelRemoval
    {
        public static void UpdateSpellbookLevel(this Spellbook spellbook)
        {
            if (spellbook.IsMythic)
            {
                return;
            }
            int maxSpellLevel = spellbook.MaxSpellLevel;
            spellbook.m_BaseLevelInternal--;
            //int num = (spellbook.Owner.Progression.MythicLevel < 3) ? 0 : (spellbook.IsStandaloneMythic ? (spellbook.Owner.Progression.MythicLevel * 2) : spellbook.Owner.Progression.MythicLevel);
            //if (spellbook.IsStandaloneMythic)
            //{
            //spellbook.m_BaseLevelInternal = num;
            //}
            /*else
			{
				spellbook.m_MythicLevelInternal = num;
			}*/
            int maxSpellLevel2 = spellbook.MaxSpellLevel;
            for (int i = maxSpellLevel; i > maxSpellLevel2; i--)
            {
                foreach (AbilityData abilityData in spellbook.GetKnownSpells(i).ToTempList<AbilityData>())
                {
                    if (!abilityData.CopiedFromScroll && !abilityData.IsTemporary)
                    {
                        spellbook.RemoveSpell(abilityData.Blueprint);
                    }
                }
            }
        }

        public static BlueprintCharacterClass GetClassSource(BlueprintScriptableObject blueprint, BlueprintCharacterClass tomatch)
        {
            BlueprintCharacterClass blueprintCharacterClass;
            if ((blueprintCharacterClass = (blueprint as BlueprintCharacterClass)) != null)
            {
                if (blueprintCharacterClass.IsMythic)
                {
                    return null;
                }
                return blueprintCharacterClass;
            }
            else
            {
                BlueprintProgression blueprintProgression;
                if ((blueprintProgression = (blueprint as BlueprintProgression)) != null)
                {
                    //	Main.logger.Log("ReturnedClass: "+blueprintProgression.FirstClass.ToString());
                    return blueprintProgression.Classes.FirstOrDefault(a => a == tomatch);
                }
                return null;
            }
        }

        public static void DestroyPet(UnitEntityData pet)
        {
            pet.SwitchFactions(BlueprintRoot.Instance.SystemMechanics.FactionNeutrals);
            pet.RemoveMaster();
            pet.State.MarkedForDeath = true;
        }

        public static void RemoveMythicLevel(UnitEntityData unitdata, UnitProgressionData data)
        {
            try
            {
                //if(classtoremove.IsMythic)
                {
                    //	data.RemoveMythicLevel();
                    //	return;
                }
                //if (data.MythicExperience < 1)
                {
                    //PFLog.Default.ErrorWithReport("UnitProgressionData.RemoveMythicLevel: MythicExperience < 1", Array.Empty<object>());
                    //return;
                }
                //data.MythicExperience--;
                //if (data.MythicLevel <= data.MythicExperience)
                //{
                //	return;
                //}
                var classtoremove = data.ClassesOrder.Last(a => !a.IsMythic);

                data.Owner.Stats.HitPoints.BaseValue -= ((int)classtoremove.GetHitDie(unitdata.Descriptor));
                int index = data.m_ClassesOrder.FindLastIndex((BlueprintCharacterClass c) => c == classtoremove);
                BlueprintCharacterClass characterClass = data.m_ClassesOrder.Get(index, null);
                ClassData classData = data.GetClassData(characterClass);
                if (classData == null)
                {
                    PFLog.Default.ErrorWithReport("UnitProgressionData.RemoveMythicLevel: can't find mythic class data", Array.Empty<object>());
                    return;
                }
                int level = classData.Level;
                classData.Level--;
                if (classData.Level < 1)
                {
                    data.Classes.Remove(classData);
                }
                foreach (KeyValuePair<BlueprintProgression, ProgressionData> tuple in TempListExtension.ToTempList<BlueprintProgression, ProgressionData>(data.m_Progressions))
                {
                    BlueprintProgression blueprintProgression;
                    ProgressionData progressionData;
                    tuple.Deconstruct(out blueprintProgression, out progressionData);
                    BlueprintProgression blueprintProgression2 = blueprintProgression;
                    ProgressionData progressionData2 = progressionData;
                    //	Main.logger.Log(progressionData2.Blueprint.FirstClass?.NameForAcronym + " ProgressionData, Current Class:" + classData.CharacterClass.NameForAcronym);

                    if (progressionData2.Blueprint.Classes != null && progressionData2.Blueprint.Classes.Any(a => a == classData.CharacterClass))
                    {
                        progressionData2.Level = blueprintProgression2.CalcLevel(unitdata.Descriptor);
                        //Main.logger.Log(progressionData2.Blueprint.NameForAcronym+"  "+progressionData2.Level);
                        if (progressionData2.Level < 1)
                        {
                            Main.logger.Log(progressionData.Blueprint.NameForAcronym + " :removed");
                            var levelentry = progressionData.GetLevelEntry(progressionData.Level);
                            //var levelentriesnew = progressionData.LevelEntries.ToList();
                            //levelentriesnew.Remove(levelentry);
                            //progressionData.LevelEntries = levelentriesnew.ToArray();
                            foreach (var feature in levelentry.Features)
                            {
                                //Main.logger.Log("ProgressionRemovedFeature: "+ feature.NameForAcronym);
                                unitdata.Facts.RemoveAll<Feature>(a => a.Blueprint == feature);
                            }
                            data.m_Progressions.Remove(blueprintProgression2);
                        }
                        //else
                        //else if(progressionData2.LevelEntries.Any(a => a.Level > progressionData2.Level+1))
                        //else if (progressionData2.LevelEntries.All(a => a.Level > progressionData2.Level + 1))
                        {
                            /*	var levelentry = progressionData.GetLevelEntry(progressionData.Level+1);
                                //var levelentriesnew = progressionData.LevelEntries.ToList();
                                //levelentriesnew.Remove(levelentry);
                                //progressionData.LevelEntries = levelentriesnew.ToArray();
                                foreach (var feature in levelentry.Features)
                                {
                                    Main.logger.Log("ProgressionRemovedFeature: " + feature.NameForAcronym);
                                    unitdata.Facts.RemoveAll<Feature>(a => a.Blueprint == feature);
                                }
                                data.m_Progressions.Remove(blueprintProgression2);*/
                        }
                        //else
                        {
                            //	return;
                        }
                    }
                }

                //^^ this above here works (well no exceptions) (NEEDS TO REMOVE PROGRESSIONS SUCH AS BLOODRAGER PRIMALIST TAKE RAGE POWERS)

                foreach (KeyValuePair<BlueprintFeatureSelection, FeatureSelectionData> tuple2 in data.m_Selections.ToTempList<BlueprintFeatureSelection, FeatureSelectionData>())
                {
                    BlueprintFeatureSelection blueprintFeatureSelection;
                    FeatureSelectionData featureSelectionData;
                    tuple2.Deconstruct(out blueprintFeatureSelection, out featureSelectionData);
                    BlueprintFeatureSelection key = blueprintFeatureSelection;
                    FeatureSelectionData featureSelectionData2 = featureSelectionData;
                    //Main.logger.Log(tuple2.Key.NameForAcronym + "  :" + tuple2.Value.ToString());
                    if (featureSelectionData2.Source.Blueprint.GetType() == typeof(BlueprintProgression))
                    {
                        var progressionbp = ((BlueprintProgression)featureSelectionData2.Source.Blueprint);
                        if (progressionbp.Classes != null && progressionbp.Classes.Any(a => a == classData.CharacterClass))
                        // probably this too
                        {
                            //if (featureSelectionData2.SelectionsByLevel.All((KeyValuePair<int, List<BlueprintFeature>> s) => s.Value.Empty<BlueprintFeature>()))
                            //Main.logger.Log(level.ToString());
                            //foreach(var thing in featureSelectionData2.SelectionsByLevel)
                            {
                                //	Main.logger.Log(thing.Key.ToString() + thing.Value.ToString());
                            }
                            if (featureSelectionData2.SelectionsByLevel.All(a => a.Key == level))
                            // ^^ this ( i think)
                            {
                                //Main.logger.Log("Removed Selection: "+key.NameForAcronym);
                                foreach (var featurelist in featureSelectionData2.SelectionsByLevel)
                                {
                                    foreach (var feature in featurelist.Value)
                                    {
                                        //	Main.logger.Log("RemovedSelectionFeature : "+feature.NameForAcronym);
                                        data.Features.RemoveFact(feature);
                                    }
                                }
                                data.m_Selections.Remove(key);
                            }
                            featureSelectionData2.RemoveLevel(level);
                        }
                    }
                }
                using (ContextData<AddPet.DeactivateAction>.Request().Setup(new Action<UnitEntityData>(DestroyPet)))
                {
                    //	using (List<Feature>.Enumerator enumerator = data.Features.Enumerable.ToTempList<Feature>().GetEnumerator())
                    foreach (var current in data.Features.Enumerable.ToTempList<Feature>())
                    {
                        //label_22:
                        //while (enumerator.MoveNext())
                        {
                            //Feature current = enumerator.Current;
                            //while (true)
                            {
                                if (((current.IsAttached && !current.IsMythic) && ((current.Blueprint.GetType() != typeof(BlueprintProgression) || !data.m_Progressions.Keys.Any(a => a == (BlueprintProgression)current.Blueprint))) && /*((current.SourceClass != null && current.SourceClass == classData.CharacterClass) &&*/ (current.SourceLevel == level || current.SourceLevel == data.CharacterLevel))) /*|| current.RankToSource.HasItem<Feature.SourceAndLevel>((Func<Feature.SourceAndLevel, bool>)(i => GetClassSource(i.Blueprint) == classData.CharacterClass && i.Level == level)))//)*/
                                {
                                    if (current.Rank > 1)
                                    {
                                        //Main.logger.Log("RemovedRank: " + current.Name);
                                        current.RankToSource.Remove<Feature.SourceAndLevel>((Predicate<Feature.SourceAndLevel>)(i => (GetClassSource(i.Blueprint, classData.CharacterClass) == classData.CharacterClass || current.SourceClass == null) && i.Level == level));

                                        current.SetSourceSameAsLastRankToSourceValue();
                                        current.RemoveRank();
                                    }
                                    else
                                    {
                                        if (current != null && !current.Name.IsNullOrEmpty() && data.Owner.Facts.Contains(current))
                                        {
                                            //	Main.logger.Log("RemovedFact: " + current.Name);
                                            if (data.Owner.HasFact(current))
                                            {
                                                data.Owner.RemoveFact((EntityFact)current);
                                            }
                                        }
                                    }
                                }
                                //else
                                //goto label_22;
                            }
                        }
                    }
                }
                Spellbook spellbook = (classData.Spellbook != null) ? data.Owner.GetSpellbook(classData.Spellbook) : null;
                if (spellbook != null)
                {
                    spellbook.UpdateSpellbookLevel();
                    spellbook.UpdateAllSlotsSize();
                    if (spellbook.CasterLevel < 1)
                    {
                        data.Owner.DeleteSpellbook(classData.Spellbook);
                    }
                    else
                    {
                        spellbook.UpdateAllSlotsSize(true);
                    }
                }
                //Remove BAB
                {
                    var currentlevelbab = classData.CharacterClass.BaseAttackBonus.GetBonus(level);
                    //Main.logger.Log(currentlevelbab.ToString()+" CurrentBaB");
                    var tosubtract = currentlevelbab - classData.CharacterClass.BaseAttackBonus.GetBonus(level - 1);
                    //Main.logger.Log(tosubtract.ToString()+" ToSubtract");
                    //unitdata.Stats.BaseAttackBonus.PermanentValue = (unitdata.Stats.BaseAttackBonus.PermanentValue - tosubtract);
                    unitdata.Stats.BaseAttackBonus.BaseValue = (unitdata.Stats.BaseAttackBonus.PermanentValue - tosubtract);
                }
                var charentry = GlobalLevelInfo.Instance.ForCharacter(unitdata);
                
                //Remove Excess skill points with the memory, if no memory reset.
                {
                    if (!Main.settings.KeepSkillPoints)
                    {
                        if (!charentry.SkillsByLevel.ContainsKey(data.CharacterLevel))
                        {
                            var stats = unitdata.Stats;
                            stats.SkillAthletics.BaseValue = 0;
                            stats.SkillKnowledgeArcana.BaseValue = 0;
                            stats.SkillKnowledgeWorld.BaseValue = 0;
                            stats.SkillLoreNature.BaseValue = 0;
                            stats.SkillLoreReligion.BaseValue = 0;
                            stats.SkillMobility.BaseValue = 0;
                            stats.SkillPerception.BaseValue = 0;
                            stats.SkillPersuasion.BaseValue = 0;
                            stats.SkillStealth.BaseValue = 0;
                            stats.SkillThievery.BaseValue = 0;
                            stats.SkillUseMagicDevice.BaseValue = 0;
                        }
                        else
                        {
                            foreach (var skill in charentry.SkillsByLevel[data.CharacterLevel])
                            {
                                //Main.logger.Log("Decreased " + skill.Key + "By : " +skill.Value);
                                unitdata.Stats.GetStat(skill.Key).BaseValue -= skill.Value;
                            }
                            charentry.SkillsByLevel.Remove(data.CharacterLevel - 1);
                        }
                    }
                }
                //Ability Scores
                {
                    if (charentry.AbilityScoresByLevel.TryGetValue(data.CharacterLevel, out StatType attribute))
                    {
                        //Main.logger.Log("Decreased :" + attribute.ToString());
                        unitdata.Stats.GetAttribute(attribute).BaseValue -= 1;
                    }
                }
                data.CharacterLevel--;
                //Animal Companion
                if (unitdata.GetPet(Kingmaker.Enums.PetType.AnimalCompanion) != null)
                {
                    //Main.logger.Log(AddPet.RankToLevelAnimalCompanion[data.CharacterLevel].ToString());
                    var addpet = data.Features.Manager.List.FirstOrDefault(a => a.GetComponent<AddPet>() != null);
                    if (addpet != null)
                    {
                        var addpetcomponent = addpet.GetComponent<AddPet>();
                        var effectivelevelforpet = unitdata.GetFact(addpetcomponent.LevelRank);
                        var petlevel = AddPet.RankToLevelAnimalCompanion[effectivelevelforpet.GetRank()];
                        //Main.logger.Log(petlevel.ToString());
                        var pet = unitdata.GetPet(Kingmaker.Enums.PetType.AnimalCompanion);
                        for (int i = pet.Progression.CharacterLevel; i > petlevel; i--)
                        {
                            //Main.logger.Log("DecreasePet" );
                            ArbitraryLevelRemoval.RemoveMythicLevel(pet, pet.Progression);
                            pet.Progression.Experience = pet.Progression.ExperienceTable.Bonuses[petlevel];
                        }
                    }
                    //if(unitdata.GetPet(Kingmaker.Enums.PetType.AnimalCompanion)?.Progression.CharacterLevel > AddPet.RankToLevelAnimalCompanion[data.CharacterLevel])
                }
                data.m_ClassesOrder.RemoveAt(index);
                data.Classes.Sort();
                unitdata.Stats.CleanupModifiers();
                data.Owner.OnGainClassLevel(characterClass);
                data.RecalculateHP();
                data.TryRestoreClassFeatures();
                data.UpdateAdditionalVisualSettings();
                data.SyncWithView();
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
    }
}