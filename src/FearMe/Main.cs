using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Extensions;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FearMe
{
	// References:
	// https://github.com/RandyKnapp/ValheimMods/blob/main/ValheimModding-GettingStarted.md
	// https://valheim-modding.github.io/Jotunn/guides/overview.html
	// https://github.com/Valheim-Modding/Jotunn
	// https://harmony.pardeike.net/articles/patching.html
	// https://github.com/pardeike/Harmony/tree/master
	// https://github-wiki-see.page/m/BepInEx/HarmonyX/wiki/Transpiler-helpers
	// https://elin-modding-resources.github.io/Elin.Docs/articles/50_Patching/Transpiler%20101/codematcher
	// https://gist.github.com/JavidPack/454477b67db8b017cb101371a8c49a1c
	// https://valheim-modding.github.io/Jotunn/data/localization/translations/English.html
	// https://github.com/loco-choco/TranspilerHandbook/blob/main/transpiler.md


	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	[BepInDependency(Jotunn.Main.ModGuid)]
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
	[SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
	class Main : BaseUnityPlugin
	{
		/** !! Also update these in Package/manifest.json!! **/
		public const string PLUGIN_GUID = "tulivu.valheimmods.fearme";
		public const string PLUGIN_NAME = "FearMe";
		public const string PLUGIN_VERSION = "0.1.0";


		// OPTION: Is the mod as a whole enabled?
		/*
		 * This needs to be reliably set and checked
		 * because if there are exceptions thrown in the Player class,
		 * it can trigger the game to DESTROY the character, lossing it completely.
		 * 
		 * E.g., patching could fail when the game updates, causing null references, if not careful.
		 */
		public static bool Enabled { get { return _loaded && _enabled != null && _enabled.Value; } }
		private static bool _loaded = false;
		private static ConfigEntry<bool> _enabled;

		private static ConfigEntry<string> _itemLevels;

		private static ConfigEntry<string> _monstersBravery;

#pragma warning disable IDE0051 // IDE0051: "Remove unused private members" - they are used, but only at runtime, so the compiler can't see it
		private void Awake()
		{
			try
			{
				Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

				LoadConfig();
				RegisterRPCs();

				_loaded = true;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during Main.Awake:");
			}
		}

		private void OnDestroy()
		{
			_loaded = false;

			UnloadConfig();
		}
#pragma warning restore IDE0051

		private const string GENERAL_SECTION = "General";
		private const string DATA_SECTION = "Data";

		private void LoadConfig()
		{
			_enabled = Config.BindConfig(
				section: GENERAL_SECTION,
				key: "Enabled",
				description: "Enable this mod",
				defaultValue: true);
			_enabled.SettingChanged += Enabled_SettingChanged;


			_itemLevels = Config.BindConfig<string>(
				section: DATA_SECTION,
				key: "ItemLevels",
				description:
@"Armor levels, based on the biomes they come from.
	0 - Naked
	1 - Starter (Rags)
	2 - Meadows (Leather)
	3 - BlackForest (Bronze/Troll)
	4 - Swamp (Iron/Root)
	5 - Mountain (Wolf/Fenris)
	6 - Plains (Padded)
	7 - Mistlands (Carapace/Mage)
	8 - Ashlands (Flametal/Ashlands Mage)",
				defaultValue: null);
			_itemLevels.SettingChanged += ItemLevels_SettingChanged;

			// Put the default values in the config so it's easier to override, if anyone wants to.
			if (string.IsNullOrWhiteSpace(_itemLevels.Value))
				_itemLevels.Value = SimpleJson.SimpleJson.SerializeObject(ItemData.ItemLevels);


			_monstersBravery = Config.BindConfig<string>(
				section: DATA_SECTION,
				key: "MonsterBravery",
				description:
@"Bravery levels, correlated to the biomes the monsters are in, with some tweaks for flavor.
	2 - Meadows
	3 - Black Forest
	4 - Swamp
	5 - Mountain
	6 - Plains
	7 - Mistlands
	8 - Ashlands",
				defaultValue: null);
			_monstersBravery.SettingChanged += MonstersBravery_SettingChanged;

			// Put the default values in the config so it's easier to override, if anyone wants to.
			if (string.IsNullOrWhiteSpace(_monstersBravery.Value))
				_monstersBravery.Value = SimpleJson.SimpleJson.SerializeObject(MonsterData.MonsterBravery);
		}

		private void Enabled_SettingChanged(object sender, System.EventArgs e)
		{
			// If disabling, the cache won't be used anymore
			// If enabling, it can't be trusted since it wasn't tracking updates

			PlayerExtensions.ClearPlayerItemLevels();
		}

		private void ItemLevels_SettingChanged(object sender, EventArgs e)
		{
			var args = e as SettingChangedEventArgs;
			var itemLevelsJson = args?.ChangedSetting?.BoxedValue as string;
			if (string.IsNullOrWhiteSpace(itemLevelsJson))
				ItemData.ItemLevels = null;
			else
			{
				var itemLevels = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(itemLevelsJson);
				if (itemLevels == null || !itemLevels.Any())
					ItemData.ItemLevels = null;
				else
					ItemData.ItemLevels = itemLevels;
			}
		}

		private void MonstersBravery_SettingChanged(object sender, EventArgs e)
		{
			var args = e as SettingChangedEventArgs;
			var monstersBraveryJson = args?.ChangedSetting?.BoxedValue as string;
			if (string.IsNullOrWhiteSpace(monstersBraveryJson))
				MonsterData.MonsterBravery = null;
			else
			{
				var monstersBravery = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(monstersBraveryJson);
				if (monstersBravery == null || !monstersBravery.Any())
					MonsterData.MonsterBravery = null;
				else
					MonsterData.MonsterBravery = monstersBravery;
			}
		}

		private void UnloadConfig()
		{
			if (_enabled != null)
			{
				_enabled.SettingChanged -= Enabled_SettingChanged;
				_enabled = null;
			}

			if (_itemLevels != null)
			{
				_itemLevels.SettingChanged -= ItemLevels_SettingChanged;
				_itemLevels = null;
			}

			if (_monstersBravery != null)
			{
				_monstersBravery.SettingChanged -= MonstersBravery_SettingChanged;
				_monstersBravery = null;
			}
		}

		private void RegisterRPCs()
		{
			PlayerExtensions.RegisterRPCs();
		}
	}
}