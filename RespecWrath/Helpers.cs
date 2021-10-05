using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace RespecModBarley
{
    public static class Helpers
    {
        

        public static T GetBlueprint<T>(string id) where T : SimpleBlueprint
        {
            var assetId = new BlueprintGuid(System.Guid.Parse(id));
            return GetBlueprint<T>(assetId);
        }
        public static T GetBlueprint<T>(BlueprintGuid id) where T : SimpleBlueprint
        {
            SimpleBlueprint asset = ResourcesLibrary.TryGetBlueprint(id);
            T value = asset as T;
            if (value == null) { Main.logger.Error($"COULD NOT LOAD: {id} - {typeof(T)}"); }
            return value;
        }

        public static bool ThisIsADietyFeature(BlueprintFeature candidate)
        {
            
            BlueprintFeatureSelection deityselect = Helpers.GetBlueprint<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            return deityselect.m_AllFeatures.Any(x=>x.Equals(candidate.ToReference<BlueprintFeatureReference>()));
        }

        public static bool ThisIsABackgroundSelector(BlueprintFeature candidate)
        {
            
            BlueprintFeatureSelection backgroundselectorSelector = Helpers.GetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");
            return backgroundselectorSelector.m_AllFeatures.Any(x => x.Equals(candidate.ToReference<BlueprintFeatureReference>()));
        }

        public static bool ThisIsABackground(BlueprintFeature candidate)
        {
            BlueprintFeatureSelection backgroundselectorSelector = Helpers.GetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");
            foreach (BlueprintFeatureReference f in backgroundselectorSelector.m_AllFeatures)
            {
                if (f.GetBlueprint() is BlueprintFeatureSelection select)
                {
                    if (select.m_AllFeatures.Any(x => x.Equals(candidate.ToReference<BlueprintFeatureReference>())))
                    {
                        return true;
                    }
                }
            }

            return false;

        }
    }
}
