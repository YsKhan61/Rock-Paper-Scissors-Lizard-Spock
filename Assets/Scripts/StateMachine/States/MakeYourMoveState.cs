using System.Collections;
using RPSLS.Services;
using RPSLS.StateMachine.States.Base;
using RPSLS.UI.Screens;
using UnityEngine;

namespace RPSLS.StateMachine.States
{
    public class MakeYourMoveState : StateBase
    {
        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Initialise()
        {
            var fsmService = Bootstrap.GetService<StateMachineService>();
            yield return fsmService.StartCoroutine(TimerRoutine());
            fsmService.CurrentFsm.SetState(new ValidateMoveState());
        }

        private IEnumerator TimerRoutine()
        {
            var eof = new WaitForEndOfFrame();
            var hudScreen = Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .GetScreen<GameplayHudScreen>();
            var value = 1F;
            while (value > 0)
            {
                hudScreen.UpdateTimeBar(value -= Time.deltaTime);
                yield return eof;
            }
        }
    }
}