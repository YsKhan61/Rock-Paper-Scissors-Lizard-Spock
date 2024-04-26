using System.Collections;
using RPSLS.Audio;
using RPSLS.Entities;
using RPSLS.Miscellaneous;
using RPSLS.Services;
using RPSLS.StateMachine.States.Base;
using RPSLS.UI.Screens;
using UnityEngine;

namespace RPSLS.StateMachine.States
{
    public class ValidateMoveState : StateBase
    {
        private bool? _currentResult;

        /// <summary>
        /// Initialize the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Initialise()
        {
            var interfaceService = Bootstrap.GetService<UserInterfaceService>();
            interfaceService.ToggleInteractions(false);

            var fsmService = Bootstrap.GetService<StateMachineService>();
            _currentResult = CalculateResults(Bootstrap.GetService<GameManagementService>().CurrentPlayerSelection);

            interfaceService.CurrentInterface.GetScreen<GameplayHudScreen>().ShowVignette(_currentResult);

            if (!_currentResult.HasValue)
                Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.TIE);
            else if (_currentResult.Value)
                Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.WON);
            else
            {
                Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.LOST);
                Handheld.Vibrate();
            }

            yield return fsmService.StartCoroutine(Perform());
        }

        /// <summary>
        /// Perform the state.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerator Perform()
        {
            var fsmService = Bootstrap.GetService<StateMachineService>();
            yield return new WaitForSeconds(3F);
            if (!_currentResult.HasValue)
            {
                fsmService.CurrentFsm.SetState(new PlayState());
                Bootstrap.GetService<ScoreService>().IncrementCurrentScore(0);
                Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.REFILL);
            }
            else if (_currentResult.Value)
            {
                fsmService.CurrentFsm.SetState(new PlayState());
                Bootstrap.GetService<ScoreService>().IncrementCurrentScore(1);
                Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.REFILL);
            }
            else
            {
                fsmService.CurrentFsm.SetState(new InitialState());
                Bootstrap.GetService<ScoreService>().UpdateHighScore();
                Bootstrap.GetService<ScoreService>().ResetCurrentScore();
            }

            Bootstrap.GetService<GameManagementService>().ResetValues();
            Bootstrap.GetService<UserInterfaceService>().ToggleInteractions(true);
        }

        private bool? CalculateResults(GameEnums.PlayableHandType playerSelectedType)
        {
            var hudScreen = Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .GetScreen<GameplayHudScreen>();
            if (playerSelectedType == GameEnums.PlayableHandType.None)
            {
                hudScreen.ShowOutcomeMessage("You didn't play your hand!");
                return false;
            }

            var cpuSelectedOption = (GameEnums.PlayableHandType) Random.Range(1, 6);
            var playerHand = PlayableHandBase.FormPlayableHand(playerSelectedType);
            var cpuHand = PlayableHandBase.FormPlayableHand(cpuSelectedOption);
            var result = playerHand.CheckWinAgainstOtherHand(cpuHand, out var message);

            hudScreen.UpdateCPUTurnSprite(cpuSelectedOption);
            hudScreen.ShowOutcomeMessage(message);
            return result;
        }
    }
}