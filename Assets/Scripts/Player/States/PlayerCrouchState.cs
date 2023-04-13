using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {
    public class PlayerCrouchState : PlayerBaseState {
        
        public PlayerCrouchState(PlayerCharacterManager cm, StateMachine _sm) : base(cm, _sm) {
            character = cm;
            stateMachine = _sm;
        }

        public override void Enter() {
            base.Enter();
            SetAnimation(character.CROUCH);
        }

        public override void LogicalUpdate() {
            base.LogicalUpdate();
            if(character.movementInput.magnitude > 0) {
                character.animator.SetFloat(character.SPEED, 1f, character.animDampTime, Time.deltaTime);
                Move(character.crouchSpeed);
            } else {
                character.animator.SetFloat(character.SPEED, 0f, character.animDampTime, Time.deltaTime);
				Move(0f);
			}
        }

        public override void HandleInput() {
            base.HandleInput();
            if(!character.isCrouching) {
                if(character.movementInput.magnitude > 0) {
                    if(character.isWalking) {
                        character.playerSM.SwitchState(character.walking);
                    } else {
						character.playerSM.SwitchState(character.running);
					}
                } else {
                    character.playerSM.SwitchState(character.idling);
                }
            }
        }

        public override void Exit() {
            base.Exit();
            SetAnimation(character.LOCOMOTION);
        }

    }
}