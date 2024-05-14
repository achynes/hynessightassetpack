using UnityEngine;
using System.Collections.Generic;

namespace HynesSight.StateMachine
{
    public sealed class StateMachine : MonoBehaviour
    {
		// The state machine will enter the 0 entry state on start.
		[SerializeField]
        List<StateDefinition> _states = null;

		List<StateDefinition> _stateInstances = null;

		public StateDefinition CurrentState { get; private set; }

		void Awake()
        {
            if (_states.Count == 0)
                return;

            _stateInstances = new List<StateDefinition>(_states.Count);

            foreach (StateDefinition state in _states)
            {
				StateDefinition newStateDefinition = (StateDefinition)System.Activator.CreateInstance(state.GetType());
				_stateInstances.Add(newStateDefinition);

				if (null == CurrentState)
					CurrentState = newStateDefinition;
            }
        }

		public bool ChangeState(StateDefinition nextState, params object[] stateChangeData)
        {
            if (!nextState.CanEnter(CurrentState))
                return false;

			StateDefinition previousState = null;
            if (null != CurrentState)
			{
				previousState = CurrentState;
                CurrentState.OnExit();
			}

            CurrentState = nextState;
            CurrentState.OnEnter(previousState, stateChangeData);

            return true;
        }

        private void Update()
        {
            if (null != CurrentState)
                CurrentState.OnUpdate(Time.deltaTime);
        }

		private void FixedUpdate()
		{
			if (null != CurrentState)
				CurrentState.OnFixedUpdate(Time.fixedDeltaTime);
		}

		public void NotifyState(params object[] notificationData)
        {
            CurrentState.OnNotify(notificationData);
        }
    }
}