using System.Collections;
using System.Collections.Generic;
using RPSLS.Miscellaneous;
using RPSLS.Services;
using RPSLS.UI.Base;
using RPSLS.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

namespace RPSLS.UI.Screens
{
    public class GameplayHudScreen : ScreenBase
    {
        [SerializeField] private Transform optionsParent;
        [SerializeField] private List<Sprite> iconSprites;
        [SerializeField] private CustomSlider timeBar;
        [SerializeField] private HorizontalLayoutGroup optionsLayoutGroup;
        [SerializeField] private TextMeshProUGUI countdownTmp;
        [SerializeField] private TextMeshProUGUI currentScoreTmp;
        [SerializeField] private Image cpuHandImg;
        [SerializeField] private Image vignetteImg;
        [SerializeField] private CaptionCreator gameCaptions;
        [SerializeField] private Gradient handLostGradient;
        [SerializeField] private Gradient handWonGradient;
        [SerializeField] private Gradient handTieGradient;
        [SerializeField] private AnimationCurve bounceCurve;

        private const string OptionsItemKey = "PlayableItem";
        private const int OptionsCount = 5;

        private List<PlayableItemUI> _playableOptions;
        private Vector3 _initialCpuHandPos;

        /// <summary>
        /// On back key pressed.
        /// </summary>
        public override void OnBackKeyPressed() =>
            PreviousScreen(true);

        /// <summary>
        /// Show the countdown timer by starting a coroutine.
        /// </summary>
        /// <returns></returns>
        internal Coroutine ShowCountdownTimer() =>
            StartCoroutine(CountdownRoutine());

        /// <summary>
        /// Update the time bar value.
        /// </summary>
        /// <param name="val"></param>
        internal void UpdateTimeBar(float val) =>
            timeBar.Value = val;

        /// <summary>
        /// Update the current score.
        /// </summary>
        /// <param name="score"></param>
        internal void UpdateCurrentScore(int score) =>
            currentScoreTmp.text = $"Current Score '{score}'";

        /// <summary>
        /// Set the player option type.
        /// </summary>
        /// <param name="handType">which playable hand is set</param>
        internal void SetPlayerOptionType(GameEnums.PlayableHandType handType) =>
            _playableOptions.ForEach(item => item.ToggleScale(item.HandType == handType));

        /// <summary>
        /// Show the outcome message.
        /// </summary>
        /// <param name="message"></param>
        internal void ShowOutcomeMessage(string message)
        {
            gameCaptions.TranslateCaptionText(message);
            Debug.LogError(message.ToColoredString(Color.green));
        }

        /// <summary>
        /// Update the CPU turn sprite.
        /// </summary>
        /// <param name="handType"></param>
        internal void UpdateCPUTurnSprite(GameEnums.PlayableHandType handType) =>
            cpuHandImg.sprite = handType == GameEnums.PlayableHandType.None
                ? iconSprites[5]
                : GetIconSprite(handType);

        /// <summary>
        /// Show the vignette effect
        /// </summary>
        /// <param name="hasPlayerWon"></param>
        internal void ShowVignette(bool? hasPlayerWon) =>
            StartCoroutine(VignetteRoutine(hasPlayerWon));

        /// <summary>
        /// Show the bounce effect on CPU hand.
        /// </summary>
        internal void BounceCpuHand() =>
            StartCoroutine(BounceRoutine());

        protected override void InitializeScreen()
        {
            base.InitializeScreen();
            _playableOptions = new List<PlayableItemUI> {null, null, null, null, null};
            PopulatePlayerOptions();
            _initialCpuHandPos = cpuHandImg.transform.position;
        }

        protected internal override void EnableScreen()
        {
            base.EnableScreen();
            UpdateCPUTurnSprite(GameEnums.PlayableHandType.None);
            UpdateTimeBar(1F);
        }

        protected internal override void DisableScreen()
        {
            base.DisableScreen();
            _playableOptions.ForEach(item =>
            {
                if (item) item.ToggleScale(false);
            });
        }

        private void PopulatePlayerOptions()
        {
            // I could have done this in synchronous fashion, but thought of doing it asynchronously
            var prm = new InstantiationParameters(Vector3.zero, Quaternion.identity, optionsParent);
            var count = OptionsCount;
            while (count-- > 0)
                if (!Bootstrap.GetService<AssetService>().LoadAndInstantiate(OptionsItemKey, prm))
                    Debug.LogError("Asset was not found");
            StartCoroutine(UpdateOptionValuesRoutine());
        }

        private IEnumerator UpdateOptionValuesRoutine()
        {
            yield return new WaitUntil(() => optionsParent.childCount == OptionsCount);
            optionsLayoutGroup.enabled = true;
            yield return null;
            var count = OptionsCount;
            PlayableItemUI playableItemUI;
            GameEnums.PlayableHandType currentHandType;
            while (count-- > 0)
            {
                currentHandType = (GameEnums.PlayableHandType) (count + 1);
                playableItemUI = optionsParent.GetChild(count).GetComponent<PlayableItemUI>();
                playableItemUI.SetValues(GetIconSprite(currentHandType), currentHandType);
                _playableOptions[count] = playableItemUI;
            }

            optionsLayoutGroup.enabled = false;
        }

        private void DePopulatePlayerOptions()
        {
            var count = _playableOptions.Count;
            while (count-- > 0)
                Destroy(_playableOptions[count]?.gameObject);
        }

        private IEnumerator CountdownRoutine()
        {
            var wait = new WaitForSeconds(.75F);
            var countdown = 3;
            while (countdown > 0)
            {
                countdownTmp.text = $"{countdown--}";
                yield return wait;
            }

            countdownTmp.text = string.Empty;
        }

        private IEnumerator VignetteRoutine(bool? hasPlayerWon)
        {
            var progress = 0F;
            var eof = new WaitForEndOfFrame();
            var targetGradient = hasPlayerWon.HasValue
                ? hasPlayerWon.Value
                    ? handWonGradient
                    : handLostGradient
                : handTieGradient;
            const float factor = .35F;
            while (progress < 1F)
            {
                vignetteImg.color = targetGradient.Evaluate(progress);
                yield return eof;
                progress += Time.deltaTime * factor;
            }

            vignetteImg.color = targetGradient.Evaluate(1F);
        }

        private IEnumerator BounceRoutine()
        {
            var progress = 0F;
            var finalProgress = bounceCurve[bounceCurve.length - 1].time;
            var eof = new WaitForEndOfFrame();
            var finalPosition = _initialCpuHandPos;
            finalPosition.y += 100F;

            cpuHandImg.transform.position = Vector3.Lerp(_initialCpuHandPos, finalPosition,
                bounceCurve.Evaluate(progress));

            while (progress < finalProgress)
            {
                cpuHandImg.transform.position = Vector3.Lerp(_initialCpuHandPos, finalPosition,
                    bounceCurve.Evaluate(progress));
                yield return eof;
                progress += Time.deltaTime;
            }

            cpuHandImg.transform.position = Vector3.Lerp(_initialCpuHandPos, finalPosition,
                bounceCurve.Evaluate(finalProgress));
        }

        private Sprite GetIconSprite(GameEnums.PlayableHandType handType) =>
            handType switch
            {
                GameEnums.PlayableHandType.Rock => iconSprites[0],
                GameEnums.PlayableHandType.Paper => iconSprites[1],
                GameEnums.PlayableHandType.Scissors => iconSprites[2],
                GameEnums.PlayableHandType.Lizard => iconSprites[3],
                GameEnums.PlayableHandType.Spock => iconSprites[4],
                _ => null
            };

        private void OnApplicationQuit() =>
            DePopulatePlayerOptions();
    }
}