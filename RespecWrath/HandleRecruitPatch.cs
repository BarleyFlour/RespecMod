using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;

namespace RespecModBarley
{
	[HarmonyPatch(typeof(Recruit), "SwitchToCompanion")]
	[HarmonyPatch(new Type[] { typeof(RecruitData) })]
	internal static class HandleRecruit_Patch
	{
		private static void Prefix(Recruit __instance,RecruitData data)
		{
			try
			{
				foreach (Recruit.RecruitData dataR in __instance.Recruited)
				{
					///data.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfo(data.RecruitedCompanion);
					///__instance.SwitchToCompanion(data);
					///Main.logger.Log(dataR.CompanionBlueprint.name.ToString());
					dataR.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfoBP(data.CompanionBlueprint);
					Main.GetUnitsForMemory();
				}
			}
			catch(Exception e)
			{ Main.logger.Log(e.ToString()); }
			/*data.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfo(data.RecruitedCompanion);
			bool ShouldMemory = true;
			foreach(UnitInfo info in Main.UnitMemory)
			{
					if (info.Data.CharacterName == data.RecruitedCompanion.CharacterName)
                    {
						Main.logger.Log("Amorgos");
						ShouldMemory = false;
                    }
			}
			if (ShouldMemory == true)
			{
					Main.GetUnitForMemory(data.RecruitedCompanion);
			}
			data.RecruitedCompanion.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfo(data.RecruitedCompanion);
			data.RecruitedCompanion.Blueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfo(data.RecruitedCompanion);*/

		}
		private static void Postfix(Recruit __instance, RecruitData data)
        {
			try
            {
                Main.GetUnitsForMemory();
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
		}
	}
}