using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {
    public class CrossHairTarget : MonoBehaviour {

        Camera camMain;
        Ray ray;
        RaycastHit hit;

        void Start() {
            camMain = Camera.main;
        }

        void Update() {
            ray.origin = camMain.transform.position;
            ray.direction = camMain.transform.forward;
            Physics.Raycast(ray, out hit);
            transform.position = hit.point;
        }
    }
}