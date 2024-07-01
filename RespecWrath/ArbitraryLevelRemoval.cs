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

        public static BlueprintCharacterClass GetClassSource(BlueprintScriptableObject blueprint,
            BlueprintCharacterClass tomatch)
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

        public static void RemoveClassLevel(UnitEntityData unitdata, UnitProgressionData data)
        {
            try
            {
                var classtoremove = data.ClassesOrder.Last(a => !a.IsMythic);
                data.Owner.Stats.HitPoints.BaseValue -= ((int)classtoremove.GetHitDie(unitdata.Descriptor));
                int index = data.m_ClassesOrder.FindLastIndex((BlueprintCharacterClass c) => c == classtoremove);
                BlueprintCharacterClass characterClass = data.m_ClassesOrder.Get(index, null);
                ClassData classData = data.GetClassData(characterClass);
                if (classData == null)
                {
                    Main.logger.Log("UnitProgressionData.RemoveMythicLevel: can't find class data");
                    return;
                }

                int level = classData.Level;
                classData.Level--;
                if (classData.Level < 1)
                {
                    data.Classes.Remove(classData);
                }

                foreach (KeyValuePair<BlueprintProgression, ProgressionData> tuple in TempListExtension
                             .ToTempList<BlueprintProgression, ProgressionData>(data.m_Progressions))
                {
                    BlueprintProgression blueprintProgression;
                    ProgressionData progressionData;
                    tuple.Deconstruct(out blueprintProgression, out progressionData);
                    BlueprintProgression blueprintProgression2 = blueprintProgression;
                    ProgressionData progressionData2 = progressionData;
                    if (progressionData2.Blueprint.Classes != null &&
                        progressionData2.Blueprint.Classes.Any(a => a == classData.CharacterClass))
                    {
                        progressionData2.Level = blueprintProgression2.CalcLevel(unitdata.Descriptor);
                        if (progressionData2.Level < 1)
                        {
                            Main.logger.Log(progressionData.Blueprint.NameForAcronym + " :removed");
                            var levelentry = progressionData.GetLevelEntry(progressionData.Level);
                            foreach (var feature in levelentry.Features)
                            {
                                unitdata.Facts.RemoveAll<Feature>(a => a.Blueprint == feature);
                            }

                            data.m_Progressions.Remove(blueprintProgression2);
                        }
                    }
                }

                foreach (KeyValuePair<BlueprintFeatureSelection, FeatureSelectionData> tuple2 in data.m_Selections
                             .ToTempList<BlueprintFeatureSelection, FeatureSelectionData>())
                {
                    BlueprintFeatureSelection blueprintFeatureSelection;
                    FeatureSelectionData featureSelectionData;
                    tuple2.Deconstruct(out blueprintFeatureSelection, out featureSelectionData);
                    BlueprintFeatureSelection key = blueprintFeatureSelection;
                    FeatureSelectionData featureSelectionData2 = featureSelectionData;
                    if (featureSelectionData2.Source.Blueprint.GetType() == typeof(BlueprintProgression))
                    {
                        var progressionbp = ((BlueprintProgression)featureSelectionData2.Source.Blueprint);
                        if (progressionbp.Classes != null &&
                            progressionbp.Classes.Any(a => a == classData.CharacterClass))
                        {
                            if (featureSelectionData2.SelectionsByLevel.All(a => a.Key == level))
                            {
                                foreach (var featurelist in featureSelectionData2.SelectionsByLevel)
                                {
                                    foreach (var feature in featurelist.Value)
                                    {
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
                    foreach (var current in data.Features.Enumerable.ToTempList<Feature>())
                    {
                        if (((current.IsAttached && !current.IsMythic) &&
                             ((current.Blueprint.GetType() != typeof(BlueprintProgression) ||
                               !data.m_Progressions.Keys.Any(a =>
                                   a == (BlueprintProgression)current
                                       .Blueprint))) &&
                             (current.SourceLevel == level ||
                              current.SourceLevel ==
                              data.CharacterLevel)))
                        {
                            if (current.Rank > 1)
                            {
                                current.RankToSource.Remove<Feature.SourceAndLevel>(
                                    (Predicate<Feature.SourceAndLevel>)(i =>
                                        (GetClassSource(i.Blueprint, classData.CharacterClass) ==
                                            classData.CharacterClass || current.SourceClass == null) &&
                                        i.Level == level));

                                current.SetSourceSameAsLastRankToSourceValue();
                                current.RemoveRank();
                            }
                            else
                            {
                                if (current != null && !current.Name.IsNullOrEmpty() &&
                                    data.Owner.Facts.Contains(current))
                                {
                                    if (data.Owner.HasFact(current))
                                    {
                                        data.Owner.RemoveFact((EntityFact)current);
                                    }
                                }
                            }
                        }
                    }
                }

                Spellbook spellbook =
                    (classData.Spellbook != null) ? data.Owner.GetSpellbook(classData.Spellbook) : null;
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
                    var tosubtract = currentlevelbab - classData.CharacterClass.BaseAttackBonus.GetBonus(level - 1);
                    unitdata.Stats.BaseAttackBonus.BaseValue =
                        (unitdata.Stats.BaseAttackBonus.PermanentValue - tosubtract);
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
#if DEBUG
                                Main.logger.Log($"Decreased {skill.Key} By : {skill.Value}");
#endif
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
                        unitdata.Stats.GetAttribute(attribute).BaseValue -= 1;
                    }
                }
                data.CharacterLevel--;
                //Animal Companion
                if (unitdata.GetPet(Kingmaker.Enums.PetType.AnimalCompanion) != null)
                {
                    var addpet = data.Features.Manager.List.FirstOrDefault(a => a.GetComponent<AddPet>() != null);
                    if (addpet != null)
                    {
                        var addpetcomponent = addpet.GetComponent<AddPet>();
                        var effectivelevelforpet = unitdata.GetFact(addpetcomponent.LevelRank);
                        var petlevel = AddPet.RankToLevelAnimalCompanion[effectivelevelforpet.GetRank()];
                        var pet = unitdata.GetPet(Kingmaker.Enums.PetType.AnimalCompanion);
                        for (int i = pet.Progression.CharacterLevel; i > petlevel; i--)
                        {
                            ArbitraryLevelRemoval.RemoveClassLevel(pet, pet.Progression);
                            pet.Progression.Experience = pet.Progression.ExperienceTable.Bonuses[petlevel];
                        }
                    }
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