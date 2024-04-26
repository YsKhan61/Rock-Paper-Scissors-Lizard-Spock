using System.Collections;
using RPSLS.Miscellaneous;
using RPSLS.Services.Base;
using RPSLS.UI.Screens;
using UnityEngine;

namespace RPSLS.Services
{
    public class GameManagementService : ServiceBase
    {
        internal GameEnums.PlayableHandType CurrentPlayerSelection { get; set; }

        /// <summary>
        /// Reset the values of game state
        /// </summary>
        internal void ResetValues()
        {
            CurrentPlayerSelection = GameEnums.PlayableHandType.None;
            StartCoroutine(ResetTimerRoutine());
        }

        protected override void RegisterService() =>
            Bootstrap.RegisterService(this);

        private IEnumerator ResetTimerRoutine()
        {
            var eof = new WaitForEndOfFrame();
            var hudScreen = Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .GetScreen<GameplayHudScreen>();
            var value = 0F;

            while (value < 1F)
            {
                hudScreen.UpdateTimeBar(value += Time.deltaTime * 3F);
                yield return eof;
            }
        }
    }
}