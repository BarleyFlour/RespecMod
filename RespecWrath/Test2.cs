using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(AddPet), "GetPetLevel")]
	internal static class GetPetLevel
	{
		private static void Postfix(AddPet __instance, ref int __result)
        {
			if (Main.IsRespec == true)
		    {
				__result = 0;
			}
        }
	}
	//[HarmonyPatch(typeof(UnitHelper), "CopyInternal")]
	internal static class CopyInternal_Patch
	{
		private static bool Prefix(UnitEntityData unit, bool createView, bool preview, bool copyItems, ref UnitEntityData __result)
		{
			try
			{
				UnitEntityData unitEntityData;
				using (ContextData<UnitEntityData.DoNotCreateItems>.Request())
				{
					using (ContextData<AddClassLevels.DoNotCreatePlan>.RequestIf(preview))
					{
						unitEntityData = Game.Instance.CreateUnitVacuum(unit.OriginalBlueprint);
					}
				}
				unitEntityData.PreviewOf = (preview ? unit : null);
				unitEntityData.TurnOff();
				unitEntityData.Descriptor.CustomName = unit.Descriptor.CustomName;
				unitEntityData.Descriptor.CustomGender = unit.Descriptor.CustomGender;
				unitEntityData.Descriptor.CustomAsks = unit.Descriptor.CustomAsks;
				unitEntityData.Alignment.Initialize(unit.Alignment.ValueVisible);
				unitEntityData.Descriptor.UISettings.SetPortrait(unit.Portrait);
				UnitPartDollData unitPartDollData = unit.Get<UnitPartDollData>();
				if (unitPartDollData != null)
				{
					unitPartDollData.CopyTo(unitEntityData);
				}
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
				if (copyItems)
				{
					UnitHelper.CopyItems(unit, unitEntityData);
				}
				unitEntityData.Progression.UpdateScalePercent(unit.Progression.CurrentScalePercent);
				if (createView)
				{
					UnitEntityView view = unitEntityData.CreateView();
					unitEntityData.AttachView(view);
				}
				unitEntityData.TurnOn();
				__result = unitEntityData;
				return false;
			}
			catch(Exception e)
            {
				Main.logger.Error(e.ToString());
				return true;
            }
		}
	}
}