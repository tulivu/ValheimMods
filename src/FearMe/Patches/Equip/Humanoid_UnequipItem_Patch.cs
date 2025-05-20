using HarmonyLib;
using System;

namespace FearMe.Patches.Equip
{
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
	public static class Humanoid_UnequipItem_Patch
	{
		public static void Postfix(Humanoid __instance)
		{
			try
			{
				if (!Main.Enabled)
					return;


				// When something is unequipped, recalculate the player's level
				if (__instance is Player player)
				{
					player.UpdatePlayerItemLevel();
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(Humanoid_UnequipItem_Patch)}:");
			}
		}
	}
}
