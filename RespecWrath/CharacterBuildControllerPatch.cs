using HarmonyLib;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
    [HarmonyPatch(typeof(CharacterBuildController), "SetupUI")]
    internal static class CharacterBuildController_Patch
    {
        private static void Postfix(CharacterBuildController __instance)
        {
            if(Main.IsEnabled == false){return;}
            if (__instance.LevelUpController.State.Mode == LevelUpState.CharBuildMode.CharGen && __instance.Unit.Progression.Experience > 0 && !__instance.Unit.IsCustomCompanion())
            {
                if (!__instance.Unit.IsMainCharacter)
                {
                    __instance.Character.VoiceSelector.gameObject.SetActive(false);
                    __instance.Character.NameInput.gameObject.SetActive(false);
                    __instance.Character.m_BirthDay.gameObject.SetActive(false);
                    __instance.Race.m_GenderSelector.gameObject.SetActive(false);
                }
                else
                {
                    __instance.Character.VoiceSelector.gameObject.SetActive(true);
                    __instance.Character.NameInput.gameObject.SetActive(true);
                    __instance.Character.m_BirthDay.gameObject.SetActive(true);
                    __instance.Race.m_GenderSelector.gameObject.SetActive(true);
                }
            }
            else
            {
                __instance.Character.NameInput.gameObject.SetActive(false);
                __instance.Character.VoiceSelector.gameObject.SetActive(false);
                __instance.Character.AlignmentSelector.gameObject.SetActive(false);
                __instance.Race.m_GenderSelector.gameObject.SetActive(false);
                __instance.Character.m_BirthDay.gameObject.SetActive(!__instance.LevelUpController.State.IsEmployee);
            }
        }
    }
}
