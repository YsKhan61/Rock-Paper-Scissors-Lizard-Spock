using System.Collections;

namespace RPSLS.StateMachine.States.Base
{
    public abstract class StateBase
    {
        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal virtual IEnumerator Initialise()
        {
            yield break;
        }

        /// <summary>
        /// Perform the state.
        /// </summary>
        /// <returns></returns>
        internal virtual IEnumerator Perform()
        {
            yield break;
        }

        /// <summary>
        /// Finalize the state.
        /// </summary>
        /// <returns></returns>
        internal virtual IEnumerator Finalise()
        {
            yield break;
        }
    }
}