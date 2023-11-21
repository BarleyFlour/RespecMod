using HarmonyLib;
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Rest;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RespecWrath
{
    public static class MythicRespec
    {
        public static void MyhticRespec(UnitEntityData unit)
        {
            Main.MythicXP = unit.Progression.MythicExperience;
            for (int i = 0; i < Main.MythicXP; i++)
            {
                // Main.logger.Log(i.ToString());
                unit.Progression.RemoveMythicLevel();
            }
            unit.Progression.AdvanceMythicExperience(Main.MythicXP);
        }

        public static void BubblesRespec(UnitEntityData data)
        {
            Main.settings.FreeRespec = false;
            if (Game.Instance.Player.SpendMoney(Main.respecCost))
            {
                Main.PreRespec(data);
            }
        }
    }
}