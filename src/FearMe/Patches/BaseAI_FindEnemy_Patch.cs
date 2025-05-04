using System;
using HarmonyLib;

namespace FearMe.Patches
{
#if DEBUG
	[HarmonyDebug]
#endif
	[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.FindEnemy))]
	public static class BaseAI_FindEnemy_Patch
	{
		public static void Postfix(ref Character __result, BaseAI __instance)
		{
			try
			{
				if (!Main.Enabled)
					return;


				// If the creature is cautious of the enemy target, ignore it - don't attack, but don't flee either.
				var fearLevel = __instance.GetFearLevel(__result);
				if (fearLevel == FearLevel.Cautious)
					__result = null;
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during BaseAI.FindEnemy_Postfix:");
			}
		}
	}
}
