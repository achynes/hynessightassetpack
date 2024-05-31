using UnityEngine;
using System.Collections.Generic;

namespace HynesSight.StateMachine
{
    // Populate this enum with the necessary StateTypes for the game being made.
    public enum StateType
    {
        None = 0,
        //Example = 1
    }

    /// <summary>
    /// A generic StateMachine. Override the State_Base class and add them to the StateClasses Dictionary to create new states.
    /// To use this, it is attached as a Component to a GameObject, where you can specify which states to allow on that instance of the StateMachine.
    /// </summary>
    public sealed class StateMachine : MonoBehaviour
    {
        // Populate this Dictionary with keys from the StateType enum and the corresponding classes inherited from State_Base.
        private static Dictionary<StateType, System.Type> StateClasses = new Dictionary<StateType, System.Type>()
        {
            //{StateType.Example, typeof(State_Example) },
        };

        private Dictionary<StateType, State_Base> StateInstances;

        [SerializeField]
        private StateType[] AvailableStates;

        [SerializeField]
        private StateType StartingState;

		public bool _useUnscaledDeltaTime;

        public State_Base CurrentState { get; private set; }

        public void Init()
        {
            if (StateClasses.Count == 0)
            {
                return;
            }

            StateInstances = new Dictionary<StateType, State_Base>(StateClasses.Count);

            for (int n = AvailableStates.Length - 1; n > -1; n--)
            {
                if (!StateClasses.ContainsKey(AvailableStates[n]))
                {
                    Debug.LogError("Trying to instantiate a StateType that is not in the StateClasses Dictionary.");
                    continue;
                }

                State_Base StateInstance = (State_Base)System.Activator.CreateInstance(StateClasses[AvailableStates[n]]);
                StateInstances.Add(AvailableStates[n], StateInstance);
                
                if (null == CurrentState)
                {
                    CurrentState = StateInstance;
                }
            }

            if (StateInstances.ContainsKey(StartingState))
            {
                CurrentState = StateInstances[StartingState];
            }
            else
            {
                Debug.LogError("StateInstances does not contain the StartingState specified.");
            }
        }
        
        public bool ChangeState(StateType stateType_, params object[] stateChangeData_)
        {
            StateType previousStateType = StateType.None;
            State_Base nextState = StateInstances[stateType_];

            if (!nextState.CanEnter(previousStateType))
            {
                return false;
            }

            if (null != CurrentState)
            {
                previousStateType = CurrentState.StateType;
                CurrentState.OnExit();
            }

            CurrentState = nextState;
            CurrentState.OnEnter(previousStateType, stateChangeData_);

            return true;
        }

        private void Update()
        {
            if (null != CurrentState)
            {
                CurrentState.Update(_useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime);
            }
        }

		private void FixedUpdate()
		{
			if (null != CurrentState)
			{
				CurrentState.FixedUpdate(Time.fixedDeltaTime);
			}
		}

		public void NotifyState(params object[] notificationData_)
        {
            CurrentState.Notify(notificationData_);
        }
    }
}