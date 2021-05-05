using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(UnitHelper), "RespecOnCommit")]
	internal static class Unithelper_RespecOnCommit_Patch
	{
		private static void Postfix(UnitEntityData targetUnit, UnitEntityData tempUnit)
        {
			if(Main.IsRespec == true)
			{
				Main.featurestoadd.Clear();
				Main.IsRespec = false;
				targetUnit.Progression.AdvanceMythicExperience(Main.MythicXP);
				foreach (EntityPart part in Main.partstoadd)
				{
					if(!targetUnit.Parts.Parts.Contains(part))
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
						tempUnit.Parts.m_Parts.Add(part);
					}
				}
				foreach (EntityPart part in Main.partstoadd)
				{
					if (!targetUnit.Parts.m_Parts.Contains(part))
					{
						///part.AttachToEntity(unit);
						targetUnit.Parts.m_Parts.Add(part);
					}
				}
				Main.partstoadd.Clear();
			}
		}
	}
}
