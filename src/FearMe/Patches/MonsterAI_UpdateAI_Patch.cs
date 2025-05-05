using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace FearMe.Patches
{
#if DEBUG
	[HarmonyDebug]
#endif
	[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
	public static class MonsterAI_UpdateAI_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator)

				/*
				 * Look for "this.m_fleeIfHurtWhenTargetCantBeReached" to find the code below, which is along with the other Flee() checks:
				 * 
				 * ...
				 * if (**m_fleeIfHurtWhenTargetCantBeReached** && m_targetCreature != null && m_timeSinceAttacking > 30f && m_timeSinceHurt < 20f)
				 * {
				 *	Flee(dt, m_targetCreature.transform.position);
				 *	m_lastKnownTargetPos = this.transform.position;
				 *	m_updateTargetTimer = 1f;
				 *	return true;
				 * }
				 * ...
				 */
				.MatchStartForward(
					  new CodeMatch(OpCodes.Ldarg_0)
					, new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand.ToString().Contains("fleeIfHurtWhenTargetCantBeReached"))
				)

				// Handled in Main - disables the mod
				.ThrowIfInvalid("Could not find location to patch in MonsterAI.UpdateAI")

				/*
				 * Insert this custom code:
				 * 
				 * // If GetFearLevel() is FearLevel.Afraid:2, then m_targetCreature is a scary Player - Flee() from them!
				 * // For FearLevel.Cautious:1, m_targetCreature will be null from BaseAI_FindEnemy_Patch, so continue with normal code to wander or whatever
				 * // For FearLevel.Unafraid:0, continue with normal code to do the normal charge/circle/attack logic
				 * // (Should check Main.Enabled too, but writing IL is a hassle, so relying on check in GetFearLevel instead)
				 * 
				 * if(Fear.GetFearLevel(this, this.m_targetCreature) < 2) 
				 *	goto LABEL
				 * 
				 * this.Flee(dt, this.m_targetCreature.transform.position);
				 * return true;
				 * 
				 * LABEL:
				 * ...
				 *
				 */

				// Create a label at the current location, which will get pushed down as code is inserted
				.CreateLabel(out Label label)

						// (this,
						.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))

						// this
						.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
						// .m_targetCreature)
						.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MonsterAI), nameof(MonsterAI.m_targetCreature))))

					// BaseAIExtensions.GetFearLevel(this, this.m_targetCreature)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MonsterExtensions), nameof(MonsterExtensions.GetFearLevel))))

					// constant: 2 (FearLevel.Afraid)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_2))

				// If Fear.GetFearLevel() < 2 GOTO label
				.InsertAndAdvance(new CodeInstruction(OpCodes.Blt, label))

				// this
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))

					// dt
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))

					// this
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
					// .m_targetCreature
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MonsterAI), "m_targetCreature")))
					// .transform
					.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), "transform")))
					// .position
					.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), "position")))

				// .Flee(dt, this.m_targetCreature.transform.position)
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BaseAI), "Flee")))
				// Return value not used, so clear it off the stack
				.InsertAndAdvance(new CodeInstruction(OpCodes.Pop))

				// constant: 1 (true)
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
				// return true
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ret))

				.Instructions();
		}
	}
}
