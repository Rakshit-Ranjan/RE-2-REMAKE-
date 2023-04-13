using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {
    public class PlayerWalkingState : PlayerBaseState {
        
        public PlayerWalkingState(PlayerCharacterManager _cm, StateMachine _sm) : base(_cm, _sm) {
            character = _cm;
            stateMachine = _sm;
        }

        public override void Enter() {
            base.Enter();
            character.isWalking = true;
		}

		public override void HandleInput() {
            base.HandleInput(); 
            if (character.movementInput.magnitude == 0) {
				character.playerSM.SwitchState(character.idling);
			}
            if(character.isCrouching) {
                character.playerSM.SwitchState(character.crouching);
            }
			if (walkAction.triggered) {
                stateMachine.SwitchState(character.running);
            }
        }

        public override void LogicalUpdate() {
            base.LogicalUpdate();
            character.animator.SetFloat(character.SPEED, 0.5f, character.animDampTime, Time.deltaTime);
        }

        public override void PhysicsUpdate() {
            base.PhysicsUpdate();
            Move(character.walkSpeed);
        }

        public override void Exit() {
            base.Exit();
        }

    }
}