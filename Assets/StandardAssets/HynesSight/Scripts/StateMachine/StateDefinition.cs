using UnityEngine;

namespace HynesSight.StateMachine
{
    public class StateDefinition : ScriptableObject
	{
		public virtual void OnEnter(StateDefinition previousState, params object[] stateChangeData) { }
		public virtual void OnExit() { }
		public virtual void OnUpdate(float deltaTime) { }
		public virtual void OnFixedUpdate(float fixedDeltaTime) { }
		public virtual void OnNotify(params object[] notificationData) { }

		public virtual bool CanEnter(StateDefinition previousState)
		{
			return true;
		}
	}
}
