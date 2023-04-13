using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Networking;

namespace RPG {
    public class PlayerAimingState : PlayerBaseState {
        
        public PlayerAimingState(PlayerCharacterManager c, StateMachine _sm) : base(c, _sm) {
            character = c;
            stateMachine = _sm;
        }

        public override void Enter() {
            base.Enter();
			character.readyToShoot = true;
			character.crossHair.SetActive(true);
			character.weapon = character.GetComponentInChildren<RaycastWeapon>();
			character.animator.SetBool("isAiming", true);
            character.aimCam.GetComponent<CinemachineFreeLook>().Priority = 1;
            character.normalCam.GetComponent<CinemachineFreeLook>().Priority = 0;
			character.aimLayer.rig.weight = 1f;
            character.bodyLayer.rig.weight = 1f;
			if (character.aimCamLookAt.CompareTag("RS")) {
				character.leftHandGrip.data.target = character.leftHandGripWhileAiming_RS;
				character.rightHandGrip.data.target = character.rightHandGripWhileAiming_RS;
			} else {
				character.leftHandGrip.data.target = character.leftHandGripWhileAiming_LS;
				character.rightHandGrip.data.target = character.rightHandGripWhileAiming_LS;
			}
            character.rigBuilder.Build();
			character.weapon.freeLookCam = character.aimCam.GetComponent<Cinemachine.CinemachineFreeLook>();

		}

		public override void HandleInput() {
            base.HandleInput();

			//CHECK FOR WEAPON TYPE(AUTO, SEMI-AUTOMATIC GLOCK ETC...
			shootAction.performed += s => character.isShooting = true;
			shootAction.canceled += s => character.isShooting = false;
			character.animator.SetBool(character.IS_SHOOTING, character.isShooting);
			if(character.readyToShoot && character.isShooting) {
				character.weapon.StartFiring();
			}
			//weapon.UpdateBullets(Time.deltaTime);

			//CHANGING SHOULDERS ON INPUT
			if(switchShoulderAction.triggered) {
				ChangeShoulders(character.aimCamLookAt.tag);
			}
		}
        public override void LogicalUpdate() {
            ClampAimValues();

			#region Rotating and Moving
			Vector3 rotDirection = character.camObj.forward;
			rotDirection.y = 0;
			rotDirection.Normalize();
			//setting our direction to target direction
			Quaternion tr = Quaternion.LookRotation(rotDirection);
			character.transform.rotation = Quaternion.Slerp(character.transform.rotation, tr, character.rotSpeed * Time.deltaTime); // rotating with camera
			Move(character.aimSpeed); // movement
            character.animator.SetFloat("Horizontal", Mathf.RoundToInt(character.movementInput.x), character.animDampTime, Time.deltaTime);
            character.animator.SetFloat("Vertical", Mathf.RoundToInt(character.movementInput.y), character.animDampTime, Time.deltaTime);
			#endregion

            if(character.isCrouching) {
				character.weapon.recoilEffect = 0.5f;
                character.animator.SetFloat(character.STRAFE_SPEED, 0f, character.animDampTime, Time.deltaTime);
            } else {
				character.weapon.recoilEffect = 1f;
				character.animator.SetFloat(character.STRAFE_SPEED, 1f, character.animDampTime, Time.deltaTime);
			}

			if (!character.isAiming) {
                character.playerSM.SwitchState(character.idling);
			}

		}

        public override void Exit() {
            base.Exit();
			character.animator.SetBool("isAiming", false);
            character.aimCam.GetComponent<CinemachineFreeLook>().Priority = 0;
			character.normalCam.GetComponent<CinemachineFreeLook>().Priority = 1;
			character.aimLayer.rig.weight = 0f;
			character.bodyLayer.rig.weight = 0f;
			character.leftHandGrip.data.target = character.leftHandGripHoldPose;
			character.rightHandGrip.data.target = character.rightHandGripHoldPose;
			character.rigBuilder.Build();
			character.crossHair.SetActive(false);
		}

		private void ChangeShoulders(string shoulder) {
			if(shoulder.Equals("RS")) {
				character.weaponAimPos.position = character.LS_WeaponAimPos.position;
				
				character.leftHandGrip.data.target = character.leftHandGripWhileAiming_LS;
				character.rightHandGrip.data.target = character.rightHandGripWhileAiming_LS;
				character.rigBuilder.Build();
				
				character.aimCamLookAt = character.aimCamLookAt_LS;
				SetAimCam(character.aimCamLookAt);
			} else {
				character.weaponAimPos.position = character.RS_WeaponAimPos.position;

				character.leftHandGrip.data.target = character.leftHandGripWhileAiming_RS;
				character.rightHandGrip.data.target = character.rightHandGripWhileAiming_RS;
				character.rigBuilder.Build();
				
				character.aimCamLookAt = character.aimCamLookAt_RS;
				SetAimCam(character.aimCamLookAt);
			}
		}

    }
}