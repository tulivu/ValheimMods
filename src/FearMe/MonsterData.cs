using System.Collections.Generic;

namespace FearMe
{
	internal static class MonsterData
	{
		// TODO load from file


		// Bravery levels are correlated to BiomeItemLevel, with some tweaks for flavor.
		// 2 - Meadows
		// 3 - Black Forest
		// 4 - Swamp
		// 5 - Mountain
		// 6 - Plains
		// 7 - Mistlands
		// 8 - Ashlands

		public static IDictionary<string, int> MonsterBravery = new Dictionary<string, int>()
		{
			{"$enemy_neck", 2},
			{"$enemy_boar", 2},
			{"$enemy_greyling", 2},
			{"$enemy_ghost", 5},
			{"$enemy_greydwarf", 3},
			{"$enemy_greydwarfbrute", 4},
			{"$enemy_greydwarfshaman", 4},
			{"$enemy_skeletonpoison", 4},
			{"$enemy_troll", 6},
			{"$enemy_abomination", 6},
			{"$enemy_blob", 4},
			{"$enemy_blobelite", 5},
			{"$enemy_draugr", 4},
			{"$enemy_draugrelite", 5},
			{"$enemy_leech", 4},
			{"$enemy_skeleton", 4},
			{"$enemy_surtling", 4},
			{"$enemy_wraith", 5},
			{"$enemy_bat", 5},
			{"$enemy_dragon", 5},
			{"$enemy_drake", 5},
			{"$enemy_fenring", 6},
			{"$enemy_fenringcultist", 6},
			{"$enemy_stonegolem", 7},
			{"$enemy_ulv", 5},
			{"$enemy_wolf", 6},
			{"$enemy_blobtar", 6},
			{"$enemy_deathsquito", 7},
			{"$enemy_goblin", 6},
			{"$enemy_goblinbrute", 8},
			{"$enemy_goblinshaman", 6},
			{"$enemy_lox", 9},
			{"$enemy_babyseeker", 6},
			{"$enemy_gjall", 9},
			{"$enemy_seeker", 7},
			{"$enemy_seekerbrute", 8},
			{"$enemy_tick", 7},
			{"$enemy_asksvin", 8},
			{"$enemy_bloblava", 8},
			{"$enemy_charred", 8},
			{"$enemy_charred_archer", 8},
			{"$enemy_charred_grunt", 8},
			{"$enemy_charred_mage", 8},
			{"$enemy_charred_melee", 8},
			{"$enemy_charred_melee_Dyrnwyn", 8},
			{"$enemy_charred_melee_Fader", 8},
			{"$enemy_charred_twitcher", 8},
			{"$enemy_charred_twitcher_summoned", 8},
			{"$enemy_fallenvalkyrie", 9},
			{"$enemy_morgen", 8},
			{"$enemy_volture", 8}
		};
	}
}