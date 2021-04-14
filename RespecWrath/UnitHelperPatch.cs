using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(UnitHelper),"Respec")]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(Action)})]
	internal static class UnithelperRespec_patch
	{
		private static void Postfix(UnitEntityData unit)
		{
			int[] initStatsByUnit = Main.GetInitStatsByUnit(unit);
			unit.Descriptor.Stats.Strength.BaseValue = initStatsByUnit[0];
			unit.Descriptor.Stats.Dexterity.BaseValue = initStatsByUnit[1];
			unit.Descriptor.Stats.Constitution.BaseValue = initStatsByUnit[2];
			unit.Descriptor.Stats.Intelligence.BaseValue = initStatsByUnit[3];
			unit.Descriptor.Stats.Wisdom.BaseValue = initStatsByUnit[4];
			unit.Descriptor.Stats.Charisma.BaseValue = initStatsByUnit[5];
		}
	}
}
