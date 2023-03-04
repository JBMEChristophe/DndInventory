using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InventoryControlLib.Model
{
    class ExtendedBorder : Border
    {
        public int CellY { get; set; }
        public int CellX { get; set; }
    }
}
