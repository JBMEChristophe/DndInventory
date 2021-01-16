using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace DNDinventory {
    public class SavedSettings {
        // Set defaults here
        public string host = "localhost";
        public string port = "100";
        public string outputFolder = "Transfers";
    }

    public class SettingsFileHandler {
        public SavedSettings currentSettings = new SavedSettings();
        XmlSerializer ser = new XmlSerializer(typeof(SavedSettings));
        
        public void WriteToXml (string filename) {
            TextWriter writer = new StreamWriter(filename);
            ser.Serialize(writer, currentSettings);
            writer.Close();
        }

        public void ReadFromXml (string filename) {
            if (File.Exists(filename)) {
                FileStream fs = new FileStream(filename, FileMode.Open);
                try {
                    currentSettings = (SavedSettings) ser.Deserialize(fs);
                } catch (Exception) { } //#TODO show 'file failed to load' error
                fs.Close();
            }
        }
    }
}
