using Cinemachine;
using PsychoticLab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

namespace RPG {
    public abstract class PlayerBaseState : State {

        protected Vector3 moveDirection;

		protected PlayerCharacterManager character;
        public InputAction moveAction;
        public InputAction aimAction;
        public InputAction walkAction;
        public InputAction mouseAction;
        public InputAction crouchAction;
        public InputAction takeCoverAction;
        public InputAction shootAction;
        public InputAction switchShoulderAction;


		public PlayerBaseState(PlayerCharacterManager _characterManager, StateMachine _stateMachine) : base(_stateMachine) {
            character = _characterManager;
            stateMachine = _stateMachine;
            moveAction = character.inputAction.actions["Player Locomotion"];
            walkAction = character.inputAction.actions["Walk"];
            aimAction = character.inputAction.actions["Aim"];
            crouchAction = character.inputAction.actions["Crouch"];
            mouseAction = character.inputAction.actions["Mouse"];
            takeCoverAction = character.inputAction.actions["TakeCover"];
            shootAction = character.inputAction.actions["Shoot"];
            switchShoulderAction = character.inputAction.actions["SwitchShoulder"];
		}

        public override void Enter() {
            base.Enter();

        }

        public override void HandleInput() {
            base.HandleInput();
			character.movementInput = moveAction.ReadValue<Vector2>().normalized; // get WASD input
            character.mouseInput = mouseAction.ReadValue<Vector2>().normalized;
            if (walkAction.triggered) character.isWalking = !character.isWalking; // get walk input
            if (crouchAction.triggered) character.isCrouching = !character.isCrouching; // get crouch input
            if(takeCoverAction.triggered) {
                character.checkingForCover = true;
            }
            if (aimAction.triggered) {
                character.playerSM.SwitchState(character.aiming);
            }
            if (aimAction.phase == InputActionPhase.Performed) {
                character.isAiming = true;
            } else {
                character.isAiming = false;
            }

            if(character.inCover) {
                if(takeCoverAction.triggered) {
                    character.inCover = false;
                    character.checkingForCover = false;
                    character.isInteracting = false;
                    character.animator.SetBool(character.IN_COVER, false);
                }
            }
        }

        public override void LogicalUpdate() {
            base.LogicalUpdate();
            ClampAimValues();
            character.animator.SetFloat("Velocity", character.rb.velocity.sqrMagnitude, character.animDampTime, Time.deltaTime);
            RotatePlayer();
            if(character.checkingForCover) {
                CheckForCover();
            }
        }

        public void Move(float speed) {
            if (!character.isInteracting) {
                moveDirection = character.camObj.right * character.movementInput.x;
                moveDirection += character.camObj.forward * character.movementInput.y;
                moveDirection.y = 0;
                moveDirection.Normalize();
                Vector3 vel = Vector3.ProjectOnPlane(moveDirection, Vector3.zero);
                character.rb.velocity = speed * vel;
            } 
            else {
                if(character.inCover) {
                    RaycastHit bh;
                    // USE TO CHECK WHICH TYPE OF COVER
                    bool bodyHit = Physics.Raycast(character.coverHelperTransform.position, character.coverHelperTransform.forward, out bh, 1f, character.whatIsCover);
					//USE TO RESTRAIN MOVEMENT SO THAT PLAYER DOESNT MOVE OUTSIDE THE COVER
                    bool rightHit = Physics.Raycast(character.coverHelperRightTransform.position, character.coverHelperRightTransform.forward, 1f, character.whatIsCover);
                    bool leftHit = Physics.Raycast(character.coverHelperLeftTransform.position, character.coverHelperLeftTransform.forward, 1f, character.whatIsCover);
                    if (bh.collider != null) {
                        if (bh.collider.CompareTag(character.LOW_COVER)) {
                            character.animator.SetFloat(character.COVER_TYPE, 0, character.animDampTime, Time.deltaTime);
                        } else if (bh.collider.CompareTag(character.HIGH_COVER)) {
                            character.animator.SetFloat(character.COVER_TYPE, 1, character.animDampTime, Time.deltaTime);
                        }
                    }
                    //CHECK BORDERS OF COVER
					if (!rightHit && leftHit) {
                        character.movementInput.x = Mathf.Clamp(character.movementInput.x, -1, 0);
                    } else if(rightHit && !leftHit) {
						character.movementInput.x  = Mathf.Clamp(character.movementInput.x, 0, 1);
                    }

                    //MOVEMENT
					moveDirection = character.transform.right * character.movementInput.x;
					moveDirection.y = 0;
					moveDirection.Normalize();
					Vector3 vel = Vector3.ProjectOnPlane(moveDirection, Vector3.zero);
					character.rb.velocity = character.coverSpeed * vel;
					character.animator.SetFloat(character.HORIZONTAL, character.movementInput.x, character.animDampTime, Time.deltaTime);
				}
            }
        }

        public void RotatePlayer() {
            if (!character.isInteracting) {
                // getting target direction
                Vector3 rotDirection = character.camObj.right * character.movementInput.x;
                rotDirection += character.camObj.forward * character.movementInput.y;
                rotDirection.y = 0;
                rotDirection.Normalize();
                if (rotDirection == Vector3.zero) {
                    rotDirection = character.transform.forward;
                }
                //setting our direction to target direction
                Quaternion tr = Quaternion.LookRotation(rotDirection);
                character.transform.rotation = Quaternion.Slerp(character.transform.rotation, tr, character.rotSpeed * Time.deltaTime);
            } else {
                if(character.inCover) {
                    RaycastHit hit;
                    Physics.Raycast(character.coverHelperTransform.position, character.coverHelperTransform.forward, out hit, 1f, character.whatIsCover);
                    Quaternion tr = Quaternion.LookRotation(-hit.normal);
                    character.transform.rotation = Quaternion.Slerp(character.transform.rotation, tr, character.rotSpeed);
                }
            }

        }

        public void SetAnimation(int hash) {
            character.animator.CrossFadeInFixedTime(hash, character.animDampTime);
        }

        public void ClampAimValues() {
			character.aimCamLookAt.rotation *= Quaternion.AngleAxis(character.mouseInput.x * character.aimSensitivityX * Time.deltaTime, Vector3.up);
			character.aimCamLookAt.rotation *= Quaternion.AngleAxis(character.mouseInput.y * character.aimSensitivityY * Time.deltaTime, Vector3.right);
            var angles = character.aimCamLookAt.localEulerAngles;
            angles.z = 0;
            var angle = character.aimCamLookAt.localEulerAngles.x;
            if(angle > 180 && angle < 340) {
                angles.x = 340;
            } else if(angle < 180 && angle > 40) {
                angles.x = 40;
            }

            character.aimCamLookAt.localEulerAngles = angles;

		}

        public void CheckForCover() {
            RaycastHit hit;
            bool coverFound = Physics.Raycast(character.transform.position, character.transform.forward, out hit, character.checkForCoverDist, character.whatIsCover);
            if(coverFound) {
                MoveToCover(hit);
            } else {
                character.checkingForCover = false;
            }
        }

        public void MoveToCover(RaycastHit hit) {
            character.isInteracting = true;
            //Debug.Log(hit.point);
            float distToTarget = Vector3.Distance(character.transform.position, hit.point);
            if(distToTarget > character.moveToCoverStopDist) {
                character.rb.AddForce(character.transform.forward * character.moveToCoverSpeed * Time.deltaTime, ForceMode.Force);

			} else {
                character.rb.velocity = Vector3.zero;
				character.checkingForCover = false;
                character.inCover = true;
				character.animator.SetBool(character.IN_COVER, true);
            }
        }



        public void SetAimCam(Transform t) {
            CinemachineFreeLook ac = character.aimCam.GetComponent<CinemachineFreeLook>();
            ac.Follow = t;
            ac.LookAt = t;
        }

		public override void OnDrawGizmosSelected() {
            base.OnDrawGizmosSelected();
            Gizmos.DrawLine(character.transform.position, character.transform.position + (character.transform.forward * character.checkForCoverDist));
        }

    }
}