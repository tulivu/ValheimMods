using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
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

		private void LoadConfig()
		{
			BindConfig();
			BindData();
		}

		private void UnloadConfig()
		{
			UnbindConfig();
			UnbindData();
		}

		private void BindConfig()
		{
			_enabled = Config.BindConfig(
				section: "General",
				key: "Enabled",
				description: "Enable this mod",
				defaultValue: true);

			_enabled.SettingChanged += Enabled_SettingChanged;
		}

		private void Enabled_SettingChanged(object sender, System.EventArgs e)
		{
			// If disabling, the cache won't be used anymore
			// If enabling, it can't be trusted since it wasn't tracking updates

			PlayerExtensions.ClearPlayerItemLevels();
		}

		private void UnbindConfig()
		{
			if (_enabled != null)
			{
				_enabled.SettingChanged -= Enabled_SettingChanged;
				_enabled = null;
			}
		}

		private void BindData()
		{
			var itemDataPath = Path.Combine(BepInEx.Paths.ConfigPath, "FearMe.ItemData.json");
			if (File.Exists(itemDataPath))
			{
				var json = File.ReadAllText(itemDataPath);
				var itemLevels = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(json);
				ItemData.ItemLevels = itemLevels;
			}
#if DEBUG
			else
			{
				var json = SimpleJson.SimpleJson.SerializeObject(ItemData.ItemLevels);
				File.WriteAllText(itemDataPath, json);
			}
#endif

			var monsterDataPath = Path.Combine(BepInEx.Paths.ConfigPath, "FearMe.MonsterData.json");
			if (File.Exists(monsterDataPath))
			{
				var json = File.ReadAllText(monsterDataPath);
				var monsterBravery = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(json);
				MonsterData.MonsterBravery = monsterBravery;
			}
#if DEBUG
			else
			{
				var json = SimpleJson.SimpleJson.SerializeObject(MonsterData.MonsterBravery);
				File.WriteAllText(monsterDataPath, json);
			}
#endif
		}

		private void UnbindData()
		{
		}

		//private void BindData()
		//{
		//	BindData("ItemData.json", ImportItemData, ExportItemData);
		//	BindData("MonsterData.json", ImportMonsterData, ExportMonsterData);
		//}

		//private void ImportItemData(string data)
		//{
		//	var itemLevels = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(data);
		//	ItemData.ItemLevels = itemLevels;
		//}

		//private string ExportItemData()
		//{
		//	var data = SimpleJson.SimpleJson.SerializeObject(ItemData.ItemLevels);
		//	return data;
		//}

		//private void ImportMonsterData(string data)
		//{
		//	var monsterBravery = SimpleJson.SimpleJson.DeserializeObject<IDictionary<string, int>>(data);
		//	MonsterData.MonsterBravery = monsterBravery;
		//}

		//private string ExportMonsterData()
		//{
		//	var data = SimpleJson.SimpleJson.SerializeObject(MonsterData.MonsterBravery);
		//	return data;
		//}

		//private void UnbindData()
		//{
		//	foreach (var watcher in _watchers)
		//	{
		//		watcher.Dispose();
		//	}
		//	_watchers.Clear();
		//}

		//private IList<DataWatcher> _watchers = new List<DataWatcher>();

		//private void BindData(string filename, Action<string> importData, Func<string> exportData = null)
		//{
		//	var dataWatcher = new DataWatcher(filename, importData, exportData);
		//	_watchers.Add(dataWatcher);
		//}

		//private class DataWatcher : IDisposable
		//{
		//	private FileSystemWatcher _watcher;
		//	private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

		//	private Action<string> _importData;
		//	private Func<string> _exportData;

		//	private bool _disposedValue = false;


		//	public DataWatcher(string filename, Action<string> importData, Func<string> exportData)
		//	{
		//		var dataFileWatcher = new FileSystemWatcher(BepInEx.Paths.ConfigPath, filename);
		//		dataFileWatcher.Changed += ReloadDataFile;
		//		dataFileWatcher.Created += ReloadDataFile;
		//		dataFileWatcher.Renamed += ReloadDataFile;

		//		dataFileWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
		//		dataFileWatcher.EnableRaisingEvents = true;

		//		_watcher = dataFileWatcher;
		//		_importData = importData;
		//		_exportData = exportData;
		//	}

		//	private void ReloadDataFile(object sender, FileSystemEventArgs eventArgs)
		//	{
		//		try
		//		{
		//			if ((DateTimeOffset.UtcNow - _lastUpdate).TotalMilliseconds < 2000)
		//				return;

		//			if (File.Exists(eventArgs.FullPath))
		//			{
		//				var data = File.ReadAllText(eventArgs.FullPath);
		//				_importData(data);
		//			}
		//			else
		//			{
		//				if (_exportData != null)
		//				{
		//					var data = _exportData();
		//					File.WriteAllText(data, eventArgs.FullPath);
		//				}
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Utils.LogException(ex, "Exception during ReloadDataFile:");
		//		}
		//	}

		//	protected virtual void Dispose(bool disposing)
		//	{
		//		if (!_disposedValue)
		//		{
		//			if (disposing)
		//			{
		//				_watcher.Dispose();
		//			}

		//			_disposedValue = true;
		//		}
		//	}

		//	public void Dispose()
		//	{
		//		Dispose(disposing: true);
		//		GC.SuppressFinalize(this);
		//	}
		//}

		private void RegisterRPCs()
		{
			PlayerExtensions.RegisterRPCs();
		}
	}
}