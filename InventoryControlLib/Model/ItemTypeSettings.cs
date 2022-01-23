using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryControlLib.Model
{
    public class ItemTypeSetting
    {
        public ItemType Type { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }

        public ItemTypeSetting(ItemType type, int rowSpan = 1, int columnSpan = 1)
        {
            Type = type;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
        }
        public ItemTypeSetting()
        {}
    }

    public class ItemTypeSettingManager
    {
        public List<ItemTypeSetting> Settings { get; set; }

        public ItemTypeSettingManager()
        {
            Settings = new List<ItemTypeSetting>();
        }

        public void LoadOrCreateAndSaveDefault(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Load(path);
            }
            else
            {
                generateDefaultSettings();
                Save(path);
            }
        }

        public void Save(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Utilities.XmlHelper<List<ItemTypeSetting>>.WriteToXml(path, Settings);
            }
        }

        public void Load(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Settings = Utilities.XmlHelper<List<ItemTypeSetting>>.ReadFromXml(path);
            }
        }

        public ItemTypeSetting GetSetting(ItemType type)
        {
            return Settings.Where(i => i.Type == type).FirstOrDefault();
        }

        private void generateDefaultSettings()
        {
            foreach (ItemType suit in (ItemType[])Enum.GetValues(typeof(ItemType)))
            {
                ItemTypeSetting setting = new ItemTypeSetting(suit);
                switch (suit)
                {
                    case ItemType.ArtisansTools:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.Treasure:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.SimpleWeapon:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.MartialWeapon:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.MeleeWeapon:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.RangedWeapon:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.Staff:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.Mount:
                        setting.RowSpan = 3;
                        setting.ColumnSpan = 3;
                        break;
                    case ItemType.Instrument:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.Vehicle:
                        setting.RowSpan = 5;
                        setting.ColumnSpan = 5;
                        break;
                    case ItemType.HeavyArmor:
                        setting.RowSpan = 3;
                        setting.ColumnSpan = 3;
                        break;
                    case ItemType.MediumArmor:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.LightArmor:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.Shield:
                        setting.RowSpan = 3;
                        setting.ColumnSpan = 3;
                        break;
                    case ItemType.Armor:
                        setting.RowSpan = 2;
                        setting.ColumnSpan = 2;
                        break;
                    case ItemType.Rod:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.Wand:
                        setting.RowSpan = 2;
                        break;
                    case ItemType.Weapon:
                        setting.RowSpan = 2;
                        break;
                    default:
                        break;
                }
                Settings.Add(setting);
            }
        }
    }
}
