using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {

	public class PlayerRunningState : PlayerBaseState {

		public PlayerRunningState(PlayerCharacterManager _c, StateMachine _sm) : base(_c, _sm) {
			character = _c;
			stateMachine = _sm;
		}

		public override void Enter() {
			base.Enter();
		}

		public override void HandleInput() {
			base.HandleInput();
			if(character.movementInput.magnitude == 0) {
				character.playerSM.SwitchState(character.idling);
			}
			if (walkAction.triggered) {
				stateMachine.SwitchState(character.walking);
			}
			if (character.isCrouching) {
				character.playerSM.SwitchState(character.crouching);
			}
		}

		public override void LogicalUpdate() {
			base.LogicalUpdate();
			character.animator.SetFloat(character.SPEED, 1f, character.animDampTime, Time.deltaTime);
		}

		public override void PhysicsUpdate() {
			base.PhysicsUpdate();
			Move(character.runSpeed);
		}

	}

}