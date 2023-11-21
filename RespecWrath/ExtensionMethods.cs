﻿using HarmonyLib;

///credit to Vek17 for this
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RespecWrath
{
    internal static class ExtensionMethods
    {
        public static bool IsStoryCompanionLocal(this UnitEntityData unit)
        {
            return (unit.Blueprint.GetComponent<UnitIsStoryCompanion>() != null || unit.Blueprint.GetComponent<ClassLevelLimit>() != null);
        }
        public static IEnumerable<GameAction> FlattenAllActions(this BlueprintAbility Ability)
        {
            return
                Ability.GetComponents<AbilityExecuteActionOnCast>()
                    .SelectMany(a => a.FlattenAllActions())
                .Concat(
                Ability.GetComponents<AbilityEffectRunAction>()
                    .SelectMany(a => a.FlattenAllActions()));
        }

        public static IEnumerable<GameAction> FlattenAllActions(this AbilityExecuteActionOnCast Action)
        {
            return FlattenAllActions(Action.Actions.Actions);
        }

        public static IEnumerable<GameAction> FlattenAllActions(this AbilityEffectRunAction Action)
        {
            return FlattenAllActions(Action.Actions.Actions);
        }

        public static IEnumerable<GameAction> FlattenAllActions(this IEnumerable<GameAction> Actions)
        {
            List<GameAction> NewActions = new List<GameAction>();
            NewActions.AddRange(Actions.OfType<ContextActionOnRandomTargetsAround>().SelectMany(a => a.Actions.Actions));
            NewActions.AddRange(Actions.OfType<ContextActionConditionalSaved>().SelectMany(a => a.Failed.Actions));
            NewActions.AddRange(Actions.OfType<ContextActionConditionalSaved>().SelectMany(a => a.Succeed.Actions));
            NewActions.AddRange(Actions.OfType<Conditional>().SelectMany(a => a.IfFalse.Actions));
            NewActions.AddRange(Actions.OfType<Conditional>().SelectMany(a => a.IfTrue.Actions));
            if (NewActions.Count > 0)
            {
                return Actions.Concat(FlattenAllActions(NewActions));
            }
            return Actions;
        }

        public static V PutIfAbsent<K, V>(this IDictionary<K, V> self, K key, V value) where V : class
        {
            V oldValue;
            if (!self.TryGetValue(key, out oldValue))
            {
                self.Add(key, value);
                return value;
            }
            return oldValue;
        }

        public static bool IsMC(this UnitEntityData unitEntityData)
        {
            return unitEntityData.IsMainCharacter || unitEntityData.IsCloneOfMainCharacter;
        }

        public static V PutIfAbsent<K, V>(this IDictionary<K, V> self, K key, Func<V> ifAbsent) where V : class
        {
            V value;
            if (!self.TryGetValue(key, out value))
            {
                self.Add(key, value = ifAbsent());
                return value;
            }
            return value;
        }

        public static T[] AddToArray<T>(this T[] array, T value)
        {
            var len = array.Length;
            var result = new T[len + 1];
            Array.Copy(array, result, len);
            result[len] = value;
            return result;
        }

        public static T[] RemoveFromArrayByType<T, V>(this T[] array)
        {
            List<T> list = new List<T>();

            foreach (var c in array)
            {
                if (!(c is V))
                {
                    list.Add(c);
                }
            }

            return list.ToArray();
        }

        public static T[] AddToArray<T>(this T[] array, params T[] values)
        {
            var len = array.Length;
            var valueLen = values.Length;
            var result = new T[len + valueLen];
            Array.Copy(array, result, len);
            Array.Copy(values, 0, result, len, valueLen);
            return result;
        }

        public static T[] AddToArray<T>(this T[] array, IEnumerable<T> values) => AddToArray(array, values.ToArray());

        public static T[] InsertBeforeElement<T>(this T[] array, T value, T element)
        {
            var len = array.Length;
            var result = new T[len + 1];
            int x = 0;
            bool added = false;
            for (int i = 0; i < len; i++)
            {
                if (array[i].Equals(element) && !added)
                {
                    result[x++] = value;
                    added = true;
                }
                result[x++] = array[i];
            }
            return result;
        }

        public static T[] InsertAfterElement<T>(this T[] array, T value, T element)
        {
            var len = array.Length;
            var result = new T[len + 1];
            int x = 0;
            bool added = false;
            for (int i = 0; i < len; i++)
            {
                if (array[i].Equals(element) && !added)
                {
                    result[x++] = array[i];
                    result[x++] = value;
                    added = true;
                }
                else
                {
                    result[x++] = array[i];
                }
            }
            return result;
        }

        public static T[] RemoveFromArray<T>(this T[] array, T value)
        {
            var list = array.ToList();
            return list.Remove(value) ? list.ToArray() : array;
        }

        public static string StringJoin<T>(this IEnumerable<T> array, Func<T, string> map, string separator = " ") => string.Join(separator, array.Select(map));

        ///static readonly FastRef<BlueprintScriptableObject, string> blueprintScriptableObject_set_AssetId = Helpers.CreateFieldSetter<BlueprintScriptableObject, string>("m_AssetGuid");

#if DEBUG
        static readonly Dictionary<String, BlueprintScriptableObject> assetsByName = new Dictionary<String, BlueprintScriptableObject>();

        internal static readonly List<BlueprintScriptableObject> newAssets = new List<BlueprintScriptableObject>();
#endif

        /*  public static void AddAsset(this LibraryScriptableObject library, BlueprintScriptableObject blueprint, String guid)
          {
              //Main.Mod.Debug(MethodBase.GetCurrentMethod());
              if (guid == "")
              {
                  guid = Helpers.GuidStorage.getGuid(blueprint.name);
              }
              //Main.Mod.Debug(guid);
              blueprintScriptableObject_set_AssetId(blueprint) = guid;
              // Sanity check that we don't stop on our own GUIDs or someone else's.
              BlueprintScriptableObject existing;
              if (library.BlueprintsByAssetId.TryGetValue(guid, out existing))
              {
                  throw Main.Error($"Duplicate AssetId for {blueprint.name}, existing entry ID: {guid}, name: {existing.name}, type: {existing.GetType().Name}");
              }
              else if (guid == "")
              {
                  throw Main.Error($"Missing AssetId: {guid}, name: {existing.name}, type: {existing.GetType().Name}");
              }
  #if DEBUG
              newAssets.Add(blueprint);
  #endif
  #if false
              // Sanity check that names are unique. This is less important, but the feat selection UI
              // gets confused if multiple entries have the same name.
              if (assetsByName.TryGetValue(blueprint.name, out existing))
              {
                  Log.Write($"Warning: Duplicate name, existing entry ID: {existing.AssetGuid}, name: {existing.name}, type: {existing.GetType().Name}");
              }
              else
              {
                  assetsByName.Add(blueprint.name, blueprint);
              }
  #endif

              library.GetAllBlueprints().Add(blueprint);
              library.BlueprintsByAssetId[guid] = blueprint;
              Helpers.GuidStorage.addEntry(blueprint.name, guid);
          }*/

        public static void SetFeatures(this BlueprintFeatureSelection selection, IEnumerable<BlueprintFeature> features)
        {
            SetFeatures(selection, features.ToArray());
        }

        public static void SetFeatures(this BlueprintFeatureSelection selection, params BlueprintFeature[] features)
        {
            selection.m_AllFeatures = selection.m_Features = features.Select(bp => bp.ToReference<BlueprintFeatureReference>()).ToArray();
        }

        public static void InsertComponent(this BlueprintScriptableObject obj, int index, BlueprintComponent component)
        {
            var components = obj.ComponentsArray.ToList();
            components.Insert(index, component);
            obj.SetComponents(components);
        }

        public static void AddComponent(this BlueprintScriptableObject obj, BlueprintComponent component)
        {
            obj.SetComponents(obj.ComponentsArray.AddToArray(component));
        }

        public static void RemoveComponent(this BlueprintScriptableObject obj, BlueprintComponent component)
        {
            obj.SetComponents(obj.ComponentsArray.RemoveFromArray(component));
        }

        public static void RemoveComponents<T>(this BlueprintScriptableObject obj) where T : BlueprintComponent
        {
            var compnents_to_remove = obj.GetComponents<T>().ToArray();
            foreach (var c in compnents_to_remove)
            {
                obj.SetComponents(obj.ComponentsArray.RemoveFromArray(c));
            }
        }

        public static void RemoveComponents<T>(this BlueprintScriptableObject obj, Predicate<T> predicate) where T : BlueprintComponent
        {
            var compnents_to_remove = obj.GetComponents<T>().ToArray();
            foreach (var c in compnents_to_remove)
            {
                if (predicate(c))
                {
                    obj.SetComponents(obj.ComponentsArray.RemoveFromArray(c));
                }
            }
        }

        public static void AddComponents(this BlueprintScriptableObject obj, IEnumerable<BlueprintComponent> components) => AddComponents(obj, components.ToArray());

        public static void AddComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            var c = obj.ComponentsArray.ToList();
            c.AddRange(components);
            obj.SetComponents(c.ToArray());
        }

        public static void SetComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            // Fix names of components. Generally this doesn't matter, but if they have serialization state,
            // then their name needs to be unique.
            var names = new HashSet<string>();
            foreach (var c in components)
            {
                if (string.IsNullOrEmpty(c.name))
                {
                    c.name = $"${c.GetType().Name}";
                }
                if (!names.Add(c.name))
                {
                    String name;
                    for (int i = 0; !names.Add(name = $"{c.name}${i}"); i++) ;
                    c.name = name;
                }
            }

            obj.ComponentsArray = components;
        }

        public static void SetComponents(this BlueprintScriptableObject obj, IEnumerable<BlueprintComponent> components)
        {
            SetComponents(obj, components.ToArray());
        }

        /*  public static void AddAsset(this LibraryScriptableObject library, BlueprintScriptableObject blueprint, String guid1, String guid2)
          {
              library.AddAsset(blueprint, Helpers.MergeIds(guid1, guid2));
          }*/

        /* public static T Get<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
         {
             return (T)library.BlueprintsByAssetId[assetId];
         }

         public static T TryGet<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
         {
             BlueprintScriptableObject result;
             if (library.BlueprintsByAssetId.TryGetValue(assetId, out result))
             {
                 return (T)result;
             }
             return null;
         }*/

        /*  public static T CopyAndAdd<T>(this LibraryScriptableObject library, String assetId, String newName, String newAssetId, String newAssetId2 = null) where T : BlueprintScriptableObject
          {
              return CopyAndAdd(library, Get<T>(library, assetId), newName, newAssetId, newAssetId2);
          }

         public static T CopyAndAdd<T>(this LibraryScriptableObject library, T original, String newName, String newAssetId, String newAssetId2 = null) where T : BlueprintScriptableObject
          {
              var clone = UnityEngine.Object.Instantiate(original);
              clone.name = newName;
              var id = newAssetId2 != null ? Helpers.MergeIds(newAssetId, newAssetId2) : newAssetId;
              AddAsset(library, clone, id);
              return clone;
          }*/

        public static T CreateCopy<T>(this T original, Action<T> action = null) where T : UnityEngine.Object
        {
            var clone = UnityEngine.Object.Instantiate(original);
            if (action != null)
            {
                action(clone);
            }
            return clone;
        }

        /*static readonly FastRef<BlueprintUnitFact, LocalizedString> blueprintUnitFact_set_Description = Helpers.CreateFieldSetter<BlueprintUnitFact, LocalizedString>("m_Description");
        static readonly FastRef<BlueprintUnitFact, Sprite> blueprintUnitFact_set_Icon = Helpers.CreateFieldSetter<BlueprintUnitFact, Sprite>("m_Icon");
        static readonly FastRef<BlueprintUnitFact, LocalizedString> blueprintUnitFact_set_DisplayName = Helpers.CreateFieldSetter<BlueprintUnitFact, LocalizedString>("m_DisplayName");
        static readonly FastRef<BlueprintUnitFact, LocalizedString> blueprintUnitFact_get_Description = Helpers.CreateFieldGetter<BlueprintUnitFact, LocalizedString>("m_Description");
        static readonly FastRef<BlueprintUnitFact, LocalizedString> blueprintUnitFact_get_DisplayName = Helpers.CreateFieldGetter<BlueprintUnitFact, LocalizedString>("m_DisplayName");*/

        /*   public static void SetNameDescriptionIcon(this BlueprintUnitFact feature, String displayName, String description, Sprite icon)
           {
               SetNameDescription(feature, displayName, description);
               feature.SetIcon(icon);
           }

           public static void SetNameDescriptionIcon(this BlueprintUnitFact feature, BlueprintUnitFact other)
           {
               SetNameDescription(feature, other);
               feature.SetIcon(other.Icon);
           }*/

        /* public static void SetNameDescription(this BlueprintUnitFact feature, String displayName, String description)
         {
             feature.SetName(Helpers.CreateString(feature.name + ".Name", displayName));
             feature.SetDescription(description);
         }

         public static void SetNameDescription(this BlueprintUnitFact feature, BlueprintUnitFact other)
         {
             blueprintUnitFact_set_DisplayName(feature) = other.GetName();
             blueprintUnitFact_set_Description(feature) = other.GetDescription();
         }

         public static LocalizedString GetName(this BlueprintUnitFact fact) => (LocalizedString)blueprintUnitFact_get_DisplayName(fact);
         public static LocalizedString GetDescription(this BlueprintUnitFact fact) => (LocalizedString)blueprintUnitFact_get_Description(fact);

         public static void SetIcon(this BlueprintUnitFact feature, Sprite icon)
         {
             blueprintUnitFact_set_Icon(feature) = icon;
         }

         public static void SetName(this BlueprintUnitFact feature, LocalizedString name)
         {
             blueprintUnitFact_set_DisplayName(feature) = name;
         }

         public static void SetName(this BlueprintUnitFact feature, String name)
         {
             blueprintUnitFact_set_DisplayName(feature) = Helpers.CreateString(feature.name + ".Name", name);
         }

         public static void SetDescription(this BlueprintUnitFact feature, String description)
         {
             blueprintUnitFact_set_Description(feature) = Helpers.CreateString(feature.name + ".Description", description);
         }

         public static void SetDescription(this BlueprintUnitFact feature, LocalizedString description)
         {
             blueprintUnitFact_set_Description(feature) = description;
         }

         public static bool HasFeatureWithId(this

        Entry level, String id)
         {
             return level.Features.Any(f => HasFeatureWithId(f, id));
         }

         public static bool HasFeatureWithId(this BlueprintUnitFact fact, String id)
         {
             if (fact.AssetGuid == id) return true;
             foreach (var c in fact.ComponentsArray)
             {
                 var addFacts = c as AddFacts;
                 if (addFacts != null) return addFacts.Facts.Any(f => HasFeatureWithId(f, id));
             }
             return false;
         }

         static readonly FastRef<BlueprintArchetype, Sprite> blueprintArchetype_set_Icon = Helpers.CreateFieldSetter<BlueprintArchetype, Sprite>("m_Icon");
         */

        public static void FixDomainSpell(this BlueprintAbility spell, int level, string spellListId)
        {
            var spellList = ResourcesLibrary.TryGetBlueprint<BlueprintSpellList>(spellListId);
            var spells = spellList.SpellsByLevel.First(s => s.SpellLevel == level).Spells;
            spells.Clear();
            spells.Add(spell);
        }

        public static bool HasAreaEffect(this BlueprintAbility spell)
        {
            return spell.AoERadius.Meters > 0f || spell.ProjectileType != AbilityProjectileType.Simple;
        }

        ///public static void SetIcon(this BlueprintAbilityResource resource, Sprite icon) => setIcon(resource) = icon;

        /*static readonly FastRef<BlueprintAbilityResource, Sprite> setIcon = Helpers.CreateFieldSetter<BlueprintAbilityResource, Sprite>("m_Icon");
        //internal static readonly FastSetter<BlueprintAbilityResource, object> setMaxAmount = Helpers.CreateFieldSetter<BlueprintAbilityResource, object>("m_MaxAmount");
        //internal static readonly FastGetter<BlueprintAbilityResource, object> getMaxAmount = Helpers.CreateFieldGetter<BlueprintAbilityResource, object>("m_MaxAmount");
        //static readonly Type blueprintAbilityResource_Amount = HarmonyLib.AccessTools.Inner(typeof(BlueprintAbilityResource), "Amount");

        internal static IEnumerable<BlueprintComponent> WithoutSpellComponents(this IEnumerable<BlueprintComponent> components)
        {
            return components.Where(c => !(c is SpellComponent) && !(c is SpellListComponent));
        }

        internal static int GetCost(this BlueprintAbility.MaterialComponentData material)
        {
            var item = material?.Item;
            return item == null ? 0 : item.Cost * material.Count;
        }

        public static AddConditionImmunity CreateImmunity(this UnitCondition condition)
        {
            var b = Helpers.Create<AddConditionImmunity>();
            b.Condition = condition;
            return b;
        }

        public static AddCondition CreateAddCondition(this UnitCondition condition)
        {
            var a = Helpers.Create<AddCondition>();
            a.Condition = condition;
            return a;
        }

        public static BuffDescriptorImmunity CreateBuffImmunity(this SpellDescriptor spell)
        {
            var b = Helpers.Create<BuffDescriptorImmunity>();
            b.Descriptor = spell;
            return b;
        }

        public static SpellImmunityToSpellDescriptor CreateSpellImmunity(this SpellDescriptor spell)
        {
            var s = Helpers.Create<SpellImmunityToSpellDescriptor>();
            s.Descriptor = spell;
            return s;
        }*/

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = Array.Empty<GameAction>();
            return new ActionList() { Actions = actions };
        }

        public static T Create<T>(Action<T> init = null) where T : new()
        {
            var result = new T();
            init?.Invoke(result);
            return result;
        }

        public static void addAction(this Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction action, Kingmaker.ElementsSystem.GameAction game_action)
        {
            if (action.Actions != null)
            {
                action.Actions = CreateActionList(action.Actions.Actions);
                action.Actions.Actions = action.Actions.Actions.AddToArray(game_action);
            }
            else
            {
                action.Actions = CreateActionList(game_action);
            }
        }

        public static void ReplaceComponent(this BlueprintScriptableObject blueprint, BlueprintComponent oldComponent, BlueprintComponent newComponent)
        {
            BlueprintComponent[] compnents_to_remove = blueprint.ComponentsArray;
            bool found = false;
            for (int i = 0; i < compnents_to_remove.Length; i++)
            {
                if (compnents_to_remove[i] == oldComponent)
                {
                    blueprint.RemoveComponent(oldComponent);
                }
            }
            if (found)
            {
                blueprint.AddComponent(newComponent);
            }
        }

        public static void ReplaceComponents<T>(this BlueprintScriptableObject blueprint, BlueprintComponent newComponent) where T : BlueprintComponent
        {
            blueprint.ReplaceComponents<T>(c => true, newComponent);
        }

        public static void ReplaceComponents<T>(this BlueprintScriptableObject blueprint, Predicate<T> predicate, BlueprintComponent newComponent) where T : BlueprintComponent
        {
            var compnents_to_remove = blueprint.GetComponents<T>().ToArray();
            bool found = false;
            foreach (var c in compnents_to_remove)
            {
                if (predicate(c))
                {
                    blueprint.SetComponents(blueprint.ComponentsArray.RemoveFromArray(c));
                    found = true;
                }
            }
            if (found)
            {
                blueprint.AddComponent(newComponent);
            }
        }

        public static SimpleBlueprint[] GetBlueprints()
        {
            var blueprints = (Dictionary<BlueprintGuid, BlueprintsCache.BlueprintCacheEntry>)AccessTools
            .Field(typeof(BlueprintsCache), "m_LoadedBlueprints")
            .GetValue(ResourcesLibrary.BlueprintsCache);
            var keys = blueprints.Keys.ToArray();
            return keys.Select(k => ResourcesLibrary.TryGetBlueprint(k)).ToArray();
        }
    }
}