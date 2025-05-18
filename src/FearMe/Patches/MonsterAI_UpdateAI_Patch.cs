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
			 * Starting from the beginning of the method,
			 * look for "this.m_fleeIfHurtWhenTargetCantBeReached" to find the code below,
			 * which is along with the other Flee() checks:
			 * 
			 *   ...
			 *   if (**m_fleeIfHurtWhenTargetCantBeReached** && m_targetCreature != null && m_timeSinceAttacking > 30f && m_timeSinceHurt < 20f)
			 *   {
			 *	   Flee(dt, m_targetCreature.transform.position);
			 *	   m_lastKnownTargetPos = this.transform.position;
			 *	   m_updateTargetTimer = 1f;
			 *	   return true;
			 *   }
			 *   ...
			 *   
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

				// Handled in Main - disables the mod.
				// Might happen if some other mod changes this function,
				// or if the base game is changed.
				.ThrowIfInvalid("Could not find location to patch in MonsterAI.UpdateAI");


			/*
			 * Insert this custom IL before the found code:
			 * 
			 * ...
			 * OLDLABELS:
			 * 
			 * if(!MonsterAI_UpdateAI_Patch.RunAway(this, dt)) 
			 *	 goto NEWLABEL
			 * 
			 * return true;
			 * 
			 * NEWLABEL:
			 * ...
			 *
			*/

			// Setup the labels so existing code jumps to the start of the new block of code, instead of jumping over it.

			// Where code was jumping to before - being moved to the new inserted instructions
			var oldLabels = matcher.Labels;
			matcher.Labels = new List<Label>();

			// Label the current instruction to use to jump to later,
			// before inserting any new instructions,
			// which will push this down the stack after the new instructions.
			matcher.CreateLabel(out var newLabel);


			matcher
					// (this
					.Insert(new CodeInstruction(OpCodes.Ldarg_0))

				// Move the old labels here, to the start of this inserted code,
				// so that the existing code jumps to this new instruction
				// instead of where it was jumping to before, which was after this code
				.AddLabels(oldLabels)
				.Advance(1)

					// , dt)
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))

				// MonsterExtensions.RunAway(this, dt)
				.InsertAndAdvance(new CodeInstruction(
					OpCodes.Call,
					AccessTools.Method(
						typeof(MonsterAI_UpdateAI_Patch),
						nameof(MonsterAI_UpdateAI_Patch.RunAway))))


				// if (!MonsterExtensions.RunAway(this, dt))
				//   jump to NEWLABEL;
				.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, newLabel))
						// Else

						// constant: 1 (true)
						.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))

					// return true;
					.InsertAndAdvance(new CodeInstruction(OpCodes.Ret));

			// NEWLABEL ends up here

			return matcher.Instructions();
		}

		// Called from the inserted IL.
		// Put as much of the custom logic as possible here, to simplify the custom IL coding
		public static bool RunAway(this BaseAI ai, float dt)
		{
			bool fleeing = false;

			var target = ai.GetTargetCreature();
			var fearLevel = ai.GetFearLevel(target);
			if (fearLevel == FearLevel.Afraid)
			{
				fleeing = true;
				ai.Flee(dt, target.transform.position);
			}

			return fleeing;
		}
	}
}
