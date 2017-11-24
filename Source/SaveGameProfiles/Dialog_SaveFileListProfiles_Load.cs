using RimWorld;
using UnityEngine;
using Verse;

namespace SaveGameProfiles {
    public class Dialog_SaveFileListProfiles_Load : Dialog_SaveFileList_Load {
        public override void DoWindowContents(Rect inRect) {
            Main.LogMessage("DoWindowContents called for Dialog_SaveFileListProfiles_Load");
            base.DoWindowContents(inRect);
        }
    }

    public class Dialog_SaveFileListProfiles_Save : Dialog_SaveFileList {
        public override void DoWindowContents(Rect inRect) {
            Main.LogMessage("DoWindowContents called for Dialog_SaveFileListProfiles_Save");
            base.DoWindowContents(inRect);
        }

        protected override bool ShouldDoTypeInField {
            get {
                return true;
            }
        }

        public Dialog_SaveFileListProfiles_Save() {
            base.interactButLabel = "OverwriteButton".Translate();
            base.bottomAreaHeight = 85f;
            if (Faction.OfPlayer.HasName) {
                base.typingName = Faction.OfPlayer.Name;
            } else {
                base.typingName = SaveGameFilesUtility.UnusedDefaultFileName(Faction.OfPlayer.def.LabelCap);
            }
        }

        protected override void DoFileInteraction(string mapName) {
            LongEventHandler.QueueLongEvent(delegate {
                GameDataSaveLoader.SaveGame(mapName);
            }, "SavingLongEvent", false, null);
            Messages.Message("SavedAs".Translate(mapName), MessageTypeDefOf.SilentInput);
            PlayerKnowledgeDatabase.Save();
            this.Close(true);
        }

    }
}
