using System.Reflection;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SaveGameProfiles {
    [StaticConstructorOnStartup]
    public class Main {
        public static MethodInfo FileNameColorMethod;
        public static MethodInfo DoFileInteractionMethod;
        public static MethodInfo ReloadFilesMethod;
        public static MethodInfo DoTypeInFieldMethod;

        public static FieldInfo filesField;
        public static FieldInfo bottomAreaHeightField;
        public static FieldInfo scrollPositionField;
        public static FieldInfo interactButLabelField;
        public static FieldInfo shouldDoTypeInFieldField;

        public static Texture2D TexButton_DeleteX;

        private const string LOG_HEADER = "[SaveGameProfiles]: ";
        static Main() {
            LogMessage("Loading SaveGameProfiles");
            var harmony = HarmonyInstance.Create("com.github.bc.rimworld.mod.saveGameProfiles");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SaveProfileManager.LoadSaveFiles();
            SaveProfileManager.LoadSaveProfiles();

            FileNameColorMethod = typeof(Dialog_FileList).GetMethod("FileNameColor",
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[] { typeof(SaveFileInfo) }, null);
            DoFileInteractionMethod = typeof(Dialog_FileList).GetMethod("DoFileInteraction",
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[] { typeof(string) }, null);
            ReloadFilesMethod = typeof(Dialog_FileList).GetMethod("ReloadFiles",
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[] { }, null);
            DoTypeInFieldMethod = typeof(Dialog_FileList).GetMethod("DoTypeInField",
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[] { typeof(Rect) }, null);

            filesField = typeof(Dialog_FileList).GetField("files",
                BindingFlags.Instance | BindingFlags.NonPublic);
            bottomAreaHeightField = typeof(Dialog_FileList).GetField("bottomAreaHeight",
                BindingFlags.Instance | BindingFlags.NonPublic);
            scrollPositionField = typeof(Dialog_FileList).GetField("scrollPosition",
                BindingFlags.Instance | BindingFlags.NonPublic);
            interactButLabelField = typeof(Dialog_FileList).GetField("interactButLabel",
                BindingFlags.Instance | BindingFlags.NonPublic);
            shouldDoTypeInFieldField = typeof(Dialog_FileList).GetField("ShouldDoTypeInField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            TexButton_DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
        }

        public static void LogMessage(string text) {
            Log.Message(LOG_HEADER + text);
        }

        public static void LogError(string text) {
            Log.Error(LOG_HEADER + text);
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList_Load), "DoFileInteraction")]
    public class Dialog_SaveFileList_Load_Patch {
        public static bool Prefix(ref Dialog_SaveFileList_Load __instance, ref string saveFileName) {
            Main.LogMessage("DoFileInteraction load onfile: " + saveFileName);
            return true;
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList_Save), "DoFileInteraction")]
    public class Dialog_SaveFileList_Save_Patch {
        public static bool Prefix(ref Dialog_SaveFileList_Save __instance, ref string mapName) {
            Main.LogMessage("DoFileInteraction save on file: " + mapName);
            return true;
        }
    }

    [HarmonyPatch(typeof(Dialog_FileList), "DoWindowContents")]
    public class Dialog_FileList_WindowContents_Patch {
        public static bool Prefix(ref Dialog_FileList __instance, ref Rect inRect) {
            return true;
        }

        public static void Postfix(ref Dialog_FileList __instance, ref Rect inRect) {
            Window curWindow = Find.WindowStack.currentlyDrawnWindow;
            Rect position = inRect.ContractedBy(1f);
            GUI.BeginGroup(position);

            GUI.color = Color.white;
            Rect buttonRect = new Rect(0f, 0f, position.width - 15f, 36f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            Widgets.Label(buttonRect, "Testing 123");

            // TODO: Figure out Translate call
            if (Widgets.ButtonText(buttonRect, "Select Save Profile", true, false, true)) {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (SaveProfile saveProfile in SaveProfileManager.SaveProfiles.Values) {
                    list.Add(new FloatMenuOption(saveProfile.Name, delegate {
                        SaveProfileManager.CurrentProfile = saveProfile;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }

                    list.Add(new FloatMenuOption("Create Save Profile", delegate {
                        Find.WindowStack.Add(new Dialog_NameProfile());
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                Find.WindowStack.Add(new FloatMenu(list));
            }

            GUI.EndGroup();
        }
    }

    /*
    [HarmonyPatch(typeof(Dialog_FileList), "DoMainMenuControls")]
    public class MainMenuControls_Patch {
        public static bool Prefix(ref Dialog_FileList __instance, ref Rect inRect) {
            if (Current.)
        }
    }
    */

    /*
    [HarmonyPatch(typeof(Dialog_FileList), "DoWindowContents")]
    public class Dialog_FileList_Patch {
        public static bool Prefix(ref Dialog_FileList __instance, ref Rect inRect) {
            __instance.DoWindowContents
        }

        public static bool Prefix(ref Dialog_FileList __instance, ref Rect inRect) {
            List<SaveFileInfo> files = (List<SaveFileInfo>)Main.filesField.GetValue(__instance);
            Vector2 vector = new Vector2((float)(inRect.width - 16.0), 36f);
            Vector2 vector2 = new Vector2(100f, (float)(vector.y - 2.0));
            inRect.height -= 45f;
            float num = (float)(vector.y + 3.0);
            float height = (float)files.Count * num;
            Rect viewRect = new Rect(0f, 0f, (float)(inRect.width - 16.0), height);
            Rect outRect = new Rect(inRect.AtZero());
            // outRect.height -= __instance.bottomAreaHeight;
            outRect.height -= (float)Main.bottomAreaHeightField.GetValue(__instance);
            // Widgets.BeginScrollView(outRect, ref __instance.scrollPosition, viewRect, true);
            Vector2 scrollPosition = (Vector2)Main.scrollPositionField.GetValue(__instance);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            float num2 = 0f;
            int num3 = 0;
            foreach (SaveFileInfo file in files) {
                Rect rect = new Rect(0f, num2, vector.x, vector.y);
                if (num3 % 2 == 0) {
                    Widgets.DrawAltRect(rect);
                }
                Rect position = rect.ContractedBy(1f);
                GUI.BeginGroup(position);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileInfo.Name);
                // GUI.color = __instance.FileNameColor(file);
                GUI.color = (Color)Main.FileNameColorMethod.Invoke(__instance, new object[] { file });
                Rect rect2 = new Rect(15f, 0f, position.width, position.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(rect2, fileNameWithoutExtension);
                GUI.color = Color.white;
                Rect rect3 = new Rect(270f, 0f, 200f, position.height);
                Dialog_FileList.DrawDateAndVersion(file, rect3);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                float num4 = (float)(vector.x - 2.0 - vector2.x - vector2.y);
                Rect rect4 = new Rect(num4, 0f, vector2.x, vector2.y);
                // if (Widgets.ButtonText(rect4, __instance.interactButLabel, true, false, true)) {
                if (Widgets.ButtonText(rect4, (string)Main.interactButLabelField.GetValue(__instance), true, false, true)) {
                    // __instance.DoFileInteraction(Path.GetFileNameWithoutExtension(file.FileInfo.Name));
                    Main.DoFileInteractionMethod.Invoke(__instance, new object[] { Path.GetFileNameWithoutExtension(file.FileInfo.Name) });
                }
                Rect rect5 = new Rect((float)(num4 + vector2.x + 5.0), 0f, vector2.y, vector2.y);
                // if (Widgets.ButtonImage(rect5, TexButton.DeleteX)) {

                // Workaround since ref parameters can't be used inside delegates
                object instance = __instance;
                if (Widgets.ButtonImage(rect5, Main.TexButton_DeleteX)) {
                    FileInfo localFile = file.FileInfo;
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate {
                        localFile.Delete();
                        // __instance.ReloadFiles();
                        Main.ReloadFilesMethod.Invoke(instance, new object[] { });
                    }, true, null));
                }
                TooltipHandler.TipRegion(rect5, "DeleteThisSavegame".Translate());
                GUI.EndGroup();
                num2 = (float)(num2 + (vector.y + 3.0));
                num3++;
            }
            Widgets.EndScrollView();
            // if (__instance.ShouldDoTypeInField) {
            if ((bool)Main.shouldDoTypeInFieldField.GetValue(__instance)) {
                // __instance.DoTypeInField(inRect.AtZero());
                Main.DoTypeInFieldMethod.Invoke(__instance, new object[] { inRect.AtZero() });
            }

            return false;
        }
    }
        */
}
