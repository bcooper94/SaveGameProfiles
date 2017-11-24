using RimWorld;
using UnityEngine;
using Verse;

namespace SaveGameProfiles {
    public class Dialog_NameProfile : Window {
        private string curName;

        public override void DoWindowContents(Rect rect) {
            Text.Font = GameFont.Small;
            bool flag = false;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
                flag = true;
                Event.current.Use();
            }

            Rect rect2;

            Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "Enter a save profile name");
            this.curName = Widgets.TextField(new Rect(0f, (float)(rect.height - 35.0), (float)(rect.width / 2.0 - 20.0), 35f), this.curName);
            rect2 = new Rect((float)(rect.width / 2.0 + 20.0), (float)(rect.height - 35.0), (float)(rect.width / 2.0 - 20.0), 35f);

            if (!Widgets.ButtonText(rect2, "OK".Translate(), true, false, true) && !flag)
                return;
            if (SaveProfileManager.CreateSaveProfile(this.curName)) {
                Find.WindowStack.TryRemove(this, true);
            } else {
                Messages.Message("Invalid profile name", MessageTypeDefOf.RejectInput);
            }

            Event.current.Use();
        }
    }
}
