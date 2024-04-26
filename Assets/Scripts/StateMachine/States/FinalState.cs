using System.Collections;
using RPSLS.StateMachine.States.Base;
using UnityEngine;

namespace RPSLS.StateMachine.States
{
    public class FinalState : StateBase
    {
        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Initialise()
        {
            Application.Quit(0);
            yield break;
        }
    }
}