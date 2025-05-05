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


				if (__result != null && __result.IsPlayer())
				{
					var fearLevel = __instance.GetFearLevel(__result);
#if DEBUG
					Jotunn.Logger.LogDebug($"{__instance?.m_character?.m_name ?? "UNKONWN"} ({__instance?.m_character?.m_level ?? 1}) is {Enum.GetName(typeof(FearLevel), fearLevel)} of {__result?.m_name ?? "UNKNOWN"}");
#endif

					// If the creature is cautious of the enemy target, ignore it - don't attack, but don't flee either.
					// Doesn't stop attacking if already attacking, though
					if (fearLevel == FearLevel.Cautious)
						__result = null;
				}
			}
			catch (Exception e)
			{
				Utils.LogException(e, "Exception during BaseAI.FindEnemy_Postfix:");
			}
		}
	}
}
