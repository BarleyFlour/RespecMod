///credit to spacehamster for (most) of this
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using RespecWrath;
using System;

[HarmonyPatch(typeof(BlueprintsCache), "Init")]
internal static class ResourcesLibrary_InitializeLibrary_Patch
{
#pragma warning disable CS0169
	private static bool Initialized;
#pragma warning restore CS0169
	/*static bool Prefix()
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
	}*/

    private static void Postfix()
    {
        //ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentHead>("9b422adf4bdb9994a9690ac20ca74370").m_EquipmentEntity = ResourcesLibrary.TryGetBlueprint<KingmakerEquipmentEntity>("fea832fe875ad1748a97cf50166ce394").ToReference<KingmakerEquipmentEntityReference>();
        //Traverse.Create(ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentHead>("9b422adf4bdb9994a9690ac20ca74370")).Field("EquipmentEntity").SetValue(ResourcesLibrary.TryGetBlueprint<KingmakerEquipmentEntity>("fea832fe875ad1748a97cf50166ce394"));
        if (!Main.haspatched)
        {
            try
            {
                ///var asd = new AddAbilityScorePoint();
                ///ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16612cc8d88ef83458d05fd063709af7").AddComponent(asd);
                Main.PatchLibrary();
            }
            catch (Exception ex)
            {
                Main.logger.Log("Error while patching library");
                Main.logger.Log(ex.ToString());
            }
        }
        /// if (Main.haspatched) {return;}
        /*try
		{
			Main.logger.Log("Library patching initiated");

			/*var arueshalaeFeatureList = Stuf.ArueshalaeFeatureList.ToReference<BlueprintFeatureReference>();
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
			var noSelectionIfAlreadyHasFeature = new NoSelectionIfAlreadyHasFeature();
			noSelectionIfAlreadyHasFeature.m_Features = new BlueprintFeatureReference[] { arueshalaeFeatureList };
			ExtensionMethods.AddComponent(Stuf.TieflingHeritageSelect, noSelectionIfAlreadyHasFeature);*//*
			var noSelectionIfAlreadyHasFeatureBackgroundSelect = new NoSelectionIfAlreadyHasFeature();
			noSelectionIfAlreadyHasFeatureBackgroundSelect.AnyFeatureFromSelection = false;
			var FeatureListsT = Utilities.GetScriptableObjects<BlueprintFeature>().ToList().FindAll(list => list.name.Contains("_FeatureList")).ToArray().Select(lis => lis.ToReference<BlueprintFeatureReference>()).ToArray();
			foreach(BlueprintFeatureReference f in FeatureListsT)
            {
				Main.logger.Log(f.ToString());
            }
			///noSelectionIfAlreadyHasFeatureBackgroundSelect.m_Features = new BlueprintFeatureReference[] { arueshalaeFeatureList, nenioFeatureList, cameliaFeatureList, seelahFeatureList, emberFeatureList, lannFeatureList, daeranFeatureList, stauntonFeatureList, regillFeatureList, sosielvaenicFeatureList, delamereFeatureList, woljifFeatureList, ciarFeatureList, aneviaFeatureList, wenduagFeatureList, galfreyFeatureList, greyborFeatureList, treverFeatureList };
			noSelectionIfAlreadyHasFeatureBackgroundSelect.m_Features = FeatureListsT;
			ExtensionMethods.AddComponent(Stuf.BackgroundSelect, noSelectionIfAlreadyHasFeatureBackgroundSelect);
			var UnitBPs = Utilities.GetScriptableObjects<BlueprintScriptableObject>().OfType<BlueprintUnit>().ToList();
			var Companions = UnitBPs.FindAll(BPUnits => BPUnits.NameForAcronym.Contains("_Companion") && BPUnits.Components.OfType<ClassLevelLimit>().Any());
			foreach(BlueprintUnit data in Companions)
            {
				Main.GetUnitForMemory(data);
				/*var F = new BlueprintUnitFact();
				F.name = "IsCompanion";
				Traverse.Create(F).Field("Name").SetValue("IsCompanion");
				Traverse.Create(F).Field("NameForAcronym").SetValue("IsCompanion");
				Traverse.Create(F).Field("NameSafe").SetValue("IsCompanion");
				Traverse.Create(F).Field("Name").SetValue("IsCompanion");*/

        ///data.AddFacts.AddItem(F);
        ///Main.logger.Log(data.name);
        /*}
        /*foreach (UnitEntityData data in Game.Instance.Player.PartyCharacters)
        {
            if(data.IsStoryCompanionLocal() == true)
            {
                var add = data.CharacterName.ToString() + "" + data.OriginalBlueprint.GetComponent<ClassLevelLimit>().LevelLimit.ToString();
                Main.UnitMemory.Add(add);
                Main.logger.Log(add.ToString());
                Main.logger.Log("stuff");
                Main.logger.Log(Main.UnitMemory.ToString());
            }
        }*/

        ///var BackgroundArray = new BlueprintFeature[] {Stuf.BackgroundAcolyte, Stuf.BackgroundAcrobat, Stuf.BackgroundAldoriSwordsman, Stuf.BackgroundAlkenstarAlchemist, Stuf.BackgroundAndoranDiplomat, Stuf.BackgroundBountyHunter, Stuf.BackgroundCheliaxianDiabolist, Stuf.BackgroundCourtIntriguer, Stuf.BackgroundEmissary, Stuf.BackgroundFarmhand, Stuf.BackgroundGebianNecromancer, Stuf.BackgroundGladiator, Stuf.BackgroundGuard, Stuf.BackgroundHealer, Stuf.BackgroundHermit, Stuf.BackgroundHunter, Stuf.BackgroundLeader, Stuf.BackgroundLumberjack, Stuf.BackgroundMartialDisciple, Stuf.BackgroundMendevianOrphan, Stuf.BackgroundMercenary, Stuf.BackgroundMiner, Stuf.BackgroundMugger, Stuf.BackgroundMwangianHunter, Stuf.BackgroundNexianScholar, Stuf.BackgroundNomad, Stuf.BackgroundOsirionHistorian, Stuf.BackgroundPickpocket, Stuf.BackgroundQadiranWanderer, Stuf.BackgroundRahadoumFaithless, Stuf.BackgroundRiverKingdomsDaredevil, Stuf.BackgroundsBaseSelection, Stuf.BackgroundsClericSpellLikeSelection, Stuf.BackgroundsCraftsmanSelection, Stuf.BackgroundsDruidSpellLikeSelection, Stuf.BackgroundShacklesCorsair, Stuf.BackgroundSmith, Stuf.BackgroundsNobleSelection, Stuf.BackgroundsOblateSelection, Stuf.BackgroundsRegionalSelection, Stuf.BackgroundsScholarSelection, Stuf.BackgroundsStreetUrchinSelection, Stuf.BackgroundsWandererSelection, Stuf.BackgroundsWarriorSelection, Stuf.BackgroundsWizardSpellLikeSelection, Stuf.BackgroundUstalavPeasant, Stuf.BackgroundVarisianExplorer, Stuf.BackgroundWarriorOfTheLinnormKings };
        /*}
          catch (Exception ex)
          {
              Main.logger.Log("Error while patching library");
              Main.logger.Log(ex.ToString());
          }*/
    }
}

namespace RespecWrath
{
	public static class Stuf
	{
		/*public static T Create<T>(Action<T> init = null) where T : ScriptableObject
		{
			var result = ScriptableObject.CreateInstance<T>();
			if (init != null) init(result);
			return result;
		}*/
		public static BlueprintFeature[] deityfeatures
		{
			get
			{
				if (!Main.haspatched) Main.PatchLibrary();
				return m_deityfeatures;
			}
			set
			{
				m_deityfeatures = value;
			}
		}
		public static BlueprintFeature[] m_deityfeatures;

		public static BlueprintFeature[] m_BackgroundFeatures;
		public static BlueprintFeature[] backgroundfeatures
		{
			get
            {
				if (!Main.haspatched) Main.PatchLibrary();
				return m_BackgroundFeatures;
            }
			set
            {
				m_BackgroundFeatures = value;
            }
		}
        /*public static T Get<T>(this LibraryScriptableObject library, String assetId) where T : BlueprintScriptableObject
		{
			return (T)ResourcesLibrary.TryGetBlueprint(assetId);
		}*/
        /*public static BlueprintUnit Arueshalae => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("a352873d37ec6c54c9fa8f6da3a6b3e1");
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
		public static BlueprintUnit Player => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("4391e8b9afbb0cf43aeba700c089f56d");*/

        /*public static BlueprintUnit CompanionBear => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("a207eff7953731b44acf1a3fa4354c2d");
		public static BlueprintUnit CompanionBoar => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("3eb6ad60c8b9fe34fafa32e1f429ff5b");
		public static BlueprintUnit CompanionCentipede => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9df16ffd0c8cec4d99a0ae6f025a3f8");
		public static BlueprintUnit CompanionDog => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("918939943bf32ba4a95470ea696c2ba5");
		public static BlueprintUnit CompanionElk => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("8e43d402ca1a2ad44ac9d2c9fe99f32c");
		public static BlueprintUnit CompanionHorse => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("fb8300e8298c08d4a9f50dfa1203e98d");
		public static BlueprintUnit CompanionLeopard => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54cf380dee486ff42b803174d1b9da1b");
		public static BlueprintUnit CompanionMammoth => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e7aa96d15a45238438ae4cfb476f6bb9");
		public static BlueprintUnit CompanionMonitor => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("57381165c3f4b4740a872e54f62c3a14");
		public static BlueprintUnit CompanionSmilodon => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9");
		public static BlueprintUnit CompanionTriceratops => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("51744ec49565c0340b11a1a6dac7920b");
		public static BlueprintUnit CompanionVelociraptor => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("28d1986d57a7081439fbb581aa6f960c");
		public static BlueprintUnit CompanionWolf => ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("eab864d9ca3415644a792792fd81bf87");*/
        public static BlueprintFeatureSelection DeitySelect => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

        //public static BlueprintScriptableObject TieflingHeritageSelect => ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>("c862fd0e4046d2d4d9702dd60474a181");
        public static BlueprintFeatureSelection BackgroundSelect => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");

        /*public static BlueprintFeatureSelection BackgroundSelectSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");
		public static BlueprintFeatureSelection BackgroundsWandererSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("0cdd576724fce2240b372455889fac87");
		public static BlueprintFeatureSelection BackgroundsWizardSpellLikeSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("1139a014bb6cdcf4db0e11649ddfa60c");
		public static BlueprintFeatureSelection BackgroundsScholarSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("273fab44409035f42a7e2af0858a463d");
		public static BlueprintFeatureSelection BackgroundsWarriorSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("291f372e27b29f149ad15ff219fe15d9");
		public static BlueprintFeatureSelection BackgroundsDruidSpellLikeSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("2fc911f0de029134585b5f35ff16be88");
		public static BlueprintFeatureSelection BackgroundsNobleSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("7b11f589e81617a46b3e5eda3632508d");
		public static BlueprintFeatureSelection BackgroundsClericSpellLikeSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("94f680dbbb083cc43962249e446a3e10");
		public static BlueprintFeatureSelection BackgroundsOblateSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c25021c31f302c6449ecdbc978822507");
		public static BlueprintFeatureSelection BackgroundsStreetUrchinSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e17f74060f864ff459393e11d5e7fe2f");
		public static BlueprintFeatureSelection BackgroundsBaseSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");
		public static BlueprintFeatureSelection BackgroundsRegionalSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("fa621a249cc836f4382ca413b976e65e");
		public static BlueprintFeatureSelection BackgroundsCraftsmanSelection => ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("fd36aaff6f41a2f4f9e91925d49a0d85");
		public static BlueprintFeature BackgroundHunter => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("dd715e89fceb7ad44b120a49e4395332");
		public static BlueprintFeature BackgroundHermit => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("aec583b2cf2305d4fa6a58d640a54f16");
		public static BlueprintFeature BackgroundNomad => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3335c5ea5b84e7f499498382d9dd33a5");

		public static BlueprintFeature AcidSplashBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("429741e7aac24764489cf22970241596");
		public static BlueprintFeature DazeBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("766b3afb932c48f43973f77bc6e17e9e");
		public static BlueprintFeature DisruptUndeadBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("45a538e8ab40a5d48966e8224578cc3b");
		public static BlueprintFeature FlareBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("dbad8a46a6278e24785e2b5b24c23de0");
		public static BlueprintFeature JoltBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("602c54b6823bd9e42a644ad4f1ad9ebe");
		public static BlueprintFeature MageLightBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("6e4cd4c19c1db674ca6e2635594927ae");
		public static BlueprintFeature RayOfFrostBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("f8f2076f829ea854381d7dbde3d05c43");
		public static BlueprintFeature ResistanceBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("5f94b6d19093f60499f7532f1b084b06");
		public static BlueprintFeature TouchOfFatigueBackgroundsFeature => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d972164e651ff7345800fc849c00855b");

		public static BlueprintFeature BackgroundGladiator => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("1ea754d5573a0ed4e9fca4c30519f247");
		public static BlueprintFeature BackgroundMercenary => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("43b7315eb58242943848750af3671a25");
		public static BlueprintFeature BackgroundGuard => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3a793809b0f11b74f8b79c252067bfe0");
		public static BlueprintFeature BackgroundBountyHunter => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("15a072cca47e2444ebfd178b71f4e797");

		public static BlueprintFeature BackgroundEmissary => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2aca424b9a5e1b1408078ac0a4ed7bc3");
		public static BlueprintFeature BackgroundLeader => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c4ec4d736067fa945b512a235b13030b");
		public static BlueprintFeature BackgroundCourtIntriguer => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d7f5d62db424d3c4bbfb178bbb0b4e22");

		public static BlueprintFeature BackgroundHealer => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("f0d2840b6564c6f408c1e068d0707ca0");
		public static BlueprintFeature BackgroundAcolyte => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7cc9014488caa5445a0c8b5e17b95466");
		public static BlueprintFeature BackgroundMartialDisciple => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("46d69a77c26701a459607c3f42e3664a");

		public static BlueprintFeature BackgroundAcrobat => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("621e1bb8e1d5e114da5a107351f5c7b3");
		public static BlueprintFeature BackgroundMugger => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("88453bd5448456748ab531cddad38721");
		public static BlueprintFeature BackgroundPickpocket => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9da9faf5f0ef4904db4a59a22dafbb06");

		public static BlueprintFeature BackgroundFarmhand => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9da9faf5f0ef4904db4a59a22dafbb06");
		public static BlueprintFeature BackgroundSmith => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9da9faf5f0ef4904db4a59a22dafbb06");
		public static BlueprintFeature BackgroundMiner => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9da9faf5f0ef4904db4a59a22dafbb06");
		public static BlueprintFeature BackgroundLumberjack => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9da9faf5f0ef4904db4a59a22dafbb06");

		public static BlueprintFeature BackgroundAldoriSwordsman => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ce0ea6b388ac467408d6da224fab403d");
		public static BlueprintFeature BackgroundVarisianExplorer => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("881e63a01d79ee445b96e888368b015d");
		public static BlueprintFeature BackgroundRiverKingdomsDaredevil => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c9cb4176176b4164ca3c0a65feda0226");
		public static BlueprintFeature BackgroundCheliaxianDiabolist => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("67ab276984c26a645a7472091779d514");
		public static BlueprintFeature BackgroundAndoranDiplomat => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("f6b2d489b1d907f47a734b5f72f5200d");
		public static BlueprintFeature BackgroundQadiranWanderer => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("410ac851e51173141a96e02cca78e613");
		public static BlueprintFeature BackgroundOsirionHistorian => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("759e9118b89068245afc0e25817aaf58");
		public static BlueprintFeature BackgroundMwangianHunter => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("15b454c99494b8143ade4817fff816cc");
		public static BlueprintFeature BackgroundNexianScholar => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("1206bb79daa23564a9d20021ca8674e5");
		public static BlueprintFeature BackgroundGebianNecromancer => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("25b4f7ff8723d6e498f9cdc5ef2fad57");
		public static BlueprintFeature BackgroundAlkenstarAlchemist => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("f51af2d4fa3358844879cbc5ee0f1073");
		public static BlueprintFeature BackgroundRahadoumFaithless => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("f99465e6886253744aaef25d9b7c90c1");
		public static BlueprintFeature BackgroundShacklesCorsair => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("25fa74e08bcc43c47816fa364f1d48b6");
		public static BlueprintFeature BackgroundUstalavPeasant => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("e633b595bfb776d48858c0c51a6414ff");
		public static BlueprintFeature BackgroundWarriorOfTheLinnormKings => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("494c972a8e7626749aeda25582e2e88f");
		public static BlueprintFeature BackgroundMendevianOrphan => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("1d27bef027bdecc42a80c950cdc11380");
		*/
        //public static BlueprintFeature Airborne => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("70cffb448c132fa409e49156d013b175");
        //	public static BlueprintRace HumanRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
        //public static BlueprintRace HalfElfRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
        //	public static BlueprintRace HalfOrcRace => ResourcesLibrary.TryGetBlueprint<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");

        /*public static BlueprintFeature ArueshalaeFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7993c81bd04ffda4bac123eb7f6752c4");
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
		public static BlueprintFeature TreverFeatureList => ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3552b16bcfb64944c8b5825661fa7b90");*/
    }
}