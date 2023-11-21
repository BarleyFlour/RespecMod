using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;

namespace RespecWrath
{
    [HarmonyPatch(typeof(LevelUpController), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(UnitEntityData), typeof(bool), typeof(LevelUpState.CharBuildMode),typeof(bool) })]
    [HarmonyPriority(9999)]
    internal static class LevelUpController_ctor_Patch
    {
        private static void Postfix(LevelUpController __instance, UnitEntityData unit, LevelUpState.CharBuildMode mode)
        {
            try
            {
                if (Main.IsRespec == true)
                {
                    if (unit.IsMC())
                    {
                        /*if (Main.settings.PreserveMCRace)
                        {
                            __instance.SelectRace(unit.Progression.Race);
                        }*/

                        if (Main.settings.PreserveMCName)
                        {
                            __instance.SelectName(unit.CharacterName);
                        }

                        if (Main.settings.PreserveMCAlignment)
                        {
                            __instance.SelectAlignment(unit.Alignment.m_Value.Value);
                            unit.Alignment.CopyFrom(unit.Alignment);
                        }

                        if (Main.settings.PreserveMCBirthday)
                        {
                            __instance.SetBirthDay(unit.Descriptor.BirthDay, unit.Descriptor.BirthMonth);
                        }
                        if (Main.settings.PreserveVoice)
                        {
                            __instance.SelectGender(unit.Gender);
                            __instance.SelectVoice(unit.Descriptor.Asks);
                        }
                    }
                    else if (unit.IsStoryCompanionLocal() && Main.settings.PreserveVoice)
                    {
                        __instance.SelectGender(unit.Gender);
                        __instance.SelectVoice(unit.Descriptor.Asks);
                    }
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
    }
}