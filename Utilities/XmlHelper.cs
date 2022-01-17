using System;
using System.IO;
using System.Xml.Serialization;

namespace Utilities
{
    public static class XmlHelper<T>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static XmlSerializer ser = new XmlSerializer(typeof(T));

        public static void WriteToXml(string filename, T o)
        {
            logger.Info($"> WriteToXml(filename: {filename})");

            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(filename))
            {
                ser.Serialize(writer, o);
            }
            logger.Info($"< WriteToXml(filename: {filename})");
        }

        public static T ReadFromXml(string filename)
        {
            logger.Info($"> ReadFromXml(filename: {filename})");
            if (File.Exists(filename))
            {
                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    try
                    {
                        var result = (T)ser.Deserialize(fs);
                        logger.Info($"< ReadFromXml(filename: {filename}).return({result})");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Whoeps, something went wrong");
                        throw ex;
                    }
                }
            }
            var defaultResult = default(T);
            logger.Info($"< ReadFromXml(filename: {filename}).return({defaultResult})");
            return defaultResult;
        }
    }
}
