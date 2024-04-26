using RPSLS.StateMachine.States.Base;
using UnityEngine;

namespace RPSLS.StateMachine.Base
{
    public abstract class StateMachineBase
    {
        protected StateBase CurrentState;
        private readonly MonoBehaviour _referencedBehaviour;

        /// <summary>
        /// Set the current state of the state machine.
        /// </summary>
        /// <param name="newState"></param>
        internal void SetState(StateBase newState)
        {
            CurrentState = newState;
            _referencedBehaviour.StartCoroutine(CurrentState.Initialise());
        }

        protected StateMachineBase(MonoBehaviour referencedBehaviour) =>
            _referencedBehaviour = referencedBehaviour;

    }
}