using HarmonyLib;
using System;

namespace FearMe.Patches
{
	[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
	public static class ZNet_SendPlayerList_Patch
	{
		public static void Postfix(ZNet __instance)
		{
			try
			{
				if (!Main.Enabled)
					return;


				PlayerExtensions.BroadcastPlayerItemLevels();
			}
			catch (Exception e)
			{
				Utils.LogException(e, $"Exception during {nameof(ZNet_SendPlayerList_Patch)}: ");
			}
		}
	}
}
