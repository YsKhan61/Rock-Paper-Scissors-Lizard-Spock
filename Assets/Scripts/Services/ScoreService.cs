using RPSLS.GameData;
using RPSLS.Services.Base;
using RPSLS.UI.Screens;

namespace RPSLS.Services
{
    public class ScoreService : ServiceBase
    {
        private int _currentScore;
        private GameplayHudScreen _hudScreen;

        /// <summary>
        /// Reset the current score to 0.
        /// </summary>
        internal void ResetCurrentScore() =>
            _currentScore = 0;

        /// <summary>
        /// Increment the current score by the score step.
        /// </summary>
        /// <param name="scoreStep"></param>
        internal void IncrementCurrentScore(int scoreStep)
        {
            if (!_hudScreen)
                _hudScreen = Bootstrap.GetService<UserInterfaceService>()
                    .CurrentInterface.GetScreen<GameplayHudScreen>();
            _currentScore += scoreStep;
            _hudScreen.UpdateCurrentScore(_currentScore);
        }

        /// <summary>
        /// Update the high score if the current score is greater than the high score.
        /// </summary>
        internal void UpdateHighScore()
        {
            if (_currentScore > PlayerPrefsManager.HighScore)
                PlayerPrefsManager.HighScore = _currentScore;
        }


        protected override void RegisterService() =>
            Bootstrap.RegisterService(this);
    }
}