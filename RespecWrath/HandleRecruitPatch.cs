/*using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
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

namespace RespecModBarley
{
	[HarmonyPatch(typeof(CompanionRecruitTrigger), "HandleRecruit")]
	[HarmonyPatch(new Type[] { typeof(UnitEntityData) })]
	internal static class HandleRecruit_Patch
	{
		public static int intorglvl;
		private static void Postfix(UnitEntityData companion)
		{
			foreach (string x in Main.orglvllist)
			{
				if (x.Contains(companion.CharacterName.ToString()))
				{
					///Main.logger.Log(x);
					var getNumbers = (from t in x
									  where char.IsDigit(t)
									  select t);
					foreach (char cha in getNumbers)
					{
						intorglvl = Int32.Parse(cha.ToString());
					}
					///Main.logger.Log(new string(getNumbers));

					if (companion.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit != 0)
					{
						Main.logger.Log(intorglvl.ToString());
						companion.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = intorglvl;
					}
				}
			}
		}
	}
}*/