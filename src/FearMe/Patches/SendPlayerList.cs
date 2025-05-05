using System;
using HarmonyLib;

namespace FearMe.Patches
{
#if DEBUG
	[HarmonyDebug]
#endif
	[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPlayerList))]
	public static class ZNet_SendPlayerList_Patch
	{
		public static void Postfix()
		{
			try
			{
				if (!Main.Enabled)
					return;


				PlayerExtensions.BroadcastPlayerItemLevels();
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during BaseAI.FindEnemy_Postfix:");
			}
		}
	}
}
