using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace RespecModBarley
{
    public class Settings : UnityModManager.ModSettings
    {
        public int PointsCount;
        public bool OriginalStats;
        public bool BackgroundDeity;
        public bool PreserveMCName = true;
        public bool PreserveMCBirthday = true;
        public bool PreserveMCAlignment = true;
        public bool PreservePortrait = true;
        //public bool PreserveMCRace = true;
        public bool PreserveVoice = true;
        public bool FreeRespec = false;
        public bool FullRespecStoryCompanion = false;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            var filepath = Path.Combine(modEntry.Path, "Settings.json");
            try
            {

                JsonSerializer serializer = new JsonSerializer();
#if (DEBUG)
                serializer.Formatting = Formatting.Indented;
#endif
                using (StreamWriter sw = new StreamWriter(filepath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.Error(ex.ToString());
            }
        }
        public static Settings Load(ModEntry modEntry)
        {
            var filepath = Path.Combine(modEntry.Path, "Settings.json");
            if (File.Exists(filepath))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader sr = new StreamReader(filepath))
                    using (JsonTextReader reader = new JsonTextReader(sr))
                    {
                        Settings result = serializer.Deserialize<Settings>(reader);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    modEntry.Logger.Error($"Can't read {filepath}.");
                    modEntry.Logger.Error(ex.ToString());
                }
            }
            return new Settings();
        }
    }
}
