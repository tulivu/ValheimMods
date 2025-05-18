using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FearMe.Patches
{
	//[HarmonyDebug]
	[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
	public static class MonsterAI_UpdateAI_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			/*
			 * Look for "this.m_fleeIfHurtWhenTargetCantBeReached" to find the code below, which is along with the other Flee() checks:
			 * 
			 * ...
			 * if (**m_fleeIfHurtWhenTargetCantBeReached** && m_targetCreature != null && m_timeSinceAttacking > 30f && m_timeSinceHurt < 20f)
			 * {
			 *	 Flee(dt, m_targetCreature.transform.position);
			 *	 m_lastKnownTargetPos = this.transform.position;
			 *	 m_updateTargetTimer = 1f;
			 *	 return true;
			 * }
			 * ...
			 */
			var matcher = new CodeMatcher(instructions, generator)
				.MatchStartForward(
					  new CodeMatch(OpCodes.Ldarg_0)
					, new CodeMatch(new CodeInstruction(
						OpCodes.Ldfld,
						AccessTools.Field(
							typeof(MonsterAI),
							nameof(MonsterAI.m_fleeIfHurtWhenTargetCantBeReached))))
				)

				// Handled in Main - disables the mod
				.ThrowIfInvalid("Could not find location to patch in MonsterAI.UpdateAI");


			/*
			 * Insert this custom code:
			 * 
			 * // If GetFearLevel()
			 * //   Is FearLevel.Afraid:2, then m_targetCreature is a scary Player -
			 * //     Flee() from them!
			 * //   Is FearLevel.Cautious:1, then m_targetCreature will be null from BaseAI_FindEnemy_Patch -
			 * //     continue with normal code to wander or whatever
			 * //   Is FearLevel.Unafraid:0, then
			 * //     continue with normal code to do the normal charge/circle/attack logic
			 * 
			 * ...
			 * OLDLABELS:
			 * 
			 * if(Fear.GetFearLevel(this, this.m_targetCreature) < 2) 
			 *	 goto NEWLABEL
			 * 
			 * MonsterAI_UpdateAI_Patch.RunAway(this, dt);
			 * 
			 * return true;
			 * 
			 * NEWLABEL:
			 * ...
			 *
			*/

			// Setup the labels so existing code jumps to the start of the new block of code, instead of jumping over it.

			var oldLabels = matcher.Labels;
			matcher.Labels = new List<Label>();

			matcher.CreateLabel(out var newLabel);

			matcher
						// (this,
						.Insert(new CodeInstruction(OpCodes.Ldarg_0))
						.AddLabels(oldLabels)
						.Advance(1)

						// this
						.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
						// .m_targetCreature)
						.InsertAndAdvance(new CodeInstruction(
							OpCodes.Ldfld,
							AccessTools.Field(
								typeof(MonsterAI),
								nameof(MonsterAI.m_targetCreature))))

					// BaseAIExtensions.GetFearLevel(this, this.m_targetCreature)
					.InsertAndAdvance(new CodeInstruction(
						OpCodes.Call,
						AccessTools.Method(
							typeof(MonsterExtensions),
							nameof(MonsterExtensions.GetFearLevel))))

					// constant: 2 (FearLevel.Afraid)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_2))

				// If (BaseAIExtensions.GetFearLevel(this, this.m_targetCreature) < FearLevel.Afraid)
				//   jump to NEWLABEL;
				.InsertAndAdvance(new CodeInstruction(OpCodes.Blt, newLabel))

					// Else

					// this
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))

					// dt
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))

				// MonsterAI_UpdateAI_Patch.RunAway(this, dt);
				.InsertAndAdvance(new CodeInstruction(
					OpCodes.Call,
					AccessTools.Method(
						typeof(MonsterExtensions),
						nameof(MonsterExtensions.RunAway))))


					// constant: 1 (true)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))

				// return true
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ret));

			// NEWLABEL ends up here

			return matcher.Instructions();
		}
	}
}
