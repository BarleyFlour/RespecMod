using HarmonyLib;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(UnitHelper), "RespecOnCommit")]
	internal static class Unithelper_RespecOnCommit_Patch
	{
		private static void Postfix(UnitEntityData targetUnit, UnitEntityData tempUnit)
        {
			Main.featurestoadd.Clear();
			Main.partstoadd.Clear();
			Main.IsRespec = false;
			///targetUnit.m_Group = Main.unitgroupparty;
			///tempUnit.m_Group = Main.unitgroupparty;
			foreach (EntityPart part in Main.partstoadd)
			{
				if (!targetUnit.Parts.Parts.Contains(part))
				{
					part.AttachToEntity(targetUnit);
					part.TurnOn();
					targetUnit.Parts.m_Parts.Add(part);
				}
			}
			foreach (EntityPart part in Main.partstoadd)
			{
				if (!targetUnit.Parts.Parts.Contains(part))
				{
					part.AttachToEntity(tempUnit);
					part.TurnOn();
					tempUnit.Parts.m_Parts.Add(part);
				}
			}
		}
	}
}
