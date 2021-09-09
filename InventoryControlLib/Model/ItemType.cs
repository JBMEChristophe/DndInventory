using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryControlLib.Model
{
    public enum ItemType
    {
        [Description("Adventuring Gear")]
        AdventuringGear,
        [Description("Ammunition")]
        Ammunition,
        [Description("Generic Variant")]
        GenericVariant,
        [Description("Trade Good")]
        TradeGood,
        [Description("Artisan's Tools")]
        ArtisansTools,
        [Description("Food and Drink")]
        FoodAndDrink,
        [Description("Treasure")]
        Treasure,
        [Description("Spellcasting Focus")]
        SpellcastingFocus,
        [Description("Firearm")]
        Firearm,
        [Description("Futuristic")]
        Futuristic,
        [Description("Simple Weapon")]
        SimpleWeapon,
        [Description("Martial Weapon")]
        MartialWeapon,
        [Description("Melee Weapon")]
        MeleeWeapon,
        [Description("Ranged Weapon")]
        RangedWeapon,
        [Description("Staff")]
        Staff,
        [Description("Poison")]
        Poison,
        [Description("Modern")]
        Modern,
        [Description("Mount")]
        Mount,
        [Description("Instrument")]
        Instrument,
        [Description("Tack and Harness")]
        TackAndHarness,
        [Description("Potion")]
        Potion,
        [Description("Renaissance")]
        Renaissance,
        [Description("Explosive")]
        Explosive,
        [Description("Vehicle")]
        Vehicle,
        [Description("Heavy Armor")]
        HeavyArmor,
        [Description("Medium Armor")]
        MediumArmor,
        [Description("Light Armor")]
        LightArmor,
        [Description("Shield")]
        Shield,
        [Description("Armor")]
        Armor,
        [Description("Gaming Set")]
        GamingSet,
        [Description("Tools")]
        Tools,
        [Description("Ring")]
        Ring,
        [Description("Wondrous Item")]
        WondrousItem,
        [Description("Rod")]
        Rod,
        [Description("Wand")]
        Wand,
        [Description("Tattoo")]
        Tattoo,
        [Description("Scroll")]
        Scroll,
        [Description("Other")]
        Other,
        [Description("Unknown")]
        Unknown,
        [Description("Inventory")]
        Inventory,
    }
}
