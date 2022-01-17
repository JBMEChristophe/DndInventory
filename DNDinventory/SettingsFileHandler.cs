using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;
using static DNDinventory.UpdatePolicy;
using Utilities;
using System.ComponentModel;

namespace DNDinventory
{
    public enum DebugSetting
    {
        [Description("None")]
        None,
        [Description("Reduced Item Loading")]
        ReducedItemLoading
    }

    public class Settings 
    {
        // Set defaults here
        public string Host = "localhost";
        public string Port = "100";
        public string OutputFolder = "Transfers";
        public string ImagesFolder = "Images";
        public string CatalogFolder = "Catalogs";
        public string InventoriesFolder = "Inventories";
        public DebugSetting Debug = DebugSetting.None;

        public override string ToString()
        {
            return $"host: {Host}; port: {Port}; outputFolder:{OutputFolder}; ImagesFolder:{ImagesFolder}; CatalogFolder:{CatalogFolder}; InventoriesFolder:{InventoriesFolder}; Debug:{EnumHelper.GetDescriptionFromEnumValue(Debug)}";
        }
    }

    public class SettingsFileHandler
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings currentSettings;

        public SettingsFileHandler ()
        {
            logger.Info($"> SettingsFileHandler()");
            currentSettings = new Settings();
            logger.Info($"< SettingsFileHandler()");
        }
        
        public void WriteToXml (string filename)
        {
            logger.Info($"> WriteToXml(filename: {filename})");
            XmlHelper<Settings>.WriteToXml(filename, currentSettings);
            logger.Info($"< WriteToXml(filename: {filename})");
        }

        public void ReadFromXml (string filename)
        {
            logger.Info($"> ReadFromXml(filename: {filename})");
            try
            {
                var settings = XmlHelper<Settings>.ReadFromXml(filename);
                if (settings != null)
                {
                    logger.Debug("Set current settings");
                    logger.Trace($"new settings: [{settings}]");
                    currentSettings = settings;
                }
            }
            catch(System.InvalidOperationException)
            {
                logger.Debug("Trying SettingsV0");
                //try older save file
                var settings = XmlHelper<SettingsV0>.ReadFromXml(filename);
                if (settings != null)
                {
                    logger.Debug("Set current settings by using SettingsV0");
                    currentSettings = new Settings { Host = settings.host, Port = settings.port, OutputFolder = settings.outputFolder };
                    logger.Trace($"new settings: [{currentSettings}]");
                    WriteToXml(filename);
                }
            }
            logger.Info($"< ReadFromXml(filename: {filename})");
        }
    }
}
