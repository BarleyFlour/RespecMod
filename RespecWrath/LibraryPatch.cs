///credit to spacehamster for (most) of this
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using RespecModBarley;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[HarmonyPatch(typeof(ResourcesLibrary), "InitializeLibrary")]
static class ResourcesLibrary_InitializeLibrary_Patch
{
	static bool Initialized;
	static bool Prefix()
	{
		if (Initialized)
		{
			// When wrath first loads into the main menu InitializeLibrary is called by Kingmaker.GameStarter.
			// When loading into maps, Kingmaker.Runner.Start will call InitializeLibrary which will
			// clear the ResourcesLibrary.s_LoadedBlueprints cache which causes loaded blueprints to be garbage collected.
			// Return false here to prevent ResourcesLibrary.InitializeLibrary from being called twice 
			// to prevent blueprints from being garbage collected.
			return false;
		}
		else
		{
			return true;
		}
	}
	static void Postfix()
	{
		if (Initialized) return;
		Initialized = true;
		try
		{
			Main.logger.Log("Library patching initiated");
			var BPEdit1 = Stuf.Arueshalae.GetComponent<ClassLevelLimit>().LevelLimit = 0;

			var BPEdit3 = Stuf.Nenio.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit4 = Stuf.Nenio.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit5 = Stuf.Camelia.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit6 = Stuf.Camelia.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit7 = Stuf.Seelah.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit8 = Stuf.Seelah.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit9 = Stuf.Ember.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit10 = Stuf.Ember.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit11 = Stuf.Lann.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit12 = Stuf.Lann.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit13 = Stuf.Daeran.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit14 = Stuf.Daeran.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit15 = Stuf.Staunton.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit16 = Stuf.Staunton.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit17 = Stuf.Regill.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit18 = Stuf.Regill.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit19 = Stuf.SosielVaenic.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit20 = Stuf.SosielVaenic.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit21 = Stuf.Delamere.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit22 = Stuf.Delamere.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit23 = Stuf.Woljif.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit24 = Stuf.Woljif.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit25 = Stuf.Ciar.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit26 = Stuf.Ciar.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit27 = Stuf.Anevia.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit28 = Stuf.Anevia.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit29 = Stuf.Wenduag.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit30 = Stuf.Wenduag.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit31 = Stuf.LichGalfrey.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit32 = Stuf.LichGalfrey.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit33 = Stuf.EvilArueshalae.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit34 = Stuf.EvilArueshalae.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit35 = Stuf.Galfrey.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit36 = Stuf.Galfrey.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit39 = Stuf.Greybor.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit49 = Stuf.Greybor.GetComponent<AddClassLevels>().Levels = 0;

			var BPEdit50 = Stuf.Trever.GetComponent<ClassLevelLimit>().LevelLimit = 0;
			//var BPEdit51 = Stuf.Trever.GetComponent<AddClassLevels>().Levels = 0;

			var arueshalaeFeatureList = Stuf.ArueshalaeFeatureList.ToReference<BlueprintFeatureReference>();
			var nenioFeatureList = Stuf.NenioFeatureList.ToReference<BlueprintFeatureReference>();
			var cameliaFeatureList = Stuf.CameliaFeatureList.ToReference<BlueprintFeatureReference>();
			var seelahFeatureList = Stuf.SeelahFeatureList.ToReference<BlueprintFeatureReference>();
			var emberFeatureList = Stuf.EmberFeatureList.ToReference<BlueprintFeatureReference>();
			var lannFeatureList = Stuf.LannFeatureList.ToReference<BlueprintFeatureReference>();
			var daeranFeatureList = Stuf.DaeranFeatureList.ToReference<BlueprintFeatureReference>();
			var stauntonFeatureList = Stuf.StauntonFeatureList.ToReference<BlueprintFeatureReference>();
			var regillFeatureList = Stuf.RegillFeatureList.ToReference<BlueprintFeatureReference>();
			var sosielvaenicFeatureList = Stuf.SosielVaenicFeatureList.ToReference<BlueprintFeatureReference>();
			var delamereFeatureList = Stuf.DelamereFeatureList.ToReference<BlueprintFeatureReference>();
			var woljifFeatureList = Stuf.WoljifFeatureList.ToReference<BlueprintFeatureReference>();
			var ciarFeatureList = Stuf.CiarFeatureList.ToReference<BlueprintFeatureReference>();
			var aneviaFeatureList = Stuf.AneviaFeatureList.ToReference<BlueprintFeatureReference>();
			var wenduagFeatureList = Stuf.WenduagFeatureList.ToReference<BlueprintFeatureReference>();
			var galfreyFeatureList = Stuf.GalfreyFeatureList.ToReference<BlueprintFeatureReference>();
			var greyborFeatureList = Stuf.GreyborFeatureList.ToReference<BlueprintFeatureReference>();
			var treverFeatureList = Stuf.TreverFeatureList.ToReference<BlueprintFeatureReference>();
			var noSelectionIfAlreadyHasFeature = ScriptableObject.CreateInstance<NoSelectionIfAlreadyHasFeature>();
			noSelectionIfAlreadyHasFeature.AnyFeatureFromSelection = false;
			noSelectionIfAlreadyHasFeature.m_Features = new BlueprintFeatureReference[] { arueshalaeFeatureList };
			ExtensionMethods.AddComponent(Stuf.TieflingHeritageSelect, noSelectionIfAlreadyHasFeature);

			var noSelectionIfAlreadyHasFeatureBackgroundSelect = ScriptableObject.CreateInstance<NoSelectionIfAlreadyHasFeature>();
			noSelectionIfAlreadyHasFeatureBackgroundSelect.AnyFeatureFromSelection = false;
			noSelectionIfAlreadyHasFeatureBackgroundSelect.m_Features = new BlueprintFeatureReference[] { arueshalaeFeatureList, nenioFeatureList, cameliaFeatureList, seelahFeatureList, emberFeatureList, lannFeatureList, daeranFeatureList, stauntonFeatureList, regillFeatureList, sosielvaenicFeatureList, delamereFeatureList, woljifFeatureList, ciarFeatureList, aneviaFeatureList, wenduagFeatureList, galfreyFeatureList, greyborFeatureList, treverFeatureList };
			ExtensionMethods.AddComponent(Stuf.BackgroundSelect, noSelectionIfAlreadyHasFeatureBackgroundSelect);
		}
		catch (Exception ex)
		{
			Main.logger.Log("Error while patching library");
			Main.logger.Log(ex.ToString());
		}
	}
}
namespace RespecModBarley
{
	static public class Stuf
	{
		public static T Create<T>(Action<T> init = null) where T : ScriptableObject
		{
			var result = ScriptableObject.CreateInstance<T>();
			if (init != null) init(result);
			return result;
		}
		public static T Get<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
		{
			return (T)ResourcesLibrary.TryGetBlueprint(assetId);
		}
		public static BlueprintUnit Arueshalae => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("a352873d37ec6c54c9fa8f6da3a6b3e1");
		public static BlueprintUnit Nenio => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("1b893f7cf2b150e4f8bc2b3c389ba71d");
		public static BlueprintUnit Camelia => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("397b090721c41044ea3220445300e1b8");
		public static BlueprintUnit Seelah => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
		public static BlueprintUnit Ember => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("2779754eecffd044fbd4842dba55312c");
		public static BlueprintUnit Lann => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("cb29621d99b902e4da6f5d232352fbda");
		public static BlueprintUnit Daeran => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("096fc4a96d675bb45a0396bcaa7aa993");
		public static BlueprintUnit Staunton => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("0bcf3c125a28d164191e874e3c0c52de");
		public static BlueprintUnit Regill => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("0d37024170b172346b3769df92a971f5");
		public static BlueprintUnit SosielVaenic => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("1cbbbb892f93c3d439f8417ad7cbb6aa");
		public static BlueprintUnit Delamere => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("6b1f599497f5cfa42853d095bda6dafd");
		public static BlueprintUnit Woljif => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("766435873b1361c4287c351de194e5f9");
		public static BlueprintUnit Ciar => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("7ece3afabe2b6f343b17d1eaa409d273");
		public static BlueprintUnit Anevia => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("88162735402ac094d8a08867814902dd");
		public static BlueprintUnit Wenduag => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("ae766624c03058440a036de90a7f2009");
		public static BlueprintUnit LichGalfrey => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d58b81fd7ec14784fa05bc29fb6c7ae0");
		public static BlueprintUnit EvilArueshalae => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e3bc95db7e2181d41847b3a1d858258d");
		public static BlueprintUnit Galfrey => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e46927657a79db64ea30758db3f42bb9");
		public static BlueprintUnit Kestoglyr => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e551850403d61eb48bb2de010d12c894");
		public static BlueprintUnit Greybor => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f72bb7c48bb3e45458f866045448fb58");
		public static BlueprintUnit Trever => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("0bb1c03b9f7bbcf42bb74478af2c6258");
		public static BlueprintScriptableObject TieflingHeritageSelect => ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>("c862fd0e4046d2d4d9702dd60474a181");
		public static BlueprintScriptableObject BackgroundSelect => ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>("f926dabeee7f8a54db8f2010b323383c");
		public static BlueprintFeature Airborne => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("70cffb448c132fa409e49156d013b175");
		public static BlueprintRace HumanRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
		public static BlueprintRace HalfElfRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
		public static BlueprintRace HalfOrcRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");

		public static BlueprintFeature ArueshalaeFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7993c81bd04ffda4bac123eb7f6752c4");
		public static BlueprintFeature NenioFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("751afafb3b7017544ac6373901747f60");
		public static BlueprintFeature CameliaFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c84c2f0728cc18f46a9e2796fcc08ac4");
		public static BlueprintFeature SeelahFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("777ae11136378a64883059457966a325");
		public static BlueprintFeature EmberFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("b38df30d993476640a17f8c44bd2fffe");
		public static BlueprintFeature LannFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("76c17406e58c73f4ea49dff84843ba38");
		public static BlueprintFeature DaeranFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2ee47e0864a587140b068e43b4844421");
		public static BlueprintFeature StauntonFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("5783ecee3c3eb644e91cd770001ed1c3");
		public static BlueprintFeature RegillFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8dab304f06da25c418f48348e2c22c84");
		public static BlueprintFeature SosielVaenicFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c35688e0390571e41802a3e0bce2fb98");
		public static BlueprintFeature DelamereFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("1f0e1ef92fa98514ea42e5970114d4b8");
		public static BlueprintFeature WoljifFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4bf1e3da22a4fe44f8a516cc24e6ef79");
		public static BlueprintFeature CiarFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ec014b7a7a6cd814090c1cd6c7a599d8");
		public static BlueprintFeature AneviaFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("050c6099637a5ab488845e3040d07bc4");
		public static BlueprintFeature WenduagFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("e4fb695e280c78f4c87854598ee7e70a");
		public static BlueprintFeature GalfreyFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d35df3cb678f6a74b91b143362ec0451");
		public static BlueprintFeature GreyborFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0c487d8576cc68046a8b02bf7e94d5c2");
		public static BlueprintFeature TreverFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3552b16bcfb64944c8b5825661fa7b90");
	}
}