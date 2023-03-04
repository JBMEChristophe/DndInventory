using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DNDinventory.Model
{
    struct DefaultInventorySettings
    {
        public DefaultInventorySettings(Size size, bool edit, bool delete)
        {
            Size = size;
            EditRights = edit;
            DeleteRights = delete;
        }

        public Size Size;
        public bool EditRights;
        public bool DeleteRights;
    }
}
