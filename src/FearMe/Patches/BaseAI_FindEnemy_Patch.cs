using HarmonyLib;
using System;

namespace FearMe.Patches
{
	//[HarmonyDebug]
	[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.FindEnemy))]
	public static class BaseAI_FindEnemy_Patch
	{
		public static void Postfix(ref Character __result, BaseAI __instance)
		{
			try
			{
				if (!Main.Enabled)
					return;


				var fearLevel = __instance.GetFearLevel(__result);

				// If the creature is cautious of the enemy target, ignore it - don't attack, but don't flee either.
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
