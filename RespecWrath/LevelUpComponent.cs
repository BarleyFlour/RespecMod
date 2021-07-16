/*using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RespecModBarley
{
    [ComponentName("AddAbilityScorePoint")]
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintFeature))]
    public class AddAbilityScorePoint : UnitFactComponentDelegate, IGlobalSubscriber
    {
        public bool IsSelectableFeature = true;
        public override void OnFactAttached()
        {
            if(IsSelectableFeature == true)
            {
               var asd = Game.Instance.LevelUpController.LevelUpActions.Where(a => a.Priority == Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpActionPriority.Features);
                foreach(ILevelUpAction action in asd)
                {
                    Traverse.Create(action).Field("Priority").SetValue(LevelUpActionPriority.RaceStat);
                }
                var asd2 = Game.Instance.LevelUpController.LevelUpActions.Where(a => a.Priority == Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpActionPriority.AddAttribute || a.Priority == Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpActionPriority.RemoveAttribute);
                foreach (ILevelUpAction action in asd2)
                {
                    Traverse.Create(action).Field("Priority").SetValue(LevelUpActionPriority.Spells);
                }
            }
            Game.Instance.LevelUpController.State.AttributePoints++;
        }
    }
}*/