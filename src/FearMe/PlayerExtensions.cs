using System;
using System.Collections.Generic;

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

		public static int GetPlayerItemLevel(this Player player)
		{
			if (!Main.Enabled)
				return 0;


			if (!_playersItemLevels.TryGetValue(player.GetPlayerID(), out int itemLevel))
			{
				Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} not in item cache");
				itemLevel = UpdatePlayerItemLevel(player);
			}

			return itemLevel;
		}

		public static int UpdatePlayerItemLevel(this Player player)
		{
			try
			{
				if (!Main.Enabled)
					return 0;


				// Figure out the armor levels of the equipped gear, based on the biomes they come from.

				(int itemLevelSum, int numKnownItems, int qualitySum) = player.SumEquipment();
				int playerItemLevel = CalculateItemLevel(itemLevelSum, numKnownItems, qualitySum);
				_playersItemLevels[player.GetPlayerID()] = playerItemLevel; // Cache the calculation for later

				Jotunn.Logger.LogInfo($"Player {player.GetPlayerID()} new playerItemLevel: {playerItemLevel}");
				Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} itemLevelSum: {itemLevelSum}, numKnownItems: {numKnownItems}, qualitySum: {qualitySum}, playerItemLevel: {playerItemLevel}");

				return playerItemLevel;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during UpdatePlayerItemLevel:");

				return 0;
			}
		}

		private static (int itemLevelSum, int numKnownItems, int qualitySum) SumEquipment(this Player player)
		{
			var itemLevelSum = 0;
			int itemLevel;

			// Trying to allow for mods with new armor, so only include items in the average if they are known.
			var numKnownItems = 0;

			var qualitySum = 0;

			if (player.m_helmetItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_helmetItem.m_shared.m_name, out itemLevel))
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} helmet ItemLevel: {itemLevel}");
					itemLevelSum += itemLevel;
					numKnownItems++;
				}
				else
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} unknown helmet {player.m_helmetItem.m_shared.m_name}");
				}

				qualitySum += player.m_helmetItem.m_quality;
			}

			if (player.m_chestItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_chestItem.m_shared.m_name, out itemLevel))
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} chest ItemLevel: {itemLevel}");
					itemLevelSum += itemLevel;
					numKnownItems++;
				}
				else
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} unknown chest {player.m_chestItem.m_shared.m_name}");
				}

				qualitySum += player.m_chestItem.m_quality;
			}

			if (player.m_shoulderItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_shoulderItem.m_shared.m_name, out itemLevel))
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} shoulder ItemLevel: {itemLevel}");
					itemLevelSum += itemLevel;
					numKnownItems++;
				}
				else
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} unknown shoulder {player.m_shoulderItem.m_shared.m_name}");
				}

				qualitySum += player.m_helmetItem.m_quality;
			}

			if (player.m_legItem != null)
			{
				if (ItemData.ItemLevels.TryGetValue(player.m_legItem.m_shared.m_name, out itemLevel))
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} legs ItemLevel: {itemLevel}");
					itemLevelSum += itemLevel;
					numKnownItems++;
				}
				else
				{
					Jotunn.Logger.LogDebug($"Player {player.GetPlayerID()} unknown legs {player.m_legItem.m_shared.m_name}");
				}

				qualitySum += player.m_legItem.m_quality;
			}

			return (itemLevelSum, numKnownItems, qualitySum);
		}

		private static int CalculateItemLevel(int itemLevelSum, int numKnownItems, int qualitySum)
		{
			var playerItemLevel = 0;

			if (numKnownItems > 0)
			{
				// Add a little extra initial armor to smooth the transition from high-quality/low-biome to low-quality/high-biome armor.
				var totalItemLevelSum = itemLevelSum + (numKnownItems - 1);

				// Average the item levels, ignoring unknown gear.
				var averageItemLevel = totalItemLevelSum / numKnownItems;

				// Add a little bonus for maxed out gear.
				playerItemLevel = averageItemLevel + (qualitySum / 16);
			}
			else
			{
				if (qualitySum > 0)
				{
					playerItemLevel = -1; // Wearing gear, but don't know about any of it
				}
			}

			return playerItemLevel;
		}
	}
}
