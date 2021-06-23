using InventoryControlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.ViewModel
{
    public class InventorySelectViewModel
    {
        public List<UpdateGrid> Inventories
        {
            get
            {
                return GridManager.Instance.Grids;
            }
        }

        public Guid SelectedGridId { get; set; }

        public void Init()
        {
            SelectedGridId = GridManager.Instance.GroundId;
        }
    }
}
