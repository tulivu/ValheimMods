using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FearMe
{
	public static class PlayerExtensions
	{
		// Cache of players' armor levels, updated whenever they change equipped items.
		private static IDictionary<long, int> _playersItemLevels = new Dictionary<long, int>();

		public static void ClearPlayerItemLevels()
		{
			_playersItemLevels.Clear();
		}

		public static int GetPlayerItemLevel(this Player player)
		{
			try
			{
				if (!Main.Enabled)
					return 0;


				int itemLevel = 0;

				var playerId = player.GetPlayerID();
				if (playerId != 0)
				{
					if (!_playersItemLevels.TryGetValue(playerId, out itemLevel))
					{
						itemLevel = UpdatePlayerItemLevel(playerId, player);
					}
				}

				return itemLevel;
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(GetPlayerItemLevel)}:");
				return 0;
			}
		}

		private static void SetPlayerItemLevel(long playerId, int playerItemLevel, bool send)
		{
			_playersItemLevels[playerId] = playerItemLevel;

			if (send)
				SendPlayerItemLevel(playerId, playerItemLevel);
		}

		public static void UpdatePlayerItemLevel(this Player player)
		{
			try
			{
				if (!Main.Enabled)
					return;


				var playerId = player.GetPlayerID();
				UpdatePlayerItemLevel(playerId, player);
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(UpdatePlayerItemLevel)}:");
			}
		}

		private static int UpdatePlayerItemLevel(long playerId, Player player)
		{
			int playerItemLevel = 0;

			if (playerId != 0)
			{
				(int itemLevelSum, int qualitySum, int numItems) = player.SumEquipment();
				playerItemLevel = CalculateItemLevel(itemLevelSum, qualitySum, numItems);

				if (!_playersItemLevels.TryGetValue(playerId, out var previousPlayerItemLevel) || previousPlayerItemLevel != playerItemLevel)
				{
					SetPlayerItemLevel(playerId, playerItemLevel, send: true);
				}
			}

			return playerItemLevel;
		}

		private static (int itemLevelSum, int qualitySum, int numItems) SumEquipment(this Player player)
		{
			var itemLevelSum = 0;
			var qualitySum = 0;
			var numItems = 0;

			int itemLevel;

			if (player.m_helmetItem != null)
			{
				// Only count items we know about
				if (ItemData.ItemLevels.TryGetValue(player.m_helmetItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numItems++;
				}

				qualitySum += player.m_helmetItem.m_quality;
			}
			else
				numItems++; // An empty slot counts as level 0

			if (player.m_chestItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_chestItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numItems++;
				}

				qualitySum += player.m_chestItem.m_quality;
			}
			else
				numItems++;

			// Ignore the cloak, since it's more utility than armor.
			/*
			if (player.m_shoulderItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_shoulderItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numKnownItems++;
				}

				qualitySum += player.m_shoulderItem.m_quality;
			}
			else
				numItems++;
			*/

			if (player.m_legItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_legItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numItems++;
				}

				qualitySum += player.m_legItem.m_quality;
			}
			else
				numItems++;

			return (itemLevelSum, qualitySum, numItems);
		}

		private static int CalculateItemLevel(int itemLevelSum, int qualitySum, int numItems)
		{
			var playerItemLevel = -1;

			if (numItems > 0)
			{
				playerItemLevel = ((3 * itemLevelSum + qualitySum) - (numItems - 1)) / (3 * numItems);
			}

			return playerItemLevel;
		}


		private static CustomRPC _playerItemLevelRPC;
		private static CustomRPC _allPlayersItemLevelsRPC;

		public static void RegisterRPCs()
		{
			_playerItemLevelRPC = NetworkManager.Instance.AddRPC(
				"RPC_PlayerItemLevel",
				OnServerReceive_PlayerItemLevelRPC,
				OnClientReceive_PlayerItemLevelRPC);

			_allPlayersItemLevelsRPC = NetworkManager.Instance.AddRPC(
				"RPC_AllPlayersItemLevels",
				OnServerReceive_AllPlayersItemLevelsRPC,
				OnClientReceive_AllPlayersItemLevelsRPC);
		}

		private static IEnumerator OnServerReceive_PlayerItemLevelRPC(long sender, ZPackage package)
		{
			try
			{
				if (package != null && package.Size() > 0)
				{
					var playerId = package.ReadLong();
					var playerItemLevel = package.ReadInt();

					// On the server, track the players' levels to send to the clients
					SetPlayerItemLevel(playerId, playerItemLevel, send: false);
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(OnServerReceive_PlayerItemLevelRPC)}:");
			}

			yield break;
		}

		private static IEnumerator OnClientReceive_PlayerItemLevelRPC(long sender, ZPackage package)
		{
			yield break;
		}

		private static void SendPlayerItemLevel(long playerId, int playerItemLevel)
		{
			if (ZNet.instance == null)
				return;

			var package = new ZPackage();

			package.Write(playerId);
			package.Write(playerItemLevel);

			_playerItemLevelRPC.SendPackage(ZRoutedRpc.Everybody, package);
		}

		private static IEnumerator OnServerReceive_AllPlayersItemLevelsRPC(long sender, ZPackage package)
		{
			yield break;
		}

		private static IEnumerator OnClientReceive_AllPlayersItemLevelsRPC(long sender, ZPackage package)
		{
			try
			{
				if (package != null && package.Size() > 0)
				{
					ClearPlayerItemLevels();

					var numRecords = package.ReadInt();
					for (var i = 0; i < numRecords; i++)
					{
						var playerId = package.ReadLong();
						var playerItemLevel = package.ReadInt();

						SetPlayerItemLevel(playerId, playerItemLevel, send: false);
					}
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(OnClientReceive_PlayerItemLevelRPC)}:");
			}

			yield break;
		}

		// Periodically send the current players' levels.
		public static void BroadcastPlayerItemLevels()
		{
			try
			{
				if (!Main.Enabled || !_playersItemLevels.Any() || ZNet.instance == null)
					return;

				ZPackage package = new ZPackage();

				package.Write(_playersItemLevels.Count);
				foreach (var x in _playersItemLevels)
				{
					package.Write(x.Key);
					package.Write(x.Value);
				}

				_allPlayersItemLevelsRPC.SendPackage(ZRoutedRpc.Everybody, package);
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(BroadcastPlayerItemLevels)}:");
			}
		}
	}
}
