using System;
using System.Collections;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

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


				if (!_playersItemLevels.TryGetValue(player.GetPlayerID(), out int itemLevel))
					itemLevel = UpdatePlayerItemLevel(player);

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
			Jotunn.Logger.LogDebug($"Setting player {playerId} to {playerItemLevel}");
			_playersItemLevels[playerId] = playerItemLevel; // Cache the calculation for later

			if(send)
				SendPlayerItemLevel(playerId, playerItemLevel);
		}

		public static int UpdatePlayerItemLevel(this Player player)
		{
			try
			{
				if (!Main.Enabled)
					return 0;


				int playerItemLevel = 0;

				var playerId = player.GetPlayerID();
				if (playerId != 0)
				{
					(int itemLevelSum, int qualitySum, int numItems) = player.SumEquipment();
					playerItemLevel = CalculateItemLevel(itemLevelSum, qualitySum, numItems);

					SetPlayerItemLevel(playerId, playerItemLevel, true);
				}

				return playerItemLevel;
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(UpdatePlayerItemLevel)}:");
				return 0;
			}
		}

		// Figure out the armor levels of the equipped gear, based on the biomes they are from
		private static (int itemLevelSum, int qualitySum, int numItems) SumEquipment(this Player player)
		{
			var itemLevelSum = 0;
			var qualitySum = 0;
			var numItems = 0;

			int itemLevel;

			if (player.m_helmetItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_helmetItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numItems++;
				}

				qualitySum += player.m_helmetItem.m_quality;
			}

			if (player.m_chestItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_chestItem.m_shared.m_name, out itemLevel))
				{
					itemLevelSum += itemLevel;
					numItems++;
				}

				qualitySum += player.m_chestItem.m_quality;
			}

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

		public static void RegisterRPCs()
		{
			_playerItemLevelRPC = NetworkManager.Instance.AddRPC(
				"RPC_PlayerItemLevel",
				OnServerReceive_PlayerItemLevelRPC,
				OnClientReceive_PlayerItemLevelRPC);
		}

		private static IEnumerator OnServerReceive_PlayerItemLevelRPC(long sender, ZPackage package)
		{
			try
			{
				Jotunn.Logger.LogDebug($"{nameof(OnServerReceive_PlayerItemLevelRPC)} from {sender}");

				if (package != null && package.Size() > 0)
				{
					var playerId = package.ReadLong();
					var playerItemLevel = package.ReadInt();

					SetPlayerItemLevel(playerId, playerItemLevel, true);
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
			try
			{
				Jotunn.Logger.LogDebug($"{nameof(OnClientReceive_PlayerItemLevelRPC)} from {sender}");

				if (package != null && package.Size() > 0)
				{
					var playerId = package.ReadLong();
					var playerItemLevel = package.ReadInt();

					// If the server is repeating our own message, skip it
					var localPlayerId = Player.m_localPlayer?.GetPlayerID() ?? 0;
					if (localPlayerId != 0 && localPlayerId != playerId)
						SetPlayerItemLevel(playerId, playerItemLevel, false);
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(OnClientReceive_PlayerItemLevelRPC)}:");
			}

			yield break;
		}

		private static void SendPlayerItemLevel(long playerId, int playerItemLevel)
		{
			if (!ZNet.IsSinglePlayer)
			{
				var sender = ZRoutedRpc.instance == null ? "NULL" : ZRoutedRpc.instance.m_id.ToString();
				Jotunn.Logger.LogDebug($"Sending {playerId} {playerItemLevel} from {sender} to {ZRoutedRpc.Everybody}");

				var package = new ZPackage();
				package.Write(playerId);
				package.Write(playerItemLevel);
				_playerItemLevelRPC.SendPackage(ZRoutedRpc.Everybody, package);
			}
		}

		public static void BroadcastPlayerItemLevels()
		{
			try
			{
				if (!ZNet.IsSinglePlayer)
				{
					foreach (var x in _playersItemLevels)
					{
						SendPlayerItemLevel(x.Key, x.Value);
					}
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(BroadcastPlayerItemLevels)}:");
			}
		}
	}
}
