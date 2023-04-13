using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG {
	public abstract class State {

		protected StateMachine stateMachine;

		public State(StateMachine sm) { stateMachine = sm; }

		public virtual void Enter() { }

		public virtual void HandleInput() { }

		public virtual void LogicalUpdate() { }

		public virtual void PhysicsUpdate() { }

		public virtual void OnDrawGizmosSelected() { }

		public virtual void Exit() { }


	}
}