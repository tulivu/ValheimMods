using System;
using System.Collections;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace FearMe
{
	public static class PlayerExtensions
	{
		// Cache of players' armor levels, updated whenever they change equipped items.
		// TODO: does this work in multiplayer?
		private static IDictionary<long, int> _playersItemLevels = new Dictionary<long, int>();

		public static void ClearPlayerItemLevels()
		{
			// ok to do this, even if mod is disabled

			_playersItemLevels.Clear();
		}

		private static void SetPlayerItemLevel(long playerId, int playerItemLevel)
		{
			Jotunn.Logger.LogMessage($"UpdatePlayerItemLevel {playerId} to {playerItemLevel}");
			_playersItemLevels[playerId] = playerItemLevel; // Cache the calculation for later
		}

		public static int GetPlayerItemLevel(this Player player)
		{
			if (!Main.Enabled)
				return 0;


			if (!_playersItemLevels.TryGetValue(player.GetPlayerID(), out int itemLevel))
				itemLevel = UpdatePlayerItemLevel(player);

			return itemLevel;
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

					SetPlayerItemLevel(playerId, playerItemLevel);

					if (!ZNet.IsSinglePlayer)
					{
						var sender = ZRoutedRpc.instance == null ? "NULL" : ZRoutedRpc.instance.m_id.ToString();
						Jotunn.Logger.LogMessage($"PlayerItemLevelRPC sending {playerId} {playerItemLevel} from {sender} to {ZRoutedRpc.Everybody}");

						var package = new ZPackage();
						package.Write(playerId);
						package.Write(playerItemLevel);
						_playerItemLevelRPC.SendPackage(ZRoutedRpc.Everybody, package);
					}
				}

				return playerItemLevel;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during UpdatePlayerItemLevel:");

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
			Jotunn.Logger.LogMessage($"OnServerReceive_PlayerItemLevelRPC from {sender}");

			if (package != null && package.Size() > 0)
			{
				var playerId = package.ReadLong();
				var playerItemLevel = package.ReadInt();

				SetPlayerItemLevel(playerId, playerItemLevel);
			}

			yield break;
		}

		// React to the RPC call on a client
		private static IEnumerator OnClientReceive_PlayerItemLevelRPC(long sender, ZPackage package)
		{
			Jotunn.Logger.LogMessage($"OnClientReceive_PlayerItemLevelRPC from {sender}");

			if (package != null && package.Size() > 0)
			{
				var playerId = package.ReadLong();
				var playerItemLevel = package.ReadInt();

				SetPlayerItemLevel(playerId, playerItemLevel);
			}

			yield break;
		}
	}
}
