using System.Collections;
using RPSLS.Audio;
using RPSLS.Services;
using RPSLS.StateMachine.States.Base;
using RPSLS.UI.Screens;
using UnityEngine;

namespace RPSLS.StateMachine.States
{
    public class InitialState : StateBase
    {
        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Initialise()
        {
            yield return null;
            Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .ActivateThisScreen<MainMenuScreen>();
            yield return null;
            Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.BG_MUSIC);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}