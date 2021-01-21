using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DNDinventory
{
    public class UpdatePolicy
    {
        // Prior to Version 0.1.0
        [XmlRoot(ElementName = "SavedSettings")]
        public class SettingsV0
        {
            // Set defaults here
            public string host = "localhost";
            public string port = "100";
            public string outputFolder = "Transfers";
        }
    }
}
