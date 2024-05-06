namespace HynesSight.StateMachine
{
    public abstract class State_Base
    {
#pragma warning disable 0414
        private StateMachine _OwnerStateMachine;
#pragma warning restore 041
        
        /// <summary>
        /// Override this to set the StateType of each inherited state.
        /// </summary>
        public virtual StateType StateType { get { return StateType.None; } }

        public virtual void OnEnter(StateType previousState, params object[] stateChangeData) { }
        public virtual void OnExit() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime) { }
		public virtual void Notify(params object[] notificationData) { }
        
        /// <summary>
        /// This function is called once when the state object is instantiated along with the StateMachine. Don't forget to call the base method when overriding.
        /// </summary>
        public virtual void Init(StateMachine OwnerStateMachine)
        {
            _OwnerStateMachine = OwnerStateMachine;
        }

        /// <summary>
        /// Override this function to specify unique conditions under which the state can be entered.
        /// </summary>
        /// <typeparam name="T">The type of the state being transitioned from.</typeparam>
        public virtual bool CanEnter(StateType previousState)
        {
            return true;
        }
    }
}
