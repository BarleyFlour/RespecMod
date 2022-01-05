/*namespace RespecModBarley
{
	[HarmonyPatch(typeof(UnitHelper), "RespecOnCommit")]
	internal static class Unithelper_RespecOnCommit_Patch
	{
		private static void Postfix(UnitEntityData targetUnit)
        {
			if(Main.IsRespec == false)
            {
				return;
            }
			if (Main.IsRespec == true)
			{
				try
				{
					Main.featurestoadd.Clear();
					Main.IsRespec = false;
					targetUnit.Progression.AdvanceMythicExperience(Main.MythicXP);
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(targetUnit);
							///part.TurnOn();
							targetUnit.Parts.m_Parts.Add(part);
						}
					}
					/*foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.Parts.Contains(part))
						{
							part.AttachToEntity(tempUnit);
							tempUnit.Parts.m_Parts.Add(part);
						}
					}
					foreach (EntityPart part in Main.partstoadd)
					{
						if (!targetUnit.Parts.m_Parts.Contains(part))
						{
							///part.AttachToEntity(unit);
							targetUnit.Parts.m_Parts.Add(part);
						}
					}
					Main.partstoadd.Clear();
					Main.EntityUnit = null;
					foreach (EntityPart entityPart in targetUnit.Parts.m_Parts)
					{
						targetUnit.OnPartAddedOrPostLoad(entityPart);
					}
					if (Main.NenioEtudeBool == true)
					{
						var KitsuneHeritageClassic = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cd6cd774fb7cc844b8417193ee3a5ebe");
						var KitsuneHeritageKeen = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("d6bc49651fbaa2944bba6e2e5a1720ff");
						var facts = new List<BlueprintUnitFact> { KitsuneHeritageClassic, KitsuneHeritageKeen };
						foreach (IHiddenUnitFacts i in targetUnit.Parts.Get<UnitPartHiddenFacts>().m_HiddenFacts)
						{
							foreach (BlueprintUnitFact fact in facts)
							{
								i.Facts.Add(fact);
							}
						}
						Main.NenioEtudeBool = false;
					}
				}
				catch(Exception e) { Main.logger.Log(e.ToString()); }
			}
		}
	}
}*/