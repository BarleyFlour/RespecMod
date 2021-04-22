using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecWrath
{
    [HarmonyPatch(typeof(UnitHelper))]
    [HarmonyPatch("CopyInternal")]
    static class UnitProgressionData_CopyFrom_Patch
    {
        static void Postfix(UnitEntityData unit, UnitEntityData __result)
        {
            //升级时会用这个方法复制一个UnitEntityData，其中涉及到复制UnitProgressionData
            //默认状况下，复制的UnitProgressionData的CharacterLevel等于所有非神话职业等级之和
            //如果人物等级不等于这个默认值，会出问题（比如在低于默认值时，可能没到20级就升不了级了，因为非神话职业等级之和已经提前超过了20级）
            //修正这一点。

            var UnitProgressionData_CharacterLevel = AccessTools.Property(typeof(UnitProgressionData), nameof(UnitProgressionData.CharacterLevel));
            UnitProgressionData_CharacterLevel.SetValue(__result.Descriptor.Progression, unit.Descriptor.Progression.CharacterLevel);
        }
    }
}