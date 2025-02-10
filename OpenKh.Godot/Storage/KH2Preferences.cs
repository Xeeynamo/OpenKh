using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Godot.Helpers;
using OpenKh.Kh2;
using OpenKh.Kh2.SystemData;

namespace OpenKh.Godot.Storage
{
    public static class KH2Preferences
    {
        public static readonly Cmd Commands;
        
        //PREF
        public static readonly List<Plyr> PlayerPreferences;
        public static readonly List<Fmab> FormAbilitiesPreferences;
        public static readonly List<Prty> PartyPreferences;
        public static readonly List<Sstm> SystemPreferences;
        public static readonly List<Magi> MagicPreferences;

        static KH2Preferences()
        {
            var systemData = Bar.Read(new MemoryStream(PackFileSystem.Open(Game.Kh2, "03system.bin").OriginalData));

            var preferences = systemData.FirstOrDefault(i => i.Name.Equals("pref", System.StringComparison.OrdinalIgnoreCase));
            if (preferences is not null)
            {
                var preferencesBar = Bar.Read(preferences.Stream);
                //TODO
                
                preferencesBar.PrintEntries();
            }
        }
    }
}
