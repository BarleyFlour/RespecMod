using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.JsonUtility.CollectionConverters;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
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
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utils;
using UnityEngine;

namespace RespecModBarley
{
	static class RespecClass
	{
		public static void Respecialize(this UnitEntityData unit, Action successCallback = null)
		{
			UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: requested for {0}", unit), Array.Empty<object>());
			int experience = unit.Descriptor.Progression.Experience;
			ReplaceUnitBlueprintForRespec replaceUnitBlueprintForRespec = unit.Blueprint.GetComponent<ReplaceUnitBlueprintForRespec>().Or(null);
			BlueprintUnit unit2 = ((replaceUnitBlueprintForRespec != null) ? replaceUnitBlueprintForRespec.Blueprint.Or(null) : null) ?? unit.Blueprint;
			UnitEntityData newUnit = Game.Instance.CreateUnitVacuum(unit2);
			newUnit.Progression.AdvanceExperienceTo(experience, false, false);
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
				CharGenDollRoom component = dollRoom.GetComponent<CharGenDollRoom>();
				if (component)
				{
					component.CreateDolls();
				}
				dollRoom.SetUnit(newUnit);
			}
			BlueprintFeature[] array = (from i in unit.Progression.Features.Enumerable.Except((Feature i) => i.SourceProgression || i.SourceClass || i.SourceRace || i.Blueprint.IsClassFeature)
										select i.Blueprint).ToArray<BlueprintFeature>();
			Feature[] array2 = (from i in newUnit.Progression.Features.Enumerable
								where !i.Blueprint.IsClassFeature && !unit.HasFact(i.Blueprint)
								select i).ToArray<Feature>();
			foreach (BlueprintFeature blueprintFeature in array)
			{
				if (!newUnit.HasFact(blueprintFeature))
				{
					newUnit.AddFact(blueprintFeature, null, null);
					UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: restore feature {0}", blueprintFeature), Array.Empty<object>());
				}
			}
			foreach (Feature feature in array2)
			{
				newUnit.RemoveFact(feature);
				UnitHelper.Channel.Log(string.Format("UnitHelper.Respec: remove feature {0}", feature), Array.Empty<object>());
			}
			if (newUnit.Progression.CharacterLevel < 1)
			{
				newUnit.Progression.DropLevelPlansAll();
			}
			UnitEntityData[] petsToRemove = (from i in unit.Pets
											 select i.Entity).NotNull<UnitEntityData>().ToArray<UnitEntityData>();
			EventBus.RaiseEvent<ILevelUpInitiateUIHandler>(delegate (ILevelUpInitiateUIHandler h)
			{
				h.HandleLevelUpStart(newUnit.Descriptor, delegate
				{
					RespecOnCommit(unit, newUnit,petsToRemove ,successCallback);
				}, delegate
				{
					newUnit.Dispose();
				}, LevelUpState.CharBuildMode.Respec);
			});
		}
		private static void RespecOnCommit(UnitEntityData targetUnit, UnitEntityData tempUnit, UnitEntityData[] petsToRemove, Action successCallback)
		{
			Vector3 position = targetUnit.Position;
			float orientation = targetUnit.Orientation;
			Transform parent = targetUnit.View.transform.parent;
			List<IUnitPartSurviveRespec> list = targetUnit.Parts.GetAll<IUnitPartSurviveRespec>((IUnitPartSurviveRespec p) => p.ShouldSurvive).ToTempList<IUnitPartSurviveRespec>();
			foreach (UnitEntityData unitEntityData in petsToRemove)
			{
				unitEntityData.RemoveMaster();
				unitEntityData.MarkForDestroy();
			}
			foreach (Kingmaker.Items.Slots.ItemSlot itemSlot in tempUnit.Body.EquipmentSlots)
			{
				itemSlot.RemoveItem(true);
			}
			bool isTurnedOn = targetUnit.IsTurnedOn;
			if (isTurnedOn)
			{
				targetUnit.TurnOff();
			}
			RestController.RemoveNegativeEffects(targetUnit);
			targetUnit.Descriptor.ResurrectAndFullRestore();
			IGrouping<BlueprintBuff, TimeSpan?>[] array = (from i in targetUnit.Buffs.Enumerable.Where(delegate(Buff i)
			{
				MechanicsContext maybeContext = i.MaybeContext;
				return ((maybeContext != null) ? maybeContext.ParentContext : null) == null && !i.Blueprint.IsClassFeature;
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
			foreach (Kingmaker.Items.Slots.ItemSlot itemSlot2 in targetUnit.Body.EquipmentSlots)
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
			using (ContextData<EntityDataBase.ForcePostLoad>.Request())
			{
				using (ContextData<UnitEntityData.Respec>.Request())
				{
					targetUnit.PostLoad();
				}
			}
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
			using (List<EntityFact>.Enumerator enumerator3 = targetUnit.Facts.List.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					enumerator3.Current.CallComponents<IUpdatePet>(delegate(IUpdatePet c)
					{
						c.TryUpdatePet();
					});
				}
			}
			foreach (IUnitPartSurviveRespec unitPartSurviveRespec in list)
			{
				unitPartSurviveRespec.CopyAfterRespec(targetUnit);
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
			EventBus.RaiseEvent<IUnitChangedAfterRespecHandler>(delegate(IUnitChangedAfterRespecHandler h)
			{
				h.HandleUnitChangedAfterRespec(targetUnit);
			}, true);
			///patch things
			if (Main.IsRespec == true)
			{
				try
				{
					Main.featurestoadd.Clear();
					Main.IsRespec = false;
					targetUnit.Progression.AdvanceMythicExperience(Main.MythicXP);
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(targetUnit);
							///part.TurnOn();
							targetUnit.Parts.m_Parts.Add(part);
						}
					}
					/*foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(tempUnit);
							tempUnit.Parts.m_Parts.Add(part);
						}
					}*/
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.m_Parts.Contains(part))
						{
							///part.AttachToEntity(unit);
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
					    /*foreach (UnitEntityData data in targetUnit.Pets)
						{
							///Main.PetBool = true;
							/*data.Progression.Classes.Clear();
							data.Progression.CharacterLevel = 0;
							/*data.Progression.Classes.Clear();
							Main.logger.Log(data.CharacterName.ToString());
							data.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
							data.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = 0;
							EventBus.RaiseEvent<ILevelUpInitiateUIHandler>(delegate (ILevelUpInitiateUIHandler h)
							{
								h.HandleLevelUpStart(data.Descriptor, null, null, LevelUpState.CharBuildMode.CharGen);
							}, true);*//*
						}*/
				}
				catch (Exception e) { Main.logger.Log(e.ToString()); }
			}
		}
	}
}
