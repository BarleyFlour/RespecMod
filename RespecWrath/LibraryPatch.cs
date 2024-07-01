///credit to spacehamster for (most) of this

using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using RespecWrath;
using System;

[HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
internal static class ResourcesLibrary_InitializeLibrary_Patch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (Main.haspatched) return;
        try
        {
            Main.PatchLibrary();
        }
        catch (Exception ex)
        {
            Main.logger.Log("Error while patching library");
            Main.logger.Log(ex.ToString());
        }
    }
}

namespace RespecWrath
{
    public static class DeityBackground
    {
        public static BlueprintFeature[] DeityFeatures
        {
            get
            {
                if (!Main.haspatched) Main.PatchLibrary();
                return m_DeityFeatures;
            }
            set { m_DeityFeatures = value; }
        }

        public static BlueprintFeature[] m_DeityFeatures;

        public static BlueprintFeature[] m_BackgroundFeatures;

        public static BlueprintFeature[] backgroundfeatures
        {
            get
            {
                if (!Main.haspatched) Main.PatchLibrary();
                return m_BackgroundFeatures;
            }
            set { m_BackgroundFeatures = value; }
        }

        public static BlueprintFeatureSelection DeitySelect =>
            ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

        public static BlueprintFeatureSelection BackgroundSelect =>
            ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");
    }
}