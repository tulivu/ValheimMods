using System;

namespace FearMe
{
	public enum FearLevel
	{
		NotAfraid = 0, // Do the normal attacking
		Cautious =  1, // Ignore the target - don't attack, but don't flee either
		Afraid =    2, // Scary! Run away!
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

				if (ai == null || ai is not MonsterAI monsterAI)
					return FearLevel.NotAfraid;

				if (!ai.IsAlerted() || monsterAI.IsEventCreature())
					return FearLevel.NotAfraid;

				if (ai.m_character == null || ai.m_character.m_name == null)
					return FearLevel.NotAfraid;

				var character = ai.m_character;

				if (character.IsTamed() || character.IsBoss())
					return FearLevel.NotAfraid;


				var monsterBravery = character.GetMonsterBravery();
				if (monsterBravery <= 0)
					return FearLevel.NotAfraid;

				var playerItemLevel = player.GetPlayerItemLevel();
				if (playerItemLevel <= 0)
					return FearLevel.NotAfraid;

				FearLevel fearLevel;
				const int fearThreshold = 2; // How many levels ahead before a player is scary. Must be >= 1

				// Weak player - get them!
				if (playerItemLevel <= monsterBravery)
					fearLevel = FearLevel.NotAfraid;
				// Big scary player, run away!
				else if (playerItemLevel >= monsterBravery + fearThreshold)
					fearLevel = FearLevel.Afraid;
				// A little stronger than us, maybe they'll leave us alone if we don't attack.
				else
					fearLevel = FearLevel.Cautious;

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
					monsterBravery = -1; // Don't know this monster
				}
				else
				{
					monsterBravery += (character.GetLevel() - 1); // Starred monsters are braver!
				}

				return monsterBravery;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during GetMonsterBravery:");

				return -1;
			}
		}

		public static void RunAway(this MonsterAI monsterAI, float dt)
		{
			//Jotunn.Logger.LogInfo($"{monsterAI?.m_character?.m_name ?? "NULL"} is fleeing in terror");

			monsterAI.m_targetStatic = null;
			monsterAI.Flee(dt, monsterAI.m_targetCreature.transform.position);
		}
	}
}