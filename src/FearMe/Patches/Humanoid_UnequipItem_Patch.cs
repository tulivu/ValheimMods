using System;
using HarmonyLib;

namespace FearMe.Patches
{
#if DEBUG
	[HarmonyDebug]
#endif
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
				Utils.LogException(e, "Exception during Humanoid.UnequipItem_Postfix:");
			}
		}
	}
}
