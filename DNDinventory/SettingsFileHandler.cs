using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace DNDinventory 
{
    public class SavedSettings 
    {
        // Set defaults here
        public string host = "localhost";
        public string port = "100";
        public string outputFolder = "Transfers";
    }

    public class SettingsFileHandler 
    {
        public SavedSettings currentSettings;
        XmlSerializer ser;

        public SettingsFileHandler () 
        {
            currentSettings = new SavedSettings();
            ser = new XmlSerializer(typeof(SavedSettings));
        }
        
        public void WriteToXml (string filename) 
        {
            using (var writer = new StreamWriter(filename))
            {
                ser.Serialize(writer, currentSettings);
            }
        }

        public void ReadFromXml (string filename)   
        {
            if (File.Exists(filename)) 
            {
                using (var fs = new FileStream(filename, FileMode.Open)) 
                {
                    currentSettings = (SavedSettings) ser.Deserialize(fs);
                }
            }
        }
    }
}
