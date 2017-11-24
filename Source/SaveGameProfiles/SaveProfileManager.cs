using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using Verse;

namespace SaveGameProfiles {
    public class SaveProfileManager {
        private const string DEFAULT_PROFILE_PATH = "SaveProfiles.xml";
        private const string DEFAULT_PROFILE = "Default";

        private const string SAVE_PROFILE_LIST_ATTR = "SaveProfiles";
        private const string SAVE_PROFILE_ATTR = "SaveProfile";

        private static SaveProfile currentProfile;

        public static SaveProfile CurrentProfile {
            get { return currentProfile; }
            set {
                Main.LogMessage("Setting current profile to " + value.Name);
                currentProfile = value;
            }
        }

        private static Dictionary<string, SaveProfile> saveProfiles;
        private static List<SaveFileInfo> allSaveFiles;
        private static XmlReader reader = null;
        private static XmlWriter writer = null;

        public static Dictionary<string, SaveProfile> SaveProfiles {
            get { return saveProfiles; }
        }

        public static bool CreateSaveProfile(string name) {
            bool success = false;

            if (name != null) {
                string trimmedName = name.Trim(new char[] { ' ', '\n' });

                if (IsNameValid(trimmedName) && saveProfiles.ContainsKey(trimmedName)) {
                    saveProfiles.Add(name, new SaveProfile(name));
                    success = true;
                    string fullPath = Path.GetFullPath(DEFAULT_PROFILE_PATH).ToString();
                    SaveProfileFile(fullPath);
                }
            }

            return success;
        }

        public static void LoadSaveFiles() {
            allSaveFiles = new List<SaveFileInfo>();

            foreach (FileInfo allSavedGameFile in GenFilePaths.AllSavedGameFiles) {
                try {
                    SaveFileInfo saveFile = new SaveFileInfo(allSavedGameFile);
                    allSaveFiles.Add(saveFile);
                    Main.LogMessage("Loaded save file: " + saveFile.FileInfo.FullName);
                } catch (Exception ex) {
                    Log.Error("Exception loading " + allSavedGameFile.Name + ": " + ex.ToString());
                }
            }
        }

        public static void LoadSaveProfiles() {
            saveProfiles = new Dictionary<string, SaveProfile>();
            string fullPath = Path.GetFullPath(DEFAULT_PROFILE_PATH).ToString();
            Main.LogMessage("Profile file location: " + fullPath);

            try {
                Main.LogMessage("Checking if profiles file exists...");
                if (File.Exists(fullPath)) {
                    Main.LogMessage("Found SaveProfiles.xml");
                    DoReadSaveProfiles(fullPath);
                } else {
                    Main.LogMessage("No save profile configuration file found. Creating default profile.");
                    saveProfiles.Add(DEFAULT_PROFILE, new SaveProfile(DEFAULT_PROFILE));
                    SaveProfileFile(fullPath);
                }
            } catch (Exception ex) {
                Main.LogError("Failed to check if SaveProfiles.xml exists: " + ex.Message);
            }
        }

        private static void DoReadSaveProfiles(string filePath) {
            FileStream profileStream = null;

            try {
                profileStream = new FileStream(filePath,
                    FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                reader = XmlReader.Create(profileStream);
                reader.MoveToContent();
                List<XElement> profileListElements = new List<XElement>(ReadElements(SAVE_PROFILE_LIST_ATTR));
                Main.LogMessage("Found " + profileListElements.Count + " " + SAVE_PROFILE_LIST_ATTR + " elements");

                foreach (XElement profileListElement in profileListElements) {
                    List<XElement> profileElements = new List<XElement>(profileListElement.Elements());
                    Main.LogMessage("Found " + profileElements.Count + " " + SAVE_PROFILE_ATTR + " elements");

                    foreach (XElement profileElement in profileElements) {
                        SaveProfile profile = new SaveProfile(profileElement);
                        Main.LogMessage(profile.ToString());
                        saveProfiles.Add(profile.Name, profile);
                    }
                }
            } catch (IOException ex) {
                Main.LogError("Failed to read save profiles: " + ex.Message);
            } finally {
                try {
                    if (reader != null) {
                        reader.Close();
                    }
                } catch (IOException ex) {
                    Main.LogError("Failed to close XmlReader: " + ex.Message);
                }
                try {
                    if (profileStream != null) {
                        profileStream.Close();
                    }
                } catch (IOException ex) {
                    Main.LogError("Failed to close profiles FileStream: " + ex.Message);
                }
            }

        }

        // Credit: https://stackoverflow.com/questions/2441673/reading-xml-with-xmlreader-in-c-sharp#answer-19165632
        private static IEnumerable<XElement> ReadElements(string elementName) {
            while (!reader.EOF && reader.ReadState == ReadState.Interactive) {
                // corrected for bug noted by Wes below...
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName)) {
                    // this advances the reader...so it's either XNode.ReadFrom() or reader.Read(), but not both
                    XElement matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                        yield return matchedElement;
                } else {
                    reader.Read();
                }
            }
        }

        private static void SaveProfileFile(string filePath) {
            Main.LogMessage("Creating XmlDocument from save profiles...");
            FileStream profileStream = null;

            if (saveProfiles != null && saveProfiles.Count > 0) {
                Main.LogMessage("Created SaveProfiles element. Appending SaveProfile elements...");
                List<XElement> saveProfileElements = new List<XElement>(saveProfiles.Count);

                foreach (SaveProfile profile in saveProfiles.Values) {
                    saveProfileElements.Add(profile.ToXElement());
                }

                try {
                    Main.LogMessage("Saving SaveProfiles.xml...");
                    profileStream = new FileStream(filePath,
                        FileMode.Create, FileAccess.Write, FileShare.None);
                    XElement document = XElement.Parse("<SaveProfiles></SaveProfiles>");
                    document.Add(saveProfileElements);
                    writer = XmlWriter.Create(profileStream);
                    document.WriteTo(writer);
                } catch (IOException ex) {
                    Main.LogError("Failed to save profiles: " + ex.Message);
                } finally {
                    try {
                        if (writer != null) {
                            writer.Close();
                        }
                    } catch (IOException ex) {
                        Main.LogError("Failed to close XmlWriter: " + ex.Message);
                    }
                    try {
                        if (profileStream != null) {
                            profileStream.Close();
                        }
                    } catch (IOException ex) {
                        Main.LogError("Failed to close output FileStream");
                    }
                }
            } else {
                Main.LogError("Base SaveProfiles element was null");
            }

        }

        private static bool IsNameValid(string name) {
            return !name.NullOrEmpty() && !name.Contains("<") && !name.Contains(">") && !name.Contains("\\");
        }
    }

    public class SaveProfile {
        private string name;
        private List<string> saveFiles;

        public SaveProfile() {
            this.saveFiles = new List<string>();
        }

        public SaveProfile(string name) {
            this.name = name;
            this.saveFiles = new List<string>();
        }

        public SaveProfile(string name, List<string> saveFiles) {
            this.name = name;
            this.saveFiles = saveFiles;
        }

        public SaveProfile(XElement element) {
            XElement nameElement = element.Element("Name");
            XElement saveFileElements = element.Element("SaveFiles");
            this.saveFiles = new List<string>();

            if (nameElement != null) {
                this.name = nameElement.Value;
            } else {
                throw new InvalidDataException("Save profiles must have a name");
            }

            if (saveFileElements != null) {
                if (saveFileElements.HasElements) {
                    foreach (XElement saveFileElement in saveFileElements.Descendants()) {
                        this.saveFiles.Add(saveFileElement.Value);
                    }
                }
            }
        }

        public string Name {
            get { return this.name; }
        }

        public List<string> SaveFiles {
            get { return new List<string>(this.saveFiles); }
        }

        public XElement ToXElement() {
            string saveFileXmlString = "";

            foreach (string file in this.saveFiles) {
                saveFileXmlString += "<SaveFile>" + file + "</SaveFile>";
            }

            return XElement.Parse("<SaveProfile><Name>" + name + "</Name>"
                + "<SaveFiles>" + saveFileXmlString + "</SaveFiles>" + "</SaveProfile>");
        }

        public XmlNode ToXmlNode() {
            return new XmlDocument().CreateElement("SaveProfile");
        }

        public void AddSaveFile(string file) {
            this.saveFiles.Add(file);
        }

        public override string ToString() {
            return "{SaveProfile name=\"" + name + "\", saveFiles=["
                + (saveFiles != null ? String.Join(", ", saveFiles.ToArray()) : "") + "]}";
        }

        public override bool Equals(object obj) {
            return obj is SaveProfile && ((SaveProfile)obj).name == this.name;
        }

        public override int GetHashCode() {
            return name != null ? name.GetHashCode() : base.GetHashCode();
        }
    }
}
