using RPSLS.Audio;
using RPSLS.Miscellaneous;
using RPSLS.Services;
using RPSLS.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace RPSLS.UI.Items
{
    public class PlayableItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImg;

        /// <summary>
        /// On this UI option clicked.
        /// </summary>
        public void OnOptionClicked()
        {
            var hudScreen = Bootstrap.GetService<UserInterfaceService>()
                .CurrentInterface
                .GetScreen<GameplayHudScreen>();
            hudScreen.SetPlayerOptionType(HandType);
            Bootstrap.GetService<GameManagementService>().CurrentPlayerSelection = HandType;
            transform.SetAsLastSibling();
            Bootstrap.GetService<AudioService>().PlayAudio(AudioTags.HAND_TAP);
            Debug.Log(HandType);
        }

        /// <summary>
        /// Hand type of this option.
        /// </summary>
        internal GameEnums.PlayableHandType HandType { get; private set; }

        /// <summary>
        /// Set the values of this option.
        /// </summary>
        /// <param name="iconSprite">icon sprite to set</param>
        /// <param name="handType">hand type to set</param>
        internal void SetValues(Sprite iconSprite, GameEnums.PlayableHandType handType) =>
            (iconImg.sprite, HandType) = (iconSprite, handType);

        /// <summary>
        /// Toggle the scale of this option base on selected or not
        /// </summary>
        /// <param name="selected">true if selected, false otherwise</param>
        internal void ToggleScale(bool selected) =>
            transform.localScale = selected ? GameConstants.SelectedScale : GameConstants.NormalScale;
    }
}