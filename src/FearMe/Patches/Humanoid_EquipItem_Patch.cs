using System;
using HarmonyLib;

namespace FearMe.Patches
{
#if DEBUG
	[HarmonyDebug]
#endif
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
	public static class Humanoid_EquipItem_Patch
	{
		public static void Postfix(bool __result, Humanoid __instance)
		{
			try
			{
				if (!Main.Enabled)
					return;


				// When something is equipped, recalculate the player's level
				if (__result && __instance is Player player)
				{
					player.UpdatePlayerItemLevel();
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during Humanoid.EquipItem_Postfix:");
			}
		}
	}
}
