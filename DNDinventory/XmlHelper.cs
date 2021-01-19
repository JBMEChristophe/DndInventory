using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DNDinventory
{
    public static class XmlHelper<T>
    {
        static XmlSerializer ser = new XmlSerializer(typeof(T));

        public static void WriteToXml(string filename, T o)
        {
            using (var writer = new StreamWriter(filename))
            {
                ser.Serialize(writer, o);
            }
        }

        public static T ReadFromXml(string filename)
        {
            if (File.Exists(filename))
            {
                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    try
                    {
                        return (T)ser.Deserialize(fs);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return default(T);
        }
    }
}
