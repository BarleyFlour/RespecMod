﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UI;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace RespecWrath
{
    internal static class RespecClass
    {
        public static List<BlueprintUnitFact> Neniofacts { get; set; }

        public static void Respecialize(this UnitEntityData unit, Action successCallback = null)
        {
            try
            {
                UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: requested for {0}", unit),
                    Array.Empty<object>());

                Main.MythicXP = unit.Progression.MythicExperience;
                int experience = unit.Descriptor.Progression.Experience;
                ReplaceUnitBlueprintForRespec replaceUnitBlueprintForRespec =
                    unit.Blueprint.GetComponent<ReplaceUnitBlueprintForRespec>().Or(null);
                BlueprintUnit unit2 =
                    ((replaceUnitBlueprintForRespec != null)
                        ? replaceUnitBlueprintForRespec.Blueprint.Or(null)
                        : null) ?? unit.Blueprint;
                UnitEntityData newUnit;
                //Scroll refund

                var scrollstoadd = new List<BlueprintItemEquipmentUsable>();
                {
                    var loadedscrolls = Game.Instance.BlueprintRoot.CraftRoot.m_ScrollsItems.Select(a =>
                        ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>(a.Guid));
                    foreach (var spellbook in unit.Spellbooks)
                    {
                        foreach (var scrollspell in spellbook.GetAllKnownSpells())
                        {
                            if (scrollspell.CopiedFromScroll)
                            {
                                //Main.logger.Log("NameMatch : "  + scrollspell.Blueprint.NameForAcronym);
                                /*  foreach (var asd in loadedscrolls)
                                  {
                                      Main.logger.Log(asd.Ability.NameForAcronym);
                                  }*/
                                // if (scrollspell.m_Fact != null)
                                {
                                    if (loadedscrolls.TryFind(
                                            a => a.Ability.NameForAcronym == scrollspell.Blueprint.NameForAcronym,
                                            out BlueprintItemEquipmentUsable item))
                                    {
                                        // Main.logger.Log("ItemMatch : " + scrollspell.NameForAcronym);
                                        scrollstoadd.Add(item);
                                    }
                                }
                                //
                            }
                        }
                    }
                }
                // Statbook refund
                BlueprintFeature toaddifcanceled = null;
                {
                    foreach (var blueprintf in unit.Facts.m_Facts.ToTempList())
                    {
                        if (blueprintf.GetType() == typeof(Feature))
                        {
                            var blueprintfeature = (Feature)blueprintf;
                            if (Main.statbooks.ContainsKey(blueprintfeature.NameForAcronym))
                            {
                                scrollstoadd.Add(
                                    ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>(
                                        Main.statbooks[blueprintfeature.NameForAcronym]));
                                //unit.Facts.Remove(blueprintfeature);
                                unit.RemoveFact(blueprintfeature.Blueprint);
                                toaddifcanceled = blueprintfeature.Blueprint;
                                //  Main.logger.Log("Statbook:"+blueprintfeature.NameForAcronym+"for adding");
                            }
                        }
                    }
                }
                var classestoadd = unit.Progression.Classes.Where(a => a.CharacterClass.IsMythic);

                try
                {
                    if (unit.CharacterName.Contains("Nenio") &&
                        !Game.Instance.Player.EtudesSystem.EtudeIsCompleted(
                            ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf")))
                    {
                        ///Main.logger.Log("Nene");
                        ///Main.logger.Log(ResourcesLibrary.TryGetBlueprint<BlueprintEtude>("f1877e6b308bc9c4a89c028c7b116ccf").ToString());
                        Main.NenioEtudeBool = true;
                        ///var KitsuneHeritageSelect = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ec40cc350b18c8c47a59b782feb91d1f");
                        var KitsuneHeritageClassic =
                            ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
                        var KitsuneHeritageKeen =
                            ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
                        var facts = new List<BlueprintUnitFact> { KitsuneHeritageClassic, KitsuneHeritageKeen };
                        foreach (IHiddenUnitFacts i in unit.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
                        {
                            foreach (BlueprintUnitFact fact in facts)
                            {
                                i.Facts.Remove(fact);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.logger.Log(e.ToString());
                }

                using (ContextData<UnitEntityData.DoNotCreateItems>.Request())
                {
                    newUnit = Game.Instance.CreateUnitVacuum(unit2);
                }


                /*var asd = newUnit.Facts.m_Facts.Where(a => a.Blueprint.name.Contains("Atheism"));
                if (asd.Any())
                {
                    foreach (var sdasd in asd)
                    {
                        newUnit.Progression.Features.Manager.Remove(sdasd);
                    }
                    newUnit.Facts.Remove(asd.First());
                }
                newUnit.Progression.Features.RemoveFact(newUnit.GetFeature(Utilities.GetBlueprint<BlueprintFeature>("92c0d2da0a836ce418a267093c09ca54")));
                */
                BlueprintUnit defaultPlayerCharacter = Game.Instance.BlueprintRoot.DefaultPlayerCharacter;
                UnitDescriptor descriptor = newUnit.Descriptor;
                var entityData = newUnit;
                UnitProgressionData unitProgressionData = newUnit.Progression;

                if (Main.settings.PreservePortrait && unit.IsMC())
                {
                    newUnit.UISettings.SetPortrait(unit.Portrait);

                    // newUnit.Portrait = unit.Portrait;
                }

                if (Main.settings.PreserveMCName && unit.IsMC())
                {
                    if (!unit.CharacterName.IsNullOrEmpty())
                    {
                        newUnit.Descriptor.CustomName = unit.CharacterName;
                    }
                }

                if (Main.settings.PreserveMCAlignment && unit.IsMC())
                {
                    newUnit.Alignment.CopyFrom(unit.Alignment);
                }

                //LevelUpHelper.GetTotalIntelligenceSkillPoints(descriptor, 0);
                //LevelUpHelper.GetTotalSkillPoints(descriptor, 0);
                Traverse.Create(descriptor.Progression).Field("m_Selections")
                    .GetValue<Dictionary<BlueprintFeatureSelection, FeatureSelectionData>>().Clear();
                entityData.PrepareRespec();
                entityData.Descriptor.Buffs.RawFacts.Clear();
                Traverse.Create(descriptor).Field("Stats").SetValue(new CharacterStats(descriptor));
                descriptor.Stats.HitPoints.BaseValue = defaultPlayerCharacter.MaxHP + 1;
                descriptor.Stats.Speed.BaseValue = defaultPlayerCharacter.Speed.Value;
                descriptor.UpdateSizeModifiers();

                //var BPBackgroundList = new List<BlueprintFeature> { };
                /*foreach (EntityPart entityPart in unit.Parts.Parts)
                {
                    if (Main.partslist.Contains(entityPart.ToString()))
                    {
                        Main.partstoadd.Add(entityPart);
                        ///Main.logger.Log(entityPart.ToString());
                    }
                }*/
                ///Traverse.Create(unit.Parts).Field("Parts").SetValue(Main.partstoadd);
                if (unit.Parts.Get<UnitPartCompanion>().State == CompanionState.InParty)
                {
                    unit.Ensure<UnitPartCompanion>().SetState(CompanionState.InParty);
                }

                foreach (Buff buff in entityData.Buffs)
                {
                    try
                    {
                        buff.Detach();
                        buff.Remove();
                        buff.TurnOff();
                        buff.Dispose();
                    }
                    catch (Exception e)
                    {
                        Main.logger.Error(e.ToString());
                        continue;
                    }
                }

                /*if (unit.IsStoryCompanionLocal())
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
                newUnit.PreviewOf = unit;
                newUnit.Progression.AdvanceExperienceTo(unit.Descriptor.Progression.Experience, false, false);
                newUnit.Progression.AdvanceMythicExperience(unit.Descriptor.Progression.MythicExperience, false);
                IEnumerable<BlueprintCharacterClass> obligatoryMythicClasses = from i in unit.Progression.ClassesOrder
                    where i.IsMythic
                    select i;
                newUnit.Progression.SetObligatoryMythicClasses(obligatoryMythicClasses);
                if (unit.IsMainCharacter || unit.IsCloneOfMainCharacter)
                {
                    newUnit.MarkAsCloneOfMainCharacter();
                }

                if (newUnit.Progression.CharacterLevel < 1)
                {
                    foreach (ModifiableValueAttributeStat modifiableValueAttributeStat in newUnit.Stats.Attributes)
                    {
                        modifiableValueAttributeStat.BaseValue = 10;
                    }
                }

                UICommon uicommon = Game.Instance.UI.Common.Or(null);
                DollRoom dollRoom = (uicommon != null) ? uicommon.DollRoom : null;
                if (dollRoom != null)
                {
                    dollRoom.CreateDefaultDolls();
                    dollRoom.m_Unit = (newUnit);
                }

                BlueprintFeature[] array = (from i in unit.Progression.Features.Enumerable.Except((Feature i) =>
                        i.SourceProgression || i.SourceClass || i.SourceRace || i.Blueprint.IsClassFeature ||
                        i.SourceAbility || i.SourceFact != null)
                    select i.Blueprint).ToArray<BlueprintFeature>();
                Feature[] array2 = (from i in newUnit.Progression.Features.Enumerable
                    where !i.Blueprint.IsClassFeature && !unit.HasFact(i.Blueprint)
                    select i).ToArray<Feature>();
                if (!Main.settings.FullRespecStoryCompanion)
                {
                    foreach (BlueprintFeature blueprintFeature in array)
                    {
                        Main.logger.Log("FeatureAdd:" + blueprintFeature.name);
                        if (!newUnit.HasFact(blueprintFeature))
                        {
                            newUnit.AddFact(blueprintFeature, null, null);
                            UnitHelper.Channel.Log(
                                string.Format("UnitHelper.Respec: restore feature {0}", blueprintFeature),
                                Array.Empty<object>());
                            //Main.logger.Log(string.Format("UnitHelper.Respec: restore feature {0}",blueprintFeature));
                        }
                    }
                }

                foreach (Feature feature in array2)
                {
                    newUnit.RemoveFact(feature);
                    UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: remove feature {0}", feature),
                        Array.Empty<object>());
                    // Main.logger.Log(string.Format("UnitHelper.Respec: remove feature {0}", feature));
                }

                if (Main.settings.BackgroundDeity)
                {
                    newUnit.Facts.RemoveAll((EntityFact a) =>
                        a.NameForAcronym.Contains("Background") || a.NameForAcronym.Contains("background"));
                }

                //
                if (!Main.settings.FullRespecStoryCompanion && unit.IsStoryCompanionLocal() && !unit.IsMC())
                {
                    foreach (var raceFeature in unit.Progression.Race.Features.m_Array)
                    {
                        newUnit.AddFact(raceFeature);
                    }
                }
                
                if (Main.settings.FullRespecStoryCompanion && unit.IsStoryCompanionLocal())
                {
                    //  Main.logger.Log("IsStoryCompanionLocal and full respec");
                    var component =
                        unit.Progression.Race.ComponentsArray.FirstOrDefault(a =>
                            a.GetType() == typeof(AddFeatureOnApply));
                    if (component != null && ((AddFeatureOnApply)component).Feature != null)
                    {
                        newUnit.RemoveFact(((AddFeatureOnApply)component).Feature);
                    }

                    foreach (var raceFeature in unit.Progression.Race.Features)
                    {
                        // Main.logger.Log("Removed:"+raceFeature.ToString());

                        //  Main.logger.Log(raceFeature.NameForAcronym);

                        if (raceFeature.GetType() == typeof(BlueprintFeatureSelection))
                        {
                            foreach (var subfeature in ((BlueprintFeatureSelection)(raceFeature)).Features)
                            {
                                //  Main.logger.Log("Removed SubFeature:" + subfeature.ToString());
                                newUnit.RemoveFact(subfeature);
                            }
                        }

                        newUnit.RemoveFact(raceFeature);
                    }
                }

                if (newUnit.Progression.CharacterLevel < 1)
                {
                    newUnit.Progression.DropLevelPlans(false);
                    newUnit.Progression.DropLevelPlans(true);
                    //newUnit.Progression.DropLevelPlans(true);
                }

                UnitEntityData[] petsToRemove = (from i in unit.Pets
                    select i.Entity).NotNull<UnitEntityData>().ToArray<UnitEntityData>();
                EventBus.RaiseEvent<ILevelUpInitiateUIHandler>(delegate(ILevelUpInitiateUIHandler h)
                {
                    //LevelUpConfig()
                    var Lvlupconfig = LevelUpConfig.Create(newUnit, LevelUpState.CharBuildMode.Respec);
                    Lvlupconfig.SetLevelupAfterRespec(false);
                    Lvlupconfig =
                        Lvlupconfig.SetOnCommit(delegate
                        {
                            RespecOnCommit2(unit, newUnit, petsToRemove, successCallback, scrollstoadd);
                        }).SetOnStop(delegate
                        {
                            Main.IsRespec = false;
                            Main.featureSelection = null;
                            Main.featurestoadd.Clear();
                            Main.IsRespec = false;
                            Main.partstoadd.Clear();
                            RespecOnStop(newUnit, toaddifcanceled);
                        });
                    Main.IsRespec = true;
                    h.HandleLevelUpStart(Lvlupconfig);
                    /*h.HandleLevelUpStart(newUnit.Descriptor, delegate
                    {
                        RespecOnCommit(unit, newUnit, petsToRemove, successCallback);
                    }, delegate
                    {
                        newUnit.Dispose();
                        Main.IsRespec = false;
                        Main.featureSelection = null;
                        Main.featurestoadd.Clear();
                        Main.IsRespec = false;
                        Main.partstoadd.Clear();
                    }, LevelUpState.CharBuildMode.Respec);*/
                });
                
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }

        /*private static void RespecOnCommit(UnitEntityData targetUnit, UnitEntityData tempUnit, UnitEntityData[] petsToRemove, Action successCallback)
		{
			//Main.logger.Log("oncommit");
			var bab = targetUnit.Progression.Classes.First().BaseAttackBonus.GetBonus(1);
			targetUnit.Stats.BaseAttackBonus.PermanentValue = bab;
			targetUnit.Stats.BaseAttackBonus.BaseValue = bab;
			targetUnit.Stats.BaseAttackBonus.m_BaseValue = bab;
			targetUnit.Stats.BaseAttackBonus.OnPermanentValueUpdated();
			Vector3 position = targetUnit.Position;
			float orientation = targetUnit.Orientation;
			Transform parent = targetUnit.View.transform.parent;
			List<IUnitPartSurviveRespec> list = targetUnit.Parts.GetAll<IUnitPartSurviveRespec>((IUnitPartSurviveRespec p) => p.ShouldSurviveRespec).ToTempList<IUnitPartSurviveRespec>();
			ClassData[] collection = (from _class in targetUnit.Progression.Classes
									  where _class.CharacterClass.IsMythic
									  select new ClassData(_class.CharacterClass)).ToArray<ClassData>();
			UnitPartRider riderPart = targetUnit.RiderPart;
			if (riderPart != null)
			{
				riderPart.DismountForce();
			}
			foreach (UnitEntityData unitEntityData in petsToRemove)
			{
				unitEntityData.RemoveMaster();
				unitEntityData.MarkForDestroy();
			}
			List<ItemEntity> list2 = new List<ItemEntity>();
			using (ContextData<Kingmaker.Items.Slots.ItemSlot.IgnoreLock>.Request())
			{
				foreach (Kingmaker.Items.Slots.ItemSlot itemSlot in targetUnit.Body.EquipmentSlots)
				{
					list2.Add(itemSlot.MaybeItem);
					itemSlot.RemoveItem(true);
				}
			}
			bool isTurnedOn = targetUnit.IsTurnedOn;
			if (isTurnedOn)
			{
				targetUnit.TurnOff();
			}
			RestController.RemoveNegativeEffects(targetUnit);
			targetUnit.Descriptor.ResurrectAndFullRestore();
			IGrouping<BlueprintBuff, TimeSpan?>[] array = (from i in targetUnit.Buffs.Enumerable.Where(delegate (Buff i)
			{
				MechanicsContext maybeContext = i.MaybeContext;
				return ((maybeContext != null) ? maybeContext.ParentContext : null) == null && !i.Blueprint.IsClassFeature;
			})
														   where string.IsNullOrEmpty(i.SourceAreaEffectId)
														   select i).NotNull<Buff>().GroupBy((Buff i) => i.Blueprint, delegate (Buff i)
														   {
															   if (!i.IsPermanent)
															   {
																   return new TimeSpan?(i.TimeLeft);
															   }
															   return null;
														   }).ToArray<IGrouping<BlueprintBuff, TimeSpan?>>();
			UnitEntityView view = targetUnit.View;
			targetUnit.DetachView();
			UnitEntityView unitEntityView = view.Or(null);
			if (unitEntityView != null)
			{
				unitEntityView.DestroyViewObject();
			}
			foreach (Kingmaker.Items.Slots.ItemSlot itemSlot2 in tempUnit.Body.EquipmentSlots)
			{
				itemSlot2.RemoveItem(true);
			}
			tempUnit.TurnOff();
			tempUnit.PreSave();
			PortraitData customPortraitRaw = tempUnit.UISettings.CustomPortraitRaw;
			BlueprintPortrait portraitBlueprintRaw = tempUnit.UISettings.PortraitBlueprintRaw;
			tempUnit.PrepareRespec();
			JObject jobject = JObject.FromObject(tempUnit);
			jobject.Remove("UniqueId");
			jobject.Remove("m_AutoUseAbility");
			JObject jobject2 = (JObject)jobject["Descriptor"];
			jobject2.Remove("m_Inventory");
			jobject2.Remove("Body");
			jobject2.Remove("UISettings");
			string value = jobject.ToString().Replace(tempUnit.UniqueId, targetUnit.UniqueId);
			try
			{
				IContractResolver contractResolver = DefaultJsonSettings.DefaultSettings.ContractResolver;
				DefaultJsonSettings.DefaultSettings.ContractResolver = new CollectionClearingContractResolver();
				JsonConvert.PopulateObject(value, targetUnit);
				DefaultJsonSettings.DefaultSettings.ContractResolver = contractResolver;
				targetUnit.Resources.PersistantResources = tempUnit.Resources.PersistantResources;
			}
			catch (Exception ex)
			{
				UnitHelper.Channel.Exception(ex, null, Array.Empty<object>());
				throw;
			}
			targetUnit.Parts.ClearCache();
			foreach (IUnitPartSurviveRespec unitPartSurviveRespec in list)
			{
				unitPartSurviveRespec.CopyAfterRespec(targetUnit);
			}
			using (ContextData<EntityDataBase.ForcePostLoad>.Request())
			{
				using (ContextData<UnitEntityData.Respec>.Request())
				{
					targetUnit.PostLoad();
				}
			}
			targetUnit.Progression.Classes.AddRange(collection);
			targetUnit.Descriptor.FixInventoryOnPlayerPostLoad();
			targetUnit.Position = position;
			targetUnit.Orientation = orientation;
			targetUnit.AttachToViewOnLoad(null);
			targetUnit.View.transform.SetParent(parent, true);
			if (isTurnedOn)
			{
				targetUnit.TurnOn();
			}
			targetUnit.Alignment.UpdateValue();
			targetUnit.UISettings.SetPortraitUnsafe(portraitBlueprintRaw, customPortraitRaw);
			foreach (IGrouping<BlueprintBuff, TimeSpan?> grouping in array)
			{
				foreach (TimeSpan? timeSpan in grouping)
				{
					targetUnit.AddBuff(grouping.Key, null, timeSpan);
					UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: restore buff {0} (duration: {1})", grouping.Key, timeSpan), Array.Empty<object>());
				}
			}
			using (List<EntityFact>.Enumerator enumerator4 = targetUnit.Facts.List.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					enumerator4.Current.CallComponents<IUpdatePet>(delegate (IUpdatePet c)
					{
						c.TryUpdatePet();
					});
				}
			}
			using (ContextData<Kingmaker.Items.Slots.ItemSlot.IgnoreLock>.Request())
			{
				for (int j = 0; j < list2.Count; j++)
				{
					ItemEntity itemEntity = list2[j];
					Kingmaker.Items.Slots.ItemSlot itemSlot3 = targetUnit.Body.EquipmentSlots[j];
					if (itemEntity != null && itemSlot3.CanInsertItem(itemEntity))
					{
						itemSlot3.InsertItem(itemEntity);
					}
				}
			}
			targetUnit.Stats.CleanupModifiers();
			foreach (ModifiableValue modifiableValue in targetUnit.Stats.AllStats)
			{
				modifiableValue.UpdateValue();
			}
			targetUnit.UISettings.CleanupSlots();
			targetUnit.UISettings.TryToInitialize();
			try
			{
				if (successCallback != null)
				{
					successCallback();
				}
			}
			catch (Exception ex2)
			{
				UnitHelper.Channel.Exception(ex2, null, Array.Empty<object>());
			}
			EventBus.RaiseEvent<IUnitChangedAfterRespecHandler>(delegate (IUnitChangedAfterRespecHandler h)
			{
				h.HandleUnitChangedAfterRespec(targetUnit);
			}, true);
			///patch things
			if (Main.IsRespec == true)
			{
				try
				{
					targetUnit.Descriptor.Stats.HitPoints.BaseValue = targetUnit.Descriptor.Stats.HitPoints.BaseValue + -1;
					Main.featurestoadd.Clear();
					Main.IsRespec = false;
					//Main.logger.Log("Commited");
					targetUnit.Progression.AdvanceMythicExperience(Main.MythicXP);
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(targetUnit);
							part.TurnOn();
							targetUnit.Parts.m_Parts.Add(part);
							part.OnPostLoad();
							part.PostLoad();
							///part.PostLoad(targetUnit);
						}
					}
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(tempUnit);
							tempUnit.Parts.m_Parts.Add(part);
						}
					}
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.m_Parts.Contains(part))
						{
							part.AttachToEntity(targetUnit);
							targetUnit.Parts.m_Parts.Add(part);
						}
					}
					Main.partstoadd.Clear();
					Main.EntityUnit = null;
					foreach (EntityPart entityPart in targetUnit.Parts.m_Parts)
					{
						targetUnit.OnPartAddedOrPostLoad(entityPart);
					}
					if (Main.NenioEtudeBool == true)
					{
						var KitsuneHeritageClassic = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
						var KitsuneHeritageKeen = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
						var facts = new List<BlueprintUnitFact> { KitsuneHeritageClassic, KitsuneHeritageKeen };
						foreach (IHiddenUnitFacts i in targetUnit.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
						{
							foreach (BlueprintUnitFact fact in facts)
							{
								i.Facts.Add(fact);
							}
						}
						Main.NenioEtudeBool = false;
					}
				}
				catch (Exception e) { Main.logger.Log(e.ToString()); }
			}
		}*/

        private static void RespecOnCommit2(UnitEntityData targetUnit, UnitEntityData tempUnit,
            UnitEntityData[] petsToRemove, Action successCallback, List<BlueprintItemEquipmentUsable> scrollstoadd)
        {
            try
            {
                PFLog.History.Party.Log(
                    string.Format("Respec unit: {0}, ", targetUnit) +
                    string.Format("level: {0} ({1}), ", targetUnit.Progression.CharacterLevel,
                        targetUnit.Progression.Experience) + string.Format("mythic: {0} ({1})",
                        targetUnit.Progression.MythicLevel, targetUnit.Progression.MythicExperience),
                    Array.Empty<object>());
                var bab = targetUnit.Progression.Classes.FirstOrDefault(c => !c.CharacterClass.IsMythic).BaseAttackBonus
                    .GetBonus(tempUnit.Progression.CharacterLevel);
                Main.logger.Log(
                    $"TmpUnit = {tempUnit.Progression.CharacterLevel} : TrgUnit = {targetUnit.Progression.CharacterLevel}");
                targetUnit.Stats.BaseAttackBonus.PermanentValue = bab;
                targetUnit.Stats.BaseAttackBonus.BaseValue = bab;
                targetUnit.Stats.BaseAttackBonus.m_BaseValue = bab;
                targetUnit.Stats.BaseAttackBonus.OnPermanentValueUpdated();

                //add scrolls
                foreach (var scroll in scrollstoadd)
                {
                    Game.Instance.Player.Inventory.Add(new ItemEntityUsable(scroll));
                }

                Vector3 position = targetUnit.Position;
                float orientation = targetUnit.Orientation;
                Transform parent = targetUnit.View.transform.parent;
                List<IUnitPartSurviveRespec> list = targetUnit.Parts
                    .GetAll<IUnitPartSurviveRespec>((IUnitPartSurviveRespec p) => p.ShouldSurviveRespec)
                    .ToTempList<IUnitPartSurviveRespec>();
                ClassData[] collection = (from _class in targetUnit.Progression.Classes
                    where _class.CharacterClass.IsMythic
                    select new ClassData(_class.CharacterClass)).ToArray<ClassData>();
                UnitPartRider riderPart = targetUnit.RiderPart;
                if (riderPart != null)
                {
                    riderPart.DismountForce();
                }

                foreach (UnitEntityData unitEntityData in petsToRemove)
                {
                    foreach (Kingmaker.Items.Slots.ItemSlot itemSlot in unitEntityData.Body.EquipmentSlots)
                    {
                        if (itemSlot.MaybeItem != null && itemSlot.CanRemoveItem())
                        {
                            itemSlot.RemoveItem(true);
                        }
                    }

                    unitEntityData.RemoveMaster();
                    unitEntityData.MarkForDestroy();
                }

                if (targetUnit.IsMainCharacter)
                {
                    tempUnit.Descriptor.AddEssentialMark();
                }

                List<ItemEntity> list2 = new List<ItemEntity>();
                using (ContextData<Kingmaker.Items.Slots.ItemSlot.IgnoreLock>.Request())
                {
                    foreach (Kingmaker.Items.Slots.ItemSlot itemSlot in targetUnit.Body.EquipmentSlots)
                    {
                        list2.Add(itemSlot.MaybeItem);
                        itemSlot.RemoveItem(true);
                    }
                }

                foreach (Ability ability in targetUnit.Abilities.RawFacts.ToTempList<Ability>())
                {
                    if (ability.Data.TemporarilyDisabled)
                    {
                        targetUnit.RemoveFact(ability);
                    }
                }
                foreach (ActivatableAbility activatableAbility in targetUnit.ActivatableAbilities.RawFacts.ToTempList<ActivatableAbility>())
                {
                    if (activatableAbility.TemporarilyDisabled)
                    {
                        targetUnit.RemoveFact(activatableAbility);
                    }
                }
                foreach (ActivatableAbility activatableAbility2 in targetUnit.ActivatableAbilities.RawFacts.ToTempList<ActivatableAbility>())
                {
                    activatableAbility2.TurnOffImmediately();
                }
                if (targetUnit.Body.IsPolymorphed)
                {
                    targetUnit.Body.CancelPolymorphEffect();
                }
                bool isTurnedOn = targetUnit.IsTurnedOn;
                if (isTurnedOn)
                {
                    targetUnit.TurnOff(EntityDataBase.Reason.Default);
                }

                targetUnit.Descriptor.RemoveNegativeEffects();
                targetUnit.Descriptor.ResurrectAndFullRestore();
                IGrouping<BlueprintBuff, TimeSpan?>[] array = (from i in targetUnit.Buffs.Enumerable.Where(
                        delegate(Buff i)
                        {
                            MechanicsContext maybeContext = i.MaybeContext;
                            return ((maybeContext != null) ? maybeContext.ParentContext : null) == null &&
                                   !i.Blueprint.IsClassFeature;
                        })
                    where string.IsNullOrEmpty(i.SourceAreaEffectId)
                    select i).NotNull<Buff>().GroupBy((Buff i) => i.Blueprint, delegate(Buff i)
                {
                    if (!i.IsPermanent)
                    {
                        return new TimeSpan?(i.TimeLeft);
                    }

                    return null;
                }).ToArray<IGrouping<BlueprintBuff, TimeSpan?>>();
                UnitEntityView view = targetUnit.View;
                targetUnit.DetachView();
                UnitEntityView unitEntityView = view.Or(null);
                if (unitEntityView != null)
                {
                    unitEntityView.DestroyViewObject();
                }

                foreach (Kingmaker.Items.Slots.ItemSlot itemSlot2 in tempUnit.Body.EquipmentSlots)
                {
                    itemSlot2.RemoveItem(true);
                }

                tempUnit.TurnOff();
                tempUnit.PreSave();
                PortraitData customPortraitRaw = tempUnit.UISettings.CustomPortraitRaw;
                BlueprintPortrait portraitBlueprintRaw = tempUnit.UISettings.PortraitBlueprintRaw;
                tempUnit.PrepareRespec();
                IContractResolver contractResolver = DefaultJsonSettings.DefaultSettings.ContractResolver;
                try
                {
                    DefaultJsonSettings.DefaultSettings.ContractResolver = new RespecContractResolver();
                    JsonSerializer jsonSerializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);
                    JObject jobject = JObject.FromObject(tempUnit, jsonSerializer);
                    jobject.Remove("UniqueId");
                    jobject.Remove("m_AutoUseAbility");
                    JObject jobject2 = (JObject)jobject["Descriptor"];
                    jobject2.Remove("m_Inventory");
                    jobject2.Remove("Body");
                    jobject2.Remove("UISettings");
                    JsonConvert.PopulateObject(jobject.ToString().Replace(tempUnit.UniqueId, targetUnit.UniqueId),
                        targetUnit, DefaultJsonSettings.DefaultSettings);
                    targetUnit.Resources.PersistantResources = tempUnit.Resources.PersistantResources;
                }
                catch (Exception ex)
                {
                    UnitHelper.Channel.Exception(ex, null, Array.Empty<object>());
                    throw;
                }
                finally
                {
                    DefaultJsonSettings.DefaultSettings.ContractResolver = contractResolver;
                }

                targetUnit.Parts.ClearCache();
                foreach (IUnitPartSurviveRespec unitPartSurviveRespec in list)
                {
                    unitPartSurviveRespec.CopyAfterRespec(targetUnit);
                }

                using (ContextData<EntityDataBase.ForcePostLoad>.Request())
                {
                    using (ContextData<UnitEntityData.Respec>.Request())
                    {
                        targetUnit.PostLoad();
                    }
                }

                targetUnit.Progression.Classes.AddRange(collection);
                targetUnit.Descriptor.FixInventoryOnPlayerPostLoad();
                targetUnit.Position = position;
                targetUnit.Orientation = orientation;
                targetUnit.AttachToViewOnLoad(null);
                targetUnit.View.transform.SetParent(parent, true);
                if (isTurnedOn)
                {
                    targetUnit.TurnOn();
                }

                targetUnit.Alignment.UpdateValue();
                targetUnit.UISettings.SetPortraitUnsafe(portraitBlueprintRaw, customPortraitRaw);
                foreach (IGrouping<BlueprintBuff, TimeSpan?> grouping in array)
                {
                    foreach (TimeSpan? timeSpan in grouping)
                    {
                        if (!targetUnit.Buffs.HasFact(grouping.Key))
                        {
                            targetUnit.AddBuff(grouping.Key, null, timeSpan);
                            UnitHelper.Channel.Log(
                                string.Format("UnitHelper.Respec: restore buff {0} (duration: {1})", grouping.Key,
                                    timeSpan), Array.Empty<object>());
                        }
                    }
                }

                /*   using (List<EntityFact>.Enumerator enumerator5 = targetUnit.Facts.List.GetEnumerator())
                   {
                       while (enumerator5.MoveNext())
                       {
                           enumerator5.Current.CallComponents<IUpdatePet>(delegate (IUpdatePet c)
                           {
                               if (c != null)
                               {
                           // c.TryUpdatePet();
                               }
                           });
                       }
                   }*/
                using (ContextData<Kingmaker.Items.Slots.ItemSlot.IgnoreLock>.Request())
                {
                    for (int j = 0; j < list2.Count; j++)
                    {
                        ItemEntity itemEntity = list2[j];
                        Kingmaker.Items.Slots.ItemSlot itemSlot3 = targetUnit.Body.EquipmentSlots[j];
                        if (itemEntity != null && itemSlot3.CanInsertItem(itemEntity))
                        {
                            itemSlot3.InsertItem(itemEntity);
                        }
                    }
                }

                targetUnit.Stats.CleanupModifiers();
                foreach (ModifiableValue modifiableValue in targetUnit.Stats.AllStats)
                {
                    modifiableValue.UpdateValue();
                }

                targetUnit.UISettings.CleanupSlots();
                targetUnit.UISettings.TryToInitialize();
                try
                {
                    if (successCallback != null)
                    {
                        successCallback();
                    }
                }
                catch (Exception ex2)
                {
                    UnitHelper.Channel.Exception(ex2, null, Array.Empty<object>());
                }

                EventBus.RaiseEvent<IUnitChangedAfterRespecHandler>(
                    delegate(IUnitChangedAfterRespecHandler h) { h.HandleUnitChangedAfterRespec(targetUnit); }, true);
                //if (Main.IsRespec == true)
                {
                    try
                    {
                        targetUnit.View.UpdateBodyEquipmentModel();
                        targetUnit.View.UpdateClassEquipment();
                        targetUnit.Descriptor.Stats.HitPoints.BaseValue =
                            targetUnit.Descriptor.Stats.HitPoints.BaseValue + -1;
                        Main.featurestoadd.Clear();
                        //Main.logger.Log("Commited");
                        targetUnit.Progression.AdvanceMythicExperience(Main.MythicXP);
                        foreach (EntityPart part in Main.partstoadd)
                        {
                            if (!targetUnit.Parts.Parts.Contains(part))
                            {
                                //Main.logger.Log(part.ToString());
                                part.AttachToEntity(targetUnit);
                                part.TurnOn();
                                targetUnit.Parts.m_Parts.Add(part);
                                part.OnPostLoad();
                                part.PostLoad();
                                ///part.PostLoad(targetUnit);
                            }
                        }

                        foreach (EntityPart part in Main.partstoadd)
                        {
                            if (!targetUnit.Parts.Parts.Contains(part))
                            {
                                // Main.logger.Log(part.ToString());
                                part.AttachToEntity(tempUnit);
                                tempUnit.Parts.m_Parts.Add(part);
                            }
                        }

                        /*foreach (EntityPart part in Main.partstoadd)
                        {
                            if (!targetUnit.Parts.m_Parts.Contains(part))
                            {
                                part.AttachToEntity(targetUnit);
                                targetUnit.Parts.m_Parts.Add(part);
                            }
                        }*/
                        Main.partstoadd.Clear();

                        //Main.EntityUnit = null;
                        foreach (EntityPart entityPart in targetUnit.Parts.m_Parts)
                        {
                            //Main.logger.Log(entityPart.ToString());
                            targetUnit.OnPartAddedOrPostLoad(entityPart);
                        }

                        if (Main.NenioEtudeBool == true)
                        {
                            var KitsuneHeritageClassic =
                                ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
                            var KitsuneHeritageKeen =
                                ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
                            var facts = new List<BlueprintUnitFact> { KitsuneHeritageClassic, KitsuneHeritageKeen };
                            foreach (IHiddenUnitFacts i in targetUnit.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
                            {
                                foreach (BlueprintUnitFact fact in facts)
                                {
                                    i.Facts.Add(fact);
                                }
                            }

                            Main.NenioEtudeBool = false;
                        }

                        Main.IsRespec = false;
                    }
                    catch (Exception e)
                    {
                        Main.logger.Log(e.ToString());
                    }
                    /*var bab2 = targetUnit.Progression.Classes.First().BaseAttackBonus.GetBonus();
                    targetUnit.Stats.BaseAttackBonus.PermanentValue = bab2;
                    targetUnit.Stats.BaseAttackBonus.BaseValue = bab2;
                    targetUnit.Stats.BaseAttackBonus.m_BaseValue = bab2;
                    targetUnit.Stats.BaseAttackBonus.OnPermanentValueUpdated();*/
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
                throw;
            }
        }

        private static void RespecOnStop(UnitEntityData targetUnit, BlueprintFeature toadd)
        {
            //	Main.logger.Log("Stopped");
            if (Main.NenioEtudeBool == true)
            {
                var KitsuneHeritageClassic =
                    ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
                var KitsuneHeritageKeen =
                    ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
                var facts = new List<BlueprintUnitFact> { KitsuneHeritageClassic, KitsuneHeritageKeen };
                foreach (IHiddenUnitFacts i in targetUnit.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
                {
                    foreach (BlueprintUnitFact fact in facts)
                    {
                        i.Facts.Add(fact);
                    }
                }

                Main.NenioEtudeBool = false;
            }

            if (toadd != null) targetUnit.AddFact(toadd);
            targetUnit.Dispose();
            Main.IsRespec = false;
            Main.featureSelection = null;
            Main.featurestoadd.Clear();
            Main.partstoadd.Clear();
        }
    }
}