using System;

namespace FearMe
{
	public enum FearLevel
	{
		NotAfraid = 0, // Do the normal attacking
		Cautious = 1, // Ignore the target - don't attack, but don't flee either
		Afraid = 2, // Scary! Run away!
	}

	public static class MonsterExtensions
	{
		public static FearLevel GetFearLevel(this BaseAI ai, Character targetCreature)
		{
			try
			{
				if (!Main.Enabled)
					return FearLevel.NotAfraid;


				// No target, nothing to fear
				if (targetCreature == null || targetCreature is not Player player)
					return FearLevel.NotAfraid;

				// Shouldn't happen, but make sure there's actually a monster to be fearful
				if (ai == null || ai.m_character == null || ai.m_character.m_name == null)
				{
					Jotunn.Logger.LogDebug($"AI was null");
					return FearLevel.NotAfraid;
				}
				var character = ai.m_character;

				// Passive (e.g. Dvergr?) & Boss creatures aren't afraid of players
				if (character.IsTamed() || character.IsBoss() || !ai.HuntPlayer())
					return FearLevel.NotAfraid;


				var monsterItemLevel = character.GetMonsterBravery();
				if (monsterItemLevel <= 0)
					return FearLevel.NotAfraid;


				var playerItemLevel = player.GetPlayerItemLevel();
				if (playerItemLevel <= 0)
					return FearLevel.NotAfraid;


				FearLevel fearLevel;
				const int fearThreshold = 2; // How many levels ahead before a player is scary. Must be >= 1

				// Weak player - get them!
				if (playerItemLevel <= monsterItemLevel)
					fearLevel = FearLevel.NotAfraid;
				// Big scary player, run away!
				else if (playerItemLevel >= monsterItemLevel + fearThreshold)
					fearLevel = FearLevel.Afraid;
				// A little stronger than us, maybe they'll leave us alone if we don't attack.
				else
					fearLevel = FearLevel.Cautious;

				Jotunn.Logger.LogDebug(
					$"playerItemLevel: {playerItemLevel}, monsterItemLevel: {monsterItemLevel}, fearThreshold: {fearThreshold}, fearLevel: {fearLevel}");

				return fearLevel;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during GetFearLevel:");

				return FearLevel.NotAfraid;
			}
		}

		private static int GetMonsterBravery(this Character character)
		{
			try
			{
				// Bravery levels are roughly correlated to BiomeItemLevel so monsters get braver as the biomes get harder

				int monsterBravery;

				if (!MonsterData.MonsterBravery.TryGetValue(character.m_name, out monsterBravery))
				{
					Jotunn.Logger.LogDebug($"Unknown monster {character.m_name}");
					monsterBravery = -1; // Don't know this monster
				}
				else
				{
					Jotunn.Logger.LogDebug($"Monster level {character.m_level}");
					monsterBravery += (character.m_level - 1); // Starred monsters are braver!
				}

				return monsterBravery;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during GetMonsterBravery:");

				return -1;
			}
		}
	}
}