using System.Reflection;
using Harmony;
using Verse;

namespace SaveGameProfiles {
    [StaticConstructorOnStartup]
    public class Main {
        private const string LOG_HEADER = "[SaveGameProfiles]: ";
        static Main() {
            LogMessage("Loading SaveGameProfiles");
            var harmony = HarmonyInstance.Create("com.github.bc.rimworld.mod.saveGameProfiles");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SaveProfileManager.LoadSaveProfiles();
        }

        public static void LogMessage(string text) {
            Log.Message(LOG_HEADER + text);
        }

        public static void LogError(string text) {
            Log.Error(LOG_HEADER + text);
        }
    }
}
