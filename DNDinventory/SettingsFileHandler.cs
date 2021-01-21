using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;
using static DNDinventory.UpdatePolicy;

namespace DNDinventory 
{
    public class Settings 
    {
        // Set defaults here
        public string host = "localhost";
        public string port = "100";
        public string outputFolder = "Transfers";
    }

    public class SettingsFileHandler 
    {
        public Settings currentSettings;
        XmlSerializer ser;

        public SettingsFileHandler () 
        {
            currentSettings = new Settings();
            ser = new XmlSerializer(typeof(Settings));
        }
        
        public void WriteToXml (string filename) 
        {
            XmlHelper<Settings>.WriteToXml(filename, currentSettings);
        }

        public void ReadFromXml (string filename)   
        {
            try
            {
                var settings = XmlHelper<Settings>.ReadFromXml(filename);
                if (settings != null)
                {
                    currentSettings = settings;
                }
            }
            catch(System.InvalidOperationException)
            {
                //try older save file
                var settings = XmlHelper<SettingsV0>.ReadFromXml(filename);
                if (settings != null)
                {
                    currentSettings = new Settings { host = settings.host, port = settings.port, outputFolder = settings.outputFolder };
                    WriteToXml(filename);
                }
            }
        }
    }
}
