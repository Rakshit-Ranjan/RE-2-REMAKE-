using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPG {
    public class PlayerIdlingState : PlayerBaseState {

        public PlayerIdlingState(PlayerCharacterManager _character, StateMachine _sm) : base(_character, _sm) {
            character = _character;
            stateMachine = _sm;
        }

        public override void Enter() {
            base.Enter(); 
		}

		public override void LogicalUpdate() {
            base.LogicalUpdate();
            character.animator.SetFloat(character.SPEED, 0, character.animDampTime, Time.deltaTime);
            
        }

        public override void HandleInput() {
            base.HandleInput();
            if(character.movementInput.magnitude > 0) {
                if (character.isWalking) {
                    character.playerSM.SwitchState(character.walking);
                } else {
                    character.playerSM.SwitchState(character.running);
                }
            }
            if(character.isCrouching) {
                character.playerSM.SwitchState(character.crouching);
            }
        }

        public override void PhysicsUpdate() {
            base.PhysicsUpdate();
            Move(0f);
        }



    }
}