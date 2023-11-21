using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using RespecWrath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Owlcat.Runtime.Core;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;

namespace RespecWrath
{
    [HarmonyPatch(typeof(AddClassLevels), nameof(AddClassLevels.LevelUp), new Type[] { typeof(AddClassLevels), typeof(UnitDescriptor), typeof(int), typeof(UnitFact) })]
    public static class AddClassLevels_LevelUp_Patcher
    {
        static bool ShouldSkipAddingLevels(UnitEntityData unit)
        {
            return !(Main.IsHilorRespec || (Main.settings.OriginalLevel && Main.IsRespec) || !Main.IsRespec && (Main.isrecruit || !Kingmaker.Game.Instance.Player.AllCharacters.Contains(jc => jc.Blueprint.CharacterName == unit.Blueprint.CharacterName) || (!unit.IsStoryCompanionLocal() && !Main.isrecruit) || unit.IsPet));
        }
        static bool ShouldAddFeature(UnitEntityData unit, BlueprintFeature feature)
        {
            if (ShouldSkipAddingLevels(unit))
            {
                return true;
            }
            else if (feature.Groups.Any() && unit.Facts.m_Facts.Where(a => a.GetType() == typeof(Feature)).Any(a => ((BlueprintFeature)a.Blueprint).Groups.Any(b => b == FeatureGroup.BackgroundSelection))) return true;
            else return false;
        }
        static bool IsInParty(UnitEntityData unit)
        {
            return (!Main.IsRespec && (Main.isrecruit || !Kingmaker.Game.Instance.Player.AllCharacters.Contains(jc => jc.Blueprint.CharacterName == unit.Blueprint.CharacterName) || (!unit.IsStoryCompanionLocal() && !Main.isrecruit)));
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            try
            {
                var code = new List<CodeInstruction>(instructions);
                //No.1
                {
                    int insertionIndex = -1;
                    Label SkipToHere = il.DefineLabel();
                    for (int i = 0; i < code.Count - 1; i++)
                    {

                        if (code[i].opcode == OpCodes.Ldarg_1 && code[i + 1].opcode == OpCodes.Ldfld && code[i + 2].opcode == OpCodes.Callvirt && code[i + 3].opcode == OpCodes.Ldarg_0 && code[i + 4].opcode == OpCodes.Ldarg_1 && code[i + 5].opcode == OpCodes.Call)
                        {
                            //insertionIndex = i;
                            code[i].labels.Add(SkipToHere);
                           // Main.logger.Log($"Inserted Label at index: {i}");
                            break;
                        }
                    }
                    for (int i = 0; i < code.Count - 1; i++)
                    {
                        if (code[i].opcode == OpCodes.Ldarg_0 && code[i + 1].opcode == OpCodes.Ldarg_1 && code[i + 2].opcode == OpCodes.Ldloc_2 && code[i + 3].opcode == OpCodes.Ldarg_3 && code[i + 4].opcode == OpCodes.Ldc_I4_1)
                        {
                            insertionIndex = i - 2;
                          //  Main.logger.Log($"Set Insertion point to index: {i}");
                            break;
                        }
                    }
                    var instructionsToInsert = new List<CodeInstruction>();
                    //source c# code > if (Main.IsHilorRespec || Main.settings.OriginalLevel || !Main.IsRespec && (Main.isrecruit || !Game.Instance.Player.AllCharacters.Contains(jc => jc.Blueprint.CharacterName == unit.Blueprint.CharacterName) || (!unit.Unit.IsStoryCompanionLocal() && !Main.isrecruit) || unit.Unit.IsPet))
                    {
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(UnitDescriptor), nameof(UnitDescriptor.Unit))));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(AddClassLevels_LevelUp_Patcher), nameof(AddClassLevels_LevelUp_Patcher.ShouldSkipAddingLevels))));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, SkipToHere));
                    }
                    if (insertionIndex != -1)
                    {
                        //Main.logger.Log($"Inserted IL Code at index: {insertionIndex}");

                        code.InsertRange(insertionIndex, instructionsToInsert);

                        //foreach (var ilcodeinstruction in code) Main.logger.Log(ilcodeinstruction.ToString());
                    }
                }
                //No.2
                {
                    int insertionIndex = -1;
                    Label SkipToHere = il.DefineLabel();
                    for (int i = 0; i < code.Count - 1; i++)
                    {
                        if (code[i].opcode == OpCodes.Ldloc_S && code[i + 1].opcode == OpCodes.Ldc_I4_1 && code[i + 2].opcode == OpCodes.Add && code[i + 3].opcode == OpCodes.Stloc_S)
                        {
                            //insertionIndex = i;
                            code[i].labels.Add(SkipToHere);
                           // Main.logger.Log($"Inserted Label at index: {i}");
                            break;
                        }
                    }
                    for (int i = 0; i < code.Count - 1; i++)
                    {
                        if (code[i].opcode == OpCodes.Ldarg_1 && code[i + 1].opcode == OpCodes.Ldloc_S && code[i + 2].opcode == OpCodes.Ldnull && code[i + 3].opcode == OpCodes.Ldnull && code[i + 4].opcode == OpCodes.Call)
                        {
                            insertionIndex = i - 2;
                           // Main.logger.Log($"Set Insertion point to index: {i}");
                            break;
                        }
                    }
                    var instructionsToInsert = new List<CodeInstruction>();
                    //source c# code > if (Main.IsHilorRespec || Main.settings.OriginalLevel || !Main.IsRespec && (Main.isrecruit || !Game.Instance.Player.AllCharacters.Contains(jc => jc.Blueprint.CharacterName == unit.Blueprint.CharacterName) || (!unit.Unit.IsStoryCompanionLocal() && !Main.isrecruit) || unit.Unit.IsPet))
                    {
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(UnitDescriptor), nameof(UnitDescriptor.Unit))));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, (short)8));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(AddClassLevels_LevelUp_Patcher), nameof(AddClassLevels_LevelUp_Patcher.ShouldAddFeature))));
                        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, SkipToHere));
                    }
                    if (insertionIndex != -1)
                    {
                       // Main.logger.Log($"Inserted IL Code at index: {insertionIndex}");
                        code.InsertRange(insertionIndex, instructionsToInsert);
                        //foreach (var ilcodeinstruction in code) Main.logger.Log(ilcodeinstruction.ToString());
                    }
                }
                return code;
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
                throw e;
            }
        }
    }
}
