using System.Collections.Generic;

namespace FearMe
{
	internal static class ItemData
	{
		// https://valheim-wiki.com/Category:Armor
		// https://valheim.fandom.com/wiki/Armor
		public enum BiomeItemLevel
		{
			None		= 0, // Naked
			Starter		= 1, // Rags
			Meadows		= 2, // Leather
			BlackForest	= 3, // Bronze/Troll
			Swamp		= 4, // Iron/Root
			Mountain	= 5, // Wolf/Fenris
			Plains		= 6, // Padded
			Mistlands	= 7, // Carapace/Mage
			Ashlands	= 8, // Flametal/Ashlands Mage
		}

		// Armor levels, based on the biomes they come from.
		public static IDictionary<string, int> ItemLevels {
			get { return _itemLevels != null ? _itemLevels : (_itemLevels = GetDefaults()); }
			set { _itemLevels = value; }
		}
		private static IDictionary<string, int> _itemLevels;

		private static IDictionary<string, int> GetDefaults()
		{
			// https://valheim-modding.github.io/Jotunn/data/localization/translations/English.html
			return new Dictionary<string, int>()
			{
				// Helmet
				{"$item_helmet_midsummercrown",     (int)BiomeItemLevel.Starter},
				{"$item_helmet_yule",               (int)BiomeItemLevel.Starter},
				{"$item_helmet_dverger",            (int)BiomeItemLevel.Starter},
				{"$item_helmet_odin",               (int)BiomeItemLevel.Starter},
				{"$item_helmet_leather",            (int)BiomeItemLevel.Meadows},
				{"$item_helmet_trollleather",       (int)BiomeItemLevel.BlackForest},
				{"$item_helmet_bronze",             (int)BiomeItemLevel.BlackForest},
				{"$item_helmet_iron",               (int)BiomeItemLevel.Swamp},
				{"$item_helmet_root",               (int)BiomeItemLevel.Swamp},
				{"$item_helmet_fenris",             (int)BiomeItemLevel.Mountain},
				{"$item_helmet_drake",              (int)BiomeItemLevel.Mountain},
				{"$item_helmet_padded",             (int)BiomeItemLevel.Plains},
				{"$item_helmet_mage",               (int)BiomeItemLevel.Mistlands},
				{"$item_helmet_carapace",           (int)BiomeItemLevel.Mistlands},
				{"$item_helmet_mage_ashlands",      (int)BiomeItemLevel.Ashlands},
				{"$item_helmet_flametal",           (int)BiomeItemLevel.Ashlands},
				{"$item_helmet_medium_ashlands",    (int)BiomeItemLevel.Ashlands},

				// Chest
				{"$item_chest_rags",                (int)BiomeItemLevel.Starter},
				{"$item_chest_leather",             (int)BiomeItemLevel.Meadows},
				{"$item_chest_trollleather",        (int)BiomeItemLevel.BlackForest},
				{"$item_chest_bronze",              (int)BiomeItemLevel.BlackForest},
				{"$item_chest_root",                (int)BiomeItemLevel.Swamp},
				{"$item_chest_iron",                (int)BiomeItemLevel.Swamp},
				{"$item_chest_fenris",              (int)BiomeItemLevel.Mountain},
				{"$item_chest_wolf",                (int)BiomeItemLevel.Mountain},
				{"$item_chest_pcuirass",            (int)BiomeItemLevel.Plains},
				{"$item_chest_carapace",            (int)BiomeItemLevel.Mistlands},
				{"$item_chest_mage",                (int)BiomeItemLevel.Mistlands},
				{"$item_chest_flametal",            (int)BiomeItemLevel.Ashlands},
				{"$item_chest_mage_ashlands",       (int)BiomeItemLevel.Ashlands},
				{"$item_chest_medium_ashlands",     (int)BiomeItemLevel.Ashlands},

				// Cape
				{"$item_cape_odin",                 (int)BiomeItemLevel.Starter},
				{"$item_cape_deerhide",             (int)BiomeItemLevel.Meadows},
				{"$item_cape_trollhide",            (int)BiomeItemLevel.BlackForest},
				{"$item_cape_wolf",                 (int)BiomeItemLevel.Mountain},
				{"$item_cape_lox",                  (int)BiomeItemLevel.Plains},
				{"$item_cape_linen",                (int)BiomeItemLevel.Plains},
				{"$item_cape_feather",              (int)BiomeItemLevel.Mistlands},
				{"$item_cape_ash",                  (int)BiomeItemLevel.Ashlands},
				{"$item_cape_asksvin",              (int)BiomeItemLevel.Ashlands},

				// Legs
				{"$item_legs_rags",                 (int)BiomeItemLevel.Starter},
				{"$item_legs_leather",              (int)BiomeItemLevel.Meadows},
				{"$item_legs_bronze",               (int)BiomeItemLevel.BlackForest},
				{"$item_legs_trollleather",         (int)BiomeItemLevel.BlackForest},
				{"$item_legs_iron",                 (int)BiomeItemLevel.Swamp},
				{"$item_legs_root",                 (int)BiomeItemLevel.Swamp},
				{"$item_legs_fenris",               (int)BiomeItemLevel.Mountain},
				{"$item_legs_wolf",                 (int)BiomeItemLevel.Mountain},
				{"$item_legs_pgreaves",             (int)BiomeItemLevel.Plains},
				{"$item_legs_carapace",             (int)BiomeItemLevel.Mistlands},
				{"$item_legs_mage",                 (int)BiomeItemLevel.Mistlands},
				{"$item_legs_flametal",             (int)BiomeItemLevel.Ashlands},
				{"$item_legs_mage_ashlands",        (int)BiomeItemLevel.Ashlands},
				{"$item_legs_medium_ashlands",      (int)BiomeItemLevel.Ashlands},
			};
		}
	}
}