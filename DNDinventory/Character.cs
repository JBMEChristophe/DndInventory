using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory
{
    public class Character
    {
        public String name { get; private set; }
        public String gender { get; private set; }
        public DndRace dndRace { get; private set; }
        public DndClass dndClass { get; private set; }

        public Character(String name, string gender, DndRace dndRace, DndClass dndClass)
        {
            this.name = name;
            this.gender = gender;
            this.dndRace = dndRace;
            this.dndClass = dndClass;
        }
    }
}
