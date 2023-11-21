using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using System;
using static Kingmaker.Designers.EventConditionActionSystem.Actions.Recruit;

namespace RespecWrath
{
    [HarmonyPatch(typeof(Recruit), "SwitchToCompanion")]
    [HarmonyPatch(new Type[] { typeof(RecruitData) })]
    internal static class HandleRecruit_Patch
    {
        private static void Prefix(Recruit __instance, RecruitData data)
        {
            try
            {
                //Main.logger.Log("recruited" + data.CompanionBlueprint.CharacterName);
                // Main.logger.Log(Main.GetUnitInfoBP(data.CompanionBlueprint)[0].ToString());
                Main.isrecruit = true;
                //testy
                /*data.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit =
                    Main.GetUnitInfoBP(data.CompanionBlueprint)[0];
                data.CompanionBlueprint.GetComponent<MythicLevelLimit>().LevelLimit =
                    Main.GetUnitInfoBP(data.CompanionBlueprint)[1];*/

                /*foreach (Recruit.RecruitData dataR in __instance.Recruited)
                {
                    ///data.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfo(data.RecruitedCompanion);
                    ///__instance.SwitchToCompanion(data);
                    ///Main.logger.Log(dataR.CompanionBlueprint.name.ToString());
                    dataR.CompanionBlueprint.GetComponent<ClassLevelLimit>().LevelLimit = Main.GetUnitInfoBP(data.CompanionBlueprint)[0];
                    dataR.CompanionBlueprint.GetComponent<MythicLevelLimit>().LevelLimit = Main.GetUnitInfoBP(data.CompanionBlueprint)[1];
                    Main.GetUnitForMemory(dataR.CompanionBlueprint);
                }*/
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
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
                Main.isrecruit = false;
                //Main.logger.Log(Main.isrecruit.ToString());

                // Main.GetUnitsForMemory();
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Recruit), "RunAction")]
    [HarmonyPatch(new Type[] { })]
    internal static class HandleRecruitRun_Patch
    {
        private static void Prefix(Recruit __instance)
        {
            try
            {
                Main.isrecruit = true;
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }
}