using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
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
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.UnitLogic;

namespace RespecModBarley
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
    }

    [HarmonyPatch(typeof(AddClassLevels), "LevelUp")]
    [HarmonyPatch(new Type[] { typeof(AddClassLevels), typeof(UnitDescriptor), typeof(int), typeof(UnitFact) })]
    [HarmonyPriority(9999)]
    static class AddClassLevels_LevelUp
    {
        public static bool Prefix(AddClassLevels c, UnitDescriptor unit, int levels, UnitFact fact = null)
        {
            using (ContextData<AddClassLevels.ExecutionMark>.Request())
            {
                int num;
                if (!c.CharacterClass.IsMythic)
                {
                    ClassLevelLimit classLevelLimit = unit.OriginalBlueprint.GetComponent<ClassLevelLimit>().Or(null);
                    num = ((classLevelLimit != null) ? classLevelLimit.LevelLimit : int.MaxValue);
                }
                else
                {
                    MythicLevelLimit mythicLevelLimit =
                        unit.OriginalBlueprint.GetComponent<MythicLevelLimit>().Or(null);
                    num = ((mythicLevelLimit != null) ? mythicLevelLimit.LevelLimit : int.MaxValue);
                }

                int num2 = num;
                if (ContextData<DefaultBuildData>.Current != null)
                {
                    num2 = 0;
                }

                if (TacticalCombatHelper.IsActive)
                {
                    for (int i = 0; i < levels; i++)
                    {
                        unit.Progression.AddFakeLevel(c.CharacterClass);
                        unit.Progression.ReapplyFeaturesOnLevelUp();
                    }
                }
                else
                {
                    Dictionary<SelectionEntry, HashSet<int>> selectionsHistory =
                        new Dictionary<SelectionEntry, HashSet<int>>();
                    for (int j = 0; j < levels; j++)
                    {
                        if ((c.CharacterClass.IsMythic
                            ? unit.Progression.MythicLevel
                            : unit.Progression.CharacterLevel) < num2)
                        {
                            using (ProfileScope.New("AddClassLevels.AddLevel", (UnityEngine.Object)null))
                            {
                                using (ContextData<IgnorePrerequisites>.Request())
                                {
                                    if(!Main.IsRespec && (Main.isrecruit || !Game.Instance.Player.AllCharacters.Contains(jc => jc.Blueprint.CharacterName == unit.Blueprint.CharacterName) || (!unit.Unit.IsStoryCompanion() && !Main.isrecruit) || unit.Unit.IsPet))
                                    {
                                        AddClassLevels.AddLevel(c, unit, selectionsHistory, fact, true);
                                    }
                                }
                            }

                            if (!c.CharacterClass.IsMythic && j == 0)
                            {
                                SelectionEntry selectionEntry = c.Selections.FirstItem((SelectionEntry it) =>
                                    it.Selection == BlueprintRoot.Instance.Progression.DeitySelection);
                                BlueprintFeature blueprintFeature = (selectionEntry != null)
                                    ? selectionEntry.Features.FirstOrDefault<BlueprintFeature>()
                                    : null;
                                bool flag = unit.Progression.Features.RawFacts.HasItem((Feature it) =>
                                    it.Blueprint.Groups.Contains(FeatureGroup.Deities));
                                if (blueprintFeature != null && !flag && !unit.HasFact(blueprintFeature))
                                {
                                    ((Feature)unit.AddFact(blueprintFeature, null, null)).SetSource(BlueprintRoot.Instance.Progression.FeatsProgression, 1);
                                    unit.Progression.AddSelection(BlueprintRoot.Instance.Progression.DeitySelection,
                                        BlueprintRoot.Instance.Progression.FeatsProgression, 1, blueprintFeature);
                                }
                            }
                        }
                        else if (unit.IsPlayerFaction && !ContextData<AddClassLevels.DoNotCreatePlan>.Current &&
                                 !c.CharacterClass.IsMythic)
                        {
                            LevelUpPlanUnitHolder levelUpPlanUnitHolder = unit.Get<LevelUpPlanUnitHolder>();
                            UnitDescriptor unitDescriptor = (levelUpPlanUnitHolder != null)
                                ? levelUpPlanUnitHolder.RequestPlan()
                                : null;
                            if (unitDescriptor != null)
                            {
                                LevelPlanData plan =
                                    AddClassLevels.AddLevel(c, unitDescriptor, selectionsHistory, fact, true);
                                unit.Progression.AddLevelPlan(plan);
                                if (!c.CharacterClass.IsMythic)
                                {
                                    unit.Progression.SetLevelPlanSkills(c.Skills);
                                }
                            }
                        }
                    }

                    unit.Progression.ReapplyFeaturesOnLevelUp();
                    AddClassLevels.PrepareSpellbook(c, unit);
                    UnitEntityView view = unit.Unit.View;
                    if (view != null)
                    {
                        view.UpdateClassEquipment();
                    }

                    RestController.ApplyRest(unit);
                }
            }
            return false;
        }
    }
}
