using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Extensions;
using Jotunn.Utils;

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
		 * This really, really need this to be reliably set and checked
		 * because if there are exceptions thrown in the Player class,
		 * it can trigger the game to DESTROY the character, lossing it completely.
		 * E.g., patching could fail when the game updates, causing null references, if not careful.
		 */
		public static bool Enabled { get { return _loaded && _enabled != null && _enabled.Value; } }
		private static bool _loaded = false;
		private static ConfigEntry<bool> _enabled;

		private Harmony _harmony;

#pragma warning disable IDE0051 // "Remove unused private members" - they are used, but only at runtime, so the compiler can't see it
		private void Awake()
		{
			try
			{
				_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);
				BindConfig();

				_loaded = true;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during Main.Awake:");
			}
		}

		private void OnDestroy()
		{
			// Not sure if this matters, but cleanup seems like a good practice

			UnbindConfig();
			_harmony?.UnpatchSelf();
		}
#pragma warning restore IDE0051

		private void BindConfig()
		{
			_enabled = Config.BindConfig(
				section: "General",
				key: "Enabled",
				description: "Enable this mod",
				defaultValue: true);

			_enabled.SettingChanged += Enabled_SettingChanged;
		}

		private void UnbindConfig()
		{
			if (_enabled != null)
				_enabled.SettingChanged -= Enabled_SettingChanged;
		}

		private void Enabled_SettingChanged(object sender, System.EventArgs e)
		{
			// If disabling, the cache won't be used anymore
			// If enabling, it can't be trusted anymore since it wasn't tracking updates, and so needs to be rebuilt.

			PlayerExtensions.ClearPlayerItemLevels();
		}
	}
}