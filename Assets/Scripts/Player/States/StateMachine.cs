using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {
    public class StateMachine {

        public State currentState;

        public void Initialize(State state) {
            currentState = state;
            currentState.Enter();
        }

        public void SwitchState(State nextState) {
            currentState.Exit();
            currentState = nextState;
            currentState.Enter();
        }

    }
}