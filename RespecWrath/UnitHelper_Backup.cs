using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
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

namespace Kingmaker.UnitLogic
{
	// Token: 0x020012A1 RID: 4769
	public static class UnitHelper
	{
		// Token: 0x0600801D RID: 32797 RVA: 0x001FEB14 File Offset: 0x001FCD14
		public static UnitEntityData Copy(this UnitEntityData unit, bool createView, bool preview)
		{
			UnitEntityData result;
			try
			{
				using (ProfileScope.New("Copy Unit", null))
				{
					result = UnitHelper.CopyInternal(unit, createView, preview);
				}
			}
			catch (Exception exception)
			{
				LogChannel.Default.ExceptionWithReport(exception, null, Array.Empty<object>());
				result = null;
			}
			return result;
		}

		// Token: 0x0600801E RID: 32798 RVA: 0x001FEB78 File Offset: 0x001FCD78
		private static UnitEntityData CopyInternal(UnitEntityData unit, bool createView, bool preview)
		{
			UnitEntityData unitEntityData;
			using (ContextData<UnitEntityData.DoNotCreateItems>.Request())
			{
				using (ContextData<AddClassLevels.DoNotCreatePlan>.RequestIf(preview))
				{
					unitEntityData = Game.Instance.CreateUnitVacuum(unit.OriginalBlueprint);
				}
			}
			unitEntityData.CopyOf = unit;
			unitEntityData.TurnOff();
			unitEntityData.Alignment.Initialize(unit.Alignment.Value);
			unitEntityData.Descriptor.UISettings.SetPortrait(unit.Portrait);
			unitEntityData.Descriptor.Doll = unit.Descriptor.Doll;
			unitEntityData.Descriptor.EnsureOwnInventory();
			if (preview)
			{
				unitEntityData.Facts.EnsureFactProcessor<BuffCollection>().SetupPreview(unitEntityData.Descriptor);
			}
			unitEntityData.Descriptor.Progression.CopyFrom(unit.Descriptor.Progression);
			UnitHelper.CopyStats(unit, unitEntityData);
			UnitHelper.CopyFacts(unit, unitEntityData);
			UnitHelper.CopySpellbook(unit, unitEntityData);
			UnitHelper.CopyProficiencies(unit, unitEntityData);
			UnitHelper.CopyItems(unit, unitEntityData);
			if (createView)
			{
				UnitEntityView view = unitEntityData.CreateView();
				unitEntityData.AttachView(view);
			}
			unitEntityData.TurnOn();
			return unitEntityData;
		}

		// Token: 0x0600801F RID: 32799 RVA: 0x001FECA4 File Offset: 0x001FCEA4
		private static void CopyFacts(UnitEntityData original, UnitEntityData target)
		{
			foreach (EntityFact entityFact in original.Facts.List)
			{
				MechanicsContext maybeContext = entityFact.MaybeContext;
				if (!(((maybeContext != null) ? maybeContext.Root.AssociatedBlueprint : null) is BlueprintItemEnchantment))
				{
					UnitFact unitFact = entityFact as UnitFact;
					if (entityFact.SourceItem == null)
					{
						Feature feature = unitFact as Feature;
						Feature feature2 = (feature != null) ? target.GetFeature(feature.Blueprint, feature.Param) : null;
						if ((feature != null && (feature2 == null || feature2.GetRank() < feature.GetRank())) || !target.HasFact(entityFact.Blueprint))
						{
							MechanicsContext maybeContext2 = entityFact.MaybeContext;
							MechanicsContext mechanicsContext = (maybeContext2 != null) ? maybeContext2.ParentContext : null;
							MechanicsContext mechanicsContext2 = (mechanicsContext != null) ? mechanicsContext.CloneFor(mechanicsContext.AssociatedBlueprint, target.Descriptor) : null;
							if (mechanicsContext2 != null)
							{
								mechanicsContext2.UnlinkParent();
							}
							EntityFact entityFact2 = (unitFact != null) ? unitFact.Blueprint.CreateFact(mechanicsContext2, target.Descriptor, null) : new EntityFact(entityFact.Blueprint);
							Feature feature3;
							if ((feature3 = (entityFact2 as Feature)) != null && feature != null)
							{
								feature3.SetSameSource(feature);
								feature3.Param = feature.Param;
							}
							target.Facts.Add<EntityFact>(entityFact2);
						}
					}
				}
			}
		}

		// Token: 0x06008020 RID: 32800 RVA: 0x001FEE20 File Offset: 0x001FD020
		private static void CopySpellbook(UnitEntityData original, UnitEntityData target)
		{
			foreach (Spellbook spellbook in original.Descriptor.Spellbooks)
			{
				Spellbook spellbook2 = target.Descriptor.DemandSpellbook(spellbook.Blueprint);
				foreach (BlueprintSpellList blueprintSpellList in spellbook.SpecialLists)
				{
					if (!spellbook2.SpecialLists.HasItem(blueprintSpellList))
					{
						spellbook2.AddSpecialList(blueprintSpellList);
					}
				}
				if (!spellbook.IsStandaloneMythic)
				{
					while (spellbook2.RawBaseLevel < spellbook.RawBaseLevel)
					{
						spellbook2.AddBaseLevel();
					}
				}
				if (spellbook.IsMythic)
				{
					while (spellbook2.MythicLevel < spellbook.MythicLevel)
					{
						spellbook2.AddMythicLevel();
					}
				}
				spellbook2.OppositionSchools.AddRange(spellbook.OppositionSchools);
				spellbook2.OppositionDescriptors = spellbook.OppositionDescriptors;
				foreach (AbilityData abilityData in spellbook.GetAllKnownSpells())
				{
					spellbook2.AddKnown(abilityData.SpellLevel, abilityData.Blueprint, true);
				}
			}
		}

		// Token: 0x06008021 RID: 32801 RVA: 0x001FEFA4 File Offset: 0x001FD1A4
		private static void CopyProficiencies(UnitEntityData original, UnitEntityData target)
		{
			foreach (ArmorProficiencyGroup proficiency in original.Descriptor.Proficiencies.ArmorProficiencies)
			{
				if (!target.Descriptor.Proficiencies.Contains(proficiency))
				{
					target.Descriptor.Proficiencies.Add(proficiency);
				}
			}
			foreach (WeaponCategory proficiency2 in original.Descriptor.Proficiencies.WeaponProficiencies)
			{
				if (!target.Descriptor.Proficiencies.Contains(proficiency2))
				{
					target.Descriptor.Proficiencies.Add(proficiency2);
				}
			}
		}

		// Token: 0x06008022 RID: 32802 RVA: 0x001FF07C File Offset: 0x001FD27C
		private static void CopyItems(UnitEntityData original, UnitEntityData target)
		{
			List<Kingmaker.Items.Slots.ItemSlot> list = original.Body.EquipmentSlots.ToTempList<Kingmaker.Items.Slots.ItemSlot>();
			List<Kingmaker.Items.Slots.ItemSlot> list2 = target.Body.EquipmentSlots.ToTempList<Kingmaker.Items.Slots.ItemSlot>();
			for (int i = 0; i < list.Count; i++)
			{
				Kingmaker.Items.Slots.ItemSlot itemSlot = list[i];
				if (itemSlot.MaybeItem != null)
				{
					list2[i].InsertItem(itemSlot.MaybeItem.Blueprint.CreateEntity());
				}
			}
		}

		// Token: 0x06008023 RID: 32803 RVA: 0x001FF0E8 File Offset: 0x001FD2E8
		private static void CopyStats(UnitEntityData original, UnitEntityData target)
		{
			foreach (ModifiableValue modifiableValue in original.Stats.AllStats)
			{
				ModifiableValue stat = target.Stats.GetStat(modifiableValue.Type);
				stat.BaseValue = modifiableValue.BaseValue;
				foreach (ModifiableValue.Modifier modifier in modifiableValue.Modifiers)
				{
					if (modifier.ModDescriptor == ModifierDescriptor.Racial)
					{
						EntityFact source = modifier.Source;
						BlueprintRace blueprintRace;
						if ((blueprintRace = (((source != null) ? source.Blueprint : null) as BlueprintRace)) != null && blueprintRace.SelectableRaceStat)
						{
							stat.AddModifier(modifier.ModValue, modifier.ModDescriptor);
						}
					}
				}
			}
		}

		// Token: 0x06008024 RID: 32804 RVA: 0x001FF1D8 File Offset: 0x001FD3D8
		public static void Respec(this UnitEntityData unit, Action successCallback = null)
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
			foreach (ModifiableValueAttributeStat modifiableValueAttributeStat in newUnit.Stats.Attributes)
			{
				modifiableValueAttributeStat.BaseValue = 10;
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
			EventBus.RaiseEvent<ILevelUpInitiateUIHandler>(delegate (ILevelUpInitiateUIHandler h)
			{
				h.HandleLevelUpStart(newUnit.Descriptor, new Action(base.< Respec > g__OnCommit | 0), new Action(base.< Respec > g__OnStop | 1), LevelUpState.CharBuildMode.Respec);
			});
		}

		// Token: 0x06008025 RID: 32805 RVA: 0x001FF4B0 File Offset: 0x001FD6B0
		private static void RespecOnCommit(UnitEntityData targetUnit, UnitEntityData tempUnit, Action successCallback)
		{
			Vector3 position = targetUnit.Position;
			float orientation = targetUnit.Orientation;
			UnitEntityData[] source = (from i in targetUnit.Pets
									   select i.Entity).NotNull<UnitEntityData>().ToArray<UnitEntityData>();
			UnitPartDualCompanion unitPartDualCompanion = targetUnit.Get<UnitPartDualCompanion>();
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
					enumerator3.Current.CallComponents<IUpdatePet>(delegate (IUpdatePet c)
					{
						c.TryUpdatePet();
					});
				}
			}
			foreach (EntityPartRef<UnitEntityData, UnitPartPet> entityPartRef in targetUnit.Pets)
			{
				if (!source.HasItem(entityPartRef.Entity))
				{
					UnitEntityData entity = entityPartRef.Entity;
					if (entity != null)
					{
						entity.MarkForDestroy();
					}
				}
			}
			if (unitPartDualCompanion != null)
			{
				UnitPartDualCompanion unitPartDualCompanion2 = targetUnit.Ensure<UnitPartDualCompanion>();
				unitPartDualCompanion2.IsActive = unitPartDualCompanion.IsActive;
				unitPartDualCompanion2.IsDead = unitPartDualCompanion.IsDead;
				unitPartDualCompanion2.PairCompanion = unitPartDualCompanion.PairCompanion;
			}
			targetUnit.Stats.CleanupModifiers();
			foreach (ModifiableValue modifiableValue in targetUnit.Stats.AllStats)
			{
				modifiableValue.UpdateValue();
			}
			targetUnit.UISettings.CleanupSlots();
			targetUnit.UISettings.TryToInitialize(targetUnit);
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
			});
		}

		// Token: 0x06008026 RID: 32806 RVA: 0x001FFADC File Offset: 0x001FDCDC
		[Cheat(Name = "respec", Description = "Respec selected unit", ExecutionPolicy = ExecutionPolicy.PlayMode)]
		[UsedImplicitly]
		private static void CheatRespecUnit()
		{
			UnitEntityData singleSelectedUnit = Game.Instance.UI.SelectionManager.SingleSelectedUnit;
			if (singleSelectedUnit == null)
			{
				PFLog.Default.Error("CheatRespecUnit: failed, one (and only one) unit must be selected", Array.Empty<object>());
				return;
			}
			singleSelectedUnit.Respec(null);
		}

		// Token: 0x06008027 RID: 32807 RVA: 0x001FFB23 File Offset: 0x001FDD23
		public static TFact AddFact<TFact>(this UnitEntityData _this, BlueprintUnitFact blueprint, MechanicsContext parentContext = null, FeatureParam param = null) where TFact : UnitFact
		{
			return _this.Descriptor.AddFact(blueprint, parentContext, param);
		}

		// Token: 0x06008028 RID: 32808 RVA: 0x001FFB34 File Offset: 0x001FDD34
		public static TFact AddFact<TFact>(this UnitDescriptor _this, BlueprintUnitFact blueprint, MechanicsContext parentContext = null, FeatureParam param = null) where TFact : UnitFact
		{
			if (!blueprint)
			{
				return default(TFact);
			}
			try
			{
				using ((param != null) ? ContextData<Feature.ParamContextData>.Request().Setup(param.Value) : null)
				{
					TFact fact;
					if ((fact = (blueprint.CreateFact(parentContext, _this, null) as TFact)) != null)
					{
						return _this.Unit.Facts.Add<TFact>(fact);
					}
					PFLog.Default.Error("UnitHelper.AddFact: can't create fact of type {0} from blueprint {1}", new object[]
					{
						typeof(TFact).Name,
						blueprint.GetType().Name
					});
				}
			}
			finally
			{
			}
			return default(TFact);
		}

		// Token: 0x06008029 RID: 32809 RVA: 0x001FFC14 File Offset: 0x001FDE14
		public static EntityFact AddFact(this UnitEntityData _this, BlueprintUnitFact blueprint, MechanicsContext parentContext = null, FeatureParam param = null)
		{
			return _this.Descriptor.AddFact(blueprint, parentContext, param);
		}

		// Token: 0x0600802A RID: 32810 RVA: 0x001FFC24 File Offset: 0x001FDE24
		public static EntityFact AddFact(this UnitDescriptor _this, BlueprintUnitFact blueprint, MechanicsContext parentContext = null, FeatureParam param = null)
		{
			if (!blueprint)
			{
				return null;
			}
			BlueprintBuff blueprintBuff = blueprint as BlueprintBuff;
			if (!blueprintBuff)
			{
				return _this.AddFact(blueprint, parentContext, param);
			}
			if (parentContext == null)
			{
				return _this.AddBuff(blueprintBuff, _this.Unit, null, null);
			}
			return _this.AddBuff(blueprintBuff, parentContext, null);
		}

		// Token: 0x0600802B RID: 32811 RVA: 0x001FFC80 File Offset: 0x001FDE80
		public static Buff AddBuff(this UnitEntityData _this, BlueprintBuff blueprint, [NotNull] UnitEntityData caster, TimeSpan? duration = null, AbilityParams abilityParams = null)
		{
			return _this.Descriptor.AddBuff(blueprint, caster, duration, abilityParams);
		}

		// Token: 0x0600802C RID: 32812 RVA: 0x001FFC92 File Offset: 0x001FDE92
		public static Buff AddBuff(this UnitDescriptor _this, BlueprintBuff blueprint, [NotNull] UnitEntityData caster, TimeSpan? duration = null, AbilityParams abilityParams = null)
		{
			if (!blueprint)
			{
				return null;
			}
			return _this.Buffs.AddBuff(blueprint, caster, duration, abilityParams);
		}

		// Token: 0x0600802D RID: 32813 RVA: 0x001FFCAE File Offset: 0x001FDEAE
		public static Buff AddBuff(this UnitEntityData _this, BlueprintBuff blueprint, [CanBeNull] MechanicsContext parentContext, TimeSpan? duration = null)
		{
			return _this.Descriptor.AddBuff(blueprint, parentContext, duration);
		}

		// Token: 0x0600802E RID: 32814 RVA: 0x001FFCBE File Offset: 0x001FDEBE
		public static Buff AddBuff(this UnitDescriptor _this, BlueprintBuff blueprint, [CanBeNull] MechanicsContext parentContext, TimeSpan? duration = null)
		{
			if (!blueprint)
			{
				return null;
			}
			if (parentContext == null)
			{
				return _this.AddBuff(blueprint, _this.Unit, duration, null);
			}
			return _this.Buffs.AddBuff(blueprint, parentContext, duration);
		}

		// Token: 0x0600802F RID: 32815 RVA: 0x001FFCEB File Offset: 0x001FDEEB
		public static void RemoveFact(this UnitEntityData _this, BlueprintUnitFact blueprint)
		{
			_this.Descriptor.RemoveFact(blueprint);
		}

		// Token: 0x06008030 RID: 32816 RVA: 0x001FFCFC File Offset: 0x001FDEFC
		public static void RemoveFact(this UnitDescriptor _this, BlueprintUnitFact blueprint)
		{
			if (!blueprint)
			{
				return;
			}
			_this.Unit.Facts.Remove(_this.Unit.Facts.Get<EntityFact>((EntityFact i) => i.Blueprint == blueprint), true);
		}

		// Token: 0x06008031 RID: 32817 RVA: 0x001FFD54 File Offset: 0x001FDF54
		public static void RemoveFact(this UnitDescriptor _this, BlueprintFeature blueprint, FeatureParam param)
		{
			if (!blueprint)
			{
				return;
			}
			_this.Unit.Facts.Remove(_this.Unit.Facts.Get<Feature>((Feature i) => i.Blueprint == blueprint && i.Param == param), true);
		}

		// Token: 0x06008032 RID: 32818 RVA: 0x001FFDB0 File Offset: 0x001FDFB0
		public static void RemoveFact(this UnitEntityData _this, EntityFact fact)
		{
			_this.Descriptor.RemoveFact(fact);
		}

		// Token: 0x06008033 RID: 32819 RVA: 0x001FFDBE File Offset: 0x001FDFBE
		public static void RemoveFact(this UnitDescriptor _this, EntityFact fact)
		{
			if (fact == null)
			{
				return;
			}
			_this.Unit.Facts.Remove(fact, true);
		}

		// Token: 0x06008034 RID: 32820 RVA: 0x001FFDD6 File Offset: 0x001FDFD6
		public static bool HasFact(this UnitEntityData _this, BlueprintFact blueprint)
		{
			return _this.Descriptor.HasFact(blueprint);
		}

		// Token: 0x06008035 RID: 32821 RVA: 0x001FFDE4 File Offset: 0x001FDFE4
		public static bool HasFact(this UnitDescriptor _this, BlueprintFact blueprint)
		{
			return _this.Unit.Facts.Contains((EntityFact i) => i.Blueprint == blueprint);
		}

		// Token: 0x06008036 RID: 32822 RVA: 0x001FFE1A File Offset: 0x001FE01A
		public static bool HasFact(this UnitEntityData _this, EntityFact fact)
		{
			return _this.Descriptor.HasFact(fact);
		}

		// Token: 0x06008037 RID: 32823 RVA: 0x001FFE28 File Offset: 0x001FE028
		public static bool HasFact(this UnitDescriptor _this, EntityFact fact)
		{
			return fact != null && _this.Unit.Facts.Contains(fact);
		}

		// Token: 0x06008038 RID: 32824 RVA: 0x001FFE40 File Offset: 0x001FE040
		public static T GetFact<T>(this UnitEntityData _this, BlueprintUnitFact blueprint) where T : EntityFact
		{
			return _this.Descriptor.GetFact(blueprint) as T;
		}

		// Token: 0x06008039 RID: 32825 RVA: 0x001FFE58 File Offset: 0x001FE058
		public static EntityFact GetFact(this UnitEntityData _this, BlueprintUnitFact blueprint)
		{
			return _this.Descriptor.GetFact(blueprint);
		}

		// Token: 0x0600803A RID: 32826 RVA: 0x001FFE68 File Offset: 0x001FE068
		public static EntityFact GetFact(this UnitDescriptor _this, BlueprintUnitFact blueprint)
		{
			return _this.Unit.Facts.Get<EntityFact>((EntityFact i) => i.Blueprint == blueprint);
		}

		// Token: 0x0600803B RID: 32827 RVA: 0x001FFE9E File Offset: 0x001FE09E
		public static Feature GetFeature(this UnitEntityData _this, BlueprintFeature feature, FeatureParam param = null)
		{
			return _this.Descriptor.GetFeature(feature, param);
		}

		// Token: 0x0600803C RID: 32828 RVA: 0x001FFEB0 File Offset: 0x001FE0B0
		public static Feature GetFeature(this UnitDescriptor _this, BlueprintFeature feature, FeatureParam param = null)
		{
			return _this.Unit.Facts.Get<Feature>((Feature i) => i.Blueprint == feature && i.Param == param);
		}

		// Token: 0x0600803D RID: 32829 RVA: 0x001FFEF0 File Offset: 0x001FE0F0
		public static bool TryBreakFree(this UnitEntityData _this, [CanBeNull] UnitEntityData grappler, UnitHelper.BreakFreeFlags flags, [CanBeNull] MechanicsContext context, int? overrideDC = null)
		{
			if (grappler == null)
			{
				grappler = _this;
			}
			bool flag = flags.HasFlag(UnitHelper.BreakFreeFlags.CanUseCMB);
			bool flag2 = flags.HasFlag(UnitHelper.BreakFreeFlags.CMD2DC);
			int dc = 0;
			if (overrideDC != null)
			{
				dc = overrideDC.Value;
			}
			else if (flag2)
			{
				RuleCalculateCMD ruleCalculateCMD = new RuleCalculateCMD(_this, grappler, CombatManeuver.Grapple);
				ruleCalculateCMD = (((context != null) ? context.TriggerRule<RuleCalculateCMD>(ruleCalculateCMD) : null) ?? Rulebook.Trigger<RuleCalculateCMD>(ruleCalculateCMD));
				dc = ruleCalculateCMD.Result;
			}
			else if (context != null)
			{
				dc = context.Params.DC;
			}
			RuleCalculateCMB ruleCalculateCMB = new RuleCalculateCMB(_this, grappler, CombatManeuver.Grapple);
			ruleCalculateCMB = (((context != null) ? context.TriggerRule<RuleCalculateCMB>(ruleCalculateCMB) : null) ?? Rulebook.Trigger<RuleCalculateCMB>(ruleCalculateCMB));
			if (flag && Math.Max(_this.Stats.SkillMobility, _this.Stats.SkillAthletics) < ruleCalculateCMB.Result)
			{
				RuleCombatManeuver ruleCombatManeuver = new RuleCombatManeuver(_this, grappler, CombatManeuver.Grapple);
				ruleCombatManeuver = (((context != null) ? context.TriggerRule<RuleCombatManeuver>(ruleCombatManeuver) : null) ?? Rulebook.Trigger<RuleCombatManeuver>(ruleCombatManeuver));
				return ruleCombatManeuver.Success;
			}
			StatType statType = (_this.Stats.SkillMobility <= _this.Stats.SkillAthletics) ? StatType.SkillAthletics : StatType.SkillMobility;
			if (!flag)
			{
				statType = ((_this.Stats.Strength.Bonus > _this.Stats.GetStat(statType)) ? StatType.Strength : statType);
			}
			return GameHelper.TriggerSkillCheck(new RuleSkillCheck(_this, statType, dc), context, true).IsPassed;
		}

		// Token: 0x0600803E RID: 32830 RVA: 0x0020006E File Offset: 0x001FE26E
		public static bool IsCustomCompanion(this UnitEntityData _this)
		{
			return _this.Blueprint == BlueprintRoot.Instance.CustomCompanion;
		}

		// Token: 0x0600803F RID: 32831 RVA: 0x00200085 File Offset: 0x001FE285
		public static bool IsCustomCompanion(this UnitDescriptor _this)
		{
			return _this.OriginalBlueprint == BlueprintRoot.Instance.CustomCompanion;
		}

		// Token: 0x06008040 RID: 32832 RVA: 0x0020009C File Offset: 0x001FE29C
		public static bool IsInCompanionRoster(this UnitEntityData _this)
		{
			UnitPartCompanion unitPartCompanion = _this.Get<UnitPartCompanion>();
			CompanionState companionState = (unitPartCompanion != null) ? unitPartCompanion.State : CompanionState.None;
			return _this == Game.Instance.Player.MainCharacter || companionState == CompanionState.InParty || companionState == CompanionState.InPartyDetached || companionState == CompanionState.Remote;
		}

		// Token: 0x06008041 RID: 32833 RVA: 0x002000EB File Offset: 0x001FE2EB
		public static bool IsExCompanion(this UnitEntityData _this)
		{
			UnitPartCompanion unitPartCompanion = _this.Get<UnitPartCompanion>();
			return unitPartCompanion != null && unitPartCompanion.State == CompanionState.ExCompanion;
		}

		// Token: 0x06008042 RID: 32834 RVA: 0x00200104 File Offset: 0x001FE304
		public static bool CanBeKnockedOff(this UnitEntityData _this)
		{
			return !_this.Descriptor.State.Prone.Active && (!_this.View || !_this.View.IsGetUp) && !_this.Descriptor.State.HasConditionImmunity(UnitCondition.Prone);
		}

		// Token: 0x06008043 RID: 32835 RVA: 0x0020015C File Offset: 0x001FE35C
		public static int GetConcentrationBonus(this UnitEntityData _this)
		{
			int num = 0;
			using (List<EntityFact>.Enumerator enumerator = _this.Facts.List.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					UnitFact unitFact;
					if ((unitFact = (enumerator.Current as UnitFact)) != null)
					{
						foreach (EntityFactComponent entityFactComponent in unitFact.Components)
						{
							IConcentrationBonusProvider concentrationBonusProvider = entityFactComponent.SourceBlueprintComponent as IConcentrationBonusProvider;
							num += ((concentrationBonusProvider != null) ? concentrationBonusProvider.GetStaticConcentrationBonus(entityFactComponent) : 0);
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06008044 RID: 32836 RVA: 0x00200218 File Offset: 0x001FE418
		public static UnitEntityData CreatePreview(this UnitEntityData _this, bool createView)
		{
			return _this.Copy(createView, true);
		}

		// Token: 0x06008045 RID: 32837 RVA: 0x00200224 File Offset: 0x001FE424
		[CanBeNull]
		public static WeaponSlot GetThreatHandMelee(this UnitEntityData unit)
		{
			if (unit.IsThreatHandMelee(unit.Body.PrimaryHand))
			{
				return unit.Body.PrimaryHand;
			}
			ItemEntityWeapon maybeWeapon = unit.Body.PrimaryHand.MaybeWeapon;
			if ((maybeWeapon == null || !maybeWeapon.Blueprint.IsTwoHanded) && unit.IsThreatHandMelee(unit.Body.SecondaryHand))
			{
				return unit.Body.SecondaryHand;
			}
			foreach (WeaponSlot weaponSlot in unit.Body.AdditionalLimbs)
			{
				if (unit.IsThreatHandMelee(weaponSlot))
				{
					return weaponSlot;
				}
			}
			return null;
		}

		// Token: 0x06008046 RID: 32838 RVA: 0x002002E8 File Offset: 0x001FE4E8
		public static WeaponSlot GetThreatHandRanged(this UnitEntityData unit)
		{
			if (unit.IsThreatHandRanged(unit.Body.PrimaryHand))
			{
				return unit.Body.PrimaryHand;
			}
			return null;
		}

		// Token: 0x06008047 RID: 32839 RVA: 0x0020030C File Offset: 0x001FE50C
		[CanBeNull]
		public static WeaponSlot GetThreatHand(this UnitEntityData unit)
		{
			if (unit.IsThreatHand(unit.Body.PrimaryHand))
			{
				return unit.Body.PrimaryHand;
			}
			ItemEntityWeapon maybeWeapon = unit.Body.PrimaryHand.MaybeWeapon;
			if ((maybeWeapon == null || !maybeWeapon.Blueprint.IsTwoHanded) && unit.IsThreatHand(unit.Body.SecondaryHand))
			{
				return unit.Body.SecondaryHand;
			}
			foreach (WeaponSlot weaponSlot in unit.Body.AdditionalLimbs)
			{
				if (unit.IsThreatHand(weaponSlot))
				{
					return weaponSlot;
				}
			}
			return null;
		}

		// Token: 0x06008048 RID: 32840 RVA: 0x002003D0 File Offset: 0x001FE5D0
		private static bool IsThreatHandMelee(this UnitEntityData unit, WeaponSlot hand)
		{
			return unit.IsThreatHand(hand) && hand.Weapon.Blueprint.IsMelee;
		}

		// Token: 0x06008049 RID: 32841 RVA: 0x002003ED File Offset: 0x001FE5ED
		private static bool IsThreatHandRanged(this UnitEntityData unit, WeaponSlot hand)
		{
			return hand.Weapon.Blueprint.IsRanged;
		}

		// Token: 0x0600804A RID: 32842 RVA: 0x00200400 File Offset: 0x001FE600
		private static bool IsThreatHand(this UnitEntityData unit, WeaponSlot hand)
		{
			return hand.HasWeapon && (hand.Weapon.Blueprint.IsMelee || unit.State.Features.SnapShot) && (!hand.Weapon.Blueprint.IsUnarmed || unit.Descriptor.State.Features.ImprovedUnarmedStrike);
		}

		// Token: 0x0600804B RID: 32843 RVA: 0x00200470 File Offset: 0x001FE670
		public static bool CanAttack(this UnitEntityData unit, ItemEntityWeapon weapon)
		{
			if (unit.Descriptor.State.HasCondition(UnitCondition.CanNotAttack))
			{
				return false;
			}
			UnitPartAttackTypeRestriction unitPartAttackTypeRestriction = unit.Get<UnitPartAttackTypeRestriction>();
			return weapon == null || unitPartAttackTypeRestriction == null || unitPartAttackTypeRestriction.CanAttack(weapon.Blueprint.AttackType);
		}

		// Token: 0x0600804C RID: 32844 RVA: 0x002004B4 File Offset: 0x001FE6B4
		public static bool CanAttack(this UnitEntityData unit, Func<UnitEntityData, ItemEntityWeapon> weaponGetter)
		{
			if (unit.Descriptor.State.HasCondition(UnitCondition.CanNotAttack))
			{
				return false;
			}
			UnitPartAttackTypeRestriction unitPartAttackTypeRestriction = unit.Get<UnitPartAttackTypeRestriction>();
			if (unitPartAttackTypeRestriction == null)
			{
				return true;
			}
			ItemEntityWeapon itemEntityWeapon = (weaponGetter != null) ? weaponGetter(unit) : null;
			return itemEntityWeapon == null || unitPartAttackTypeRestriction.CanAttack(itemEntityWeapon.Blueprint.AttackType);
		}

		// Token: 0x0600804D RID: 32845 RVA: 0x00200508 File Offset: 0x001FE708
		public static bool IsReach(this UnitEntityData unit, UnitEntityData enemy, WeaponSlot hand)
		{
			float meters = hand.Weapon.AttackRange.Meters;
			return unit.DistanceTo(enemy) < unit.View.Corpulence + enemy.View.Corpulence + meters && unit.HasLOS(enemy);
		}

		// Token: 0x0600804E RID: 32846 RVA: 0x00200554 File Offset: 0x001FE754
		public static bool IsAttackOfOpportunityReach(this UnitEntityData unit, UnitEntityData enemy, WeaponSlot hand)
		{
			float num = hand.Weapon.AttackRange.Meters;
			if (hand.Weapon.Blueprint.IsRanged)
			{
				if (!unit.State.Features.SnapShot)
				{
					return false;
				}
				num = unit.Stats.ReachRange.Meters + (unit.State.Features.ImprovedSnapShot ? (5.Feet().Meters + (unit.State.Features.GreaterSnapShot ? 5.Feet().Meters : 0f)) : 0f);
			}
			return unit.DistanceTo(enemy) < unit.View.Corpulence + enemy.View.Corpulence + num && unit.HasLOS(enemy);
		}

		// Token: 0x0600804F RID: 32847 RVA: 0x0020063C File Offset: 0x001FE83C
		[CanBeNull]
		public static UnitEntityData GetSaddledUnit(this UnitEntityData unit)
		{
			if (unit == null)
			{
				return null;
			}
			UnitPartRider riderPart = unit.RiderPart;
			if (riderPart == null)
			{
				return null;
			}
			return riderPart.SaddledUnit;
		}

		// Token: 0x06008050 RID: 32848 RVA: 0x00200654 File Offset: 0x001FE854
		[CanBeNull]
		public static UnitEntityData GetRider(this UnitEntityData unit)
		{
			if (unit == null)
			{
				return null;
			}
			UnitPartSaddled saddledPart = unit.SaddledPart;
			if (saddledPart == null)
			{
				return null;
			}
			return saddledPart.Rider;
		}

		// Token: 0x06008051 RID: 32849 RVA: 0x0020066C File Offset: 0x001FE86C
		public static UnitHelper.DamageEstimate EstimateDamage(ItemEntityWeapon weapon, UnitEntityData target)
		{
			UnitPartDamageReduction unitPartDamageReduction = target.Get<UnitPartDamageReduction>();
			UnitDescriptor wielder = weapon.Wielder;
			BaseDamage[] array = (from d in Rulebook.Trigger<RuleCalculateWeaponStats>(new RuleCalculateWeaponStats(((wielder != null) ? wielder.Unit : null) ?? Game.Instance.DefaultUnit, weapon, null)).DamageDescription
								  select d.CreateDamage()).ToArray<BaseDamage>();
			float num = 0f;
			bool flag = true;
			foreach (BaseDamage baseDamage in array)
			{
				DiceFormula dice = baseDamage.Dice;
				float num2 = (float)(dice.Dice.Sides() + 1) / 2f * (float)dice.Rolls + (float)baseDamage.Bonus;
				if (unitPartDamageReduction != null)
				{
					if (baseDamage is PhysicalDamage)
					{
						flag &= unitPartDamageReduction.CanBypass(baseDamage, weapon);
					}
					num2 = unitPartDamageReduction.EstimateDamage(num2, baseDamage, weapon);
				}
				num += num2;
			}
			return new UnitHelper.DamageEstimate
			{
				Value = Math.Max(1, (int)num),
				BypassDR = flag,
				Chunks = array
			};
		}

		// Token: 0x06008052 RID: 32850 RVA: 0x00200785 File Offset: 0x001FE985
		public static bool IsStoryCompanion(this UnitEntityData unit)
		{
			return unit.Blueprint.GetComponent<UnitIsStoryCompanion>() != null;
		}

		// Token: 0x04005812 RID: 22546
		private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("Respec");

		// Token: 0x02002695 RID: 9877
		[Flags]
		public enum BreakFreeFlags
		{
			// Token: 0x0400916A RID: 37226
			Default = 3,
			// Token: 0x0400916B RID: 37227
			CanUseCMB = 1,
			// Token: 0x0400916C RID: 37228
			CMD2DC = 2
		}

		// Token: 0x02002696 RID: 9878
		public struct DamageEstimate
		{
			// Token: 0x0400916D RID: 37229
			public int Value;

			// Token: 0x0400916E RID: 37230
			public bool BypassDR;

			// Token: 0x0400916F RID: 37231
			public BaseDamage[] Chunks;
		}
	}
}
