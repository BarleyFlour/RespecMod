using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Progression.ChupaChupses;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Progression.Feats;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Progression.Main;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RespecWrath
{
    internal class AssetLoader
    {
        public static Sprite LoadInternal(string folder, string file, Vector2Int size)
        {
#if UMM
            return Image2Sprite.Create($"{Main.ModEntry.Path}Assets{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}{file}", size);
#endif
#if WrathMod
            return Image2Sprite.Create($"{Main.modEntry.Path + @"\"}Assets{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}{file}", size);
#endif
        }

        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static class Image2Sprite
        {
            public static string icons_folder = "";

            public static Sprite Create(string filePath, Vector2Int size)
            {
                Main.logger.Log("Creating sprite");
                var bytes = File.ReadAllBytes(icons_folder + filePath);
                var texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
                _ = texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, size.x, size.y), new Vector2(0, 0));
            }
        }
    }

    public static class Helpers
    {
        public static T CreateBlueprint<T>([NotNull] string name, Action<T> init = null) where T : SimpleBlueprint, new()
        {
            var result = new T
            {
                name = name,
                AssetGuid = new BlueprintGuid(Guid.NewGuid())
            };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(result.AssetGuid, result);
            result.OnEnable();
            init?.Invoke(result);
            return result;
        }

        public static LocalizedString CreateString(string key, string value)
        {
            // See if we used the text previously.
            // (It's common for many features to use the same localized text.
            // In that case, we reuse the old entry instead of making a new one.)
            LocalizedString localized;
            /* if (textToLocalizedString.TryGetValue(value, out localized))
             {
                 return localized;
             }*/
            //var strings = LocalizationManager.CurrentPack.Strings;
            //String oldValue;
            //if (strings.TryGetValue(key, out oldValue) && value != oldValue)
            {
#if DEBUG
           //     Main.LogDebug($"Info: duplicate localized string `{key}`, different text.");
#endif
                //}
                // strings[key] = value;
                localized = new LocalizedString
                {
                    m_Key = key
                };
                LocalizationManager.CurrentPack.PutString(key,value);
                // textToLocalizedString[value] = localized;
                return localized;
            }
        }
    }

    [HarmonyPatch(typeof(FeatProgressionVM))]
    internal static class FeatProgressionVM_Patches
    {
        private static BlueprintFeatureSelection AttribSelection = null;
        private static BlueprintFeature[] AttribFeatures = new BlueprintFeature[6];

        private static BlueprintFeatureReference[] AttribFeatureRefs;

        private static string[] AttribNames = {
            "strength",
            "dexterity",
            "constitution",
            "intelligence",
            "wisdom",
            "charisma",
        };

        private static string[] AttributeEnhancementBuffs = {
            "b175001b42b1a02479881b72fe132116",
            "f011d0ab4a405e54aa0e83cd10e54430",
            "c3de8cc9a0f50e2418dde526d8855faa",
            "c8c9872e9e02026479d82b9264b9cc6b",
            "73fc1d19f14339042ba5af34872c1745",
            "7ed853ffcfd29914cb098cd7b1c46cc4",
        };

        [HarmonyPatch(nameof(FeatProgressionVM.BuildFeats)), HarmonyPostfix]
        private static void BuildFeats(FeatProgressionVM __instance)
        {
            if (!Main.settings.AttributeInClassPage) return;
            if (AttribSelection == null)
            {
                for (int i = 0; i < AttribFeatures.Length; i++)
                {
                    var name = AttribNames[i];
                    var key = $"barley-attrib-{name}";
                    var title = $"{char.ToUpper(name[0])}{name.Substring(1)}";
                    AttribFeatures[i] = Helpers.CreateBlueprint<BlueprintFeature>(key, feature =>
                    {
                        feature.Groups = Array.Empty<FeatureGroup>();
                        feature.m_DisplayName = Helpers.CreateString($"{key}.name", title);
                        feature.m_Description = Helpers.CreateString($"{key}.description", title);
                        feature.m_DescriptionShort = Helpers.CreateString($"{key}.description-short", title);
                        feature.m_Icon = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(AttributeEnhancementBuffs[i]).m_Icon;
                    });
                }

                AttribFeatureRefs = AttribFeatures.Select(f => f.ToReference<BlueprintFeatureReference>()).ToArray();

                AttribSelection = Helpers.CreateBlueprint<BlueprintFeatureSelection>("barley-attrib-selection", feature =>
                {
                    feature.Groups = Array.Empty<FeatureGroup>();
                    feature.m_DisplayName = Helpers.CreateString("barley-attrib-selection.name", "Attribute Selection");
                    feature.m_Description = Helpers.CreateString("barley-attrib-selection.description", "Attribute Selection");
                    feature.m_DescriptionShort = Helpers.CreateString("barley-attrib-selection.description-short", "Attribute Selection");
                    feature.m_Features = AttribFeatureRefs;
                    feature.m_AllFeatures = AttribFeatureRefs;
                    feature.m_Icon = AssetLoader.LoadInternal("icons", "attrib_selection.png", new Vector2Int(192, 192));
                });
            }

            var line = new List<FeatureProgressionChupaChupsVM>();

            for (int level = 4; level <= 20; level += 4)
            {
                BlueprintFeature feature = null;

                if (__instance.Unit.Unit.Progression.CharacterLevel < level)
                    feature = AttribSelection;
                else
                {
                    if (GlobalLevelInfo.Instance.ForCharacter(__instance.Unit.Unit).AbilityScoresByLevel.TryGetValue(level, out var stat))
                    {
                        feature = AttribFeatures[(int)stat - 1];
                    }
                    else
                    {
                        feature = AttribSelection;
                    }
                }

                ProgressionVM.FeatureEntry featureEntry = new ProgressionVM.FeatureEntry
                {
                    Feature = feature,
                    Level = level,
                    Index = 0,
                };

                __instance.m_FeatureEntries.Add(featureEntry);
                line.Add(__instance.GetChupaChups(featureEntry));
            }
            __instance.MainChupaChupsLines.Add(line);
        }
    }
}