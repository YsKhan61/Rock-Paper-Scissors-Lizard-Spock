using System.Collections;
using RPSLS.Services;
using RPSLS.StateMachine.States.Base;
using RPSLS.UI.Screens;
using UnityEngine;

namespace RPSLS.StateMachine.States
{
    public class PlayState : StateBase
    {
        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Initialise()
        {
            Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface.ActivateThisScreen<GameplayHudScreen>();
            yield return Bootstrap.GetService<StateMachineService>().StartCoroutine(Perform());
        }

        /// <summary>
        /// Perform the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Perform()
        {
            yield return new WaitForSeconds(.5F);
            var hudScreen = Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .GetScreen<GameplayHudScreen>();
            hudScreen.BounceCpuHand();
            yield return hudScreen.ShowCountdownTimer();
            Bootstrap.GetService<StateMachineService>().CurrentFsm.SetState(new MakeYourMoveState());
        }
    }
}